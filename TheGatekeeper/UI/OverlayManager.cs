using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TheGatekeeper.Models;
using static TheGatekeeper.GenericOverlay;

namespace TheGatekeeper
{
    public class OverlayManager
    {
        private readonly Form _owner;
        private Panel _overlayPanel;
        private Panel _card;

        private DocumentOverlay _documentOverlay;
        private HeartbeatOverlay _heartbeatOverlay;
        private StickerOverlay _stickerOverlay;
        private RadioOverlay _radioOverlay;
        private GenericOverlay _genericOverlay;
        private FingerprintOverlay _fingerprintOverlay;
        private RadiationOverlay _radiationOverlay;

        public int CurrentDay { get; set; } = 1;
        public Character CurrentCharacter { get; set; }

        public OverlayManager(Form owner)
        {
            _owner = owner;
            BuildBaseOverlay();

            _documentOverlay = new DocumentOverlay(_card);
            _heartbeatOverlay = new HeartbeatOverlay(_card);
            _stickerOverlay = new StickerOverlay(_card);
            _radioOverlay = new RadioOverlay(_card);
            _genericOverlay = new GenericOverlay(_card);
            _fingerprintOverlay = new FingerprintOverlay(_card);
            _radiationOverlay = new RadiationOverlay(_card);
        }

        private void BuildBaseOverlay()
        {
            _overlayPanel = new Panel
            {
                BackColor = Color.FromArgb(210, 0, 0, 0),
                Visible = false,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Default
            };
            _overlayPanel.Click += (s, e) => Hide();
            // Пробрасываем MouseUp на Form1 — для клика по часам и другим зонам
            _overlayPanel.MouseUp += (s, e) =>
            {
                if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
                var screenPt = _overlayPanel.PointToScreen(e.Location);
                (_owner as Form1)?.TryOpenClockFromOverlay(screenPt);
            };

            int cw = 680, ch = 460;
            _card = new Panel
            {
                Size = new Size(cw, ch),
                BackColor = Color.FromArgb(255, 10, 12, 18),
                Cursor = Cursors.Default
            };
            _card.Paint += DrawCardBorder;
            _card.MouseClick += (s, e) => { };

            _overlayPanel.Controls.Add(_card);
            _owner.Controls.Add(_overlayPanel);
            _overlayPanel.BringToFront();

            _owner.Resize += (s, e) => CenterCard();
            CenterCard();
        }

        private void CenterCard()
        {
            _card.Location = new Point(
                (_overlayPanel.Width - _card.Width) / 2,
                (_overlayPanel.Height - _card.Height) / 2);
        }


        private void DrawCardBorder(object sender, PaintEventArgs pe)
        {
            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(Color.FromArgb(180, 51, 102, 170), 1.5f))
                g.DrawRectangle(pen, 1, 1, _card.Width - 2, _card.Height - 2);

            using (var br = new LinearGradientBrush(
                new Rectangle(0, 0, _card.Width, 4),
                Color.FromArgb(160, 51, 102, 200),
                Color.Transparent,
                LinearGradientMode.Vertical))
                g.FillRectangle(br, 0, 0, _card.Width, 4);

            using (var pen = new Pen(Color.FromArgb(200, 51, 130, 200), 2))
            {
                int a = 16;
                g.DrawLine(pen, 0, 0, a, 0);
                g.DrawLine(pen, 0, 0, 0, a);
                g.DrawLine(pen, _card.Width - a, 0, _card.Width, 0);
                g.DrawLine(pen, _card.Width, 0, _card.Width, a);
                g.DrawLine(pen, 0, _card.Height - a, 0, _card.Height);
                g.DrawLine(pen, 0, _card.Height, a, _card.Height);
            }
        }

        public void ShowDocument()
        {
            ShowCard();
            _documentOverlay.Show(CurrentCharacter, CurrentDay);
        }

        public void ShowHeartbeat()
        {
            ShowCard();
            _heartbeatOverlay.Show(CurrentCharacter, CurrentDay);
        }

        public void ShowSticker(int stickerIndex)
        {
            ShowCard();
            _stickerOverlay.Show(stickerIndex, CurrentDay);
        }

        public void ShowRadio()
        {
            ShowCard();
            _radioOverlay.Show(CurrentDay);
        }

        public void ShowGeneric(string title, string body)
        {
            ShowCard();
            _genericOverlay.Show(title, body);
        }

        private void ShowCard()
        {
            HideAllPanels();
            _overlayPanel.Visible = true;
            _overlayPanel.BringToFront();
        }
        public void ShowFingerprint()
        {
            ShowCard();
            _fingerprintOverlay.Show(CurrentCharacter, CurrentDay);
        }
        public void ShowRadiation()
        {
            ShowCard();
            _radiationOverlay.Show(CurrentCharacter, CurrentDay);
        }

        public void Hide()
        {
            _overlayPanel.Visible = false;
            _heartbeatOverlay.StopAnimation();
        }

        public bool IsVisible => _overlayPanel.Visible;

        private void HideAllPanels()
        {
            _documentOverlay.Hide();
            _heartbeatOverlay.Hide();
            _stickerOverlay.Hide();
            _radioOverlay.Hide();
            _genericOverlay.Hide();
            _fingerprintOverlay?.Hide();
            _radiationOverlay?.Hide();
        }
    }

    public abstract class BaseOverlayPanel
    {
        protected Panel Card;
        protected Panel ContentPanel;

        protected BaseOverlayPanel(Panel card)
        {
            Card = card;
            ContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };
            card.Controls.Add(ContentPanel);

            var closeBtn = new Button
            {
                Text = "✕",
                Size = new Size(28, 28),
                Location = new Point(card.Width - 36, 8),
                ForeColor = Color.FromArgb(140, 120, 150, 190),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 51, 102, 170);
            closeBtn.Click += (s, e) => FindManager()?.Hide();
            ContentPanel.Controls.Add(closeBtn);
            BuildUI();
        }

        protected abstract void BuildUI();

        public void Hide() => ContentPanel.Visible = false;

        protected Label MakeTitle(string text, int y = 14)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(20, y),
                Size = new Size(Card.Width - 60, 28),
                ForeColor = Color.FromArgb(255, 100, 170, 240),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ContentPanel.Controls.Add(lbl);
            return lbl;
        }

        protected Label MakeDivider(int y)
        {
            var lbl = new Label
            {
                Location = new Point(20, y),
                Size = new Size(Card.Width - 40, 1),
                BackColor = Color.FromArgb(60, 51, 102, 170)
            };
            ContentPanel.Controls.Add(lbl);
            return lbl;
        }

        private OverlayManager FindManager()
        {
            var form = Card.FindForm() as Form1;
            return form?.OverlayManagerInstance;
        }
    }

    public class DocumentOverlay : BaseOverlayPanel
    {
        private Label _lblTitle, _lblId, _lblName, _lblAge, _lblOccupation;
        private Label _lblOrigin, _lblCode, _lblStatus, _lblNote;
        private Panel _photoPlaceholder;
        private Label _lblPhotoText;

        public DocumentOverlay(Panel card) : base(card) { }

        protected override void BuildUI()
        {
            MakeTitle("VOID TERMINAL // IDENTITY DOCUMENT");
            MakeDivider(46);

            _photoPlaceholder = new Panel
            {
                Location = new Point(20, 58),
                Size = new Size(120, 150),
                BackColor = Color.FromArgb(20, 30, 45),
            };
            _photoPlaceholder.Paint += (s, pe) =>
            {
                pe.Graphics.DrawRectangle(
                    new Pen(Color.FromArgb(80, 51, 102, 170), 1),
                    0, 0, _photoPlaceholder.Width - 1, _photoPlaceholder.Height - 1);
            };
            _lblPhotoText = new Label
            {
                Dock = DockStyle.Fill,
                Text = "[ PHOTO\nCLASSIFIED ]",
                ForeColor = Color.FromArgb(60, 100, 140),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 8),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _photoPlaceholder.Controls.Add(_lblPhotoText);
            ContentPanel.Controls.Add(_photoPlaceholder);

            var stamp = new Label
            {
                Location = new Point(24, 185),
                Size = new Size(112, 20),
                Text = "VOID AUTHORITY",
                ForeColor = Color.FromArgb(100, 180, 60, 60),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 7, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            ContentPanel.Controls.Add(stamp);

            int x = 160, startY = 58;
            _lblId = MakeField("ID #", x, startY);
            _lblName = MakeField("NAME", x, startY + 38);
            _lblAge = MakeField("AGE / TYPE", x, startY + 76);
            _lblOccupation = MakeField("OCCUPATION", x, startY + 114);
            _lblOrigin = MakeField("ORIGIN", x, startY + 152);
            _lblCode = MakeField("ACCESS CODE", x, startY + 190);

            _lblStatus = new Label
            {
                Location = new Point(20, 230),
                Size = new Size(Card.Width - 40, 24),
                ForeColor = Color.FromArgb(220, 60, 220, 100),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            ContentPanel.Controls.Add(_lblStatus);

            MakeDivider(258);

            var noteLbl = new Label
            {
                Location = new Point(20, 264),
                Size = new Size(160, 18),
                Text = "INSPECTOR NOTE:",
                ForeColor = Color.FromArgb(130, 130, 150),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 8, FontStyle.Bold)
            };
            ContentPanel.Controls.Add(noteLbl);

            _lblNote = new Label
            {
                Location = new Point(20, 282),
                Size = new Size(Card.Width - 40, 80),
                ForeColor = Color.FromArgb(200, 200, 160),
                BackColor = Color.FromArgb(15, 255, 220, 100),
                Font = new Font("Consolas", 9),
                Padding = new Padding(6),
                TextAlign = ContentAlignment.TopLeft
            };
            ContentPanel.Controls.Add(_lblNote);
        }

        private Label MakeField(string fieldName, int x, int y)
        {
            var key = new Label
            {
                Location = new Point(x, y),
                Size = new Size(110, 16),
                Text = fieldName + ":",
                ForeColor = Color.FromArgb(100, 130, 170),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 8)
            };
            var val = new Label
            {
                Location = new Point(x, y + 16),
                Size = new Size(Card.Width - x - 20, 18),
                ForeColor = Color.FromArgb(220, 220, 200),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold)
            };
            ContentPanel.Controls.Add(key);
            ContentPanel.Controls.Add(val);
            return val;
        }

        public void Show(Character character, int day)
        {
            ContentPanel.Visible = true;
            if (character == null) return;

            _lblId.Text = $"VD-{day:D2}{new Random().Next(1000, 9999)}";
            _lblName.Text = character.Name;
            _lblOccupation.Text = character.Occupation ?? "— UNSPECIFIED —";
            _lblCode.Text = character.AccessCode ?? "— NOT PROVIDED —";

            string[] statuses = { "ENTRY PENDING", "UNDER REVIEW", "FLAGGED", "PRIORITY CHECK" };
            _lblStatus.Text = $"STATUS: {statuses[Math.Min(day - 1, statuses.Length - 1)]}";

            _lblNote.Text = GenerateNote(character, day);
        }

        private string GenerateNote(Character character, int day)
        {
            var notes = new[]
            {
                "Compare subject's testimony with document data.\nDiscrepancies may indicate falsification.",
                $"Day {day}: verify access code and origin.\nSynthetics use outdated databases.",
                "Pay attention to occupation — it must match\nthe subject's clearance zone.",
                "WARNING: document forgery cases increased.\nWhen in doubt — detain."
            };
            return notes[Math.Min(day - 1, notes.Length - 1)];
        }
    }

    public class HeartbeatOverlay : BaseOverlayPanel
    {
        private Panel _ecgPanel;
        private Label _lblBpm, _lblStatus, _lblAnalysis;
        private Timer _animTimer;
        private float _phase = 0f;
        private int _bpm = 78;
        private bool _anomaly = false;

        public HeartbeatOverlay(Panel card) : base(card) { }

        protected override void BuildUI()
        {
            MakeTitle("VITAL SIGNS // CARDIAC MONITOR");
            MakeDivider(46);

            _ecgPanel = new Panel
            {
                Location = new Point(20, 58),
                Size = new Size(Card.Width - 40, 180),
                BackColor = Color.FromArgb(255, 4, 12, 8)
            };
            _ecgPanel.Paint += DrawECG;
            ContentPanel.Controls.Add(_ecgPanel);

            _lblBpm = new Label
            {
                Location = new Point(20, 248),
                Size = new Size(200, 40),
                ForeColor = Color.FromArgb(255, 0, 230, 80),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ContentPanel.Controls.Add(_lblBpm);

            var bpmLabel = new Label
            {
                Location = new Point(165, 262),
                Size = new Size(60, 18),
                Text = "BPM",
                ForeColor = Color.FromArgb(100, 180, 120),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9)
            };
            ContentPanel.Controls.Add(bpmLabel);

            _lblStatus = new Label
            {
                Location = new Point(20, 294),
                Size = new Size(Card.Width - 40, 20),
                ForeColor = Color.FromArgb(180, 200, 200),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ContentPanel.Controls.Add(_lblStatus);

            MakeDivider(320);

            _lblAnalysis = new Label
            {
                Location = new Point(20, 328),
                Size = new Size(Card.Width - 40, 100),
                ForeColor = Color.FromArgb(200, 200, 180),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9),
                TextAlign = ContentAlignment.TopLeft
            };
            ContentPanel.Controls.Add(_lblAnalysis);

            _animTimer = new Timer { Interval = 40 };
            _animTimer.Tick += (s, e) =>
            {
                _phase += _anomaly ? 0.09f : 0.06f;
                if (_phase > Math.PI * 2) _phase -= (float)(Math.PI * 2);
                _ecgPanel?.Invalidate();
            };
        }

        private void DrawECG(object sender, PaintEventArgs pe)
        {
            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = _ecgPanel.Width, h = _ecgPanel.Height;

            using (var gridPen = new Pen(Color.FromArgb(25, 0, 180, 60), 1))
            {
                for (int x = 0; x < w; x += 30) g.DrawLine(gridPen, x, 0, x, h);
                for (int y = 0; y < h; y += 20) g.DrawLine(gridPen, 0, y, w, y);
            }

            int midY = h / 2;
            int points = w;
            var pts = new PointF[points];
            float speed = _bpm / 60f;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / points * 4f * (float)Math.PI + _phase * speed;
                float y = ECGWaveform(t, _anomaly);
                pts[i] = new PointF(i, midY - y * (h * 0.38f));
            }

            Color baseColor = _anomaly ? Color.FromArgb(255, 230, 80, 0) : Color.FromArgb(255, 0, 230, 80);
            int[] glowAlphas = { 15, 30, 70 };
            int[] glowWidths = { 6, 3, 1 };

            for (int gi = 0; gi < 3; gi++)
            {
                using (var pen = new Pen(Color.FromArgb(glowAlphas[gi], baseColor), glowWidths[gi]))
                    g.DrawLines(pen, pts);
            }

            using (var pen = new Pen(baseColor, 1.5f))
                g.DrawLines(pen, pts);
        }

        private float ECGWaveform(float t, bool anomaly)
        {
            float mod = t % (2f * (float)Math.PI);
            float v = 0;

            v += 0.15f * (float)Math.Exp(-Math.Pow(mod - 0.8f, 2) / 0.04f);
            v -= 0.05f * (float)Math.Exp(-Math.Pow(mod - 1.4f, 2) / 0.005f);
            float rHeight = anomaly ? 1.4f : 1.0f;
            v += rHeight * (float)Math.Exp(-Math.Pow(mod - 1.6f, 2) / 0.003f);
            v -= 0.25f * (float)Math.Exp(-Math.Pow(mod - 1.8f, 2) / 0.008f);
            v += 0.30f * (float)Math.Exp(-Math.Pow(mod - 2.6f, 2) / 0.12f);

            if (anomaly)
            {
                v += 0.2f * (float)Math.Sin(mod * 7f) * (float)Math.Exp(-Math.Pow(mod - 1.0f, 2) / 0.3f);
            }

            return v;
        }

        public void Show(Character character, int day)
        {
            ContentPanel.Visible = true;

            var rnd = new Random();
            _anomaly = character?.Species != "Human";
            _bpm = _anomaly ? rnd.Next(55, 70) : rnd.Next(72, 95);
            _phase = 0f;

            _lblBpm.Text = _bpm.ToString();
            _lblBpm.ForeColor = _anomaly
                ? Color.FromArgb(255, 230, 120, 0)
                : Color.FromArgb(255, 0, 230, 80);

            _lblStatus.Text = _anomaly
                ? "⚠  ANOMALY DETECTED — NON-STANDARD WAVEFORM"
                : "✓  CARDIAC PATTERN: WITHIN HUMAN RANGE";
            _lblStatus.ForeColor = _anomaly
                ? Color.FromArgb(220, 200, 80, 0)
                : Color.FromArgb(180, 80, 200, 100);

            _lblAnalysis.Text = _anomaly
                ? "ECG Analysis: harmonic artifacts detected\n" +
                  $"in T-segment (offset ~{rnd.Next(12, 28)}ms).\n" +
                  "Synthetic pauses between R-peaks.\n" +
                  "Recommendation: INTENSIFY INTERROGATION."
                : "ECG Analysis: normal sinus rhythm.\n" +
                  $"R-R variability: {rnd.Next(28, 55)}ms (physiological).\n" +
                  "No signs of synthetic origin.\n" +
                  "Recommendation: standard check.";

            _animTimer.Start();
        }

        public void StopAnimation() => _animTimer?.Stop();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  СТИКЕРЫ — ДИЗАЙН КАК НАСТОЯЩИЕ КЛЕЙКИЕ СТИКЕРЫ
    // ═══════════════════════════════════════════════════════════════════════
    public class StickerOverlay : BaseOverlayPanel
    {
        private static readonly string[][,] _content = new[]
        {
            // День 1
            new string[,]
            {
                {
                    "DAY 1 MEMO",
                    "Check EVERYONE:\n" +
                    "✓ access code\n" +
                    "✓ place of arrival\n" +
                    "✓ purpose of visit\n\n" +
                    "🤖 Robots: pauses >0.3s\n" +
                    "👽 Aliens: \"we/us\"\n\n" +
                    "❗ 3 mistakes = end"
                },
                {
                    "ACCESS CODES - DAY 1",
                    "Alpha:  7741-X\n" +
                    "Beta:   3392-K\n" +
                    "VOID:   ????  (classified)\n\n" +
                    "Shift A: 06:00–14:00\n" +
                    "Shift B: 14:00–22:00\n\n" +
                    "Mismatch = detain"
                },
                {
                    "TRAIT GUIDE",
                    "🤖 ROBOT:\n  synthetic speech\n  pauses >0.3s\n\n" +
                    "👽 ALIEN:\n  \"we\" not \"I\"\n  harmonic drift\n\n" +
                    "👤 HUMAN:\n  emotions + stress"
                }
            },
            // День 2
            new string[,]
            {
                {
                    "DAY 2 ALERT",
                    "⚠ Robots now mimic\nemotions!\n\n" +
                    "Check:\n" +
                    "✓ micro-pauses\n" +
                    "✓ updated codes\n" +
                    "✓ family knowledge"
                },
                {
                    "CODES - DAY 2",
                    "Alpha:  8812-R  ← NEW\n" +
                    "Beta:   3392-K\n" +
                    "VOID:   VD-007\n\n" +
                    "⚠ Old codes invalid!\n\n" +
                    "Robots use old DBs"
                },
                {
                    "DAY 2 ANALYSIS",
                    "Yesterday missed:\n  2 synthetics\n\n" +
                    "They learned.\n" +
                    "Ask more questions\n" +
                    "before deciding!"
                }
            },
            // День 3+
            new string[,]
            {
                {
                    "CRITICAL DAY 3+",
                    "⚠ Mimicry at 68%!\n\n" +
                    "Standard signs\nunreliable.\n\n" +
                    "Use:\n" +
                    "✓ documents\n" +
                    "✓ ECG monitor\n" +
                    "✓ pressure timer"
                },
                {
                    "CODES - DAY 3+",
                    "Alpha:  CLASSIFIED\n" +
                    "Beta:   CLASSIFIED\n" +
                    "VOID:   CLASSIFIED\n\n" +
                    "Request from\nCommand!\n\n" +
                    "CH-A: 156.8 MHz"
                },
                {
                    "HIGH THREAT",
                    "Breach detected:\n  X robots passed\n\n" +
                    "ALERT MODE!\n\n" +
                    "Everyone is suspect\n\n" +
                    "❗ VILLAIN ACTIVE"
                }
            }
        };

        private static readonly Color[] _stickerColors =
        {
            Color.FromArgb(255, 255, 245, 120),  // Жёлтый стикер
            Color.FromArgb(255, 255, 180, 200),  // Розовый стикер
            Color.FromArgb(255, 180, 230, 255),  // Голубой стикер
        };

        public StickerOverlay(Panel card) : base(card) { }

        protected override void BuildUI() { }

        public void Show(int stickerIndex, int day)
        {
            ContentPanel.Controls.Clear();
            ContentPanel.Visible = true;

            int daySet = Math.Min(day - 1, _content.Length - 1);
            var set = _content[daySet];
            int idx = Math.Min(stickerIndex, set.GetLength(0) - 1);
            string title = set[idx, 0];
            string body = set[idx, 1];
            Color stickerColor = _stickerColors[stickerIndex % _stickerColors.Length];

            // Создаём стикер как панель
            var sticker = new Panel
            {
                Size = new Size(320, 300),
                BackColor = stickerColor,
                Cursor = Cursors.Default
            };

            sticker.Location = new Point(
                (Card.Width - sticker.Width) / 2,
                (Card.Height - sticker.Height) / 2);

            sticker.Paint += (s, pe) => DrawStickerStyle(pe.Graphics, sticker, stickerColor);

            // Заголовок - рукописный шрифт
            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(16, 24),
                Size = new Size(sticker.Width - 32, 32),
                ForeColor = Color.FromArgb(255, 20, 10, 0),
                BackColor = Color.Transparent,
                Font = new Font("Segoe Script", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.TopLeft
            };

            // Подчёркивание заголовка
            var underline = new Panel
            {
                Location = new Point(16, 58),
                Size = new Size(sticker.Width - 32, 2),
                BackColor = Color.FromArgb(100, 40, 20, 0)
            };

            // Тело текста - рукописный шрифт
            var lblBody = new Label
            {
                Text = body,
                Location = new Point(16, 70),
                Size = new Size(sticker.Width - 32, sticker.Height - 85),
                ForeColor = Color.FromArgb(255, 25, 15, 0),
                BackColor = Color.Transparent,
                Font = new Font("Segoe Script", 9),
                TextAlign = ContentAlignment.TopLeft
            };

            sticker.Controls.AddRange(new Control[] { lblTitle, underline, lblBody });
            ContentPanel.Controls.Add(sticker);

            // Кнопка закрытия
            var closeBtn = new Button
            {
                Text = "✕",
                Size = new Size(32, 32),
                Location = new Point(Card.Width - 40, 10),
                ForeColor = Color.FromArgb(180, 60, 60, 80),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => FindManager()?.Hide();
            ContentPanel.Controls.Add(closeBtn);
        }

        private void DrawStickerStyle(Graphics g, Panel sticker, Color baseColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = sticker.Width, h = sticker.Height;

            // Тень (эффект объёма)
            using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillRectangle(shadow, 3, 3, w - 3, h - 3);
            }

            // Основной цвет стикера
            using (var brush = new SolidBrush(baseColor))
            {
                g.FillRectangle(brush, 0, 0, w, h);
            }

            // Клеевая полоска сверху (чуть темнее)
            Color glueLine = Color.FromArgb(
                baseColor.A,
                (int)(baseColor.R * 0.8),
                (int)(baseColor.G * 0.8),
                (int)(baseColor.B * 0.8));

            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, w, 12),
                glueLine,
                baseColor,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, 0, 0, w, 12);
            }

            // Рамка (лёгкая)
            using (var pen = new Pen(Color.FromArgb(50, 80, 60, 0), 1))
            {
                g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
            }

            // Линованная бумага (едва видимые линии)
            using (var linePen = new Pen(Color.FromArgb(15, 100, 80, 60), 1))
            {
                for (int y = 80; y < h - 15; y += 24)
                {
                    g.DrawLine(linePen, 14, y, w - 14, y);
                }
            }

            // Небольшой загиб уголка (правый нижний)
            var cornerPoints = new Point[]
            {
                new Point(w - 20, h),
                new Point(w, h - 20),
                new Point(w, h)
            };
            using (var cornerBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                g.FillPolygon(cornerBrush, cornerPoints);
            }
        }

        private OverlayManager FindManager()
        {
            var form = Card.FindForm() as Form1;
            return form?.OverlayManagerInstance;
        }
    }

    public class RadioOverlay : BaseOverlayPanel
    {
        private Label _lblSignal, _lblMessages;
        private Timer _staticTimer;
        private int _staticFrame = 0;

        private static readonly string[][] _radioByDay = new[]
        {
            new[]
            {
                "> [06:14] Command: Standard mode",
                "> [06:31] Perimeter-2: Movement at gates",
                "> [07:02] Medical: ECG anomaly",
                "> [08:10] Unknown: ...they're among us..."
            },
            new[]
            {
                "> [06:05] Day 2. Quota increased",
                "> [06:50] Outdated Alpha code detected",
                "> [07:20] Document forgery detected",
                "> [08:33] Unknown: ...don't trust docs..."
            },
            new[]
            {
                "> [06:01] BREACH IN SECTOR 4",
                "> [06:18] Searching for infiltrators",
                "> [07:00] Villain active",
                "> [07:44] Agent X: Trust no one"
            },
        };

        public RadioOverlay(Panel card) : base(card) { }

        protected override void BuildUI()
        {
            MakeTitle("COMM UNIT // INCOMING SIGNAL");
            MakeDivider(46);

            _lblSignal = new Label
            {
                Location = new Point(20, 56),
                Size = new Size(Card.Width - 40, 18),
                ForeColor = Color.FromArgb(180, 0, 200, 80),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9),
                Text = "FREQ: 156.8 MHz  |  SIGNAL: ████████░░  82%"
            };
            ContentPanel.Controls.Add(_lblSignal);

            MakeDivider(78);

            _lblMessages = new Label
            {
                Location = new Point(20, 86),
                Size = new Size(Card.Width - 40, 300),
                ForeColor = Color.FromArgb(200, 180, 230, 180),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10),
                TextAlign = ContentAlignment.TopLeft
            };
            ContentPanel.Controls.Add(_lblMessages);

            _staticTimer = new Timer { Interval = 800 };
            _staticTimer.Tick += (s, e) =>
            {
                _staticFrame++;
                string[] bars = { "████████░░", "███████░░░", "█████████░", "████░░░░░░" };
                int sig = 60 + (_staticFrame % 4) * 8;
                if (_lblSignal != null && !_lblSignal.IsDisposed)
                    _lblSignal.Text = $"FREQ: 156.8 MHz  |  SIGNAL: {bars[_staticFrame % 4]}  {sig}%";
            };
        }

        public void Show(int day)
        {
            ContentPanel.Visible = true;
            int dayIdx = Math.Min(day - 1, _radioByDay.Length - 1);
            _lblMessages.Text = StoryRadioData.GetMessages(day);
            _staticTimer.Start();
        }

        public new void Hide()
        {
            _staticTimer?.Stop();
            ContentPanel.Visible = false;
        }
    }

    public class GenericOverlay : BaseOverlayPanel
    {
        private Label _lblTitle, _lblBody;

        public GenericOverlay(Panel card) : base(card) { }

        protected override void BuildUI()
        {
            _lblTitle = MakeTitle("TERMINAL");
            MakeDivider(46);

            _lblBody = new Label
            {
                Location = new Point(20, 56),
                Size = new Size(Card.Width - 40, 360),
                ForeColor = Color.FromArgb(200, 153, 187, 221),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10),
                TextAlign = ContentAlignment.TopLeft
            };
            ContentPanel.Controls.Add(_lblBody);
        }

        public void Show(string title, string body)
        {
            _lblTitle.Text = title;
            _lblBody.Text = body;
            ContentPanel.Visible = true;
        }

    }
    public class RadiationOverlay : BaseOverlayPanel
    {
        private Panel _displayPanel;
        private Label _lblValue, _lblStatus, _lblAnalysis;
        private Timer _animTimer;
        private float _phase = 0f;
        private float _currentRadiation = 0.12f;
        private float _targetRadiation = 0.12f;
        private bool _anomaly = false;

        public RadiationOverlay(Panel card) : base(card) { }

        protected override void BuildUI()
        {
            MakeTitle("RADIATION DOSIMETER // GEIGER-MÜLLER");
            MakeDivider(46);

            _displayPanel = new Panel
            {
                Location = new Point(20, 58),
                Size = new Size(Card.Width - 40, 180),
                BackColor = Color.FromArgb(255, 2, 8, 4)
            };
            _displayPanel.Paint += DrawRadiationDisplay;
            ContentPanel.Controls.Add(_displayPanel);

            _lblValue = new Label
            {
                Location = new Point(20, 248),
                Size = new Size(200, 40),
                ForeColor = Color.FromArgb(255, 0, 255, 100),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ContentPanel.Controls.Add(_lblValue);

            var unitLabel = new Label
            {
                Location = new Point(165, 262),
                Size = new Size(80, 18),
                Text = "µSv/h",
                ForeColor = Color.FromArgb(100, 200, 150),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9)
            };
            ContentPanel.Controls.Add(unitLabel);

            _lblStatus = new Label
            {
                Location = new Point(20, 294),
                Size = new Size(Card.Width - 40, 20),
                ForeColor = Color.FromArgb(180, 200, 200),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ContentPanel.Controls.Add(_lblStatus);

            MakeDivider(320);

            _lblAnalysis = new Label
            {
                Location = new Point(20, 328),
                Size = new Size(Card.Width - 40, 100),
                ForeColor = Color.FromArgb(200, 200, 180),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9),
                TextAlign = ContentAlignment.TopLeft
            };
            ContentPanel.Controls.Add(_lblAnalysis);

            _animTimer = new Timer { Interval = 50 };
            _animTimer.Tick += (s, e) =>
            {
                _phase += 0.1f;
                _currentRadiation += (_targetRadiation - _currentRadiation) * 0.05f;
                if (Math.Abs(_currentRadiation - _targetRadiation) < 0.001f)
                    _currentRadiation = _targetRadiation;
                _displayPanel?.Invalidate();
                UpdateValueLabel();
            };
        }

        private void DrawRadiationDisplay(object sender, PaintEventArgs pe)
        {
            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = _displayPanel.Width, h = _displayPanel.Height;

            // Тёмно-зелёный фон с сеткой
            g.Clear(Color.FromArgb(2, 8, 4));
            using (var pen = new Pen(Color.FromArgb(20, 0, 200, 0), 1))
            {
                for (int x = 0; x < w; x += 20) g.DrawLine(pen, x, 0, x, h);
                for (int y = 0; y < h; y += 20) g.DrawLine(pen, 0, y, w, y);
            }

            // Шкала (горизонтальный бар)
            int barX = 40, barY = h / 2 - 10, barW = w - 80, barH = 20;
            using (var backBrush = new SolidBrush(Color.FromArgb(20, 0, 100, 0)))
                g.FillRectangle(backBrush, barX, barY, barW, barH);

            // Закрашенная часть шкалы
            float fillPercent = Math.Min(1f, _currentRadiation / 2.0f); // 2.0 µSv/h = 100%
            int fillW = (int)(barW * fillPercent);
            Color fillColor = _anomaly ? Color.OrangeRed : Color.Lime;
            using (var fillBrush = new SolidBrush(Color.FromArgb(200, fillColor)))
                g.FillRectangle(fillBrush, barX, barY, fillW, barH);

            // Рамка шкалы
            using (var pen = new Pen(Color.FromArgb(100, 0, 200, 0), 2))
                g.DrawRectangle(pen, barX, barY, barW, barH);

            // Метки
            using (var font = new Font("Consolas", 8))
            using (var brush = new SolidBrush(Color.FromArgb(150, 0, 200, 0)))
            {
                g.DrawString("0.0", font, brush, barX - 20, barY + 3);
                g.DrawString("1.0", font, brush, barX + barW / 2 - 12, barY + 3);
                g.DrawString("2.0", font, brush, barX + barW - 20, barY + 3);
            }

            // График щелчков Гейгера (имитация)
            int points = w;
            var pts = new PointF[points];
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)points * 10f + _phase;
                float y = (float)(Math.Sin(t * 5) * 0.2 + Math.Sin(t * 23) * 0.1) * (0.5f + _currentRadiation);
                pts[i] = new PointF(i, h / 2 + 40 + y * 30);
            }
            using (var pen = new Pen(Color.FromArgb(180, 0, 255, 0), 1.5f))
                g.DrawLines(pen, pts);
        }

        private void UpdateValueLabel()
        {
            _lblValue.Text = $"{_currentRadiation:F2}";
            _lblValue.ForeColor = _anomaly ? Color.OrangeRed : Color.FromArgb(0, 255, 100);
        }

        public void Show(Character character, int day)
        {
            ContentPanel.Visible = true;
            var rnd = new Random();

            _anomaly = character?.Species != "Human";
            _targetRadiation = _anomaly ? (float)(0.5 + rnd.NextDouble() * 1.2) : (float)(0.08 + rnd.NextDouble() * 0.15);
            _currentRadiation = _targetRadiation * 0.7f;
            _phase = 0f;

            _lblStatus.Text = _anomaly
                ? "⚠  WARNING: Elevated radiation signature"
                : "✓  Radiation level within normal background";
            _lblStatus.ForeColor = _anomaly ? Color.OrangeRed : Color.Lime;

            _lblAnalysis.Text = _anomaly
                ? "Geiger counter indicates abnormal beta/gamma activity.\n" +
                  "Possible internal radioisotope source.\n" +
                  "Recommendation: secondary scan advised."
                : "Background radiation consistent with civilian sector.\n" +
                  "No artificial isotopes detected.\n" +
                  "Subject is safe to approach.";

            _animTimer.Start();
        }

        public new void Hide()
        {
            _animTimer?.Stop();
            ContentPanel.Visible = false;
        }
    }

    public class FingerprintOverlay : BaseOverlayPanel
    {
        private Panel _scanPanel;
        private Label _lblStatus, _lblResult;
        private Timer _animTimer;
        private float _scanLineY = 0f;
        private bool _scanComplete = false;
        private bool _match = false;

        public FingerprintOverlay(Panel card) : base(card) { }

        protected override void BuildUI()
        {
            MakeTitle("FINGERPRINT SCANNER // BIOMETRIC ID");
            MakeDivider(46);

            _scanPanel = new Panel
            {
                Location = new Point(20, 58),
                Size = new Size(300, 200),
                BackColor = Color.FromArgb(255, 5, 10, 20)
            };
            _scanPanel.Paint += DrawScanPanel;
            ContentPanel.Controls.Add(_scanPanel);

            _lblStatus = new Label
            {
                Location = new Point(20, 270),
                Size = new Size(Card.Width - 40, 24),
                ForeColor = Color.Cyan,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ContentPanel.Controls.Add(_lblStatus);

            MakeDivider(300);

            _lblResult = new Label
            {
                Location = new Point(20, 308),
                Size = new Size(Card.Width - 40, 100),
                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9),
                TextAlign = ContentAlignment.TopLeft
            };
            ContentPanel.Controls.Add(_lblResult);

            _animTimer = new Timer { Interval = 30 };
            _animTimer.Tick += (s, e) =>
            {
                if (!_scanComplete)
                {
                    _scanLineY += 8f;
                    if (_scanLineY >= _scanPanel.Height)
                    {
                        _scanLineY = 0;
                        _scanComplete = true;
                        _animTimer.Stop();
                        _lblStatus.Text = _match ? "✓ MATCH CONFIRMED" : "✗ NO MATCH - UNKNOWN PRINT";
                        _lblStatus.ForeColor = _match ? Color.Lime : Color.Red;
                        _lblResult.Text = _match
                            ? "Subject fingerprint matches civil database.\nClearance level: STANDARD.\nNo flags raised."
                            : "Subject fingerprint not found in any known database.\nPossible synthetic epidermis or alien dermal pattern.\nRecommendation: FLAG FOR REVIEW.";
                    }
                }
                _scanPanel?.Invalidate();
            };
        }

        private void DrawScanPanel(object sender, PaintEventArgs pe)
        {
            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = _scanPanel.Width, h = _scanPanel.Height;

            // Фон
            g.Clear(Color.FromArgb(5, 10, 20));

            // Рисуем стилизованный отпечаток пальца (дуги)
            int centerX = w / 2, centerY = h / 2;
            using (var pen = new Pen(Color.FromArgb(100, 150, 200), 2))
            {
                for (int i = 0; i < 6; i++)
                {
                    int radius = 30 + i * 12;
                    g.DrawArc(pen, centerX - radius, centerY - radius - 10, radius * 2, radius * 2, 200, 140);
                }
            }

            // Линия сканирования
            if (!_scanComplete)
            {
                int lineY = (int)_scanLineY;
                using (var pen = new Pen(Color.FromArgb(200, 0, 255, 255), 2))
                    g.DrawLine(pen, 0, lineY, w, lineY);
                using (var brush = new SolidBrush(Color.FromArgb(30, 0, 255, 255)))
                    g.FillRectangle(brush, 0, lineY - 2, w, 4);
            }

            // Рамка
            using (var pen = new Pen(Color.FromArgb(80, 100, 150), 1))
                g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
        }

        public void Show(Character character, int day)
        {
            ContentPanel.Visible = true;
            _scanComplete = false;
            _scanLineY = 0;
            _match = (character?.Species == "Human");
            _lblStatus.Text = "SCANNING...";
            _lblStatus.ForeColor = Color.Cyan;
            _lblResult.Text = "Place finger on scanner.\nAnalyzing dermal patterns...";
            _animTimer.Start();
        }

        public new void Hide()
        {
            _animTimer?.Stop();
            ContentPanel.Visible = false;
        }
    }
}
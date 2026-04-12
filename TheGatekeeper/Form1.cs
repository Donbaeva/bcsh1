using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheGatekeeper.Models;
using TheGatekeeper.Models;
using System.Threading.Tasks;


namespace TheGatekeeper
{
    public partial class Form1 : Form
    {
        private const int BaseW = 1280;
        private const int BaseH = 720;

        private Image backBackground, frontBackground;
        private Image btnDefault, btnRed, btnBlue, btnGreen;
        private Image currentBtnImage;
        private Image currentCharacter;

        private int shutterHeight = 0;
        private const int shutterMaxHeight = 450;
        private Timer shutterTimer;
        private bool isAnimating = false;
        private bool isClosing = false;

        // ─── Зоны клика — кнопки решения ────────────────────────────────────
        private readonly Rectangle redZoneBase = new Rectangle(50, 550, 100, 80);
        private readonly Rectangle blueZoneBase = new Rectangle(120, 550, 100, 80);
        private readonly Rectangle greenZoneBase = new Rectangle(280, 550, 100, 80);
        private readonly Rectangle monitorRectBase = new Rectangle(350, 170, 585, 450);

        // ─── ЛЕВЫЕ 3 ЭКРАНА ─────────────────────────────
        private readonly Rectangle zoneLeftTop = new Rectangle(58, 150, 140, 92);
        private readonly Rectangle zoneLeftMiddle = new Rectangle(53, 310, 165, 55);
        private readonly Rectangle zoneLeftBottom = new Rectangle(54, 380, 78, 58);

        // ─── ПРАВЫЕ СТИКЕРЫ (4 шт) ─────────────────────
        private readonly Rectangle zoneSticker1 = new Rectangle(1025, 200, 85, 28);
        private readonly Rectangle zoneSticker2 = new Rectangle(1020, 81, 90,85);
        private readonly Rectangle zoneSticker3 = new Rectangle(1038, 255, 75, 60);
        private readonly Rectangle zoneSticker4 = new Rectangle(1120, 143, 117, 140);

        // ─── ПРАВЫЙ ЭКРАН ПОД СТИКЕРАМИ ────────────────
        private readonly Rectangle zoneRightScreen = new Rectangle(1047, 330, 185, 90);

        // ─── РАДИО БОЛЬШОЕ СНИЗУ ───────────────────────
        private readonly Rectangle zoneBigRadio = new Rectangle(1070, 555, 210, 125);

        // ─── МАЛЕНЬКАЯ РАЦИЯ ЛЕВЕЕ ─────────────────────
        private readonly Rectangle zoneSmallRadio = new Rectangle(950, 490, 63, 130);

        // ─── НИЖНИЙ ЧЁРНЫЙ ЭКРАН ───────────────────────
        private readonly Rectangle zoneDialogueScreen = new Rectangle(510, 498, 275, 74);
        private int hoveredZone = -1;

        // ─── Буфер рендера ───────────────────────────────────────────────────
        private BufferedGraphicsContext _bufCtx;
        private BufferedGraphics _buffer;

        // ─── Вспышка ─────────────────────────────────────────────────────────
        private int flashAlpha = 0;
        private Color flashColor = Color.Transparent;
        private Timer flashTimer;

        // ─── UI-лейблы (прозрачный фон) ──────────────────────────────────────
        private Label lblScore, lblHealth, lblDay, lblQuota;
        private Label lblPressure;
        private Label lblName;
        private Label lblDialogue;

        // ─── Всплывающее окно (overlay) ──────────────────────────────────────
        private Panel overlayPanel;
        private Label overlayTitle;
        private Label overlayBody;
        private Button overlayClose;

        private int score = 0, health = 3, day = 1;
        private int charactersChecked = 0, dailyQuota = 5;
        private int pressureSeconds = 0;
        private Timer pressureTimer;

        public OverlayManager OverlayManagerInstance { get; private set; }

        private List<Character> todayCast;
        private Character currentCharacterData;
        private int currentCharacterIndex = 0;


        // Секунды смены для часов
        private int shiftSeconds = 0;
        private Timer clockTimer;

        // Таймер печатающей машинки для диалогов
        private Timer typingTimer;
        private string fullDialogueText = "";
        private int typingIndex = 0;
        private List<(Character character, string decision)> dailyDecisions = new List<(Character character, string decision)>();

        private readonly Random rnd = new Random();

        private readonly Rectangle zoneClock = new Rectangle(480, 572, 170, 50);

        // ─── Контент всплывашек ──────────────────────────────────────────────
        private readonly (string Title, string Body)[] overlayData = new[]
        {
            (
                "BIOMETRIC SCANNER // UNIT-7",
                "STATUS: ACTIVE — SCANNING...\n\n" +
                "Current subject profile:\n" +
                "• Body temp:        36.7°C  (NOMINAL)\n" +
                "• Pulse rate:       78 BPM\n" +
                "• Skin conductance: ELEVATED\n" +
                "• Retinal scan:     INCOMPLETE\n" +
                "• Voice pattern:    ANALYZING\n\n" +
                "⚠ ANOMALY: Micro-pause index 0.31s — above threshold\n\n" +
                "Cross-referencing VOID database...\n" +
                "Match confidence: 47% — INCONCLUSIVE\n\n" +
                "Recommendation: proceed with interrogation."
            ),
            (
                "VITAL SIGNS MONITOR",
                "CARDIAC:      78 BPM — normal range\n" +
                "RESPIRATORY:  16 / min\n" +
                "O₂ SAT:       98%\n\n" +
                "EEG PATTERN:\n" +
                "  Alpha waves:  9–13 Hz\n" +
                "  Beta surge:   stress marker detected\n\n" +
                "⚡ STRESS INDICATOR: Elevated cortisol pattern\n\n" +
                "Neural activity:       ABOVE BASELINE\n" +
                "Deception probability: 34%\n\n" +
                "Note: Robots show flat EEG.\n" +
                "Aliens show harmonic oscillation at 0.3 Hz offset."
            ),
            (
                "SYSTEM DATA // TERMINAL 7741",
                "SECURITY LEVEL: 3  (CLASSIFIED)\n" +
                "ZONE:           VOID PERIMETER\n" +
                "SHIFT:          DAY 1 — INSPECTOR ACTIVE\n\n" +
                "THREAT MATRIX:\n" +
                "  Human infiltrators:  LOW\n" +
                "  Synthetic entities:  MEDIUM ⚠\n" +
                "  Alien contacts:      MEDIUM ⚠\n" +
                "  Unknown:             MONITORING\n\n" +
                "DAILY QUOTA:      3 subjects\n" +
                "ERROR TOLERANCE:  3 mistakes\n\n" +
                "ALERT: AI uprising activity in sector 7.\n" +
                "Heightened vigilance required.\n" +
                "Synthetics are learning to mimic humans."
            ),
            (
                "SYSTEM LOG // VOID TERMINAL",
                "[06:00:01] Boot sequence complete\n" +
                "[06:00:02] Auth module:          ONLINE\n" +
                "[06:00:05] Biometric scanner:    READY\n" +
                "[06:00:08] Network:              SECURE (AES-256)\n" +
                "[06:01:14] First subject detected at perimeter\n" +
                "[06:01:30] Pressure timer:       INITIALIZED\n" +
                "[06:03:22] ANOMALY: Signal pattern non-human\n" +
                "[06:03:25] Cross-reference:      VOID DB queried\n" +
                "[06:03:28] Result: 47% match — INCONCLUSIVE\n" +
                "[06:04:00] WARNING: Deception protocol detected\n" +
                "[06:04:10] Awaiting inspector decision\n\n" +
                "SYSTEM NOMINAL. All modules operational.\n" +
                "Press ESC to exit."
            ),
            (
                "INSPECTOR NOTE #1",
                "CHECK EVERY SUBJECT:\n" +
                "  ✓ Access code\n" +
                "  ✓ Place of arrival\n" +
                "  ✓ Purpose of visit\n\n" +
                "HOW TO IDENTIFY A ROBOT:\n" +
                "  • Micro-pauses > 0.3 s between words\n" +
                "  • Too flat / even intonation\n" +
                "  • Biometrics: synthetic pauses\n\n" +
                "HOW TO IDENTIFY AN ALIEN:\n" +
                "  • Uses \"we / us\" instead of \"I / me\"\n" +
                "  • Harmonic drift in voice\n\n" +
                "❗ DO NOT LET THE VILLAIN PASS!\n" +
                "   Health = 3 mistakes maximum."
            ),
            (
                "ACCESS CODES // CURRENT",
                "ACTIVE CODES (Day 1):\n\n" +
                "  Alpha Zone:      7741-X\n" +
                "  Beta Zone:       3392-K\n" +
                "  VOID Perimeter:  ???? (CLASSIFIED)\n" +
                "  Medical:         MED-009\n\n" +
                "SHIFT SCHEDULE:\n" +
                "  Shift A:  06:00 — 14:00\n" +
                "  Shift B:  14:00 — 22:00\n" +
                "  Shift C:  22:00 — 06:00\n\n" +
                "⚠ Codes change every day!\n" +
                "  If a mismatch is detected — detain the subject.\n\n" +
                "Note: robots often use outdated codes."
            ),
            (
                "TRAIT REFERENCE TABLE",
                "🤖 ROBOT:\n" +
                "  • Synthetic speech, pauses > 0.3 s\n" +
                "  • Signal: \"synthetic cadence\"\n" +
                "  • No genuine emotions\n\n" +
                "👽 ALIEN:\n" +
                "  • Uses plural pronouns: \"we / us / our\"\n" +
                "  • Signal: \"harmonic drift\"\n" +
                "  • Non-human formants\n\n" +
                "👤 HUMAN:\n" +
                "  • Emotional stress variations\n" +
                "  • Signal: \"emotional micro-variance\"\n" +
                "  • Natural speech patterns\n\n" +
                "⚡ From Day 3 onward, their mimicry becomes significantly better!"
            ),
            (
                "SHIFT CHRONOMETER",
                "Current Shift: A  (06:00 — 14:00)\n\n" +
                "Pressure Timer indicates stress levels.\n" +
                "Hesitation leads to score penalties.\n\n" +
                "PENALTIES:\n" +
                "   0–60%:  No penalty\n" +
                "  60–70%:  −2 points\n" +
                "  70–80%:  −3 points\n" +
                "  80–90%:  −4 points\n" +
                "  90–100%: −5 points\n\n" +
                "HINT: Ask 2–3 key questions\n" +
                "and make your decision quickly."
            ),
            (
                "EMERGENCY PROTOCOL",
                "⚡ EMERGENCY NODE — TERMINAL POWER\n\n" +
                "STATUS: ONLINE\n" +
                "(Blue = Normal Operation)\n\n" +
                "In the event of a Villain Breach:\n" +
                "  • LOCKDOWN protocol activated\n" +
                "  • All exits sealed\n" +
                "  • Response team dispatched\n\n" +
                "⚠ 'Find the Villain' Mode:\n" +
                "  The Villain is hidden among today's subjects.\n" +
                "  Failure to intercept = GAME OVER.\n\n" +
                "Emergency Code: [CLASSIFIED]\n" +
                "Security Clearance: 3+"
            ),
            (
                "COMMUNICATION BLOCK",
                "FREQUENCIES:\n" +
                "  CH-A (Command):  156.8 MHz  ACTIVE ✓\n" +
                "  CH-B (Medical):  162.4 MHz  ACTIVE ✓\n" +
                "  CH-C (Perimeter): 171.0 MHz  SIGNAL INTERFERENCE ⚠\n" +
                "  Emergency:       121.5 MHz  STANDBY\n\n" +
                "INCOMING MESSAGES:\n" +
                "  > \"Perimeter-2: movement at the gates\"\n" +
                "  > \"Command: AI activity detected\"\n" +
                "  > \"Medical: Subject 3 — ECG anomaly\"\n\n" +
                "⚠ Channel C is unstable.\n" +
                "  AI interference probable.\n" +
                "  Use for emergency transmission only."
            ),
        };

        private Rectangle[] interactiveZones;

        public Form1()
        {
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.KeyPreview = true;

            typingTimer = new Timer { Interval = 30 };
            typingTimer.Tick += TypingTimer_Tick;

            _bufCtx = BufferedGraphicsManager.Current;
            ReallocBuffer();
            this.Resize += (s, e) => ReallocBuffer();

            interactiveZones = new[]
{
    zoneLeftTop,
    zoneLeftMiddle,
    zoneLeftBottom,

    zoneSticker1,
    zoneSticker2,
    zoneSticker3,
    zoneSticker4,

    zoneRightScreen,
    zoneBigRadio,
    zoneSmallRadio,
    zoneDialogueScreen
};

            LoadResources();
            CreateTransparentUI();
            CreateOverlay();
            todayCast = CharacterFactory.GenerateMixedCast(
    day,
    guaranteedHumans: 1,      // 1 точно человек из папки "Люди"
    guaranteedRobots: 0,
    guaranteedAliens: 0,
    randomTypeCount: 4        // 4 персонажа случайного типа из "Персонажи"
);
            currentCharacterIndex = 0;
            LoadCurrentCharacter();
            currentCharacterIndex++;

            if (currentCharacterIndex < todayCast.Count)
            {
                LoadCurrentCharacter();
            }
            else
            {
                ShowDaySummary();
            }

            shutterTimer = new Timer { Interval = 15 };
            shutterTimer.Tick += ShutterTimer_Tick;

            OverlayManagerInstance = new OverlayManager(this);

            flashTimer = new Timer { Interval = 30 };
            flashTimer.Tick += FlashTimer_Tick;

            typingTimer = new Timer { Interval = 30 };
            typingTimer.Tick += TypingTimer_Tick;

            pressureTimer = new Timer { Interval = 1000 };
            pressureTimer.Tick += (s, e) =>
            {
                if (pressureSeconds < 60) pressureSeconds++;
                UpdatePressureUI();
            };



            this.MouseClick += Form_MouseClick;
            this.MouseMove += Form_MouseMove;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (OverlayManagerInstance != null && OverlayManagerInstance.IsVisible)
                    {
                        OverlayManagerInstance.Hide();
                        e.Handled = true;
                        return;
                    }
                    else Application.Exit();
                }
            };

            pressureTimer.Start();
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            int newHovered = -1;

            for (int i = 0; i < interactiveZones.Length; i++)
            {
                if (ScaleRect(interactiveZones[i]).Contains(e.Location))
                {
                    newHovered = i;
                    break;
                }
            }

            if (hoveredZone != newHovered)
            {
                hoveredZone = newHovered;
                Cursor = hoveredZone >= 0 ? Cursors.Hand : Cursors.Default;
                Redraw();
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  БУФЕР
        // ═══════════════════════════════════════════════════════════════════
        private void ReallocBuffer()
        {
            _buffer?.Dispose();
            _bufCtx.MaximumBuffer = new Size(
                Math.Max(BaseW, ClientSize.Width) + 1,
                Math.Max(BaseH, ClientSize.Height) + 1);
            if (ClientSize.Width > 0 && ClientSize.Height > 0)
                _buffer = _bufCtx.Allocate(
                    this.CreateGraphics(),
                    new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
        }

        // ═══════════════════════════════════════════════════════════════════
        //  РЕСУРСЫ
        // ═══════════════════════════════════════════════════════════════════
        private void LoadResources()
        {
            string bg = Path.Combine(Application.StartupPath, "Images", "Background");
            backBackground = LoadImg(Path.Combine(bg, "back_panel.png"));
            frontBackground = LoadImg(Path.Combine(bg, "front_panel.png"));
            btnDefault = LoadImg(Path.Combine(bg, "buttons_default.png"));
            btnRed = LoadImg(Path.Combine(bg, "buttons_red.png"));
            btnBlue = LoadImg(Path.Combine(bg, "buttons_blue.png"));
            btnGreen = LoadImg(Path.Combine(bg, "buttons_green.png"));
            currentBtnImage = btnDefault;
        }

        private Image LoadImg(string path) =>
            File.Exists(path) ? Image.FromFile(path) : null;

        // ═══════════════════════════════════════════════════════════════════
        //  ПРОЗРАЧНЫЕ LABEL-ЭЛЕМЕНТЫ
        // ═══════════════════════════════════════════════════════════════════
        private void CreateTransparentUI()
        {
            this.Controls.Clear();

            lblScore = MakeLabel("📊 0", Color.Gold, new Point(400, 10), 140);
            lblHealth = MakeLabel("❤️ 3", Color.Tomato, new Point(550, 10), 100);
            lblDay = MakeLabel("📅 DAY 1", Color.Cyan, new Point(660, 10), 130);
            lblQuota = MakeLabel("📋 0/3", Color.White, new Point(800, 10), 110);

            lblPressure = new Label
            {
                Text = "PRESSURE:   0%  [░░░░░░░░░░]",
                Location = new Point(400, 32),
                Size = new Size(540, 18),
                ForeColor = Color.FromArgb(180, 0, 255, 255),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ПРОЗРАЧНОЕ ОКНО ДЛЯ ИМЕНИ (с полупрозрачным тёмным фоном)
            lblName = new Label
            {
                Text = "OBJECT APPROACHING...",
                Location = new Point(400, 464),
                Size = new Size(565, 26),
                ForeColor = Color.FromArgb(255, 230, 230, 230),
                BackColor = Color.FromArgb(100, 0, 0, 0), // Полупрозрачный чёрный
                Font = new Font("Consolas", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ПРОЗРАЧНОЕ ОКНО ДЛЯ ДИАЛОГОВ (с полупрозрачным тёмным фоном)
            lblDialogue = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(0, 255, 120),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 4, 10, 4)
            };

            Rectangle zone = ScaleRect(zoneDialogueScreen);
            lblDialogue.Location = zone.Location;
            lblDialogue.Size = zone.Size;

            foreach (var c in new Control[]
                { lblScore, lblHealth, lblDay, lblQuota, lblPressure, lblName, lblDialogue })
                ScaleControl(c);

            this.Controls.AddRange(new Control[]
                { lblScore, lblHealth, lblDay, lblQuota, lblPressure, lblName, lblDialogue });
        }

        private void ScaleControl(Control c)
        {
            float sx = (float)Screen.PrimaryScreen.Bounds.Width / BaseW;
            float sy = (float)Screen.PrimaryScreen.Bounds.Height / BaseH;
            c.Location = new Point((int)(c.Left * sx), (int)(c.Top * sy));
            c.Size = new Size((int)(c.Width * sx), (int)(c.Height * sy));
        }

        private Label MakeLabel(string text, Color color, Point loc, int width) =>
            new Label
            {
                Text = text,
                ForeColor = color,
                BackColor = Color.Transparent,
                Location = loc,
                Size = new Size(width, 24),
                Font = new Font("Consolas", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

        // ═══════════════════════════════════════════════════════════════════
        //  OVERLAY — всплывающее окно поверх всего
        // ═══════════════════════════════════════════════════════════════════
        private void CreateOverlay()
        {
            overlayPanel = new Panel
            {
                BackColor = Color.FromArgb(210, 0, 0, 0),
                Visible = false,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Default
            };
            overlayPanel.Click += (s, e) => HideOverlay();

            int cardW = 620, cardH = 420;
            var card = new Panel
            {
                BackColor = Color.FromArgb(255, 13, 16, 21),
                Size = new Size(cardW, cardH),
                Location = new Point(
                    (int)((BaseW - cardW) / 2 * Screen.PrimaryScreen.Bounds.Width / (float)BaseW),
                    (int)((BaseH - cardH) / 2 * Screen.PrimaryScreen.Bounds.Height / (float)BaseH)),
                Cursor = Cursors.Default
            };
            card.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(255, 51, 102, 170), 2))
                    g.DrawRectangle(pen, 1, 1, card.Width - 2, card.Height - 2);
                using (var br = new LinearGradientBrush(
                    new Rectangle(0, 0, card.Width, 3),
                    Color.FromArgb(180, 51, 102, 170), Color.Transparent, LinearGradientMode.Vertical))
                    g.FillRectangle(br, 0, 0, card.Width, 3);
            };
            card.Click += (s, e) => { };
            card.MouseClick += (s, e) => { };

            overlayTitle = new Label
            {
                Location = new Point(20, 18),
                Size = new Size(cardW - 60, 26),
                ForeColor = Color.FromArgb(255, 102, 170, 238),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var divider = new Label
            {
                Location = new Point(20, 46),
                Size = new Size(cardW - 40, 1),
                BackColor = Color.FromArgb(60, 51, 102, 170)
            };

            overlayBody = new Label
            {
                Location = new Point(20, 56),
                Size = new Size(cardW - 40, cardH - 80),
                ForeColor = Color.FromArgb(255, 153, 187, 221),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10),
                TextAlign = ContentAlignment.TopLeft
            };

            overlayClose = new Button
            {
                Text = "✕",
                Location = new Point(cardW - 38, 10),
                Size = new Size(26, 26),
                ForeColor = Color.FromArgb(120, 100, 130, 170),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            overlayClose.FlatAppearance.BorderSize = 0;
            overlayClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 51, 102, 170);
            overlayClose.Click += (s, e) => HideOverlay();

            card.Controls.AddRange(new Control[] { overlayTitle, divider, overlayBody, overlayClose });
            overlayPanel.Controls.Add(card);
            this.Controls.Add(overlayPanel);
            overlayPanel.BringToFront();
        }


        private void ShowOverlay(int index)
        {
            // Если кликнули по зоне стикера 1 (индекс 3 в вашем массиве interactiveZones)
            if (index == 3)
            {
                using (var checklist = new ChecklistForm())
                {
                    checklist.ShowDialog(this);
                }
                return;
            }

            // Для остальных зон берем динамический текст
            var note = NoteManager.GetDynamicNote(index, currentCharacterData);

            overlayTitle.Text = note.Title;
            overlayBody.Text = note.Body;
            overlayPanel.Visible = true;
            overlayPanel.BringToFront();
        }

        private void HideOverlay()
        {
            overlayPanel.Visible = false;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ПЕРСОНАЖ
        // ═══════════════════════════════════════════════════════════════════
        private void LoadCurrentCharacter()
        {
            if (todayCast == null || currentCharacterIndex >= todayCast.Count)
                return;

            currentCharacterData = todayCast[currentCharacterIndex];

            currentCharacter?.Dispose();
            currentCharacter = currentCharacterData.Photo;

            lblName.Text = currentCharacterData.Name;
            StartTypingEffect(currentCharacterData.Dialogue);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ЭФФЕКТ ПЕЧАТАЮЩЕЙ МАШИНКИ
        // ═══════════════════════════════════════════════════════════════════
        private void StartTypingEffect(string text)
        {
            typingTimer.Stop();
            fullDialogueText = text;
            typingIndex = 0;
            lblDialogue.Text = "";
            typingTimer.Start();
        }


        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (typingIndex < fullDialogueText.Length)
            {
                lblDialogue.Text += fullDialogueText[typingIndex];
                typingIndex++;
            }
            else
            {
                typingTimer.Stop();
            }
        }


        // ═══════════════════════════════════════════════════════════════════
        //  КЛИК МЫШИ
        // ═══════════════════════════════════════════════════════════════════
        private async void Form_MouseClick(object sender, MouseEventArgs e)
        {
            if (overlayPanel.Visible) return;

            Point p = e.Location;

            // 1. Проверяем интерактивные зоны
            for (int i = 0; i < interactiveZones.Length; i++)
            {
                if (ScaleRect(interactiveZones[i]).Contains(p))
                {
                    // Рация открывает интерактивное окно с вопросами
                    if (i == 9) // zoneRadioPanel
                    {
                        ShowQuestionDialog();
                        return;
                    }
                    ShowOverlay(i);
                    return;
                }
                if (ScaleRect(zoneDialogueScreen).Contains(p))
                {
                    OpenCharacterDialogue();
                    return;
                }
            }

            // 2. Проверяем кнопки решения
            if (isAnimating) return;

            Image nextBtn = null;
            Color fColor = Color.Transparent;
            string decision = "";

            if (ScaleRect(redZoneBase).Contains(p))
            {
                nextBtn = btnRed;
                fColor = Color.Red;
                decision = "ROBOT";
                dailyDecisions.Add((currentCharacterData, decision));
            }
            else if (ScaleRect(blueZoneBase).Contains(p))
            {
                nextBtn = btnBlue;
                fColor = Color.DodgerBlue;
                decision = "ALIEN";
                dailyDecisions.Add((currentCharacterData, decision));
            }
            else if (ScaleRect(greenZoneBase).Contains(p))
            {
                nextBtn = btnGreen;
                fColor = Color.Lime;
                decision = "HUMAN";
                dailyDecisions.Add((currentCharacterData, decision));
            }

            if (nextBtn == null) return;

            isAnimating = true;
            currentBtnImage = nextBtn;
            pressureTimer.Stop();

            StartTypingEffect($"Decision: {decision}. Processing...");
            charactersChecked++;

            if (charactersChecked >= dailyQuota)
            {
                await Task.Delay(1500);
                ShowDaySummary();
                return;
            }

            UpdateStatsUI();
            StartFlash(fColor);
            Redraw();

            await Task.Delay(250);
            currentBtnImage = btnDefault;

            isClosing = true;
            shutterTimer.Start();
        }
        private void OpenCharacterDialogue() => ShowQuestionDialog();

        
        // ═══════════════════════════════════════════════════════════════════
        //  ЗАМЕНИТЕ метод GenerateAnswer в Form1.cs на этот:
        // ═══════════════════════════════════════════════════════════════════

        private async void ShowQuestionDialog()
        {
            var questionForm = new Form
            {
                Text = "INTERROGATION PROTOCOL",
                Size = new Size(500, 400),
                BackColor = Color.FromArgb(15, 20, 25),
                ForeColor = Color.FromArgb(200, 220, 240),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Font = new Font("Consolas", 10)
            };

            var lblInfo = new Label
            {
                Text = "Select question to ask the subject:",
                Location = new Point(20, 20),
                Size = new Size(440, 30),
                ForeColor = Color.Cyan
            };

            var questions = new[]
            {
        "What is your access code?",
        "Where are you coming from?",
        "What is your purpose here?",
        "Do you have family?",
        "How do you feel today?"
    };

            int y = 60;
            foreach (var q in questions)
            {
                var btn = new Button
                {
                    Text = q,
                    Location = new Point(20, y),
                    Size = new Size(440, 35),
                    BackColor = Color.FromArgb(30, 50, 70),
                    ForeColor = Color.FromArgb(200, 220, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 9),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(51, 102, 170);
                btn.Click += (s, e) =>
                {
                    questionForm.Close();
                    string answer = CharacterAI.GenerateAnswer(currentCharacterData, q);
                    StartTypingEffect(answer);
                };
                questionForm.Controls.Add(btn);
                y += 45;
            }

            questionForm.Controls.Add(lblInfo);
            questionForm.ShowDialog(this);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ШТОРКА
        // ═══════════════════════════════════════════════════════════════════
        private void ShutterTimer_Tick(object sender, EventArgs e)
        {
            if (isClosing)
            {
                shutterHeight = Math.Min(shutterMaxHeight, shutterHeight + 35);
                if (shutterHeight >= shutterMaxHeight)
                {
                    isClosing = false;
                    currentCharacterIndex++;

                    if (currentCharacterIndex < todayCast.Count)
                    {
                        LoadCurrentCharacter();
                    }
                    else
                    {
                        ShowDaySummary();
                    }
                    pressureSeconds = 0;
                    pressureTimer.Start();
                }
            }
            else
            {
                shutterHeight = Math.Max(0, shutterHeight - 35);
                if (shutterHeight <= 0)
                {
                    shutterTimer.Stop();
                    isAnimating = false;
                }
            }
            Redraw();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ВСПЫШКА
        // ═══════════════════════════════════════════════════════════════════
        private void StartFlash(Color color)
        {
            flashColor = color;
            flashAlpha = 90;
            if (!flashTimer.Enabled) flashTimer.Start();
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            flashAlpha -= 10;
            if (flashAlpha <= 0) { flashAlpha = 0; flashTimer.Stop(); }
            Redraw();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  UI — ОБНОВЛЕНИЕ
        // ═══════════════════════════════════════════════════════════════════
        private void UpdateStatsUI()
        {
            if (lblScore == null) return;
            lblScore.Text = $"📊 {score}";
            lblHealth.Text = $"❤️ {health}";
            lblDay.Text = $"📅 DAY {day}";
            lblQuota.Text = $"📋 {charactersChecked}/{dailyQuota}";
            lblHealth.ForeColor = health <= 1 ? Color.Red : Color.Tomato;
        }

        private void ShowDaySummary()
        {
            pressureTimer.Stop();

            // Создаём и показываем форму
            var summaryForm = new DaySummaryForm(
                day,
                score,
                health,
                charactersChecked,
                dailyQuota,
                dailyDecisions
            );

            // Очищаем решения для следующего дня
            dailyDecisions.Clear();

            var result = summaryForm.ShowDialog(this);

            if (summaryForm.ContinueToNextDay)
            {
                StartNextDay();
            }
            else
            {
                // Возврат в меню (можно просто закрыть игру или показать WelcomeForm)
                this.Close();
                // Если хотите вернуться в меню, замените на:
                // Application.Restart();
            }
        }
        private void GenerateNewDay()
        {
            todayCast = CharacterFactory.GenerateDayCast(
                day,
                humanCount: 3,
                robotCount: day >= 2 ? 1 : 0,
                alienCount: day >= 3 ? 1 : 0
            );

            currentCharacterIndex = 0;
            LoadCurrentCharacter();
        }

        private void StartNextDay()
        {
            day++;
            charactersChecked = 0;
            pressureSeconds = 0;
            dailyQuota = Math.Min(3 + (day - 1), 10);

            // Генерируем новый состав на следующий день
            todayCast = CharacterFactory.GenerateMixedCast(
                day,
                guaranteedHumans: 1,
                guaranteedRobots: 0,
                guaranteedAliens: 0,
                randomTypeCount: 4
            );

            currentCharacterIndex = 0;
            LoadCurrentCharacter();

            lblDialogue.ForeColor = Color.Lime;
            UpdateStatsUI();
            pressureTimer.Start();
            Redraw();
        }

        private void UpdatePressureUI()
        {
            int pct = Math.Min(100, (int)(pressureSeconds * 100f / 60));
            string bar = new string('█', pct / 10) + new string('░', 10 - pct / 10);
            Color c = pct < 50 ? Color.FromArgb(180, 0, 255, 255)
                    : pct < 75 ? Color.FromArgb(210, 255, 190, 0)
                    : Color.FromArgb(230, 255, 60, 80);
            lblPressure.ForeColor = c;
            lblPressure.Text = $"PRESSURE: {pct,3}%  [{bar}]";
        }
        private string GetCharacterMonitorText()
        {
            if (currentCharacterData == null)
                return "NO SUBJECT";

            return
                $"NAME: {currentCharacterData.Name}\n" +
                $"TYPE: {currentCharacterData.Species}\n" +
                $"JOB: {currentCharacterData.Occupation}\n" +
                $"REASON: {currentCharacterData.ReasonToEnter}\n" +
                $"DAY: {currentCharacterData.Day}";
        }
        

        // ═══════════════════════════════════════════════════════════════════
        //  РЕНДЕР
        // ═══════════════════════════════════════════════════════════════════
        private void Redraw()
        {
            if (_buffer == null) return;
            Graphics g = _buffer.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            if (backBackground != null)
                g.DrawImage(backBackground, 0, 0, ClientSize.Width, ClientSize.Height);

            if (currentCharacter != null)
            {
                Rectangle dest = FitInRect(currentCharacter.Size, ScaleRect(monitorRectBase));
                g.DrawImage(currentCharacter, dest);
            }

            DrawShutter(g);
            DrawClock(g);
            DrawMonitorText(g);


            if (frontBackground != null)
                g.DrawImage(frontBackground, 0, 0, ClientSize.Width, ClientSize.Height);

            DrawInteractiveGlow(g);

            if (currentBtnImage != null)
                g.DrawImage(currentBtnImage, 0, 0, ClientSize.Width, ClientSize.Height);

            if (flashAlpha > 0 && flashColor != Color.Transparent)
            {
                using (var br = new SolidBrush(Color.FromArgb(flashAlpha, flashColor)))
                    g.FillRectangle(br, 0, 0, ClientSize.Width, ClientSize.Height);
            }

            _buffer.Render();

        }

        private void DrawMonitorText(Graphics g)
        {
            Rectangle rect = ScaleRect(zoneRightScreen);

            using (Font font = new Font("Consolas", 9, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.Lime))
            {
                g.DrawString(GetCharacterMonitorText(), font, brush, rect);
            }

        }
        private void DrawInteractiveGlow(Graphics g)
        {
            if (hoveredZone < 0) return;

            Rectangle r = ScaleRect(interactiveZones[hoveredZone]);

            using (GraphicsPath path = RoundedRect(r, 10))
            {
                for (int i = 8; i >= 1; i--)
                {
                    using (Pen p = new Pen(Color.FromArgb(18 * i, 0, 255, 255), i))
                    {
                        g.DrawPath(p, path);
                    }
                }

                using (SolidBrush br = new SolidBrush(Color.FromArgb(18, 0, 255, 255)))
                {
                    g.FillPath(br, path);
                }
            }
        }
        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }
        private string[] GetDayStickerTexts()
        {
            return new[]
            {
        $"DAY {day}",
        $"CODE D-{day}X",
        day < 3 ? "CHECK ID" : "CHECK SPEECH",
        $"THREAT {Math.Min(day, 5)}"
    };
        }

        private void DrawStickerTexts(Graphics g)
        {
            var texts = GetDayStickerTexts();

            Rectangle[] zones =
            {
        ScaleRect(zoneSticker1),
        ScaleRect(zoneSticker2),
        ScaleRect(zoneSticker3),
        ScaleRect(zoneSticker4)
    };

            using (Font font = new Font("Consolas", 8, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    g.DrawString(texts[i], font, brush, zones[i]);
                }
            }
        }

        private void DrawClock(Graphics g)
        {
            Rectangle zone = ScaleRect(zoneClock);
            int m = shiftSeconds / 60, s = shiftSeconds % 60;
            string time = $"{m:D2}:{s:D2}";

            using (var font = new Font("Consolas", zone.Height * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var br = new SolidBrush(Color.FromArgb(255, 0, 170, 255)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(time, font, br, zone, sf);
            }
        }

        private void DrawShutter(Graphics g)
        {
            if (shutterHeight <= 0) return;

            Rectangle monitor = ScaleRect(monitorRectBase);
            float sy = (float)ClientSize.Height / BaseH;
            int scaledH = (int)(shutterHeight * sy);

            const int slats = 12;
            int slatH = Math.Max(1, scaledH / slats);

            for (int i = 0; i < slats; i++)
            {
                int top = monitor.Y + i * slatH;
                int visible = Math.Min(slatH, scaledH - i * slatH);
                if (visible <= 0) break;

                using (var br = new LinearGradientBrush(
                    new Rectangle(monitor.X, top, monitor.Width, Math.Max(1, slatH)),
                    Color.FromArgb(210, 215, 225),
                    Color.FromArgb(70, 75, 85),
                    LinearGradientMode.Vertical))
                {
                    br.InterpolationColors = new ColorBlend(4)
                    {
                        Colors = new[] { Color.FromArgb(200,205,215), Color.FromArgb(240,245,255),
                                            Color.FromArgb(150,155,165), Color.FromArgb(65,70,80) },
                        Positions = new[] { 0f, 0.35f, 0.65f, 1f }
                    };
                    g.FillRectangle(br, new Rectangle(monitor.X, top, monitor.Width, visible));
                }

                int blickH = Math.Min(slatH / 4, visible - 2);
                if (blickH > 0)
                {
                    using (var br = new LinearGradientBrush(
                        new Rectangle(monitor.X, top + 2, monitor.Width, blickH + 1),
                        Color.FromArgb(80, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255),
                        LinearGradientMode.Vertical))
                        g.FillRectangle(br, new Rectangle(monitor.X, top + 2, monitor.Width, blickH));
                }

                using (var p = new Pen(Color.FromArgb(40, 30, 32), 2))
                    g.DrawLine(p, monitor.X, top + visible - 1, monitor.Right, top + visible - 1);
                using (var p = new Pen(Color.FromArgb(55, 255, 255, 255), 1))
                    g.DrawLine(p, monitor.X, top, monitor.Right, top);
            }

            var area = new Rectangle(monitor.X, monitor.Y, monitor.Width, scaledH);
            using (var sh = new LinearGradientBrush(area,
                Color.FromArgb(100, 0, 0, 0), Color.Transparent, LinearGradientMode.Horizontal))
                g.FillRectangle(sh, new Rectangle(monitor.X, monitor.Y, 20, scaledH));

            using (var sh = new LinearGradientBrush(area,
                Color.Transparent, Color.FromArgb(100, 0, 0, 0), LinearGradientMode.Horizontal))
                g.FillRectangle(sh, new Rectangle(monitor.Right - 20, monitor.Y, 20, scaledH));

            using (var p = new Pen(Color.FromArgb(180, 20, 22, 28), 3))
                g.DrawLine(p, monitor.X, monitor.Y + scaledH - 1, monitor.Right, monitor.Y + scaledH - 1);
        }

        private Rectangle ScaleRect(Rectangle r)
        {
            float sx = (float)ClientSize.Width / BaseW;
            float sy = (float)ClientSize.Height / BaseH;
            return new Rectangle(
                (int)Math.Round(r.X * sx), (int)Math.Round(r.Y * sy),
                (int)Math.Round(r.Width * sx), (int)Math.Round(r.Height * sy));
        }

        private Rectangle FitInRect(Size src, Rectangle dest)
        {
            float scale = Math.Min((float)dest.Width / src.Width, (float)dest.Height / src.Height);
            int w = (int)(src.Width * scale), h = (int)(src.Height * scale);
            return new Rectangle(
                dest.X + (dest.Width - w) / 2,
                dest.Y + (dest.Height - h) / 2,
                w, h);
        }

        protected override void OnPaint(PaintEventArgs e) => Redraw();
        protected override void OnPaintBackground(PaintEventArgs e) { }
    }
}
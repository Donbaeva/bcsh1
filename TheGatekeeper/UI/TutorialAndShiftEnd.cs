using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TheGatekeeper
{
    // ═══════════════════════════════════════════════════════════════════════
    //  TutorialForm — экран до начала смены
    //
    //  Показывается ОДИН РАЗ (только День 1) перед LoadCurrentCharacter().
    //  Игрок видит пустой пост. Нажимает на зоны — получает подсказки.
    //  Потом нажимает "START SHIFT" — появляется первый персонаж.
    //
    //  КАК ВЫЗВАТЬ: в Form1.cs в методе InitModeSession() для StoryMode,
    //  перед строкой LoadCurrentCharacter(), добавить:
    //
    //      if (day == 1 && currentMode == GameMode.StoryMode)
    //      {
    //          var tut = new TutorialForm();
    //          tut.ShowDialog(this);
    //      }
    // ═══════════════════════════════════════════════════════════════════════
    public class TutorialForm : Form
    {
        private Label _lblHint;
        private Button _btnStart;
        private bool _startEnabled = false;
        private int _zonesClicked = 0;
        private const int ZonesNeeded = 3; // minimum 3 zones to proceed кнопку

        // Зоны с подсказками (координаты условные для 900×600 формы)
        private readonly (Rectangle Zone, string Title, string Hint)[] _zones =
        {
            (
                new Rectangle(60, 140, 150, 100),
                "MONITOR — DOCUMENTS",
                "Subject documents are displayed here.\n" +
                "Click to verify name, occupation\n" +
                "and access code. Compare with what the subject says."
            ),
            (
                new Rectangle(60, 280, 180, 60),
                "ECG — CARDIAC MONITOR",
                "Subject ECG readout.\n" +
                "Robots have mechanical pauses between peaks.\n" +
                "Aliens show irregular waveforms.\n" +
                "Humans have normal sinus rhythm."
            ),
            (
                new Rectangle(60, 370, 85, 65),
                "DOSIMETER",
                "Measures subject radiation signature.\n" +
                "WARNING: unit offline today\n" +
                "due to scheduled maintenance.\n" +
                "Readings unreliable — do not use today."
            ),
            (
                new Rectangle(600, 470, 220, 80),
                "RADIO — TRANSMISSIONS",
                "Incoming messages from command,\n" +
                "medical bay, and unknown sources.\n" +
                "Listen carefully — critical intel\n" +
                "sometimes comes through here."
            ),
            (
                new Rectangle(700, 300, 180, 100),
                "INTERROGATION — SMALL RADIO",
                "Press to ask the subject a question.\n" +
                "Ask about access code, family,\n" +
                "origin, purpose of visit.\n" +
                "Answers are saved in the log."
            ),
            (
                new Rectangle(55, 530, 105, 85),
                "VERDICT BUTTONS",
                "🔴 RED   = subject is ROBOT\n" +
                "🔵 BLUE  = subject is ALIEN\n" +
                "🟢 GREEN = subject is HUMAN\n\n" +
                "Wrong verdict = penalty."
            ),
        };

        private int _hoveredZone = -1;

        public TutorialForm()
        {
            this.Text = "BRIEFING — DAY 1";
            this.Size = new Size(900, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(8, 10, 16);
            this.DoubleBuffered = true;

            // Подсказка снизу
            _lblHint = new Label
            {
                Text = "← Click any highlighted zone to learn what it does",
                Location = new Point(20, 530),
                Size = new Size(680, 60),
                ForeColor = Color.FromArgb(180, 0, 200, 120),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(_lblHint);

            // Кнопка "Начать смену" — заблокирована пока не нажато несколько зон
            _btnStart = new Button
            {
                Text = "START SHIFT",
                Location = new Point(710, 545),
                Size = new Size(160, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 100, 120),
                BackColor = Color.FromArgb(15, 20, 30),
                Cursor = Cursors.Default,
                Enabled = false
            };
            _btnStart.FlatAppearance.BorderColor = Color.FromArgb(40, 60, 80);
            _btnStart.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            this.Controls.Add(_btnStart);

            this.MouseMove += (s, e) =>
            {
                int prev = _hoveredZone;
                _hoveredZone = -1;
                for (int i = 0; i < _zones.Length; i++)
                    if (_zones[i].Zone.Contains(e.Location)) { _hoveredZone = i; break; }
                if (_hoveredZone != prev) this.Invalidate();
                this.Cursor = _hoveredZone >= 0 ? Cursors.Hand : Cursors.Default;
            };

            this.MouseClick += (s, e) =>
            {
                for (int i = 0; i < _zones.Length; i++)
                {
                    if (_zones[i].Zone.Contains(e.Location))
                    {
                        ShowZoneHint(i);
                        return;
                    }
                }
            };

            this.Paint += OnPaint;
        }

        private void ShowZoneHint(int index)
        {
            var (_, title, hint) = _zones[index];

            _lblHint.Text = $"[ {title} ]\n{hint}";
            _lblHint.ForeColor = Color.FromArgb(220, 0, 220, 120);

            _zonesClicked++;
            if (_zonesClicked >= ZonesNeeded && !_startEnabled)
            {
                _startEnabled = true;
                _btnStart.Enabled = true;
                _btnStart.ForeColor = Color.FromArgb(0, 220, 120);
                _btnStart.BackColor = Color.FromArgb(15, 40, 25);
                _btnStart.FlatAppearance.BorderColor = Color.FromArgb(0, 160, 80);
                _btnStart.Cursor = Cursors.Hand;

                // Мигание кнопки чтобы привлечь внимание
                var blinkTimer = new Timer { Interval = 400 };
                int blinks = 0;
                blinkTimer.Tick += (s, e) =>
                {
                    blinks++;
                    _btnStart.BackColor = blinks % 2 == 0
                        ? Color.FromArgb(15, 40, 25)
                        : Color.FromArgb(5, 60, 35);
                    if (blinks >= 6) blinkTimer.Stop();
                };
                blinkTimer.Start();
            }

            this.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = this.Width, h = this.Height;

            // Фон с лёгким градиентом
            using (var br = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(8, 10, 16),
                Color.FromArgb(5, 8, 14),
                LinearGradientMode.Vertical))
                g.FillRectangle(br, 0, 0, w, h);

            // Рамка формы
            using (var pen = new Pen(Color.FromArgb(60, 51, 102, 170), 2))
                g.DrawRectangle(pen, 1, 1, w - 2, h - 2);

            // Угловые акценты
            using (var pen = new Pen(Color.FromArgb(160, 51, 130, 200), 2))
            {
                int a = 20;
                g.DrawLine(pen, 0, 0, a, 0); g.DrawLine(pen, 0, 0, 0, a);
                g.DrawLine(pen, w - a, 0, w, 0); g.DrawLine(pen, w, 0, w, a);
                g.DrawLine(pen, 0, h - a, 0, h); g.DrawLine(pen, 0, h, a, h);
                g.DrawLine(pen, w - a, h, w, h); g.DrawLine(pen, w, h - a, w, h);
            }

            // Заголовок
            using (var font = new Font("Consolas", 16, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(200, 100, 180, 255)))
            {
                g.DrawString("═══  DAY 1 — SHIFT START  ═══", font, brush, new Point(20, 18));
            }

            // Подзаголовок
            using (var font = new Font("Consolas", 9))
            using (var brush = new SolidBrush(Color.FromArgb(140, 120, 140, 160)))
            {
                g.DrawString(
                    "You are a gate inspector on an orbital station. Your task — identifyть кто перед тобой: человек, робот или пришелец.\n" +
                    "Click the highlighted zones below to does.",
                    font, brush, new Rectangle(20, 52, w - 40, 40));
            }

            // Разделитель
            using (var pen = new Pen(Color.FromArgb(40, 51, 102, 170), 1))
                g.DrawLine(pen, 20, 96, w - 20, 96);

            // Схематичный "стол инспектора" (упрощённая визуализация)
            DrawDeskSchema(g, w, h);

            // Зоны кликов
            for (int i = 0; i < _zones.Length; i++)
            {
                var zone = _zones[i].Zone;
                bool hovered = (i == _hoveredZone);

                // Пульсирующая рамка зоны
                Color zoneColor = hovered
                    ? Color.FromArgb(200, 0, 220, 140)
                    : Color.FromArgb(100, 0, 160, 100);

                using (var pen = new Pen(zoneColor, hovered ? 2 : 1.5f))
                {
                    // Пунктирная рамка
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(pen, zone);
                }

                // Заливка при наведении
                if (hovered)
                {
                    using (var br = new SolidBrush(Color.FromArgb(30, 0, 200, 100)))
                        g.FillRectangle(br, zone);
                }

                // Иконка-подсказка "?"
                using (var font = new Font("Consolas", 11, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(hovered ? 220 : 100, 0, 200, 120)))
                {
                    string icon = GetZoneIcon(i);
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(icon, font, brush, zone, sf);
                }

                // Стрелка + название зоны
                DrawZoneLabel(g, zone, _zones[i].Title, hovered);
            }

            // Прогресс — "изучи зоны"
            if (!_startEnabled)
            {
                int needed = ZonesNeeded - Math.Min(_zonesClicked, ZonesNeeded);
                using (var font = new Font("Consolas", 9, FontStyle.Italic))
                using (var brush = new SolidBrush(Color.FromArgb(100, 120, 120, 140)))
                    g.DrawString(
                        $"Study {needed} more zone{(needed == 1 ? "" : "s")} to unlock the shift.",
                        font, brush, new Point(710, 535));
            }
        }

        private void DrawDeskSchema(Graphics g, int w, int h)
        {
            // Стол инспектора — упрощённая схема в нижней части
            // Рисуем силуэты оборудования чтобы игрок понимал к чему относятся зоны

            // Основание стола
            using (var pen = new Pen(Color.FromArgb(30, 51, 80, 120), 1))
                g.DrawRectangle(pen, 40, 120, w - 80, h - 200);

            // Монитор с документами (левая верхняя зона)
            DrawSchemaBox(g, 60, 140, 150, 100,
                "MONITOR\nDOCS", Color.FromArgb(60, 100, 150, 200));

            // ЭКГ (левая средняя)
            DrawSchemaBox(g, 60, 280, 180, 60,
                "ECG", Color.FromArgb(60, 0, 180, 80));

            // Дозиметр (левая нижняя)
            DrawSchemaBox(g, 60, 370, 85, 65,
                "DOSIMETER\n⚠ offline", Color.FromArgb(40, 200, 120, 0));

            // Стикеры (правая верхняя часть)
            DrawSchemaBox(g, 640, 120, 220, 140,
                "STICKERS\nNOTES", Color.FromArgb(50, 200, 180, 0));

            // Радио большое
            DrawSchemaBox(g, 600, 470, 220, 80,
                "RADIO", Color.FromArgb(60, 180, 80, 200));

            // Малая рация (допрос)
            DrawSchemaBox(g, 700, 300, 180, 100,
                "INTERROGATE\n[SMALL RADIO]", Color.FromArgb(60, 80, 160, 220));

            // Кнопки решения
            DrawSchemaBox(g, 55, 530, 105, 85,
                "VERDICT\nBUTTONS", Color.FromArgb(50, 200, 50, 50));

            // Экран субъекта (центр)
            DrawSchemaBox(g, 270, 130, 320, 350,
                "SUBJECT\n(appears here\nfor inspection)",
                Color.FromArgb(25, 100, 100, 150));

            // Диалоговый экран
            DrawSchemaBox(g, 320, 470, 260, 55,
                "DIALOGUE — click to\nopen conversation log",
                Color.FromArgb(40, 0, 180, 100));
        }

        private void DrawSchemaBox(Graphics g, int x, int y, int bw, int bh,
            string label, Color tint)
        {
            using (var br = new SolidBrush(tint))
                g.FillRectangle(br, x, y, bw, bh);

            using (var pen = new Pen(Color.FromArgb(tint.A + 60, tint.R, tint.G, tint.B), 1))
                g.DrawRectangle(pen, x, y, bw, bh);

            using (var font = new Font("Consolas", 7.5f))
            using (var brush = new SolidBrush(Color.FromArgb(160, 180, 200, 220)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(label, font, brush, new RectangleF(x, y, bw, bh), sf);
            }
        }

        private void DrawZoneLabel(Graphics g, Rectangle zone, string title, bool hovered)
        {
            if (!hovered) return;

            using (var font = new Font("Consolas", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(200, 0, 230, 130)))
            {
                // Маленький тег над зоной
                g.DrawString($"► {title}", font, brush,
                    new Point(zone.X, zone.Y - 16));
            }
        }

        private string GetZoneIcon(int index)
        {
            switch (index)
            {
                case 0: return "📄";
                case 1: return "❤";
                case 2: return "☢";
                case 3: return "📻";
                case 4: return "❓";
                case 5: return "🔴🔵🟢";
                default: return "?";
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ShiftEndOverlay — надпись "Конец смены" поверх игры
    //
    //  КАК ВЫЗВАТЬ: в Form1.cs в OnCastExhausted() перед ShowDaySummary(),
    //  заменить прямой вызов на:
    //      ShowShiftEndOverlay(() => ShowDaySummary());
    // ═══════════════════════════════════════════════════════════════════════
    public static class ShiftEndMessages
    {
        // Вызывается из Form1 — показывает анимированную надпись "Конец смены"
        // callback вызывается когда надпись исчезает
        public static void Show(Form owner, int day, Action callback)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = true
            };
            panel.Paint += (s, pe) => DrawShiftEnd(pe.Graphics, panel.Width, panel.Height, day);
            owner.Controls.Add(panel);
            panel.BringToFront();

            // Через 2.5 секунды убираем и вызываем summary
            var timer = new Timer { Interval = 2500 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (!panel.IsDisposed)
                {
                    owner.Controls.Remove(panel);
                    panel.Dispose();
                }
                callback?.Invoke();
            };
            timer.Start();
        }

        private static void DrawShiftEnd(Graphics g, int w, int h, int day)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Тёмный полупрозрачный фон
            using (var br = new SolidBrush(Color.FromArgb(200, 5, 8, 14)))
                g.FillRectangle(br, 0, 0, w, h);

            // Горизонтальная линия сверху
            using (var pen = new Pen(Color.FromArgb(120, 51, 102, 200), 2))
            {
                g.DrawLine(pen, w / 4, h / 2 - 60, w * 3 / 4, h / 2 - 60);
                g.DrawLine(pen, w / 4, h / 2 + 60, w * 3 / 4, h / 2 + 60);
            }

            // Основной текст
            string mainText = "SHIFT COMPLETE";
            using (var font = new Font("Consolas", 28, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(220, 180, 220, 255)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(mainText, font, brush,
                    new RectangleF(0, h / 2 - 35, w, 40), sf);
            }

            // Подпись
            string subText = $"SHIFT ENDED — DAY {day}";
            using (var font = new Font("Consolas", 12))
            using (var brush = new SolidBrush(Color.FromArgb(140, 100, 140, 180)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(subText, font, brush,
                    new RectangleF(0, h / 2 + 10, w, 30), sf);
            }

            // Декоративные точки
            using (var brush = new SolidBrush(Color.FromArgb(80, 51, 102, 200)))
            {
                for (int i = 0; i < 5; i++)
                    g.FillEllipse(brush, w / 2 - 50 + i * 25, h / 2 + 48, 8, 8);
            }
        }
    }
}
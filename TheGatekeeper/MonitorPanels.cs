// ═══════════════════════════════════════════════════════════════════════
//  Form1_MonitorPanels.cs  — partial class Form1
//
//  Три независимых плавающих окна вместо оверлея для зон 0, 1, 2:
//    • MonitorPanel0 — общее состояние субъекта (зона zoneLeftTop)
//    • MonitorPanel1 — анимация пульса (зона zoneLeftMiddle)
//    • MonitorPanel2 — радиация / дозиметр (зона zoneLeftBottom)
//
//  Особенности:
//    - Перетаскиваются мышью
//    - Закрываются крестиком (не ESC, не клик мимо)
//    - При pressureSeconds > 39 (≈65%) — добавляются помехи на анимации
//    - Анимация обновляется таймером каждые 50 мс
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TheGatekeeper.Models;
using static TheGatekeeper.Models.Character;

namespace TheGatekeeper
{
    public partial class Form1
    {
        // ─── Ссылки на три открытых панели (null = закрыта) ─────────────
        private MonitorFloatPanel _panel0; // статус
        private MonitorFloatPanel _panel1; // пульс
        private MonitorFloatPanel _panel2; // радиация

        // ════════════════════════════════════════════════════════════════
        //  ОТКРЫТИЕ ПАНЕЛЕЙ — вызывается из ShowOverlay
        // ════════════════════════════════════════════════════════════════

        internal void OpenMonitorPanel(int index)
        {
            switch (index)
            {
                case 0: OpenOrFocus(ref _panel0, CreateStatusPanel); break;
                case 1: OpenOrFocus(ref _panel1, CreatePulsePanel); break;
                case 2: OpenOrFocus(ref _panel2, CreateRadiationPanel); break;
            }
        }

        private void OpenOrFocus(ref MonitorFloatPanel field,
                                 Func<MonitorFloatPanel> factory)
        {
            if (field != null && !field.IsDisposed)
            {
                field.BringToFront();
                return;
            }
            field = factory();
            field.Show(this);
        }

        // ════════════════════════════════════════════════════════════════
        //  ОБНОВЛЕНИЕ ПОМЕХ — вызывается из pressureTimer
        // ════════════════════════════════════════════════════════════════

        internal void UpdateMonitorPanelNoise()
        {
            float noiseLevel = GetNoiseLevel();
            UpdatePanel(_panel0, noiseLevel);
            UpdatePanel(_panel1, noiseLevel);
            UpdatePanel(_panel2, noiseLevel);
        }

        private void UpdatePanel(MonitorFloatPanel p, float noise)
        {
            if (p != null && !p.IsDisposed)
                p.NoiseLevel = noise;
        }

        // pressureSeconds 0..60 → noise 0..1 (активно после 65% = 39 сек)
        internal float GetNoiseLevel()
        {
            if (pressureSeconds <= 39) return 0f;
            return Math.Min(1f, (pressureSeconds - 39) / 21f); // 39..60 → 0..1
        }

        // ════════════════════════════════════════════════════════════════
        //  ФАБРИКИ ПАНЕЛЕЙ
        // ════════════════════════════════════════════════════════════════

        private MonitorFloatPanel CreateStatusPanel()
        {
            var p = new MonitorFloatPanel("SUBJECT STATUS", this, PanelKind.Status);
            p.SetCharacter(currentCharacterData);
            // Стартовая позиция — левый верхний угол экрана
            p.StartPosition = FormStartPosition.Manual;
            p.Location = new Point(
                (int)(ScaleRect(zoneLeftTop).X),
                (int)(ScaleRect(zoneLeftTop).Y + ScaleRect(zoneLeftTop).Height + 8));
            return p;
        }

        private MonitorFloatPanel CreatePulsePanel()
        {
            var p = new MonitorFloatPanel("PULSE MONITOR", this, PanelKind.Pulse);
            p.SetCharacter(currentCharacterData);
            p.StartPosition = FormStartPosition.Manual;
            p.Location = new Point(
                (int)(ScaleRect(zoneLeftMiddle).X),
                (int)(ScaleRect(zoneLeftMiddle).Y + ScaleRect(zoneLeftMiddle).Height + 8));
            return p;
        }

        private MonitorFloatPanel CreateRadiationPanel()
        {
            var p = new MonitorFloatPanel("RADIATION // BIO-SCAN", this, PanelKind.Radiation);
            p.SetCharacter(currentCharacterData);
            p.StartPosition = FormStartPosition.Manual;
            p.Location = new Point(
                (int)(ScaleRect(zoneLeftBottom).X),
                (int)(ScaleRect(zoneLeftBottom).Y + ScaleRect(zoneLeftBottom).Height + 8));
            return p;
        }

        // ════════════════════════════════════════════════════════════════
        //  ЗАКРЫТЬ ВСЕ ПАНЕЛИ при смене персонажа
        // ════════════════════════════════════════════════════════════════

        internal void CloseAllMonitorPanels()
        {
            ClosePanel(ref _panel0);
            ClosePanel(ref _panel1);
            ClosePanel(ref _panel2);
        }

        private void ClosePanel(ref MonitorFloatPanel p)
        {
            if (p != null && !p.IsDisposed) { p.Close(); p.Dispose(); }
            p = null;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  ВИДЫ ПАНЕЛЕЙ
    // ════════════════════════════════════════════════════════════════════
    public enum PanelKind { Status, Pulse, Radiation }

    // ════════════════════════════════════════════════════════════════════
    //  ПЛАВАЮЩАЯ ПАНЕЛЬ МОНИТОРА
    // ════════════════════════════════════════════════════════════════════
    public class MonitorFloatPanel : Form
    {
        // ─── Настройки ───────────────────────────────────────────────
        private const int PW = 280;   // ширина панели
        private const int PH = 200;   // высота панели

        private readonly PanelKind _kind;
        private readonly Form1 _owner;
        private Character _character;

        // ─── Анимация ────────────────────────────────────────────────
        private Timer _animTimer;
        private float _animPhase = 0f;   // общая фаза (0..1 зацикленная)
        private float _noiseLevel = 0f;  // 0=чисто 1=максимальные помехи
        private Random _rng = new Random();

        // ─── Для пульса: история точек ──────────────────────────────
        private float[] _pulseHistory = new float[56];  // точки ЭКГ
        private int _pulseHead = 0;
        private float _pulsePhase = 0f;

        // ─── Перетаскивание ──────────────────────────────────────────
        private bool _dragging;
        private Point _dragOffset;

        public float NoiseLevel
        {
            get => _noiseLevel;
            set { _noiseLevel = Math.Max(0f, Math.Min(1f, value)); }
        }

        // ════════════════════════════════════════════════════════════
        //  КОНСТРУКТОР
        // ════════════════════════════════════════════════════════════
        public MonitorFloatPanel(string title, Form1 owner, PanelKind kind)
        {
            _kind = kind;
            _owner = owner;

            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(PW, PH);
            this.BackColor = Color.FromArgb(6, 8, 12);
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Перетаскивание
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _dragging = true;
                    _dragOffset = e.Location;
                }
            };
            this.MouseMove += (s, e) =>
            {
                if (_dragging)
                    this.Location = new Point(
                        this.Left + e.X - _dragOffset.X,
                        this.Top + e.Y - _dragOffset.Y);
            };
            this.MouseUp += (s, e) => _dragging = false;

            // Анимационный таймер
            _animTimer = new Timer { Interval = 50 };
            _animTimer.Tick += (s, e) =>
            {
                _animPhase = (_animPhase + 0.04f) % 1f;
                TickPulse();
                this.Invalidate();
            };
            _animTimer.Start();

            this.FormClosed += (s, e) => _animTimer?.Stop();
            this.Paint += OnPaint;
        }

        public void SetCharacter(Character ch) => _character = ch;

        // ════════════════════════════════════════════════════════════
        //  РЕНДЕР
        // ════════════════════════════════════════════════════════════
        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            DrawFrame(g);

            switch (_kind)
            {
                case PanelKind.Status: DrawStatus(g); break;
                case PanelKind.Pulse: DrawPulse(g); break;
                case PanelKind.Radiation: DrawRadiation(g); break;
            }

            // Помехи поверх всего
            if (_noiseLevel > 0f)
                DrawNoise(g);
        }

        // ─── Рамка и крестик ────────────────────────────────────────
        private void DrawFrame(Graphics g)
        {
            // Фон
            g.Clear(Color.FromArgb(6, 8, 12));

            // Внешняя рамка
            Color border = _noiseLevel > 0.5f
                ? Color.FromArgb(180, 200, 60, 30)
                : Color.FromArgb(120, 0, 180, 200);
            using (var pen = new Pen(border, 1.5f))
                g.DrawRectangle(pen, 1, 1, PW - 2, PH - 2);

            // Заголовочная полоса
            using (var hdr = new SolidBrush(Color.FromArgb(20, 0, 160, 180)))
                g.FillRectangle(hdr, 2, 2, PW - 4, 22);

            // Заголовок
            string title;
            switch (_kind)
            {
                case PanelKind.Status: title = "SUBJECT STATUS"; break;
                case PanelKind.Pulse: title = "PULSE MONITOR"; break;
                case PanelKind.Radiation: title = "RADIATION // BIO"; break;
                default: title = "MONITOR"; break;
            }
            using (var f = new Font("Consolas", 8f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(200, 0, 220, 220)))
                g.DrawString(title, f, b, 8, 6);

            // Крестик закрытия
            DrawCloseButton(g);

            // Разделитель
            using (var pen = new Pen(Color.FromArgb(40, 0, 160, 180), 1f))
                g.DrawLine(pen, 2, 25, PW - 2, 25);
        }

        private void DrawCloseButton(Graphics g)
        {
            var r = new Rectangle(PW - 22, 4, 16, 16);
            using (var pen = new Pen(Color.FromArgb(140, 200, 80, 60), 1.5f))
            {
                g.DrawLine(pen, r.Left + 3, r.Top + 3, r.Right - 3, r.Bottom - 3);
                g.DrawLine(pen, r.Right - 3, r.Top + 3, r.Left + 3, r.Bottom - 3);
            }

            // Кликабельная зона крестика
            if (!_closeHandlerAttached)
            {
                this.MouseClick += (s, e) =>
                {
                    if (new Rectangle(PW - 22, 4, 16, 16).Contains(e.Location))
                        this.Close();
                };
                _closeHandlerAttached = true;
            }
        }
        private bool _closeHandlerAttached = false;

        // ════════════════════════════════════════════════════════════
        //  ПАНЕЛЬ 0 — ОБЩЕЕ СОСТОЯНИЕ
        // ════════════════════════════════════════════════════════════
        private void DrawStatus(Graphics g)
        {
            if (_character == null) return;

            int startY = 32;

            // Иконка вида (символьная)
            string specIcon = _character is Robot ? "⬡ SYNTHETIC"
                            : _character is Alien ? "✦ FOREIGN"
                            : "◉ ORGANIC";
            Color specColor = _character is Robot ? Color.FromArgb(255, 80, 80)
                            : _character is Alien ? Color.FromArgb(80, 200, 255)
                            : Color.FromArgb(80, 220, 120);

            // День 5+ — скрываем реальный тип, показываем "UNKNOWN"
            if (_character.Day >= 5)
            {
                specIcon = "? UNCLASSIFIED";
                specColor = Color.FromArgb(180, 160, 80);
            }

            using (var f = new Font("Consolas", 9f, FontStyle.Bold))
            using (var b = new SolidBrush(specColor))
                g.DrawString(specIcon, f, b, 10, startY);

            // Горизонтальные бары состояния
            DrawStatusBar(g, "THERMAL", GetThermalValue(), Color.FromArgb(220, 120, 40), startY + 24);
            DrawStatusBar(g, "NEURAL", GetNeuralValue(), Color.FromArgb(80, 200, 255), startY + 50);
            DrawStatusBar(g, "ORGANIC", GetOrganicValue(), Color.FromArgb(80, 220, 120), startY + 76);
            DrawStatusBar(g, "SYNTHETI", GetSyntheticValue(), Color.FromArgb(220, 80, 80), startY + 102);

            // Итоговый вердикт сканера (с шумом)
            DrawScanVerdict(g, startY + 132);
        }

        private void DrawStatusBar(Graphics g, string label, float value, Color col, int y)
        {
            // label
            using (var f = new Font("Consolas", 7f))
            using (var b = new SolidBrush(Color.FromArgb(100, 160, 160)))
                g.DrawString(label, f, b, 10, y);

            // track
            int barX = 80, barW = 180, barH = 9;
            using (var trackBr = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                g.FillRectangle(trackBr, barX, y + 1, barW, barH);

            // fill — с шумом
            float noisy = value + (_noiseLevel > 0 ? ((float)_rng.NextDouble() - 0.5f) * _noiseLevel * 0.4f : 0f);
            noisy = Math.Max(0.02f, Math.Min(1f, noisy));
            int fillW = (int)(barW * noisy);

            using (var br = new LinearGradientBrush(
                new Rectangle(barX, y + 1, Math.Max(1, fillW), barH),
                Color.FromArgb(col.A, col.R / 2, col.G / 2, col.B / 2),
                col,
                LinearGradientMode.Horizontal))
                g.FillRectangle(br, barX, y + 1, fillW, barH);

            // border
            using (var pen = new Pen(Color.FromArgb(40, col), 1f))
                g.DrawRectangle(pen, barX, y + 1, barW, barH);
        }

        private void DrawScanVerdict(Graphics g, int y)
        {
            // Мигающая строка результата
            bool blink = (int)(_animPhase * 6) % 2 == 0;
            string verdict;
            Color vc;

            if (_noiseLevel > 0.6f)
            {
                verdict = blink ? "SCAN ERROR — DATA CORRUPT" : "░░░░ INTERFERENCE ░░░░";
                vc = Color.FromArgb(200, 200, 60, 30);
            }
            else if (_character?.Day >= 5)
            {
                int conf = 20 + (int)((_noiseLevel > 0 ? _rng.Next(0, 20) : 0));
                verdict = $"CONFIDENCE: {60 - conf}%  INCONCLUSIVE";
                vc = Color.FromArgb(180, 160, 80);
            }
            else
            {
                verdict = _character is Robot ? "ANOMALY DETECTED"
                        : _character is Alien ? "NON-STANDARD BIO"
                        : "BASELINE NOMINAL";
                vc = _character is Robot ? Color.FromArgb(220, 80, 80)
                   : _character is Alien ? Color.FromArgb(80, 200, 255)
                   : Color.FromArgb(80, 220, 120);
            }

            using (var f = new Font("Consolas", 8f, FontStyle.Bold))
            using (var b = new SolidBrush(blink ? vc : Color.FromArgb(vc.A / 2, vc)))
                g.DrawString(verdict, f, b, 10, y);
        }

        // Значения баров зависят от типа + шум
        private float GetThermalValue()
        {
            float base_ = _character is Robot ? 0.35f : _character is Alien ? 0.82f : 0.65f;
            return Noisy(base_);
        }
        private float GetNeuralValue()
        {
            float base_ = _character is Robot ? 0.05f : _character is Alien ? 0.60f : 0.72f;
            return Noisy(base_);
        }
        private float GetOrganicValue()
        {
            float base_ = _character is Robot ? 0.08f : _character is Alien ? 0.88f : 0.90f;
            return Noisy(base_);
        }
        private float GetSyntheticValue()
        {
            float base_ = _character is Robot ? 0.92f : _character is Alien ? 0.18f : 0.04f;
            return Noisy(base_);
        }
        private float Noisy(float v)
        {
            if (_noiseLevel <= 0f) return v;
            return Math.Max(0.01f, Math.Min(1f, v + ((float)_rng.NextDouble() - 0.5f) * _noiseLevel * 0.5f));
        }

        // ════════════════════════════════════════════════════════════
        //  ПАНЕЛЬ 1 — ПУЛЬС (ЭКГ-АНИМАЦИЯ)
        // ════════════════════════════════════════════════════════════
        private void TickPulse()
        {
            _pulsePhase += 0.08f;
            float raw = GeneratePulseSample(_pulsePhase);
            // При помехах — случайные выбросы
            if (_noiseLevel > 0.3f && (float)_rng.NextDouble() < _noiseLevel * 0.4f)
                raw += ((float)_rng.NextDouble() - 0.5f) * 2f * _noiseLevel;
            _pulseHistory[_pulseHead] = Math.Max(-1f, Math.Min(1f, raw));
            _pulseHead = (_pulseHead + 1) % _pulseHistory.Length;
        }

        private float GeneratePulseSample(float phase)
        {
            // Форма ЭКГ: P-QRS-T комплекс на основе синусоиды с пиком
            float t = phase % (float)(Math.PI * 2);
            float s = (float)Math.Sin(t);

            // Пик QRS
            float norm = t / (float)(Math.PI * 2);  // 0..1 в периоде
            float qrs = 0f;
            if (norm > 0.35f && norm < 0.45f)
            {
                float x = (norm - 0.4f) / 0.05f;   // -1..1
                qrs = (float)Math.Exp(-x * x * 8f) * 1.8f;
            }

            float base_ = _character is Robot ? 0f    // плоская линия у роботов
                        : _character is Alien ? s * 0.6f + qrs * 0.9f
                        : s * 0.3f + qrs;

            return base_;
        }

        private void DrawPulse(Graphics g)
        {
            int areaX = 10, areaY = 30, areaW = PW - 20, areaH = PH - 60;
            int cy = areaY + areaH / 2;

            // Сетка
            DrawEcgGrid(g, areaX, areaY, areaW, areaH);

            // Значение BPM
            int bpm = GetBpm();
            Color bpmColor = _noiseLevel > 0.5f
                ? Color.FromArgb(200, 200, 60, 30)
                : (_character is Robot ? Color.FromArgb(80, 80, 220) : Color.FromArgb(80, 220, 80));

            using (var f = new Font("Consolas", 18f, FontStyle.Bold))
            using (var b = new SolidBrush(bpmColor))
            {
                string bpmText = _noiseLevel > 0.7f ? "---" : $"{bpm}";
                g.DrawString(bpmText, f, b, areaX, areaY + areaH - 32);
            }
            using (var f = new Font("Consolas", 7f))
            using (var b = new SolidBrush(Color.FromArgb(80, 160, 160)))
                g.DrawString("BPM", f, b, areaX + 50, areaY + areaH - 22);

            // Линия ЭКГ
            if (_character is Robot && _character.Day < 5)
            {
                // Плоская линия с лёгким шумом
                DrawFlatLine(g, areaX, cy, areaW);
                return;
            }

            var points = new System.Collections.Generic.List<PointF>();
            for (int i = 0; i < _pulseHistory.Length; i++)
            {
                int idx = (_pulseHead + i) % _pulseHistory.Length;
                float x = areaX + (float)i / (_pulseHistory.Length - 1) * areaW;
                float y = cy - _pulseHistory[idx] * (areaH / 2f - 8f);
                points.Add(new PointF(x, y));
            }

            if (points.Count > 1)
            {
                Color lineCol = _noiseLevel > 0.5f
                    ? Color.FromArgb(200, 200, 80, 30)
                    : Color.FromArgb(220, 0, 220, 80);
                using (var pen = new Pen(lineCol, 1.5f))
                    g.DrawLines(pen, points.ToArray());

                // Glow — повторная рисовка полупрозрачной линией
                using (var glow = new Pen(Color.FromArgb(50, lineCol), 4f))
                    g.DrawLines(glow, points.ToArray());
            }

            // Метки осей
            DrawPulseLabels(g, areaX, areaY, areaW, areaH);
        }

        private void DrawEcgGrid(Graphics g, int x, int y, int w, int h)
        {
            using (var pen = new Pen(Color.FromArgb(14, 0, 180, 100), 1f))
            {
                // Вертикальные
                for (int i = 0; i <= 8; i++)
                    g.DrawLine(pen, x + i * w / 8, y, x + i * w / 8, y + h);
                // Горизонтальные
                for (int i = 0; i <= 4; i++)
                    g.DrawLine(pen, x, y + i * h / 4, x + w, y + i * h / 4);
            }
        }

        private void DrawFlatLine(Graphics g, int x, int cy, int w)
        {
            // Плоская линия с микро-шумом (для роботов до дня 5)
            var pts = new PointF[w / 3];
            for (int i = 0; i < pts.Length; i++)
            {
                float noise = _noiseLevel > 0 ? ((float)_rng.NextDouble() - 0.5f) * _noiseLevel * 6f : 0f;
                pts[i] = new PointF(x + i * 3, cy + noise);
            }
            using (var pen = new Pen(Color.FromArgb(180, 0, 200, 80), 1.5f))
                g.DrawLines(pen, pts);
        }

        private void DrawPulseLabels(Graphics g, int x, int y, int w, int h)
        {
            using (var f = new Font("Consolas", 6f))
            using (var b = new SolidBrush(Color.FromArgb(50, 0, 180, 180)))
            {
                g.DrawString("1.0", f, b, x, y + 2);
                g.DrawString("0.0", f, b, x, y + h / 2 - 6);
                g.DrawString("-1.", f, b, x, y + h - 14);
            }
        }

        private int GetBpm()
        {
            if (_character is Robot && _character.Day < 5) return 0;
            int base_ = _character is Robot ? 62
                      : _character is Alien ? 128
                      : 76;
            int jitter = _noiseLevel > 0 ? _rng.Next(-12, 12) : _rng.Next(-4, 5);
            return base_ + jitter;
        }

        // ════════════════════════════════════════════════════════════
        //  ПАНЕЛЬ 2 — РАДИАЦИЯ / БИО-СКАНЕР
        // ════════════════════════════════════════════════════════════
        private void DrawRadiation(Graphics g)
        {
            int cx = PW / 2, cy = 100;
            int baseR = 46;

            // Вращающиеся кольца (анимация фазы)
            DrawRadRings(g, cx, cy, baseR);

            // Центральная иконка — символ радиации
            DrawRadSymbol(g, cx, cy);

            // Числовые показания
            DrawRadReadings(g);
        }

        private void DrawRadRings(Graphics g, int cx, int cy, int r)
        {
            float phase = _animPhase * (float)(Math.PI * 2);

            for (int ring = 0; ring < 3; ring++)
            {
                float ringPhase = phase + ring * 1.2f;
                float scale = 1f + ring * 0.38f;
                int ri = (int)(r * scale);
                int alpha = 90 - ring * 22;

                if (_noiseLevel > 0.3f)
                    alpha = (int)(alpha * (1f - _noiseLevel * 0.7f));
                alpha = Math.Max(10, alpha);

                Color col = ring == 0 ? Color.FromArgb(alpha, 0, 220, 100)
                          : ring == 1 ? Color.FromArgb(alpha, 0, 180, 220)
                          : Color.FromArgb(alpha, 180, 80, 0);

                // Дугами вместо полного круга — вращающийся эффект
                using (var pen = new Pen(col, ring == 0 ? 2f : 1f))
                {
                    float startAngle = (float)(ringPhase * 180f / Math.PI);
                    g.DrawArc(pen, cx - ri, cy - ri, ri * 2, ri * 2, startAngle, 240f);
                    g.DrawArc(pen, cx - ri, cy - ri, ri * 2, ri * 2, startAngle + 120f, 40f);
                }
            }

            // Точечки на кольцах
            for (int i = 0; i < 6; i++)
            {
                float angle = phase + i * (float)(Math.PI / 3);
                float px = cx + (r + 4) * (float)Math.Cos(angle);
                float py = cy + (r + 4) * (float)Math.Sin(angle);
                int da = _noiseLevel > 0.4f && (float)_rng.NextDouble() < _noiseLevel * 0.5f ? 20 : 120;
                using (var b = new SolidBrush(Color.FromArgb(da, 0, 220, 160)))
                    g.FillEllipse(b, px - 2, py - 2, 4, 4);
            }
        }

        private void DrawRadSymbol(Graphics g, int cx, int cy)
        {
            // Треугольник радиации — три лопасти
            bool glitch = _noiseLevel > 0.5f && (float)_rng.NextDouble() < _noiseLevel * 0.3f;
            float phase = _animPhase * (float)(Math.PI * 2);

            for (int lobe = 0; lobe < 3; lobe++)
            {
                float baseAngle = phase * 0.3f + lobe * (float)(Math.PI * 2 / 3);
                float x1 = cx + 6 * (float)Math.Cos(baseAngle - 0.4f);
                float y1 = cy + 6 * (float)Math.Sin(baseAngle - 0.4f);
                float x2 = cx + 6 * (float)Math.Cos(baseAngle + 0.4f);
                float y2 = cy + 6 * (float)Math.Sin(baseAngle + 0.4f);
                float x3 = cx + 20 * (float)Math.Cos(baseAngle + 0.5f);
                float y3 = cy + 20 * (float)Math.Sin(baseAngle + 0.5f);
                float x4 = cx + 20 * (float)Math.Cos(baseAngle - 0.5f);
                float y4 = cy + 20 * (float)Math.Sin(baseAngle - 0.5f);

                int alpha = glitch ? _rng.Next(30, 80) : 180;
                Color col = _character is Robot ? Color.FromArgb(alpha, 220, 80, 80)
                          : _character is Alien ? Color.FromArgb(alpha, 80, 200, 220)
                          : Color.FromArgb(alpha, 80, 220, 120);

                using (var br = new SolidBrush(col))
                    g.FillPolygon(br, new[] {
                        new PointF(x1, y1), new PointF(x2, y2),
                        new PointF(x3, y3), new PointF(x4, y4) });
            }

            // Центральный круг
            int ca = glitch ? 60 : 200;
            using (var br = new SolidBrush(Color.FromArgb(ca, 0, 220, 180)))
                g.FillEllipse(br, cx - 5, cy - 5, 10, 10);
        }

        private void DrawRadReadings(Graphics g)
        {
            float baseRad = _character is Robot ? 0.12f
                          : _character is Alien ? 0.55f
                          : 0.08f;

            if (_character?.Day >= 5)
                baseRad = 0.2f + (float)_rng.NextDouble() * 0.4f; // неопределённо

            float noisy = baseRad + (_noiseLevel > 0 ? ((float)_rng.NextDouble() - 0.5f) * _noiseLevel * 0.3f : 0f);
            noisy = Math.Max(0f, Math.Min(1f, noisy));

            // Вербальный уровень
            string level;
            Color levelColor;
            if (_noiseLevel > 0.65f && (float)_rng.NextDouble() < _noiseLevel)
            {
                level = "ERR";
                levelColor = Color.FromArgb(200, 200, 60, 30);
            }
            else if (noisy < 0.2f)
            {
                level = "SAFE";
                levelColor = Color.FromArgb(200, 80, 220, 100);
            }
            else if (noisy < 0.5f)
            {
                level = "TRACE";
                levelColor = Color.FromArgb(200, 200, 180, 40);
            }
            else
            {
                level = "ALERT";
                levelColor = Color.FromArgb(200, 220, 80, 40);
            }

            using (var f = new Font("Consolas", 14f, FontStyle.Bold))
            using (var b = new SolidBrush(levelColor))
                g.DrawString(level, f, b, 10, 150);

            float msvh = noisy * 4.2f;
            using (var f = new Font("Consolas", 7f))
            using (var b = new SolidBrush(Color.FromArgb(100, 140, 140)))
                g.DrawString($"{msvh:F2} mSv/h", f, b, 10, 174);

            // Маленький бар справа
            int barX = 130, barY = 152, barW = 130, barH = 14;
            using (var trackBr = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                g.FillRectangle(trackBr, barX, barY, barW, barH);

            int fillW = (int)(barW * noisy);
            Color barCol = noisy < 0.2f ? Color.FromArgb(120, 80, 220, 100)
                         : noisy < 0.5f ? Color.FromArgb(120, 200, 180, 40)
                         : Color.FromArgb(120, 220, 80, 40);
            using (var br = new SolidBrush(barCol))
                g.FillRectangle(br, barX, barY, fillW, barH);
            using (var pen = new Pen(Color.FromArgb(40, barCol), 1f))
                g.DrawRectangle(pen, barX, barY, barW, barH);
        }

        // ════════════════════════════════════════════════════════════
        //  ПОМЕХИ — поверх всего при pressure > 65%
        // ════════════════════════════════════════════════════════════
        private void DrawNoise(Graphics g)
        {
            float n = _noiseLevel;

            // Горизонтальные полосы сдвига (scan-line glitch)
            int lineCount = (int)(n * 8);
            for (int i = 0; i < lineCount; i++)
            {
                int y = _rng.Next(0, PH);
                int h = _rng.Next(1, 4);
                int shift = (int)(((float)_rng.NextDouble() - 0.5f) * n * 30f);
                int alpha = (int)(n * 80);

                // Смещённая копия
                using (var pen = new Pen(Color.FromArgb(alpha, 0, 180, 180), h))
                    g.DrawLine(pen, shift, y, PW + shift, y);
            }

            // Пиксельный шум
            int pixCount = (int)(n * n * 300);
            for (int i = 0; i < pixCount; i++)
            {
                int px = _rng.Next(0, PW), py = _rng.Next(0, PH);
                int a = _rng.Next(40, 120);
                Color c = _rng.Next(0, 2) == 0
                    ? Color.FromArgb(a, 0, 220, 180)
                    : Color.FromArgb(a, 220, 60, 30);
                using (var b = new SolidBrush(c))
                    g.FillRectangle(b, px, py, 2, 1);
            }

            // Полупрозрачная красная рамка при высоком шуме
            if (n > 0.6f)
            {
                int a = (int)((n - 0.6f) / 0.4f * 80);
                using (var pen = new Pen(Color.FromArgb(a, 220, 40, 20), 3f))
                    g.DrawRectangle(pen, 1, 1, PW - 2, PH - 2);
            }
        }
    }
}
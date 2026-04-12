using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TheGatekeeper
{
    // Eliminates flicker: Panel with double-buffering explicitly enabled
    internal class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            UpdateStyles();
        }
    }

    public partial class WelcomeForm : Form
    {
        private Timer animationTimer;
        private Timer glitchTimer;
        private Timer storyTimer;

        private BufferedPanel menuPanel;
        private BufferedPanel storyPanel;

        private PictureBox pbMenu;
        private PictureBox pbStory;
        private BufferedPanel menuFade;
        private BufferedPanel storyFade;
        private Image imgNormal;
        private Image imgGlitch;
        private Form1 gameForm;


        private Label lblSkip;
        private readonly List<Panel> menuHitAreas = new List<Panel>();

        private int scanlineY = 0;
        private bool glitchActive = false;
        private int glitchCycle = 0;
        private int menuSelectedIndex = 0;
        private int cursorBlink = 0;
        private int revealedLines = 0;

        private bool isFullscreen = false;
        private FormBorderStyle prevBorderStyle;
        private Rectangle prevBounds;
        private bool prevTopMost;

        private PointF[] stars;
        private float[] starSizes;
        private Color[] starColors;
        private readonly Random rng = new Random(42);

        private readonly string[] menuItems =
            { "START", "GAME MODE", "SAVED", "HOW TO PLAY", "EXIT" };

        public TheGatekeeper.Models.GameMode SelectedMode { get; private set; } = TheGatekeeper.Models.GameMode.DailyQuota;

        private readonly string[] storyLines =
        {
            "// YEAR 2056",
            "",
            "World War III scorched half the Earth.",
            "Governments collapsed. AI turned on humanity.",
            "The survivors fled to orbital stations,",
            "to Jupiter's moons, and into the asteroid belt.",
            "",
            "You are Inspector of Gate Alpha.",
            "Your job: decide who gets inside.",
            "Human. Robot. Alien.",
            "",
            "ONE MISTAKE — AND THE COLONY DIES."
        };

        public WelcomeForm()
        {
            GenerateStars();
            LoadImages();
            InitializeForm();
            gameForm = new Form1();
        }

        // ── Stars ─────────────────────────────────────────────────────────────

        private void GenerateStars()
        {
            stars = new PointF[180];
            starSizes = new float[180];
            starColors = new Color[180];
            Color[] palette =
            {
                Color.White,
                Color.FromArgb(180, 220, 255, 255),
                Color.FromArgb(160, 200, 200, 255),
                Color.FromArgb(180, 255, 200, 220)
            };
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = new PointF(rng.Next(0, 1000), rng.Next(0, 700));
                starSizes[i] = (float)(rng.NextDouble() * 1.8 + 0.4);
                starColors[i] = palette[rng.Next(palette.Length)];
            }
        }

        // ── Images ────────────────────────────────────────────────────────────

        private void LoadImages()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string p1 = Path.Combine(baseDir, "Images", "Background", "image_2.png");
            string p2 = Path.Combine(baseDir, "Images", "Background", "image_4.png");
            try
            {
                imgNormal = File.Exists(p1) ? Image.FromFile(p1) : MakePlaceholder(Color.FromArgb(20, 20, 40), "NO IMAGE");
                imgGlitch = File.Exists(p2) ? Image.FromFile(p2) : MakePlaceholder(Color.FromArgb(40, 10, 40), "GLITCH");
            }
            catch
            {
                imgNormal = MakePlaceholder(Color.FromArgb(20, 20, 40), "NO IMAGE");
                imgGlitch = MakePlaceholder(Color.FromArgb(40, 10, 40), "GLITCH");
            }
        }

        private Image MakePlaceholder(Color bg, string label)
        {
            var bmp = new Bitmap(400, 700);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(bg);
                using (var f = new Font("Courier New", 14f))
                using (var b = new SolidBrush(Color.FromArgb(60, 0, 255, 255)))
                {
                    var sz = g.MeasureString(label, f);
                    g.DrawString(label, f, b,
                        (bmp.Width - sz.Width) / 2f,
                        (bmp.Height - sz.Height) / 2f);
                }
            }
            return bmp;
        }

        // ── Form setup ────────────────────────────────────────────────────────

        private void InitializeForm()
        {
            this.Text = "THE GATEKEEPER";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(4, 5, 13);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;
            this.Shown += (s, e) =>
            {
                EnterFullscreen();
                ApplyRightPanelLayout();
                LayoutMenuHitAreas();
            };

            BuildMenuState();
            BuildStoryState();
            StartAnimation();
        }

        // ── MENU STATE ────────────────────────────────────────────────────────

        private void BuildMenuState()
        {
            menuPanel = new BufferedPanel { Dock = DockStyle.Fill };
            menuPanel.Paint += MenuPanel_Paint;
            this.Controls.Add(menuPanel);

            // Photo — right 500px
            pbMenu = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = imgNormal,
                Location = new Point(this.ClientSize.Width - 500, 0),
                Size = new Size(500, this.ClientSize.Height),
                BackColor = Color.FromArgb(4, 5, 13)
            };
            menuPanel.Controls.Add(pbMenu);

            // Gradient fade between photo and background text
            menuFade = new BufferedPanel
            {
                Location = new Point(this.ClientSize.Width - 510, 0),
                Size = new Size(100, this.ClientSize.Height),
                BackColor = Color.Transparent
            };
            menuFade.Paint += (s, e) =>
            {
                using (var lgb = new LinearGradientBrush(
                    new Point(0, 0), new Point(100, 0),
                    Color.FromArgb(255, 4, 5, 13),
                    Color.Transparent))
                    e.Graphics.FillRectangle(lgb, 0, 0, menuFade.Width, menuFade.Height);
            };
            menuPanel.Controls.Add(menuFade);
            menuFade.BringToFront();

            // Invisible click areas for menu items
            menuHitAreas.Clear();
            for (int i = 0; i < menuItems.Length; i++)
            {
                int idx = i;
                var area = new Panel
                {
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                area.MouseEnter += (s, e) => { menuSelectedIndex = idx; menuPanel.Invalidate(); };
                area.MouseClick += (s, e) => HandleMenuClick(idx);
                menuPanel.Controls.Add(area);
                area.BringToFront();
                menuHitAreas.Add(area);
            }

            LayoutMenuHitAreas();
        }

        private void MenuPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = menuPanel.Width, h = menuPanel.Height;

            DrawBackground(g, w, h);
            DrawScanline(g, w, h);
            DrawCornerBrackets(g, w, h);
            DrawMenuContent(g, w, h);
            DrawStatusBar(g, w, h);
        }

        private void DrawMenuContent(Graphics g, int w, int h)
        {
            var m = GetMetrics(w, h);
            // Sub-header
            using (var f = new Font("Courier New", m.SubHeaderFont))
            using (var b = new SolidBrush(Color.FromArgb(110, 0, 255, 255)))
                g.DrawString("SECTOR-7  ·  ORBITAL GATE  ·  CHECKPOINT ALPHA", f, b, m.LeftX, m.SubHeaderY);

            // Title
            string[] titleLines = { "THE", "GATE", "KEEPER" };
            float ty = m.TitleY;
            using (var titleBrush = new SolidBrush(Color.White))
            using (var glitchBrush = new SolidBrush(Color.FromArgb(35, 255, 0, 200)))
            {
                foreach (var line in titleLines)
                {
                    using (var f = new Font("Courier New", m.TitleFont, FontStyle.Bold))
                    {
                        if (glitchActive)
                            g.DrawString(line, f, glitchBrush, m.LeftX + 2, ty + 1);
                        g.DrawString(line, f, titleBrush, m.LeftX, ty);
                    }
                    ty += m.TitleLineStep;
                }
            }

            // Divider
            using (var pen = new Pen(Color.FromArgb(35, 0, 255, 255), 1f))
                g.DrawLine(pen, m.LeftX, m.DividerY, m.LeftX + m.MenuWidth, m.DividerY);

            // Menu items — plain text, no buttons
            for (int i = 0; i < menuItems.Length; i++)
            {
                bool sel = i == menuSelectedIndex;
                int iy = (int)(m.MenuStartY + i * m.MenuItemStep);
                string itemText = menuItems[i];
                if (i == 1)
                    itemText = $"{menuItems[i]}: {ModeLabel(SelectedMode)}";

                if (sel)
                {
                    using (var f = new Font("Courier New", m.MenuArrowFont, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.FromArgb(220, 255, 0, 85)))
                        g.DrawString(">", f, b, m.ArrowX, iy + m.MenuItemTextOffsetY);

                    using (var f = new Font("Courier New", m.MenuItemFontSelected, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.White))
                    {
                        g.DrawString(itemText, f, b, m.MenuTextX, iy + m.MenuItemTextOffsetY);
                        if (cursorBlink < 5)
                        {
                            var sz = g.MeasureString(itemText, f);
                            using (var cb = new SolidBrush(Color.FromArgb(200, 0, 255, 255)))
                                g.FillRectangle(cb, m.MenuTextX + 2 + sz.Width, iy + m.MenuItemTextOffsetY + 2, m.CursorW, m.CursorH);
                        }
                    }
                }
                else
                {
                    using (var f = new Font("Courier New", m.MenuItemFontNormal))
                    using (var b = new SolidBrush(Color.FromArgb(90, 0, 200, 200)))
                        g.DrawString(itemText, f, b, m.MenuTextX, iy + m.MenuItemTextOffsetY + 1);
                }
            }
        }

        private void HandleMenuClick(int idx)
        {
            menuSelectedIndex = idx;
            menuPanel.Invalidate();
            if (idx == 0) StartCinematic();
            else if (idx == 1) ToggleMode();
            else if (idx == 4) Application.Exit();
        }

        private void ToggleMode()
        {
            SelectedMode = (TheGatekeeper.Models.GameMode)(((int)SelectedMode + 1) % 3);
            menuPanel?.Invalidate();
        }

        private static string ModeLabel(TheGatekeeper.Models.GameMode mode)
        {
            switch (mode)
            {
                case TheGatekeeper.Models.GameMode.DailyQuota: return "Daily Quota";
                case TheGatekeeper.Models.GameMode.FindTheVillain: return "Find the Villain";
                case TheGatekeeper.Models.GameMode.CriticalMission: return "Critical Mission";
                default: return "Daily Quota";
            }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                ToggleFullscreen();
                e.Handled = true;
                return;
            }

            if (menuPanel.Visible)
            {
                if (e.KeyCode == Keys.Up)
                {
                    menuSelectedIndex = (menuSelectedIndex - 1 + menuItems.Length) % menuItems.Length;
                    menuPanel.Invalidate();
                }
                else if (e.KeyCode == Keys.Down)
                {
                    menuSelectedIndex = (menuSelectedIndex + 1) % menuItems.Length;
                    menuPanel.Invalidate();
                }
                else if (e.KeyCode == Keys.Enter)
                    HandleMenuClick(menuSelectedIndex);
                else if (e.KeyCode == Keys.Escape)
                    Application.Exit();
            }
            else if (storyPanel.Visible)
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Space || e.KeyCode == Keys.Return)
                    FinishCinematic();
            }
        }

        // ── STORY STATE ───────────────────────────────────────────────────────

        private void BuildStoryState()
        {
            storyPanel = new BufferedPanel { Dock = DockStyle.Fill, Visible = false };
            storyPanel.Paint += StoryPanel_Paint;
            this.Controls.Add(storyPanel);
            storyPanel.BringToFront();

            // Photo right side
            pbStory = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = imgNormal,
                Location = new Point(this.ClientSize.Width - 500, 0),
                Size = new Size(500, this.ClientSize.Height),
                BackColor = Color.FromArgb(4, 5, 13)
            };
            storyPanel.Controls.Add(pbStory);

            // Fade edge
            storyFade = new BufferedPanel
            {
                Location = new Point(this.ClientSize.Width - 510, 0),
                Size = new Size(100, this.ClientSize.Height),
                BackColor = Color.Transparent
            };
            storyFade.Paint += (s, e) =>
            {
                using (var lgb = new LinearGradientBrush(
                    new Point(0, 0), new Point(100, 0),
                    Color.FromArgb(255, 4, 5, 13),
                    Color.Transparent))
                    e.Graphics.FillRectangle(lgb, 0, 0, storyFade.Width, storyFade.Height);
            };
            storyPanel.Controls.Add(storyFade);
            storyFade.BringToFront();

            // Skip — plain text label, bottom right
            lblSkip = new Label
            {
                Text = "SKIP  //",
                AutoSize = true,
                Font = new Font("Courier New", 9f),
                ForeColor = Color.FromArgb(70, 0, 255, 255),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblSkip.Click += (s, e) => FinishCinematic();
            lblSkip.MouseEnter += (s, e) => lblSkip.ForeColor = Color.FromArgb(200, 0, 255, 255);
            lblSkip.MouseLeave += (s, e) => lblSkip.ForeColor = Color.FromArgb(70, 0, 255, 255);
            storyPanel.Controls.Add(lblSkip);
            lblSkip.BringToFront();
        }

        private void StoryPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = storyPanel.Width, h = storyPanel.Height;

            DrawBackground(g, w, h);
            DrawScanline(g, w, h);
            DrawCornerBrackets(g, w, h);
            DrawStoryText(g, w, h);
            DrawStatusBar(g, w, h);
        }

        private void DrawStoryText(Graphics g, int w, int h)
        {
            var m = GetMetrics(w, h);
            int textX = m.StoryTextX, startY = m.StoryStartY, lineH = m.StoryLineH;

            for (int i = 0; i < Math.Min(revealedLines, storyLines.Length); i++)
            {
                string line = storyLines[i];
                if (string.IsNullOrEmpty(line)) continue;

                bool isHeader = line.StartsWith("//");
                bool isWarning = i == storyLines.Length - 1;
                bool isBold = i >= 7;

                Color col; float sz; FontStyle fs;

                if (isHeader) { col = Color.FromArgb(150, 255, 0, 85); sz = m.StoryHeaderFont; fs = FontStyle.Regular; }
                else if (isWarning) { col = Color.FromArgb(230, 255, 50, 80); sz = m.StoryWarnFont; fs = FontStyle.Bold; }
                else if (isBold) { col = Color.White; sz = m.StoryBoldFont; fs = FontStyle.Bold; }
                else { col = Color.FromArgb(170, 200, 255, 220); sz = m.StoryNormalFont; fs = FontStyle.Regular; }

                using (var f = new Font("Courier New", sz, fs))
                using (var b = new SolidBrush(col))
                    g.DrawString(line, f, b, textX, startY + i * lineH);
            }

            // Blinking cursor after last line
            if (revealedLines > 0 && cursorBlink < 5)
            {
                int li = Math.Min(revealedLines - 1, storyLines.Length - 1);
                while (li > 0 && string.IsNullOrEmpty(storyLines[li])) li--;
                using (var f = new Font("Courier New", m.StoryCursorFont))
                {
                    var ms = g.MeasureString(storyLines[li] + "_", f);
                    using (var b = new SolidBrush(Color.FromArgb(180, 0, 255, 255)))
                        g.FillRectangle(b, textX + ms.Width - 10, startY + li * lineH + 2, m.CursorW, m.CursorH);
                }
            }
        }

        // ── Cinematic flow ────────────────────────────────────────────────────

        private void StartCinematic()
        {
            menuPanel.Visible = false;
            storyPanel.Visible = true;
            storyPanel.BringToFront();

            revealedLines = 0;
            pbStory.Image = imgNormal;
            PositionSkipLabel();
            lblSkip.BringToFront();

            StartGlitch();

            storyTimer = new Timer { Interval = 750 };
            storyTimer.Tick += (s, e) =>
            {
                revealedLines++;
                storyPanel.Invalidate();

                if (revealedLines >= storyLines.Length)
                {
                    storyTimer.Stop();
                    FinishCinematic();
                }
            };
            storyTimer.Start();
        }

        private void StartGlitch()
        {
            glitchActive = true;
            var targetPb = (storyPanel != null && storyPanel.Visible) ? pbStory : pbMenu;
            if (targetPb != null) targetPb.Image = imgGlitch;

            glitchTimer?.Stop();
            glitchTimer = new Timer { Interval = 90 };
            glitchTimer.Tick += (s, e) =>
            {
                glitchTimer.Stop();
                glitchActive = false;
                if (targetPb != null) targetPb.Image = imgNormal;
            };
            glitchTimer.Start();
        }

        private void FinishCinematic()
        {
            storyTimer?.Stop();
            glitchTimer?.Stop();
            animationTimer?.Stop();

            // Показываем игровое окно
            gameForm.Show();

            // Скрываем welcome вместо закрытия
            this.Hide();
        }

        private void PositionSkipLabel()
        {
            if (lblSkip == null) return;
            lblSkip.Location = new Point(
                storyPanel.Width - lblSkip.PreferredWidth - 36,
                storyPanel.Height - 32);
        }

        // ── Shared drawing ────────────────────────────────────────────────────

        private void DrawBackground(Graphics g, int w, int h)
        {
            g.Clear(Color.FromArgb(4, 5, 13));

            DrawNebula(g, w * 4 / 5f, h * 2 / 3f, 260, 140, Color.FromArgb(18, 30, 0, 60));
            DrawNebula(g, w / 5f, h * 2 / 5f, 180, 110, Color.FromArgb(14, 0, 20, 50));
            DrawNebula(g, w / 2f, h, 350, 90, Color.FromArgb(12, 10, 0, 40));

            // Stars — moderate twinkle, no per-star alpha spike
            long tick = Environment.TickCount;
            for (int i = 0; i < stars.Length; i++)
            {
                int a = (int)(130 + 40 * Math.Sin(tick / 1400.0 + i * 0.7));
                using (var b = new SolidBrush(Color.FromArgb(a, starColors[i])))
                {
                    float s = starSizes[i];
                    g.FillEllipse(b, stars[i].X - s / 2, stars[i].Y - s / 2, s, s);
                }
            }

            DrawPlanet(g, w, h);
        }

        private void DrawNebula(Graphics g, float cx, float cy, float rx, float ry, Color col)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = col;
                    pgb.SurroundColors = new[] { Color.Transparent };
                    g.FillEllipse(pgb, cx - rx, cy - ry, rx * 2, ry * 2);
                }
            }
        }

        private void DrawPlanet(Graphics g, int w, int h)
        {
            int px = w - 60, py = -30, pr = 110;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(px - pr, py - pr, pr * 2, pr * 2);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = new PointF(px - pr / 3f, py + pr / 4f);
                    pgb.CenterColor = Color.FromArgb(60, 30, 80, 140);
                    pgb.SurroundColors = new[] { Color.FromArgb(8, 4, 10, 30) };
                    g.FillEllipse(pgb, px - pr, py - pr, pr * 2, pr * 2);
                }
            }
            using (var pen = new Pen(Color.FromArgb(28, 0, 180, 220), 1f))
                g.DrawEllipse(pen, px - pr, py - pr, pr * 2, pr * 2);

            g.TranslateTransform(px, py + pr / 2f);
            g.ScaleTransform(1f, 0.28f);
            using (var pen = new Pen(Color.FromArgb(22, 0, 180, 220), 1.5f))
                g.DrawEllipse(pen, -(pr + 40), -(pr + 40), (pr + 40) * 2, (pr + 40) * 2);
            g.ResetTransform();
        }

        private void DrawScanline(Graphics g, int w, int h)
        {
            if (scanlineY < 0 || scanlineY >= h) return;
            using (var pen = new Pen(Color.FromArgb(12, 0, 255, 255), 2f))
                g.DrawLine(pen, 0, scanlineY, w, scanlineY);
        }

        private void DrawCornerBrackets(Graphics g, int w, int h)
        {
            int s = 18, pad = 12;
            using (var pen = new Pen(Color.FromArgb(90, 0, 255, 255), 1f))
            {
                g.DrawLine(pen, pad, pad, pad + s, pad);
                g.DrawLine(pen, pad, pad, pad, pad + s);
                g.DrawLine(pen, w - pad - s, pad, w - pad, pad);
                g.DrawLine(pen, w - pad, pad, w - pad, pad + s);
                g.DrawLine(pen, pad, h - pad, pad + s, h - pad);
                g.DrawLine(pen, pad, h - pad - s, pad, h - pad);
                g.DrawLine(pen, w - pad - s, h - pad, w - pad, h - pad);
                g.DrawLine(pen, w - pad, h - pad - s, w - pad, h - pad);
            }
        }

        private void DrawStatusBar(Graphics g, int w, int h)
        {
            int y = h - 28;
            using (var pen = new Pen(Color.FromArgb(22, 0, 255, 255), 1f))
                g.DrawLine(pen, 40, y, w - 40, y);
            using (var f = new Font("Courier New", 7.5f))
            {
                using (var b = new SolidBrush(Color.FromArgb(65, 0, 200, 200)))
                    g.DrawString("SHIFT 01  ·  GATE ALPHA  ·  NEMESIS OUTPOST", f, b, 44, y + 6);
                using (var b = new SolidBrush(Color.FromArgb(75, 255, 0, 85)))
                    g.DrawString("THREAT ACTIVE", f, b, w - 140, y + 6);
            }
        }

        // ── Animation timer ───────────────────────────────────────────────────

        private void StartAnimation()
        {
            animationTimer = new Timer { Interval = 33 }; // ~30 fps
            animationTimer.Tick += (s, e) =>
            {
                scanlineY += 4;
                if (scanlineY > this.ClientSize.Height + 10) scanlineY = -10;
                cursorBlink = (cursorBlink + 1) % 12;

                if (menuPanel.Visible) menuPanel.Invalidate();
                else if (storyPanel.Visible) storyPanel.Invalidate();
            };
            animationTimer.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRightPanelLayout();
            LayoutMenuHitAreas();
            PositionSkipLabel();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            animationTimer?.Stop();
            storyTimer?.Stop();
            glitchTimer?.Stop();
            imgNormal?.Dispose();
            imgGlitch?.Dispose();
            base.OnFormClosed(e);
        }

        private void ToggleFullscreen()
        {
            if (isFullscreen) ExitFullscreen();
            else EnterFullscreen();
        }

        private void EnterFullscreen()
        {
            if (isFullscreen) return;
            prevBorderStyle = this.FormBorderStyle;
            prevBounds = this.Bounds;
            prevTopMost = this.TopMost;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Normal;
            this.Bounds = Screen.FromControl(this).Bounds;
            this.TopMost = true;
            isFullscreen = true;
        }

        private void ExitFullscreen()
        {
            if (!isFullscreen) return;
            this.TopMost = prevTopMost;
            this.FormBorderStyle = prevBorderStyle;
            this.WindowState = FormWindowState.Normal;
            this.Bounds = prevBounds;
            isFullscreen = false;
        }

        private void ApplyRightPanelLayout()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            int rightW = Clamp((int)(w * 0.46f), 420, 680);
            int fadeW = Clamp((int)(rightW * 0.18f), 70, 140);

            if (pbMenu != null)
            {
                pbMenu.Location = new Point(w - rightW, 0);
                pbMenu.Size = new Size(rightW, h);
            }
            if (menuFade != null)
            {
                menuFade.Location = new Point(w - rightW - fadeW, 0);
                menuFade.Size = new Size(fadeW, h);
            }
            if (pbStory != null)
            {
                pbStory.Location = new Point(w - rightW, 0);
                pbStory.Size = new Size(rightW, h);
            }
            if (storyFade != null)
            {
                storyFade.Location = new Point(w - rightW - fadeW, 0);
                storyFade.Size = new Size(fadeW, h);
            }
        }

        private void LayoutMenuHitAreas()
        {
            if (menuPanel == null || menuHitAreas.Count == 0) return;
            var m = GetMetrics(menuPanel.Width, menuPanel.Height);
            for (int i = 0; i < menuHitAreas.Count; i++)
            {
                var area = menuHitAreas[i];
                area.Location = new Point(m.ArrowX - 8, (int)(m.MenuStartY + i * m.MenuItemStep - 2));
                area.Size = new Size(m.MenuWidth + 60, (int)m.MenuItemStep);
            }
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

        private struct Metrics
        {
            public int LeftX;
            public int ArrowX;
            public int MenuTextX;
            public int MenuWidth;
            public int SubHeaderY;
            public float SubHeaderFont;
            public float TitleFont;
            public float TitleY;
            public float TitleLineStep;
            public int DividerY;
            public int MenuStartY;
            public float MenuItemStep;
            public int MenuItemTextOffsetY;
            public float MenuArrowFont;
            public float MenuItemFontNormal;
            public float MenuItemFontSelected;

            public int StoryTextX;
            public int StoryStartY;
            public int StoryLineH;
            public float StoryHeaderFont;
            public float StoryNormalFont;
            public float StoryBoldFont;
            public float StoryWarnFont;
            public float StoryCursorFont;

            public int CursorW;
            public int CursorH;
        }

        private Metrics GetMetrics(int w, int h)
        {
            float s = Math.Min(w / 1000f, h / 700f);
            if (s < 0.85f) s = 0.85f;
            if (s > 1.35f) s = 1.35f;

            int left = (int)(48 * s);
            int arrowX = (int)(30 * s);
            int menuTextX = (int)(52 * s);
            int menuW = (int)(340 * s);

            return new Metrics
            {
                LeftX = left,
                ArrowX = arrowX,
                MenuTextX = menuTextX,
                MenuWidth = menuW,
                SubHeaderY = (int)(36 * s),
                SubHeaderFont = 8f * s,
                TitleFont = 52f * s,
                TitleY = 60f * s,
                TitleLineStep = 58f * s,
                DividerY = (int)(350 * s),
                MenuStartY = (int)(370 * s),
                MenuItemStep = 38f * s,
                MenuItemTextOffsetY = (int)(4 * s),
                MenuArrowFont = 11f * s,
                MenuItemFontNormal = 11f * s,
                MenuItemFontSelected = 12f * s,

                StoryTextX = (int)(60 * s),
                StoryStartY = (int)(80 * s),
                StoryLineH = (int)(38 * s),
                StoryHeaderFont = 9f * s,
                StoryNormalFont = 11f * s,
                StoryBoldFont = 12f * s,
                StoryWarnFont = 12f * s,
                StoryCursorFont = 11f * s,

                CursorW = Clamp((int)(7 * s), 5, 9),
                CursorH = Clamp((int)(14 * s), 10, 18),
            };
        }
    }
}
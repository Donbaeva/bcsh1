using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheGatekeeper.Models;
using static TheGatekeeper.Models.Character;

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
        private List<StickerFloatPanel> _activeStickers = new List<StickerFloatPanel>();

        private int shutterHeight = 0;
        private const int shutterMaxHeight = 450;
        private Timer shutterTimer;
        private bool isAnimating = false;
        private bool isClosing = false;

        // ─── Зоны клика ─────────────────────────────────────────────────────
        private readonly Rectangle redZoneBase = new Rectangle(50, 550, 100, 80);
        private readonly Rectangle blueZoneBase = new Rectangle(120, 550, 100, 80);
        private readonly Rectangle greenZoneBase = new Rectangle(280, 550, 100, 80);
        private readonly Rectangle monitorRectBase = new Rectangle(350, 170, 585, 450);

        private readonly Rectangle zoneLeftTop = new Rectangle(58, 150, 140, 92);
        private readonly Rectangle zoneLeftMiddle = new Rectangle(53, 310, 165, 55);
        private readonly Rectangle zoneLeftBottom = new Rectangle(54, 380, 78, 58);
        private readonly Rectangle zoneSticker1 = new Rectangle(1025, 200, 85, 28);
        private readonly Rectangle zoneSticker2 = new Rectangle(1020, 81, 90, 85);
        private readonly Rectangle zoneSticker3 = new Rectangle(1038, 255, 75, 60);
        private readonly Rectangle zoneSticker4 = new Rectangle(1120, 143, 117, 140);
        private readonly Rectangle zoneRightScreen = new Rectangle(1047, 330, 185, 90);
        private readonly Rectangle zoneBigRadio = new Rectangle(1070, 555, 210, 125);
        private readonly Rectangle zoneSmallRadio = new Rectangle(950, 490, 63, 130);
        private readonly Rectangle zoneDialogueScreen = new Rectangle(510, 498, 275, 74);
        private readonly Rectangle zoneClock = new Rectangle(480, 572, 170, 50);

        private int hoveredZone = -1;

        // ─── Буфер рендера ───────────────────────────────────────────────────
        private BufferedGraphicsContext _bufCtx;
        private BufferedGraphics _buffer;

        // ─── Вспышка ─────────────────────────────────────────────────────────
        private int flashAlpha = 0;
        private Color flashColor = Color.Transparent;
        private Timer flashTimer;

        // ─── UI-лейблы ───────────────────────────────────────────────────────
        private Label lblScore, lblHealth, lblDay, lblQuota;
        private Label lblPressure;
        private Label lblName;
        private Label lblDialogue;
        private Label lblMode;          // плашка режима (угол экрана)

        // ─── Overlay ─────────────────────────────────────────────────────────
        private Panel overlayPanel;
        private Label overlayTitle;
        private Label overlayBody;
        private Button overlayClose;

        // ─── Состояние игры ──────────────────────────────────────────────────
        private int score = 0, health = 3, day = 1;
        private int charactersChecked = 0, dailyQuota = 5;
        private int pressureSeconds = 0;
        private Timer pressureTimer;

        public OverlayManager OverlayManagerInstance { get; private set; }

        private List<Character> todayCast;
        private Character currentCharacterData;
        private int currentCharacterIndex = 0;

        private int shiftSeconds = 0;

        private Timer typingTimer;
        private string fullDialogueText = "";
        private int typingIndex = 0;

        private List<(Character character, string decision)> dailyDecisions
            = new List<(Character character, string decision)>();

        private Rectangle[] interactiveZones;

        // ─── РЕЖИМ ИГРЫ ──────────────────────────────────────────────────────
        private GameMode currentMode;

        // Флаги для режима storyModeActive используются в Form1_StoryPatch
        internal bool storyModeActive => currentMode == GameMode.StoryMode;
        private bool huntModeActive => currentMode == GameMode.HuntMode;
        private bool endlessModeActive => currentMode == GameMode.EndlessMode;

        // ─── РЕЖИМ ОХОТЫ: данные о злодее ───────────────────────────────────
        private Character huntVillain;          // кто злодей в текущей волне
        private int huntWave = 1;       // номер волны (уровень сложности)
        private int huntWaveSize = 6;       // персонажей в волне
        private int huntCorrect = 0;       // верно пропущено/отклонено в волне
        private bool huntVillainFound = false;   // злодей найден в этой волне

        // ─── БЕСКОНЕЧНЫЙ РЕЖИМ ───────────────────────────────────────────────
        private int endlessTotal = 0;       // всего прошло персонажей
        private int endlessStreak = 0;       // серия верных ответов подряд
        private int endlessBestStreak = 0;

        // ═══════════════════════════════════════════════════════════════════
        //  КОНСТРУКТОР
        // ═══════════════════════════════════════════════════════════════════
        public Form1(GameMode mode = GameMode.StoryMode)
        {
            currentMode = mode;

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

            _bufCtx = BufferedGraphicsManager.Current;
            ReallocBuffer();
            this.Resize += (s, e) => { ReallocBuffer(); UpdateDialoguePosition(); };

            interactiveZones = new[]
            {
                zoneLeftTop, zoneLeftMiddle, zoneLeftBottom,
                zoneSticker1, zoneSticker2, zoneSticker3, zoneSticker4,
                zoneRightScreen, zoneBigRadio, zoneSmallRadio, zoneDialogueScreen
            };

            shutterTimer = new Timer { Interval = 15 };
            shutterTimer.Tick += ShutterTimer_Tick;

            flashTimer = new Timer { Interval = 30 };
            flashTimer.Tick += FlashTimer_Tick;
            InitButtonGlow();

            typingTimer = new Timer { Interval = 12 };
            typingTimer.Tick += TypingTimer_Tick;

            pressureTimer = new Timer { Interval = 1000 };
            pressureTimer.Tick += (s, e) =>
            {
                if (pressureSeconds < 60) pressureSeconds++;
                UpdatePressureUI();
                UpdateMonitorPanelNoise(); // обновляем уровень помех в панелях
                // В бесконечном режиме давление нарастает быстрее с каждым уровнем
                if (endlessModeActive && pressureSeconds >= 60)
                    ApplyEndlessPressurePenalty();
            };

            LoadResources();
            CreateTransparentUI();
            CreateOverlay();

            OverlayManagerInstance = new OverlayManager(this);

            // Инициализируем cast по режиму
            InitModeSession();

            this.MouseClick += Form_MouseClick_New;
            this.MouseMove += Form_MouseMove_New;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (OverlayManagerInstance?.IsVisible == true)
                    {
                        OverlayManagerInstance.Hide();
                        e.Handled = true;
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            };

            pressureTimer.Start();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ИНИЦИАЛИЗАЦИЯ СЕССИИ ПО РЕЖИМУ
        // ═══════════════════════════════════════════════════════════════════
        private void InitModeSession()
        {
            switch (currentMode)
            {
                case GameMode.StoryMode:
                    EndingTracker.Reset();
                    StoryFlags.HasanReachedLab = false;
                    StoryFlags.NinaArrested = false;
                    StoryFlags.MirraBetrayed = false;
                    StoryFlags.ServX1Passed = false;
                    day = 1;
                    InitDailyQuota();
                    todayCast = StorySchedule.BuildStoryCast(day, randomTypeCount: Math.Max(1, dailyQuota - 1));
                    if (day == 1) StartTutorialPhase();
                    else LoadCurrentCharacter();
                    break;

                case GameMode.HuntMode:
                    day = 1;
                    huntWave = 1;
                    huntWaveSize = 6;
                    dailyQuota = huntWaveSize;
                    BuildHuntWave();
                    break;

                case GameMode.EndlessMode:
                    day = 1;
                    endlessTotal = 0;
                    endlessStreak = 0;
                    endlessBestStreak = 0;
                    dailyQuota = 5;
                    todayCast = CharacterFactory.GenerateMixedCast(day, 1, 1, 0, 3);
                    break;
            }

            currentCharacterIndex = 0;
            if (!_tutorialActive) LoadCurrentCharacter();
            UpdateModeLabel();
            UpdateStatsUI();
        }

        // ─── ОХОТА: строим волну ─────────────────────────────────────────────
        private void BuildHuntWave()
        {
            // Чем выше волна, тем больше маскировка злодея
            int villainDay = Math.Min(huntWave + 1, 9);
            huntVillainFound = false;
            huntCorrect = 0;

            // Злодей — случайный тип (Робот или Пришелец), замаскированный под человека
            var rng = new Random();
            bool isRobot = rng.Next(0, 2) == 0;

            if (isRobot)
            {
                huntVillain = new Robot(
                    CharacterDatabase.GetRandomName(), "",
                    $"V-SN-{rng.Next(100, 999)}",
                    CharacterDatabase.GetRandomProfession(),
                    false,          // isObvious = всегда неочевиден
                    villainDay);
            }
            else
            {
                huntVillain = new Alien(
                    CharacterDatabase.GetRandomAlienName(), "",
                    CharacterDatabase.GetRandomAlienPlanet(),
                    rng.Next(0, 3),
                    false,
                    villainDay);
            }

            huntVillain.AccessCode = GenerateWrongCode(); // специально неверный
            huntVillain.Photo = null; // фабрика назначит фото
            huntVillain.Dialogue = CharacterAI.GenerateGreeting(huntVillain);

            // Обычные люди заполняют квоту
            var humans = CharacterFactory.GenerateDayCast(villainDay, huntWaveSize - 1, 0, 0);

            todayCast = humans;
            // Вставляем злодея в случайную позицию
            int pos = rng.Next(0, todayCast.Count + 1);
            todayCast.Insert(pos, huntVillain);

            dailyQuota = todayCast.Count;
        }

        private string GenerateWrongCode()
        {
            // Код от предыдущего дня — намеренно устаревший
            string[] prefixes = { "7741", "3392", "5521", "8834", "2219", "6657" };
            int idx = ((day - 1) + 5) % prefixes.Length; // сдвиг на "вчера"
            return $"{prefixes[idx]}-ERR";
        }

        // ═══════════════════════════════════════════════════════════════════
        //  MOUSE / KEYBOARD
        // ═══════════════════════════════════════════════════════════════════

        // ─── Центральный диспетчер решений ──────────────────────────────────
        private async Task ProcessDecision(string decision)
        {
            if (currentCharacterData == null) return;

            // Особые персонажи (взятки, клоны и т.д.)
            bool handled = await HandleSpecialCharacter(currentCharacterData, decision);
            if (handled) return;

            switch (currentMode)
            {
                case GameMode.StoryMode: await ProcessStoryDecision(decision); break;
                case GameMode.HuntMode: ProcessHuntDecision(decision); break;
                case GameMode.EndlessMode: ProcessEndlessDecision(decision); break;
            }

            dailyDecisions.Add((currentCharacterData, decision));
            charactersChecked++;

            UpdateStatsUI();
        }

        // ─── СЮЖЕТНЫЙ режим ─────────────────────────────────────────────────
        private async Task ProcessStoryDecision(string decision)
        {
            EndingTracker.RegisterDecision(currentCharacterData, decision);

            StartTypingEffect($"Decision: {decision}. Processing...");

            if (charactersChecked + 1 >= dailyQuota)
            {
                await Task.Delay(1500);
                if (day >= 10)
                    ShowStoryEnding();
                else
                    ShowDaySummary();
            }
        }

        // ─── РЕЖИМ ОХОТЫ ─────────────────────────────────────────────────────
        private void ProcessHuntDecision(string decision)
        {
            bool isVillain = ReferenceEquals(currentCharacterData, huntVillain);

            if (isVillain)
            {
                // Злодей — правильно его отклонить (не HUMAN)
                if (decision != "HUMAN")
                {
                    huntVillainFound = true;
                    score += 200 + huntWave * 50;
                    StartTypingEffect($"VILLAIN IDENTIFIED! +{200 + huntWave * 50} pts. Wave {huntWave} complete!");

                    // Показываем сообщение об успехе через небольшую паузу
                    Timer t = new Timer { Interval = 1800 };
                    t.Tick += (s, e) => { t.Stop(); NextHuntWave(); };
                    t.Start();
                }
                else
                {
                    // Злодей прошёл — штраф
                    health--;
                    score = Math.Max(0, score - 100);
                    StartTypingEffect($"VILLAIN PASSED THROUGH! -{100} pts. Health: {health}");
                    if (health <= 0) { ShowHuntGameOver(false); return; }
                }
            }
            else
            {
                // Обычный гражданин
                bool correct = decision == "HUMAN";
                if (correct)
                {
                    huntCorrect++;
                    score += 10;
                    StartTypingEffect($"Correct. Citizen cleared. Score +10.");
                }
                else
                {
                    health--;
                    score = Math.Max(0, score - 30);
                    StartTypingEffect($"Wrong! Innocent citizen flagged. -{30} pts.");
                    if (health <= 0) { ShowHuntGameOver(false); return; }
                }
            }

            UpdateStatsUI();
        }

        private void NextHuntWave()
        {
            huntWave++;
            huntWaveSize = Math.Min(6 + huntWave, 14);
            day = huntWave;

            BuildHuntWave();
            currentCharacterIndex = 0;
            charactersChecked = 0;
            dailyDecisions.Clear();
            pressureSeconds = 0;

            LoadCurrentCharacter();
            UpdateModeLabel();
            UpdateStatsUI();
            pressureTimer.Start();
        }

        private void ShowHuntGameOver(bool victory)
        {
            pressureTimer.Stop();
            string msg = victory
                ? $"ВОЛНА {huntWave} ПРОЙДЕНА!\n\nИтоговый счёт: {score}\nЗдоровье: {health}/3"
                : $"ЗЛОДЕЙ ПРОРВАЛСЯ\n\nВы дошли до волны {huntWave}.\nИтоговый счёт: {score}";

            MessageBox.Show(msg, victory ? "ПОБЕДА" : "КОНЕЦ СМЕНЫ",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        // ─── БЕСКОНЕЧНЫЙ режим ──────────────────────────────────────────────
        private void ProcessEndlessDecision(string decision)
        {
            bool correct = EndingTracker.IsCorrect(currentCharacterData, decision);
            endlessTotal++;

            if (correct)
            {
                endlessStreak++;
                if (endlessStreak > endlessBestStreak) endlessBestStreak = endlessStreak;

                // Бонус за серию
                int bonus = endlessStreak >= 5 ? 20 : endlessStreak >= 3 ? 15 : 10;
                score += bonus;
                StartTypingEffect($"Correct! Streak: {endlessStreak}x. +{bonus} pts.");
            }
            else
            {
                endlessStreak = 0;
                health--;
                score = Math.Max(0, score - 20);
                StartTypingEffect($"Wrong! Streak broken. -{20} pts. HP: {health}");

                if (health <= 0)
                {
                    ShowEndlessGameOver();
                    return;
                }
            }

            // Каждые 10 персонажей — переход на следующий «уровень»
            if (endlessTotal % 10 == 0)
            {
                day++;
                dailyQuota = Math.Min(dailyQuota + 1, 12);
                RegenerateEndlessCast();
                UpdateModeLabel();
            }

            UpdateStatsUI();
        }

        private void RegenerateEndlessCast()
        {
            // Добавляем новых персонажей в конец списка не трогая текущего
            var newChars = CharacterFactory.GenerateMixedCast(day, 2, 1, 1, 5);
            todayCast.AddRange(newChars);
        }

        private void ApplyEndlessPressurePenalty()
        {
            // Если игрок слишком долго думает — небольшой штраф
            pressureSeconds = 0;
            score = Math.Max(0, score - 5);
            UpdateStatsUI();
        }

        private void ShowEndlessGameOver()
        {
            pressureTimer.Stop();
            MessageBox.Show(
                $"БЕСКОНЕЧНАЯ СМЕНА ОКОНЧЕНА\n\n" +
                $"Проверено субъектов: {endlessTotal}\n" +
                $"Итоговый счёт:       {score}\n" +
                $"Лучшая серия:        {endlessBestStreak}x\n" +
                $"Дожили до уровня:    {day}",
                "КОНЕЦ СМЕНЫ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            this.Close();
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
        //  UI
        // ═══════════════════════════════════════════════════════════════════
        private void CreateTransparentUI()
        {
            this.Controls.Clear();

            lblScore = MakeLabel("📊 0", Color.Gold, new Point(400, 10), 140);
            lblHealth = MakeLabel("❤️ 3", Color.Tomato, new Point(550, 10), 100);
            lblDay = MakeLabel("📅 DAY 1", Color.Cyan, new Point(660, 10), 130);
            lblQuota = MakeLabel("📋 0/5", Color.White, new Point(800, 10), 110);

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

            lblName = new Label
            {
                Text = "OBJECT APPROACHING...",
                Location = new Point(400, 464),
                Size = new Size(565, 26),
                ForeColor = Color.FromArgb(255, 230, 230, 230),
                BackColor = Color.FromArgb(100, 0, 0, 0),
                Font = new Font("Consolas", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblDialogue = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(0, 255, 120),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 4, 10, 4)
            };
            // Позиция lblDialogue задаётся через zoneDialogueScreen — НЕ нужен ScaleControl
            // Обновляется при Resize через обработчик ниже
            lblDialogue.Location = new Point(0, 0); // временно, обновится в UpdateDialoguePosition()
            lblDialogue.Size = new Size(100, 40);

            // Плашка режима — правый верхний угол
            lblMode = new Label
            {
                Text = ModeTag(),
                Location = new Point(1050, 10),
                Size = new Size(200, 22),
                ForeColor = ModeTagColor(),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            foreach (var c in new Control[]
                { lblScore, lblHealth, lblDay, lblQuota, lblPressure, lblName, lblMode })
                ScaleControl(c);
            UpdateDialoguePosition(); // позиция диалога — отдельно через ScaleRect

            this.Controls.AddRange(new Control[]
                { lblScore, lblHealth, lblDay, lblQuota, lblPressure, lblName, lblDialogue, lblMode });
        }

        private string ModeTag()
        {
            switch (currentMode)
            {
                case GameMode.StoryMode: return "▶ ПРОТОКОЛ ВРАТА";
                case GameMode.HuntMode: return "🎯 ОХОТА";
                case GameMode.EndlessMode: return "∞ БЕСКОНЕЧНАЯ СМЕНА";
                default: return "";
            }
        }

        private Color ModeTagColor()
        {
            switch (currentMode)
            {
                case GameMode.StoryMode: return Color.Cyan;
                case GameMode.HuntMode: return Color.Tomato;
                case GameMode.EndlessMode: return Color.Gold;
                default: return Color.White;
            }
        }

        private void UpdateModeLabel()
        {
            if (lblMode == null) return;
            switch (currentMode)
            {
                case GameMode.HuntMode:
                    lblMode.Text = $"🎯 ОХОТА  Волна {huntWave}";
                    break;
                case GameMode.EndlessMode:
                    lblMode.Text = $"∞ LVL {day}  серия {endlessStreak}x";
                    break;
                default:
                    lblMode.Text = ModeTag();
                    break;
            }
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
        //  OVERLAY
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
            // Зоны 0, 1, 2 — плавающие анимированные панели мониторов
            if (index >= 0 && index <= 2)
            {
                OpenMonitorPanel(index);
                return;
            }

            // Зоны 3, 4, 5, 6 — Желтые стикеры
            if (index >= 3 && index <= 6)
            {
                // Вызываем метод, который правильно создает стикер с аргументом 'this'
                OpenSticker(index);
                return;
            }

            // index 7 = zoneRightScreen — показываем документы субъекта
            if (index == 7)
            {
                ShowFloatingDocument();
                return;
            }

            // Остальные оверлеи (рация и т.д.)
            var standardNote = NoteManager.GetDynamicNote(index, currentCharacterData);
            overlayTitle.Text = standardNote.Title;
            overlayBody.Text = standardNote.Body;
            overlayPanel.Visible = true;
        }

        // Вспомогательный метод для корректного создания стикера
        private void OpenSticker(int index)
        {
            var note = NoteManager.GetDynamicNote(index, currentCharacterData);

            // Проверяем, не открыт ли уже такой стикер, чтобы не плодить копии
            // (Для этого в Form1 должен быть список: private List<StickerFloatPanel> _activeStickers = new List<StickerFloatPanel>();)
            var existing = _activeStickers.Find(s => s.Text == note.Title && !s.IsDisposed);
            if (existing != null)
            {
                existing.BringToFront();
                return;
            }

            // ПЕРЕДАЕМ 'this' четвертым аргументом (это и есть наш owner)
            var sticker = new StickerFloatPanel(note.Title, note.Body, Cursor.Position, this);

            _activeStickers.Add(sticker);
            sticker.Show(this);
        }


        private void HideOverlay() => overlayPanel.Visible = false;

        // ═══════════════════════════════════════════════════════════════════
        //  ПЕРСОНАЖ
        // ═══════════════════════════════════════════════════════════════════
        private void LoadCurrentCharacter()
        {
            if (todayCast == null || currentCharacterIndex >= todayCast.Count) return;

            currentCharacterData = todayCast[currentCharacterIndex];
            currentCharacter?.Dispose();
            currentCharacter = currentCharacterData.Photo;
            GenerateDocForCurrentCharacter(); // генерируем документы субъекта

            // Закрываем панели предыдущего персонажа
            CloseAllMonitorPanels();
            CloseAllStickers();

            // Сброс лога диалога для нового персонажа
            ClearDialogueLog();
            // Записываем начальное приветствие в лог
            AddToDialogueLog(currentCharacterData.Name, currentCharacterData.Dialogue);

            if (huntModeActive)
                lblName.Text = $"[{huntCorrect}/{huntWaveSize - 1} cleared]";
            else
                lblName.Text = "SUBJECT APPROACHING";   // имя скрыто — узнай из допроса

            StartTypingEffect(currentCharacterData.Dialogue);

        }
        internal void CloseAllStickers()
        {
            foreach (var s in _activeStickers.ToArray())
            {
                if (s != null && !s.IsDisposed) s.Close();
            }
            _activeStickers.Clear();
        }


        // ═══════════════════════════════════════════════════════════════════
        //  TYPING EFFECT
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
            // Печатаем по 3 символа за тик — ощутимо быстрее
            for (int i = 0; i < 3; i++)
            {
                if (typingIndex < fullDialogueText.Length)
                {
                    lblDialogue.Text += fullDialogueText[typingIndex];
                    typingIndex++;
                }
                else { typingTimer.Stop(); break; }
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ДИАЛОГ ДОПРОСА
        // ═══════════════════════════════════════════════════════════════════
        private void OpenCharacterDialogue() => ShowInterrogationPanel();

        private void ShowQuestionDialog()
        {
            var qForm = new Form
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

            // В режиме ОХОТЫ — добавляем специальный вопрос-ловушку
            var questions = huntModeActive
                ? new[] {
                    "What is your access code?",
                    "Where are you coming from?",
                    "What is your purpose here?",
                    "Do you have family?",
                    "How do you feel today?",
                    "[HUNT] Describe your biological composition."
                  }
                : new[] {
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
                    BackColor = q.StartsWith("[HUNT]")
                        ? Color.FromArgb(60, 20, 20)
                        : Color.FromArgb(30, 50, 70),
                    ForeColor = Color.FromArgb(200, 220, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 9),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(51, 102, 170);
                btn.Click += (s, e) =>
                {
                    qForm.Close();
                    string answer = CharacterAI.GenerateAnswer(currentCharacterData, q);
                    LogQuestionAndAnswer(q, answer);  // записываем в лог
                    StartTypingEffect(answer);
                };
                qForm.Controls.Add(btn);
                y += 45;
            }

            qForm.Controls.Add(lblInfo);
            qForm.ShowDialog(this);
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
                        OnCastExhausted();
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

        // Вызывается когда весь cast пройден
        private void OnCastExhausted()
        {
            switch (currentMode)
            {
                case GameMode.StoryMode:
                    if (day >= 10) ShowStoryEnding();
                    else ShiftEndMessages.Show(this, day, () => ShowDaySummary());
                    break;

                case GameMode.HuntMode:
                    if (!huntVillainFound)
                    {
                        // Волна прошла, злодей не найден — штраф
                        health--;
                        if (health <= 0) { ShowHuntGameOver(false); return; }
                        MessageBox.Show(
                            $"Злодей прошёл незамеченным!\n-1 HP. Следующая волна сложнее.",
                            "ОХОТА", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    NextHuntWave();
                    break;

                case GameMode.EndlessMode:
                    // Добавляем новую порцию персонажей
                    day++;
                    dailyQuota = Math.Min(dailyQuota + 1, 15);
                    var newBatch = CharacterFactory.GenerateMixedCast(day, 2, 1, 1, 5);
                    todayCast.AddRange(newBatch);
                    LoadCurrentCharacter();
                    UpdateModeLabel();
                    break;
            }
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

            switch (currentMode)
            {
                case GameMode.StoryMode:
                    lblDay.Text = $"📅 ДЕНЬ {day}/10";
                    lblQuota.Text = $"📋 {charactersChecked}/{dailyQuota}";
                    break;
                case GameMode.HuntMode:
                    lblDay.Text = $"🎯 Волна {huntWave}";
                    lblQuota.Text = $"📋 {charactersChecked}/{dailyQuota}";
                    break;
                case GameMode.EndlessMode:
                    lblDay.Text = $"∞ LVL {day}";
                    lblQuota.Text = $"📋 {endlessTotal}  ×{endlessStreak}";
                    break;
            }

            lblHealth.ForeColor = health <= 1 ? Color.Red : Color.Tomato;
        }

        private void ShowDaySummary()
        {
            pressureTimer.Stop();

            var summaryForm = new DaySummaryForm(day, score, health,
                charactersChecked, dailyQuota, dailyDecisions);
            dailyDecisions.Clear();

            summaryForm.ShowDialog(this);

            if (summaryForm.ContinueToNextDay)
                StartNextDay();
            else
                this.Close();
        }

        private void StartNextDay()
        {
            day++;
            charactersChecked = 0;
            pressureSeconds = 0;
            InitDailyQuota(); // переменная квота 3–7 по таблице

            todayCast = storyModeActive
                ? StorySchedule.BuildStoryCast(day, randomTypeCount: Math.Max(1, dailyQuota - 1))
                : CharacterFactory.GenerateMixedCast(day, 1, 1, 0, Math.Max(1, dailyQuota - 2));

            currentCharacterIndex = 0;
            LoadCurrentCharacter();
            UpdateStatsUI();
            UpdateModeLabel();
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
            if (currentCharacterData == null) return "NO SUBJECT";

            string extra = "";
            if (huntModeActive && ReferenceEquals(currentCharacterData, huntVillain))
                extra = "\n⚠ BIOMETRIC ANOMALY";

            // С Дня 5 подсказка о сложности вместо типа вида
            string focusLine = currentCharacterData.Day >= 5
                ? $"FOCUS: {GetDailyFocusHint(currentCharacterData.Day).Replace("FOCUS: ", "")}"
                : $"TYPE:  {currentCharacterData.Species}";

            return $"NAME:  {currentCharacterData.Name}\n" +
                   $"JOB:   {currentCharacterData.Occupation}\n" +
                   $"CODE:  {currentCharacterData.AccessCode ?? "N/A"}\n" +
                   $"{focusLine}" +
                   extra;
        }

        private void ShowGameOver()
        {
            pressureTimer.Stop();
            MessageBox.Show(
                "Слишком много ошибок.\nКолония отстранила вас от должности.\n\nИГРА ОКОНЧЕНА",
                "ТРИБУНАЛ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  РЕНДЕР
        // ═══════════════════════════════════════════════════════════════════
        // Обновляет позицию и размер диалогового лейбла по ScaleRect
        private void UpdateDialoguePosition()
        {
            if (lblDialogue == null) return;
            Rectangle zone = ScaleRect(zoneDialogueScreen);
            lblDialogue.Location = zone.Location;
            lblDialogue.Size = zone.Size;
        }

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
            DrawObserverPassButton(g);
            DrawTutorialUI(g);


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
                g.DrawString(GetCharacterMonitorText(), font, brush, rect);
        }

        private void DrawInteractiveGlow(Graphics g)
        {
            if (hoveredZone < 0) return;
            Rectangle r = ScaleRect(interactiveZones[hoveredZone]);
            using (GraphicsPath path = RoundedRect(r, 10))
            {
                for (int i = 8; i >= 1; i--)
                    using (Pen p = new Pen(Color.FromArgb(18 * i, 0, 255, 255), i))
                        g.DrawPath(p, path);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(18, 0, 255, 255)))
                    g.FillPath(br, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DrawClock(Graphics g)
        {
            Rectangle zone = ScaleRect(zoneClock);
            int m = shiftSeconds / 60, s = shiftSeconds % 60;
            using (var font = new Font("Consolas", zone.Height * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var br = new SolidBrush(Color.FromArgb(255, 0, 170, 255)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString($"{m:D2}:{s:D2}", font, br, zone, sf);
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
                    Color.FromArgb(210, 215, 225), Color.FromArgb(70, 75, 85),
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
                        Color.FromArgb(80, 255, 255, 255), Color.FromArgb(0, 255, 255, 255),
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
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
        private bool _dayJustEnded = false; // флаг: день завершён, шторка не нужна

        // ─── Зоны клика ─────────────────────────────────────────────────────
        // Центры круглых кнопок в базовом разрешении
        private readonly Point redCenterBase = new Point(92, 590);
        private readonly Point blueCenterBase = new Point(200, 590);
        private readonly Point greenCenterBase = new Point(312, 590);
        private const int buttonRadiusBase = 38;
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
        private readonly Rectangle zoneDialogueScreen = new Rectangle(515, 502, 265, 60);
        private readonly Rectangle zoneClock = new Rectangle(840, 498, 75, 50);

        private int hoveredZone = -1;

        // ─── Буфер рендера ───────────────────────────────────────────────────
        private BufferedGraphicsContext _bufCtx;
        private BufferedGraphics _buffer;

        // ─── Вспышка ─────────────────────────────────────────────────────────
        private int flashAlpha = 0;
        private Color flashColor = Color.Transparent;
        private Timer flashTimer;

        // ─── UI-лейблы ───────────────────────────────────────────────────────
        private Label lblScore, lblDay, lblQuota;
        private Label lblHealth = null; // убран из HUD, оставлен для совместимости
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
        internal int credits = 0;   // кредиты — получаются от взяток, тратятся на подкупы
        private int charactersChecked = 0, dailyQuota = 5;
        private int pressureSeconds = 0;
        private Timer pressureTimer;

        public OverlayManager OverlayManagerInstance { get; private set; }

        private List<Character> todayCast;
        private Character currentCharacterData;
        private int currentCharacterIndex = 0;

        private int shiftSeconds = 0;  // реальные секунды смены
        private const int ShiftStartHour = 9;   // 09:00 — начало смены
        private const int ShiftEndHour = 17;  // 17:00 — конец смены
        private const float ShiftSpeedMult = 8f; // игровое время идёт в 8x быстрее

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
                if (pressureSeconds < 120) pressureSeconds++;
                shiftSeconds++;
                UpdatePressureUI();
                UpdateMonitorPanelNoise();
                if (endlessModeActive && pressureSeconds >= 60)
                    ApplyEndlessPressurePenalty();
                // Отслеживаем высокое давление для секретной концовки
                if (storyModeActive)
                {
                    EndingTracker.TotalTicks++;
                    int pct = (int)(pressureSeconds * 100f / 120);
                    if (pct >= 90) EndingTracker.HighPressureTicks++;
                }
                if (!isAnimating) Redraw();
            };

            LoadResources();
            CreateTransparentUI();
            CreateOverlay();

            OverlayManagerInstance = new OverlayManager(this);

            // Инициализируем cast по режиму
            InitModeSession();

            this.MouseClick += Form_MouseClick_New;
            // MouseUp для часов — срабатывает надёжнее чем MouseClick
            this.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && ScaleRect(zoneClock).Contains(e.Location))
                    ShowClockMenu();
            };
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

            // При отказе (ROBOT/ALIEN) — случайный шанс предложить взятку
            if (decision != "HUMAN" && storyModeActive)
            {
                bool offeredBribe = await TryOfferBribeOnDetect(currentCharacterData, decision);
                if (offeredBribe) return;
            }

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

            if (charactersChecked + 1 >= dailyQuota)
            {
                await Task.Delay(1500);
                _dayJustEnded = true; // шторка не должна делать index++ после этого
                if (day >= 7)
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
                ? $"WAVE {huntWave} CLEARED!\n\nFinal score: {score}\nHealth: {health}/3"
                : $"VILLAIN ESCAPED\n\nYou reached wave {huntWave}.\nFinal score: {score}";

            MessageBox.Show(msg, victory ? "VICTORY" : "SHIFT ENDED",
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
                "ENDLESS SHIFT COMPLETE\n\n" +
                $"Subjects checked: {endlessTotal}\n" +
                $"Final score:      {score}\n" +
                $"Best streak:      {endlessBestStreak}x\n" +
                $"Reached level:    {day}",
                "SHIFT ENDED",
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

            // HUD лейблы — скрыты, данные в стикере
            lblScore = MakeLabel("", Color.Gold, new Point(0, 0), 1);
            lblScore.Visible = false;
            lblHealth = null;
            lblDay = MakeLabel("", Color.Cyan, new Point(0, 0), 1);
            lblDay.Visible = false;
            lblQuota = MakeLabel("", Color.White, new Point(0, 0), 1);
            lblQuota.Visible = false;

            lblPressure = new Label
            {
                Text = "",
                Visible = false,
                Location = new Point(0, 0),
                Size = new Size(1, 1),
                BackColor = Color.Transparent
            };

            lblName = new Label
            {
                Text = "",
                Location = new Point(0, 0),
                Size = new Size(1, 1),
                Visible = false   // имя скрыто — показывается только в документах
            };

            lblDialogue = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(0, 255, 120),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 4, 10, 4),
                Cursor = Cursors.Hand  // намекает что кликабельно
            };
            // Клик по диалогу — открываем лог
            lblDialogue.Click += (s, e) => ShowDialogueLog();
            // Позиция lblDialogue задаётся через zoneDialogueScreen — НЕ нужен ScaleControl
            // Обновляется при Resize через обработчик ниже
            lblDialogue.Location = new Point(0, 0); // временно, обновится в UpdateDialoguePosition()
            lblDialogue.Size = new Size(100, 40);

            // Плашка режима — правый верхний угол
            lblMode = new Label
            {
                Text = "",
                Visible = false,
                Location = new Point(1050, 10),
                Size = new Size(200, 22),
                ForeColor = ModeTagColor(),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            foreach (var c in new Control[]
                { lblScore, lblDay, lblQuota, lblPressure, lblName, lblMode })
                ScaleControl(c);
            UpdateDialoguePosition(); // позиция диалога — отдельно через ScaleRect

            // Кнопка паузы / меню
            var btnPause = new Button
            {
                Text = "⚙",
                Location = new Point((int)(1230 * Screen.PrimaryScreen.Bounds.Width / (float)BaseW),
                                     (int)(8 * Screen.PrimaryScreen.Bounds.Height / (float)BaseH)),
                Size = new Size((int)(40 * Screen.PrimaryScreen.Bounds.Width / (float)BaseW),
                                (int)(26 * Screen.PrimaryScreen.Bounds.Height / (float)BaseH)),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(140, 160, 180),
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnPause.FlatAppearance.BorderSize = 0;
            btnPause.Click += (s, e) => ShowPauseMenu();
            this.Controls.Add(btnPause);


            this.Controls.AddRange(new Control[]
                { lblScore, lblDay, lblQuota, lblPressure, lblName, lblDialogue, lblMode });
        }

        private string ModeTag()
        {
            switch (currentMode)
            {
                case GameMode.StoryMode: return "▶ GATE PROTOCOL";
                case GameMode.HuntMode: return "🎯 HUNT";
                case GameMode.EndlessMode: return "∞ ENDLESS SHIFT";
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
                    lblMode.Text = $"🎯 HUNT  Wave {huntWave}";
                    break;
                case GameMode.EndlessMode:
                    lblMode.Text = $"∞ LVL {day}  streak {endlessStreak}x";
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
            // Пробрасываем MouseUp с overlayPanel на форму (для клика по часам)
            overlayPanel.MouseUp += (s, e) =>
            {
                var screenPt = overlayPanel.PointToScreen(e.Location);
                var formPt = this.PointToClient(screenPt);
                if (e.Button == MouseButtons.Left && ScaleRect(zoneClock).Contains(formPt))
                    ShowClockMenu();
            };

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

        // Вызывается из OverlayManager при MouseUp
        internal void TryOpenClockFromOverlay(Point screenPoint)
        {
            var formPt = this.PointToClient(screenPoint);
            if (ScaleRect(zoneClock).Contains(formPt))
                ShowClockMenu();
        }

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
            CloseAllFloatingWindows();

            // Сброс лога диалога для нового персонажа
            ClearDialogueLog();
            // Записываем начальное приветствие в лог
            AddToDialogueLog(currentCharacterData.Name, currentCharacterData.Dialogue);

            StartTypingEffect(currentCharacterData.Dialogue);

            // Вызываем OnArrival для сюжетных персонажей (передача документов и т.д.)
            if (currentCharacterData is IStoryCharacter sc)
                sc.OnArrival();

            // Случайные персонажи тоже могут нести документы
            if (!string.IsNullOrEmpty(currentCharacterData.CarriedDocumentType))
                GiveCarriedDocument(currentCharacterData);

        }
        internal void CloseAllStickers()
        {
            foreach (var s in _activeStickers.ToArray())
            {
                if (s != null && !s.IsDisposed) s.Close();
            }
            _activeStickers.Clear();
        }

        internal void CloseAllFloatingWindows()
        {
            // Закрываем SUBJECT PROFILE, INTERROGATION, DOCUMENT VAULT и другие плавающие окна
            var toClose = new System.Collections.Generic.List<Form>();
            foreach (Form f in Application.OpenForms)
            {
                if (f is Form1) continue;
                if (f.Text == "SUBJECT PROFILE" || f.Text == "INTERROGATION" ||
                    f.Text == "DOCUMENT VAULT" || f.Text == "SUBJECT STATUS" ||
                    f.Text == "IDENTITY DOCUMENT")
                    toClose.Add(f);
            }
            foreach (var f in toClose)
                if (!f.IsDisposed) f.Close();
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

                    // Если день только что завершился — StartNextDay уже всё сделал
                    if (_dayJustEnded)
                    {
                        _dayJustEnded = false;
                        shutterHeight = 0; // сразу открываем шторку
                        isAnimating = false;
                        Redraw();
                        return;
                    }

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
                    if (day >= 7) ShowStoryEnding();
                    else ShiftEndMessages.Show(this, day, () => ShowDaySummary());
                    break;

                case GameMode.HuntMode:
                    if (!huntVillainFound)
                    {
                        // Волна прошла, злодей не найден — штраф
                        health--;
                        if (health <= 0) { ShowHuntGameOver(false); return; }
                        MessageBox.Show(
                            "Villain slipped through! -1 HP. Next wave is harder.",
                            "HUNT MODE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            // HUD скрыт — данные хранятся в полях, видны в меню часов и стикере
            // Redraw НЕ вызываем здесь — он вызывается из pressureTimer и ShutterTimer
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
            shiftSeconds = 0; // сброс таймера смены
            _dayJustEnded = false;
            InitDailyQuota();

            todayCast = storyModeActive
                ? StorySchedule.BuildStoryCast(day, randomTypeCount: Math.Max(1, dailyQuota - 1))
                : CharacterFactory.GenerateMixedCast(day, 1, 1, 0, Math.Max(1, dailyQuota - 2));

            currentCharacterIndex = 0;

            // Открываем шторку через небольшую паузу — персонаж 0 загружен, шторка идёт вниз
            shutterHeight = shutterMaxHeight; // шторка закрыта
            isClosing = false;
            isAnimating = true;

            LoadCurrentCharacter();  // загружаем персонажа под шторкой
            UpdateStatsUI();
            UpdateModeLabel();
            pressureTimer.Start();

            // Каждый новый день — ежедневный документ автоматически
            ReceiveDocument(Form1.DocGetDailyFree(day));

            // Запускаем анимацию открытия шторки
            shutterTimer.Start();
            Redraw();
        }

        private void UpdatePressureUI()
        {
            int pct = Math.Min(100, (int)(pressureSeconds * 100f / 120));
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
                "Too many errors.\nThe colony has relieved you of your post.\n\nSHIFT TERMINATED",
                "TRIBUNAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            DrawMonitorText(g);

            if (frontBackground != null)
                g.DrawImage(frontBackground, 0, 0, ClientSize.Width, ClientSize.Height);

            DrawClock(g);
            DrawInteractiveGlow(g);
            DrawObserverPassButton(g);
            DrawTutorialUI(g);

            // Кнопки теперь рисуются программно в DrawInteractiveGlow

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
            // Подсветка интерактивных зон
            if (hoveredZone >= 0)
            {
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

            // Круглые кнопки ROBOT / ALIEN / HUMAN
            if (currentCharacterData != null && !currentCharacterData.IsObserver)
            {
                float sx = (float)ClientSize.Width / BaseW;
                float sy = (float)ClientSize.Height / BaseH;
                int scaledR = (int)(buttonRadiusBase * Math.Min(sx, sy));

                Point redC = new Point((int)(redCenterBase.X * sx), (int)(redCenterBase.Y * sy));
                Point blueC = new Point((int)(blueCenterBase.X * sx), (int)(blueCenterBase.Y * sy));
                Point greenC = new Point((int)(greenCenterBase.X * sx), (int)(greenCenterBase.Y * sy));

                Point[] centers = { redC, blueC, greenC };
                Color[] btnColors = {
                    Color.FromArgb(255, 60,  60),
                    Color.FromArgb(60,  120, 255),
                    Color.FromArgb(60,  220, 60),
                };
                string[] labels = { "ROBOT", "ALIEN", "HUMAN" };

                Point mouse = PointToClient(Cursor.Position);
                int hoverIdx = -1;
                for (int i = 0; i < 3; i++)
                {
                    int dx = mouse.X - centers[i].X, dy = mouse.Y - centers[i].Y;
                    if (dx * dx + dy * dy <= scaledR * scaledR) { hoverIdx = i; break; }
                }

                for (int i = 0; i < 3; i++)
                {
                    bool hov = (i == hoverIdx);
                    Rectangle circ = new Rectangle(
                        centers[i].X - scaledR, centers[i].Y - scaledR,
                        scaledR * 2, scaledR * 2);

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(circ);

                        // Свечение только при наведении
                        if (hov)
                        {
                            for (int j = 10; j >= 1; j--)
                                using (Pen p = new Pen(Color.FromArgb(18 * j, btnColors[i]), j + 1))
                                    g.DrawPath(p, path);
                            using (var br = new SolidBrush(Color.FromArgb(40, btnColors[i])))
                                g.FillPath(br, path);
                        }
                    }

                    // Подпись внутри круга
                    using (var font = new Font("Consolas", hov ? 8.5f : 7.5f, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.FromArgb(hov ? 240 : 180, Color.White)))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(labels[i], font, brush, centers[i], sf);
                    }
                }
            }
        }

        private bool IsPointInCircle(Point click, Point center, int radius)
        {
            int dx = click.X - center.X;
            int dy = click.Y - center.Y;
            return dx * dx + dy * dy <= radius * radius;
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

            // Подсветка при наведении
            bool hovered = zone.Contains(PointToClient(Cursor.Position));
            if (hovered)
            {
                using (var br = new SolidBrush(Color.FromArgb(30, 0, 200, 255)))
                    g.FillRectangle(br, zone);
                using (var pen = new Pen(Color.FromArgb(80, 0, 200, 255), 1))
                    g.DrawRectangle(pen, zone);
            }

            using (var font = new Font("Consolas", zone.Height * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var br = new SolidBrush(hovered
                ? Color.FromArgb(255, 80, 220, 255)
                : Color.FromArgb(255, 0, 170, 255)))
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
        // ═══════════════════════════════════════════════════════════════
        //  МЕНЮ ПАУЗЫ — сохранение, настройки, выход
        // ═══════════════════════════════════════════════════════════════
        internal void ShowClockMenu()
        {
            // Не открываем дубль
            foreach (Form f in Application.OpenForms)
                if (!(f is Form1) && f.Text == "SHIFT STATUS" && !f.IsDisposed)
                { f.BringToFront(); return; }

            var win = new Form
            {
                Text = "SHIFT STATUS",
                Size = new Size(300, 320),
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(10, 14, 20),
                TopMost = true,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
            };

            // Позиция — над часами
            var clkR = ScaleRect(zoneClock);
            win.Location = new Point(
                Math.Max(0, clkR.Left - win.Width + clkR.Width),
                Math.Max(0, clkR.Top - win.Height - 6));

            // Рамка
            win.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(120, 0, 160, 220), 1))
                    g.DrawRectangle(pen, 0, 0, win.Width - 1, win.Height - 1);
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, win.Width, 3),
                    Color.FromArgb(180, 0, 160, 220), Color.Transparent,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, win.Width, 3);
            };

            // Заголовок + перетаскивание
            bool drag = false; Point dragOff = Point.Empty;
            var header = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(10, 22, 36) };
            var hLbl = new Label
            {
                Text = "  ⏱  SHIFT STATUS",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(80, 160, 220),
                Font = new Font("Consolas", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var hX = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 80, 80),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            hX.FlatAppearance.BorderSize = 0;
            hX.Click += (s, e) => { win.Close(); pressureTimer.Start(); };
            header.Controls.Add(hLbl); header.Controls.Add(hX);
            void Drag(Control ctrl)
            {
                ctrl.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { drag = true; dragOff = e.Location; } };
                ctrl.MouseMove += (s, e) => { if (drag) win.Location = new Point(win.Left + e.X - dragOff.X, win.Top + e.Y - dragOff.Y); };
                ctrl.MouseUp += (s, e) => drag = false;
            }
            Drag(header); Drag(hLbl);

            // Тело — статы
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 10, 14, 10), BackColor = Color.Transparent };

            // Игровое время
            int gs = (int)(shiftSeconds * ShiftSpeedMult);
            int gh = ShiftStartHour + gs / 3600;
            int gm2 = (gs % 3600) / 60;
            string tStr = $"{Math.Min(gh, ShiftEndHour):D2}:{gm2:D2}";

            int pct = Math.Min(100, (int)(pressureSeconds * 100f / 120));
            Color pCol = pct < 50 ? Color.FromArgb(0, 200, 150)
                       : pct < 75 ? Color.FromArgb(220, 180, 0)
                       : Color.FromArgb(220, 60, 60);

            // Добавляем строки
            int ry = 10;
            void Row(string label, string value, Color valCol)
            {
                body.Controls.Add(new Label
                {
                    Text = label,
                    Location = new Point(0, ry),
                    Size = new Size(130, 20),
                    ForeColor = Color.FromArgb(70, 100, 140),
                    Font = new Font("Consolas", 8f),
                    BackColor = Color.Transparent,
                    AutoSize = false
                });
                body.Controls.Add(new Label
                {
                    Text = value,
                    Location = new Point(130, ry),
                    Size = new Size(140, 20),
                    ForeColor = valCol,
                    Font = new Font("Consolas", 9f, FontStyle.Bold),
                    BackColor = Color.Transparent,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight
                });
                ry += 24;
            }

            void Divider()
            {
                body.Controls.Add(new Label
                {
                    Location = new Point(0, ry),
                    Size = new Size(272, 1),
                    BackColor = Color.FromArgb(30, 51, 102, 170)
                });
                ry += 10;
            }

            // Время крупно
            body.Controls.Add(new Label
            {
                Text = tStr,
                Location = new Point(0, ry),
                Size = new Size(272, 36),
                ForeColor = Color.FromArgb(0, 190, 255),
                Font = new Font("Consolas", 22f, FontStyle.Bold),
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            });
            ry += 42;
            Divider();

            Row("DAY", $"{day} / 7", Color.FromArgb(100, 180, 255));
            Row("CHECKED", $"{charactersChecked} / {dailyQuota}", Color.FromArgb(0, 210, 100));
            Row("REMAINING", $"{Math.Max(0, dailyQuota - charactersChecked)}", Color.FromArgb(220, 180, 60));
            Divider();
            Row("WALLET", $"{credits} cr", Color.FromArgb(200, 220, 80));
            Row("PRESSURE", $"{pct}%", pCol);
            Divider();

            // Кнопки
            void Btn(string text, Color fg, Color bg, Action act)
            {
                var btn = new Button
                {
                    Text = text,
                    Location = new Point(0, ry),
                    Size = new Size(272, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = bg,
                    ForeColor = fg,
                    Font = new Font("Consolas", 8f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 0, 0)
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(fg.R / 6, fg.G / 6, fg.B / 6 + 15);
                btn.Click += (s, e) => { win.Close(); act(); };
                body.Controls.Add(btn);
                ry += 38;
            }

            Btn("▶  CONTINUE SHIFT", Color.FromArgb(0, 210, 100), Color.FromArgb(8, 38, 16), () => pressureTimer.Start());
            Btn("⏹  END SHIFT EARLY", Color.FromArgb(220, 160, 40), Color.FromArgb(28, 22, 8), () => ShowDaySummary());
            Btn("💾  SAVE", Color.FromArgb(100, 160, 240), Color.FromArgb(8, 18, 38), () => { SaveProgress(); pressureTimer.Start(); });
            Btn("✕  EXIT", Color.FromArgb(200, 80, 80), Color.FromArgb(28, 8, 8), () => this.Close());

            // Подгоняем высоту под контент
            win.Size = new Size(300, 28 + body.Padding.Top + ry + body.Padding.Bottom + 10);

            win.Controls.Add(body);
            win.Controls.Add(header);
            win.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { win.Close(); pressureTimer.Start(); } };
            pressureTimer.Stop();
            win.Show(this);
        }

        private void ShowPauseMenu()
        {
            pressureTimer.Stop();
            typingTimer.Stop();

            var menu = new Form
            {
                Size = new Size(340, 280),
                BackColor = Color.FromArgb(10, 14, 20),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                TopMost = true,
                ShowInTaskbar = false
            };
            menu.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(80, 51, 102, 170), 1))
                    g.DrawRectangle(pen, 0, 0, menu.Width - 1, menu.Height - 1);
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, menu.Width, 3),
                    Color.FromArgb(160, 51, 130, 200), Color.Transparent,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, menu.Width, 3);
            };

            menu.Controls.Add(new Label
            {
                Text = "  ⚙  MENU",
                Location = new Point(16, 14),
                Size = new Size(308, 22),
                ForeColor = Color.FromArgb(100, 160, 220),
                Font = new Font("Consolas", 11, FontStyle.Bold),
                BackColor = Color.Transparent
            });
            menu.Controls.Add(new Label
            {
                Location = new Point(16, 38),
                Size = new Size(308, 1),
                BackColor = Color.FromArgb(50, 51, 102, 170)
            });

            int by = 50;
            void AddMenuBtn(string text, Color fg, Color bg, Action onClick)
            {
                var btn = new Button
                {
                    Text = text,
                    Location = new Point(16, by),
                    Size = new Size(308, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = bg,
                    ForeColor = fg,
                    Font = new Font("Consolas", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0)
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(fg.R / 4, fg.G / 4, fg.B / 4 + 20);
                btn.Click += (s, e) => { menu.Close(); onClick(); };
                menu.Controls.Add(btn);
                by += 48;
            }

            AddMenuBtn("▶  CONTINUE SHIFT",
                Color.FromArgb(0, 200, 100), Color.FromArgb(10, 40, 18),
                () => { pressureTimer.Start(); typingTimer.Start(); });

            AddMenuBtn("💾  SAVE PROGRESS",
                Color.FromArgb(100, 160, 240), Color.FromArgb(10, 20, 40),
                () => { SaveProgress(); pressureTimer.Start(); });

            AddMenuBtn("🔊  SOUND  (placeholder)",
                Color.FromArgb(160, 160, 180), Color.FromArgb(14, 14, 24),
                () => { pressureTimer.Start(); });

            AddMenuBtn("✕  EXIT TO MENU",
                Color.FromArgb(200, 80, 80), Color.FromArgb(28, 10, 10),
                () => { this.Close(); });

            menu.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { menu.Close(); pressureTimer.Start(); }
            };
            menu.ShowDialog(this);
        }

        // ─── Сохранение прогресса ────────────────────────────────────────
        private void SaveProgress()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"day={day}");
                sb.AppendLine($"score={score}");
                sb.AppendLine($"credits={credits}");
                sb.AppendLine($"health={health}");
                sb.AppendLine($"loyalty={EndingTracker.Loyalty}");
                sb.AppendLine($"rebelTrust={EndingTracker.RebelTrust}");
                sb.AppendLine($"bribesAccepted={EndingTracker.BribesAccepted}");
                sb.AppendLine($"wolfWarnings={EndingTracker.WolfWarnings}");
                sb.AppendLine($"savedAt={DateTime.Now:yyyy-MM-dd HH:mm}");

                string path = System.IO.Path.Combine(
                    Application.StartupPath, "save.dat");
                System.IO.File.WriteAllText(path, sb.ToString());

                StartTypingEffect("Progress saved. Come back when you're ready, Inspector.");
            }
            catch (Exception ex)
            {
                StartTypingEffect($"Save failed: {ex.Message}");
            }
        }

    }
}
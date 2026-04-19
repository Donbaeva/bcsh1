// ═══════════════════════════════════════════════════════════════════════
//  Form1_Input.cs  — partial class Form1
//
//  Заменяет три файла:
//    Form1_ButtonFix.cs
//    Form1_ObserverAndDialogue.cs  (удалить из проекта)
//    Form1_Overlay.cs              (удалить из проекта — не использовался)
//
//  Содержит:
//  • Form_MouseClick_New  — главный обработчик кликов
//  • Form_MouseMove_New   — hover + курсор
//  • ProcessObserverDecision / DrawObserverPassButton
//  • Туториал: HandleTutorialClick / DrawTutorialUI / UpdateTutorialHover
//  • InitButtonGlow (ПОДКЛЮЧИТЬ в конструкторе Form1: InitButtonGlow();)
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public partial class Form1
    {
        // ─── Зоны кнопок ────────────────────────────────────────────────────
        private readonly Rectangle observerPassZone = new Rectangle(50, 550, 330, 80);
        private readonly Rectangle _startShiftZone = new Rectangle(50, 548, 340, 82);

        // ─── Состояние кнопок ────────────────────────────────────────────────
        private int pressedButton = -1;
        private int glowAlpha = 0;
        private Timer glowTimer;
        private int hoveredButton = -1;

        // ─── Туториал — поля объявлены в блоке StartTutorialPhase ────────────

        // ════════════════════════════════════════════════════════════════════
        //  ИНИЦИАЛИЗАЦИЯ — вызвать из конструктора Form1 после flashTimer
        // ════════════════════════════════════════════════════════════════════
        internal void InitButtonGlow()
        {
            glowTimer = new Timer { Interval = 25 };
            glowTimer.Tick += (s, e) =>
            {
                glowAlpha -= 8;
                if (glowAlpha <= 0) { glowAlpha = 0; pressedButton = -1; glowTimer.Stop(); }
                Redraw();
            };
        }

        // ════════════════════════════════════════════════════════════════════
        //  MOUSE MOVE
        // ════════════════════════════════════════════════════════════════════
        internal void Form_MouseMove_New(object sender, MouseEventArgs e)
        {
            // Туториал — свой hover
            if (_tutorialActive)
            {
                UpdateTutorialHover(e.Location);
                return;
            }

            int newHovered = -1;
            int newHoveredBtn = -1;

            for (int i = 0; i < interactiveZones.Length; i++)
                if (ScaleRect(interactiveZones[i]).Contains(e.Location)) { newHovered = i; break; }

            if (currentCharacterData != null && !currentCharacterData.IsObserver)
            {
                // Круглые кнопки — проверка через окружность
                float sx_ = (float)ClientSize.Width / BaseW;
                float sy_ = (float)ClientSize.Height / BaseH;
                int scaledR_ = (int)(buttonRadiusBase * Math.Min(sx_, sy_));
                Point mousePos_ = e.Location;
                Point rC_ = new Point((int)(redCenterBase.X * sx_), (int)(redCenterBase.Y * sy_));
                Point bC_ = new Point((int)(blueCenterBase.X * sx_), (int)(blueCenterBase.Y * sy_));
                Point gC_ = new Point((int)(greenCenterBase.X * sx_), (int)(greenCenterBase.Y * sy_));
                if (IsPointInCircle(mousePos_, rC_, scaledR_)) newHoveredBtn = 0;
                else if (IsPointInCircle(mousePos_, bC_, scaledR_)) newHoveredBtn = 1;
                else if (IsPointInCircle(mousePos_, gC_, scaledR_)) newHoveredBtn = 2;
            }

            bool changed = (hoveredZone != newHovered) || (hoveredButton != newHoveredBtn);
            hoveredZone = newHovered;
            hoveredButton = newHoveredBtn;

            bool onActive = newHovered >= 0
                || newHoveredBtn >= 0
                || ScaleRect(zoneClock).Contains(e.Location)
                || (currentCharacterData?.IsObserver == true && ScaleRect(observerPassZone).Contains(e.Location))
                || ScaleRect(zoneDialogueScreen).Contains(e.Location);

            Cursor = onActive ? Cursors.Hand : Cursors.Default;
            if (changed) Redraw();
        }

        // ════════════════════════════════════════════════════════════════════
        //  MOUSE CLICK
        // ════════════════════════════════════════════════════════════════════
        internal async void Form_MouseClick_New(object sender, MouseEventArgs e)
        {
            // Туториал — свой обработчик
            if (_tutorialActive)
            {
                HandleTutorialClick(e.Location);
                return;
            }

            if (overlayPanel.Visible) return;

            Point p = e.Location;

            // Диалог-экран → лог разговора
            if (ScaleRect(zoneDialogueScreen).Contains(p))
            {
                ShowDialogueLog();
                return;
            }

            // Интерактивные зоны
            for (int i = 0; i < interactiveZones.Length; i++)
            {
                if (ScaleRect(interactiveZones[i]).Contains(p))
                {
                    if (i == 9) { ShowInterrogationPanel(); return; }
                    ShowOverlay(i);
                    return;
                }
            }

            if (isAnimating || currentCharacterData == null) return;

            // Наблюдатель — только [ПРОПУСТИТЬ]
            if (currentCharacterData.IsObserver)
            {
                if (ScaleRect(observerPassZone).Contains(p))
                    await ProcessObserverDecision();
                return;
            }

            // Круглые кнопки — проверка через окружности
            float sx = (float)ClientSize.Width / BaseW;
            float sy = (float)ClientSize.Height / BaseH;
            int scaledR = (int)(buttonRadiusBase * Math.Min(sx, sy));
            Point redC = new Point((int)(redCenterBase.X * sx), (int)(redCenterBase.Y * sy));
            Point blueC = new Point((int)(blueCenterBase.X * sx), (int)(blueCenterBase.Y * sy));
            Point greenC = new Point((int)(greenCenterBase.X * sx), (int)(greenCenterBase.Y * sy));

            string decision = "";
            int btnIndex = -1;
            Color flashCol = Color.Transparent;

            if (IsPointInCircle(p, redC, scaledR)) { decision = "ROBOT"; btnIndex = 0; flashCol = Color.Red; }
            else if (IsPointInCircle(p, blueC, scaledR)) { decision = "ALIEN"; btnIndex = 1; flashCol = Color.DodgerBlue; }
            else if (IsPointInCircle(p, greenC, scaledR)) { decision = "HUMAN"; btnIndex = 2; flashCol = Color.Lime; }

            if (btnIndex < 0) return;

            TriggerButtonGlow(btnIndex);
            isAnimating = true;
            pressureTimer.Stop();

            StartFlash(flashCol);
            Redraw();
            await Task.Delay(250);

            await ProcessDecision(decision);

            isClosing = true;
            shutterTimer.Start();
        }

        // ════════════════════════════════════════════════════════════════════
        //  НАБЛЮДАТЕЛЬ — [ПРОПУСТИТЬ]
        // ════════════════════════════════════════════════════════════════════
        private async Task ProcessObserverDecision()
        {
            if (currentCharacterData == null || !currentCharacterData.IsObserver) return;

            if (currentCharacterData is IStoryCharacter sc)
                sc.ApplyEffect("PASS");

            StartTypingEffect($"{currentCharacterData.Name} nods and passes through.");

            isAnimating = true;
            pressureTimer.Stop();
            StartFlash(Color.FromArgb(80, 120, 180));
            Redraw();
            await Task.Delay(300);

            dailyDecisions.Add((currentCharacterData, "PASS"));
            charactersChecked++;
            UpdateStatsUI();

            isClosing = true;
            shutterTimer.Start();
        }

        internal void DrawObserverPassButton(Graphics g)
        {
            if (currentCharacterData == null || !currentCharacterData.IsObserver) return;

            Rectangle zone = ScaleRect(observerPassZone);
            bool hovered = zone.Contains(PointToClient(Cursor.Position));

            using (var br = new SolidBrush(hovered
                ? Color.FromArgb(200, 40, 80, 140)
                : Color.FromArgb(160, 20, 50, 100)))
                g.FillRectangle(br, zone);

            using (var pen = new Pen(hovered
                ? Color.FromArgb(255, 100, 160, 255)
                : Color.FromArgb(180, 51, 102, 200), 2))
                g.DrawRectangle(pen, zone);

            string label = hovered ? "[ ► PASS ]" : "[ PASS ]";
            using (var font = new Font("Consolas", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(220, 160, 210, 255)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(label, font, brush, zone, sf);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  ТУТОРИАЛ
        // ════════════════════════════════════════════════════════════════════
        private readonly string[] _tutorialHints = new[]
        {
            "MONITOR — DOCUMENTS\nCheck name, occupation and access code.\nCompare the code with what the subject tells you.",
            "ECG — CARDIAC MONITOR\nRobots: mechanical pauses between peaks.\nAliens: irregular waveform.\nHumans: normal sinus rhythm.",
            "DOSIMETER — RADIATION\n⚠ Unit offline today — scheduled maintenance.\nReadings unreliable. Do not use today.",
            "STICKERS — NOTES\nAlways contains current hints:\naccess codes, signs of robots and aliens.",
            "STICKERS — ACCESS CODES\nCodes change every day.\nSubject gave an old code? That's suspicious.",
            "СТИКЕРЫ — ПРИЗНАКИ ОПАСНОСТИ\nКраткая шпаргалка — открывай в сложных случаях.",
            "СТИКЕРЫ — ДОПОЛНИТЕЛЬНО\nПредупреждения командования. Читай в начале смены.",
            "ПРАВЫЙ МОНИТОР — ДАННЫЕ СУБЪЕКТА\nИмя, профессия, код доступа — для быстрой сверки.",
            "РАДИО — ВХОДЯЩИЕ СООБЩЕНИЯ\nКомандование, медицина, периметр.\nСлушай — там бывает критически важная информация.",
            "ДОПРОС — ЗАДАТЬ ВОПРОС\nСпрашивай про код, семью, откуда, цель визита.\nВсе ответы сохраняются в логе.",
            "ДИАЛОГОВЫЙ ЭКРАН\nЗдесь печатается то что говорит субъект.\nНажми — откроется полный лог разговора.",
        };

        // ════════════════════════════════════════════════════════════════
        //  ТУТОРИАЛ — нарративный
        // ════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════
        //  ТУТОРИАЛ — карточки поверх игры
        // ════════════════════════════════════════════════════════════════
        private int _tutorialStep = 0;
        private bool _tutorialActive = false;
        private Timer _tutorialTimer;

        // Каждая карточка: (заголовок, описание, иконка зоны -1=нет)
        private static readonly (string Header, string Body, int Zone)[] TutorialCards = {
            ("WELCOME TO GATE 7",
             "This is the last checkpoint before the colony interior.\n\nYour job: classify every subject that approaches.\nLet the right ones through. Stop the wrong ones.\nEvery mistake goes on your record.",
             -1),
            ("LEFT PANEL — BIOMETRIC MONITORS",
             "The three screens on the LEFT are your detection tools:\n\n" +
             "  TOP      →  SUBJECT STATUS  (organic / synthetic readings)\n" +
             "  MIDDLE   →  PULSE MONITOR   (ECG — watch the waveform)\n" +
             "  BOTTOM   →  RADIATION / BIO (contamination scanner)\n\n" +
             "Click each screen to open its full readout.",
             0),
            ("HOW TO READ BIOMETRICS",
             "HUMAN:   normal ECG, body temp 36–37°C, no radiation\n" +
             "ROBOT:   flat ECG or mechanical pulse, temp exactly 32°C\n" +
             "ALIEN:   irregular ECG, temp above 38.5°C, elevated radiation\n\n" +
             "After Day 5 — advanced synthetics mimic humans almost perfectly.\n" +
             "Biometrics alone won't be enough. Use dialogue.",
             1),
            ("THREE VERDICT BUTTONS",
             "The three buttons at the BOTTOM LEFT are your verdict:\n\n" +
             "  RED    →  ROBOT   (synthetic unit detected)\n" +
             "  BLUE   →  ALIEN   (non-human organism)\n" +
             "  GREEN  →  HUMAN   (clear to enter)\n\n" +
             "3 wrong verdicts = shift terminated early.",
             -1),
            ("SUBJECT DOCUMENTS",
             "Click the monitor on the RIGHT (under the stickers)\n" +
             "to open the subject's identity document.\n\n" +
             "Check every field: Name · Origin · Access Code · Occupation · Purpose\n\n" +
             "Compare what they tell you with what's written.\n" +
             "Discrepancies are your main tool.",
             7),
            ("INTERROGATION — SMALL RADIO",
             "Click the SMALL RADIO on the right side\n" +
             "to ask the subject questions.\n\n" +
             "You have 5 questions. Use them wisely.\n\n" +
             "Synthetics pause before answering.\n" +
             "Aliens say 'we' instead of 'I'.\n" +
             "Robots speak in exact, formal language.",
             9),
            ("STICKERS & BIG RADIO",
             "STICKERS (top right corner):\n" +
             "Today's access codes and threat signs.\n" +
             "Read them before every subject. Codes change daily.\n\n" +
             "BIG RADIO (bottom right):\n" +
             "Colony broadcasts — command alerts, medical warnings.\n" +
             "Sometimes critical. Never ignore.",
             8),
            ("DIALOGUE SCREEN",
             "The green text at the BOTTOM shows what the subject just said.\n\n" +
             "Click it anytime to re-read the full conversation log.\n" +
             "Documents passed to you by subjects appear in the Documents tab.\n\n" +
             "You can hand documents to Commander Felicia or Commissar Wolf\n" +
             "— each choice affects the ending.",
             10),
            ("YOU'RE READY",
             "7 days. Each one harder than the last.\n\n" +
             "Subjects will beg. They'll bribe. They'll lie beautifully.\n" +
             "Some will hand you information that changes everything.\n\n" +
             "Trust the equipment. Trust the documents.\n" +
             "And watch your back — someone is watching yours.\n\n" +
             "Good luck, Inspector.",
             -1),
        };

        internal void StartTutorialPhase()
        {
            _tutorialActive = true;
            _tutorialStep = 0;
            currentCharacterData = null;
            currentCharacter = null;
            ShowTutorialCard(_tutorialStep);
            Redraw();
        }

        private void ShowTutorialCard(int step)
        {
            if (step < 0 || step >= TutorialCards.Length) return;
            var card = TutorialCards[step];
            // Используем диалог-строку для отображения подсказки текущей карточки
            StartTypingEffect(card.Header + "\n" + card.Body);
        }

        private void EndTutorialPhase()
        {
            _tutorialActive = false;
            _tutorialTimer?.Stop();
            _tutorialTimer = null;
            LoadCurrentCharacter();
            pressureSeconds = 0;
            pressureTimer.Start();
            Redraw();
        }

        internal void HandleTutorialClick(Point p)
        {
            if (overlayPanel.Visible) { HideOverlay(); return; }

            Rectangle skipZone = ScaleRect(new Rectangle(50, 548, 160, 82));
            Rectangle nextZone = ScaleRect(new Rectangle(220, 548, 170, 82));

            if (skipZone.Contains(p)) { EndTutorialPhase(); return; }
            if (nextZone.Contains(p))
            {
                _tutorialStep++;
                if (_tutorialStep >= TutorialCards.Length) { EndTutorialPhase(); return; }
                ShowTutorialCard(_tutorialStep);
                Redraw();
                return;
            }

            // Клик по highlighted зоне — показываем оверлей
            int safeIdx = Math.Min(_tutorialStep, TutorialCards.Length - 1);
            var (_, _, zoneIdx) = TutorialCards[safeIdx];
            if (zoneIdx >= 0 && zoneIdx < interactiveZones.Length && ScaleRect(interactiveZones[zoneIdx]).Contains(p))
            {
                ShowOverlay(zoneIdx);
                return;
            }

            // Клик в любом месте = следующая карточка
            _tutorialStep++;
            if (_tutorialStep >= TutorialCards.Length) { EndTutorialPhase(); return; }
            ShowTutorialCard(_tutorialStep);
            Redraw();
        }

        internal void UpdateTutorialHover(Point p)
        {
            bool onBtn = ScaleRect(new Rectangle(50, 548, 160, 82)).Contains(p) ||
                         ScaleRect(new Rectangle(220, 548, 170, 82)).Contains(p);
            bool onZone = false;
            int safeStep = Math.Min(_tutorialStep, TutorialCards.Length - 1);
            if (safeStep >= 0)
            {
                var (_, _, zoneIdx) = TutorialCards[safeStep];
                if (zoneIdx >= 0 && zoneIdx < interactiveZones.Length)
                    onZone = ScaleRect(interactiveZones[zoneIdx]).Contains(p);
            }
            Cursor = (onBtn || onZone) ? Cursors.Hand : Cursors.Default;
            // Не вызываем Redraw() здесь — это вызывает перерисовку при каждом движении мыши
        }

        internal void DrawTutorialUI(Graphics g)
        {
            if (!_tutorialActive) return;

            var card = TutorialCards[Math.Min(_tutorialStep, TutorialCards.Length - 1)];
            int cw = ClientSize.Width, ch = ClientSize.Height;

            // Полупрозрачный оверлей
            using (var br = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
                g.FillRectangle(br, 0, 0, cw, ch);

            // Карточка по центру
            int cardW = Math.Min(620, cw - 80);
            int cardH = 380;
            int cardX = (cw - cardW) / 2;
            int cardY = (ch - cardH) / 2 - 30;

            using (var br = new SolidBrush(Color.FromArgb(245, 10, 14, 20)))
                g.FillRectangle(br, cardX, cardY, cardW, cardH);
            using (var pen = new Pen(Color.FromArgb(200, 51, 130, 200), 2))
                g.DrawRectangle(pen, cardX, cardY, cardW, cardH);
            // Верхняя акцентная линия
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(cardX, cardY, cardW, 4),
                Color.FromArgb(220, 51, 140, 220), Color.Transparent,
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                g.FillRectangle(br, cardX, cardY, cardW, 4);

            // Счётчик шагов
            using (var font = new Font("Consolas", 8f))
            using (var br = new SolidBrush(Color.FromArgb(80, 100, 140)))
            {
                string counter = $"{_tutorialStep + 1} / {TutorialCards.Length}";
                g.DrawString(counter, font, br, cardX + cardW - 60, cardY + 10);
            }

            // Заголовок
            using (var font = new Font("Consolas", 14f, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(100, 180, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Near };
                g.DrawString(card.Header, font, br, cardX + 24, cardY + 22, sf);
            }

            // Разделитель
            using (var pen = new Pen(Color.FromArgb(50, 51, 102, 170), 1))
                g.DrawLine(pen, cardX + 24, cardY + 52, cardX + cardW - 24, cardY + 52);

            // Тело карточки
            using (var font = new Font("Consolas", 10f))
            using (var br = new SolidBrush(Color.FromArgb(190, 210, 230)))
            {
                var sf = new StringFormat { LineAlignment = StringAlignment.Near };
                var textRect = new RectangleF(cardX + 24, cardY + 62, cardW - 48, cardH - 90);
                g.DrawString(card.Body, font, br, textRect, sf);
            }

            // Подсветка связанной зоны
            if (card.Zone >= 0 && card.Zone < interactiveZones.Length)
            {
                Rectangle r = ScaleRect(interactiveZones[card.Zone]);
                using (var pen = new Pen(Color.FromArgb(200, 255, 200, 0), 2.5f))
                    g.DrawRectangle(pen, r);
                using (var br = new SolidBrush(Color.FromArgb(30, 255, 200, 0)))
                    g.FillRectangle(br, r);
                // Стрелка от карточки к зоне
                Point cardCenter = new Point(cw / 2, cardY + cardH);
                Point zoneCenter = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
                using (var pen = new Pen(Color.FromArgb(120, 255, 200, 0), 1.5f)
                { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                    g.DrawLine(pen, cardCenter, zoneCenter);
            }

            // Кнопка SKIP
            DrawTutBtn(g, new Rectangle(50, 548, 160, 82),
                "SKIP", Color.FromArgb(60, 30, 30), Color.FromArgb(180, 100, 80));
            // Кнопка NEXT / START
            bool isLast = _tutorialStep >= TutorialCards.Length - 1;
            DrawTutBtn(g, new Rectangle(220, 548, 170, 82),
                isLast ? "START SHIFT" : "NEXT  ▶",
                isLast ? Color.FromArgb(10, 50, 20) : Color.FromArgb(15, 35, 60),
                isLast ? Color.FromArgb(0, 220, 100) : Color.FromArgb(80, 160, 255));

            // Подсказка "click anywhere"
            if (!isLast)
            {
                using (var font = new Font("Consolas", 8f, FontStyle.Italic))
                using (var br = new SolidBrush(Color.FromArgb(60, 100, 140)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString("click anywhere to continue", font, br, cw / 2f, cardY + cardH + 12, sf);
                }
            }
        }

        private void DrawTutBtn(Graphics g, Rectangle baseRect, string label, Color bg, Color fg)
        {
            Rectangle zone = ScaleRect(baseRect);
            bool hovered = zone.Contains(PointToClient(Cursor.Position));
            using (var br = new SolidBrush(Color.FromArgb(hovered ? 220 : 170, bg)))
                g.FillRectangle(br, zone);
            using (var pen = new Pen(Color.FromArgb(hovered ? 255 : 140, fg), hovered ? 2.5f : 1.5f))
                g.DrawRectangle(pen, zone);
            using (var font = new Font("Consolas", 11, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(hovered ? 255 : 190, fg)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(hovered ? $"► {label} ◄" : label, font, brush, zone, sf);
            }
        }

        //  ВСПОМОГАТЕЛЬНЫЕ
        // ════════════════════════════════════════════════════════════════════
        private void TriggerButtonGlow(int buttonIndex)
        {
            pressedButton = buttonIndex;
            glowAlpha = 220;
            glowTimer?.Stop();
            glowTimer?.Start();
        }

        private Image GetButtonImage(int btnIndex)
        {
            switch (btnIndex)
            {
                case 0: return btnRed;
                case 1: return btnBlue;
                case 2: return btnGreen;
                default: return btnDefault;
            }
        }

        // FallbackCharacter нужен если currentCharacter — Image, а не Character
        private class FallbackCharacter : Character
        {
            public FallbackCharacter() : base("Unknown", "", "", false, "", "", 0) { }
        }
    }
}
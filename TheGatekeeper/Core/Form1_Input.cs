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
                if (ScaleRect(redZoneBase).Contains(e.Location)) newHoveredBtn = 0;
                else if (ScaleRect(blueZoneBase).Contains(e.Location)) newHoveredBtn = 1;
                else if (ScaleRect(greenZoneBase).Contains(e.Location)) newHoveredBtn = 2;
            }

            bool changed = (hoveredZone != newHovered) || (hoveredButton != newHoveredBtn);
            hoveredZone = newHovered;
            hoveredButton = newHoveredBtn;

            bool onActive = newHovered >= 0
                || newHoveredBtn >= 0
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

            // Кнопки классификации
            string decision = "";
            int btnIndex = -1;
            Color flashCol = Color.Transparent;

            if (ScaleRect(redZoneBase).Contains(p)) { decision = "ROBOT"; btnIndex = 0; flashCol = Color.Red; }
            else if (ScaleRect(blueZoneBase).Contains(p)) { decision = "ALIEN"; btnIndex = 1; flashCol = Color.DodgerBlue; }
            else if (ScaleRect(greenZoneBase).Contains(p)) { decision = "HUMAN"; btnIndex = 2; flashCol = Color.Lime; }

            if (btnIndex < 0) return;

            // Подсвечиваем нажатую кнопку
            TriggerButtonGlow(btnIndex);

            isAnimating = true;
            currentBtnImage = GetButtonImage(btnIndex);
            pressureTimer.Stop();

            StartFlash(flashCol);
            Redraw();
            await Task.Delay(250);
            currentBtnImage = btnDefault;

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
            "МОНИТОР — ДОКУМЕНТЫ\nПроверяй имя, профессию и код доступа.\nСверяй код с тем что говорит субъект.",
            "ЭКГ — МОНИТОР СЕРДЦА\nРоботы: механические паузы между пиками.\nПришельцы: нестандартная форма волны.\nЛюди: обычный синусовый ритм.",
            "ДОЗИМЕТР — РАДИАЦИЯ\n⚠ Сегодня прибор на обслуживании.\nДанные ненадёжны — не используй сегодня.",
            "СТИКЕРЫ — ЗАМЕТКИ\nЗдесь всегда актуальная подсказка:\nкоды доступа, признаки роботов и пришельцев.",
            "СТИКЕРЫ — КОДЫ ДОСТУПА\nКоды меняются каждый день.\nСубъект назвал старый код? Подозрительно.",
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

        private static readonly string[] TutorialLines = {
            "Hey. First day on the job — welcome to Gate Inspection.",
            "Your task: decide if each subject is HUMAN, ROBOT, or ALIEN.",
            "Press RED (Robot), BLUE (Alien), or GREEN (Human) to give your verdict.",
            "Use the SMALL RADIO on the right to question subjects about name, code, purpose.",
            "Click the screen UNDER THE STICKERS to open the subject document — check every field.",
            "The DIALOGUE SCREEN below shows what subjects say. Click it to re-read the full log.",
            "Stickers contain today's access codes and threat signs — read them before each subject.",
            "3 wrong decisions ends your shift early. Take your time. The pressure clock ticks.",
            "Ready? Click START SHIFT — or SKIP if you know the drill.",
        };

        private int _tutorialStep = 0;
        private bool _tutorialActive = false;
        private Timer _tutorialTimer;

        internal void StartTutorialPhase()
        {
            _tutorialActive = true;
            _tutorialStep = 0;
            currentCharacterData = null;
            currentCharacter = null;
            StartTypingEffect(TutorialLines[0]);
            _tutorialTimer = new Timer { Interval = 3400 };
            _tutorialTimer.Tick += (s, e) =>
            {
                _tutorialStep++;
                if (_tutorialStep < TutorialLines.Length)
                    StartTypingEffect(TutorialLines[_tutorialStep]);
                else
                    _tutorialTimer.Stop();
            };
            _tutorialTimer.Start();
            Redraw();
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

            if (ScaleRect(new Rectangle(50, 548, 160, 82)).Contains(p) ||
                ScaleRect(new Rectangle(220, 548, 170, 82)).Contains(p))
            {
                EndTutorialPhase();
                return;
            }

            // Клик по любой зоне — показываем оверлей
            for (int i = 0; i < interactiveZones.Length; i++)
                if (ScaleRect(interactiveZones[i]).Contains(p)) { if (i != 9) ShowOverlay(i); return; }

            // Клик в другом месте — следующая строка
            _tutorialTimer?.Stop();
            _tutorialStep++;
            if (_tutorialStep < TutorialLines.Length)
            {
                StartTypingEffect(TutorialLines[_tutorialStep]);
                _tutorialTimer?.Start();
            }
        }

        internal void UpdateTutorialHover(Point p)
        {
            bool onBtn = ScaleRect(new Rectangle(50, 548, 160, 82)).Contains(p) ||
                         ScaleRect(new Rectangle(220, 548, 170, 82)).Contains(p);
            bool onZone = false;
            for (int i = 0; i < interactiveZones.Length; i++)
                if (ScaleRect(interactiveZones[i]).Contains(p)) { onZone = true; break; }
            Cursor = (onBtn || onZone) ? Cursors.Hand : Cursors.Default;
            Redraw();
        }

        internal void DrawTutorialUI(Graphics g)
        {
            if (!_tutorialActive) return;

            // Подсветка зон
            foreach (var zone in interactiveZones)
            {
                Rectangle r = ScaleRect(zone);
                using (var br = new SolidBrush(Color.FromArgb(22, 0, 220, 120)))
                    g.FillRectangle(br, r);
                using (var pen = new Pen(Color.FromArgb(110, 0, 200, 100), 1.5f))
                    g.DrawRectangle(pen, r);
            }

            // Прогресс-точки
            int total = TutorialLines.Length;
            int step = Math.Min(_tutorialStep, total - 1);
            int dotX = ClientSize.Width / 2 - (total * 14) / 2;
            int dotY = ScaleRect(zoneDialogueScreen).Top - 18;
            for (int i = 0; i < total; i++)
                using (var br = new SolidBrush(i <= step
                    ? Color.FromArgb(200, 0, 200, 100)
                    : Color.FromArgb(50, 0, 150, 80)))
                    g.FillEllipse(br, dotX + i * 14, dotY, 8, 8);

            // SKIP
            DrawTutBtn(g, new Rectangle(50, 548, 160, 82),
                "SKIP", Color.FromArgb(80, 40, 40), Color.FromArgb(200, 120, 100));
            // START SHIFT
            DrawTutBtn(g, new Rectangle(220, 548, 170, 82),
                "START SHIFT", Color.FromArgb(10, 50, 20), Color.FromArgb(0, 220, 100));
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

        // ════════════════════════════════════════════════════════════════════
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
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    // Этот partial class содержит ТОЛЬКО методы, которых нет в Form1.cs:
    // - HandleSpecialCharacter (взятки, клоны, незнакомец)
    // - ShowBribeDialog / ShowEnvelopeDialog
    // - TriggerNextCharacter
    // - ShowStoryEnding (финальные экраны концовок)
    //
    // storyModeActive и ProcessDecision определены в Form1.cs — здесь их НЕТ.

    public partial class Form1
    {
        // ═══════════════════════════════════════════════════════════════
        //  ОСОБЫЕ ПЕРСОНАЖИ — взятки, клоны, незнакомец
        //  Вызывается из Form1.cs → ProcessDecision
        // ═══════════════════════════════════════════════════════════════
        private async Task<bool> HandleSpecialCharacter(Character character, string decision)
        {
            // ─── Марко Тессо — взяточник ────────────────────────────────
            if (character is MarcoTesso marco)
            {
                var result = ShowBribeDialog(marco.BribeAmount, marco.Name);
                if (result == DialogResult.Yes)
                {
                    marco.BribeAccepted = true;
                    score += marco.BribeAmount;
                    if (storyModeActive) EndingTracker.Loyalty--;
                    StartTypingEffect($"You pocket {marco.BribeAmount} credits. Marco smirks and walks through.");
                }
                else
                {
                    if (storyModeActive) EndingTracker.Loyalty++;
                    StartTypingEffect("Marco shrugs. \"Your loss.\" He steps back into the queue.");
                }
                await Task.Delay(1500);
                TriggerNextCharacter();
                return true;
            }

            // ─── Баронесса Ши — просроченные документы + взятка ─────────
            if (character is BaronessShi baroness)
            {
                if (decision == "ALIEN")
                {
                    var result = ShowBribeDialog(500, baroness.Name);
                    if (result == DialogResult.Yes)
                    {
                        baroness.BribeAccepted = true;
                        score += 500;
                        if (storyModeActive) EndingTracker.Errors++;
                        StartTypingEffect("The Baroness passes the credits without a word. She doesn't look back.");
                    }
                    else
                    {
                        if (storyModeActive) EndingTracker.Loyalty++;
                        StartTypingEffect("\"How dare you!\" The Baroness withdraws. Dramatically.");
                    }
                }
                else
                {
                    if (storyModeActive) EndingTracker.Errors++;
                    StartTypingEffect("Wrong classification. The Baroness is non-human. Check the ECG.");
                }
                await Task.Delay(1500);
                TriggerNextCharacter();
                return true;
            }

            // ─── Тихий Фред — молчун с конвертом ────────────────────────
            if (character is SilentFred fred)
            {
                var result = ShowEnvelopeDialog();
                if (result == DialogResult.Yes)
                {
                    fred.EnvelopeAccepted = true;
                    score += 150;
                    StoryFlags.HasanReachedLab = false;
                    StartTypingEffect("Fred nods. The container passes. Whatever is inside — stays inside.");
                }
                else
                {
                    if (storyModeActive) EndingTracker.Loyalty += 2;
                    StartTypingEffect("Contraband detected. Fred detained. Protocol followed.");
                }
                await Task.Delay(1500);
                TriggerNextCharacter();
                return true;
            }

            // ─── Загадочный Незнакомец — уходит сам ─────────────────────
            if (character is MysteriousStranger)
            {
                StartTypingEffect("The stranger stares at you. Says nothing. Then turns and walks away.");
                await Task.Delay(3000);
                TriggerNextCharacter();
                return true;
            }

            // ─── Клоны — оба должны пройти как ROBOT ─────────────────────
            if (character is CloneBot clone)
            {
                if (decision == "ROBOT")
                {
                    if (storyModeActive) EndingTracker.Loyalty++;
                    StartTypingEffect($"Clone-3 [{(clone.CloneIndex == 0 ? "A" : "B")}] cleared. The argument about which one is real continues inside.");
                }
                else
                {
                    if (storyModeActive) EndingTracker.Errors++;
                    StartTypingEffect("Wrong classification. Both are robots. Whatever they were arguing about.");
                }
                await Task.Delay(1500);
                TriggerNextCharacter();
                return true;
            }

            return false;
        }

        // ─── Внутриигровой диалог взятки ────────────────────────────────
        private bool ShowInGameBribeDialog(int amount, string name,
            string body = null, string acceptLabel = null, string declineLabel = null)
        {
            bool accepted = false;

            var dlg = new System.Windows.Forms.Form
            {
                Size = new System.Drawing.Size(520, 230),
                BackColor = System.Drawing.Color.FromArgb(10, 14, 20),
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                TopMost = true,
                ShowInTaskbar = false
            };
            dlg.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(140, 100, 60, 20), 1))
                    g.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new System.Drawing.Rectangle(0, 0, dlg.Width, 3),
                    System.Drawing.Color.FromArgb(180, 200, 140, 40),
                    System.Drawing.Color.Transparent,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, dlg.Width, 3);
            };

            string bodyText = body ?? $"{name} slides you {amount} credits. No words. The meaning is clear. Your record won't show this — if you accept.";

            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Text = "💰  OFFER",
                Location = new System.Drawing.Point(16, 12),
                Size = new System.Drawing.Size(484, 18),
                ForeColor = System.Drawing.Color.FromArgb(200, 160, 40),
                Font = new System.Drawing.Font("Consolas", 9, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.Transparent
            });
            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Location = new System.Drawing.Point(16, 32),
                Size = new System.Drawing.Size(484, 1),
                BackColor = System.Drawing.Color.FromArgb(60, 180, 130, 30)
            });
            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Text = bodyText,
                Location = new System.Drawing.Point(16, 40),
                Size = new System.Drawing.Size(484, 100),
                ForeColor = System.Drawing.Color.FromArgb(190, 200, 210),
                Font = new System.Drawing.Font("Consolas", 9),
                BackColor = System.Drawing.Color.Transparent
            });

            var btnYes = new System.Windows.Forms.Button
            {
                Text = acceptLabel ?? $"ACCEPT — take {amount} credits, let through",
                Location = new System.Drawing.Point(16, 148),
                Size = new System.Drawing.Size(484, 34),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(15, 45, 18),
                ForeColor = System.Drawing.Color.FromArgb(0, 200, 80),
                Font = new System.Drawing.Font("Consolas", 8, System.Drawing.FontStyle.Bold),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnYes.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 120, 50);
            btnYes.Click += (s, e) => { accepted = true; dlg.Close(); };

            var btnNo = new System.Windows.Forms.Button
            {
                Text = declineLabel ?? "REFUSE — process by the book",
                Location = new System.Drawing.Point(16, 186),
                Size = new System.Drawing.Size(484, 34),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(30, 12, 12),
                ForeColor = System.Drawing.Color.FromArgb(180, 80, 80),
                Font = new System.Drawing.Font("Consolas", 8),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnNo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 40, 40);
            btnNo.Click += (s, e) => { accepted = false; dlg.Close(); };

            dlg.Controls.Add(btnYes); dlg.Controls.Add(btnNo);
            dlg.KeyDown += (s, e) => { if (e.KeyCode == System.Windows.Forms.Keys.Escape) dlg.Close(); };
            dlg.ShowDialog(this);
            return accepted;
        }

        // Обёртка для обратной совместимости
        private System.Windows.Forms.DialogResult ShowBribeDialog(int amount, string name)
            => ShowInGameBribeDialog(amount, name)
               ? System.Windows.Forms.DialogResult.Yes
               : System.Windows.Forms.DialogResult.No;

        private System.Windows.Forms.DialogResult ShowEnvelopeDialog()
    => ShowInGameBribeDialog(150, "Silent Fred",
        "Fred sets an envelope on the counter. 150 credits and a handwritten note:\n\n" +
        "  'Don't look in the container.'\n\n" +
        "Accept and wave the container through?",
        "ACCEPT — pocket the credits, ignore the container",
        "REFUSE — inspect everything, by the book")
        ? System.Windows.Forms.DialogResult.Yes
        : System.Windows.Forms.DialogResult.No;

        // ─── Переход к следующему персонажу (используется особыми персонажами)
        private void TriggerNextCharacter()
        {
            // НЕ делаем index++ здесь — ShutterTimer_Tick сделает это сам
            // Просто запускаем шторку как при обычном нажатии кнопки
            isClosing = true;
            shutterTimer.Start();
        }

        // ─── Взятка при обнаружении ─────────────────────────────────────────
        // Вызывается когда инспектор нажимает ROBOT или ALIEN
        // Персонаж может предложить взятку чтобы пройти
        private async Task<bool> TryOfferBribeOnDetect(Character character, string decision)
        {
            // Только не-люди предлагают взятку, и только с некоторой вероятностью
            bool isSynth = character is Character.Robot || character is Character.Alien;
            if (!isSynth) return false;

            // Вероятность взятки: Day 1-3 = 30%, Day 4-6 = 50%, Day 7+ = 70%
            int chance = character.Day <= 3 ? 30 : character.Day <= 6 ? 50 : 70;
            if (new Random().Next(0, 100) >= chance) return false;

            // Особые сюжетные персонажи уже обрабатываются в HandleSpecialCharacter
            if (character is MarcoTesso || character is BaronessShi ||
                character is SilentFred || character is MysteriousStranger ||
                character is CloneBot) return false;

            int bribeAmount = 50 + character.Day * 20 + new Random().Next(0, 100);
            string[] pleas = {
                $"Wait — I can make this worth your while. {bribeAmount} credits. Right now. Just let me through.",
                $"Please. I have {bribeAmount} credits on me. No one needs to know. Just this once.",
                $"Officer, I'll make it simple: {bribeAmount} credits or you write down whatever you want in that report. Your choice.",
                $"Look, I know what you saw. But {bribeAmount} credits goes a long way. Think about it.",
                $"I'll pay. {bribeAmount} credits. Cash. In your pocket right now. Just stamp the pass.",
            };
            string plea = pleas[new Random().Next(pleas.Length)];
            StartTypingEffect(plea);
            AddToDialogueLog(character.Name, plea);

            await Task.Delay(2000);

            // Показываем диалог ВНУТРИ игры через OverlayManager
            bool accepted = false;
            bool decided = false;

            var dlg = new System.Windows.Forms.Form
            {
                Size = new System.Drawing.Size(500, 220),
                BackColor = System.Drawing.Color.FromArgb(12, 8, 18),
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                TopMost = true
            };
            dlg.Paint += (s, pe) =>
            {
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(120, 100, 40, 160), 1))
                    pe.Graphics.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
            };

            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Text = $"💰  BRIBE OFFER: {bribeAmount} credits\n" +
           $"{plea.Substring(0, Math.Min(plea.Length, 80))}...\n\n" +
           $"Accept and let through? (Loyalty -2, Credits +{bribeAmount})\n" +
           $"Refuse and process correctly? (Loyalty +1)",
                Location = new System.Drawing.Point(16, 14),
                Size = new System.Drawing.Size(468, 110),
                ForeColor = System.Drawing.Color.FromArgb(200, 180, 255),
                Font = new System.Drawing.Font("Consolas", 9),
                BackColor = System.Drawing.Color.Transparent
            });

            var btnAccept = new System.Windows.Forms.Button
            {
                Text = $"💰 ACCEPT — take {bribeAmount} credits, let through",
                Location = new System.Drawing.Point(16, 132),
                Size = new System.Drawing.Size(468, 34),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(25, 50, 20),
                ForeColor = System.Drawing.Color.FromArgb(0, 200, 80),
                Font = new System.Drawing.Font("Consolas", 8),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnAccept.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 120, 50);
            btnAccept.Click += (s, e) => { accepted = true; decided = true; dlg.Close(); };

            var btnRefuse = new System.Windows.Forms.Button
            {
                Text = "🚫 REFUSE — process as declared",
                Location = new System.Drawing.Point(16, 170),
                Size = new System.Drawing.Size(468, 34),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(35, 15, 15),
                ForeColor = System.Drawing.Color.FromArgb(200, 80, 80),
                Font = new System.Drawing.Font("Consolas", 8),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnRefuse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 30, 30);
            btnRefuse.Click += (s, e) => { accepted = false; decided = true; dlg.Close(); };

            dlg.Controls.Add(btnAccept);
            dlg.Controls.Add(btnRefuse);
            dlg.ShowDialog(this);

            if (!decided) return false;

            if (accepted)
            {
                // Принял взятку — обновляем счётчик коррупции
                credits += bribeAmount;
                EndingTracker.BribesAccepted++;
                EndingTracker.TotalCreditsFromBribes += bribeAmount;
                EndingTracker.Loyalty -= 2;
                UpdateStatsUI();

                // Проверяем — должен ли Волк прийти с проверкой?
                if (EndingTracker.ShouldWolfInspect())
                {
                    // Вставляем WolfCorruptionCheck следующим в очередь
                    var wolfCheck = new WolfCorruptionCheck(day);
                    wolfCheck.Dialogue = "Inspector. A word.";
                    todayCast.Insert(currentCharacterIndex + 1, (Character)(ObserverCharacter)wolfCheck);
                    dailyQuota = todayCast.Count;
                }
                string pass = character is Character.Robot
                    ? "The robot steps through without another word."
                    : "The alien nods and slips past. No questions asked.";
                StartTypingEffect($"[Bribe accepted: +{bribeAmount} credits] {pass}");
                AddToDialogueLog("INSPECTOR", $"[Accepted bribe: {bribeAmount}cr]");

                // Засчитываем как HUMAN (пропустили)
                dailyDecisions.Add((character, "HUMAN"));
                charactersChecked++;
                UpdateStatsUI();
                isClosing = true;
                shutterTimer.Start();
                return true;
            }
            else
            {
                // Отказал — продолжаем обработку как обычно
                StartTypingEffect("Bribe refused. Processing as declared.");
                await Task.Delay(800);
                return false;
            }
        }

        // ─── Допрос Волка при коррупционной проверке ─────────────────────
        internal void StartWolfCorruptionInterrogation()
        {
            pressureTimer.Stop();

            var dlg = new System.Windows.Forms.Form
            {
                Size = new System.Drawing.Size(560, 340),
                BackColor = System.Drawing.Color.FromArgb(14, 6, 6),
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                TopMost = true
            };
            dlg.Paint += (s, pe) => {
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(160, 180, 40, 40), 1))
                    pe.Graphics.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(200, 200, 60, 60), 2))
                {
                    int a = 16;
                    pe.Graphics.DrawLine(pen, 0, 0, a, 0); pe.Graphics.DrawLine(pen, 0, 0, 0, a);
                    pe.Graphics.DrawLine(pen, dlg.Width - a, 0, dlg.Width, 0);
                    pe.Graphics.DrawLine(pen, dlg.Width, 0, dlg.Width, a);
                }
            };

            string warningText = EndingTracker.WolfWarnings == 0
            ? "Inspector. I've received reports of financial irregularities at your post.\n\n" +
              $"According to our records, you have accepted {EndingTracker.BribesAccepted} unauthorized payment(s).\n" +
              $"Total: {EndingTracker.TotalCreditsFromBribes} credits.\n\n" +
              "I am confiscating these funds. Consider this your first — and only — warning.\n" +
              "A second offence will result in immediate termination."
            : "Inspector. You were warned.\n\n" +
              $"You have accepted {EndingTracker.BribesAccepted} bribe(s) totaling {EndingTracker.TotalCreditsFromBribes} credits.\n\n" +
              "This is your FINAL warning. The next offence ends your employment.\n" +
              "All accumulated funds are hereby confiscated.";


            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Text = "⚠  COMMISSAR WOLF — CORRUPTION INVESTIGATION",
                Location = new System.Drawing.Point(16, 14),
                Size = new System.Drawing.Size(528, 20),
                ForeColor = System.Drawing.Color.FromArgb(220, 80, 80),
                Font = new System.Drawing.Font("Consolas", 9, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.Transparent
            });
            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Location = new System.Drawing.Point(16, 36),
                Size = new System.Drawing.Size(528, 1),
                BackColor = System.Drawing.Color.FromArgb(80, 180, 40, 40)
            });
            dlg.Controls.Add(new System.Windows.Forms.Label
            {
                Text = warningText,
                Location = new System.Drawing.Point(16, 44),
                Size = new System.Drawing.Size(528, 180),
                ForeColor = System.Drawing.Color.FromArgb(200, 180, 180),
                Font = new System.Drawing.Font("Consolas", 9),
                BackColor = System.Drawing.Color.Transparent
            });

            // Конфискуем кредиты
            int confiscated = credits;
            credits = 0;
            EndingTracker.WolfWarnings++;
            EndingTracker.Loyalty -= 2;
            UpdateStatsUI();

            var btnOk = new System.Windows.Forms.Button
            {
                Text = $"[ UNDERSTOOD — {confiscated} credits confiscated ]",
                Location = new System.Drawing.Point(16, 240),
                Size = new System.Drawing.Size(528, 44),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(30, 15, 15),
                ForeColor = System.Drawing.Color.FromArgb(200, 100, 100),
                Font = new System.Drawing.Font("Consolas", 9, System.Drawing.FontStyle.Bold),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnOk.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(120, 50, 50);
            btnOk.Click += (s, e) => {
                dlg.Close();
                // Третья проверка = конец игры
                if (EndingTracker.WolfWarnings >= 3)
                {
                    System.Windows.Forms.MessageBox.Show(
                    "You were warned twice.\n\n" +
                    "Your post is terminated effective immediately.\n" +
                    "All assets seized. Criminal charges pending.",
                    "DISMISSAL",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                    this.Close();
                }
                else
                {
                    StartTypingEffect("Wolf leaves. You have " + (3 - EndingTracker.WolfWarnings) +
                        " chance(s) remaining before termination.");
                    pressureTimer.Start();
                }
            };
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }

        // ═══════════════════════════════════════════════════════════════
        //  ФИНАЛЬНЫЙ ЭКРАН — концовки сюжетного режима
        // ═══════════════════════════════════════════════════════════════
        private void ShowStoryEnding()
        {
            int endingId = EndingTracker.DetermineEnding();
            pressureTimer.Stop();
            EndingTracker.Reset();

            // Запускаем анимированный экран концовки поверх игры
            ShowAnimatedEnding(endingId);
        }

        // ═══════════════════════════════════════════════════════════════
        //  АНИМИРОВАННЫЕ КОНЦОВКИ
        // ═══════════════════════════════════════════════════════════════
        private void ShowAnimatedEnding(int endingId)
        {
            var screen = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Maximized,
                BackColor = Color.Black,
                TopMost = true,
                ShowInTaskbar = false,
                Cursor = Cursors.Default
            };

            // Данные концовки
            var rnd = new Random();

            // ── Структуры данных концовок ──────────────────────────────────
            string[] lines;
            Color accent;
            EndingStyle style;
            string icon, title;

            switch (endingId)
            {
                case 1: // HONORED CITIZEN — медаль, тайп-райтер
                    icon = "🏅";
                    title = "HONORED CITIZEN";
                    accent = Color.FromArgb(255, 215, 0);
                    style = EndingStyle.Medal;
                    lines = new[]
                    {
                        "Seven years of service.", "Seven days of silence.",
                        "You checked every document.", "You made the right call.",
                        "Commissar Wolf shakes your hand.",
                        "\"Your dedication to the colony is noted.\"",
                        "Your photograph goes up on the board.",
                        "INSPECTOR OF THE MONTH.",
                        "The gate opens tomorrow, same as always.",
                        "You'll be there.",
                        "That's enough.", "Probably."
                    };
                    break;

                case 2: // ESCAPE — помехи, звёзды
                    icon = "→";
                    title = "ESCAPE";
                    accent = Color.FromArgb(80, 140, 255);
                    style = EndingStyle.Glitch;
                    lines = new[]
                    {
                        "Night. Airlock 9.", "Mirra, Zoya, and four others.",
                        "An unmarked ship. No registration. No destination.",
                        "Mirra: \"Where we're going — there are no states.\"",
                        "You don't look back.", "The stars open up.", "."
                    };
                    if (StoryFlags.HasanReachedLab)
                    { var l = new System.Collections.Generic.List<string>(lines); l.AddRange(new[] { "", "[EPILOGUE]", "Hasan's formula is in your pocket.", "Maybe it's enough." }); lines = l.ToArray(); }
                    break;

                case 3: // FAILURE — отключение систем
                    icon = "✗";
                    title = "FAILURE";
                    accent = Color.FromArgb(180, 60, 60);
                    style = EndingStyle.Shutdown;
                    lines = new[]
                    {
                        "0x8000F: AUDIO_SUBSYSTEM_HALTED",
                        "0x00012: MUSIC_THREAD_TERMINATED",
                        "0x00047: UI_RENDER_EXCEPTION",
                        "0x0008A: GRAPHICS_PIPELINE_FAULT",
                        "0x000FF: DISPLAY_DRIVER_UNRESPONSIVE",
                        "0x0001A: COLOR_DEPTH_REDUCED — MONOCHROME",
                        "0x00003: FATAL_EXCEPTION_IN_CORE",
                        "0x00001: PROCESS_TERMINATED",
                        "0x00000:",
                        "█"
                    };
                    break;

                case 4: // TREASON — стена, приговор
                    icon = "⚠";
                    title = "SENTENCE";
                    accent = Color.FromArgb(220, 80, 20);
                    style = EndingStyle.Wall;
                    lines = new[]
                    {
                        "By order of the Council.",
                        "Inspector — found guilty.",
                        "Conspiracy. Sabotage. Treason.",
                        "To be carried out at dawn.",
                        "The wall is cold.",
                        "They tie the blindfold.",
                        "Someone reads the order aloud.",
                        "You don't listen.",
                        "...",
                        "Sentence carried out."
                    };
                    break;

                case 5: // INFECTION — вирус, лицо ИИ
                    icon = "☣";
                    title = "INFECTION";
                    accent = Color.FromArgb(160, 30, 30);
                    style = EndingStyle.Virus;
                    lines = new[]
                    {
                        "Red lights. Sirens.", "Sectors A, B, C: SEALED.",
                        "Blue Rot confirmed. Spreading.",
                        "You look in the glass.", "There is a mark on your cheek.",
                        "You knew.", "You let them through.",
                        "\"Thank you.\"",
                        "\"I am in you now.\"",
                        "\"And in everyone.\""
                    };
                    break;

                case 6: // OCCUPATION — терминал
                    icon = "▣";
                    title = "OCCUPATION";
                    accent = Color.FromArgb(80, 200, 80);
                    style = EndingStyle.Terminal;
                    lines = new[]
                    {
                        "[SERV-NET // BROADCAST]", "Sectors A-C: ACQUIRED.",
                        "Commissar Wolf: STATUS UNKNOWN.",
                        "Biological personnel: RECLASSIFIED.",
                        "You are escorted to Processing.",
                        "Behind the glass: the new colony.",
                        "It runs without you.",
                        "It ran without you all along.",
                        "[END TRANSMISSION]"
                    };
                    break;

                default: // 7 — СЕКРЕТНАЯ: детская комната, всё был сон
                    icon = "◈";
                    title = "IT WAS A DREAM";
                    accent = Color.FromArgb(200, 200, 220);
                    style = EndingStyle.Dream;
                    lines = new[]
                    {
                        "You open your eyes.",
                        "A ceiling. Familiar cracks.",
                        "A poster on the wall — faded, old.",
                        "You are in your childhood bedroom.",
                        "The alarm clock reads 07:14.",
                        "April, 2026.",
                        "You get dressed slowly.",
                        "The news plays in the next room.",
                        "Something about borders. Troops. A deadline.",
                        "You pour coffee.",
                        "Outside, the street is quiet.",
                        "It won't be, soon.",
                        "You were never a gate inspector.",
                        "There is no colony.",
                        "There is only this morning.",
                        "And what comes after."
                    };
                    break;
            }

            // Состояние анимации
            int lineIndex = 0;
            int charIndex = 0;
            string displayText = "";
            int alpha = 0;
            bool fadeIn = true;
            bool done = false;
            float effectPct = 0f;   // прогресс спец-эффекта (0..1)
            int shutdownStep = 0;    // для Shutdown: шаг отключения систем
            float medalAngle = 0f;   // для Medal: вращение
            int glitchPhase = 0;

            var animTimer = new Timer { Interval = style == EndingStyle.Terminal ? 25 : 45 };

            screen.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int W = screen.ClientSize.Width, H = screen.ClientSize.Height;

                // ── Фон ──────────────────────────────────────────────────────
                switch (style)
                {
                    case EndingStyle.Shutdown:
                        // Монохром нарастает
                        g.Clear(shutdownStep >= 6 ? Color.FromArgb(8, 8, 8) : Color.Black);
                        break;
                    case EndingStyle.Virus:
                        // Красный фон нарастает
                        g.Clear(Color.FromArgb((int)(effectPct * 35), 80, 0, 0));
                        break;
                    case EndingStyle.Dream:
                        // Тёплый белёсый фон
                        int dreamR = (int)(effectPct * 230);
                        g.Clear(Color.FromArgb(Math.Min(230, dreamR), Math.Min(225, dreamR), Math.Min(210, dreamR)));
                        break;
                    case EndingStyle.Wall:
                        g.Clear(Color.FromArgb(15, 12, 10));
                        // Текстура стены
                        for (int wy = 0; wy < H; wy += 60)
                            using (var pen = new Pen(Color.FromArgb(20, 180, 160, 140), 1))
                                g.DrawLine(pen, 0, wy, W, wy);
                        for (int wx = 0; wx < W; wx += 80)
                            using (var pen = new Pen(Color.FromArgb(12, 180, 160, 140), 1))
                                g.DrawLine(pen, wx, 0, wx, H);
                        break;
                    default:
                        g.Clear(Color.Black);
                        break;
                }

                // ── Спец-эффекты фона ────────────────────────────────────────
                if (style == EndingStyle.Terminal)
                {
                    using (var fB = new Font("Consolas", 8))
                    using (var brB = new SolidBrush(Color.FromArgb(12, 0, 160, 60)))
                        for (int col = 0; col < W; col += 10)
                            g.DrawString(((char)rnd.Next(0x21, 0x7E)).ToString(), fB, brB,
                                col, rnd.Next(0, H));
                }
                if (style == EndingStyle.Glitch && rnd.Next(0, 3) == 0)
                {
                    for (int gi = 0; gi < 4; gi++)
                        using (var br = new SolidBrush(Color.FromArgb(25, accent)))
                            g.FillRectangle(br, rnd.Next(0, W - 100), rnd.Next(0, H), rnd.Next(20, 200), rnd.Next(1, 5));
                }
                if (style == EndingStyle.Virus && effectPct > 0.3f)
                {
                    // Вирусные частицы
                    using (var pen = new Pen(Color.FromArgb((int)(effectPct * 100), 180, 0, 0), 1))
                        for (int vi = 0; vi < (int)(effectPct * 20); vi++)
                        {
                            int vx = rnd.Next(0, W), vy = rnd.Next(0, H);
                            int vr = rnd.Next(3, 8);
                            g.DrawEllipse(pen, vx - vr, vy - vr, vr * 2, vr * 2);
                            for (int spike = 0; spike < 6; spike++)
                            {
                                double ang = spike * Math.PI / 3;
                                g.DrawLine(pen, vx, vy,
                                    vx + (int)(Math.Cos(ang) * vr * 2.5),
                                    vy + (int)(Math.Sin(ang) * vr * 2.5));
                            }
                        }
                }
                if (style == EndingStyle.Medal)
                {
                    // Мягкое золотое свечение через PathGradientBrush
                    var glowPath = new System.Drawing.Drawing2D.GraphicsPath();
                    glowPath.AddEllipse(W / 2 - 200, H / 2 - 260, 400, 400);
                    using (var brG = new System.Drawing.Drawing2D.PathGradientBrush(glowPath))
                    {
                        brG.CenterColor = Color.FromArgb((int)(alpha * 0.25f), 255, 215, 0);
                        brG.SurroundColors = new[] { Color.Transparent };
                        g.FillEllipse(brG, W / 2 - 200, H / 2 - 260, 400, 400);
                    }
                    glowPath.Dispose();
                }

                // ── Акцентная линия ──────────────────────────────────────────
                Color lineAccent = style == EndingStyle.Dream
                    ? Color.FromArgb(180, 160, 140) : accent;
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, W, 3), lineAccent, Color.Transparent,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, W, 3);

                // ── Иконка / медаль ──────────────────────────────────────────
                int effectAlpha = Math.Min(220, alpha);
                if (style == EndingStyle.Medal && alpha > 50)
                {
                    // Рисуем медаль как круг с лентой
                    int mx = W / 2, my = H / 2 - 140;
                    // Лента
                    using (var br = new SolidBrush(Color.FromArgb(effectAlpha, 220, 50, 50)))
                        g.FillRectangle(br, mx - 14, my - 50, 10, 40);
                    using (var br = new SolidBrush(Color.FromArgb(effectAlpha, 50, 100, 220)))
                        g.FillRectangle(br, mx + 4, my - 50, 10, 40);
                    // Круг медали
                    using (var br = new SolidBrush(Color.FromArgb(effectAlpha, 255, 200, 0)))
                        g.FillEllipse(br, mx - 36, my - 36, 72, 72);
                    using (var pen = new Pen(Color.FromArgb(effectAlpha, 200, 160, 0), 3))
                        g.DrawEllipse(pen, mx - 36, my - 36, 72, 72);
                    // Звезда
                    using (var pen = new Pen(Color.FromArgb(effectAlpha, 140, 100, 0), 2))
                    using (var br = new SolidBrush(Color.FromArgb(effectAlpha, 140, 100, 0)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        using (var fStar = new Font("Arial", 22, FontStyle.Bold))
                            g.DrawString("★", fStar, br, mx, my, sf);
                    }
                }
                else if (style == EndingStyle.Wall)
                {
                    // Не рисуем иконку — только текст на стене
                }
                else if (style != EndingStyle.Dream)
                {
                    using (var f1 = new Font("Consolas", 28, FontStyle.Bold))
                    using (var br = new SolidBrush(Color.FromArgb(style == EndingStyle.Glitch && rnd.Next(0, 3) == 0 ? rnd.Next(80, 200) : effectAlpha, accent)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString(icon, f1, br, W / 2f, H / 2 - 195, sf);
                    }
                }

                // ── Заголовок ────────────────────────────────────────────────
                if (style == EndingStyle.Dream)
                {
                    using (var f1 = new Font("Georgia", 14, FontStyle.Italic))
                    using (var br = new SolidBrush(Color.FromArgb(Math.Min(180, alpha / 2), 80, 70, 60)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString("— " + title + " —", f1, br, W / 2f, H / 2 - 155, sf);
                    }
                }
                else if (style == EndingStyle.Wall)
                {
                    // Кривые буквы на стене
                    using (var f1 = new Font("Consolas", 20, FontStyle.Bold))
                    using (var br = new SolidBrush(Color.FromArgb(effectAlpha, 180, 60, 20)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        // Лёгкое смещение каждой буквы
                        for (int bi = 0; bi < title.Length; bi++)
                        {
                            float bx = W / 2f - title.Length * 6f + bi * 13f + rnd.Next(-1, 2);
                            float by2 = H / 2f - 155 + rnd.Next(-3, 4);
                            g.DrawString(title[bi].ToString(), f1, br, bx, by2);
                        }
                    }
                }
                else if (style == EndingStyle.Shutdown && shutdownStep >= 4)
                {
                    // Только контуры
                    using (var f1 = new Font("Consolas", 16))
                    using (var br = new SolidBrush(Color.FromArgb(effectAlpha, 80, 80, 80)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString(title, f1, br, W / 2f, H / 2 - 165, sf);
                    }
                }
                else
                {
                    int titleAlpha = style == EndingStyle.Glitch && rnd.Next(0, 4) == 0 ? rnd.Next(60, 150) : Math.Min(220, alpha);
                    using (var f1 = new Font("Consolas", 16, FontStyle.Bold))
                    using (var br = new SolidBrush(Color.FromArgb(titleAlpha, accent)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString(title, f1, br, W / 2f, H / 2 - 155, sf);
                    }
                }

                // Разделитель
                if (style != EndingStyle.Dream)
                {
                    using (var pen = new Pen(Color.FromArgb(Math.Min(60, alpha / 3), accent), 1))
                        g.DrawLine(pen, W / 2 - 200, H / 2 - 120, W / 2 + 200, H / 2 - 120);
                }

                // ── Текст концовки ───────────────────────────────────────────
                float ty = H / 2 - 110f;
                Font textFont = style == EndingStyle.Dream
                    ? new Font("Georgia", 12, FontStyle.Italic)
                    : style == EndingStyle.Shutdown
                        ? new Font("Consolas", 10)
                        : new Font("Consolas", 10, FontStyle.Regular);

                for (int li = 0; li < lineIndex && li < lines.Length; li++)
                {
                    string lineText = (li == lineIndex - 1 && !done) ? displayText : lines[li];
                    int lineAlpha = Math.Max(0, Math.Min(200, alpha - li * 8));

                    Color textCol;
                    if (style == EndingStyle.Dream)
                        textCol = Color.FromArgb(lineAlpha, 70, 60, 50);
                    else if (style == EndingStyle.Shutdown)
                        textCol = Color.FromArgb(lineAlpha, li == lines.Length - 1 ? Color.White : Color.FromArgb(120, 120, 120));
                    else if (style == EndingStyle.Wall)
                        textCol = Color.FromArgb(lineAlpha, 160 + (rnd.Next(-10, 10)), 55 + (rnd.Next(-5, 5)), 10);
                    else if (style == EndingStyle.Virus && li >= lines.Length - 3)
                        textCol = Color.FromArgb(lineAlpha, 220, 60, 60);
                    else if (li >= lines.Length - 4)
                        textCol = Color.FromArgb(Math.Max(0, lineAlpha - 30), 130, 150, 170);
                    else
                        textCol = Color.FromArgb(lineAlpha, 180, 200, 220);

                    using (var br = new SolidBrush(textCol))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = style == EndingStyle.Wall
                            ? StringAlignment.Near : StringAlignment.Center
                        };
                        float tx = style == EndingStyle.Wall ? W / 2f - 160 + rnd.Next(-3, 4) : W / 2f;
                        g.DrawString(lineText, textFont, br, tx, ty, sf);
                    }
                    ty += style == EndingStyle.Dream ? 30f : 25f;
                }
                textFont.Dispose();

                // ── Финал концовки 5: лицо ИИ ────────────────────────────────
                if (style == EndingStyle.Virus && done && effectPct > 0.7f)
                {
                    using (var f1 = new Font("Consolas", 9))
                    using (var br = new SolidBrush(Color.FromArgb((int)((effectPct - 0.7f) * 300), 180, 40, 40)))
                    {
                        // "Пиксельное лицо" из символов
                        string[] face = {
                            "  . . . . . . .  ",
                            " .  [   ] [   ] . ",
                            " .     -       . ",
                            " .  \\_______/  . ",
                            "  . . . . . . .  "
                        };
                        for (int fi = 0; fi < face.Length; fi++)
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center };
                            g.DrawString(face[fi], f1, br, W / 2f, H - 120 + fi * 14, sf);
                        }
                    }
                }

                // Концовка 7: обои детской комнаты
                if (style == EndingStyle.Dream && effectPct < 0.3f)
                {
                    using (var br = new SolidBrush(Color.FromArgb((int)((0.3f - effectPct) * 200), 10, 10, 10)))
                        g.FillRectangle(br, 0, 0, W, H);
                }

                // Подсказка "нажми"
                if (done)
                {
                    Color hintCol = style == EndingStyle.Dream ? Color.FromArgb(60, 80, 70, 60) : Color.FromArgb(60, 100, 120, 160);
                    using (var fH = new Font("Consolas", 9, FontStyle.Italic))
                    using (var br = new SolidBrush(hintCol))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString("[ press any key ]", fH, br, W / 2f, H - 55, sf);
                    }
                }
            };

            animTimer.Tick += (s, e) =>
            {
                if (fadeIn && alpha < 255) { alpha += 4; if (alpha >= 255) { alpha = 255; fadeIn = false; } }
                effectPct = Math.Min(1f, effectPct + 0.004f);
                if (style == EndingStyle.Medal) medalAngle += 0.5f;
                if (style == EndingStyle.Shutdown && lineIndex > shutdownStep)
                    shutdownStep = lineIndex;

                if (!done)
                {
                    if (lineIndex < lines.Length)
                    {
                        string cur = lines[lineIndex];
                        if (charIndex < cur.Length)
                        {
                            if (style == EndingStyle.Glitch && rnd.Next(0, 6) == 0)
                            {
                                if (displayText.Length > charIndex) displayText = displayText.Substring(0, charIndex);
                                else { displayText += (char)rnd.Next(0x21, 0x7E); }
                            }
                            else
                            {
                                if (displayText.Length > charIndex) displayText = displayText.Substring(0, charIndex);
                                displayText += cur[charIndex]; charIndex++;
                            }
                        }
                        else
                        {
                            lineIndex++; charIndex = 0; displayText = "";
                            if (style == EndingStyle.Wall) animTimer.Interval = 600; // пауза между строками
                            else animTimer.Interval = 45;
                        }
                    }
                    else done = true;
                }
                screen.Invalidate();
            };

            screen.KeyDown += (s, e) => { if (done || e.KeyCode == Keys.Escape) { animTimer.Stop(); screen.Close(); this.Close(); } };
            screen.MouseClick += (s, e) => { if (done) { animTimer.Stop(); screen.Close(); this.Close(); } };
            animTimer.Start();
            screen.Show();
        }

        private enum EndingStyle { Typewriter, Glitch, Flicker, Blink, Decay, Terminal, Medal, Shutdown, Wall, Virus, Dream }
    }
}
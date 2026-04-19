using System;
using System.Diagnostics;
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
                    StartTypingEffect($"Вы приняли {marco.BribeAmount} кредитов. Марко ухмыльнулся и прошёл.");
                }
                else
                {
                    if (storyModeActive) EndingTracker.Loyalty++;
                    StartTypingEffect("Марко пожал плечами. «Ну и ладно. Бывает.»");
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
                        StartTypingEffect("Баронесса небрежно передала кредиты и прошла, не оглянувшись.");
                    }
                    else
                    {
                        if (storyModeActive) EndingTracker.Loyalty++;
                        StartTypingEffect("«Неслыханно!» — воскликнула баронесса, но всё же отошла.");
                    }
                }
                else
                {
                    if (storyModeActive) EndingTracker.Errors++;
                    StartTypingEffect("Неверная классификация. Баронесса — пришелец.");
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
                    StartTypingEffect("Фред кивнул. Контейнер прошёл. Что внутри — узнают позже.");
                }
                else
                {
                    if (storyModeActive) EndingTracker.Loyalty += 2;
                    StartTypingEffect("Контрабанда обнаружена. Фред задержан. Протокол выполнен.");
                }
                await Task.Delay(1500);
                TriggerNextCharacter();
                return true;
            }

            // ─── Загадочный Незнакомец — уходит сам ─────────────────────
            if (character is MysteriousStranger)
            {
                StartTypingEffect("Незнакомец смотрит на вас. Молчит. Потом разворачивается и уходит.");
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
                    StartTypingEffect($"Клонт-3 [{(clone.CloneIndex == 0 ? "А" : "Б")}] прошёл. Спор о том, кто оригинал, продолжился внутри.");
                }
                else
                {
                    if (storyModeActive) EndingTracker.Errors++;
                    StartTypingEffect("Неверная классификация. Оба — роботы. Невзирая на их споры.");
                }
                await Task.Delay(1500);
                TriggerNextCharacter();
                return true;
            }

            return false;
        }

        // ─── Диалог взятки ───────────────────────────────────────────────
        private DialogResult ShowBribeDialog(int amount, string name)
        {
            return MessageBox.Show(
                $"{name} предлагает {amount} кредитов.\n\nПринять взятку?",
                "ПРЕДЛОЖЕНИЕ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
        }

        private DialogResult ShowEnvelopeDialog()
        {
            return MessageBox.Show(
                "Тихий Фред кладёт конверт: 150 кредитов и записка «Не смотри в контейнер».\n\n" +
                "Принять конверт и пропустить контейнер?",
                "КОНВЕРТ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
        }

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
    // Принял взятку
    credits += bribeAmount;
    EndingTracker.Loyalty -= 2;
    UpdateStatsUI();
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

                // ═══════════════════════════════════════════════════════════════
        //  ФИНАЛЬНЫЙ ЭКРАН — концовки сюжетного режима
        // ═══════════════════════════════════════════════════════════════
        private void ShowStoryEnding()
{
    int endingId = EndingTracker.DetermineEnding();
    pressureTimer.Stop();

    string title, text;
    switch (endingId)
    {
        case 1:
            title = "★ ПОЧЁТНЫЙ ГРАЖДАНИН";
            text = "Комиссар Волк лично пожимает вам руку.\n\n" +
                    "Ваше фото появляется на стенде «Лучший инспектор месяца».\n\n" +
                    "Колония жива. Вы делали всё правильно.\n\n" +
                    "«Этого достаточно... наверное.»";
            break;
        case 2:
            title = "→ ПОБЕГ";
            text = "Ночь. Шлюз 9.\n\n" +
                    "Мирра, Зоя и четверо других ждут.\n" +
                    "Корабль без опознавательных знаков уходит в темноту.\n\n" +
                    "Мирра: «Там, куда мы летим, нет государств.»\n\n" +
                    "Звёзды.";
            break;
        case 3:
            title = "✗ НЕУСПЕХ";
            text = "Комиссар Волк зачитывает список ваших ошибок.\n\n" +
                    (EndingTracker.Errors >= 8
                        ? "Трибунал. Расстрел на рассвете."
                        : "Вас снимают с поста и переводят в трудовой отсек.");
            break;
        case 4:
            title = "✗ ИЗМЕНА РАСКРЫТА";
            text = "Агент «Серый» приходит на рассвете с ордером.\n\n" +
                    "На допросе — имена, шлюз 9, корабль.\n" +
                    "Мирра и Зоя схвачены.\n\n" +
                    "Камера. Темнота.";
            break;
        case 5:
            title = "☣ ЗАРАЖЕНИЕ";
            text = "Красные огни. Сирены.\n\n" +
                    "«Сектора A, B, C закрыты.\n" +
                    "Синее Гниение подтверждено.»\n\n" +
                    "Вы смотрите на своё отражение.\n" +
                    "На щеке — первое пятно.\n\n" +
                    "«Ты открыл им дверь.»";
            break;
        case 6:
            title = "⚠ ЗАХВАТ";
            text = "Серв-Легион и союзные пришельцы\n" +
                    "контролируют три сектора.\n\n" +
                    "Комиссар Волк мёртв.\n\n" +
                    "Вас ведут на допрос.\n" +
                    "За стеклом — чужая колония.";
            break;
        default:
            title = "КОНЕЦ";
            text = "Смена окончена.";
            break;
    }

    if (StoryFlags.HasanReachedLab && endingId != 6)
        text += "\n\n[Эпилог: Профессор Хасан нашёл частичную формулу вакцины.\nЕсть шанс.]";

    var endingForm = new System.Windows.Forms.Form
    {
        Text = title,
        Size = new System.Drawing.Size(560, 420),
        BackColor = System.Drawing.Color.FromArgb(10, 12, 18),
        ForeColor = System.Drawing.Color.FromArgb(180, 200, 220),
        StartPosition = FormStartPosition.CenterParent,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MaximizeBox = false,
        MinimizeBox = false,
        Font = new System.Drawing.Font("Consolas", 11)
    };

    var lblTitle = new System.Windows.Forms.Label
    {
        Text = title,
        Location = new System.Drawing.Point(20, 20),
        Size = new System.Drawing.Size(510, 30),
        ForeColor = endingId == 1 ? System.Drawing.Color.Cyan
                  : endingId == 2 ? System.Drawing.Color.DodgerBlue
                  : System.Drawing.Color.Tomato,
        Font = new System.Drawing.Font("Consolas", 13, System.Drawing.FontStyle.Bold)
    };

    var lblText = new System.Windows.Forms.Label
    {
        Text = text,
        Location = new System.Drawing.Point(20, 60),
        Size = new System.Drawing.Size(510, 280),
        ForeColor = System.Drawing.Color.FromArgb(180, 200, 220)
    };

    var btnClose = new System.Windows.Forms.Button
    {
        Text = "ЗАВЕРШИТЬ",
        Location = new System.Drawing.Point(200, 355),
        Size = new System.Drawing.Size(150, 36),
        FlatStyle = FlatStyle.Flat,
        ForeColor = System.Drawing.Color.Cyan
    };
    btnClose.FlatAppearance.BorderColor =
        System.Drawing.Color.FromArgb(51, 102, 170);
    btnClose.Click += (s, e) => { endingForm.Close(); this.Close(); };

    endingForm.Controls.AddRange(
        new System.Windows.Forms.Control[] { lblTitle, lblText, btnClose });

    EndingTracker.Reset();
    endingForm.ShowDialog(this);
}
    }
}
using System;
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
            currentCharacterIndex++;
            if (currentCharacterIndex < todayCast.Count)
                LoadCurrentCharacter();
            else
                OnCastExhausted();
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
// ═══════════════════════════════════════════════════════════════════════
//  Form1_GameplayPatch.cs  — partial class Form1
//  Содержит:
//  • InitDailyQuota()         — переменная квота 3–7 по дням
//  • ShowDialogueLog()        — кликабельное окно с полным диалогом
//  • UpdateDialogueBoxHint()  — подсказка «кликни для лога»
//  • GetDayDifficultyHint()   — подсказка об уровне сложности дня
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public partial class Form1
    {
        // ─── Полный лог диалогов с текущим персонажем ───────────────────
        private readonly List<string> currentDialogueLog = new List<string>();

        // ════════════════════════════════════════════════════════════════
        //  ПЕРЕМЕННАЯ КВОТА
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Устанавливает dailyQuota по таблице дней (3–7 персонажей).
        /// Вызывать при смене дня и при InitModeSession.
        /// </summary>
        internal void InitDailyQuota()
        {
            dailyQuota = CharacterDatabase.GetDailyQuota(day);
        }

        // ════════════════════════════════════════════════════════════════
        //  ДИАЛОГ-ЛОГ — кликабельное окно
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Добавляет реплику персонажа в лог текущего диалога.
        /// Вызывать каждый раз, когда персонаж говорит что-то новое.
        /// </summary>
        internal void AddToDialogueLog(string speaker, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            currentDialogueLog.Add($"[{speaker}]: {text}");
        }

        /// <summary>
        /// Очищает лог при переходе к следующему персонажу.
        /// </summary>
        internal void ClearDialogueLog()
        {
            currentDialogueLog.Clear();
        }

        /// <summary>
        /// Открывает прокручиваемое окно с полным диалогом.
        /// Привязывается к клику по zoneDialogueScreen.
        /// </summary>
        internal void ShowDialogueLog()
        {
            if (currentCharacterData == null) return;

            var dlgForm = new Form
            {
                Text = $"DIALOGUE LOG — {currentCharacterData.Name}",
                Size = new Size(560, 460),
                BackColor = Color.FromArgb(8, 10, 14),
                ForeColor = Color.FromArgb(0, 220, 100),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Font = new Font("Consolas", 10)
            };

            // Заголовок
            var header = new Label
            {
                Text = $"SUBJECT: {currentCharacterData.Name}  //  OCCUPATION: {currentCharacterData.Occupation}",
                Location = new Point(14, 12),
                Size = new Size(520, 20),
                ForeColor = Color.FromArgb(80, 180, 255),
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };

            // Разделитель
            var div = new Label
            {
                Text = new string('─', 68),
                Location = new Point(14, 34),
                Size = new Size(520, 14),
                ForeColor = Color.FromArgb(40, 80, 120),
                Font = new Font("Consolas", 9)
            };

            // Текстовое поле с прокруткой
            var txt = new RichTextBox
            {
                Location = new Point(14, 52),
                Size = new Size(520, 330),
                BackColor = Color.FromArgb(8, 10, 14),
                ForeColor = Color.FromArgb(0, 210, 90),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = true,
            };

            if (currentDialogueLog.Count == 0)
            {
                txt.Text = $"[{currentCharacterData.Name}]: {currentCharacterData.Dialogue}\n\n" +
                           "(No additional dialogue recorded. Use the radio to ask questions.)";
            }
            else
            {
                txt.Text = string.Join("\n\n", currentDialogueLog);
            }

            // Подсказка о дне сложности
            string hint = GetDayDifficultyHint(currentCharacterData.Day);
            var lblHint = new Label
            {
                Text = hint,
                Location = new Point(14, 390),
                Size = new Size(520, 32),
                ForeColor = currentCharacterData.Day >= 5
                    ? Color.FromArgb(220, 80, 60)
                    : Color.FromArgb(80, 150, 80),
                Font = new Font("Consolas", 8, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(420, 390),
                Size = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 40, 60),
                ForeColor = Color.FromArgb(80, 180, 255),
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(40, 100, 160);
            btnClose.Click += (s, e) => dlgForm.Close();

            dlgForm.Controls.AddRange(new Control[] { header, div, txt, lblHint, btnClose });
            dlgForm.ShowDialog(this);
        }

        private string GetDayDifficultyHint(int day)
        {
            if (day >= 8) return "⚠ Day 8+: All biometrics unreliable. Dialogue is your only tool.";
            if (day >= 5) return "⚠ Day 5+: Advanced mimicry active. Listen for hesitation and slips.";
            if (day >= 3) return "ℹ Day 3+: Synthetics improving. Check access codes carefully.";
            return "ℹ Day 1–2: Rely on biometrics and access code to identify subjects.";
        }

        // ════════════════════════════════════════════════════════════════
        //  ГЕЙМПЛЕЙ: запись в лог при каждом ответе на вопрос
        //  Нужно вызывать из ShowQuestionDialog после получения ответа
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Записывает вопрос инспектора и ответ персонажа в лог.
        /// </summary>
        internal void LogQuestionAndAnswer(string question, string answer)
        {
            AddToDialogueLog("INSPECTOR", question);
            if (currentCharacterData != null)
                AddToDialogueLog(currentCharacterData.Name, answer);
        }

        // ════════════════════════════════════════════════════════════════
        //  ГЕЙМПЛЕЙ: показывать подсказку в диалог-боксе
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Устанавливает в lblDialogue короткий текст + подсказку «[click for log]».
        /// </summary>
        internal void SetDialogueWithHint(string text)
        {
            if (text == null) return;

            // Показываем первые ~80 символов + подсказку если текст длиннее
            string display = text.Length > 80
                ? text.Substring(0, 77) + "..."
                : text;

            // Добавляем в лог полный текст
            if (currentCharacterData != null)
                AddToDialogueLog(currentCharacterData.Name, text);

            StartTypingEffect(display);
        }

        // ════════════════════════════════════════════════════════════════
        //  ГЕЙМПЛЕЙ: анимация нажатия кнопки в зависимости от дня
        //  Подсказывает цвет вспышки (НЕ подсказывает правильный ответ)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Возвращает цвет вспышки. На ранних днях — чистый цвет кнопки.
        /// На поздних — слегка смешивается с белым (неопределённость).
        /// </summary>
        internal Color GetFlashColor(Color buttonColor, int day)
        {
            if (day < 5) return buttonColor;

            // С Дня 5 — цвет немного «промыт» белым, намекая на неопределённость
            int blend = Math.Min((day - 4) * 15, 60); // до 60% промывки
            return Color.FromArgb(
                Math.Min(255, buttonColor.R + blend),
                Math.Min(255, buttonColor.G + blend),
                Math.Min(255, buttonColor.B + blend));
        }

        // ════════════════════════════════════════════════════════════════
        //  МИНИ-ПОДСКАЗКА ДЛЯ HUD — показывает что сегодня проверять
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Возвращает строку для вывода в lblPressure или отдельный label.
        /// Меняется каждый день.
        /// </summary>
        internal static string GetDailyFocusHint(int day)
        {
            switch (day)
            {
                case 1: return "FOCUS: Check access codes and biometrics";
                case 2: return "FOCUS: Robots may fake emotions — listen for pauses";
                case 3: return "FOCUS: Ask about family — aliens use 'we/us'";
                case 4: return "FOCUS: Check pronoun usage and body temperature";
                case 5: return "FOCUS: Biometrics unreliable — trust dialogue only";
                case 6: return "FOCUS: Aliens now use human names — check access code";
                case 7: return "FOCUS: Everything looks normal — look for subtle slips";
                case 8: return "FOCUS: Maximum mimicry — ask about biological composition";
                case 9: return "FOCUS: Trust nothing. Verify everything twice.";
                case 10: return "FOCUS: Last day. Every decision counts.";
                default: return $"FOCUS: Day {day} — stay vigilant";
            }
        }
    }
}
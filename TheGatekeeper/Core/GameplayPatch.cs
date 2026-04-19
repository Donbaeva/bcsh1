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
            ShowInGameDialogPanel();
        }

        // ─── Внутриигровая панель: вкладки Dialogue / Documents ─────────────
        internal void ShowInGameDialogPanel()
        {
            if (overlayPanel == null) return;

            // Скрываем стандартные overlayTitle/overlayBody — используем кастомный контент
            overlayTitle.Visible = false;
            overlayBody.Visible = false;
            overlayClose.Visible = false;

            // Получаем card (первый дочерний Panel overlayPanel)
            Panel card = null;
            foreach (Control c in overlayPanel.Controls)
                if (c is Panel p) { card = p; break; }
            if (card == null) return;

            // Убираем старый кастомный контент если был
            var toRemove = new System.Collections.Generic.List<Control>();
            foreach (Control c in card.Controls)
                if (c.Tag?.ToString() == "dlg_panel") toRemove.Add(c);
            foreach (var c in toRemove) card.Controls.Remove(c);

            int cw = card.Width, ch = card.Height;

            // Имя субъекта / заголовок
            string title = currentCharacterData != null
                ? $"COMMUNICATION LOG — {currentCharacterData.Occupation ?? "SUBJECT"}"
                : "COMMUNICATION LOG";

            var lblTitle = new Label
            {
                Text = title,
                Tag = "dlg_panel",
                Location = new Point(16, 14),
                Size = new Size(cw - 60, 24),
                ForeColor = Color.FromArgb(100, 170, 238),
                Font = new Font("Consolas", 11, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            // Кнопка закрыть
            var btnX = new Button
            {
                Text = "✕",
                Tag = "dlg_panel",
                Location = new Point(cw - 36, 10),
                Size = new Size(26, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(120, 100, 130, 170),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) =>
            {
                overlayTitle.Visible = true; overlayBody.Visible = true; overlayClose.Visible = true;
                var rem = new System.Collections.Generic.List<Control>();
                foreach (Control c in card.Controls) if (c.Tag?.ToString() == "dlg_panel") rem.Add(c);
                foreach (var c in rem) card.Controls.Remove(c);
                HideOverlay();
            };
            card.Controls.Add(btnX);

            // ── Вкладки ────────────────────────────────────────────────────
            var tabDlg = MakeTabButton("💬 DIALOGUE", new Point(16, 44), true);
            var tabDoc = MakeTabButton("📁 DOCUMENTS", new Point(146, 44), false);
            tabDlg.Tag = tabDoc.Tag = "dlg_panel";
            card.Controls.Add(tabDlg); card.Controls.Add(tabDoc);

            // Разделитель под вкладками
            var divTab = new Label
            {
                Tag = "dlg_panel",
                Location = new Point(16, 72),
                Size = new Size(cw - 32, 1),
                BackColor = Color.FromArgb(60, 51, 102, 170)
            };
            card.Controls.Add(divTab);

            // ── Контент-панель ──────────────────────────────────────────────
            var contentPanel = new Panel
            {
                Tag = "dlg_panel",
                Location = new Point(16, 80),
                Size = new Size(cw - 32, ch - 130),
                BackColor = Color.Transparent
            };
            card.Controls.Add(contentPanel);

            // ── Уведомление о документе (если есть непринятый) ─────────────
            var pendingDoc = _pendingDocument;
            Panel docNotifyPanel = null;
            if (pendingDoc != null)
            {
                docNotifyPanel = new Panel
                {
                    Tag = "dlg_panel",
                    Location = new Point(16, ch - 118),
                    Size = new Size(cw - 32, 50),
                    BackColor = Color.FromArgb(20, 60, 30)
                };
                docNotifyPanel.Paint += (s, pe) =>
                {
                    using (var pen = new Pen(Color.FromArgb(100, 0, 160, 80), 1))
                        pe.Graphics.DrawRectangle(pen, 0, 0, docNotifyPanel.Width - 1, docNotifyPanel.Height - 1);
                };
                var notifyLbl = new Label
                {
                    Text = $"📄 {pendingDoc.From} handed you something: \"{pendingDoc.Title}\"",
                    Location = new Point(8, 4),
                    Size = new Size(380, 18),
                    ForeColor = Color.FromArgb(160, 220, 140),
                    Font = new Font("Consolas", 8f, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                var btnAccept = new Button
                {
                    Text = "ACCEPT",
                    Location = new Point(390, 6),
                    Size = new Size(80, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(15, 60, 25),
                    ForeColor = Color.FromArgb(0, 200, 80),
                    Font = new Font("Consolas", 8),
                    Cursor = Cursors.Hand
                };
                btnAccept.FlatAppearance.BorderColor = Color.FromArgb(0, 140, 60);
                btnAccept.Click += (s, e) =>
                {
                    AcceptPendingDocument();
                    docNotifyPanel.Visible = false;
                    // Переключаемся на вкладку Documents
                    ShowDocumentsTab(contentPanel, cw);
                };
                var btnDecline = new Button
                {
                    Text = "DECLINE",
                    Location = new Point(475, 6),
                    Size = new Size(80, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 15, 15),
                    ForeColor = Color.FromArgb(180, 80, 80),
                    Font = new Font("Consolas", 8),
                    Cursor = Cursors.Hand
                };
                btnDecline.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
                btnDecline.Click += (s, e) =>
                {
                    _pendingDocument = null;
                    docNotifyPanel.Visible = false;
                };
                var btnLine2 = new Label
                {
                    Text = "Accept and store in your document vault, or decline.",
                    Location = new Point(8, 26),
                    Size = new Size(460, 14),
                    ForeColor = Color.FromArgb(80, 140, 80),
                    Font = new Font("Consolas", 7.5f),
                    BackColor = Color.Transparent
                };
                docNotifyPanel.Controls.AddRange(new Control[] { notifyLbl, btnAccept, btnDecline, btnLine2 });
                card.Controls.Add(docNotifyPanel);
            }

            // ── Показываем вкладку Dialogue по умолчанию ───────────────────
            ShowDialogueTab(contentPanel, cw);

            tabDlg.Click += (s, e) =>
            {
                SetTabActive(tabDlg, true); SetTabActive(tabDoc, false);
                ShowDialogueTab(contentPanel, cw);
            };
            tabDoc.Click += (s, e) =>
            {
                SetTabActive(tabDlg, false); SetTabActive(tabDoc, true);
                ShowDocumentsTab(contentPanel, cw);
            };

            overlayPanel.Visible = true;
            overlayPanel.BringToFront();
        }

        private Button MakeTabButton(string text, Point loc, bool active)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(120, 24),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            SetTabActive(btn, active);
            return btn;
        }

        private void SetTabActive(Button btn, bool active)
        {
            btn.BackColor = active ? Color.FromArgb(30, 50, 80) : Color.FromArgb(12, 18, 28);
            btn.ForeColor = active ? Color.FromArgb(120, 180, 255) : Color.FromArgb(60, 90, 130);
            btn.FlatAppearance.BorderColor = active
                ? Color.FromArgb(60, 102, 170)
                : Color.FromArgb(25, 40, 70);
        }

        // ── Содержимое вкладки DIALOGUE ──────────────────────────────────────
        private void ShowDialogueTab(Panel content, int cw)
        {
            content.Controls.Clear();
            var txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(6, 8, 12),
                ForeColor = Color.FromArgb(0, 210, 90),
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = true
            };
            if (currentDialogueLog == null || currentDialogueLog.Count == 0)
                txt.Text = currentCharacterData != null
                    ? $"[{currentCharacterData.Name}]: {currentCharacterData.Dialogue}\n\n(Ask questions using the small radio on the right.)"
                    : "(No active subject.)";
            else
                txt.Text = string.Join("\n\n", currentDialogueLog);

            // Подсветка: INSPECTOR — голубой, субъект — зелёный
            content.Controls.Add(txt);

            // Подсказка дня
            string hint = currentCharacterData != null ? GetDayDifficultyHint(currentCharacterData.Day) : "";
            if (!string.IsNullOrEmpty(hint))
            {
                var hLbl = new Label
                {
                    Text = hint,
                    Dock = DockStyle.Bottom,
                    Height = 18,
                    ForeColor = currentCharacterData.Day >= 5
                        ? Color.FromArgb(200, 80, 60) : Color.FromArgb(70, 140, 70),
                    Font = new Font("Consolas", 7.5f, FontStyle.Italic),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(2, 0, 0, 0)
                };
                content.Controls.Add(hLbl);
            }
        }

        // ── Содержимое вкладки DOCUMENTS ────────────────────────────────────
        private void ShowDocumentsTab(Panel content, int cw)
        {
            content.Controls.Clear();

            if (_documentVault == null || _documentVault.Count == 0)
            {
                content.Controls.Add(new Label
                {
                    Text = "\n  No documents in vault.\n\n  Accept documents from subjects\n  during inspection.",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.FromArgb(60, 90, 120),
                    Font = new Font("Consolas", 9.5f),
                    BackColor = Color.Transparent
                });
                return;
            }

            // Список слева
            var listBox = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(170, content.Height),
                BackColor = Color.FromArgb(8, 12, 20)
            };
            // Просмотр справа
            var viewBox = new Panel
            {
                Location = new Point(172, 0),
                Size = new Size(content.Width - 174, content.Height),
                BackColor = Color.Transparent
            };
            content.Controls.Add(listBox);
            content.Controls.Add(viewBox);

            void ShowDocView(SecretDocument d)
            {
                d.IsRead = true;
                viewBox.Controls.Clear();

                var stampLbl = new Label
                {
                    Text = $"[ {d.Stamp} ]",
                    Location = new Point(0, 0),
                    Size = new Size(viewBox.Width, 16),
                    AutoSize = false,
                    ForeColor = DocStampColor(d.Stamp),
                    Font = new Font("Consolas", 8f, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                var titleLbl = new Label
                {
                    Text = d.Title,
                    Location = new Point(0, 18),
                    Size = new Size(viewBox.Width, 20),
                    AutoSize = false,
                    ForeColor = Color.FromArgb(200, 220, 240),
                    Font = new Font("Consolas", 10f, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                var fromLbl = new Label
                {
                    Text = $"FROM: {d.From}  //  DAY {d.Day}",
                    Location = new Point(0, 42),
                    Size = new Size(viewBox.Width, 13),
                    AutoSize = false,
                    ForeColor = Color.FromArgb(60, 90, 140),
                    Font = new Font("Consolas", 7.5f),
                    BackColor = Color.Transparent
                };
                var sepLine = new Label
                {
                    Location = new Point(0, 58),
                    Size = new Size(viewBox.Width, 1),
                    BackColor = Color.FromArgb(40, 51, 102, 170)
                };
                var body = new RichTextBox
                {
                    Text = d.Content,
                    Location = new Point(0, 62),
                    Size = new Size(viewBox.Width, viewBox.Height - 130),
                    BackColor = Color.FromArgb(6, 8, 12),
                    ForeColor = Color.FromArgb(170, 195, 220),
                    Font = new Font("Consolas", 9f),
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    ScrollBars = RichTextBoxScrollBars.Vertical,
                    WordWrap = true
                };

                // Кнопки действий
                int btnY = viewBox.Height - 62;
                var btnFel = new Button
                {
                    Text = "📋 HAND TO FELICIA  (+Loyalty +2)",
                    Location = new Point(0, btnY),
                    Size = new Size(viewBox.Width, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(10, 40, 18),
                    ForeColor = Color.FromArgb(0, 190, 80),
                    Font = new Font("Consolas", 7.5f),
                    Cursor = Cursors.Hand
                };
                btnFel.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 40);
                btnFel.Click += (s, e) =>
                {
                    score += 20;
                    EndingTracker.Loyalty += 2;
                    _documentVault.Remove(d);
                    AddToDialogueLog("INSPECTOR", $"[Handed \"{d.Title}\" to Commander Felicia]");
                    StartTypingEffect("You pass the document. Felicia reads it carefully. \"I'll look into this.\"");
                    ShowDocumentsTab(content, cw);
                };

                var btnWolf = new Button
                {
                    Text = "🔴 REPORT TO WOLF  (+Loyalty +3, +Caught)",
                    Location = new Point(0, btnY + 32),
                    Size = new Size(viewBox.Width, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(35, 10, 10),
                    ForeColor = Color.FromArgb(200, 80, 80),
                    Font = new Font("Consolas", 7.5f),
                    Cursor = Cursors.Hand
                };
                btnWolf.FlatAppearance.BorderColor = Color.FromArgb(100, 30, 30);
                btnWolf.Click += (s, e) =>
                {
                    score += 30;
                    EndingTracker.Loyalty += 3;
                    EndingTracker.Caught += 1;
                    _documentVault.Remove(d);
                    AddToDialogueLog("INSPECTOR", $"[Filed \"{d.Title}\" with Commissar Wolf]");
                    StartTypingEffect("Wolf takes it without a word. Someone will pay for this.");
                    ShowDocumentsTab(content, cw);
                };

                viewBox.Controls.AddRange(new Control[] { stampLbl, titleLbl, fromLbl, sepLine, body, btnFel, btnWolf });
            }

            // Список документов
            int y = 4;
            foreach (var doc in _documentVault)
            {
                var d = doc;
                var btn = new Button
                {
                    Text = (d.IsRead ? "  " : "● ") + d.Title.Substring(0, Math.Min(d.Title.Length, 20)),
                    Location = new Point(2, y),
                    Size = new Size(164, 36),
                    BackColor = d.IsRead ? Color.FromArgb(10, 14, 22) : Color.FromArgb(18, 30, 48),
                    ForeColor = d.IsRead ? Color.FromArgb(80, 110, 150) : Color.FromArgb(160, 210, 255),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 7.5f),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(25, 40, 65);
                btn.Click += (s, e) => ShowDocView(d);
                listBox.Controls.Add(btn);
                y += 40;
            }

            // Открываем первый документ
            var first = _documentVault.Find(d => !d.IsRead) ?? _documentVault[0];
            ShowDocView(first);
        }

        private Color DocStampColor(string stamp)
        {
            switch (stamp?.ToUpper())
            {
                case "CLASSIFIED": return Color.FromArgb(220, 60, 60);
                case "REBEL": return Color.FromArgb(200, 160, 40);
                case "MEDICAL": return Color.FromArgb(60, 200, 140);
                case "EVIDENCE": return Color.FromArgb(200, 100, 220);
                default: return Color.FromArgb(120, 150, 180);
            }
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
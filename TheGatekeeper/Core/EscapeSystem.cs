// ═══════════════════════════════════════════════════════════════════════
//  EscapeSystem.cs  — partial class Form1  (папка Core)
//
//  Всё самодостаточно. Не требует изменений в других файлах кроме:
//  1. Form1.cs  Redraw()               → добавить: DrawEscapeButton(g);
//  2. Form1.cs  LoadCurrentCharacter() → добавить: EndingTracker.WolfQuestionsThisVisit = 0;
//  3. Form1.cs  InitModeSession()      → добавить: EscapeFlags.Reset();
//  4. Form1_Input.cs Form_MouseClick_New() → добавить в начало: if (TryHandleEscapeClick(p)) return;
//  5. Form1_Input.cs Form_MouseMove_New()  → добавить условие в onActive (см. низ файла)
//  6. SecretDocumentSystem.cs ReceiveDocument() → 2 строки (см. низ файла)
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    // ═══════════════════════════════════════════════════════════════════
    //  ESCAPE FLAGS
    // ═══════════════════════════════════════════════════════════════════
    public static class EscapeFlags
    {
        public static bool HasNinasNote = false;
        public static bool ConfirmedEscape = false;
        public static bool EscapeRevealed = false;

        public static int SuspicionScore = 0;
        public static int HiddenDocuments = 0;
        public static int DocumentsPassedToFelicia = 0;
        public static bool ToldZoya = false;
        public static bool ToldMirra = false;
        public static bool WolfSuspects = false;
        public static bool CastroSuspects = false;
        public static bool GreyKnows = false;
        public static bool CarryingHotDoc = false;

        public static void Reset()
        {
            HasNinasNote = ConfirmedEscape = EscapeRevealed = false;
            SuspicionScore = HiddenDocuments = DocumentsPassedToFelicia = 0;
            ToldZoya = ToldMirra = WolfSuspects = CastroSuspects = GreyKnows = CarryingHotDoc = false;
        }

        public static void AddSuspicion(int amount, string reason)
            => SuspicionScore = Math.Min(100, SuspicionScore + amount);

        public static void ReduceSuspicion(int amount, string reason)
            => SuspicionScore = Math.Max(0, SuspicionScore - amount);

        /// <summary>
        /// 0 = чистый побег | 1 = ушёл но следят | 2 = поймали | 3 = предали
        /// </summary>
        public static int DetermineEscapeOutcome()
        {
            if (GreyKnows && SuspicionScore >= 60) return 2;
            if (WolfSuspects && CastroSuspects) return 2;
            if (SuspicionScore >= 80) return 2;
            if (SuspicionScore >= 40 || WolfSuspects || GreyKnows) return 1;
            if (EscapeRevealed && DocumentsPassedToFelicia == 0) return 3;
            return 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  PARTIAL CLASS FORM1
    // ═══════════════════════════════════════════════════════════════════
    public partial class Form1
    {
        // Зона кнопки ESCAPE (справа от [PASS])
        private readonly Rectangle escapeButtonZoneVisible = new Rectangle(220, 548, 165, 82);

        // ════════════════════════════════════════════════════════════════
        //  ТРИГГЕРЫ — вызываются из OnArrival() персонажей
        // ════════════════════════════════════════════════════════════════

        internal void OnNinasNoteReceived()
        {
            if (EscapeFlags.HasNinasNote) return;
            EscapeFlags.HasNinasNote = true;
            EscapeFlags.AddSuspicion(5, "Received rebel note");
            ShowEscapeToast(
                "NOTE RECEIVED",
                "Nina's message is in your documents.\nA new option appears between subjects.",
                Color.FromArgb(200, 160, 40));
        }

        internal void OnDocumentsGivenToFelicia(int count)
        {
            EscapeFlags.DocumentsPassedToFelicia += count;
            EndingTracker.DocumentsGivenToCommissar = false;
            EscapeFlags.ReduceSuspicion(15, "Trusted Felicia with evidence");
        }

        internal void OnWolfArrival()
        {
            if (!EscapeFlags.HasNinasNote) return;
            int rebelCount = _documentVault.FindAll(
                d => d.Stamp == "REBEL" || d.Stamp == "EVIDENCE").Count;
            if (rebelCount >= 2)
            {
                EscapeFlags.AddSuspicion(15, "Wolf arrived while holding rebel docs");
                EscapeFlags.WolfSuspects = true;
                ShowHideDocumentPrompt();
            }
            else if (EndingTracker.RebelTrust >= 3)
            {
                EscapeFlags.AddSuspicion(10, "Wolf senses rebel trust");
                EscapeFlags.WolfSuspects = true;
            }
        }

        internal void OnCastroArrival()
        {
            if (!EscapeFlags.HasNinasNote) return;
            EscapeFlags.AddSuspicion(8, "Castro patrol while note held");
            EscapeFlags.CastroSuspects = true;
            ShowCastroSearchPrompt();
        }

        internal void OnGreyArrival()
        {
            if (EndingTracker.RebelTrust >= 4)
            {
                EscapeFlags.GreyKnows = true;
                EscapeFlags.AddSuspicion(30, "Grey confirmed rebel contact");
            }
            else if (EndingTracker.RebelTrust >= 2)
                EscapeFlags.AddSuspicion(15, "Grey suspects rebel contact");
        }

        // ════════════════════════════════════════════════════════════════
        //  ФЕЛИСИЯ — диалог передачи документов
        //  Вызывается из CommanderFelicia.OnArrival() в StoryCharacters.cs
        // ════════════════════════════════════════════════════════════════
        internal void ShowFeliciaDocumentTransferDialog(CommanderFelicia felicia)
        {
            var rebelDocs = _documentVault.FindAll(
                d => d.Stamp == "REBEL" || d.Stamp == "EVIDENCE" || d.Stamp == "MEDICAL");

            if (rebelDocs.Count == 0)
            {
                StartTypingEffect(
                    "Felicia scans your desk. " +
                    "\"Nothing to report? Very well. Your record speaks for itself.\"");
                return;
            }

            var titles = new System.Text.StringBuilder();
            foreach (var d in rebelDocs)
                titles.AppendLine($"  • {d.Title}");

            var dlg = CreateEscapeDialog(580, 290,
                "COMMANDER FELICIA — Do you have anything to hand over?",
                Color.FromArgb(80, 160, 220));

            AddEscapeLabel(dlg,
                $"You have {rebelDocs.Count} document(s) that may be relevant:\n{titles}",
                16, 50, 548, 110, Color.FromArgb(180, 200, 220));

            AddEscapeButton(dlg,
                $"HAND OVER — give Felicia {rebelDocs.Count} document(s)",
                Color.FromArgb(80, 200, 255), Color.FromArgb(8, 20, 35),
                16, 172, () =>
                {
                    dlg.Close();
                    felicia.ReceivedDocuments = true;
                    OnDocumentsGivenToFelicia(rebelDocs.Count);
                    StartTypingEffect(
                        "Felicia takes the documents without a word. " +
                        "She reads the first page. Her expression doesn't change. " +
                        "\"Thank you, Inspector. I'll handle this myself.\"");
                    AddToDialogueLog("INSPECTOR", "[Handed documents to Commander Felicia]");
                });

            AddEscapeButton(dlg,
                "KEEP THEM — not yet",
                Color.FromArgb(140, 140, 160), Color.FromArgb(16, 16, 22),
                16, 218, () =>
                {
                    dlg.Close();
                    StartTypingEffect(
                        "Felicia nods slowly. " +
                        "\"The offer stands. Think carefully.\"");
                });

            dlg.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        //  ДИАЛОГ — СПРЯТАТЬ ДОКУМЕНТЫ ОТ ВОЛКА
        // ════════════════════════════════════════════════════════════════
        private void ShowHideDocumentPrompt()
        {
            var dlg = CreateEscapeDialog(540, 236,
                "⚠  COMMISSAR WOLF IS APPROACHING",
                Color.FromArgb(180, 40, 40));

            AddEscapeLabel(dlg,
                "Wolf is heading to your desk.\n\n" +
                "You're holding documents that could incriminate you.\n" +
                "You have seconds to decide.",
                16, 50, 508, 80, Color.FromArgb(210, 190, 190));

            AddEscapeButton(dlg,
                "HIDE — slip them out of sight before he arrives",
                Color.FromArgb(0, 180, 80), Color.FromArgb(8, 30, 12),
                16, 142, () =>
                {
                    dlg.Close();
                    EscapeFlags.HiddenDocuments++;
                    EscapeFlags.ReduceSuspicion(8, "Hid docs before Wolf");
                    StartTypingEffect(
                        "You slide the documents out of sight just before Wolf reaches the desk. " +
                        "Your hands are steady. Your face — less so.");
                    AddToDialogueLog("INSPECTOR", "[Hid rebel documents from Wolf]");
                });

            AddEscapeButton(dlg,
                "DO NOTHING — hope he doesn't look too closely",
                Color.FromArgb(200, 160, 40), Color.FromArgb(24, 20, 8),
                16, 188, () =>
                {
                    dlg.Close();
                    EscapeFlags.AddSuspicion(12, "Did nothing while Wolf checked desk");
                    StartTypingEffect(
                        "You leave everything as it is. Wolf's eyes move across your desk. " +
                        "He says nothing. But Wolf always notices things. That's his job.");
                });

            dlg.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        //  ДИАЛОГ — ОБЫСК КАСТРО
        // ════════════════════════════════════════════════════════════════
        private void ShowCastroSearchPrompt()
        {
            var dlg = CreateEscapeDialog(540, 278,
                "⚠  SERGEANT CASTRO — SPOT INSPECTION",
                Color.FromArgb(160, 100, 20));

            AddEscapeLabel(dlg,
                "Castro eyes your desk. She mentions she needs to check\n" +
                "\"standard post compliance\" — she's looking for contraband.\n\n" +
                "What do you say?",
                16, 50, 508, 72, Color.FromArgb(210, 200, 180));

            AddEscapeButton(dlg,
                "ACT NORMAL — \"Everything is in order, Sergeant.\"",
                Color.FromArgb(80, 160, 255), Color.FromArgb(8, 16, 36),
                16, 134, () =>
                {
                    dlg.Close();
                    if (EscapeFlags.SuspicionScore < 40)
                    {
                        EscapeFlags.ReduceSuspicion(5, "Confident response");
                        StartTypingEffect(
                            "\"I'll take your word for it.\" " +
                            "Castro scans the desk with her eyes, then moves on. You breathe again.");
                    }
                    else
                    {
                        EscapeFlags.AddSuspicion(8, "Castro noticed something");
                        StartTypingEffect(
                            "Castro pauses. \"Interesting reading material.\" " +
                            "She doesn't elaborate. Moves on. That was not a compliment.");
                    }
                    AddToDialogueLog("INSPECTOR", "[Told Castro everything is in order]");
                });

            AddEscapeButton(dlg,
                "REDIRECT — \"Anything specific I can help you find?\"",
                Color.FromArgb(0, 200, 100), Color.FromArgb(6, 26, 14),
                16, 180, () =>
                {
                    dlg.Close();
                    EscapeFlags.ReduceSuspicion(10, "Controlled Castro search");
                    StartTypingEffect(
                        "Castro looks at you. \"No. Carry on.\" She moves to the next post. " +
                        "Taking control — good instinct.");
                    AddToDialogueLog("INSPECTOR", "[Redirected Castro's inspection]");
                });

            AddEscapeButton(dlg,
                "PANIC — start explaining yourself unprompted",
                Color.FromArgb(220, 80, 80), Color.FromArgb(24, 8, 8),
                16, 226, () =>
                {
                    dlg.Close();
                    EscapeFlags.AddSuspicion(20, "Panicked under Castro");
                    EscapeFlags.CastroSuspects = true;
                    StartTypingEffect(
                        "\"I haven't done anything, it's just documents — I didn't read them, " +
                        "well one of them — \" Castro stares at you. \"I didn't ask.\" " +
                        "She writes something down.");
                    AddToDialogueLog("INSPECTOR", "[Panicked during Castro inspection]");
                });

            dlg.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        //  КНОПКА ESCAPE НАЖАТА
        // ════════════════════════════════════════════════════════════════
        internal void HandleEscapeButtonClick()
        {
            if (!EscapeFlags.HasNinasNote || EscapeFlags.ConfirmedEscape) return;
            pressureTimer.Stop();
            ShowEscapeConfirmationDialog();
        }

        private void ShowEscapeConfirmationDialog()
        {
            string dangerText;
            Color dangerColor;
            if (EscapeFlags.SuspicionScore >= 70)
            { dangerText = "⚠ DANGER: CRITICAL — You've been noticed."; dangerColor = Color.FromArgb(220, 60, 60); }
            else if (EscapeFlags.SuspicionScore >= 40)
            { dangerText = "⚠ DANGER: ELEVATED — Someone suspects."; dangerColor = Color.FromArgb(220, 160, 40); }
            else
            { dangerText = "✓ DANGER: LOW — You've been careful."; dangerColor = Color.FromArgb(60, 200, 100); }

            string stateText =
                $"Documents passed to Felicia:  {EscapeFlags.DocumentsPassedToFelicia}\n" +
                $"Hidden from Wolf:             {EscapeFlags.HiddenDocuments}\n" +
                $"Wolf suspects you:            {(EscapeFlags.WolfSuspects ? "YES ⚠" : "No")}\n" +
                $"Agent Grey confirmed contact: {(EscapeFlags.GreyKnows ? "YES ⚠" : "No")}\n" +
                $"Suspicion score:              {EscapeFlags.SuspicionScore} / 100";

            var dlg = CreateEscapeDialog(580, 358,
                "⟶  AIRLOCK 9 — 03:00 TONIGHT",
                Color.FromArgb(80, 140, 255));

            AddEscapeLabel(dlg, dangerText, 16, 48, 548, 24, dangerColor);
            dlg.Controls.Add(new Label
            {
                Location = new Point(16, 76),
                Size = new Size(548, 1),
                BackColor = Color.FromArgb(40, 80, 140, 200)
            });
            AddEscapeLabel(dlg, stateText, 16, 84, 548, 105, Color.FromArgb(160, 185, 210));
            AddEscapeLabel(dlg,
                "Mirra's ship leaves at 03:00.\nOnce you leave Gate 7 — there is no coming back.",
                16, 200, 548, 40, Color.FromArgb(180, 195, 215));

            AddEscapeButton(dlg,
                "▶  LEAVE — walk away from Gate 7 forever",
                Color.FromArgb(80, 200, 255), Color.FromArgb(6, 18, 34),
                16, 252, () =>
                {
                    dlg.Close();
                    EscapeFlags.ConfirmedEscape = true;
                    EndingTracker.RebelTrust += 3;
                    ProcessEscapeAttempt();
                });

            AddEscapeButton(dlg,
                "✕  NOT YET — stay at the post",
                Color.FromArgb(120, 120, 140), Color.FromArgb(14, 14, 20),
                16, 298, () =>
                {
                    dlg.Close();
                    pressureTimer.Start();
                    StartTypingEffect(
                        "Not yet. You return to your post. " +
                        "The clock is still running. Airlock 9 is still there.");
                });

            dlg.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        //  ИТОГ ПОБЕГА
        // ════════════════════════════════════════════════════════════════
        private void ProcessEscapeAttempt()
        {
            pressureTimer.Stop();
            int outcome = EscapeFlags.DetermineEscapeOutcome();

            string title; Color accent; string[] lines;
            switch (outcome)
            {
                case 0:
                    title = "CLEAN ESCAPE"; accent = Color.FromArgb(80, 200, 255);
                    lines = new[] {
                        "You walk away from Gate 7 at 02:47.",
                        "Nobody stops you.", "The corridor is empty.",
                        "Airlock 9 opens without a sound.",
                        "Mirra is there. Zoya is there.",
                        "One seat, empty, with your name on it.", "",
                        "The ship detaches from the station.",
                        "Through the porthole — the colony gets smaller.",
                        "Then smaller.", "Then gone.", "", "You made it."
                    }; break;
                case 1:
                    title = "NARROW ESCAPE"; accent = Color.FromArgb(220, 160, 40);
                    lines = new[] {
                        "You leave at 02:51.",
                        "Someone has been watching Gate 7.",
                        "You hear boots behind you at Corridor 4.",
                        "You don't look back.", "",
                        "Airlock 9. The ship is warm. Running.",
                        "You get in. The door seals.", "",
                        "Through the hull — alarms.",
                        "They know you're gone.",
                        "But you're already gone.", "",
                        "The stars don't wait for permissions."
                    }; break;
                case 2:
                    title = "CAUGHT"; accent = Color.FromArgb(220, 60, 60);
                    lines = new[] {
                        "You reach Corridor 4.",
                        "Two figures step out of the shadow.", "",
                        "Commissar Wolf.", "And Agent Grey.", "",
                        "\"Inspector.\"", "\"We've been expecting this.\"", "",
                        "Your hands are behind your back before you finish the sentence.", "",
                        "Through the window of the transport vehicle —",
                        "you see the ship go.",
                        "A crescent scar on the hull.", "", "Then nothing."
                    }; break;
                default:
                    title = "BETRAYED"; accent = Color.FromArgb(140, 120, 160);
                    lines = new[] {
                        "Airlock 9 is sealed when you arrive.",
                        "Security tape across the door.",
                        "The ship is gone.", "", "Two hours early.", "",
                        "Someone talked.", "Or someone was made to talk.", "",
                        "You stand at the sealed airlock for a long time.",
                        "Then you walk back to Gate 7.", "",
                        "The post is still there.", "The chair is still there.",
                        "You sit down.", "",
                        "The morning shift starts in four hours."
                    }; break;
            }
            ShowEscapeEndingScreen(title, lines, accent);
        }

        // ════════════════════════════════════════════════════════════════
        //  ЭКРАН КОНЦОВКИ ПОБЕГА
        // ════════════════════════════════════════════════════════════════
        private void ShowEscapeEndingScreen(string title, string[] lines, Color accent)
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

            int lineIndex = 0, charIndex = 0, alpha = 0;
            string displayText = "";
            bool done = false;

            var timer = new Timer { Interval = 50 };
            timer.Tick += (s, e) =>
            {
                if (alpha < 255) alpha += 5;
                if (!done)
                {
                    if (lineIndex < lines.Length)
                    {
                        string cur = lines[lineIndex];
                        if (charIndex < cur.Length) { displayText += cur[charIndex]; charIndex++; }
                        else { lineIndex++; charIndex = 0; displayText = ""; }
                    }
                    else done = true;
                }
                screen.Invalidate();
            };

            screen.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int W = screen.ClientSize.Width, H = screen.ClientSize.Height;
                g.Clear(Color.Black);

                using (var br = new LinearGradientBrush(
                    new Rectangle(0, 0, W, 3), accent, Color.Transparent,
                    LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, W, 3);

                using (var f1 = new Font("Consolas", 18, FontStyle.Bold))
                using (var br = new SolidBrush(Color.FromArgb(Math.Min(220, alpha), accent)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(title, f1, br, W / 2f, H / 2 - 170, sf);
                }

                using (var pen = new Pen(Color.FromArgb(Math.Min(60, alpha / 3), accent), 1))
                    g.DrawLine(pen, W / 2 - 220, H / 2 - 132, W / 2 + 220, H / 2 - 132);

                float ty = H / 2f - 118f;
                using (var f2 = new Font("Consolas", 10))
                {
                    for (int li = 0; li < lineIndex && li < lines.Length; li++)
                    {
                        string lt = (li == lineIndex - 1 && !done) ? displayText : lines[li];
                        int la = Math.Max(0, Math.Min(200, alpha - li * 6));
                        Color tc = li >= lines.Length - 3
                            ? Color.FromArgb(la, 140, 160, 190)
                            : Color.FromArgb(la, 190, 205, 220);
                        using (var br = new SolidBrush(tc))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center };
                            g.DrawString(lt, f2, br, W / 2f, ty, sf);
                        }
                        ty += 24f;
                    }
                }

                if (done)
                    using (var f3 = new Font("Consolas", 9, FontStyle.Italic))
                    using (var br = new SolidBrush(Color.FromArgb(60, 100, 120, 160)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString("[ press any key ]", f3, br, W / 2f, H - 55, sf);
                    }
            };

            screen.KeyDown += (s, e) =>
            { if (done || e.KeyCode == Keys.Escape) { timer.Stop(); screen.Close(); this.Close(); } };
            screen.MouseClick += (s, e) =>
            { if (done) { timer.Stop(); screen.Close(); this.Close(); } };

            timer.Start();
            screen.Show();
        }

        // ════════════════════════════════════════════════════════════════
        //  ОТРИСОВКА КНОПКИ ESCAPE
        //  Добавить в Form1.cs → Redraw(), после DrawObserverPassButton(g):
        //      DrawEscapeButton(g);
        // ════════════════════════════════════════════════════════════════
        internal void DrawEscapeButton(Graphics g)
        {
            if (!EscapeFlags.HasNinasNote || EscapeFlags.ConfirmedEscape) return;
            if (currentCharacterData != null) return;

            Rectangle zone = ScaleRect(escapeButtonZoneVisible);
            bool hovered = zone.Contains(PointToClient(Cursor.Position));

            Color btnColor = EscapeFlags.SuspicionScore >= 70
                ? Color.FromArgb(220, 60, 60)
                : EscapeFlags.SuspicionScore >= 40
                    ? Color.FromArgb(200, 150, 40)
                    : Color.FromArgb(60, 140, 220);

            using (var br = new SolidBrush(hovered
                ? Color.FromArgb(180, btnColor.R / 4, btnColor.G / 4, btnColor.B / 4)
                : Color.FromArgb(110, btnColor.R / 5, btnColor.G / 5, btnColor.B / 5)))
                g.FillRectangle(br, zone);

            using (var pen = new Pen(hovered
                ? Color.FromArgb(255, btnColor)
                : Color.FromArgb(180, btnColor), hovered ? 2f : 1.5f))
                g.DrawRectangle(pen, zone);

            if (EscapeFlags.SuspicionScore > 0)
            {
                int barW = (int)(zone.Width * 0.8f * EscapeFlags.SuspicionScore / 100f);
                using (var br = new SolidBrush(Color.FromArgb(55, btnColor)))
                    g.FillRectangle(br, zone.X + zone.Width / 10, zone.Bottom - 10, barW, 4);
            }

            string label = hovered ? "[ ⟶ ESCAPE ]" : "[ ESCAPE ]";
            using (var font = new Font("Consolas", 11, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(hovered ? 230 : 180,
                hovered ? Color.White : btnColor)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(label, font, brush,
                    new RectangleF(zone.X, zone.Y, zone.Width, zone.Height - 14), sf);
            }

            using (var f2 = new Font("Consolas", 7.5f))
            using (var br2 = new SolidBrush(Color.FromArgb(130, btnColor)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString($"risk {EscapeFlags.SuspicionScore}%", f2, br2,
                    zone.X + zone.Width / 2f, zone.Bottom - 13, sf);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ОБРАБОТЧИК КЛИКА
        //  Добавить в Form1_Input.cs → Form_MouseClick_New(),
        //  В начало метода ПЕРЕД "if (overlayPanel.Visible) return;":
        //      if (TryHandleEscapeClick(p)) return;
        // ════════════════════════════════════════════════════════════════
        internal bool TryHandleEscapeClick(Point p)
        {
            if (!EscapeFlags.HasNinasNote || EscapeFlags.ConfirmedEscape) return false;
            if (currentCharacterData != null) return false;
            if (ScaleRect(escapeButtonZoneVisible).Contains(p))
            {
                HandleEscapeButtonClick();
                return true;
            }
            return false;
        }

        // ════════════════════════════════════════════════════════════════
        //  TOAST УВЕДОМЛЕНИЕ
        // ════════════════════════════════════════════════════════════════
        private void ShowEscapeToast(string title, string body, Color accent)
        {
            var toast = new Panel
            {
                Size = new Size(380, 90),
                BackColor = Color.FromArgb(230, 8, 14, 22),
                Cursor = Cursors.Hand,
                Location = new Point(this.ClientSize.Width - 395, this.ClientSize.Height - 110)
            };
            toast.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(180, accent), 1))
                    g.DrawRectangle(pen, 0, 0, toast.Width - 1, toast.Height - 1);
                using (var br = new SolidBrush(accent))
                    g.FillRectangle(br, 0, 0, 4, toast.Height);
                using (var f1 = new Font("Consolas", 8f, FontStyle.Bold))
                using (var br = new SolidBrush(Color.FromArgb(200, accent)))
                    g.DrawString("⟶ " + title, f1, br, 12, 10);
                using (var f2 = new Font("Consolas", 8.5f))
                using (var br = new SolidBrush(Color.FromArgb(190, 210, 230)))
                    g.DrawString(body, f2, br, new RectangleF(12, 30, toast.Width - 20, 52));
            };
            this.Controls.Add(toast);
            toast.BringToFront();
            toast.Click += (s, e) => { this.Controls.Remove(toast); toast.Dispose(); };
            var t = new Timer { Interval = 5000 };
            t.Tick += (s, e) =>
            {
                t.Stop();
                if (!toast.IsDisposed) { this.Controls.Remove(toast); toast.Dispose(); }
            };
            t.Start();
        }

        // ════════════════════════════════════════════════════════════════
        //  ВСПОМОГАТЕЛЬНЫЕ
        // ════════════════════════════════════════════════════════════════
        private Form CreateEscapeDialog(int width, int height, string headerText, Color accent)
        {
            var dlg = new Form
            {
                Size = new Size(width, height),
                BackColor = Color.FromArgb(10, 14, 20),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                TopMost = true,
                ShowInTaskbar = false
            };
            dlg.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(160, accent), 1))
                    g.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
                using (var br = new LinearGradientBrush(
                    new Rectangle(0, 0, dlg.Width, 3),
                    Color.FromArgb(200, accent), Color.Transparent,
                    LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, dlg.Width, 3);
            };
            dlg.Controls.Add(new Label
            {
                Text = headerText,
                Location = new Point(16, 12),
                Size = new Size(width - 32, 22),
                ForeColor = Color.FromArgb(220, accent),
                Font = new Font("Consolas", 9, FontStyle.Bold),
                BackColor = Color.Transparent
            });
            dlg.Controls.Add(new Label
            {
                Location = new Point(16, 36),
                Size = new Size(width - 32, 1),
                BackColor = Color.FromArgb(60, accent)
            });
            dlg.KeyDown += (s, e) =>
            { if (e.KeyCode == Keys.Escape) { dlg.Close(); pressureTimer.Start(); } };
            return dlg;
        }

        private void AddEscapeLabel(Form dlg, string text, int x, int y, int w, int h, Color color)
        {
            dlg.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                ForeColor = color,
                Font = new Font("Consolas", 9),
                BackColor = Color.Transparent
            });
        }

        private void AddEscapeButton(Form dlg, string text, Color fg, Color bg,
            int x, int y, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(dlg.Width - x * 2, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Consolas", 8, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(
                Math.Max(0, fg.R / 5), Math.Max(0, fg.G / 5), Math.Min(255, fg.B / 5 + 15));
            btn.Click += (s, e) => { dlg.Close(); onClick(); };
            dlg.Controls.Add(btn);
        }
    }
}

/*
════════════════════════════════════════════════════════════════════════════
  6 МИНИМАЛЬНЫХ ИЗМЕНЕНИЙ В ДРУГИХ ФАЙЛАХ
════════════════════════════════════════════════════════════════════════════

① Form1.cs → Redraw(), после DrawObserverPassButton(g):
    DrawEscapeButton(g);

② Form1.cs → LoadCurrentCharacter(), после:
    currentCharacterData = todayCast[currentCharacterIndex];
   добавить:
    EndingTracker.WolfQuestionsThisVisit = 0;

③ Form1.cs → InitModeSession(), case GameMode.StoryMode:,
   после EndingTracker.Reset():
    EscapeFlags.Reset();

④ Form1_Input.cs → Form_MouseClick_New(),
   В начало метода ПЕРЕД "if (overlayPanel.Visible) return;":
    if (TryHandleEscapeClick(p)) return;

⑤ Form1_Input.cs → Form_MouseMove_New(),
   В "bool onActive = ..." добавить:
    || (EscapeFlags.HasNinasNote && !EscapeFlags.ConfirmedEscape
        && currentCharacterData == null
        && ScaleRect(escapeButtonZoneVisible).Contains(e.Location))

⑥ SecretDocumentSystem.cs → ReceiveDocument(), после _documentVault.Add(doc):
    if (doc.Id == "nina_note" && storyModeActive)
        OnNinasNoteReceived();
    if (doc.Stamp == "REBEL" && storyModeActive && EscapeFlags.HasNinasNote)
        EscapeFlags.AddSuspicion(3, "Stored rebel document");
════════════════════════════════════════════════════════════════════════════
*/
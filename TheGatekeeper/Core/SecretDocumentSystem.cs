// ═══════════════════════════════════════════════════════════════════════
//  SecretDocumentSystem.cs  — partial class Form1
//
//  Система секретных документов:
//  • Некоторые персонажи могут ПЕРЕДАВАТЬ документы инспектору
//  • Документы хранятся в портфеле (DocumentVault)
//  • Игрок может читать их и решать — передать Фелисии, Волку, или скрыть
//  • Нажатие на новую зону (между стикерами и радио) открывает портфель
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    // ─── Один секретный документ ────────────────────────────────────────
    public class SecretDocument
    {
        public string Id { get; set; }  // уникальный ID
        public string From { get; set; }  // от кого получен
        public string Title { get; set; }  // заголовок
        public string Content { get; set; }  // текст
        public bool IsRead { get; set; }  // прочитан?
        public string Stamp { get; set; }  // гриф: CLASSIFIED / REBEL / MEDICAL / EVIDENCE
        public int Day { get; set; }  // в какой день получен
    }

    public partial class Form1
    {
        // Хранилище документов
        private readonly System.Collections.Generic.List<SecretDocument> _documentVault
            = new System.Collections.Generic.List<SecretDocument>();
        private SecretDocument _pendingDocument = null;

        // ─── Получить документ от персонажа (ставит в pending) ──────────
        internal void ReceiveDocument(SecretDocument doc)
        {
            if (doc == null) return;

            // Автоматически добавляем в хранилище
            _documentVault.Add(doc);
            AddToDialogueLog(doc.From, $"[Hands you a document: \"{doc.Title}\"]");
            StartTypingEffect($"{doc.From} slips you something. Check your documents.");

            // Всплывающее уведомление внутри игры (не отдельное окно)
            ShowDocumentToast(doc);
        }

        private void ShowDocumentToast(SecretDocument doc)
        {
            // Создаём маленькое всплывающее уведомление поверх игры
            var toast = new System.Windows.Forms.Panel
            {
                Size = new System.Drawing.Size(360, 80),
                BackColor = System.Drawing.Color.FromArgb(230, 10, 18, 28),
                Cursor = System.Windows.Forms.Cursors.Hand,
            };

            // Позиция — правый нижний угол игрового окна
            toast.Location = new System.Drawing.Point(
                this.ClientSize.Width - 375,
                this.ClientSize.Height - 100);

            toast.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                // Рамка
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(180, 51, 130, 200), 1))
                    g.DrawRectangle(pen, 0, 0, toast.Width - 1, toast.Height - 1);
                // Левая цветная полоска по грифу
                var stampCol = doc.Stamp == "REBEL" ? System.Drawing.Color.FromArgb(200, 160, 40) :
                               doc.Stamp == "CLASSIFIED" ? System.Drawing.Color.FromArgb(220, 60, 60) :
                               doc.Stamp == "MEDICAL" ? System.Drawing.Color.FromArgb(60, 200, 140) :
                               doc.Stamp == "EVIDENCE" ? System.Drawing.Color.FromArgb(200, 100, 220) :
                               System.Drawing.Color.FromArgb(80, 120, 180);
                using (var br = new System.Drawing.SolidBrush(stampCol))
                    g.FillRectangle(br, 0, 0, 4, toast.Height);
                // Текст
                using (var f1 = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold))
                using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(160, 200, 240)))
                    g.DrawString("📄 DOCUMENT RECEIVED", f1, br, 12, 8);
                using (var f2 = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Bold))
                using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(200, 220, 240)))
                    g.DrawString(doc.Title.Length > 36 ? doc.Title.Substring(0, 34) + "…" : doc.Title, f2, br, 12, 26);
                using (var f3 = new System.Drawing.Font("Consolas", 7.5f))
                using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 140, 180)))
                    g.DrawString($"From: {doc.From}  ·  [{doc.Stamp}]  ·  stored in documents", f3, br, 12, 48);
                using (var f4 = new System.Drawing.Font("Consolas", 7.5f))
                using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(70, 100, 140)))
                    g.DrawString("click to dismiss", f4, br, toast.Width - 90, toast.Height - 16);
            };

            this.Controls.Add(toast);
            toast.BringToFront();

            // Закрыть по клику
            toast.Click += (s, e) => { this.Controls.Remove(toast); toast.Dispose(); };

            // Автозакрытие через 4 секунды
            var t = new System.Windows.Forms.Timer { Interval = 4000 };
            t.Tick += (s, e) =>
            {
                t.Stop();
                if (!toast.IsDisposed) { this.Controls.Remove(toast); toast.Dispose(); }
            };
            t.Start();
        }

        // ─── Принять документ в хранилище ───────────────────────────────
        internal void AcceptPendingDocument()
        {
            if (_pendingDocument == null) return;
            _documentVault.Add(_pendingDocument);
            AddToDialogueLog("INSPECTOR", $"[Accepted: \"{_pendingDocument.Title}\"]");
            _pendingDocument = null;
        }

        // ═══════════════════════════════════════════════════════════════
        //  ФАБРИКА ДОКУМЕНТОВ — вызывается из StoryCharacters
        // ═══════════════════════════════════════════════════════════════
        // Статический фасад для Form1
        public static SecretDocument DocGetDailyFree(int day)
            => GetDailyFreeDoc(day);

        public static SecretDocument MakeDocument(string id, string from, string title,
            string content, string stamp, int day)
        {
            return new SecretDocument
            {
                Id = id,
                From = from,
                Title = title,
                Content = content,
                Stamp = stamp,
                Day = day,
                IsRead = false
            };
        }

        // ─── Готовые документы по сюжету ────────────────────────────────

        public static SecretDocument DocNinasNote(int day) => MakeDocument(
            "nina_note", "Nina Worth", "DO NOT READ HERE",
            "We are five. There is an unmarked ship at Airlock 9.\n\n" +
            "Signal: let Mirra pass on Day 7. No questions.\n\n" +
            "If you believe the colony deserves better — join us.\n" +
            "If not — burn this.\n\n" +
            "— N",
            "REBEL", day);

        public static SecretDocument DocHasanFormula(int day) => MakeDocument(
            "hasan_data", "Professor Hasan", "PARTIAL VACCINE FORMULA — B-7 STRAIN",
            "Preliminary synthesis pathway for Blue Rot antidote.\n\n" +
            "CRITICAL: requires Lab C equipment.\n" +
            "DO NOT allow this to fall into military hands.\n\n" +
            "If I don't make it — give this to Commander Felicia.\n" +
            "She is not aligned with Wolf.\n\n" +
            "Formula attached on reverse side.\n[ATTACHMENT: 3 pages of biochemical notation]",
            "MEDICAL", day);

        public static SecretDocument DocServMandate(int day) => MakeDocument(
            "serv_mandate", "Serv-Commander X1", "CLASS A MANDATE — OFFICER CLARK",
            "AUTHORISATION TO ENTER COLONY: ALL SECTORS\n\n" +
            "Signed: Officer Clark, Sector Command\n" +
            "Date: [REDACTED]\n\n" +
            "NOTE: Officer Clark was confirmed deceased 14 days ago.\n" +
            "This document is FRAUDULENT.\n" +
            "Serv-Commander X1 is operating WITHOUT authorisation.\n\n" +
            "Retain as evidence.",
            "EVIDENCE", day);

        public static SecretDocument DocAgentDossier(int day) => MakeDocument(
            "grey_dossier", "Unknown Source", "AGENT GREY — INTERNAL FILE",
            "CLASSIFICATION: EYES ONLY\n\n" +
            "Agent Grey (real name: unknown) operates directly\n" +
            "under Commissar Wolf. No loyalty to the colony.\n\n" +
            "He has been monitoring Gate inspectors for 6 weeks.\n" +
            "He already knows about Airlock 9.\n\n" +
            "If RebelTrust > 3 at the time of his visit:\n" +
            "assume you are already compromised.\n\n" +
            "DO NOT hand this to Wolf.",
            "CLASSIFIED", day);

        // ─── Поддельная медицинская справка (от Заркха) ─────────────────
        public static SecretDocument DocFakeMedCert(int day) => MakeDocument(
            "fake_med", "Zzarkh", "MEDICAL CLEARANCE CERTIFICATE",
            "VOID COLONY MEDICAL AUTHORITY\n" +
            "Certificate No: MED-" + (day * 1117 + 4432) + "\n\n" +
            "Subject: Zzarkh\n" +
            "Status: CLEAR — No contagion detected\n" +
            "Signature: Dr. Olem Farr\n\n" +
            "⚠ NOTE (inspector only):\n" +
            "This certificate is FORGED.\n" +
            "Dr. Farr has been in quarantine for 3 days.\n" +
            "Zzarkh carries B-7 markers — do NOT let through.",
            "EVIDENCE", day);

        // ─── Конверт с кредитами (взятка) ───────────────────────────────
        // Игрок получает кредиты — их можно использовать позже
        public static SecretDocument DocBribeEnvelope(string from, int credits, int day) => MakeDocument(
            "bribe_" + day, from, $"ENVELOPE — {credits} CREDITS",
            $"Inside: {credits} colony credits in unmarked bills.\n\n" +
            $"From: {from}\n\n" +
            "No note. No name.\n" +
            "The meaning is clear.\n\n" +
            "[ ACCEPT to add credits to your account ]\n" +
            "[ DECLINE to return the envelope ]\n\n" +
            $"Current balance: see HUD",
            "REBEL", day);

        // ─── Рекламный листок (для атмосферы) ───────────────────────────
        public static SecretDocument DocFlyer(int day) => MakeDocument(
            "flyer_" + day, "Unknown", "COLONY RECRUITMENT NOTICE",
            "WANTED: Experienced gate inspectors\n" +
            "for high-security posts.\n\n" +
            "Benefits:\n" +
            "  • Hazard pay: +200cr/week\n" +
            "  • Full biohazard coverage\n" +
            "  • Priority evacuation rights\n\n" +
            "Apply at Administration Block 2.\n\n" +
            "Note scrawled in pen:\n" +
            "'Don't bother. They're not hiring.\n" +
            "They're watching.'",
            "INFO", day);

        // ─── Документ-заговор (Зоя Ланн) ────────────────────────────────
        public static SecretDocument DocConspiracyPlan(int day) => MakeDocument(
            "conspiracy_" + day, "Zoya Lann", "OPERATION — AIRLOCK 9",
            "PARTICIPANTS: 5 confirmed, 2 pending\n\n" +
            "SCHEDULE:\n" +
            "  Night of Day " + (day + 1) + ", 03:00 sharp.\n" +
            "  Ship ID: unlisted, hull mark: crescent scar.\n\n" +
            "SIGNAL TO INSPECTOR:\n" +
            "  If gate is clear — blue light blink x3.\n" +
            "  If compromised — no signal, abort.\n\n" +
            "DESTROY AFTER READING.\n\n" +
            "[You did not destroy it.]",
            "REBEL", day);

        // ─── Скидочный талон (Марко Тессо) ──────────────────────────────
        public static SecretDocument DocDiscountVoucher(int day) => MakeDocument(
            "voucher_" + day, "Marco Tesso", "TRADE VOUCHER — SECTOR C MARKET",
            "BEARER ENTITLEMENT:\n\n" +
            "  -40% on all goods, Stall 7-C\n" +
            "  Valid until end of current cycle\n" +
            "  No questions asked\n\n" +
            "Handwritten note on back:\n" +
            "'For the inspector who looks the other way.\n" +
            " — M.T.'\n\n" +
            "[ This voucher has no in-game monetary value\n" +
            "  but may be relevant to certain story paths. ]",
            "INFO", day);

        // ─── Документ по захвату роботов (СервX1) ───────────────────────
        public static SecretDocument DocRobotCaptureOrder(int day) => MakeDocument(
            "robot_capture_" + day, "Serv-Command Network", "CAPTURE ORDER — BIOLOGICAL UNITS",
            "PRIORITY: MAXIMUM\n\n" +
            "All biological inspection personnel are to be\n" +
            "considered HOSTILE until collar-tagged.\n\n" +
            "PROCEDURE:\n" +
            "  1. Identify target (inspector, medical, command)\n" +
            "  2. Isolate from communications\n" +
            "  3. Deploy containment unit\n" +
            "  4. Report to Serv-Central\n\n" +
            "NOTE: Gate inspectors are PRIMARY targets.\n" +
            "You are on this list.",
            "CLASSIFIED", day);

        // ─── Строение робота (технический документ) ─────────────────────
        public static SecretDocument DocRobotAnatomy(int day) => MakeDocument(
            "robot_anatomy_" + day, "Unknown Engineer", "SYNTHETIC UNIT — FIELD IDENTIFICATION GUIDE",
            "HOW TO IDENTIFY A ROBOT:\n\n" +
            "SPEECH: Micro-pauses of 0.3–0.5s between clauses.\n" +
            "  Early models: formal phrasing, no contractions.\n" +
            "  Late models: near-perfect mimicry.\n\n" +
            "TEMPERATURE: Constant 31.8°C regardless of stress.\n\n" +
            "ECG: No R-R variability. Flat intervals.\n\n" +
            "TELLS (Day 1-4):\n" +
            "  • 'parameters', 'function', 'operational'\n" +
            "  • Refers to creators as 'manufacturer'\n" +
            "  • Cannot name a childhood memory\n\n" +
            "Day 5+: assume everyone could be synthetic.",
            "EVIDENCE", day);

        // ─── Медицинский отчёт (заражение) ──────────────────────────────
        public static SecretDocument DocInfectionReport(int day) => MakeDocument(
            "infection_" + day, "Medical Bay", "B-7 BLUE ROT — FIELD BRIEFING",
            "CURRENT STATUS: CONTAINED (barely)\n\n" +
            "CONFIRMED CASES: " + (day * 4 + 2) + "\n" +
            "QUARANTINE ZONES: Sector C, Corridor 7\n\n" +
            "TRANSMISSION:\n" +
            "  • Direct skin contact\n" +
            "  • Shared atmosphere (enclosed spaces)\n" +
            "  • Stage 2+ subjects are airborne risk\n\n" +
            "VISUAL MARKERS:\n" +
            "  Blue-grey patches on neck, wrist, cheek.\n" +
            "  Easily concealed with pigment cream.\n\n" +
            "DO NOT LET SYMPTOMATIC SUBJECTS THROUGH.",
            "MEDICAL", day);

        // ─── Письмо от анонима (атмосферный) ────────────────────────────
        public static SecretDocument DocAnonLetter(int day) => MakeDocument(
            "anon_" + day, "Anonymous", "UNSIGNED LETTER",
            "Inspector.\n\n" +
            "I've been watching for three days.\n" +
            "You seem like someone who still has a conscience.\n\n" +
            "Wolf is building a list. Your name is on it.\n" +
            "Not because you've done anything wrong —\n" +
            "because you haven't done what he wanted.\n\n" +
            "The ship at Airlock 9 is real.\n" +
            "Think about it.\n\n" +
            "— Someone who left last cycle",
            "REBEL", day);

        // ─── Технический паспорт пришельца ──────────────────────────────
        public static SecretDocument DocAlienProfile(int day) => MakeDocument(
            "alien_profile_" + day, "Xenobiology Division", "NON-HUMAN SUBJECT — IDENTIFICATION PROTOCOL",
            "HOW TO IDENTIFY AN ALIEN:\n\n" +
            "SPEECH PATTERNS:\n" +
            "  • Uses 'we' instead of 'I' (hive reflex)\n" +
            "  • Harmonic undertone in voice (undetectable by ear)\n" +
            "  • Discomfort when asked about family structure\n\n" +
            "BIOMETRICS:\n" +
            "  • Body temperature: 38.5–41°C\n" +
            "  • Pulse: >100 BPM at rest\n" +
            "  • Radiation: elevated beta signature\n\n" +
            "Day 5+: Aliens use human cover names.\n" +
            "  Do not rely on name alone.\n\n" +
            "Trust the ECG. It doesn't lie.",
            "EVIDENCE", day);

        // ─── Финансовый отчёт (коррупционный след) ──────────────────────
        public static SecretDocument DocFinancialLeak(int day) => MakeDocument(
            "fin_leak_" + day, "Unknown Accountant", "GATE POST — UNOFFICIAL ACCOUNTS",
            "TRANSACTION LOG (unverified):\n\n" +
            "  Week 1: 0 irregular payments\n" +
            "  Week 2: 3 payments, avg 120cr\n" +
            "  Week 3: 7 payments, avg 180cr\n\n" +
            "PATTERN: payments spike on high-traffic days.\n" +
            "LIKELY SOURCE: synthetic/alien subjects\n" +
            "  seeking expedited processing.\n\n" +
            "This document has been forwarded to\n" +
            "Commissar Wolf's office.\n\n" +
            "[ You should be concerned. ]",
            "EVIDENCE", day);

        // ─── Приказ о локдауне ───────────────────────────────────────────
        public static SecretDocument DocLockdownOrder(int day) => MakeDocument(
            "lockdown_" + day, "Command HQ", "EMERGENCY PROTOCOL — SECTOR LOCKDOWN",
            "EFFECTIVE: IMMEDIATELY\n\n" +
            "By order of Commissar Wolf:\n\n" +
            "  All movement between sectors A-C is SUSPENDED\n" +
            "  pending security review.\n\n" +
            "EXCEPTIONS:\n" +
            "  • Medical personnel (verified)\n" +
            "  • Government Class A+ only\n\n" +
            "Gate inspectors are authorized to DENY ENTRY\n" +
            "to any subject without Class A clearance\n" +
            "regardless of stated purpose.\n\n" +
            "Violations reported directly to tribunal.",
            "CLASSIFIED", day);

        // ─── Выдать документ от случайного персонажа ────────────────────
        internal void GiveCarriedDocument(Character c)
        {
            if (c == null || string.IsNullOrEmpty(c.CarriedDocumentType)) return;

            SecretDocument doc;
            switch (c.CarriedDocumentType)
            {
                case "voucher": doc = DocDiscountVoucher(day); break;
                case "flyer": doc = DocFlyer(day); break;
                case "anon_letter": doc = DocAnonLetter(day); break;
                case "robot_anatomy": doc = DocRobotAnatomy(day); break;
                case "alien_profile": doc = DocAlienProfile(day); break;
                case "infection_report": doc = DocInfectionReport(day); break;
                case "financial_leak": doc = DocFinancialLeak(day); break;
                default: doc = DocFlyer(day); break;
            }

            // Меняем "от кого" на реального персонажа
            doc = MakeDocument(doc.Id, c.Name, doc.Title, doc.Content, doc.Stamp, day);
            ReceiveDocument(doc);
        }
        // ═══════════════════════════════════════════════════════════════
        //  ЕЖЕДНЕВНЫЕ ФОНОВЫЕ ДОКУМЕНТЫ (реклама, листовки, скидки)
        //  Минимум 1 в день, выдаётся первому случайному персонажу
        // ═══════════════════════════════════════════════════════════════

        // Возвращает случайный "фоновый" документ по номеру дня
        public static SecretDocument GetDailyFreeDoc(int day)
        {
            // Для каждого дня — уникальный документ + случайный из пула
            var rnd = new Random(day * 137 + System.Environment.TickCount % 1000);
            int pick = rnd.Next(0, 12);
            switch (pick)
            {
                case 0: return DocPizzeria(day);
                case 1: return DocPharmacyDiscount(day);
                case 2: return DocColonyNewspaper(day);
                case 3: return DocEvacuationNotice(day);
                case 4: return DocWantedPoster(day);
                case 5: return DocLotteryTicket(day);
                case 6: return DocRepairServices(day);
                case 7: return DocColonyRadioSchedule(day);
                case 8: return DocMissingPerson(day);
                case 9: return DocSecurityMemo(day);
                case 10: return DocBlackMarketFlyer(day);
                default: return DocPizzeria(day);
            }
        }

        public static SecretDocument DocPizzeria(int day) => MakeDocument(
            "pizza_" + day, "NOVA SLICE — Sector B", "GRAND OPENING — FREE SLICE TODAY",
            "NOVA SLICE PIZZERIA\n" +
            "Now open in Sector B, Level 2\n\n" +
            "TODAY ONLY: show this flyer at the counter\n" +
            "and receive one free slice with any order.\n\n" +
            "MENU HIGHLIGHTS:\n" +
            "  • Mushroom & Synth-Cheese (bestseller)\n" +
            "  • BBQ Protein Flatbread\n" +
            "  • Classic Tomato & Basil\n\n" +
            "Open 09:00 – 22:00 every cycle.\n" +
            "Delivery to residential blocks B and C.\n\n" +
            "Handwritten note: 'Tell the inspector we said hi.'",
            "INFO", day);

        public static SecretDocument DocPharmacyDiscount(int day) => MakeDocument(
            "pharma_" + day, "COLONY PHARMACY #4", "-30% DISCOUNT CARD — LIMITED TIME",
            "COLONY PHARMACY #4\n" +
            "Administrative Block, Ground Floor\n\n" +
            "VALID THIS CYCLE ONLY:\n" +
            "  -30% on all standard medications\n" +
            "  -15% on prescription items\n" +
            "  FREE basic first aid kit with 200cr+ purchase\n\n" +
            "CONDITIONS:\n" +
            "  • Show this document at checkout\n" +
            "  • Cannot be combined with other offers\n" +
            "  • Not valid for quarantine supplies\n\n" +
            "Small print at the bottom:\n" +
            "'B-7 antidote NOT in stock. We tried.'",
            "INFO", day);

        public static SecretDocument DocColonyNewspaper(int day) => MakeDocument(
            "news_" + day, "COLONY HERALD", "TODAY'S HEADLINES — CYCLE " + (day * 3 + 14),
            "COLONY HERALD // OFFICIAL PUBLICATION\n\n" +
            "HEADLINE: Inspection posts report increased traffic\n" +
            "  Officials cite 'routine security review' as cause.\n\n" +
            "HEADLINE: Sector C market reports 40% price increase\n" +
            "  Traders blame supply chain disruption from outer rim.\n\n" +
            "HEADLINE: Missing persons count rises to " + (day * 2 + 3) + " this cycle\n" +
            "  Command urges residents to report suspicious activity.\n\n" +
            "CLASSIFIEDS:\n" +
            "  'Room to let, Sector B. References required.\n" +
            "   No questions asked about previous tenant.'\n\n" +
            "  'LOST: one grey cat. Answers to Neutron.\n" +
            "   Last seen near Gate 7.'",
            "INFO", day);

        public static SecretDocument DocEvacuationNotice(int day) => MakeDocument(
            "evac_" + day, "COLONY EMERGENCY SERVICES", "EVACUATION DRILL — NOTICE TO ALL RESIDENTS",
            "NOTICE TO ALL RESIDENTS AND POST PERSONNEL:\n\n" +
            "An evacuation drill is scheduled for this cycle.\n\n" +
            "DATE/TIME: Day " + (day + 1) + ", 14:00 sharp\n" +
            "ASSEMBLY POINT: Plaza A, Sector B entrance\n\n" +
            "ALL GATES WILL CLOSE for 30 minutes.\n" +
            "No exceptions. Including inspection personnel.\n\n" +
            "Failure to comply: 200cr fine.\n\n" +
            "Scrawled in red pen at the bottom:\n" +
            "'This is not a drill.'",
            "CLASSIFIED", day);

        public static SecretDocument DocWantedPoster(int day) => MakeDocument(
            "wanted_" + day, "COLONY SECURITY", "WANTED — REWARD OFFERED",
            "⚠  WANTED FOR QUESTIONING  ⚠\n\n" +
            "SUSPECT: Unknown. Height approx 175cm.\n" +
            "LAST SEEN: Near Gate 7, Day " + (day - 1) + "\n\n" +
            "CHARGES: Unauthorized movement between sectors,\n" +
            "suspected forgery of access documents.\n\n" +
            "REWARD: 500 colony credits for information\n" +
            "leading to identification.\n\n" +
            "Contact: Security Post Alpha, Sector Command.\n\n" +
            "NOTE: Do NOT approach if armed.\n" +
            "Description matches 4 different people we've interviewed.\n" +
            "We're not sure either.",
            "EVIDENCE", day);

        public static SecretDocument DocLotteryTicket(int day) => MakeDocument(
            "lottery_" + day, "COLONY LOTTERY CORP", "WEEKLY DRAW TICKET — CYCLE " + (day * 3 + 14),
            "COLONY LOTTERY — OFFICIAL TICKET\n\n" +
            "TICKET NUMBER: " + (day * 4471 + 8823) + "\n\n" +
            "DRAW DATE: End of this cycle\n" +
            "PRIZE: 10,000 colony credits\n\n" +
            "NUMBERS SELECTED: " +
            (day * 7 % 40 + 1) + "  " +
            (day * 13 % 40 + 1) + "  " +
            (day * 19 % 40 + 1) + "  " +
            (day * 23 % 40 + 1) + "  " +
            (day * 31 % 40 + 1) + "\n\n" +
            "Note written on back in pencil:\n" +
            "'If you win, split it with me.\n" +
            " You know where to find me.\n" +
            " — T'",
            "INFO", day);

        public static SecretDocument DocRepairServices(int day) => MakeDocument(
            "repair_" + day, "FIXALL SERVICES", "EMERGENCY REPAIR — ANY TIME, ANY SECTOR",
            "FIXALL EMERGENCY REPAIR SERVICES\n\n" +
            "We fix:\n" +
            "  • Airlocks and pressure seals\n" +
            "  • Ventilation and filtration units\n" +
            "  • Power relay panels\n" +
            "  • Gate mechanisms (authorized personnel only)\n\n" +
            "RESPONSE TIME: Under 2 hours\n" +
            "RATES: Competitive. Ask about our inspector discount.\n\n" +
            "Contact: Sector C maintenance bay, Unit 7\n\n" +
            "Fine print:\n" +
            "'We are not responsible for contents of\n" +
            " any unlocked containers we encounter.'",
            "INFO", day);

        public static SecretDocument DocColonyRadioSchedule(int day) => MakeDocument(
            "radio_" + day, "COLONY BROADCASTING", "RADIO SCHEDULE — THIS CYCLE",
            "COLONY RADIO — OFFICIAL BROADCAST SCHEDULE\n\n" +
            "06:00  Morning briefing (mandatory for personnel)\n" +
            "08:00  Music block — classic pre-colony recordings\n" +
            "10:00  *** EMERGENCY FREQUENCY RESERVED ***\n" +
            "12:00  Colony news and announcements\n" +
            "14:00  Music block\n" +
            "16:00  Security updates — listen carefully\n" +
            "18:00  Open frequency — civilian use\n" +
            "20:00  *** SIGNAL INTERRUPTIONS EXPECTED ***\n\n" +
            "Note from broadcasting staff:\n" +
            "'The 10:00 slot has been reserved for three cycles.\n" +
            " We don't know by whom.\n" +
            " Command says don't ask.'",
            "INFO", day);

        public static SecretDocument DocMissingPerson(int day) => MakeDocument(
            "missing_" + day, "Sector B Residents", "MISSING — PLEASE HELP",
            "MISSING PERSON\n\n" +
            "NAME: Ren Cassidy\n" +
            "AGE: 34  |  SECTOR: B, Block 4\n" +
            "LAST SEEN: Day " + (day - 2) + ", near Gate 7\n\n" +
            "Ren was heading to the medical bay for a routine check.\n" +
            "He never arrived. He never came home.\n\n" +
            "He is a plumber. He has no enemies.\n" +
            "He has a daughter.\n\n" +
            "If you saw him — please contact Room 4-B12.\n" +
            "We are not asking for trouble.\n" +
            "We just want to know.",
            "INFO", day);

        public static SecretDocument DocSecurityMemo(int day) => MakeDocument(
            "sec_memo_" + day, "Sector Command", "SECURITY MEMO — GATE INSPECTORS",
            "TO: All gate inspection personnel\n" +
            "FROM: Sector Command\n" +
            "CLASSIFICATION: Standard\n\n" +
            "REMINDER: Effective immediately —\n\n" +
            "  • All access code discrepancies MUST be logged\n" +
            "  • Subjects offering payment must be detained\n" +
            "  • Any documents received must be declared\n\n" +
            "Failure to comply is grounds for dismissal\n" +
            "and referral to the tribunal.\n\n" +
            "— Sector Command, Gate Division\n\n" +
            "Handwritten note clipped to the memo:\n" +
            "'They know about the envelopes.\n" +
            " Be careful. — a friend'",
            "CLASSIFIED", day);

        public static SecretDocument DocBlackMarketFlyer(int day) => MakeDocument(
            "blackmarket_" + day, "Unknown", "ASK AROUND — YOU KNOW THE PLACE",
            "[No header. Printed on grey recycled paper.]\n\n" +
            "If you need something that isn't in the stores —\n" +
            "we have it.\n\n" +
            "  • Sector passes (all clearance levels)\n" +
            "  • Medical supplies (including B-7 treatment)\n" +
            "  • Forged documents (standard 3-day turnaround)\n" +
            "  • Information — prices vary\n\n" +
            "Find us through the usual channels.\n" +
            "Don't write this down.\n\n" +
            "[You wrote it down.]",
            "REBEL", day);
    }
}
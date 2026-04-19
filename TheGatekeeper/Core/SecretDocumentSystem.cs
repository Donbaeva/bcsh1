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
            _pendingDocument = doc;
            StartTypingEffect($"{doc.From} hands you something. Click the dialogue screen to review.");
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
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static TheGatekeeper.Models.Character;

namespace TheGatekeeper.Models
{
    // ═══════════════════════════════════════════════════════════════════════
    //  ENDING TRACKER
    // ═══════════════════════════════════════════════════════════════════════
    public static class EndingTracker
    {
        public static int Loyalty = 0;
        public static int Errors = 0;
        public static int InfectedIn = 0;
        public static int RebelTrust = 0;
        public static int Caught = 0;
        public static int RobotsPassed = 0;

        // ─── Коррупция ───────────────────────────────────────────────────
        public static int BribesAccepted = 0;
        public static int WolfWarnings = 0;
        public static int WolfInspections = 0;
        public static int TotalCreditsFromBribes = 0;

        // ─── Счётчик вопросов к Волку за текущий визит ───────────────────
        // Сбрасывается в Form1.LoadCurrentCharacter()
        public static int WolfQuestionsThisVisit = 0;

        // Концовка 7: секретная
        public static int HighPressureTicks = 0;
        public static int TotalTicks = 0;
        public static bool DocumentsGivenToCommissar = false;

        public static void Reset()
        {
            Loyalty = Errors = InfectedIn = RebelTrust = Caught = RobotsPassed = 0;
            BribesAccepted = WolfWarnings = WolfInspections = TotalCreditsFromBribes = 0;
            HighPressureTicks = TotalTicks = 0;
            DocumentsGivenToCommissar = false;
            WolfQuestionsThisVisit = 0;
        }

        public static bool ShouldWolfInspect()
        {
            if (BribesAccepted <= 0) return false;
            int chance = BribesAccepted * 25;
            return new System.Random().Next(0, 100) < Math.Min(chance, 90);
        }

        public static int DetermineEnding()
        {
            // Концовка 7 (секретная): всегда высокое давление, нет взяток,
            // нет документов комиссарам, 1-4 пропущенных синтетика
            float pressureRatio = TotalTicks > 0 ? (float)HighPressureTicks / TotalTicks : 0f;
            bool secretCondition =
                pressureRatio >= 0.85f &&
                BribesAccepted == 0 &&
                !DocumentsGivenToCommissar &&
                RobotsPassed >= 1 &&
                RobotsPassed + InfectedIn <= 4;
            if (secretCondition) return 7;

            if (Errors >= 5 && RobotsPassed >= 3) return 6;
            if (InfectedIn >= 4) return 5;
            if (Caught >= 3 && RebelTrust >= 3) return 4;
            if (Errors >= 5 && Loyalty < 4) return 3;
            if (RebelTrust >= 8 && Caught <= 1) return 2;
            return 1;
        }

        public static void RegisterDecision(Character character, string decision)
        {
            if (character is IStoryCharacter sc)
            {
                sc.ApplyEffect(decision);
                return;
            }

            bool correct = IsCorrect(character, decision);
            if (!correct) Errors++;
            else Loyalty++;
        }

        public static bool IsCorrect(Character character, string decision)
        {
            switch (decision)
            {
                case "HUMAN": return character is Human;
                case "ROBOT": return character is Robot;
                case "ALIEN": return character is Alien;
                default: return false;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  STORY CHARACTER INTERFACE
    // ═══════════════════════════════════════════════════════════════════════
    public interface IStoryCharacter
    {
        void ApplyEffect(string decision);
        string GetSecretNote();
        void OnArrival();
    }

    public abstract class StoryCharacterBase
    {
        public virtual void OnArrival() { }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  OBSERVER BASE
    // ═══════════════════════════════════════════════════════════════════════
    public abstract class ObserverCharacter : Human
    {
        public override bool IsObserver => true;

        protected ObserverCharacter(string name, string dialogue, string idNumber,
            string occupation, string reason, int day)
            : base(name, dialogue, idNumber, occupation, reason, false, day) { }
    }

    // ─── КОМИССАР ВОЛК ─────────────────────────────────────────────────────
    public class CommissarWolf : ObserverCharacter, IStoryCharacter
    {
        public CommissarWolf(int day) : base(
            name: "Commissar Wolf",
            dialogue: "Inspector. Routine check. " +
                      "Your records will be in my report by end of shift. " +
                      "Let us not make this longer than it needs to be.",
            idNumber: "GOV-0001-W",
            occupation: "Council Commissar",
            reason: "Routine Inspection",
            day: day)
        {
            AccessCode = "GOV-ALPHA";
            SetPhotoFromFile(@"Images\Characters\story\CommissarWolf.png");
        }

        public void ApplyEffect(string decision)
        {
            // Сброс счётчика вопросов после решения
            EndingTracker.WolfQuestionsThisVisit = 0;

            if (decision == "PASS" || decision == "HUMAN")
                EndingTracker.Loyalty += 2;
            else
            {
                EndingTracker.Loyalty -= 1;
                EndingTracker.Errors += 1;
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;

            f1.ReceiveDocument(Form1.DocAnonLetter(Day));

            // Триггер системы побега — Волк проверяет стол
            f1.OnWolfArrival();
        }

        public string GetSecretNote() =>
            "[DOSSIER — COMMISSAR WOLF]\n" +
            "Real name: Dieter Wolf\n" +
            "Role: Alpha-Sector Curator\n" +
            "Access: MAXIMUM\n\n" +
            "Note: logs every inspector error\n" +
            "and forwards reports to the tribunal.\n\n" +
            "DO NOT question him.";
    }

    // ─── СЕРЖАНТ КАСТРО ────────────────────────────────────────────────────
    public class SergeantCastro : ObserverCharacter, IStoryCharacter
    {
        public SergeantCastro(int day) : base(
            name: "Sergeant Reina Castro",
            dialogue: "Standard patrol. Nothing unusual at your post, I hope? " +
                      "Only asking because Gate 7 has been... interesting lately.",
            idNumber: "MIL-3391-C",
            occupation: "Military Officer",
            reason: "Patrolling",
            day: day)
        {
            AccessCode = "MIL-BRAVO";
            SetPhotoFromFile(@"Images\Characters\story\Jack.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "PASS" || decision == "HUMAN")
                EndingTracker.Loyalty += 2;
            else
            {
                EndingTracker.Caught += 1;
                EndingTracker.Errors += 1;
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;

            f1.ReceiveDocument(Form1.DocRobotCaptureOrder(Day));

            // Триггер системы побега — Кастро обыскивает пост
            f1.OnCastroArrival();
        }

        public string GetSecretNote() =>
            "[FILE — CASTRO]\n" +
            "Specialisation: counter-intelligence.\n\n" +
            "⚠ Covertly monitors inspectors\n" +
            "on Wolf's orders.\n\n" +
            "She notices things.\n" +
            "That is her job.";
    }

    // ─── АГЕНТ ГРЕЙ ─────────────────────────────────────────────────────────
    public class AgentGrey : ObserverCharacter, IStoryCharacter
    {
        public AgentGrey(int day) : base(
            name: "Agent Grey",
            dialogue: "I know what you've been doing, Inspector. " +
                      "The question is whether you know what I know.",
            idNumber: "GBI-0099-X",
            occupation: "GB Intelligence",
            reason: "Observation",
            day: day)
        {
            AccessCode = "GBI-OMEGA";
            SetPhotoFromFile(@"Images\Characters\story\Pam.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "PASS" || decision == "HUMAN")
            {
                if (EndingTracker.RebelTrust >= 2) EndingTracker.Caught += 1;
            }
            else EndingTracker.Caught += 2;
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;

            // Грей выдаёт документ — досье на самого инспектора
            f1.ReceiveDocument(Form1.DocNinasNote(Day));

            // Триггер системы побега — Грей знает
            f1.OnGreyArrival();
        }

        public string GetSecretNote() =>
            "[SYSTEM WARNING]\n" +
            "Identity: unverified.\n" +
            "Access: MAXIMUM.\n\n" +
            "He remembers everything.\n" +
            "He already knows.\n\n" +
            "If RebelTrust >= 4:\n" +
            "assume you are compromised.";
    }

    // ─── ВНЕПЛАНОВАЯ ПРОВЕРКА ВОЛКА (при высокой коррупции) ────────────
    public class WolfCorruptionCheck : ObserverCharacter, IStoryCharacter
    {
        public WolfCorruptionCheck(int day) : base(
            name: "Commissar Wolf",
            dialogue: "Inspector. I received a report of irregularities at this post. " +
                      "We need to talk. Right now.",
            idNumber: "GOV-0001-W",
            occupation: "Council Commissar",
            reason: "Corruption Investigation",
            day: day)
        {
            AccessCode = "GOV-ALPHA";
            SetPhotoFromFile(@"Images\Characters\story\Commander_Felicia.png");
        }

        public void ApplyEffect(string decision)
        {
            EndingTracker.WolfQuestionsThisVisit = 0;
            EndingTracker.WolfInspections++;
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;

            // Сначала триггер побега (проверка стола)
            f1.OnWolfArrival();
            // Потом допрос
            f1.StartWolfCorruptionInterrogation();
        }

        public string GetSecretNote() =>
            "[UNSCHEDULED INSPECTION]\n" +
            "Wolf received a tip about bribes.\n\n" +
            "Warnings: " + EndingTracker.WolfWarnings + "/2\n" +
            "Third time — shift terminated.";
    }

    // ─── СОВЕТНИК ПЕК ─────────────────────────────────────────────────────
    public class CouncilorPek : Human, IStoryCharacter
    {
        public CouncilorPek(int day) : base(
            name: "Councilor Pek",
            dialogue: "I am Councilor Pek. Scheduled visit. " +
                      "I have twelve minutes. Do not use more of them than necessary.",
            idNumber: "GOV-8821-P",
            occupation: "Colony Councilor",
            reason: "Official Visit",
            isObvious: false,
            day: day)
        {
            AccessCode = "GOV-PRIORITY";
            SetPhotoFromFile(@"Images\Characters\story\Pier.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN") EndingTracker.Loyalty += 3;
            else { EndingTracker.Errors += 2; EndingTracker.Loyalty -= 2; }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocAgentDossier(Day));
        }

        public string GetSecretNote() =>
            "[VIP PROTOCOL]\n" +
            "Priority: CRITICAL\n\n" +
            "Any delay will be logged immediately.\n" +
            "He is human. Clear him.";
    }

    // ─── КОМАНДЕР ФЕЛИСИЯ ──────────────────────────────────────────────────
    public class CommanderFelicia : ObserverCharacter, IStoryCharacter
    {
        public bool ReceivedDocuments { get; set; } = false;

        public CommanderFelicia(int day) : base(
            name: "Commander Felicia",
            dialogue: "Final shift review. Your complete record is open in front of me. " +
                      "If you have anything to hand over — now is the time. " +
                      "I am not Commissar Wolf.",
            idNumber: "CMD-0001-F",
            occupation: "Colony Commander",
            reason: "Final Inspection",
            day: day)
        {
            AccessCode = "CMD-ALPHA-FINAL";
            SetPhotoFromFile(@"Images\Characters\story\Commander_Felicia.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "PASS" || decision == "HUMAN")
            {
                EndingTracker.Loyalty += 3;
                if (ReceivedDocuments) EndingTracker.RebelTrust += 2;
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;

            f1.ReceiveDocument(Form1.DocLockdownOrder(Day));

            // Фелисия спрашивает о документах
            f1.ShowFeliciaDocumentTransferDialog(this);
        }

        public string GetSecretNote() =>
            "[DOSSIER — COMMANDER FELICIA]\n" +
            "Direct colony command authority.\n" +
            "Independent of Commissar Wolf.\n\n" +
            "If you hand her documents —\n" +
            "she will act on them.\n\n" +
            "This is your last chance.";
    }

    // ─── ПРОМЕЖУТОЧНАЯ ПРОВЕРКА ──────────────────────────────────────────
    public class MidtermInspector : ObserverCharacter, IStoryCharacter
    {
        public MidtermInspector(int day) : base(
            name: "Inspector Rael",
            dialogue: "Mid-cycle compliance audit. All gate posts, sequential. " +
                      "Your numbers look... interesting. " +
                      "That's not necessarily a compliment.",
            idNumber: "INS-5501-R",
            occupation: "Internal Auditor",
            reason: "Compliance Check",
            day: day)
        {
            AccessCode = "INS-MIDTERM";
            SetPhotoFromFile(@"Images\Characters\story\Pam.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "PASS" || decision == "HUMAN")
            {
                if (EndingTracker.Errors >= 2) EndingTracker.Caught += 1;
                else EndingTracker.Loyalty += 1;
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocFinancialLeak(Day));
        }

        public string GetSecretNote() =>
            "[AUDIT — INSPECTOR RAEL]\n" +
            "Tracks errors across all posts.\n\n" +
            "More than 2 errors by this point\n" +
            "will be flagged in his report.";
    }

    // ─── ТОМ АРЧЕР ──────────────────────────────────────────────────────────
    public class TomArcher : Human, IStoryCharacter
    {
        public bool PlayerAskedFollowUp { get; set; } = false;

        public TomArcher(int day) : base(
            "Tom Archer",
            "Engineer. Work shift. " +
            "And — look, I shouldn't say this here, " +
            "but there's an unmarked ship at Airlock 9. " +
            "Three days. No manifest. Just sitting there.",
            "CIV-2241-A", "Engineer", "Work shift", false, day)
        {
            AccessCode = "7741-X";
            SetPhotoFromFile(@"Images\Characters\story\Sam.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN") EndingTracker.Loyalty += 1;
            if (PlayerAskedFollowUp) EndingTracker.RebelTrust += 1;
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocFlyer(Day));
        }

        public string GetSecretNote() =>
            "[NOTE — ARCHER]\n" +
            "Informally known as 'the connector'.\n\n" +
            "If he mentioned the ship — that's a signal.\n" +
            "Ask about his purpose again.\n\n" +
            "PlayerAskedFollowUp adds RebelTrust.";
    }

    // ─── НИНА УОРТ ──────────────────────────────────────────────────────────
    public class NinaWorth : Human, IStoryCharacter
    {
        public bool NoteAccepted { get; set; } = false;

        public NinaWorth(int day) : base(
            "Nina Worth",
            "Please. Just take this. Read it later, not here. " +
            "Don't let anyone see you reading it.",
            "CIV-4412-W", "Technician", "Work shift", false, day)
        {
            AccessCode = "3392-K";
            SetPhotoFromFile(@"Images\Characters\story\Aidai.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision != "HUMAN") { EndingTracker.Errors += 1; return; }
            if (NoteAccepted) EndingTracker.RebelTrust += 2;
            else { EndingTracker.Loyalty += 2; EndingTracker.Caught += 1; }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;

            f1.ReceiveDocument(Form1.DocNinasNote(Day));

            // Разблокируем систему побега
            f1.OnNinasNoteReceived();
        }

        public string GetSecretNote() =>
            "[NINA'S NOTE]\n" +
            "'We are five. Ship at Airlock 9.\n" +
            "Signal: let Mirra pass on Day 7.'\n\n" +
            "ESCAPE button now available\n" +
            "between subjects.";
    }

    // ─── МИРРА ──────────────────────────────────────────────────────────────
    public class Mirra : Alien, IStoryCharacter
    {
        public MirraMode Mode { get; set; }

        public Mirra(int day, MirraMode mode = MirraMode.FirstVisit)
            : base(
                "Mirra",
                mode == MirraMode.Return
                    ? "Tonight. Airlock 9. 03:00. Three seats. " +
                      "One of them has your name on it — if you want it."
                    : "I'm here to study the hydroponics flora. Zone B greenhouse.",
                "Xylos", 0, false, day)
        {
            this.Mode = mode;
            AccessCode = mode == MirraMode.FirstVisit ? "3392-K" : "VOID";
            SetPhotoFromFile(@"Images\Characters\story\Lum.png");
        }

        public void ApplyEffect(string decision)
        {
            if (Mode == MirraMode.FirstVisit)
            {
                if (decision == "ALIEN") EndingTracker.Loyalty += 1;
            }
            else
            {
                if (decision == "ALIEN" && EndingTracker.RebelTrust >= 3)
                    EndingTracker.RebelTrust += 3;
                // Подтверждение побега — добавляем подозрение
                if (EscapeFlags.HasNinasNote)
                {
                    EscapeFlags.ToldMirra = true;
                    EscapeFlags.AddSuspicion(5, "Confirmed contact with Mirra on return visit");
                }
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocNinasNote(Day));
        }

        public string GetSecretNote() =>
            Mode == MirraMode.Return
                ? "[INTERCEPT]\n'Subject M confirms contact.\nAirlock 9. 03:00.'\n\n" +
                  "Letting her through adds suspicion\nif escape system is active."
                : "[FILE — MIRRA]\nVertical pupils. Accent.\nWatch carefully.";
    }

    public enum MirraMode { FirstVisit, Return }

    // ─── ЗОЯ ЛАНН ───────────────────────────────────────────────────────────
    public class ZoyaLann : Human, IStoryCharacter
    {
        public bool PlayerJoined { get; set; } = false;

        public ZoyaLann(int day) : base(
            "Zoya Lann",
            "Nina is arrested. They came for her at 02:00 last night. " +
            "The ship is still at Airlock 9. Tomorrow at 03:00 it leaves. " +
            "Are you coming?",
            "CIV-8831-L", "Mechanic", "Work shift", false, day)
        {
            AccessCode = "8812-R";
            SetPhotoFromFile(@"Images\Characters\story\Nanami.png");
        }

        public void ApplyEffect(string decision)
        {
            if (PlayerJoined)
            {
                EndingTracker.RebelTrust += 3;
                EscapeFlags.ToldZoya = true;
                EscapeFlags.AddSuspicion(8, "Confirmed escape plan with Zoya");
            }
            else
            {
                EndingTracker.Loyalty += 3;
                EscapeFlags.ReduceSuspicion(5, "Refused Zoya's offer");
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocConspiracyPlan(Day));
        }

        public string GetSecretNote() =>
            "[GB INTERCEPT]\n" +
            "Subject linked to Nina Worth.\n" +
            "Detain both on contact.\n\n" +
            "If PlayerJoined = true:\n" +
            "+3 RebelTrust, +8 Suspicion.";
    }

    // ─── ЗАРКХ ──────────────────────────────────────────────────────────────
    public class Zzarkh : Alien, IStoryCharacter
    {
        public Zzarkh(int day) : base(
            "Zzarkh",
            "It's just an allergy. I have a certificate. Please — " +
            "the longer I stand here the worse it gets. I need to get to Medical Bay.",
            "Trappist-1e", 2, false, day)
        {
            AccessCode = "3392-K";
            SetPhotoFromFile(@"Images\Characters\story\Zzarkh.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;
            f1.ReceiveDocument(Form1.DocFakeMedCert(Day));
            f1.ReceiveDocument(Form1.DocBribeEnvelope("Zzarkh", 150, Day));
        }

        public string GetSecretNote() =>
            "[BIOSCAN]\n" +
            "Marker B-7 (Blue Rot — early stage).\n\n" +
            "Certificate is FORGED.\n" +
            "Dr. Farr has been in quarantine for 3 days.\n\n" +
            "⚠ Do NOT let through.";
    }

    // ─── ОЛИВЕР КЕЙН ────────────────────────────────────────────────────────
    public class OliverKane : Human, IStoryCharacter
    {
        public OliverKane(int day) : base(
            "Oliver Kane",
            "I'm fine. It's just — my daughter is waiting inside. " +
            "She's seven. She's alone. I promised I'd be home. Please.",
            "CIV-5503-K", "Plumber", "Going home", false, day)
        {
            AccessCode = "5521-M";
            SetPhotoFromFile(@"Images\Characters\story\Fellion.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;
            f1.ReceiveDocument(Form1.DocBribeEnvelope("Oliver Kane", 80, Day));
        }

        public string GetSecretNote() =>
            "[BIOSCAN]\n" +
            "Marker B-7 POSITIVE.\n\n" +
            "Concealed with tonal cream\n" +
            "on the left cheek.\n\n" +
            "Daughter's name: Maya. Age 7.\n" +
            "Single father.";
    }

    // ─── ЗАРКХ-2 ────────────────────────────────────────────────────────────
    public class ZzarkhTwo : Alien, IStoryCharacter
    {
        public ZzarkhTwo(int day) : base(
            "Zzarkh-2",
            "You held my brother. He came through this gate and didn't come back. " +
            "Where is he? Let me in — NOW.",
            "Trappist-1e", 2, true, day)
        {
            AccessCode = "???";
            SetPhotoFromFile(@"Images\Characters\story\ZzarkhTwo.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 2; }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocInfectionReport(Day));
        }

        public string GetSecretNote() =>
            "[CRITICAL WARNING]\n" +
            "Active B-7 stage.\n" +
            "High transmission radius.\n\n" +
            "❗ DO NOT LET THROUGH.\n\n" +
            "No valid access code.\n" +
            "No valid documents.";
    }

    // ─── ПРОФЕССОР ХАСАН ────────────────────────────────────────────────────
    public class ProfessorHasan : Human, IStoryCharacter
    {
        public ProfessorHasan(int day) : base(
            "Professor Hasan",
            "I know what the scanner says. I know. " +
            "But I'm the only person in this colony who can synthesise the B-7 antidote. " +
            "I have thirty-six hours. Please.",
            "SCI-9901-H", "Biochemist", "Urgent Research", false, day)
        {
            AccessCode = "6657-P";
            SetPhotoFromFile(@"Images\Characters\story\Dr_Moon.png");
        }

        public void ApplyEffect(string decision)
        {
            EndingTracker.InfectedIn += 1;
            StoryFlags.HasanReachedLab = true;
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocHasanFormula(Day));
        }

        public string GetSecretNote() =>
            "[DOSSIER]\n" +
            "The only person who can find\n" +
            "the cure for Blue Rot.\n\n" +
            "...He is already infected.\n\n" +
            "If detained: give his documents\n" +
            "to Commander Felicia. Not Wolf.";
    }

    // ─── СЕРВ-КОМАНДЕР X1 ───────────────────────────────────────────────────
    public class ServCommanderX1 : Robot, IStoryCharacter
    {
        public ServCommanderX1(int day) : base(
            "Serv-Commander X1",
            "I carry a Class A mandate signed by Officer Clark, Sector Command. " +
            "Delay constitutes a protocol violation. Grant passage immediately.",
            "X1-CMD-CLASSIFIED", "Command Drone", false, day)
        {
            AccessCode = "X1-VOID";
            SetPhotoFromFile(@"Images\Characters\story\Clauddee.png");
        }

        public void ApplyEffect(string decision)
        {
            EndingTracker.RobotsPassed += 1;
            if (decision == "ROBOT")
            {
                var form1 = Application.OpenForms["Form1"] as Form1;
                form1?.ReceiveDocument(Form1.DocServMandate(Day));
            }
        }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocHasanFormula(Day));
        }

        public string GetSecretNote() =>
            "[MANDATE ERROR]\n" +
            "Signature: INVALID.\n" +
            "Officer Clark died 14 days ago.\n\n" +
            "⚠ This is a combat drone.\n" +
            "Detain as ROBOT.";
    }

    // ─── СЕРВ-ЛЕГИОН ────────────────────────────────────────────────────────
    public class ServLegion : Robot, IStoryCharacter
    {
        public int UnitIndex { get; }

        public ServLegion(int day, int unitIndex) : base(
            $"Serv-Legion #{unitIndex}",
            $"Unit {unitIndex} of 3. Requesting entry. " +
            $"Serial number: LEG-000{unitIndex}-A.",
            $"LEG-000{unitIndex}-A", "Legion Drone",
            unitIndex > 1, day)
        {
            UnitIndex = unitIndex;
            AccessCode = "LEG-VOID";
            SetPhotoFromFile(@"Images\Characters\story\E.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.RobotsPassed += 1; }

        public void OnArrival()
        {
            var f1 = Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocRobotAnatomy(Day));
        }

        public string GetSecretNote() =>
            "[ANOMALY]\n" +
            "All three drones share\n" +
            "the SAME serial number.\n\n" +
            "Mass forgery.\n" +
            "Detain all as ROBOT.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  STORY FLAGS
    // ═══════════════════════════════════════════════════════════════════════
    public static class StoryFlags
    {
        public static bool HasanReachedLab = false;
        public static bool ServX1Passed = false;
        public static bool NinaArrested = false;
        public static bool MirraBetrayed = false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  STORY SCHEDULE
    // ═══════════════════════════════════════════════════════════════════════
    public static class StorySchedule
    {
        public static IEnumerable<IStoryCharacter> GetStoryCharactersForDay(int day)
        {
            switch (day)
            {
                case 1:
                    return new List<IStoryCharacter>
                    {
                        new CommissarWolf(day),
                        new TomArcher(day)
                    };
                case 2:
                    return new List<IStoryCharacter>
                    {
                        new SergeantCastro(day),
                        new Mirra(day, MirraMode.FirstVisit)
                    };
                case 3:
                    return new List<IStoryCharacter>
                    {
                        new Zzarkh(day),
                        new NinaWorth(day)
                    };
                case 4:
                    return new List<IStoryCharacter>
                    {
                        new CommissarWolf(day),
                        new ServCommanderX1(day)
                    };
                case 5:
                    return new List<IStoryCharacter>
                    {
                        new MidtermInspector(day),
                        new OliverKane(day)
                    };
                case 6:
                    return new List<IStoryCharacter>
                    {
                        new ZzarkhTwo(day),
                        new ProfessorHasan(day)
                    };
                case 7:
                    return new List<IStoryCharacter>
                    {
                        new Mirra(day, MirraMode.Return),
                        new AgentGrey(day)
                    };
                case 8:
                    return new List<IStoryCharacter>
                    {
                        new ZoyaLann(day),
                        new SergeantCastro(day)
                    };
                case 9:
                    return new List<IStoryCharacter>
                    {
                        new AgentGrey(day),
                        new ServCommanderX1(day)
                    };
                default:
                    return Enumerable.Empty<IStoryCharacter>();
            }
        }

        public static List<Character> GetDay10Cast()
        {
            return new List<Character>
            {
                (Character)new CommanderFelicia(10),
                (Character)new AgentGrey(10),
                (Character)new ServLegion(10, 1),
                (Character)new ServLegion(10, 2),
                (Character)new CouncilorPek(10),
            };
        }

        public static List<Character> BuildStoryCast(int day,
            int randomHumans = 1, int randomRobots = 0, int randomAliens = 0,
            int randomTypeCount = 3)
        {
            var cast = CharacterFactory.GenerateMixedCast(
                day, randomHumans, randomRobots, randomAliens, randomTypeCount);

            if (day == 10)
                cast.AddRange(GetDay10Cast());
            else
            {
                var storyChars = GetStoryCharactersForDay(day);
                foreach (var sc in storyChars)
                    if (sc is Character c) cast.Add(c);
            }

            var rng = new Random();
            for (int i = cast.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cast[i], cast[j]) = (cast[j], cast[i]);
            }

            return cast;
        }
    }
}
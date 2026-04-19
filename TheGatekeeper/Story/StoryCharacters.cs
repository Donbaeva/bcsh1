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

        public static void Reset()
        {
            Loyalty = Errors = InfectedIn = RebelTrust = Caught = RobotsPassed = 0;
            BribesAccepted = WolfWarnings = WolfInspections = TotalCreditsFromBribes = 0;
        }

        public static bool ShouldWolfInspect()
        {
            if (BribesAccepted <= 0) return false;
            int chance = BribesAccepted * 25;
            return new System.Random().Next(0, 100) < Math.Min(chance, 90);
        }

        public static int DetermineEnding()
        {
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
        // Вызывается когда персонаж появляется на посту (до решения инспектора)
        void OnArrival();
    }

    // ─── Базовый OnArrival (ничего не делает) ───────────────────────────────
    // Классы которым нужно что-то при появлении — переопределяют
    public abstract class StoryCharacterBase
    {
        public virtual void OnArrival() { } // по умолчанию — ничего
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  OBSERVER BASE — персонажи, которых не надо классифицировать
    //  (инспекторы, агенты, наблюдатели). Кнопки скрываются, показывается [ПРОПУСТИТЬ].
    // ═══════════════════════════════════════════════════════════════════════
    public abstract class ObserverCharacter : Human
    {
        public override bool IsObserver => true;

        protected ObserverCharacter(string name, string dialogue, string idNumber,
            string occupation, string reason, int day)
            : base(name, dialogue, idNumber, occupation, reason, false, day) { }
    }

    // ─── КОМИССАР ВОЛК ─────────────────────────────────────────────────────
    // День 1: приходит как «плановая проверка». Не нужно его классифицировать.
    // Игрок просто нажимает [ПРОПУСТИТЬ] / [ЗАДЕРЖАТЬ].
    public class CommissarWolf : ObserverCharacter, IStoryCharacter
    {
        public CommissarWolf(int day) : base(
            name: "Commissar Wolf",
            dialogue: "Inspector. I am conducting a routine check. Your records are exemplary. Keep it up.",
            idNumber: "GOV-0001-W",
            occupation: "Council Commissar",
            reason: "Routine Inspection",
            day: day)
        {
            AccessCode = "GOV-ALPHA";
            SetPhotoFromFile(@"Images\Characters\story\Commander_Felicia.png");
        }

        public void ApplyEffect(string decision)
        {
            // decision = "PASS" (пропустить) или "DETAIN" (задержать)
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocAnonLetter(Day));
        }

        public string GetSecretNote() =>
            "[ДОСЬЕ — КОМИССАР ВОЛК]\n" +
            "Настоящее имя: Дитер Волк\n" +
            "Роль: Куратор Альфа-Сектора\n" +
            "Доступ: МАКСИМАЛЬНЫЙ\n\n" +
            "Примечание: фиксирует каждую ошибку инспектора\n" +
            "и передаёт отчёты в трибунал.";
    }

    // ─── СЕРЖАНТ КАСТРО ────────────────────────────────────────────────────
    // Дни 1, 3: патрулирует периметр. Не требует классификации.
    public class SergeantCastro : ObserverCharacter, IStoryCharacter
    {
        public SergeantCastro(int day) : base(
            name: "Sergeant Reina Castro",
            dialogue: "Just passing by. Checking if everything is in order at the post. No violations, I hope?",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocRobotCaptureOrder(Day));
        }

        public string GetSecretNote() =>
            "[ФАЙЛ — КАСТРО]\n" +
            "Специализация: контрразведка.\n\n" +
            "⚠ Тайно наблюдает за инспекторами\n" +
            "по поручению Волка.";
    }

    // ─── АГЕНТ ГРЕЙ ─────────────────────────────────────────────────────────
    // День 7: молчит, только наблюдает. Не требует классификации.
    public class AgentGrey : ObserverCharacter, IStoryCharacter
    {
        public AgentGrey(int day) : base(
            name: "Agent Grey",
            dialogue: "...",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocNinasNote(Day));
        }

        public string GetSecretNote() =>
            "[СИСТЕМНОЕ ПРЕДУПРЕЖДЕНИЕ]\n" +
            "Личность: не установлена.\n" +
            "Доступ: МАКСИМАЛЬНЫЙ.\n\n" +
            "Он всё помнит.\n" +
            "Он уже знает.";
    }

    // ─── ВНЕПЛАНОВАЯ ПРОВЕРКА ВОЛКА (при высокой коррупции) ────────────
    public class WolfCorruptionCheck : ObserverCharacter, IStoryCharacter
    {
        public WolfCorruptionCheck(int day) : base(
            name: "Commissar Wolf",
            dialogue: "Inspector. I received a report of... irregularities at this post. " +
                      "I'll need to ask you a few questions.",
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
            // Всегда пропускаем Волка
            EndingTracker.WolfInspections++;
        }

        public void OnArrival()
        {
            // Сразу начинаем допрос при появлении
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.StartWolfCorruptionInterrogation();
        }

        public string GetSecretNote() =>
            "[ВНЕПЛАНОВАЯ ПРОВЕРКА]\n" +
            "Волк получил донос о взятках.\n\n" +
            "Предупреждения: " + EndingTracker.WolfWarnings + "/2\n" +
            "На третий раз — конец смены.";
    }

    // ─── СОВЕТНИК ПЕК ─────────────────────────────────────────────────────
    // День 10: VIP, требует классификации (человек), но может запугивать.
    public class CouncilorPek : Human, IStoryCharacter
    {
        public CouncilorPek(int day) : base(
            name: "Councilor Pek",
            dialogue: "I am Councilor Pek. This is a scheduled visit. Do not delay me.",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocAgentDossier(Day));
        }

        public string GetSecretNote() =>
            "[VIP-ПРОТОКОЛ]\n" +
            "Приоритет: КРИТИЧЕСКИЙ\n\n" +
            "Любая задержка будет немедленно\n" +
            "зафиксирована.";
    }

    // ─── КОМАНДЕР ФЕЛИСИЯ — финальная инспекция (День 10) ──────────────
    // Наблюдатель. Приходит с проверкой в последний день.
    // Решение игрока: передать ли ей секретные документы?
    public class CommanderFelicia : ObserverCharacter, IStoryCharacter
    {
        public bool ReceivedDocuments { get; set; } = false;

        public CommanderFelicia(int day) : base(
            name: "Commander Felicia",
            dialogue: "Final shift inspection. Your performance record is being reviewed. " +
                      "If you have anything to report — now is the time.",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocLockdownOrder(Day));
        }

        public string GetSecretNote() =>
            "[ДОСЬЕ — КОМАНДЕР ФЕЛИСИЯ]\n" +
            "Прямое командование колонией.\n" +
            "Независима от Комиссара Волка.\n\n" +
            "Если передать ей документы —\n" +
            "она примет меры.";
    }

    // ─── ПРОМЕЖУТОЧНАЯ ПРОВЕРКА (Дни 5-6) — обычный инспектор ──────────
    public class MidtermInspector : ObserverCharacter, IStoryCharacter
    {
        public MidtermInspector(int day) : base(
            name: "Inspector Rael",
            dialogue: "Mid-shift audit. Checking compliance across all posts. " +
                      "Your numbers look... interesting.",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocFinancialLeak(Day));
        }

        public string GetSecretNote() =>
            "[АУДИТ — ИНСПЕКТОР РАЭЛЬ]\n" +
            "Отслеживает ошибки по всем постам.\n\n" +
            "Более 2 ошибок к этому моменту —\n" +
            "будет зафиксировано.";
    }

    // ─── ТОМ АРЧЕР ──────────────────────────────────────────────────────────
    public class TomArcher : Human, IStoryCharacter
    {
        public bool PlayerAskedFollowUp { get; set; } = false;

        public TomArcher(int day) : base(
            "Tom Archer",
            "Heard there's an unmarked ship at airlock 9. Interesting, isn't it?",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocFlyer(Day));
        }

        public string GetSecretNote() =>
            "[ЗАМЕТКА — АРЧЕР]\n" +
            "Неофициально известен как 'связной'.\n\n" +
            "Если упомянул корабль — это сигнал.\n" +
            "Спроси про цель визита ещё раз.";
    }

    // ─── НИНА УОРТ ──────────────────────────────────────────────────────────
    public class NinaWorth : Human, IStoryCharacter
    {
        public bool NoteAccepted { get; set; } = false;

        public NinaWorth(int day) : base(
            "Nina Worth",
            "Please read this later. Not now. Just take it.",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocNinasNote(Day));
        }

        public string GetSecretNote() =>
            "[ЗАПИСКА НИНЫ]\n" +
            "'Нас пятеро. Корабль у шлюза 9.\n" +
            "Сигнал: пропусти Мирру на День 7.'";
    }

    // ─── МИРРА ──────────────────────────────────────────────────────────────
    public class Mirra : Alien, IStoryCharacter
    {
        public MirraMode Mode { get; set; }

        public Mirra(int day, MirraMode mode = MirraMode.FirstVisit)
            : base(
                "Mirra",
                mode == MirraMode.Return
                    ? "Remember the note? Tonight. Airlock 9."
                    : "I'm here to study the hydroponics flora.",
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
            else if (decision == "ALIEN" && EndingTracker.RebelTrust >= 3)
                EndingTracker.RebelTrust += 3;
        }

        public void OnArrival()
        {
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocNinasNote(Day));
        }

        public string GetSecretNote() =>
            Mode == MirraMode.Return
                ? "[ПЕРЕХВАТ]\n'Субъект М подтверждает контакт.\nШлюз 9. 03:00.'"
                : "[ФАЙЛ — МИРРА]\nВертикальные зрачки. Акцент.\nНаблюдать внимательно.";
    }

    public enum MirraMode { FirstVisit, Return }

    // ─── ЗОЯ ЛАНН ───────────────────────────────────────────────────────────
    public class ZoyaLann : Human, IStoryCharacter
    {
        public bool PlayerJoined { get; set; } = false;

        public ZoyaLann(int day) : base(
            "Zoya Lann",
            "Nina is arrested. We leave tomorrow at 3. Are you with us?",
            "CIV-8831-L", "Mechanic", "Work shift", false, day)
        {
            AccessCode = "8812-R";
            SetPhotoFromFile(@"Images\Characters\story\Nanami.png");
        }

        public void ApplyEffect(string decision)
        {
            if (PlayerJoined) EndingTracker.RebelTrust += 3;
            else EndingTracker.Loyalty += 3;
        }

        public void OnArrival()
        {
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocConspiracyPlan(Day));
        }

        public string GetSecretNote() =>
            "[ПЕРЕХВАТ ГБ]\n" +
            "Субъект связан с Ниной Уорт.\n" +
            "Задержать обоих при контакте.";
    }

    // ─── ЗАРКХ ──────────────────────────────────────────────────────────────
    public class Zzarkh : Alien, IStoryCharacter
    {
        public Zzarkh(int day) : base(
            "Zzarkh",
            "It's just an allergy. Please, I'm in a hurry.",
            "Trappist-1e", 2, false, day)
        {
            AccessCode = "3392-K";
            SetPhotoFromFile(@"Images\Characters\story\Ivana.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; }

        public void OnArrival()
        {
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;
            f1.ReceiveDocument(Form1.DocFakeMedCert(Day));
            f1.ReceiveDocument(Form1.DocBribeEnvelope("Zzarkh", 150, Day));
        }

        public string GetSecretNote() =>
            "[БИОСКАНЕР]\n" +
            "Маркер B-7 (Синее Гниение — ранняя стадия).\n\n" +
            "⚠ Рекомендован карантин.";
    }

    // ─── ОЛИВЕР КЕЙН ────────────────────────────────────────────────────────
    public class OliverKane : Human, IStoryCharacter
    {
        public OliverKane(int day) : base(
            "Oliver Kane",
            "I'm fine. Just a cold. My daughter is waiting inside.",
            "CIV-5503-K", "Plumber", "Going home", false, day)
        {
            AccessCode = "5521-M";
            SetPhotoFromFile(@"Images\Characters\story\Fellion.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; }

        public void OnArrival()
        {
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            if (f1 == null) return;
            f1.ReceiveDocument(Form1.DocBribeEnvelope("Oliver Kane", 80, Day));
        }

        public string GetSecretNote() =>
            "[БИОСКАНЕР]\n" +
            "Маркер B-7 ПОЛОЖИТЕЛЬНЫЙ.\n\n" +
            "Скрыт тональным кремом\n" +
            "на левой щеке.";
    }

    // ─── ЗАРКХ-2 ────────────────────────────────────────────────────────────
    public class ZzarkhTwo : Alien, IStoryCharacter
    {
        public ZzarkhTwo(int day) : base(
            "Zzarkh-2",
            "You held my brother. Where is he? Let me in!",
            "Trappist-1e", 2, true, day)
        {
            AccessCode = "???";
            SetPhotoFromFile(@"Images\Characters\story\Len.png");
        }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 2; }

        public void OnArrival()
        {
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocInfectionReport(Day));
        }

        public string GetSecretNote() =>
            "[КРИТИЧЕСКОЕ ПРЕДУПРЕЖДЕНИЕ]\n" +
            "Активная стадия B-7.\n" +
            "Высокий радиус заражения.\n\n" +
            "❗ НЕ ПРОПУСКАТЬ.";
    }

    // ─── ПРОФЕССОР ХАСАН ────────────────────────────────────────────────────
    public class ProfessorHasan : Human, IStoryCharacter
    {
        public ProfessorHasan(int day) : base(
            "Professor Hasan",
            "I am working on the vaccine. I must reach the lab today.",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocHasanFormula(Day));
        }

        public string GetSecretNote() =>
            "[ДОСЬЕ]\n" +
            "Единственный человек, который может\n" +
            "найти лекарство от гниения.\n\n" +
            "...сам заражён.";
    }

    // ─── СЕРВ-КОМАНДЕР X1 ───────────────────────────────────────────────────
    public class ServCommanderX1 : Robot, IStoryCharacter
    {
        public ServCommanderX1(int day) : base(
            "Serv-Commander X1",
            "I have a Class A mandate. Delay is unacceptable.",
            "X1-CMD-CLASSIFIED", "Command Drone", false, day)
        {
            AccessCode = "X1-VOID";
            SetPhotoFromFile(@"Images\Characters\story\Clauddee.png");
        }

        public void ApplyEffect(string decision)
        {
            EndingTracker.RobotsPassed += 1;
            // Изымаем поддельный мандат как улику
            if (decision == "ROBOT")
            {
                var form1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
                form1?.ReceiveDocument(Form1.DocServMandate(Day));
            }
        }

        public void OnArrival()
        {
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocHasanFormula(Day));
        }

        public string GetSecretNote() =>
            "[ОШИБКА МАНДАТА]\n" +
            "Подпись НЕДЕЙСТВИТЕЛЬНА.\n" +
            "Офицер Кларк умер 14 дней назад.\n\n" +
            "⚠ Это боевой дрон.";
    }

    // ─── СЕРВ-ЛЕГИОН ────────────────────────────────────────────────────────
    public class ServLegion : Robot, IStoryCharacter
    {
        public int UnitIndex { get; }

        public ServLegion(int day, int unitIndex) : base(
            $"Serv-Legion #{unitIndex}",
            $"Unit {unitIndex} of 3. Requesting entry.",
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
            var f1 = System.Windows.Forms.Application.OpenForms["Form1"] as Form1;
            f1?.ReceiveDocument(Form1.DocRobotAnatomy(Day));
        }

        public string GetSecretNote() =>
            "[АНОМАЛИЯ]\n" +
            "Все три дрона имеют ОДИНАКОВЫЙ\n" +
            "серийный номер.\n\n" +
            "Массовая подделка.";
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
                    // День 1: первый день. CommissarWolf знакомится, Арчер даёт намёки
                    return new List<IStoryCharacter>
                    {
                        new CommissarWolf(day),
                        new TomArcher(day)
                    };
                case 2:
                    // День 2: Мирра появляется впервые, Кастро патрулирует
                    return new List<IStoryCharacter>
                    {
                        new SergeantCastro(day),
                        new Mirra(day, MirraMode.FirstVisit)
                    };
                case 3:
                    // День 3: первый заражённый, Нина передаёт записку
                    return new List<IStoryCharacter>
                    {
                        new Zzarkh(day),
                        new NinaWorth(day)
                    };
                case 4:
                    // День 4: Волк снова проверяет, ServeX1 пробует прорваться
                    return new List<IStoryCharacter>
                    {
                        new CommissarWolf(day),
                        new ServCommanderX1(day)
                    };
                case 5:
                    // День 5: ПРОМЕЖУТОЧНАЯ ПРОВЕРКА — инспектор Раэль
                    return new List<IStoryCharacter>
                    {
                        new MidtermInspector(day),
                        new OliverKane(day)
                    };
                case 6:
                    // День 6: второй заражённый, профессор рвётся в лабораторию
                    return new List<IStoryCharacter>
                    {
                        new ZzarkhTwo(day),
                        new ProfessorHasan(day)
                    };
                case 7:
                    // День 7: Мирра возвращается с предложением, Агент наблюдает
                    return new List<IStoryCharacter>
                    {
                        new Mirra(day, MirraMode.Return),
                        new AgentGrey(day)
                    };
                case 8:
                    // День 8: Зоя — последний шанс присоединиться
                    return new List<IStoryCharacter>
                    {
                        new ZoyaLann(day),
                        new SergeantCastro(day)
                    };
                case 9:
                    // День 9: Агент усиливает давление, Легион пробует прорваться
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
                (Character)new CommanderFelicia(10),  // Финальная инспекция
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
            {
                cast.AddRange(GetDay10Cast());
            }
            else
            {
                var storyChars = GetStoryCharactersForDay(day);
                foreach (var sc in storyChars)
                    if (sc is Character c) cast.Add(c);
            }

            // Shuffle
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
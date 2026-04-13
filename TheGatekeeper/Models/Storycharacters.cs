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
    //  ENDING TRACKER — stores counters, determines the final outcome
    // ═══════════════════════════════════════════════════════════════════════
    public static class EndingTracker
    {
        public static int Loyalty = 0;      // Loyalty to the regime
        public static int Errors = 0;       // Incorrect decisions
        public static int InfectedIn = 0;   // Infected people let inside
        public static int RebelTrust = 0;   // Trust from allies/rebels
        public static int Caught = 0;       // Evidence of treason
        public static int RobotsPassed = 0; // Hostile robots/aliens passed

        public static void Reset()
        {
            Loyalty = Errors = InfectedIn = RebelTrust = Caught = RobotsPassed = 0;
        }

        public static int DetermineEnding()
        {
            // 6 — Invasion (Catastrophic outcome)
            if (Errors >= 5 && RobotsPassed >= 3) return 6;

            // 5 — Outbreak (Infection)
            if (InfectedIn >= 4) return 5;

            // 4 — Treason Revealed
            if (Caught >= 3 && RebelTrust >= 3) return 4;

            // 3 — Failure
            if (Errors >= 5 && Loyalty < 4) return 3;

            // 2 — Escape
            if (RebelTrust >= 8 && Caught <= 1) return 2;

            // 1 — Honorary Citizen (Default good ending)
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
    }

    // --- CHARACTERS ---

    public class CommissarWolf : Human, IStoryCharacter
    {
        public CommissarWolf(int day) : base(
            name: "Commissar Wolf",
            dialogue: "Inspector. I am conducting a routine check. Your records are exemplary. Keep it up.",
            idNumber: "GOV-0001-W",
            occupation: "Council Commissar",
            reason: "Routine Inspection",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\story\Commander_Felicia.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN") EndingTracker.Loyalty += 2;
            else { EndingTracker.Loyalty -= 1; EndingTracker.Errors += 1; }
        }

        public string GetSecretNote() => "[DOSSIER — COMMISSAR WOLF]\nReal Name: Dieter Wolf\nRole: Alpha Sector Curator\nAccess: MAXIMUM\nNote: Reports every error to the tribunal.";
    }

    public class SergeantCastro : Human, IStoryCharacter
    {
        public SergeantCastro(int day) : base(
            name: "Sergeant Reina Castro",
            dialogue: "Just passing by. Checking if everything is in order at the post. No violations, I hope?",
            idNumber: "MIL-3391-C",
            occupation: "Military Officer",
            reason: "Patrolling",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\story\Jack.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN") EndingTracker.Loyalty += 2;
            else { EndingTracker.Caught += 1; EndingTracker.Errors += 1; }
        }

        public string GetSecretNote() => "[FILE — CASTRO]\nSpecialization: Counter-intelligence.\nWarning: Secretly monitoring inspectors for Wolf.";
    }

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
        { SetPhotoFromFile(@"Image\Character\story\Pier.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN") EndingTracker.Loyalty += 3;
            else { EndingTracker.Errors += 2; EndingTracker.Loyalty -= 2; }
        }

        public string GetSecretNote() => "[VIP PROTOCOL]\nPriority: CRITICAL\nAny delay will be reported immediately.";
    }

    public class TomArcher : Human, IStoryCharacter
    {
        public bool PlayerAskedFollowUp { get; set; } = false;
        public TomArcher(int day) : base("Tom Archer", "Heard there's an unmarked ship at airlock 9. Interesting, isn't it?", "CIV-2241-A", "Engineer", "Work shift", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Sam.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN") EndingTracker.Loyalty += 1;
            if (PlayerAskedFollowUp) EndingTracker.RebelTrust += 1;
        }

        public string GetSecretNote() => "[NOTE — ARCHER]\nInformally known as a 'fixer'. If he mentions a ship, it's a signal.";
    }

    public class NinaWorth : Human, IStoryCharacter
    {
        public bool NoteAccepted { get; set; } = false;
        public NinaWorth(int day) : base("Nina Worth", "Please read this later. Not now. Just take it.", "CIV-4412-W", "Technician", "Work shift", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Aidai.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision != "HUMAN") { EndingTracker.Errors += 1; return; }
            if (NoteAccepted) EndingTracker.RebelTrust += 2;
            else { EndingTracker.Loyalty += 2; EndingTracker.Caught += 1; }
        }

        public string GetSecretNote() => "[NINA'S NOTE]\n'We are five. Ship at airlock 9. Signal: Let Mirra pass on Day 7.'";
    }

    public class Mirra : Alien, IStoryCharacter
    {
        public MirraMode Mode { get; set; }
        public Mirra(int day, MirraMode mode = MirraMode.FirstVisit) : base("Mirra", mode == MirraMode.Return ? "Remember the note? Tonight. Airlock 9." : "I'm here to study the hydroponics flora.", "Xylos", 0, false, day)
        { this.Mode = mode; SetPhotoFromFile(@"Image\Character\story\Lum.png"); }

        public void ApplyEffect(string decision)
        {
            if (Mode == MirraMode.FirstVisit) { if (decision == "ALIEN") EndingTracker.Loyalty += 1; }
            else if (decision == "ALIEN" && EndingTracker.RebelTrust >= 3) EndingTracker.RebelTrust += 3;
        }

        public string GetSecretNote() => Mode == MirraMode.Return ? "[INTERCEPTED]\n'Subject M confirms contact. Airlock 9. 03:00.'" : "[FILE — MIRRA]\nVertical pupils. Accented. Watch carefully.";
    }

    public enum MirraMode { FirstVisit, Return }

    public class ZoyaLann : Human, IStoryCharacter
    {
        public bool PlayerJoined { get; set; } = false;
        public ZoyaLann(int day) : base("Zoya Lann", "Nina is arrested. We leave tomorrow at 3. Are you with us?", "CIV-8831-L", "Mechanic", "Work shift", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Nanami.png"); }

        public void ApplyEffect(string decision)
        {
            if (PlayerJoined) EndingTracker.RebelTrust += 3;
            else EndingTracker.Loyalty += 3;
        }

        public string GetSecretNote() => "[GB INTERCEPT]\nSubject linked to Nina Worth. Arrest both if they communicate.";
    }

    public class AgentGrey : Human, IStoryCharacter
    {
        public AgentGrey(int day) : base("Agent Grey", "...", "GBI-0099-X", "GB Intelligence", "Observation", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Pam.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN" && EndingTracker.RebelTrust >= 2) EndingTracker.Caught += 1;
            else EndingTracker.Caught += 2;
        }

        public string GetSecretNote() => "[SYSTEM WARNING]\nIdentity unknown. Access: MAXIMUM. He remembers everything.";
    }

    public class Zzarkh : Alien, IStoryCharacter
    {
        public Zzarkh(int day) : base("Zzarkh", "It's just an allergy. Please, I'm in a hurry.", "Trappist-1e", 2, false, day)
        { SetPhotoFromFile(@"Image\Character\story\Ivana.png"); }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; }

        public string GetSecretNote() => "[BIOSCANNER]\nOrganic marker B-7 (Blue Rot - early stage). Quarantine recommended.";
    }

    public class OliverKane : Human, IStoryCharacter
    {
        public OliverKane(int day) : base("Oliver Kane", "I'm fine. Just a cold. My daughter is waiting inside.", "CIV-5503-K", "Plumber", "Going home", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Fellion.png"); }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; }

        public string GetSecretNote() => "[BIOSCANNER]\nMarker B-7 POSITIVE. Hidden by makeup on left cheek.";
    }

    public class ZzarkhTwo : Alien, IStoryCharacter
    {
        public ZzarkhTwo(int day) : base("Zzarkh-2", "You held my brother. Where is he? Let me in!", "Trappist-1e", 2, true, day)
        { SetPhotoFromFile(@"Image\Character\story\Len.png"); }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 2; }

        public string GetSecretNote() => "[CRITICAL ALERT]\nActive B-7 stage. High contagion radius. DO NOT PASS.";
    }

    public class ProfessorHasan : Human, IStoryCharacter
    {
        public ProfessorHasan(int day) : base("Professor Hasan", "I am working on the vaccine. I must reach the lab today.", "SCI-9901-H", "Biochemist", "Urgent Research", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Dr_Moon.png"); }

        public void ApplyEffect(string decision) { EndingTracker.InfectedIn += 1; StoryFlags.HasanReachedLab = true; }

        public string GetSecretNote() => "[DOSSIER]\nThe only man who can cure the rot is infected himself.";
    }

    public class ServCommanderX1 : Robot, IStoryCharacter
    {
        public ServCommanderX1(int day) : base("Serv-Commander X1", "I have a Class A mandate. Delay is unacceptable.", "X1-CMD-CLASSIFIED", "Command Drone", false, day)
        { SetPhotoFromFile(@"Image\Character\story\Clauddee.png"); }

        public void ApplyEffect(string decision) { EndingTracker.RobotsPassed += 1; }

        public string GetSecretNote() => "[MANDATE ERROR]\nSignature INVALID. Officer Clark died 14 days ago. Combat drone.";
    }

    public class ServLegion : Robot, IStoryCharacter
    {
        public int UnitIndex { get; }
        public ServLegion(int day, int unitIndex) : base($"Serv-Legion #{unitIndex}", $"Unit {unitIndex} of 3. Requesting entry.", $"LEG-000{unitIndex}-A", "Legion Drone", unitIndex > 1, day)
        { UnitIndex = unitIndex; SetPhotoFromFile(@"Image\Character\story\E.png"); }

        public void ApplyEffect(string decision) { EndingTracker.RobotsPassed += 1; }

        public string GetSecretNote() => "[ANOMALY]\nAll three drones share the SAME serial number. Mass forgery.";
    }

    public static class StoryFlags
    {
        public static bool HasanReachedLab = false;
        public static bool ServX1Passed = false;
        public static bool NinaArrested = false;
        public static bool MirraBetrayed = false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  STORY SCHEDULE — 2 characters per day
    // ═══════════════════════════════════════════════════════════════════════
    public static class StorySchedule
    {
        public static IEnumerable<IStoryCharacter> GetStoryCharactersForDay(int day)
        {
            switch (day)
            {
                case 1:
                    return new List<IStoryCharacter> { new CommissarWolf(day), new SergeantCastro(day) };
                case 2:
                    return new List<IStoryCharacter> { new TomArcher(day), new Mirra(day, MirraMode.FirstVisit) };
                case 3:
                    return new List<IStoryCharacter> { new Zzarkh(day), new SergeantCastro(day) }; // Castro checks again
                case 4:
                    return new List<IStoryCharacter> { new NinaWorth(day), new CommissarWolf(day) };
                case 5:
                    return new List<IStoryCharacter> { new ServCommanderX1(day), new OliverKane(day) };
                case 6:
                    return new List<IStoryCharacter> { new ZzarkhTwo(day), new ProfessorHasan(day) };
                case 7:
                    return new List<IStoryCharacter> { new Mirra(day, MirraMode.Return), new AgentGrey(day) };
                default:
                    return Enumerable.Empty<IStoryCharacter>();
            }
        }

        public static List<Character> GetDay10Cast()
        {
            return new List<Character>
            {
                (Character)new CouncilorPek(10),
                (Character)new AgentGrey(10),
                (Character)new ServLegion(10, 1),
                (Character)new ServLegion(10, 2),
                (Character)new ServLegion(10, 3),
            };
        }

        public static List<Character> BuildStoryCast(int day, int randomHumans = 1, int randomRobots = 0, int randomAliens = 0, int randomTypeCount = 3)
        {
            var cast = CharacterFactory.GenerateMixedCast(day, randomHumans, randomRobots, randomAliens, randomTypeCount);

            if (day == 10)
            {
                cast.AddRange(GetDay10Cast());
            }
            else
            {
                var storyCharacters = GetStoryCharactersForDay(day);
                foreach (var sc in storyCharacters)
                {
                    if (sc is Character c) cast.Add(c);
                }
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
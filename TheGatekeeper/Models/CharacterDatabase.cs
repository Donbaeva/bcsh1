using System;

namespace TheGatekeeper.Models
{
    public static class CharacterDatabase
    {
        private static Random rnd = new Random();

        public static readonly string[] FirstNames = {
            "Alex", "Jamie", "Casey", "Nova", "Orion", "Morgan", "Riley", "Cameron", "Taylor", "Jordan",
            "Avery", "Quinn", "Reese", "Sage", "Blair", "Dakota", "Emerson", "Finley", "Harper", "Parker",
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles",
            "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua",
            "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan",
            "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon",
            "Benjamin", "Samuel", "Gregory", "Frank", "Alexander", "Raymond", "Patrick", "Jack", "Dennis", "Jerry",
            "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen",
            "Nancy", "Lisa", "Betty", "Margaret", "Sandra", "Ashley", "Kimberly", "Emily", "Donna", "Michelle",
            "Dorothy", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Sharon", "Laura", "Cynthia",
            "Kathleen", "Amy", "Shirley", "Angela", "Helen", "Anna", "Brenda", "Pamela", "Nicole", "Emma",
            "Samantha", "Katherine", "Christine", "Debra", "Rachel", "Catherine", "Carolyn", "Janet", "Ruth", "Maria"
        };

        public static readonly string[] LastNames = {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
            "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
            "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
            "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
            "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts",
            "Vance", "Kovacs", "Zane", "Hawke", "Stark", "Winters", "Ashford", "Mercer", "Reeves", "Sinclair",
            "Donovan", "Morrow", "Hale", "Sterling", "Locke", "Graves", "Pierce", "Vaughn", "Cross", "Frost",
            "Steele", "Kane", "Blackwood", "Thorne", "Irons", "Caine", "Shade", "Flint", "Hawk", "Wolf"
        };

        // Настоящие инопланетные имена — используются ТОЛЬКО до Дня 4 включительно
        private static readonly string[] AlienNamesObvious =
        {
            "Ma'saryk", "Zyx", "Kr'zzak", "Vel'kor", "Nyx'ara",
            "Qlx'thor", "Az'rael", "Vex'lon", "Kor'thas", "Zyl'vex",
            "Zzarth", "Vell", "Krxx", "Nyxar", "Qeth",
        };

        // Человеческие прикрытия — используются с Дня 5 пришельцами
        // Нарочито обычные, ничем не выдающиеся
        private static readonly string[] AlienCoverNames =
        {
            "Marcus Webb",   "Lena Frost",    "Owen Carr",     "Diana Cole",
            "Peter Hale",    "Cora Vance",    "Simon Marsh",   "Elena Cross",
            "Victor Reed",   "Nadia Stone",   "Carl Ashton",   "Irene Locke",
            "Hugo Steele",   "Vera Thorne",   "Leon Graves",   "Mira Kane",
            "Otto Black",    "Sonja Pierce",  "Miles Shade",   "Tara Wolf",
            "Rene Flint",    "Anya Hawk",     "Cole Irons",    "Lyra Caine",
        };

        public static string GetRandomName() =>
            FirstNames[rnd.Next(FirstNames.Length)] + " " + LastNames[rnd.Next(LastNames.Length)];

        // После Дня 4 пришельцы используют человеческие имена-прикрытия
        public static string GetRandomAlienName(int day = 1)
        {
            if (day >= 5)
                return AlienCoverNames[rnd.Next(AlienCoverNames.Length)];
            return AlienNamesObvious[rnd.Next(AlienNamesObvious.Length)];
        }

        public static readonly string[] Reasons = {
            "Work", "Trade", "Tourism", "Diplomacy", "Medical", "Research",
            "Family visit", "Business meeting", "Conference", "Relocation",
            "Education", "Cultural exchange", "Maintenance", "Inspection"
        };

        public static readonly string[] Professions = {
            "Engineer", "Medic", "Pilot", "Scientist", "Trader", "Security",
            "Technician", "Analyst", "Consultant", "Logistics Officer",
            "Teacher", "Journalist", "Artist", "Chef", "Driver",
            "Administrator", "Accountant", "Lawyer", "Architect", "Researcher"
        };

        // Переменная квота по дням: 3–7 персонажей
        // Дни 1–2: 3–4 (обучение), Дни 3–5: 4–5, Дни 6–10: 5–7
        public static int GetDailyQuota(int day)
        {
            switch (day)
            {
                case 1: return 3;
                case 2: return 4;
                case 3: return 4;
                case 4: return 5;
                case 5: return 5;
                case 6: return 6;
                case 7: return 6;
                case 8: return 7;
                case 9: return 7;
                case 10: return 7;
                default: return Math.Min(3 + day, 7);
            }
        }

        public static string GetRandomReason() =>
            Reasons[rnd.Next(Reasons.Length)];

        public static string GetRandomProfession() =>
            Professions[rnd.Next(Professions.Length)];

        public static string GetRandomAlienPlanet() =>
            new[] { "Zog-7", "Xylos", "Kepler-186f", "Proxima Centauri b", "Trappist-1e" }[rnd.Next(5)];
    }
}
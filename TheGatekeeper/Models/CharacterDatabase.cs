using System;

namespace TheGatekeeper.Models
{
    public static class CharacterDatabase
    {
        private static Random rnd = new Random();

        public static readonly string[] FirstNames = {
            // Универсальные / международные
            "Alex", "Jamie", "Casey", "Nova", "Orion", "Morgan", "Riley", "Cameron", "Taylor", "Jordan",
            "Avery", "Quinn", "Reese", "Sage", "Blair", "Dakota", "Emerson", "Finley", "Harper", "Parker",
            // Мужские
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles",
            "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua",
            "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan",
            "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon",
            "Benjamin", "Samuel", "Gregory", "Frank", "Alexander", "Raymond", "Patrick", "Jack", "Dennis", "Jerry",
            // Женские
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

        // Имена для пришельцев (остаются странными)
        public static readonly string[] AlienNames =
        {
            "Ma'saryk", "Zyx", "Kr'zzak", "Vel'kor", "Nyx'ara", "NeO", "Morpheus", "Gan'dalf", "Vold'mort",
            "Fro'Do", "Qlx'thor", "Az'rael", "Vex'lon", "Kor'thas", "Zyl'vex", "ThOrr", "Azgaard", "Lokii",
            "Nap'Leon", "Moz'Art", "Cleo'Patra", "Kaf'Ka", "Kne'dlik", "Svi'Cko'Va", "Pil'Sner",
        };

        public static string GetRandomName() =>
            FirstNames[rnd.Next(FirstNames.Length)] + " " + LastNames[rnd.Next(LastNames.Length)];

        public static string GetRandomAlienName() =>
            AlienNames[rnd.Next(AlienNames.Length)];

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

        // ═══════════════════════════════════════════════════════════════════
        // МЕТОДЫ ПОЛУЧЕНИЯ СЛУЧАЙНЫХ ДАННЫХ
        // ═══════════════════════════════════════════════════════════════════

        public static string GetRandomReason() =>
            Reasons[rnd.Next(Reasons.Length)];

        public static string GetRandomProfession() =>
            Professions[rnd.Next(Professions.Length)];

        public static string GetRandomAlienPlanet() =>
            new[] { "Zog-7", "Xylos", "Kepler-186f", "Proxima Centauri b", "Trappist-1e" }[rnd.Next(5)];
    }
}
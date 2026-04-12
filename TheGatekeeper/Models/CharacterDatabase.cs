using System;

namespace TheGatekeeper.Models
{
    public static class CharacterDatabase
    {
        private static Random rnd = new Random();

        public static readonly string[] FirstNames = { "Alex", "Jamie", "Casey", "Nova", "Orion" };
        public static readonly string[] LastNames = { "Smith", "Vance", "Kovacs", "Zane" };

        // Имена для пришельцев
        public static readonly string[] AlienNames =
        {
            "Xar'thul", "Zyx", "Kr'zzak", "Vel'kor", "Nyx'ara",
            "Qlx'thor", "Az'rael", "Vex'lon", "Kor'thas", "Zyl'vex"
        };

        public static string GetRandomName() =>
            FirstNames[rnd.Next(FirstNames.Length)] + " " + LastNames[rnd.Next(LastNames.Length)];

        public static string GetRandomAlienName() =>
            AlienNames[rnd.Next(AlienNames.Length)];

        public static string GetRandomReason() =>
            new[] { "Work", "Trade", "Tourism", "Diplomacy", "Medical", "Research" }[rnd.Next(6)];

        public static string GetRandomProfession() =>
            new[] { "Engineer", "Medic", "Pilot", "Scientist", "Trader", "Security" }[rnd.Next(6)];

        public static string GetRandomRobotModel() =>
            new[] { "Unit-X", "Sentry-7", "Droid-A1", "SynthCore-9", "Automaton-Beta" }[rnd.Next(5)];

        public static string GetRandomAlienPlanet() =>
            new[] { "Zog-7", "Xylos", "Kepler-186f", "Proxima Centauri b", "Trappist-1e" }[rnd.Next(5)];
    }
}
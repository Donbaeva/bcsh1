using System;

namespace TheGatekeeper.Models
{
    public static class CharacterDatabase
    {
        private static Random rnd = new Random();

        public static readonly string[] FirstNames = { "Alex", "Jamie", "Casey", "Nova", "Orion" };
        public static readonly string[] LastNames = { "Smith", "Vance", "Kovacs", "Zane" };

        public static string GetRandomName() =>
            FirstNames[rnd.Next(FirstNames.Length)] + " " + LastNames[rnd.Next(LastNames.Length)];

        public static string GetRandomReason() =>
            new[] { "Work", "Trade", "Tourism", "Diplomacy" }[rnd.Next(4)];

        public static string GetRandomProfession() =>
            new[] { "Engineer", "Medic", "Pilot", "Scientist" }[rnd.Next(4)];

        public static string GetRandomRobotModel() =>
            new[] { "Unit-X", "Sentry-7", "Droid-A1" }[rnd.Next(3)];

        public static string GetRandomAlienPlanet() =>
            new[] { "Zog-7", "Xylos", "Kepler-186f" }[rnd.Next(3)];
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace TheGatekeeper.Models
{
    public static class CharacterFactory
    {
        private static Random rnd = new Random();

        public static List<Character> GenerateDayCast(int day, int humanCount, int robotCount, int alienCount)
        {
            var characters = new List<Character>();

            // Сборка Людей
            for (int i = 0; i < humanCount; i++)
            {
                characters.Add(new Human(
                    CharacterDatabase.GetRandomName(),
                    "I am here for " + CharacterDatabase.GetRandomReason(),
                    $"ID-{rnd.Next(1000, 9999)}",
                    CharacterDatabase.GetRandomProfession(),
                    CharacterDatabase.GetRandomReason(),
                    rnd.Next(0, 10) > 2 // 80% нормальный человек
                , day));
            }

            // Сборка Роботов
            for (int i = 0; i < robotCount; i++)
            {
                characters.Add(new Robot(
                    CharacterDatabase.GetRandomName(), // Роботы могут иметь человеческие имена
                    "BEEP BOOP",
                    $"SN-{rnd.Next(100, 999)}",
                    CharacterDatabase.GetRandomRobotModel(),
                    rnd.Next(0, 10) > 4 // 60% очевидный робот
                , day));
            }

            // Перемешиваем список
            return characters.OrderBy(x => rnd.Next()).ToList();
        }
        public static Character GenerateRandom(int day)
        {
            // Генерируем пачку из 1 человека, 0 роботов, 0 пришельцев (или как вам нужно)
            var cast = GenerateDayCast(day, 1, 0, 0);
            return cast.Count > 0 ? cast[0] : null;
        }
    }
}
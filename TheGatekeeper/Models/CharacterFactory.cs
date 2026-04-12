using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TheGatekeeper.Models
{
    public static class CharacterFactory
    {
        private static Random rnd = new Random();

        // Кэш путей к изображениям
        private static List<string> randomCharacterPhotos;  // ИИ сам решает тип
        private static List<string> humanPhotos;            // Гарантированно люди
        private static List<string> robotPhotos;            // Гарантированно роботы
        private static List<string> alienPhotos;            // Гарантированно пришельцы
        private static List<string> storyCharacterPhotos;   // Сюжетные персонажи

        static CharacterFactory()
        {
            LoadPhotoPaths();
        }

        /// <summary>
        /// Загружает пути к изображениям из папок
        /// </summary>
        private static void LoadPhotoPaths()
        {
            string baseDir = Path.Combine(Application.StartupPath, "Images", "Characters");

            randomCharacterPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Персонажи"));
            humanPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Люди"));
            robotPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Роботы"));
            alienPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Инопланетяне"));
            storyCharacterPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Персонажи с сюжетом"));
        }

        /// <summary>
        /// Загружает все изображения из указанной папки
        /// </summary>
        private static List<string> LoadImagesFromFolder(string folderPath)
        {
            var result = new List<string>();

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return result;
            }

            var extensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };
            foreach (var ext in extensions)
            {
                result.AddRange(Directory.GetFiles(folderPath, ext));
            }

            return result;
        }

        /// <summary>
        /// Получает случайное фото из списка
        /// </summary>
        private static Image GetRandomPhoto(List<string> photoList)
        {
            if (photoList == null || photoList.Count == 0)
                return CreatePlaceholderImage("NO PHOTO");

            string path = photoList[rnd.Next(photoList.Count)];
            try
            {
                return Image.FromFile(path);
            }
            catch
            {
                return CreatePlaceholderImage("ERROR");
            }
        }

        /// <summary>
        /// Создаёт placeholder-изображение с текстом
        /// </summary>
        private static Image CreatePlaceholderImage(string text)
        {
            var bmp = new Bitmap(400, 400);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(30, 30, 30));
                using (var font = new Font("Consolas", 20, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Red))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(text, font, brush, new RectangleF(0, 0, 400, 400), sf);
                }
            }
            return bmp;
        }

        public static List<Character> GenerateDayCast(int day, int humanCount, int robotCount, int alienCount)
        {
            var characters = new List<Character>();

            // Сборка Людей (из папки "Люди" - гарантированно люди)
            for (int i = 0; i < humanCount; i++)
            {
                var human = new Human(
                    CharacterDatabase.GetRandomName(),
                    "", // Диалог будет сгенерирован позже
                    $"ID-{rnd.Next(1000, 9999)}",
                    CharacterDatabase.GetRandomProfession(),
                    CharacterDatabase.GetRandomReason(),
                    rnd.Next(0, 10) > 2, // 80% нормальный человек
                    day
                );

                // Загружаем фото из папки "Люди"
                human.Photo = GetRandomPhoto(humanPhotos);

                // Генерируем приветствие
                human.Dialogue = CharacterAI.GenerateGreeting(human);

                characters.Add(human);
            }

            // Сборка Роботов (из папки "Роботы" - гарантированно роботы)
            for (int i = 0; i < robotCount; i++)
            {
                var robot = new Robot(
                    CharacterDatabase.GetRandomName(),
                    "", // Диалог будет сгенерирован
                    $"SN-{rnd.Next(100, 999)}",
                    CharacterDatabase.GetRandomRobotModel(),
                    rnd.Next(0, 10) > 4, // 60% очевидный робот
                    day
                );

                // Загружаем фото из папки "Роботы"
                robot.Photo = GetRandomPhoto(robotPhotos);

                // Генерируем приветствие с учётом навыка маскировки
                robot.Dialogue = CharacterAI.GenerateGreeting(robot);

                characters.Add(robot);
            }

            // Сборка Пришельцев (из папки "Инопланетяне" - гарантированно пришельцы)
            for (int i = 0; i < alienCount; i++)
            {
                var alien = new Alien(
                    CharacterDatabase.GetRandomAlienName(),
                    "", // Диалог будет сгенерирован
                    CharacterDatabase.GetRandomAlienPlanet(),
                    rnd.Next(2, 8),
                    rnd.Next(0, 10) > 5, // 50% очевидный пришелец
                    day
                );

                // Загружаем фото из папки "Инопланетяне"
                alien.Photo = GetRandomPhoto(alienPhotos);

                // Генерируем приветствие с учётом навыка маскировки
                alien.Dialogue = CharacterAI.GenerateGreeting(alien);

                characters.Add(alien);
            }

            // Перемешиваем список
            return characters.OrderBy(x => rnd.Next()).ToList();
        }

        /// <summary>
        /// Создаёт персонажа со СЛУЧАЙНЫМ типом из папки "Персонажи"
        /// ИИ сам решает: человек, робот или пришелец
        /// </summary>
        public static Character GenerateRandomTypeCharacter(int day)
        {
            // ИИ случайно выбирает тип (0 = Human, 1 = Robot, 2 = Alien)
            int randomType = rnd.Next(0, 3);

            Character character = null;

            switch (randomType)
            {
                case 0: // Человек
                    character = new Human(
                        CharacterDatabase.GetRandomName(),
                        "",
                        $"ID-{rnd.Next(1000, 9999)}",
                        CharacterDatabase.GetRandomProfession(),
                        CharacterDatabase.GetRandomReason(),
                        rnd.Next(0, 10) > 2,
                        day
                    );
                    break;

                case 1: // Робот
                    character = new Robot(
                        CharacterDatabase.GetRandomName(),
                        "",
                        $"SN-{rnd.Next(100, 999)}",
                        CharacterDatabase.GetRandomRobotModel(),
                        rnd.Next(0, 10) > 4,
                        day
                    );
                    break;

                case 2: // Пришелец
                    character = new Alien(
                        CharacterDatabase.GetRandomAlienName(),
                        "",
                        CharacterDatabase.GetRandomAlienPlanet(),
                        rnd.Next(2, 8),
                        rnd.Next(0, 10) > 5,
                        day
                    );
                    break;
            }

            // Загружаем фото из папки "Персонажи" (ИИ сам определил тип)
            if (character != null)
            {
                character.Photo = GetRandomPhoto(randomCharacterPhotos);
                character.Dialogue = CharacterAI.GenerateGreeting(character);
            }

            return character;
        }

        /// <summary>
        /// Генерирует группу персонажей, где часть из конкретных папок, 
        /// а часть - случайного типа из папки "Персонажи"
        /// </summary>
        public static List<Character> GenerateMixedCast(int day,
            int guaranteedHumans, int guaranteedRobots, int guaranteedAliens,
            int randomTypeCount)
        {
            var characters = new List<Character>();

            // Добавляем гарантированных персонажей из конкретных папок
            characters.AddRange(GenerateDayCast(day, guaranteedHumans, guaranteedRobots, guaranteedAliens));

            // Добавляем персонажей случайного типа из папки "Персонажи"
            for (int i = 0; i < randomTypeCount; i++)
            {
                characters.Add(GenerateRandomTypeCharacter(day));
            }

            return characters.OrderBy(x => rnd.Next()).ToList();
        }

        public static Character GenerateRandom(int day)
        {
            var cast = GenerateDayCast(day, 1, 0, 0);
            return cast.Count > 0 ? cast[0] : null;
        }
    }
}
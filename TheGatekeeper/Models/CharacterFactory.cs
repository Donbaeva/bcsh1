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
    //  РАЗВЛЕКАТЕЛЬНЫЕ ПЕРСОНАЖИ — специальные классы
    // ═══════════════════════════════════════════════════════════════════════

    // ─── ТУПЫЕ / РАСТЕРЯННЫЕ ────────────────────────────────────────────

    public class BorisPykh : Human
    {
        public BorisPykh(int day) : base(
            "Борис Пых",
            "Это... магазин запчастей? Мне сказали третий коридор налево.",
            "ПЫХ-2031-Б", "Фермер", "Перепутал дверь", false, day)
        { }
    }

    public class CloneBot : Robot
    {
        public int CloneIndex { get; }
        public CloneBot(int day, int index) : base(
            $"Клонт-3 [{(index == 0 ? "А" : "Б")}]",
            index == 0
                ? "Я настоящий. Он сломан. У него баг в памяти."
                : "Это он сломан. Я помню наш серийный номер: 000-А.",
            "000-А", "Неизвестно", true, day)
        {
            CloneIndex = index;
        }
    }

    public class Uuu : Alien
    {
        public Uuu(int day) : base("Ууу", "Ууу.", "Неизвестно", 0, false, day) { }
    }

    public class DmitryUnlucky : Human
    {
        public DmitryUnlucky(int day) : base(
            "Дмитрий Невезучий",
            "Документы в кармане... нет, в другом... нет, я их, кажется, постирал.",
            "НЕВ-0000-Д", "Разнорабочий", "По делам", false, day)
        { }
    }

    // ─── БОЛТЛИВЫЕ ──────────────────────────────────────────────────────

    public class ProfessorCross : Human
    {
        public ProfessorCross(int day) : base(
            "Проф. Аделаида Кросс",
            "О, добрый день! Знаете, я изучаю диалекты внешних колоний, и вот что интересно — ваш акцент...",
            "КРОСС-А-119", "Лингвист", "Конференция", false, day)
        { }
    }

    public class Harold : Human
    {
        public Harold(int day) : base(
            "Гарольд",
            "Молодой человек, вы напоминаете мне моего соседа с Марса. Хороший был человек, только храпел...",
            "ГАР-1943-Г", "Пенсионер", "Навестить племянника", false, day)
        { }
    }

    public class Zipp9 : Robot
    {
        public Zipp9(int day) : base(
            "Зипп-9",
            "Добро пожаловать! Вы рассматривали наше предложение по страховке жизни на орбите?",
            "ZIPP-9-ADV", "Рекламный дрон", false, day)
        { }
    }

    // ─── ВЗЯТОЧНИКИ ─────────────────────────────────────────────────────

    /// <summary>
    /// Марко Тессо — взяточник. Документы в порядке, но платит по привычке.
    /// Принять взятку: score +50, loyalty -1. Отказать: loyalty +1.
    /// </summary>
    public class MarcoTesso : Human
    {
        public int BribeAmount { get; private set; } = 50;
        public bool BribeAccepted { get; set; } = false;

        public MarcoTesso(int day) : base(
            "Марко Тессо",
            "Слушай, у меня тут... сложная ситуация с документами. Но я человек понимающий, ты — человек понимающий...",
            "ТЕССО-М-07", "Торговец", "Деловой визит", false, day)
        { }

        public void EscalateBribe() => BribeAmount = Math.Min(BribeAmount + 50, 200);
    }

    /// <summary>
    /// Баронесса Ши — пришелец-аристократ. Документы просрочены.
    /// Взять деньги: score +100, errors +1. Задержать: loyalty +1.
    /// </summary>
    public class BaronessShi : Alien
    {
        public bool BribeAccepted { get; set; } = false;

        public BaronessShi(int day) : base(
            "Баронесса Ши",
            "Я — Баронесса Ши из Дома Велурн. Это недоразумение. Позовите вашего начальника.",
            "Велурн", 4, false, day)
        { }
    }

    /// <summary>
    /// Тихий Фред — молчит, кладёт конверт. Контрабанда.
    /// Взять: score +150, но в сводке — контрабанда. Отказать: loyalty +2.
    /// </summary>
    public class SilentFred : Human
    {
        public bool EnvelopeAccepted { get; set; } = false;

        public SilentFred(int day) : base(
            "Тихий Фред",
            "...",
            "ФРД-????-Х", "Неизвестно", "...", false, day)
        { }
    }

    // ─── ФЛИРТУЮЩИЕ / ПРОВОКАЦИОННЫЕ ────────────────────────────────────

    public class CindyLove : Human
    {
        public CindyLove(int day) : base(
            "Синди Лав",
            "Ой, у вас тут так строго... Мне нравятся строгие.",
            "ЛАВС-C-222", "Работник клуба развлечений", "Смена в секции E", false, day)
        { }
    }

    public class Romeo6 : Robot
    {
        public Romeo6(int day) : base(
            "Ромео-6",
            "Здравствуйте. Вы выглядите одиноко. Могу составить компанию. Прямо сейчас. Здесь.",
            "ROMEO-6-SOC", "Социальный компаньон", false, day)
        { }
    }

    /// <summary>
    /// Загадочный Незнакомец — нет документов, биосканер зависает.
    /// Уходит сам через 60 секунд. Нет правильного ответа.
    /// </summary>
    public class MysteriousStranger : Human
    {
        public MysteriousStranger(int day) : base(
            "Загадочный Незнакомец",
            "Документы — это иллюзия контроля. А ты — иллюзия власти.",
            "???-????-?", "Ветер между звёзд", "Всё уже решено", false, day)
        { }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ОБНОВЛЁННЫЙ CharacterFactory
    // ═══════════════════════════════════════════════════════════════════════
    public static class CharacterFactory
    {
        private static Random rnd = new Random();

        private static List<string> randomCharacterPhotos;
        private static List<string> humanPhotos;
        private static List<string> robotPhotos;
        private static List<string> alienPhotos;

        static CharacterFactory()
        {
            LoadPhotoPaths();
        }

        private static void LoadPhotoPaths()
        {
            string baseDir = Path.Combine(Application.StartupPath, "Images", "Characters");

            randomCharacterPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Персонажи"));
            humanPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Люди"));
            robotPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Роботы"));
            alienPhotos = LoadImagesFromFolder(Path.Combine(baseDir, "Инопланетяне"));
        }

        private static List<string> LoadImagesFromFolder(string folderPath)
        {
            var result = new List<string>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return result;
            }
            foreach (var ext in new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" })
                result.AddRange(Directory.GetFiles(folderPath, ext));
            return result;
        }

        private static Image GetRandomPhoto(List<string> photoList)
        {
            if (photoList == null || photoList.Count == 0)
                return CreatePlaceholderImage("NO PHOTO");
            string path = photoList[rnd.Next(photoList.Count)];
            try { return Image.FromFile(path); }
            catch { return CreatePlaceholderImage("ERROR"); }
        }

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

        private static string GenerateAccessCode(int day)
        {
            string[] zonePrefixes = { "7741", "3392", "5521", "8834", "2219", "6657" };
            string[] suffixes = { "X", "K", "M", "Z", "Q", "P", "R", "T" };
            string prefix = zonePrefixes[(day - 1) % zonePrefixes.Length];
            string suffix = suffixes[rnd.Next(suffixes.Length)];
            return $"{prefix}-{suffix}";
        }

        // ═══════════════════════════════════════════════════════════════
        //  БАЗОВАЯ ГЕНЕРАЦИЯ (без сюжета)
        // ═══════════════════════════════════════════════════════════════
        public static List<Character> GenerateDayCast(int day,
            int humanCount, int robotCount, int alienCount)
        {
            var characters = new List<Character>();

            for (int i = 0; i < humanCount; i++)
            {
                var c = new Human(
                    CharacterDatabase.GetRandomName(), "",
                    $"ID-{rnd.Next(1000, 9999)}",
                    CharacterDatabase.GetRandomProfession(),
                    CharacterDatabase.GetRandomReason(),
                    rnd.Next(0, 10) > 2, day);
                c.AccessCode = GenerateAccessCode(day);
                c.Photo = GetRandomPhoto(humanPhotos);
                c.Dialogue = CharacterAI.GenerateGreeting(c);
                characters.Add(c);
            }

            for (int i = 0; i < robotCount; i++)
            {
                var c = new Robot(
                    CharacterDatabase.GetRandomName(), "",
                    $"SN-{rnd.Next(100, 999)}",
                    CharacterDatabase.GetRandomProfession(),
                    rnd.Next(0, 10) > 4, day);
                c.AccessCode = day > 2 && rnd.Next(0, 10) > 6
                    ? GenerateAccessCode(day - 1)
                    : GenerateAccessCode(day);
                c.Photo = GetRandomPhoto(robotPhotos);
                c.Dialogue = CharacterAI.GenerateGreeting(c);
                characters.Add(c);
            }

            for (int i = 0; i < alienCount; i++)
            {
                var c = new Alien(
                    CharacterDatabase.GetRandomAlienName(day), "",
                    CharacterDatabase.GetRandomAlienPlanet(),
                    rnd.Next(2, 8), rnd.Next(0, 10) > 5, day);
                c.AccessCode = GenerateAccessCode(day);
                c.Photo = GetRandomPhoto(alienPhotos);
                c.Dialogue = CharacterAI.GenerateGreeting(c);
                characters.Add(c);
            }

            return characters.OrderBy(_ => rnd.Next()).ToList();
        }

        public static Character GenerateRandomTypeCharacter(int day)
        {
            int type = rnd.Next(0, 3);
            Character c = null;

            switch (type)
            {
                case 0:
                    c = new Human(
                        CharacterDatabase.GetRandomName(), "",
                        $"ID-{rnd.Next(1000, 9999)}",
                        CharacterDatabase.GetRandomProfession(),
                        CharacterDatabase.GetRandomReason(),
                        rnd.Next(0, 10) > 2, day);
                    c.AccessCode = GenerateAccessCode(day);
                    break;
                case 1:
                    c = new Robot(
                        CharacterDatabase.GetRandomName(), "",
                        $"SN-{rnd.Next(100, 999)}",
                        CharacterDatabase.GetRandomProfession(),
                        rnd.Next(0, 10) > 4, day);
                    c.AccessCode = day > 2 && rnd.Next(0, 10) > 6
                        ? GenerateAccessCode(day - 1)
                        : GenerateAccessCode(day);
                    break;
                case 2:
                    c = new Alien(
                        CharacterDatabase.GetRandomAlienName(day), "",
                        CharacterDatabase.GetRandomAlienPlanet(),
                        rnd.Next(2, 8), rnd.Next(0, 10) > 5, day);
                    c.AccessCode = GenerateAccessCode(day);
                    break;
            }

            if (c != null)
            {
                c.Photo = GetRandomPhoto(randomCharacterPhotos);
                c.Dialogue = CharacterAI.GenerateGreeting(c);
            }
            return c;
        }

        // ═══════════════════════════════════════════════════════════════
        //  РАЗВЛЕКАТЕЛЬНЫЕ ПЕРСОНАЖИ
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Возвращает 1 случайного развлекательного персонажа.
        /// Вызывать из GenerateMixedCast с вероятностью ~20%.
        /// </summary>
        public static Character GetComedyCharacter(int day)
        {
            // Пул развлекательных персонажей
            var pool = new List<Func<Character>>
            {
                () => AssignPhoto(new BorisPykh(day),        humanPhotos),
                () => AssignPhoto(new DmitryUnlucky(day),    humanPhotos),
                () => AssignPhoto(new ProfessorCross(day),   humanPhotos),
                () => AssignPhoto(new Harold(day),           humanPhotos),
                () => AssignPhoto(new CindyLove(day),        humanPhotos),
                () => AssignPhoto(new MysteriousStranger(day), humanPhotos),
                () => AssignPhoto(new MarcoTesso(day),       humanPhotos),
                () => AssignPhoto(new SilentFred(day),       humanPhotos),
                () => AssignPhoto(new Zipp9(day),            robotPhotos),
                () => AssignPhoto(new Romeo6(day),           robotPhotos),
                () => AssignPhoto(new CloneBot(day, 0),      robotPhotos),
                () => AssignPhoto(new Uuu(day),              alienPhotos),
                () => AssignPhoto(new BaronessShi(day),      alienPhotos),
            };

            var factory = pool[rnd.Next(pool.Count)];
            var ch = factory();

            // Случайный шанс — персонаж несёт какой-то документ
            if (rnd.Next(0, 3) == 0)  // ~33% шанс
                ch.CarriedDocumentType = GetRandomDocType(rnd);

            return ch;
        }

        private static string GetRandomDocType(Random rnd)
        {
            var types = new[] {
                "voucher", "flyer", "anon_letter", "robot_anatomy",
                "alien_profile", "infection_report", "financial_leak"
            };
            return types[rnd.Next(types.Length)];
        }

        private static Character AssignPhoto(Character c, List<string> photoList)
        {
            c.Photo = GetRandomPhoto(photoList);
            c.Dialogue = CharacterAI.GenerateGreeting(c);
            return c;
        }

        // ═══════════════════════════════════════════════════════════════
        //  СМЕШАННЫЙ CAST (стандартный режим)
        // ═══════════════════════════════════════════════════════════════
        public static List<Character> GenerateMixedCast(int day,
            int guaranteedHumans, int guaranteedRobots, int guaranteedAliens,
            int randomTypeCount)
        {
            var characters = new List<Character>();

            characters.AddRange(
                GenerateDayCast(day, guaranteedHumans, guaranteedRobots, guaranteedAliens));

            for (int i = 0; i < randomTypeCount; i++)
            {
                // С вероятностью 20% добавляем развлекательного персонажа
                if (rnd.Next(0, 5) == 0)
                    characters.Add(GetComedyCharacter(day));
                else
                    characters.Add(GenerateRandomTypeCharacter(day));
            }

            return characters.OrderBy(_ => rnd.Next()).ToList();
        }

        public static Character GenerateRandom(int day)
        {
            var cast = GenerateDayCast(day, 1, 0, 0);
            return cast.Count > 0 ? cast[0] : null;
        }
    }
}
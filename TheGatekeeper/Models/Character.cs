using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TheGatekeeper.Models
{
    public abstract class Character
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public string AccessCode { get; set; }
        public string AccessZone { get; set; }
        public Image Photo { get; set; }
        public string Dialogue { get; set; }
        public string Species { get; set; }
        public bool IsObvious { get; set; }
        public string Occupation { get; set; }
        public string ReasonToEnter { get; set; }
        public int Day { get; set; }

        // ─── Новое свойство: персонаж-наблюдатель, не требует классификации ──
        // Если true — кнопки ROBOT/ALIEN/HUMAN скрываются, показывается кнопка [ПРОПУСТИТЬ]
        public virtual bool IsObserver => false;

        public Character(string name, string dialogue, string species, bool isObvious,
                         string occupation, string reason, int day)
        {
            Name = name;
            Dialogue = dialogue;
            Species = species;
            IsObvious = isObvious;
            Occupation = occupation;
            ReasonToEnter = reason;
            Day = day;
        }

        /// <summary>
        /// Загружает фото. Путь может быть абсолютным или относительным к Application.StartupPath.
        /// </summary>
        public void SetPhotoFromFile(string path)
        {
            // Сначала пробуем как есть (абсолютный путь)
            if (!File.Exists(path))
            {
                // Затем относительно стартовой директории
                path = Path.Combine(Application.StartupPath, path);
            }

            if (File.Exists(path))
            {
                try
                {
                    Photo = Image.FromFile(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
                    Photo = CreatePhotoPlaceholder(Name);
                }
            }
            else
            {
                Console.WriteLine($"Фото не найдено: {path}");
                Photo = CreatePhotoPlaceholder(Name);
            }
        }

        private static Image CreatePhotoPlaceholder(string name)
        {
            var bmp = new Bitmap(300, 300);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(25, 30, 35));
                using (var font = new Font("Consolas", 13, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(80, 100, 130)))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString($"[{name}]", font, brush,
                        new RectangleF(0, 0, 300, 300), sf);
                }
            }
            return bmp;
        }

        // ─── Вложенные классы ────────────────────────────────────────────────

        public class Human : Character
        {
            public string IdNumber { get; set; }
            public Human(string name, string dialogue, string idNumber, string occupation,
                         string reason, bool isObvious, int day = 1)
                : base(name, dialogue, "Human", isObvious, occupation, reason, day)
            {
                IdNumber = idNumber;
            }
        }

        public class Robot : Character
        {
            public string SerialNumber { get; set; }
            public string Model { get; set; }
            public Robot(string name, string dialogue, string serialNumber, string occupation,
                         bool isObvious, int day = 1)
                : base(name, dialogue, "Robot", isObvious, occupation, dialogue, day)
            {
                SerialNumber = serialNumber;
            }
        }

        public class Alien : Character
        {
            public string HomePlanet { get; set; }
            public int Tentacles { get; set; }
            public Alien(string name, string dialogue, string homePlanet, int tentacles,
                         bool isObvious, int day = 1)
                : base(name, dialogue, "Alien", isObvious, "Observer", dialogue, day)
            {
                HomePlanet = homePlanet;
                Tentacles = tentacles;
            }
        }
    }
}
using System;
using System.Drawing;
using System.IO;

namespace TheGatekeeper.Models
{
    // Исправленная версия модели Character:
    // Свойство Photo изменено с string на Image, чтобы корректно хранить загруженное изображение.
    // Если в проекте где-то присваивается Image напрямую, это теперь будет компилироваться.
    public abstract class Character
    {
        public string Name { get; set; }
        public string Profession { get; set; }  // профессия
        public string AccessCode { get; set; }  // код доступа
        public string AccessZone { get; set; }
        public Image Photo { get; set; }                // <-- изменено: теперь Image, а не string
        public string Dialogue { get; set; }
        public string Species { get; set; }
        public bool IsObvious { get; set; }
        public string Occupation { get; set; }
        public string ReasonToEnter { get; set; }
        public int Day { get; set; }

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
        public void SetPhotoFromFile(string path)
        {
            Console.WriteLine($"Попытка загрузки фото: {path}"); // Добавьте это
            if (File.Exists(path))
            {
                Photo = Image.FromFile(path);
                Console.WriteLine("Успешно загружено!");
            }
            else
            {
                Console.WriteLine("ФАЙЛ НЕ НАЙДЕН!");
            }
        }
        

        // ЛЮДИ
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

        // РОБОТЫ
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

        // ПРИШЕЛЬЦЫ
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
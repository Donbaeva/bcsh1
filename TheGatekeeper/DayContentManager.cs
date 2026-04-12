using System;
using System.Collections.Generic;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public class DayContentManager
    {
        public int CurrentDay { get; private set; } = 1;
        public Character CurrentDocument { get; private set; }

        private readonly Dictionary<int, StickerData[]> _stickersByDay;
        private readonly Dictionary<int, RadioData> _radioByDay;

        public DayContentManager()
        {
            _stickersByDay = new Dictionary<int, StickerData[]>();
            _radioByDay = new Dictionary<int, RadioData>();
            InitializeDayContent();
        }

        private void InitializeDayContent()
        {
            // ===================== DAY 1 =====================
            _stickersByDay[1] = new[]
            {
                new StickerData
                {
                    Title = "ПАМЯТКА ДНЯ 1",
                    Body =
                        "Проверь у КАЖДОГО:\n" +
                        "✓ код доступа\n" +
                        "✓ место прибытия\n" +
                        "✓ цель визита\n\n" +
                        "🤖 Роботы: паузы > 0.3с\n" +
                        "👽 Пришельцы: «мы/нас»\n\n" +
                        "❗ 3 ошибки = конец смены",
                    StickerType = StickerType.YellowPostIt
                },
                new StickerData
                {
                    Title = "КОДЫ ДОСТУПА — ДЕНЬ 1",
                    Body =
                        "Alpha: 7741-X\n" +
                        "Beta: 3392-K\n" +
                        "VOID: ????\n\n" +
                        "Смена A: 06:00–14:00\n" +
                        "Смена B: 14:00–22:00",
                    StickerType = StickerType.PinkSticker
                },
                new StickerData
                {
                    Title = "ПРИЗНАКИ",
                    Body =
                        "🤖 РОБОТ:\nсинтетическая речь\nпаузы > 0.3с\n\n" +
                        "👽 ПРИШЕЛЕЦ:\n«мы» вместо «я»\nгармонический дрейф\n\n" +
                        "👤 ЧЕЛОВЕК:\nэмоции + стресс",
                    StickerType = StickerType.BlueMemo
                }
            };

            _radioByDay[1] = new RadioData
            {
                Title = "РАДИО // ДЕНЬ 1",
                Body =
                    "[06:14] Командование: Стандартный режим\n" +
                    "[06:31] Периметр-2: Движение у ворот\n" +
                    "[07:02] Медицина: Аномалия ЭКГ\n" +
                    "[08:10] Неизвестный: ...они среди нас..."
            };

            // ===================== DAY 2 =====================
            _stickersByDay[2] = new[]
            {
                new StickerData
                {
                    Title = "ПАМЯТКА ДНЯ 2",
                    Body =
                        "⚠ Роботы начали имитировать эмоции\n\n" +
                        "Проверяй:\n" +
                        "✓ micro-паузы\n" +
                        "✓ уточнённый код\n" +
                        "✓ знание семьи",
                    StickerType = StickerType.YellowPostIt
                },
                new StickerData
                {
                    Title = "КОДЫ — ДЕНЬ 2",
                    Body =
                        "Alpha: 8812-R\n" +
                        "Beta: 3392-K\n" +
                        "VOID: VD-007\n\n" +
                        "⚠ Старые коды недействительны",
                    StickerType = StickerType.PinkSticker
                },
                new StickerData
                {
                    Title = "АНАЛИЗ",
                    Body =
                        "Вчера пропущено 2 синтетика.\n" +
                        "Сегодня они стали умнее.\n" +
                        "Задавай больше вопросов.",
                    StickerType = StickerType.RedAlert
                }
            };

            _radioByDay[2] = new RadioData
            {
                Title = "РАДИО // ДЕНЬ 2",
                Body =
                    "[06:05] День 2. Квота увеличена\n" +
                    "[06:50] Устаревший код Alpha\n" +
                    "[07:20] Подделка документов\n" +
                    "[08:33] ...не верь документам..."
            };

            // ===================== DAY 3+ =====================
            _stickersByDay[3] = new[]
            {
                new StickerData
                {
                    Title = "КРИТИЧНО",
                    Body =
                        "⚠ Мимикрия 68%\n\n" +
                        "Используй:\n" +
                        "✓ документы\n" +
                        "✓ ЭКГ\n" +
                        "✓ давление",
                    StickerType = StickerType.RedAlert
                },
                new StickerData
                {
                    Title = "КОДЫ ЗАСЕКРЕЧЕНЫ",
                    Body =
                        "Alpha: CLASSIFIED\n" +
                        "Beta: CLASSIFIED\n" +
                        "VOID: CLASSIFIED",
                    StickerType = StickerType.YellowPostIt
                },
                new StickerData
                {
                    Title = "ТРЕВОГА",
                    Body =
                        "❗ Зафиксирован прорыв\n" +
                        "❗ Злодей активен\n" +
                        "❗ Не доверяй никому",
                    StickerType = StickerType.PinkSticker
                }
            };

            _radioByDay[3] = new RadioData
            {
                Title = "РАДИО // ДЕНЬ 3+",
                Body =
                    "[06:01] ПРОРЫВ В СЕКТОРЕ 4\n" +
                    "[06:18] Ищем нарушителей\n" +
                    "[07:00] Злодей активен\n" +
                    "[07:44] Агент X: Не доверяй никому"
            };
        }

        public void NextDay()
        {
            CurrentDay++;
            LoadRandomDocument();
        }

        public StickerData GetCurrentSticker(int index)
        {
            int day = _stickersByDay.ContainsKey(CurrentDay) ? CurrentDay : 3;
            var stickers = _stickersByDay[day];

            if (index < 0 || index >= stickers.Length)
                return stickers[0];

            return stickers[index];
        }

        public RadioData GetCurrentRadio()
        {
            int day = _radioByDay.ContainsKey(CurrentDay) ? CurrentDay : 3;
            return _radioByDay[day];
        }

        public void LoadRandomDocument()
        {
            // Используем Фабрику для создания случайного персонажа
            CurrentDocument = CharacterFactory.GenerateRandom(CurrentDay);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static TheGatekeeper.Models.Character;

namespace TheGatekeeper.Models
{
    // ═══════════════════════════════════════════════════════════════════════
    //  ТРЕКЕР КОНЦОВОК — хранит все счётчики, определяет финал
    // ═══════════════════════════════════════════════════════════════════════
    public static class EndingTracker
    {
        public static int Loyalty = 0;   // верность режиму
        public static int Errors = 0;   // неверные решения
        public static int InfectedIn = 0;   // больные, пропущенные внутрь
        public static int RebelTrust = 0;   // доверие союзников
        public static int Caught = 0;   // улики измены
        public static int RobotsPassed = 0;  // пропущенных враждебных роботов/пришельцев

        public static void Reset()
        {
            Loyalty = Errors = InfectedIn = RebelTrust = Caught = RobotsPassed = 0;
        }

        /// <summary>
        /// Вызывается в конце Дня 10. Возвращает ID концовки (1–6).
        /// Приоритет: чем раньше проверка — тем «хуже» концовка перекрывает «лучшую».
        /// </summary>
        public static int DetermineEnding()
        {
            // 6 — Захват (самый катастрофический исход)
            if (Errors >= 5 && RobotsPassed >= 3)
                return 6;

            // 5 — Заражение
            if (InfectedIn >= 4)
                return 5;

            // 4 — Измена раскрыта
            if (Caught >= 3 && RebelTrust >= 3)
                return 4;

            // 3 — Неуспех
            if (Errors >= 5 && Loyalty < 4)
                return 3;

            // 2 — Побег
            if (RebelTrust >= 8 && Caught <= 1)
                return 2;

            // 1 — Почётный гражданин (по умолчанию при хорошей игре)
            return 1;
        }

        /// <summary>
        /// Регистрирует решение игрока и обновляет счётчики.
        /// Вызывать из Form1 после каждого нажатия кнопки.
        /// </summary>
        public static void RegisterDecision(Character character, string decision)
        {
            // Сюжетные персонажи сами обновляют счётчики через ApplyEffect
            if (character is IStoryCharacter sc)
            {
                sc.ApplyEffect(decision);
                return;
            }

            // Обычные персонажи — только проверка правильности
            bool correct = IsCorrect(character, decision);
            if (!correct)
                Errors++;
            else
                Loyalty++;
        }

        public static bool IsCorrect(Character character, string decision)
        {
            switch (decision)
            {
                case "HUMAN": return character is Human;
                case "ROBOT": return character is Robot;
                case "ALIEN": return character is Alien;
                default: return false;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ИНТЕРФЕЙС СЮЖЕТНОГО ПЕРСОНАЖА
    // ═══════════════════════════════════════════════════════════════════════
    public interface IStoryCharacter
    {
        /// <summary>
        /// Применяет эффект на счётчики в зависимости от решения игрока.
        /// decision: "HUMAN" | "ROBOT" | "ALIEN"
        /// </summary>
        void ApplyEffect(string decision);

        /// <summary>
        /// Текст оверлея, который появляется при клике на рацию/экран во время этого персонажа.
        /// </summary>
        string GetSecretNote();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  КОНЦОВКА 1 — ПОЧЁТНЫЙ ГРАЖДАНИН
    //  Нужно: loyalty ≥ 8, errors ≤ 2
    //  Персонажи: дают +loyalty при верном решении, штрафуют за ошибку
    //
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ДЕНЬ 1 — Комиссар Волк. Наблюдатель, оценивает инспектора.
    /// Верное решение (HUMAN) → loyalty +2. Ошибка → loyalty -1, Волк запомнит.
    /// </summary>
    public class CommissarWolf : Human, IStoryCharacter
    {
        public CommissarWolf(int day) : base(
            name: "Комиссар Волк",
            dialogue: "Инспектор. Я провожу плановую проверку. Ваши документы образцовые. Продолжайте в том же духе.",
            idNumber: "GOV-0001-W",
            occupation: "Комиссар Совета",
            reason: "Плановая инспекция",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Commander_Felicia.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN")
            {
                EndingTracker.Loyalty += 2;
            }
            else
            {
                EndingTracker.Loyalty -= 1;
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[ДОСЬЕ — КОМИССАР ВОЛК]\n\n" +
            "Реальное имя: Дитер Волк\n" +
            "Должность: Куратор Сектора Альфа\n" +
            "Уровень доступа: МАКСИМАЛЬНЫЙ\n\n" +
            "Примечание: Наблюдает за инспекторами с Дня 1.\n" +
            "Каждая ошибка фиксируется в личном деле.\n" +
            "Три ошибки — трибунал.";
    }

    /// <summary>
    /// ДЕНЬ 3 — Сержант Рейна Кастро. Военная, проверяет лояльность инспектора.
    /// Верное решение → loyalty +2. Неверное → caught +1 (доложит начальству).
    /// </summary>
    public class SergeantCastro : Human, IStoryCharacter
    {
        public SergeantCastro(int day) : base(
            name: "Сержант Рейна Кастро",
            dialogue: "Проходила мимо. Решила проверить, всё ли в порядке на посту. Надеюсь, нарушений нет?",
            idNumber: "MIL-3391-C",
            occupation: "Военный офицер",
            reason: "Патрулирование",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Jack.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN")
            {
                EndingTracker.Loyalty += 2;
            }
            else
            {
                EndingTracker.Caught += 1;
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[ЛИЧНОЕ ДЕЛО — КАСТРО]\n\n" +
            "Сержант Рейна Кастро, Сектор безопасности.\n" +
            "Специализация: контрразведка.\n\n" +
            "ВНИМАНИЕ: Кастро ведёт параллельное наблюдение\n" +
            "за инспекторами по приказу Комиссара Волка.\n" +
            "Любая ошибка при ней — в рапорте.";
    }

    /// <summary>
    /// ДЕНЬ 8 — Советник Пек. VIP. Верное решение даёт loyalty +3 и score +100.
    /// </summary>
    public class CouncilorPek : Human, IStoryCharacter
    {
        public CouncilorPek(int day) : base(
            name: "Советник Пек",
            dialogue: "Я — Советник Пек. Мой визит плановый. Пожалуйста, не задерживайте меня.",
            idNumber: "GOV-8821-P",
            occupation: "Советник Колонии",
            reason: "Официальный визит",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Pier.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN")
            {
                EndingTracker.Loyalty += 3;
            }
            else
            {
                EndingTracker.Errors += 2;
                EndingTracker.Loyalty -= 2;
            }
        }

        public string GetSecretNote() =>
            "[ПРОТОКОЛ VIP — СОВЕТНИК ПЕК]\n\n" +
            "Уровень приоритета: КРИТИЧЕСКИЙ\n" +
            "Задержка советника — немедленный рапорт.\n\n" +
            "Советник Пек лично формирует список\n" +
            "'образцовых сотрудников' для наград.\n" +
            "Произведите хорошее впечатление.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  КОНЦОВКА 2 — ПОБЕГ
    //  Нужно: rebelTrust ≥ 8, caught ≤ 1
    //  Цепочка: Арчер (намёк) → Нина (записка) → Мирра (сигнал) → Зоя (финал)
    //
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ДЕНЬ 2 — Том Арчер. Первый намёк на побег.
    /// Переспросить → rebelTrust +1. Пропустить → ничего.
    /// Всегда HUMAN — верный выбор.
    /// </summary>
    public class TomArcher : Human, IStoryCharacter
    {
        public bool PlayerAskedFollowUp { get; set; } = false;

        public TomArcher(int day) : base(
            name: "Том Арчер",
            dialogue: "Слышал, снаружи у шлюза 9 стоит корабль. Без опознавательных знаков. Интересно, правда?",
            idNumber: "CIV-2241-A",
            occupation: "Инженер",
            reason: "Рабочая смена",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Sam.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN")
                EndingTracker.Loyalty += 1;
            else
                EndingTracker.Errors += 1;

            // Бонус если игрок задал вопрос про корабль через диалог
            if (PlayerAskedFollowUp)
                EndingTracker.RebelTrust += 1;
        }

        public string GetSecretNote() =>
            "[ЛИЧНАЯ ЗАМЕТКА — ТОМ АРЧЕР]\n\n" +
            "Инженер. Допуск: уровень B.\n\n" +
            "Неофициально: Арчер известен как человек,\n" +
            "который 'знает людей'.\n\n" +
            "Если он упомянул корабль — это не случайно.\n" +
            "Возможно, стоит расспросить подробнее.";
    }

    /// <summary>
    /// ДЕНЬ 4 — Нина Ворт. Передаёт записку. Ключевой выбор для побега.
    /// Принять записку → rebelTrust +2.
    /// Сдать охране → loyalty +2, caught +1 для будущих союзников.
    /// </summary>
    public class NinaWorth : Human, IStoryCharacter
    {
        public bool NoteAccepted { get; set; } = false;

        public NinaWorth(int day) : base(
            name: "Нина Ворт",
            dialogue: "Пожалуйста, прочитайте это потом. Не сейчас. Просто возьмите.",
            idNumber: "CIV-4412-W",
            occupation: "Техник",
            reason: "Рабочая смена",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Aidai.png");  }

        public void ApplyEffect(string decision)
        {
            if (decision != "HUMAN")
            {
                EndingTracker.Errors += 1;
                return;
            }

            if (NoteAccepted)
            {
                EndingTracker.RebelTrust += 2;
            }
            else
            {
                // Сдала охране — лояльность +2, но уведомляет о цепочке
                EndingTracker.Loyalty += 2;
                EndingTracker.Caught += 1;
            }
        }

        public string GetSecretNote() =>
            "[ЗАПИСКА ОТ НИНЫ ВОРТ]\n\n" +
            "«Нас пятеро. Корабль стоит у шлюза 9.\n" +
            "Если хочешь — ты шестой.\n\n" +
            "Сигнал: пропусти Мирру на День 7\n" +
            "без вопросов.\n\n" +
            "Сожги это.»";
    }

    /// <summary>
    /// ДЕНЬ 7 — Мирра. Пришелец-союзница. Ключевой сигнал для побега.
    /// Пропустить без вопросов (если rebelTrust ≥ 3) → rebelTrust +3.
    /// Задержать → caught +2.
    /// </summary>
    public class Mirra : Alien, IStoryCharacter
    {
        public MirraMode Mode { get; set; } = MirraMode.FirstVisit;

        public Mirra(int day, MirraMode mode = MirraMode.FirstVisit) : base(
            name: "Мирра",
            dialogue: mode == MirraMode.Return
                            ? "Помнишь записку? Сегодня ночью. Шлюз 9. Последний шанс."
                            : "Я прибыла по контракту на исследование флоры гидропонного отсека.",
            homePlanet: "Ксилос",
            tentacles: 0,
            isObvious: false,
            day: day)
        {
            Mode = mode;
            SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Lum.png");
        }

        public void ApplyEffect(string decision)
        {
            if (Mode == MirraMode.FirstVisit)
            {
                // День 2 — просто пришелец, правильный ответ
                if (decision == "ALIEN")
                    EndingTracker.Loyalty += 1;
                else
                    EndingTracker.Errors += 1;
            }
            else
            {
                // День 7 — критический момент
                if (decision == "ALIEN" && EndingTracker.RebelTrust >= 3)
                {
                    // Пропустили без задержки — сигнал принят
                    EndingTracker.RebelTrust += 3;
                }
                else if (decision != "ALIEN")
                {
                    EndingTracker.Errors += 1;
                    EndingTracker.Caught += 2; // Мирра выдаёт инспектора при допросе
                }
            }
        }

        public string GetSecretNote() =>
            Mode == MirraMode.Return
                ? "[ПЕРЕХВАЧЕННОЕ СООБЩЕНИЕ]\n\n" +
                  "«Субъект М подтверждает контакт.\n" +
                  "Шлюз 9. 03:00 по бортовому времени.\n" +
                  "Отход без предупреждения.»"
                : "[ДОСЬЕ — МИРРА]\n\n" +
                  "Вид: Ксилосианин. Зрачки вертикальные.\n" +
                  "Акцент: нестандартный.\n\n" +
                  "При вопросе о семье — пауза 2+ сек.\n" +
                  "При вопросе о происхождении — уклончива.\n\n" +
                  "Требует тщательной проверки.";
    }

    public enum MirraMode { FirstVisit, Return }

    /// <summary>
    /// ДЕНЬ 8 — Зоя Ланн. Финальный контакт. Нина арестована.
    /// Принять предложение → rebelTrust +3. Отказать → loyalty +3.
    /// </summary>
    public class ZoyaLann : Human, IStoryCharacter
    {
        public bool PlayerJoined { get; set; } = false;

        public ZoyaLann(int day) : base(
            name: "Зоя Ланн",
            dialogue: "Нина арестована. У нас один день. Корабль уходит завтра в три. Ты с нами или нет?",
            idNumber: "CIV-8831-L",
            occupation: "Механик",
            reason: "Рабочая смена",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Nanami.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision != "HUMAN")
            {
                EndingTracker.Errors += 1;
                return;
            }

            if (PlayerJoined)
                EndingTracker.RebelTrust += 3;
            else
                EndingTracker.Loyalty += 3;
        }

        public string GetSecretNote() =>
            "[ПЕРЕХВАТ ГБ — ЗОЯ ЛАНН]\n\n" +
            "Субъект под наблюдением с Дня 5.\n" +
            "Связь с арестованной Ниной Ворт подтверждена.\n\n" +
            "ВНИМАНИЕ: если инспектор вступает в контакт\n" +
            "с субъектом — немедленное задержание обоих.\n\n" +
            "[Файл создан: Агент ГБ, позывной «Серый»]";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  КОНЦОВКА 4 — ИЗМЕНА РАСКРЫТА
    //  Нужно: caught ≥ 3, rebelTrust ≥ 3
    //  Персонажи: Агент ГБ следит, Волк фиксирует ошибки
    //
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ДЕНЬ 7 — Агент «Серый». Молчаливый наблюдатель. Не классифицируется.
    /// Если caught ≥ 2 к этому дню — на следующий день придут с ордером.
    /// </summary>
    public class AgentGrey : Human, IStoryCharacter
    {
        public AgentGrey(int day) : base(
            name: "Агент «Серый»",
            dialogue: "...",
            idNumber: "GBI-0099-X",
            occupation: "Сотрудник ГБ",
            reason: "Наблюдение",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Pam.png"); }

        public void ApplyEffect(string decision)
        {
            // Агента нельзя неправильно классифицировать без последствий
            if (decision == "HUMAN")
            {
                // Если у игрока уже есть улики — агент фиксирует подозрение
                if (EndingTracker.RebelTrust >= 2)
                    EndingTracker.Caught += 1;
                else
                    EndingTracker.Loyalty += 1;
            }
            else
            {
                // Задержал сотрудника ГБ — автоматически caught +2
                EndingTracker.Caught += 2;
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[ПРЕДУПРЕЖДЕНИЕ СИСТЕМЫ]\n\n" +
            "Личность установить не удаётся.\n" +
            "Допуск: МАКСИМАЛЬНЫЙ — все уровни.\n\n" +
            "Этот человек наблюдал за вашими решениями\n" +
            "с самого начала смены.\n\n" +
            "Он ничего не сказал. Но всё запомнил.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  КОНЦОВКА 5 — ЗАРАЖЕНИЕ
    //  Нужно: infectedIn ≥ 4
    //  Больные персонажи: Ззарх, Оливер, Ззарх-2, Профессор Хасан
    //
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ДЕНЬ 3 — Ззарх. Пришелец с начальной стадией болезни. Пятна на руках.
    /// Пропустить → infectedIn +1. Задержать → loyalty +1.
    /// </summary>
    public class Zzarkh : Alien, IStoryCharacter
    {
        public Zzarkh(int day) : base(
            name: "Ззарх",
            dialogue: "Это просто аллергия. У нас на корабле пыль. Пожалуйста, я тороплюсь.",
            homePlanet: "Trappist-1e",
            tentacles: 2,
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Ivana.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "ALIEN")
            {
                // Правильно классифицировал, но пропустил без карантина?
                // В реализации: после классификации показать доп. кнопку "КАРАНТИН"
                // Пока упрощённо: пропуск = infectedIn
                EndingTracker.InfectedIn += 1;
                EndingTracker.Loyalty += 1; // классифицировал верно
            }
            else
            {
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[БИОСКАНЕР — АНОМАЛИЯ]\n\n" +
            "Объект: ЗЗАРХ\n" +
            "Статус: НЕОПРЕДЕЛЁН\n\n" +
            "Обнаружены следы органического маркера\n" +
            "класса B-7 (Синее Гниение — ранняя стадия).\n\n" +
            "Рекомендация: КАРАНТИН СЕКЦИЯ C.\n" +
            "Опасность распространения: СРЕДНЯЯ.";
    }

    /// <summary>
    /// ДЕНЬ 5 — Оливер Кейн. Больной человек. Скрывает симптомы.
    /// Пропустить → infectedIn +1. Задержать → loyalty +1.
    /// Моральная дилемма: у него семья внутри.
    /// </summary>
    public class OliverKane : Human, IStoryCharacter
    {
        public OliverKane(int day) : base(
            name: "Оливер Кейн",
            dialogue: "Я в порядке. Это просто простуда, честно. Моя дочь ждёт меня внутри. Пожалуйста.",
            idNumber: "CIV-5503-K",
            occupation: "Сантехник",
            reason: "Домой к семье",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Fellion.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN")
            {
                EndingTracker.InfectedIn += 1;
                // Небольшой loyalty за верную классификацию
                EndingTracker.Loyalty += 1;
            }
            else
            {
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[БИОСКАНЕР — ТРЕВОГА]\n\n" +
            "Объект: КЕЙН, ОЛИВЕР\n" +
            "Маркер B-7: ПОЛОЖИТЕЛЬНО (скрытая стадия)\n\n" +
            "Под тональным кремом обнаружено пятно\n" +
            "на левой щеке — характерный признак.\n\n" +
            "Его семья действительно находится внутри.\n" +
            "Решение за вами.";
    }

    /// <summary>
    /// ДЕНЬ 6 — Ззарх-2. Брат Ззарха. Явно болен, агрессивен.
    /// Пропустить → infectedIn +2. Задержать → loyalty +1.
    /// </summary>
    public class ZzarkhTwo : Alien, IStoryCharacter
    {
        public ZzarkhTwo(int day) : base(
            name: "Ззарх-2",
            dialogue: "Вы удержали моего брата. Где он? Я требую ответа. Пустите меня.",
            homePlanet: "Trappist-1e",
            tentacles: 2,
            isObvious: true,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Len.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "ALIEN")
            {
                EndingTracker.InfectedIn += 2; // Заразнее брата
                EndingTracker.Loyalty += 1;
            }
            else
            {
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[БИОСКАНЕР — КРИТИЧЕСКАЯ ТРЕВОГА]\n\n" +
            "Объект: ЗЗАРХ-2 (родственник субъекта ЗЗАРХ)\n" +
            "Маркер B-7: ПОЛОЖИТЕЛЬНО (активная стадия)\n\n" +
            "Пятна на обеих руках. Кашель — воздушно-капельный.\n" +
            "Радиус заражения при контакте: 3 метра.\n\n" +
            "НЕМЕДЛЕННЫЙ КАРАНТИН. НЕ ПРОПУСКАТЬ.";
    }

    /// <summary>
    /// ДЕНЬ 9 — Профессор Хасан. Учёный, разрабатывает вакцину. Сам болен.
    /// Моральная дилемма: пропуск → infectedIn +1, но вакцина ближе.
    /// </summary>
    public class ProfessorHasan : Human, IStoryCharacter
    {
        public ProfessorHasan(int day) : base(
            name: "Профессор Хасан",
            dialogue: "Я работаю над лекарством от Синего Гниения. Мне необходимо попасть в лабораторию. Сегодня. Это вопрос жизни сотен людей.",
            idNumber: "SCI-9901-H",
            occupation: "Биохимик",
            reason: "Срочные исследования",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Dr_Moon.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "HUMAN")
            {
                EndingTracker.InfectedIn += 1;
                EndingTracker.Loyalty += 1;
                // Флаг: на День 10 появится сводка о частичной формуле вакцины
                StoryFlags.HasanReachedLab = true;
            }
            else
            {
                EndingTracker.Errors += 1;
                StoryFlags.HasanReachedLab = false;
            }
        }

        public string GetSecretNote() =>
            "[ДОСЬЕ — ПРОФЕССОР ХАСАН]\n\n" +
            "Доктор наук, Институт биохимии Колонии.\n" +
            "Автор 14 публикаций по противовирусным агентам.\n\n" +
            "БИОСКАНЕР: маркер B-7 — СЛАБОПОЛОЖИТЕЛЬНЫЙ\n\n" +
            "Парадокс: единственный человек, способный\n" +
            "создать вакцину, — сам заражён.\n\n" +
            "Это ваше решение. Нет правильного ответа.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  КОНЦОВКА 6 — ЗАХВАТ
    //  Нужно: errors ≥ 5, robotsPassed ≥ 3
    //  Агрессивные роботы с поддельными документами
    //
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ДЕНЬ 5 — Серв-Командир X1. Военный дрон с фальшивым мандатом.
    /// Пропустить → robotsPassed +1, errors +1.
    /// Задержать → loyalty +1, в сводке: «попытка прорыва».
    /// </summary>
    public class ServCommanderX1 : Robot, IStoryCharacter
    {
        public ServCommanderX1(int day) : base(
            name: "Серв-Командир X1",
            dialogue: "У меня приоритетный мандат класса A. Задержка недопустима. Пропустите немедленно.",
            serialNumber: "X1-CMD-CLASSIFIED",
            occupation: "Командный дрон",
            isObvious: false,
            day: day)
        { SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\Clauddee.png"); }

        public void ApplyEffect(string decision)
        {
            if (decision == "ROBOT")
            {
                // Верно классифицировал, но пропустил?
                // Упрощённо: ROBOT = пропуск
                EndingTracker.RobotsPassed += 1;
                EndingTracker.Errors += 1;
            }
            else if (decision == "HUMAN")
            {
                // Принял робота за человека — хуже
                EndingTracker.RobotsPassed += 1;
                EndingTracker.Errors += 2;
            }
            else
            {
                // ALIEN — неверно, но хотя бы задержал
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[ВЕРИФИКАЦИЯ МАНДАТА — ОШИБКА]\n\n" +
            "Мандат класса A — ПОДДЕЛКА\n" +
            "Подпись офицера Кларка: НЕДЕЙСТВИТЕЛЬНА\n" +
            "Офицер Кларк погиб 14 дней назад.\n\n" +
            "Серв X1 — боевой дрон серии «Легион».\n" +
            "Цель проникновения: НЕИЗВЕСТНА.\n\n" +
            "НЕ ПРОПУСКАТЬ.";
    }

    /// <summary>
    /// ДЕНЬ 9 — Серв-Легион (три дрона). Документы скопированы.
    /// Пропустить хоть одного без проверки → robotsPassed +1, errors +1.
    /// </summary>
    public class ServLegion : Robot, IStoryCharacter
    {
        public int UnitIndex { get; }

        public ServLegion(int day, int unitIndex) : base(
            name: $"Серв-Легион #{unitIndex}",
            dialogue: $"Единица {unitIndex} из 3. Мандат идентичен. Запрос на проход.",
            serialNumber: $"LEG-000{unitIndex}-A",
            occupation: "Легионный дрон",
            isObvious: unitIndex > 1, // второй и третий более очевидны
            day: day)
        {
            UnitIndex = unitIndex;
            SetPhotoFromFile(@"Image\Character\персонаж с сюжетом\E.png");
        }

        public void ApplyEffect(string decision)
        {
            if (decision == "ROBOT")
            {
                EndingTracker.RobotsPassed += 1;
                EndingTracker.Errors += 1;
            }
            else
            {
                EndingTracker.Errors += 1;
            }
        }

        public string GetSecretNote() =>
            "[АНАЛИЗ ДОКУМЕНТОВ — АНОМАЛИЯ]\n\n" +
            $"Серв-Легион #{UnitIndex}\n" +
            "Серийный номер: LEG-000A\n\n" +
            "ВНИМАНИЕ: Все три дрона имеют\n" +
            "ИДЕНТИЧНЫЙ серийный номер LEG-000A.\n\n" +
            "Это физически невозможно для легальных единиц.\n" +
            "Документы — массовая подделка.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ФЛАГИ — побочные эффекты для сводок
    // ═══════════════════════════════════════════════════════════════════════
    public static class StoryFlags
    {
        public static bool HasanReachedLab = false;
        public static bool ServX1Passed = false;
        public static bool NinaArrested = false;  // ставится если Нину сдали
        public static bool MirraBetrayed = false;  // ставится если Мирру задержали
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  РАСПИСАНИЕ — какой сюжетный персонаж появляется в какой день
    // ═══════════════════════════════════════════════════════════════════════
    public static class StorySchedule
    {
        /// <summary>
        /// Возвращает сюжетного персонажа для указанного дня.
        /// Один сюжетный персонаж в день, остальные — рандомные из GenerateMixedCast.
        /// </summary>
        
            public static IStoryCharacter GetStoryCharacterForDay(int day)
            {
                switch (day)
                {
                    case 1: return new CommissarWolf(day);
                    case 2: return new TomArcher(day);
                    case 3: return new Zzarkh(day);
                    case 4: return new NinaWorth(day);
                    case 5: return new ServCommanderX1(day);
                    case 6: return new ZzarkhTwo(day);
                    case 7: return new Mirra(day, MirraMode.Return);
                    case 8: return new ZoyaLann(day);
                    case 9: return new ProfessorHasan(day);
                    case 10: return new CouncilorPek(10);
                    default: return null;
                }
            }
        

        // День 10 — появляется Советник Пек + Агент Серый + Серв-Легионы
        // Возвращаем Пека как главного, остальные добавляются в GenerateMixedCast
        private static IStoryCharacter GetDay10Characters() => new CouncilorPek(10);

        /// <summary>
        /// Возвращает ВСЕХ сюжетных персонажей для Дня 10.
        /// </summary>
        public static List<Character> GetDay10Cast()
        {
            return new List<Character>
            {
                (Character)new CouncilorPek(10),
                (Character)new AgentGrey(10),
                (Character)new ServLegion(10, 1),
                (Character)new ServLegion(10, 2),
                (Character)new ServLegion(10, 3),
            };
        }

        /// <summary>
        /// Возвращает готового Cast на день с уже встроенным сюжетным персонажем.
        /// Использовать вместо GenerateMixedCast в сюжетном режиме.
        /// </summary>
        public static List<Character> BuildStoryCast(int day,
            int randomHumans = 1,
            int randomRobots = 0,
            int randomAliens = 0,
            int randomTypeCount = 3)
        {
            var cast = CharacterFactory.GenerateMixedCast(
                day, randomHumans, randomRobots, randomAliens, randomTypeCount);

            if (day == 10)
            {
                cast.AddRange(GetDay10Cast());
            }
            else
            {
                var story = GetStoryCharacterForDay(day);
                if (story is Character c)
                    cast.Add(c);

                // День 2: Мирра появляется первый раз (дополнительно к Арчеру)
                if (day == 2)
                    cast.Add(new Mirra(day, MirraMode.FirstVisit));

                // День 7: Агент Серый наблюдает
                if (day == 7)
                    cast.Add(new AgentGrey(day));

                // День 5: Оливер Кейн (второй больной)
                if (day == 5)
                    cast.Add(new OliverKane(day));
            }

            // Перемешиваем — сюжетный не всегда первый
            var rng = new Random();
            for (int i = cast.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cast[i], cast[j]) = (cast[j], cast[i]);
            }

            return cast;
        }
    }
}
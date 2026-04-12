using System.Windows.Forms;
using System.Drawing;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    // Частичный класс — дополнение к основному Form1
    public partial class Form1
    {
        // Убираем отдельное свойство Overlay (используем OverlayManagerInstance)

        private void InitOverlay()
        {
            OverlayManagerInstance = new OverlayManager(this);
        }

        private void HandleInteractiveZoneClick(int zoneIndex)
        {
            OverlayManagerInstance.CurrentDay = day;

            // Исправление ошибки CS8121:
            // currentCharacter в коде может быть либо Character, либо Image.
            // Компилятор выдаёт CS8121, если статический тип currentCharacter несовместим с шаблоном Character.
            // Решение: привести выражение к object и затем выполнять проверки шаблона.
            object cur = currentCharacter;

            if (cur is Character ch)
            {
                OverlayManagerInstance.CurrentCharacter = ch;
            }
            else if (cur is Image img)
            {
                if (OverlayManagerInstance.CurrentCharacter == null)
                {
                    OverlayManagerInstance.CurrentCharacter = new FallbackCharacter
                    {
                        Photo = img,
                        Name = "Unknown",
                    };
                }
                else
                {
                    OverlayManagerInstance.CurrentCharacter.Photo = img;
                }
            }

            switch (zoneIndex)
            {
                case 0: OverlayManagerInstance.ShowDocument(); break;  // Левый монитор → документы
                case 1: OverlayManagerInstance.ShowHeartbeat(); break;  // ЭКГ → кардиограмма с анимацией
                case 2:
                    OverlayManagerInstance.ShowGeneric(           // Панель данных → системный лог
                    "SYSTEM DATA // TERMINAL 7741",
                    "SECURITY LEVEL: 3  (CLASSIFIED)\n" +
                    $"ZONE:           VOID PERIMETER\n" +
                    $"SHIFT:          DAY {day}\n\n" +
                    "THREAT MATRIX:\n" +
                    "  Synthetic entities: MEDIUM ⚠\n" +
                    "  Alien contacts:     MEDIUM ⚠"); break;
                case 3:
                    OverlayManagerInstance.ShowGeneric(           // Правый монитор → системный лог
                    "SYSTEM LOG // VOID TERMINAL",
                    "[06:00:01] Boot sequence complete\n" +
                    "[06:00:05] Biometric scanner: READY\n" +
                    $"[06:01:14] Subject incoming — DAY {day}\n" +
                    "[06:03:22] ANOMALY: non-human signal\n" +
                    "[06:04:10] Awaiting inspector decision"); break;
                case 4: OverlayManagerInstance.ShowSticker(0); break;  // Стикер 1
                case 5: OverlayManagerInstance.ShowSticker(1); break;  // Стикер 2
                case 6: OverlayManagerInstance.ShowSticker(2); break;  // Стикер 3
                case 7:
                    OverlayManagerInstance.ShowGeneric(            // Часы
                    "ХРОНОМЕТР СМЕНЫ",
                    "ШТРАФЫ ЗА ДАВЛЕНИЕ:\n\n" +
                    "   0–60%:  без штрафа\n" +
                    "  60–70%:  −2 очка\n" +
                    "  70–80%:  −3 очка\n" +
                    "  80–100%: −5 очков\n\n" +
                    "Принимай решение быстро."); break;
                case 8:
                    OverlayManagerInstance.ShowGeneric(            // Круг
                    "АВАРИЙНЫЙ ПРОТОКОЛ",
                    "СОСТОЯНИЕ: ONLINE\n\n" +
                    "В случае прорыва злодея:\n" +
                    "  • Протокол LOCKDOWN\n" +
                    "  • Все выходы блокируются\n\n" +
                    "⚠ Пропустил злодея — игра окончена."); break;
                case 9: OverlayManagerInstance.ShowRadio(); break;     // Радио
            }
        }

        // Небольшой fallback-класс, если в currentCharacter приходит только изображение.
        // Character объявлен как abstract, но в сигнатуре не видно абстрактных членов,
        // поэтому простой наследник позволит создать минимальный объект.
        private class FallbackCharacter : Character
        {
            // Вызов базового конструктора с значениями по умолчанию,
            // чтобы удовлетворить требуемую сигнатуру Character(...)
            public FallbackCharacter()
                : base("Unknown", "", "", false, "", "", 0)
            {
            }
        }
    }
}
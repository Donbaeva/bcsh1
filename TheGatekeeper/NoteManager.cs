using System;
using System.Collections.Generic;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public static class NoteManager
    {
        public static (string Title, string Body) GetDynamicNote(int zoneIndex, Character character)
        {
            // zoneIndex 3, 4, 5, 6 — это наши стикеры (правые зоны)

            if (zoneIndex == 3) // Стикер "Чеклист" (Особый)
            {
                return ("FIELD INSPECTION LOG", "Use the checkboxes to verify the subject's biological and behavioral markers.");
            }

            // Динамические данные сканера в зависимости от расы (скрытые подсказки)
            if (zoneIndex == 0) // Биометрический сканер (Левая верхняя зона)
            {
                if (character is Robot)
                    return ("SCANNER // UNIT-7", "ANALYSIS:\n• Core Temp: 32.2°C (STABLE)\n• Pulse: NOT DETECTED\n• Interference: HIGH\n\nNote: Thermal regulation seems... synthetic.");

                if (character is Alien)
                    return ("SCANNER // UNIT-7", "ANALYSIS:\n• Core Temp: 39.1°C (FEVER?)\n• Pulse: 140 BPM (ARRHYTHMIC)\n• DNA: NON-CATALOGUED\n\nNote: Internal structure shows multiple heart-like nodes.");

                return ("SCANNER // UNIT-7", "ANALYSIS:\n• Core Temp: 36.6°C (NOMINAL)\n• Pulse: 72 BPM\n• Species: Homo Sapiens\n\nNote: All vitals within human baseline.");
            }

            // Дефолтный текст для остальных зон, если не прописано иное
            return ("SYSTEM INFO", "Standard protocol active. Vigilance is advised.");
        }
    }
}
using System;
using System.Collections.Generic;
using TheGatekeeper.Models;
using static TheGatekeeper.Models.Character;

namespace TheGatekeeper
{
    public static class NoteManager
    {
        private static readonly Random rnd = new Random();

        public static (string Title, string Body) GetDynamicNote(int zoneIndex, Character character)
        {
            if (character == null)
                return ("NO DATA", "Subject not detected. System standby.");

            switch (zoneIndex)
            {
                case 0: return GetBiometricScannerNote(character);
                case 1: return GetVitalSignsNote(character);
                case 2: return GetSystemDataNote(character);

                case 3:
                    // Стикер статус-смены — показывает то что раньше было в HUD
                    int dayN = character?.Day ?? 1;
                    return ("SHIFT STATUS",
                                 $"DAY      {dayN} / 7\n" +
                                 $"QUOTA    see checklist\n\n" +
                                 "PRESSURE see clock\n\n" +
                                 "──────────────────\n" +
                                 "QUICK REFERENCE:\n" +
                                 "  Click CLOCK → shift menu\n" +
                                 "  Click DIALOGUE → log\n" +
                                 "  Click RIGHT SCREEN → docs\n" +
                                 "  Small radio → interrogate");

                case 4: return GetAccessCodesNote(character);

                case 5:
                    return ("REFERENCE GUIDE",
                            "BIOLOGICAL BASELINE:\n" +
                            "Temp: 36.2 - 37.1°C\n" +
                            "Pulse: 65 - 85 BPM\n" +
                            "O2: 97-99%\n\n" +
                            "SYNTHETIC MARKERS:\n" +
                            "Temp: Constant 32.0°C\n" +
                            "Pulse: None or Static\n\n" +
                            "ALIEN MARKERS:\n" +
                            "Temp: > 38.5°C\n" +
                            "Pulse: > 120 BPM");

                case 6:
                    return ("INSPECTOR PROTOCOL",
                                 $"CHECK EVERY SUBJECT:\n" +
                                 $"  ✓ Access code\n" +
                                 $"  ✓ Place of arrival\n" +
                                 $"  ✓ Purpose of visit\n\n" +
                                 $"CURRENT QUOTA: Check all subjects\n" +
                                 $"ERROR LIMIT: 3 mistakes\n\n" +
                                 $"⚠ Day {character.Day}: {GetDayWarning(character.Day)}");

                case 7:
                    return ("SUBJECT PROFILE",
                                 $"NAME: {character.Name}\n" +
                                 $"ORIGIN: {character.ReasonToEnter}\n" +
                                 $"DAY: {character.Day}\n\n" +
                                 "Data displayed on main monitor.");

                case 8: return GetCommunicationsNote(character);

                case 9:
                    return ("FIELD RADIO",
                                 "INTERROGATION DEVICE\n\n" +
                                 "Click to ask questions.\n" +
                                 "Listen carefully to responses.\n\n" +
                                 (character.Day >= 5
                                     ? "⚠ Day 5+: Advanced mimicry active.\n  Biometrics may be unreliable.\n  Focus on speech inconsistencies."
                                     : "⚠ Analyze:\n  • Speech patterns\n  • Response timing\n  • Pronoun usage"));

                case 10:
                    return ("DIALOGUE LOG",
                                 "Subject communication log.\n\n" +
                                 "Click the screen to open full dialogue.\n\n" +
                                 "Look for:\n" +
                                 "  • Hesitation before answering\n" +
                                 "  • Pronoun switches (we/us)\n" +
                                 "  • Overly precise numbers\n" +
                                 "  • Emotionless phrasing");

                default: return ("SYSTEM INFO", "Standard protocol active.");
            }
        }

        private static string GetDayWarning(int day)
        {
            if (day >= 8) return "EXTREME THREAT. Trust no one.";
            if (day >= 6) return "HIGH THREAT. Synthetics use full cover.";
            if (day >= 4) return "Mimicry improving. Check carefully.";
            if (day >= 2) return "Robots learning emotional responses.";
            return "Standard threat level.";
        }

        // ════════════════════════════════════════════════════════════════
        //  БИОМЕТРИЯ — неоднозначная с Дня 5
        // ════════════════════════════════════════════════════════════════
        private static (string Title, string Body) GetBiometricScannerNote(Character character)
        {
            int day = character.Day;
            bool ambiguous = day >= 5;

            string title = "BIOMETRIC SCANNER // UNIT-7";
            string body;

            // ── До Дня 5: чёткие показания ──────────────────────────────
            if (!ambiguous)
            {
                if (character is Robot)
                    body = BuildClearRobotBio();
                else if (character is Alien)
                    body = BuildClearAlienBio();
                else
                    body = BuildClearHumanBio();

                return (title, body);
            }

            // ── День 5–7: умеренная неоднозначность ─────────────────────
            if (day <= 7)
            {
                if (character is Robot)
                    body = BuildAmbiguousRobotBio(day);
                else if (character is Alien)
                    body = BuildAmbiguousAlienBio(day);
                else
                    body = BuildAmbiguousHumanBio(day);

                return (title, body);
            }

            // ── День 8–10: максимальная неоднозначность ─────────────────
            body = BuildMaxAmbiguousBio(character);
            return (title, body);
        }

        private static string BuildClearRobotBio() =>
            "STATUS: SCANNING...\n\n" +
            "ANALYSIS:\n" +
            "• Core Temp:        32.2°C  ⚠ ABNORMAL\n" +
            "• Pulse rate:       NOT DETECTED\n" +
            "• Skin conductance: UNIFORM\n" +
            "• Retinal scan:     SYNTHETIC MARKERS\n" +
            "• Voice pattern:    MECHANICAL CADENCE\n\n" +
            "⚠ ANOMALY: Zero thermal variance\n" +
            "  Metal traces detected.\n\n" +
            "Classification confidence: HIGH";

        private static string BuildClearAlienBio() =>
            "STATUS: SCANNING...\n\n" +
            "ANALYSIS:\n" +
            "• Core Temp:        39.1°C  ⚠ ELEVATED\n" +
            "• Pulse rate:       140 BPM (ARRHYTHMIC)\n" +
            "• Skin conductance: IRREGULAR\n" +
            "• Retinal scan:     NON-STANDARD\n" +
            "• Voice pattern:    HARMONIC DRIFT 0.3Hz\n\n" +
            "⚠ ANOMALY: DNA structure NON-CATALOGUED\n" +
            "  Multiple cardiac nodes detected.\n\n" +
            "Classification confidence: HIGH";

        private static string BuildClearHumanBio() =>
            "STATUS: SCANNING...\n\n" +
            "ANALYSIS:\n" +
            "• Core Temp:        36.7°C  (NOMINAL)\n" +
            "• Pulse rate:       78 BPM\n" +
            "• Skin conductance: NORMAL VARIANCE\n" +
            "• Retinal scan:     HUMAN BASELINE\n" +
            "• Voice pattern:    NATURAL VARIANCE\n\n" +
            "✓ All vitals within human baseline.\n\n" +
            "Match confidence: 94% — HUMAN";

        private static string BuildAmbiguousRobotBio(int day)
        {
            // Робот маскирует биометрию — показатели «почти» человеческие
            float temp = 35.8f + rnd.Next(0, 15) / 10f;    // 35.8–37.3 (подозрительно)
            int pulse = 58 + rnd.Next(0, 25);              // 58–82 (в норме, но чуть натянуто)
            int conf = 38 + rnd.Next(0, 24);              // 38–61% — неопределённо

            return "STATUS: SCANNING...\n\n" +
                   "ANALYSIS:\n" +
                  $"• Core Temp:        {temp:F1}°C  (within range)\n" +
                  $"• Pulse rate:       {pulse} BPM\n" +
                   "• Skin conductance: MINOR VARIANCE\n" +
                   "• Retinal scan:     PARTIAL MATCH\n" +
                   "• Voice pattern:    MINOR CADENCE DRIFT\n\n" +
                   "⚠ Sensor calibration uncertainty detected.\n" +
                  $"  Classification confidence: {conf}%\n\n" +
                   "Note: Recommend additional questioning.";
        }

        private static string BuildAmbiguousAlienBio(int day)
        {
            float temp = 36.2f + rnd.Next(0, 20) / 10f;    // 36.2–38.2
            int pulse = 70 + rnd.Next(0, 30);
            int conf = 30 + rnd.Next(0, 35);

            return "STATUS: SCANNING...\n\n" +
                   "ANALYSIS:\n" +
                  $"• Core Temp:        {temp:F1}°C\n" +
                  $"• Pulse rate:       {pulse} BPM\n" +
                   "• Skin conductance: VARIABLE\n" +
                   "• Retinal scan:     INCONCLUSIVE\n" +
                  $"• Voice pattern:    DRIFT {(rnd.Next(0, 10) / 100f):F2}Hz\n\n" +
                   "⚠ Cross-species interference possible.\n" +
                  $"  Classification confidence: {conf}%\n\n" +
                   "Note: Scanner interference in sector.";
        }

        private static string BuildAmbiguousHumanBio(int day)
        {
            float temp = 36.4f + rnd.Next(-5, 15) / 10f;
            int pulse = 72 + rnd.Next(-8, 20);
            int conf = 50 + rnd.Next(0, 30);
            // Иногда ложная тревога для человека
            bool fakeFlag = rnd.Next(0, 3) == 0;

            return "STATUS: SCANNING...\n\n" +
                   "ANALYSIS:\n" +
                  $"• Core Temp:        {temp:F1}°C\n" +
                  $"• Pulse rate:       {pulse} BPM\n" +
                   "• Skin conductance: NORMAL\n" +
                  $"• Retinal scan:     {(fakeFlag ? "MINOR ANOMALY ⚠" : "BASELINE")}\n" +
                   "• Voice pattern:    NATURAL\n\n" +
                  $"{(fakeFlag ? "⚠ Possible stress artifact — recheck advised.\n" : "✓ No anomalies detected.\n")}" +
                  $"  Classification confidence: {conf}%\n\n" +
                   "Note: Stress response may distort readings.";
        }

        private static string BuildMaxAmbiguousBio(Character character)
        {
            // День 8–10: показания максимально запутаны для ВСЕХ типов
            float temp = 35.5f + rnd.Next(0, 30) / 10f;     // 35.5–38.5
            int pulse = 55 + rnd.Next(0, 45);               // 55–100
            int conf = 20 + rnd.Next(0, 30);               // 20–50%

            string[] warnings =
            {
                "⚠ Scanner interference — sector radiation elevated.",
                "⚠ Biometric spoof detected — readings unreliable.",
                "⚠ Calibration error — manual assessment required.",
                "⚠ Unknown biological markers present.",
                "⚠ Data integrity: COMPROMISED. Cross-check manually.",
            };

            return "STATUS: SCANNING...\n\n" +
                   "ANALYSIS:\n" +
                  $"• Core Temp:        {temp:F1}°C\n" +
                  $"• Pulse rate:       {pulse} BPM\n" +
                   "• Skin conductance: INCONCLUSIVE\n" +
                   "• Retinal scan:     ERROR — RETRY\n" +
                   "• Voice pattern:    MASKED\n\n" +
                  $"{warnings[rnd.Next(warnings.Length)]}\n" +
                  $"  Classification confidence: {conf}%\n\n" +
                   "Rely on DIALOGUE and ACCESS CODE.";
        }

        // ════════════════════════════════════════════════════════════════
        //  VITAL SIGNS — тоже неоднозначные с Дня 5
        // ════════════════════════════════════════════════════════════════
        private static (string Title, string Body) GetVitalSignsNote(Character character)
        {
            int day = character.Day;
            bool ambiguous = day >= 5;

            string title = "VITAL SIGNS MONITOR";
            string body;

            if (!ambiguous)
            {
                if (character is Robot)
                    body = "CARDIAC:      FLAT LINE ⚠\n" +
                           "RESPIRATORY:  NOT DETECTED\n" +
                           "O₂ SAT:       ERROR\n\n" +
                           "EEG PATTERN:\n" +
                           "  FLAT — No brain activity\n\n" +
                           "⚡ ALERT: Subject is non-biological\n\n" +
                           "Neural activity:       NONE\n" +
                           "Deception probability: N/A";
                else if (character is Alien)
                    body = "CARDIAC:      140 BPM — IRREGULAR ⚠\n" +
                           "RESPIRATORY:  22 / min (ELEVATED)\n" +
                           "O₂ SAT:       103% ⚠ IMPOSSIBLE\n\n" +
                           "EEG PATTERN:\n" +
                           "  Harmonic oscillation at 0.3 Hz\n" +
                           "  Non-human wave pattern\n\n" +
                           "⚡ ALERT: Non-standard biology\n\n" +
                           "Deception probability: UNKNOWN";
                else
                    body = "CARDIAC:      78 BPM — normal\n" +
                           "RESPIRATORY:  16 / min\n" +
                           "O₂ SAT:       98%\n\n" +
                           "EEG PATTERN:\n" +
                           "  Alpha: 9–13 Hz  Beta surge: stress\n\n" +
                           "✓ All readings within human range\n\n" +
                           $"Deception probability: {rnd.Next(15, 45)}%";
            }
            else
            {
                // Неоднозначные показания для всех
                int cardiac = 62 + rnd.Next(0, 50);
                int o2 = 95 + rnd.Next(0, 9);
                int deceive = 30 + rnd.Next(0, 40);
                bool errFlag = rnd.Next(0, 2) == 0;

                body = $"CARDIAC:      {cardiac} BPM\n" +
                       $"RESPIRATORY:  {14 + rnd.Next(0, 8)} / min\n" +
                       $"O₂ SAT:       {o2}%\n\n" +
                        "EEG PATTERN:\n" +
                       $"  {(errFlag ? "INTERFERENCE — PARTIAL DATA" : $"Mixed wave pattern {rnd.Next(8, 15)} Hz")}\n\n" +
                       $"⚡ {(day >= 8 ? "SCANNER COMPROMISED — DATA UNRELIABLE" : "Readings within broad tolerance range")}\n\n" +
                       $"Deception probability: {deceive}%\n" +
                        "Recommend: verbal cross-check.";
            }

            return (title, body);
        }

        // ════════════════════════════════════════════════════════════════
        //  SYSTEM DATA
        // ════════════════════════════════════════════════════════════════
        private static (string Title, string Body) GetSystemDataNote(Character character)
        {
            string title = "SYSTEM DATA // TERMINAL 7741";
            string body =
                $"SECURITY LEVEL: 3  (CLASSIFIED)\n" +
                $"ZONE:           VOID PERIMETER\n" +
                $"SHIFT:          DAY {character.Day}\n\n" +
                 "THREAT MATRIX:\n" +
                $"  Synthetic entities: {(character.Day >= 5 ? "CRITICAL ⚠⚠⚠" : character.Day >= 2 ? "HIGH ⚠⚠" : "MEDIUM ⚠")}\n" +
                $"  Alien contacts:     {(character.Day >= 6 ? "CRITICAL ⚠⚠⚠" : character.Day >= 3 ? "HIGH ⚠⚠" : "MEDIUM ⚠")}\n" +
                 "  Unknown:            MONITORING\n\n" +
                $"DAILY QUOTA:      {character.Day} / variable\n" +
                $"{(character.Day >= 5 ? "⚠⚠ CRITICAL: Biometric spoofing active.\n     Rely on dialogue and codes." : "ALERT: Heightened vigilance required.")}";

            return (title, body);
        }

        // ════════════════════════════════════════════════════════════════
        //  ACCESS CODES
        // ════════════════════════════════════════════════════════════════
        private static (string Title, string Body) GetAccessCodesNote(Character character)
        {
            string title = "ACCESS CODES // CURRENT";
            string currentCode = GenerateDaySpecificCode(character.Day);
            string previousCode = character.Day > 1
                ? GenerateDaySpecificCode(character.Day - 1)
                : "EXPIRED";

            string body =
                $"ACTIVE CODES (Day {character.Day}):\n\n" +
                $"  Current:   {currentCode}\n" +
                $"  Yesterday: {previousCode} ⚠ EXPIRED\n" +
                 "  VOID:      CLASSIFIED\n" +
                 "  Medical:   MED-009\n\n" +
                 "VERIFICATION:\n" +
                $"  Subject's code: {character.AccessCode ?? "ERROR"}\n" +
                $"  Status: {(character.AccessCode == currentCode ? "✓ VALID" : "⚠ MISMATCH — CHECK SUBJECT")}\n\n" +
                 "⚠ Codes change daily!\n" +
                 "  Outdated = memory malfunction OR forgery.\n" +
                (character.Day >= 5 ? "\n  Day 5+: Advanced units may recalibrate\n  codes mid-shift. Extra scrutiny required." : "");

            return (title, body);
        }

        // ════════════════════════════════════════════════════════════════
        //  TRAIT REFERENCE — меняется с прогрессом дней
        // ════════════════════════════════════════════════════════════════
        private static (string Title, string Body) GetTraitReferenceNote(Character character)
        {
            if (character.Day < 5)
            {
                return ("TRAIT REFERENCE",
                    "🤖 ROBOT:\n" +
                    "  • Pauses > 0.3s between words\n" +
                    "  • Flat body temperature\n" +
                    "  • No pulse detected\n" +
                    "  • Says 'parameters' 'optimal'\n\n" +
                    "👽 ALIEN:\n" +
                    "  • Uses 'we/us' instead of 'I'\n" +
                    "  • High body temperature\n" +
                    "  • Multiple heartbeats\n" +
                    "  • Harmonic voice drift\n\n" +
                    "👤 HUMAN:\n" +
                    "  • Natural emotional response\n" +
                    "  • Temperature ~36.7°C\n" +
                    "  • 70–90 BPM pulse");
            }

            return ("TRAIT REFERENCE // UPDATED",
                    "⚠ Day 5+: Advanced disguise protocols.\n\n" +
                    "🤖 ROBOT (masked):\n" +
                    "  • May simulate normal pulse\n" +
                    "  • Slight latency on open questions\n" +
                    "  • Avoids metaphors / humour\n" +
                    "  • Precise numbers: '48 hours' not 'two days'\n\n" +
                    "👽 ALIEN (masked):\n" +
                    "  • Now uses human names\n" +
                    "  • Slight pause on 'family' questions\n" +
                    "  • May slip into plural 'we'\n" +
                    "  • Origin story slightly vague\n\n" +
                    "👤 HUMAN:\n" +
                    "  • Inconsistent, emotional, messy\n" +
                    "  • Complains, jokes, forgets things\n" +
                    "  • Does NOT say 'parameters'");
        }

        // ════════════════════════════════════════════════════════════════
        //  COMMUNICATIONS
        // ════════════════════════════════════════════════════════════════
        private static (string Title, string Body) GetCommunicationsNote(Character character)
        {
            string title = "📻 GALAXY RADIO // FREQ 156.8";

            string[] broadcasts = {
        // --- Gameplay Tips (Educational) ---
        "ADVISORY: Remember, citizens! Synthetics are programmed for precision. A human says 'in an hour', a robot says 'in 3600 seconds'. Stay sharp!",
        "SECURITY TIP: Access codes expire at the stroke of midnight. If a subject provides yesterday's code, they have a memory leak... or a forged ID.",
        "HEALTH NOTICE: Species from the Xylos Sector have vertical pupils. If someone looks human but has cat-like eyes, they aren't from Earth.",
        "PSA: Nervousness is human. Inconsistency is human. If a subject is too perfect under pressure, they might have been manufactured.",
        "GUIDE: Biological hearts have a rhythmic 'thump-thump'. Alien hearts often have a harmonic hum. Listen to the pulse monitor!",
        
        // --- Humor & Lore (Atmospheric) ---
        "NEWS: The 'Great Synthetic Bake-off' was cancelled today after the winner was caught using industrial lubricant instead of butter.",
        "AD: Tired of being scanned? Buy 'Bio-Mask'! Look 100% organic to any Tier-1 scanner. (Side effects may include permanent skin greening).",
        "LOCAL: A pet Slime-Rat was found in Airlock 4. Will the owner please collect it before it achieves sentience? Thank you.",
        "WEATHER: Meteor shower expected in Sector 7. Please stay indoors unless you enjoy being hit by high-velocity space rocks.",
        "COMMERCIAL: Visit 'The Rusty Bolt' for the best oil-margaritas in the quadrant. Now serving biologicals (mostly) non-toxic snacks!",
        
        // --- Story-Linked Hints ---
        $"REPORT: High activity near the perimeter. Sector Curator {character.Day}-Alpha reminds all inspectors: Quotas are not suggestions!",
        "WANTED: A technician named Nina is sought for questioning regarding missing engine parts. If seen, do not approach—she is armed with a heavy wrench."
    };

            // Picks a random broadcast each time the radio is accessed
            string randomNews = broadcasts[rnd.Next(broadcasts.Length)];

            string body =
                "STATUS: SIGNAL STRENGTH 85%\n" +
                "──────────────────────────────\n" +
                "CURRENT BROADCAST:\n\n" +
                $"\"{randomNews}\"\n\n" +
                "──────────────────────────────\n" +
                "UPCOMING: Sector 7 Weather & Radiation Report";

            return (title, body);
        }

        private static string GetRadioHint(Character character)
        {
            if (character is Robot r && character.Day < 5)
                return "Biometric: anomaly detected";
            if (character is Alien a && character.Day < 5)
                return "Non-standard vitals logged";
            if (character.Day >= 5)
                return "Advanced units active — biometrics unreliable";
            return "All systems nominal";
        }

        private static string GenerateDaySpecificCode(int day)
        {
            string[] zonePrefixes = { "7741", "3392", "5521", "8834", "2219", "6657" };
            int prefixIndex = (day - 1) % zonePrefixes.Length;
            return $"{zonePrefixes[prefixIndex]}-X";
        }
    }
}
using System;
using System.Collections.Generic;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public static class NoteManager
    {
        public static (string Title, string Body) GetDynamicNote(int zoneIndex, Character character)
        {
            // Safety check
            if (character == null)
            {
                return ("NO DATA", "Subject not detected. System standby.");
            }

            switch (zoneIndex)
            {
                // ═══ LEFT SCREENS ═══
                case 0: // zoneLeftTop - Biometric Scanner
                    return GetBiometricScannerNote(character);

                case 1: // zoneLeftMiddle - Vital Signs Monitor
                    return GetVitalSignsNote(character);

                case 2: // zoneLeftBottom - System Data Terminal
                    return GetSystemDataNote(character);

                // ═══ RIGHT STICKERS ═══
                case 3: // zoneSticker1 - Checklist (Special)
                    return ("FIELD INSPECTION LOG",
                            "Use the checkboxes to verify the subject's biological and behavioral markers.\n\n" +
                            "Quick Reference:\n" +
                            "✓ Thermal signature\n" +
                            "✓ Speech patterns\n" +
                            "✓ Response timing\n" +
                            "✓ Identity verification");

                case 4: // zoneSticker2 - Access Codes
                    return GetAccessCodesNote(character);

                case 5: // zoneSticker3 - Trait Reference
                    return ("TRAIT REFERENCE",
                            "🤖 ROBOT:\n" +
                            "  • Pauses > 0.3s\n" +
                            "  • Flat intonation\n" +
                            "  • Low body temp\n\n" +
                            "👽 ALIEN:\n" +
                            "  • Uses 'we/us'\n" +
                            "  • High body temp\n" +
                            "  • Multiple organs\n\n" +
                            "👤 HUMAN:\n" +
                            "  • Natural variance\n" +
                            "  • Emotional cues");

                case 6: // zoneSticker4 - Inspector Notes
                    return ("INSPECTOR PROTOCOL",
                            $"CHECK EVERY SUBJECT:\n" +
                            $"  ✓ Access code\n" +
                            $"  ✓ Place of arrival\n" +
                            $"  ✓ Purpose of visit\n\n" +
                            $"CURRENT QUOTA: Check all subjects\n" +
                            $"ERROR LIMIT: 3 mistakes\n\n" +
                            $"⚠ Day {character.Day}: Threats increasing!");

                // ═══ RIGHT SCREEN ═══
                case 7: // zoneRightScreen - Character Data (shown on monitor)
                    return ("SUBJECT PROFILE",
                            $"NAME: {character.Name}\n" +
                            $"ORIGIN: {character.ReasonToEnter}\n" +
                            $"DAY: {character.Day}\n\n" +
                            "Data displayed on main monitor.");

                // ═══ RADIOS ═══
                case 8: // zoneBigRadio - Communications
                    return GetCommunicationsNote(character);

                case 9: // zoneSmallRadio - Field Radio (Interactive)
                    return ("FIELD RADIO",
                            "INTERROGATION DEVICE\n\n" +
                            "Click to ask questions.\n" +
                            "Listen carefully to responses.\n\n" +
                            "⚠ Analyze:\n" +
                            "  • Speech patterns\n" +
                            "  • Response timing\n" +
                            "  • Pronoun usage");

                // ═══ DIALOGUE SCREEN ═══
                case 10: // zoneDialogueScreen
                    return ("DIALOGUE INTERFACE",
                            "Subject communication log.\n\n" +
                            "Click to initiate dialogue.\n" +
                            "Monitor for:\n" +
                            "  • Inconsistencies\n" +
                            "  • Unusual phrasing\n" +
                            "  • Deception markers");

                default:
                    return ("SYSTEM INFO", "Standard protocol active. Vigilance is advised.");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS - Generate dynamic content based on character
        // ═══════════════════════════════════════════════════════════

        private static (string Title, string Body) GetBiometricScannerNote(Character character)
        {
            string title = "BIOMETRIC SCANNER // UNIT-7";
            string body;

            if (character is Robot)
            {
                body = "STATUS: SCANNING...\n\n" +
                       "ANALYSIS:\n" +
                       "• Core Temp:        32.2°C  ⚠ ABNORMAL\n" +
                       "• Pulse rate:       NOT DETECTED\n" +
                       "• Skin conductance: UNIFORM\n" +
                       "• Retinal scan:     SYNTHETIC MARKERS\n" +
                       "• Voice pattern:    MECHANICAL CADENCE\n\n" +
                       "⚠ ANOMALY: Zero thermal variance\n" +
                       "  Signal interference: HIGH\n\n" +
                       "Note: Thermal regulation seems... synthetic.\n" +
                       "Metal traces detected in bone density scan.";
            }
            else if (character is Alien)
            {
                body = "STATUS: SCANNING...\n\n" +
                       "ANALYSIS:\n" +
                       "• Core Temp:        39.1°C  ⚠ FEVER?\n" +
                       "• Pulse rate:       140 BPM (ARRHYTHMIC)\n" +
                       "• Skin conductance: IRREGULAR\n" +
                       "• Retinal scan:     NON-STANDARD STRUCTURE\n" +
                       "• Voice pattern:    HARMONIC DRIFT 0.3Hz\n\n" +
                       "⚠ ANOMALY: DNA structure NON-CATALOGUED\n" +
                       "  Multiple cardiac nodes detected\n\n" +
                       "Note: Internal structure shows dual-organ systems.\n" +
                       "Blood chemistry: Unknown protein markers.";
            }
            else // Human
            {
                body = "STATUS: SCANNING...\n\n" +
                       "ANALYSIS:\n" +
                       "• Core Temp:        36.7°C  (NOMINAL)\n" +
                       "• Pulse rate:       78 BPM\n" +
                       "• Skin conductance: NORMAL VARIANCE\n" +
                       "• Retinal scan:     HUMAN BASELINE\n" +
                       "• Voice pattern:    NATURAL VARIANCE\n\n" +
                       "✓ All vitals within human baseline.\n" +
                       "  Species: Homo Sapiens\n\n" +
                       "Match confidence: 94% — HUMAN\n" +
                       "Recommendation: Standard interrogation.";
            }

            return (title, body);
        }

        private static (string Title, string Body) GetVitalSignsNote(Character character)
        {
            string title = "VITAL SIGNS MONITOR";
            string body;

            if (character is Robot)
            {
                body = "CARDIAC:      FLAT LINE ⚠\n" +
                       "RESPIRATORY:  NOT DETECTED\n" +
                       "O₂ SAT:       ERROR\n\n" +
                       "EEG PATTERN:\n" +
                       "  FLAT — No brain activity\n" +
                       "  Electrical interference detected\n\n" +
                       "⚡ ALERT: Subject may be non-biological\n\n" +
                       "Neural activity:       NONE\n" +
                       "Deception probability: N/A\n\n" +
                       "Note: Robots show flat EEG and no pulse.";
            }
            else if (character is Alien)
            {
                body = "CARDIAC:      140 BPM — IRREGULAR ⚠\n" +
                       "RESPIRATORY:  22 / min (ELEVATED)\n" +
                       "O₂ SAT:       103% ⚠ IMPOSSIBLE\n\n" +
                       "EEG PATTERN:\n" +
                       "  Harmonic oscillation at 0.3 Hz offset\n" +
                       "  Non-human wave pattern\n\n" +
                       "⚡ ALERT: Biological markers NON-STANDARD\n\n" +
                       "Neural activity:       UNUSUAL FREQUENCY\n" +
                       "Deception probability: UNKNOWN\n\n" +
                       "Note: Aliens show harmonic EEG patterns.";
            }
            else // Human
            {
                body = "CARDIAC:      78 BPM — normal range\n" +
                       "RESPIRATORY:  16 / min\n" +
                       "O₂ SAT:       98%\n\n" +
                       "EEG PATTERN:\n" +
                       "  Alpha waves:  9–13 Hz\n" +
                       "  Beta surge:   stress marker detected\n\n" +
                       "⚡ STRESS INDICATOR: Elevated cortisol pattern\n\n" +
                       "Neural activity:       ABOVE BASELINE\n" +
                       "Deception probability: 34%\n\n" +
                       "All readings within human parameters.";
            }

            return (title, body);
        }

        private static (string Title, string Body) GetSystemDataNote(Character character)
        {
            string title = "SYSTEM DATA // TERMINAL 7741";

            string body = $"SECURITY LEVEL: 3  (CLASSIFIED)\n" +
                         $"ZONE:           VOID PERIMETER\n" +
                         $"SHIFT:          DAY {character.Day} — INSPECTOR ACTIVE\n\n" +
                         "THREAT MATRIX:\n" +
                         "  Human infiltrators:  LOW\n" +
                         $"  Synthetic entities:  {(character.Day >= 2 ? "HIGH ⚠⚠" : "MEDIUM ⚠")}\n" +
                         $"  Alien contacts:      {(character.Day >= 3 ? "HIGH ⚠⚠" : "MEDIUM ⚠")}\n" +
                         "  Unknown:             MONITORING\n\n" +
                         "DAILY QUOTA:      Variable\n" +
                         "ERROR TOLERANCE:  3 mistakes\n\n" +
                         $"{(character.Day >= 3 ? "⚠⚠ CRITICAL: Advanced mimicry detected!" : "ALERT: Heightened vigilance required.")}";

            return (title, body);
        }

        private static (string Title, string Body) GetAccessCodesNote(Character character)
        {
            string title = "ACCESS CODES // CURRENT";

            // Генерируем актуальные коды на текущий день
            string currentDayCode = GenerateDaySpecificCode(character.Day);
            string previousDayCode = character.Day > 1
                ? GenerateDaySpecificCode(character.Day - 1)
                : "EXPIRED";

            string body = $"ACTIVE CODES (Day {character.Day}):\n\n" +
                         $"  Current Access:  {currentDayCode}\n" +
                         $"  Previous Day:    {previousDayCode} ⚠ EXPIRED\n" +
                         "  VOID Perimeter:  ???? (CLASSIFIED)\n" +
                         "  Medical:         MED-009\n\n" +
                         "VERIFICATION:\n" +
                         $"  Subject's code:  {character.AccessCode ?? "ERROR"}\n" +
                         $"  Status:          {(character.AccessCode == currentDayCode ? "✓ VALID" : "⚠ CHECK REQUIRED")}\n\n" +
                         "⚠ Codes change daily!\n" +
                         "  Outdated codes indicate:\n" +
                         "  • Memory malfunction (Robot)\n" +
                         "  • Forged credentials\n" +
                         "  • Database desync\n\n" +
                         "Note: Synthetics often use expired codes.";

            return (title, body);
        }

        private static string GenerateDaySpecificCode(int day)
        {
            string[] zonePrefixes = { "7741", "3392", "5521", "8834", "2219", "6657" };
            int prefixIndex = (day - 1) % zonePrefixes.Length;
            return $"{zonePrefixes[prefixIndex]}-X";
        }

        private static (string Title, string Body) GetCommunicationsNote(Character character)
        {
            string title = "COMMUNICATION BLOCK";

            string body = "FREQUENCIES:\n" +
                         "  CH-A (Command):  156.8 MHz  ACTIVE ✓\n" +
                         "  CH-B (Medical):  162.4 MHz  ACTIVE ✓\n" +
                         $"  CH-C (Perimeter): 171.0 MHz  {(character.Day >= 2 ? "INTERFERENCE ⚠⚠" : "SIGNAL OK ✓")}\n" +
                         "  Emergency:       121.5 MHz  STANDBY\n\n" +
                         "INCOMING MESSAGES:\n" +
                         $"  > \"Perimeter-2: Subject {character.Name} at gate\"\n" +
                         $"  > \"Command: Day {character.Day} quota in progress\"\n" +
                         $"  > \"Medical: {(character is Robot ? "Biometric anomaly detected" : character is Alien ? "Non-standard vitals logged" : "All systems nominal")}\"\n\n" +
                         $"{(character.Day >= 3 ? "⚠⚠ Channel C compromised. AI interference." : "All channels operational.")}";

            return (title, body);
        }
    }
}
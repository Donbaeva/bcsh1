using System.Collections.Generic;
using TheGatekeeper.Models;

namespace TheGatekeeper.Utils
{
    public static class ToolSystem
    {
        public static List<string> GetAvailableTools(int day)
        {
            var tools = new List<string> { "🎤 Voice Analyzer" };

            if (day >= 2) tools.Add("💓 Pulse Meter");
            if (day >= 3) tools.Add("📡 Radiation Detector");
            if (day >= 4) tools.Add("🖐️ Fingerprint Scan");

            return tools;
        }

        public static string UseToolOn(string tool, Character character)
        {
            if (character == null) return "No subject to inspect.";

            switch (tool)
            {
                case "🎤 Voice Analyzer":
                    if (character.Type == "Robot")
                        return "⚠️ VOICE ANALYSIS: Synthetic harmonics detected. Classification: ROBOT. Action recommended.";
                    if (character.Type == "Alien")
                        return "⚠️ VOICE ANALYSIS: Unknown modulation pattern. Classification: ALIEN. Action recommended.";
                    return "✅ VOICE ANALYSIS: Natural vocal modulation. Likely HUMAN.";

                case "💓 Pulse Meter":
                    if (character.Pulse == 0)
                        return "⚠️ PULSE: NO HEARTBEAT. Not human physiology.";
                    if (character.Pulse < 60)
                        return $"⚠️ PULSE: LOW ({character.Pulse} BPM). Suspicious.";
                    if (character.Pulse > 100)
                        return $"⚠️ PULSE: HIGH ({character.Pulse} BPM). Stress or non-human physiology?";
                    return $"✅ PULSE: NORMAL ({character.Pulse} BPM).";

                case "📡 Radiation Detector":
                    if (character.Radiation > 30)
                        return $"☢️ RADIATION: CRITICAL ({character.Radiation} mSv). Not standard human profile.";
                    if (character.Radiation > 15)
                        return $"⚠️ RADIATION: ELEVATED ({character.Radiation} mSv). Suspicious.";
                    return $"✅ RADIATION: NORMAL ({character.Radiation} mSv).";

                case "🖐️ Fingerprint Scan":
                    if (character.Type == "Robot")
                        return "⚠️ FINGERPRINTS: Artificial ridge patterns detected. Classification: ROBOT.";
                    if (character.Type == "Alien")
                        return "⚠️ FINGERPRINTS: No match in database. Classification: ALIEN.";
                    return "✅ FINGERPRINTS: Match found in database. Likely HUMAN.";

                default:
                    return "Tool not recognized.";
            }
        }
    }
}
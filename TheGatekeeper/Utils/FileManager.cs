using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper.Utils
{
    public static class FileManager
    {
        private static string savePath = Application.StartupPath + @"\Saves\";
        private static string resultsPath = Application.StartupPath + @"\Results\";

        private static string ToJson<T>(T obj)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static T FromJson<T>(string json)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)ser.ReadObject(ms);
            }
        }

        [System.Runtime.Serialization.DataContract]
        private class CharacterExport
        {
            [System.Runtime.Serialization.DataMember] public string Name { get; set; }
            [System.Runtime.Serialization.DataMember] public string Dialogue { get; set; }
            [System.Runtime.Serialization.DataMember] public string Type { get; set; }
            [System.Runtime.Serialization.DataMember] public bool IsObvious { get; set; }
            [System.Runtime.Serialization.DataMember] public string Occupation { get; set; }
            [System.Runtime.Serialization.DataMember] public string ReasonToEnter { get; set; }
        }

        /// <summary>
        /// Save progress to JSON
        /// </summary>
        public static void SaveGame(SaveData data, string fileName = "save.json")
        {
            try
            {
                // Create directory if missing
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                // Serialize as JSON
                string json = ToJson(data);

                // Write file
                File.WriteAllText(savePath + fileName, json, Encoding.UTF8);

                MessageBox.Show("✅ Game saved!", "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load progress from JSON
        /// </summary>
        public static SaveData LoadGame(string fileName = "save.json")
        {
            try
            {
                string fullPath = savePath + fileName;
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath, Encoding.UTF8);
                    return FromJson<SaveData>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        /// <summary>
        /// Save end-game result to TXT
        /// </summary>
        public static void SaveResult(GameResult result)
        {
            try
            {
                if (!Directory.Exists(resultsPath))
                    Directory.CreateDirectory(resultsPath);

                // TXT format (plain text)
                string txtLine = $"{result.Date:yyyy-MM-dd HH:mm:ss} | " +
                                $"Score: {result.FinalScore} | " +
                                $"Days: {result.DaysSurvived} | " +
                                $"Level: {result.MaxLevel} | " +
                                $"{(result.IsVictory ? "VICTORY" : "DEFEAT")}";

                File.AppendAllText(resultsPath + "game_results.txt", txtLine + Environment.NewLine, Encoding.UTF8);

                // Also store JSON
                string json = ToJson(result);
                string jsonFileName = $"result_{result.Date:yyyy-MM-dd_HH-mm-ss}.json";
                File.WriteAllText(resultsPath + jsonFileName, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Result save error: " + ex.Message);
            }
        }

        /// <summary>
        /// Load all results from TXT
        /// </summary>
        public static List<string> LoadAllResults()
        {
            List<string> results = new List<string>();
            string fullPath = resultsPath + "game_results.txt";

            if (File.Exists(fullPath))
            {
                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                results.AddRange(lines);
            }

            return results;
        }

        /// <summary>
        /// Save characters to JSON (for editing)
        /// </summary>
        public static void SaveCharacters(List<Character> characters, string fileName = "characters.json")
        {
            try
            {
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                var characterData = new List<CharacterExport>();
                foreach (var c in characters)
                {
                    characterData.Add(new CharacterExport
                    {
                        Name = c.Name,
                        Dialogue = c.Dialogue,
                        Type = c.Species, // Исправлено на Species
                        IsObvious = c.IsObvious,
                        Occupation = c.Occupation,
                        ReasonToEnter = c.ReasonToEnter
                    });
                }

                string json = ToJson(characterData);
                File.WriteAllText(savePath + fileName, json, Encoding.UTF8);
            }
            catch { }
        }

        /// <summary>
        /// Check whether a save exists
        /// </summary>
        public static bool HasSaveGame()
        {
            return File.Exists(savePath + "save.json");
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(savePath + "save.json"))
                    File.Delete(savePath + "save.json");
            }
            catch { }
        }
    }
}
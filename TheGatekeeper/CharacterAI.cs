using System;
using System.Collections.Generic;

namespace TheGatekeeper.Models
{
    public static class CharacterAI
    {
        private static Random rnd = new Random();

        public static string GenerateAnswer(Character character, string question)
        {
            string q = question.ToLower();

            if (ContainsAny(q, "code", "access", "permit", "id"))
                return GetAccessCodeAnswer(character);

            if (ContainsAny(q, "why", "purpose", "reason", "business", "goal"))
                return character.ReasonToEnter;

            if (ContainsAny(q, "feel", "emotion", "nervous", "scared", "how are you"))
                return GetEmotionalAnswer(character);

            if (ContainsAny(q, "from", "where", "origin", "home", "live"))
                return GetOriginAnswer(character);

            if (ContainsAny(q, "family", "parents", "wife", "husband", "kids", "relatives"))
                return GetFamilyAnswer(character);

            return GetDefaultAnswer(character);
        }

        private static string GetAccessCodeAnswer(Character character)
        {
            var options = new List<string>();

            if (character is Robot && character.Day <= 2)
            {
                options.Add($"Access code is {character.AccessCode}. It is... fully functional.");
                options.Add($"{character.AccessCode}. Everything is within parameters, officer.");
            }
            else if (character is Alien && character.Day <= 2)
            {
                options.Add($"Our... my code is {character.AccessCode}. Please, check it.");
                options.Add($"The sequence is {character.AccessCode}. It should be correct.");
            }
            else // Humans and High-level Mimics
            {
                options.Add($"It's {character.AccessCode}. Is there a problem?");
                options.Add($"My code? Sure, it's {character.AccessCode}.");
                options.Add($"Here it is: {character.AccessCode}.");
            }

            return options[rnd.Next(options.Count)];
        }

        private static string GetEmotionalAnswer(Character character)
        {
            if (character is Robot)
            {
                if (character.Day <= 2) return "I am... operating normally. Just a bit tired from the commute.";
                if (character.Day <= 4) return "To be honest, these gates make me feel slightly tense. It's the lights, I think.";
                return "I'm fine, officer. Just had a long morning and a cold coffee.";
            }

            if (character is Alien)
            {
                if (character.Day <= 2) return "I feel... heavy today. It is just the atmosphere, I suppose.";
                return "A bit anxious. My family is waiting for me, so I'm in a bit of a hurry.";
            }

            // Humans
            string[] humanFeels = {
                "Honestly? I'm exhausted. This line was huge.",
                "A little stressed. I hate being late for work.",
                "I'm okay, just trying to stay patient.",
                "A bit uneasy. These checkpoints are never pleasant, are they?"
            };
            return humanFeels[rnd.Next(humanFeels.Length)];
        }

        private static string GetOriginAnswer(Character character)
        {
            if (character is Alien alien && character.Day <= 3)
            {
                // Очень тонкий намек: упоминание "sector" вместо "district" или "street"
                if (rnd.Next(10) < 2) return $"I'm from... Sector 7. I mean, the Seventh District.";
            }

            string[] places = { "The Residential Zone.", "Just outside the main city.", "The Northern suburbs.", "Old Town." };
            return places[rnd.Next(places.Length)];
        }

        private static string GetFamilyAnswer(Character character)
        {
            if (character is Robot && character.Day <= 3)
            {
                // Роботы часто слишком конкретны или используют странные термины
                return "My family unit is stable. My parents live in the suburbs.";
            }

            string[] familyAnswers = {
                "Yes, my wife is waiting for me at home.",
                "I live with my parents. They're getting old, so I help them out.",
                "No, I'm single. Just me and my cat.",
                "Yeah, I have two kids. They're a handful, believe me."
            };
            return familyAnswers[rnd.Next(familyAnswers.Length)];
        }

        private static string GetDefaultAnswer(Character character)
        {
            string[] defaults = {
        "Could you repeat that, officer?",
        "I'm not sure I understand the question.",
        "Is that relevant to my entry permit?",
        "Sorry, my mind was elsewhere. What did you say?"
    };
            return defaults[rnd.Next(defaults.Length)];
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            foreach (var key in keywords)
                if (text.Contains(key)) return true;
            return false;
        }

        public static string GenerateGreeting(Character character)
        {
            string[] greetings = {
                "Morning, officer. Here are my papers.",
                "Hello. I'm hoping to get through quickly today.",
                "Good day. Everything should be in order.",
                "Hi. Ready for the inspection."
            };

            // Роботы на ранних этапах чуть более формальны
            if (character is Robot && character.Day <= 2)
                return "Greetings. I am seeking entry for work purposes.";

            return greetings[rnd.Next(greetings.Length)];
        }
    }
}
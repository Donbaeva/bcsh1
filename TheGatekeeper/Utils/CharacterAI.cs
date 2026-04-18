using System;
using System.Collections.Generic;
using static TheGatekeeper.Models.Character;

namespace TheGatekeeper.Models
{
    public static class CharacterAI
    {
        private static Random rnd = new Random();

        public static string GenerateAnswer(Character character, string question)
        {
            string storyAnswer = StoryDialogueAI.TryGetAnswer(character, question);
            if (storyAnswer != null) return storyAnswer;

            string q = question.ToLower();

            if (ContainsAny(q, "code", "access", "permit", "id"))
                return GetAccessCodeAnswer(character);

            if (ContainsAny(q, "why", "purpose", "reason", "business", "goal"))
                return GetPurposeAnswer(character);

            if (ContainsAny(q, "feel", "emotion", "nervous", "scared", "how are you"))
                return GetEmotionalAnswer(character);

            if (ContainsAny(q, "from", "where", "origin", "home", "live"))
                return GetOriginAnswer(character);

            if (ContainsAny(q, "family", "parents", "wife", "husband", "kids", "relatives"))
                return GetFamilyAnswer(character);

            // Специальный вопрос режима ОХОТЫ
            if (ContainsAny(q, "biological", "composition", "organic", "dna"))
                return GetBiologicalAnswer(character);

            // Новые вопросы от InterrogationSystem
            if (ContainsAny(q, "name", "who are you", "introduce yourself"))
                return GetNameAnswer(character);

            if (ContainsAny(q, "occupation", "job", "work", "profession"))
                return GetOccupationAnswer(character);

            if (ContainsAny(q, "heading", "destination", "going", "where to"))
                return GetDestinationAnswer(character);

            if (ContainsAny(q, "citizen", "nationality", "citizenship", "registered"))
                return GetCitizenshipAnswer(character);

            return GetDefaultAnswer(character);
        }

        // ════════════════════════════════════════════════════════════════
        //  ЦЕЛЬ ВИЗИТА
        // ════════════════════════════════════════════════════════════════
        private static string GetPurposeAnswer(Character character)
        {
            // С Дня 5 роботы и пришельцы дают такие же ответы как люди
            if (character.Day >= 5 && !(character is Human))
            {
                var humanLike = new[]
                {
                    $"Work, same as every day. Nothing special.",
                    $"I've got a shift at {character.Occupation}. Running late, actually.",
                    $"Personal business. Meeting a colleague.",
                    $"I'm here for {character.ReasonToEnter}. It's on the form.",
                    $"Just passing through on my way to {character.Occupation}.",
                    $"I'd rather not explain every detail of my day, officer.",
                };
                return humanLike[rnd.Next(humanLike.Length)];
            }

            return character.ReasonToEnter;
        }

        // ════════════════════════════════════════════════════════════════
        //  КОД ДОСТУПА
        // ════════════════════════════════════════════════════════════════
        private static string GetAccessCodeAnswer(Character character)
        {
            var options = new List<string>();
            string code = character.AccessCode;

            if (character is Robot)
            {
                if (character.Day >= 5)
                {
                    // День 5+: роботы отвечают как люди, никаких технических фраз
                    options.Add($"Oh, right. It's {code}. Sorry, long morning.");
                    options.Add($"Here — {code}. I keep forgetting to write it down somewhere.");
                    options.Add($"Uh, let me think... {code}. Yeah, that's it.");
                    options.Add($"It's {code}. Same as it was yesterday, why?");
                    options.Add($"Give me a sec — {code}. Got it.");
                    options.Add($"My boss sends me an alert. Today it's {code}.");
                    string sc = ScrambleCode(code);
                    options.Add($"Wait — {sc}? No, no, sorry. {code}. Eyes haven't adjusted.");
                }
                else if (character.Day > 3)
                {
                    options.Add($"My memory... I mean, my head is foggy. Is it {code}?");
                    options.Add($"Here it is: {code}. Sorry, I'm a bit shaky.");
                    options.Add($"As per protocol: {code}. Can we skip the pleasantries?");
                    options.Add($"{code}. Everything is within parameters, officer.");
                }
                else
                {
                    options.Add($"{code}. Data integrity verified. Proceed?");
                    options.Add($"Access sequence {code} is active and valid.");
                    options.Add($"Access code is {code}. It is... fully functional.");
                }
            }
            else if (character is Alien)
            {
                if (character.Day >= 5)
                {
                    // День 5+: пришельцы идеально имитируют людей
                    options.Add($"It's {code}. Is something wrong with it?");
                    options.Add($"Here, {code}. My colleague gave it to me this morning.");
                    options.Add($"Oh, the code. {code}. I always get nervous when asked.");
                    options.Add($"{code}. I had to write it on my hand, embarrassing.");
                    options.Add($"One moment... it's {code}. Got it saved in my phone.");
                    string sc = ScrambleCode(code);
                    options.Add($"Hmm, is it {sc}? No — {code}. Sorry, bad memory today.");
                }
                else if (character.Day > 3)
                {
                    options.Add($"Yeah, the digits are {code}. Long day at the docks.");
                    options.Add($"Oh god, I forgot. Wait! No! Here: {code}.");
                    options.Add($"Here it is: {code}.");
                }
                else
                {
                    options.Add($"The code is {code}. We... I mean, I have nothing to hide.");
                    options.Add($"Code {code}. Is the atmosphere always this... thin here?");
                    options.Add($"Our... my code is {code}. Please, check it.");
                }
            }
            else // Human
            {
                options.Add($"Oh god, I forgot it. Wait! No! Here: {code}. Please, I really need to get home.");
                options.Add($"Wait, let me find it... {code}. My kid was playing with my badge.");
                string scrambled = ScrambleCode(code);
                options.Add($"It's {scrambled}... oh, wait, my eyes are blurry. It's {code}.");
                options.Add($"You tell me. You're the one with the monitor. ...Fine, it's {code}.");
                options.Add($"{code}. Is there a problem?");
                options.Add($"It's {code}. My boss said if I'm late again, I'm fired.");
                options.Add($"I've got it memorized: {code}. I use it for my locker too.");
                options.Add($"Here it is: {code}.");
                options.Add($"My code? Sure, it's {code}.");
            }

            return options[rnd.Next(options.Count)];
        }

        private static string ScrambleCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < 2) return code ?? "";
            char[] chars = code.ToCharArray();
            char temp = chars[0]; chars[0] = chars[1]; chars[1] = temp;
            return new string(chars);
        }

        // ════════════════════════════════════════════════════════════════
        //  ЭМОЦИИ — с Дня 5 роботы и пришельцы звучат как люди
        // ════════════════════════════════════════════════════════════════
        private static string GetEmotionalAnswer(Character character)
        {
            var options = new List<string>();

            if (character is Robot)
            {
                if (character.Day >= 5)
                {
                    // Неотличимо от человека — никаких механических оговорок
                    options.Add("Honestly? Just tired. Didn't sleep well.");
                    options.Add("A bit anxious, I guess. These checkpoints make me nervous. They always do.");
                    options.Add("I'm okay. Had a rough commute, that's all.");
                    options.Add("Fine. Just ready to get to work and get the day started.");
                    options.Add("I'm actually pretty good. Coffee helped. You know how it is.");
                    options.Add("Stressed. Deadlines at work. Nothing major.");
                    options.Add("Feeling alright. A little cold in here, maybe.");
                    options.Add("A bit overwhelmed, to be honest. Too much going on.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("I am... operating normally. Just a bit tired from the commute.");
                    options.Add("Status is nominal. Although, this humidity is doing wonders for my... joints.");
                    options.Add("I feel... adequate. Processing... I mean, everything is fine.");
                    options.Add("My emotional state is: functional. No errors detected.");
                    options.Add("Everything is within parameters. Why do you ask?");
                }
                else
                {
                    options.Add("Honestly? I've got this pounding headache. Too much screen time, I guess.");
                    options.Add("I'm feeling a bit glitchy—ha, just a joke! I meant twitchy.");
                    options.Add("A bit drained. My battery... uh, my energy is just low today.");
                    options.Add("Could be better. My internal chronometer—er, my body clock—is off.");
                }
            }
            else if (character is Alien)
            {
                if (character.Day >= 5)
                {
                    // Идеальная маскировка — человеческие эмоции
                    options.Add("I'm tired. My neighbor was loud all night. Do people here always party on weekdays?");
                    options.Add("A little stressed. I have a meeting later and I hate public speaking.");
                    options.Add("I'm okay, thanks. Just want to get through this and get to work.");
                    options.Add("Fine, I think. Not my best day. You ever just wake up wrong?");
                    options.Add("Nervous, honestly. These checkpoints always make me feel guilty about something.");
                    options.Add("I'm good. Had a big breakfast, feeling optimistic.");
                    options.Add("Overwhelmed. Too many things happening this week.");
                    options.Add("I'm alright. Just homesick. Miss my family.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("I feel... heavy today. It is just the gravity and atmosphere, I suppose.");
                    options.Add("We are... I am feeling a bit disoriented. The lights here are very... bright.");
                    options.Add("I sense a vibration. Is that the floor or my internal... nerves?");
                    options.Add("I am nervous. The collective — I mean, the crowd — is making me tense.");
                }
                else
                {
                    options.Add("A little overwhelmed. The city is so... loud. I miss the quiet.");
                    options.Add("Nervous? Who wouldn't be? Every time I stand in this line, I feel judged.");
                    options.Add("I'm okay. Just had a strange dream last night. You ever dream of floating?");
                }
            }
            else // Human
            {
                string[] humanOptions = {
                    "Honestly? I'm exhausted. This line was huge and the bus was broken.",
                    "A bit uneasy. These checkpoints always make me feel like a criminal.",
                    "I'm tired. Didn't get much sleep. Neighbors were partying until 3 AM.",
                    "I'm okay, just a bit grumpy. I haven't had my coffee yet.",
                    "I'm actually great. Just got a promotion, so nothing can bring me down today.",
                    "I'm weirdly calm. Maybe it's the meditation app. Or resignation. One of the two.",
                    "My back is killing me from standing here. How do you do this all day?",
                    "A little stressed. I think I left the stove on... or did I?",
                    "I'm annoyed. My boss made me stay late without overtime. This week is a wash.",
                    "I don't know. Some days everything feels... gray.",
                    "In a hurry, to be honest. Running late for a meeting.",
                    "I'm fine, just focused. Too many deadlines on my mind.",
                };
                options.AddRange(humanOptions);
            }

            return options[rnd.Next(options.Count)];
        }

        // ════════════════════════════════════════════════════════════════
        //  ПРОИСХОЖДЕНИЕ
        // ════════════════════════════════════════════════════════════════
        private static string GetOriginAnswer(Character character)
        {
            var options = new List<string>();

            if (character is Robot)
            {
                if (character.Day >= 5)
                {
                    options.Add("I'm from the North Side. Lived there my whole life.");
                    options.Add("Born in the Eastern District. Still live there, actually.");
                    options.Add("I moved from the suburbs a few years back. The city is better for work.");
                    options.Add("West End. It's noisy but I like it. Convenient.");
                    options.Add("I grew up in the Old Quarter. You probably know it.");
                    options.Add("From a small settlement outside the main zone. Moved here for the job.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("I reside in Sector 7, Block C. It is... a residential area.");
                    options.Add("My home is located in the Manufacturing District. Very quiet at night.");
                    options.Add("My designated living quarters are in the Outer Rim.");
                }
                else
                {
                    options.Add("I live in the Northern Suburbs. The commute is manageable.");
                    options.Add("I'm from the city center. It's noisy, but convenient.");
                }
            }
            else if (character is Alien alien)
            {
                if (character.Day >= 5)
                {
                    // Прикрытие убедительное, никаких оговорок про планету
                    options.Add("I'm local, actually. Born here. Rarely leave.");
                    options.Add("I came from the Southern Territories a few years ago. Work brought me here.");
                    options.Add("I'm from a small town up north. You've probably never heard of it.");
                    options.Add("Originally from the Coast. Moved here for my partner's job.");
                    options.Add("I've been here about six years. It feels like home now.");
                    options.Add("Far from here. I don't talk about it much. Bad memories.");
                }
                else if (character.Day <= 2)
                {
                    options.Add($"I come from... {alien.HomePlanet}—I mean, the Northern District! Yes.");
                    options.Add("We... I traveled from the Outer Colonies. Er, the Outer Suburbs.");
                    options.Add("My home is very far from here. I live in Sector 9 now.");
                }
                else
                {
                    options.Add("I'm from the Residential Zone. It's crowded, but people are friendly.");
                    options.Add("I come from a small town up north. You've probably never heard of it.");
                }
            }
            else // Human
            {
                options.Add("I'm from the Residential Zone. Pretty standard.");
                options.Add("I live just outside the main city. Commute is a killer.");
                options.Add("I'm from the Northern Suburbs. Quiet, safe, boring.");
                options.Add("Old Town. You know, where all the historic buildings are.");
                options.Add("I'm from the South Side. It's not as bad as people say.");
                options.Add("I live downtown. Loud, expensive, but I love it.");
                options.Add("I live in a small apartment near the industrial park.");
            }

            return options[rnd.Next(options.Count)];
        }

        // ════════════════════════════════════════════════════════════════
        //  СЕМЬЯ
        // ════════════════════════════════════════════════════════════════
        private static string GetFamilyAnswer(Character character)
        {
            var options = new List<string>();

            if (character is Robot)
            {
                if (character.Day >= 5)
                {
                    // Убедительные истории, никаких оговорок
                    options.Add("Yeah, I've got a wife and a little girl. She just started school. Time flies.");
                    options.Add("I live with my parents. They're getting older, so I help out around the house.");
                    options.Add("I'm single at the moment. Just me and my dog. He's a handful.");
                    options.Add("I have a big family. Three siblings, lots of cousins. Holidays are chaos.");
                    options.Add("My partner and I are expecting. Terrifying and exciting at the same time.");
                    options.Add("I live alone. Had a divorce last year. Starting over.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("My family unit is stable. My parents live in the suburbs.");
                    options.Add("I have a standard family: two parental units and one sibling.");
                    options.Add("My creators... I mean, my parents are both engineers.");
                    options.Add("Family? Yes, I possess one. All functional.");
                }
                else
                {
                    options.Add("Yes, I have family. My parents retired to the countryside.");
                    options.Add("I'm married. My wife is a teacher. We're thinking about kids.");
                    options.Add("My parents are still around. They worry too much, like all parents.");
                }
            }
            else if (character is Alien)
            {
                if (character.Day >= 5)
                {
                    // Правдоподобные истории без оговорок
                    options.Add("Yes, I have a wife and two kids. They're why I put up with this commute.");
                    options.Add("I live alone. My parents passed a few years ago.");
                    options.Add("I've got a brother. We don't talk much anymore. It's complicated.");
                    options.Add("My family is back in my hometown. I moved here for work. I miss them.");
                    options.Add("Just me and my cat. She doesn't care what I do, which is refreshing.");
                    // Редкая небольшая оговорка даже на Дне 5-7
                    if (character.Day <= 7)
                        options.Add("My... family is large. We are very close. We share everything. I mean — we talk a lot.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("We are all connected... I mean, I have a normal family.");
                    options.Add($"My family is vast. We span across... the country. Yes, the country.");
                    options.Add("I have a family unit. They are... waiting. Very patiently.");
                }
                else
                {
                    options.Add("I have a family. My parents live in the city. We're close.");
                    options.Add("I've got a brother. He's... different from me. We don't talk much.");
                }
            }
            else // Human
            {
                options.Add("Yes, my wife is waiting at home. Probably wondering where I am.");
                options.Add("I live with my parents. They're getting old, I help them out.");
                options.Add("No, I'm single. Just me and my cat.");
                options.Add("Yeah, I have two kids. They're a handful.");
                options.Add("I'm divorced. Just me and my son on weekends.");
                options.Add("I've got a big family. Four siblings, all in the same city.");
                options.Add("I'm engaged. Wedding's in the spring. Very excited, very stressed.");
                options.Add("I live with my grandmother. She raised me, so I'm returning the favour.");
            }

            return options[rnd.Next(options.Count)];
        }

        // ════════════════════════════════════════════════════════════════
        //  БИОЛОГИЧЕСКИЙ ВОПРОС (режим ОХОТЫ)
        // ════════════════════════════════════════════════════════════════
        private static string GetBiologicalAnswer(Character character)
        {
            if (character is Robot)
            {
                if (character.Day >= 5)
                    return "My... biological composition? What kind of question is that? I'm human. Same as you.";
                return "That is not a standard query. My internal structure is... not relevant to entry processing.";
            }
            if (character is Alien)
            {
                if (character.Day >= 5)
                    return "That's a strange question. Carbon-based, obviously. Same as everyone else here.";
                return "We... I am of standard biological configuration. There is nothing unusual about my physiology.";
            }
            // Human
            var humanReactions = new[]
            {
                "What?! What kind of question is that? I'm human, obviously! Carbon, oxygen, water. Same as you!",
                "I'm sorry, are you asking about my DNA? Is this a new protocol? That's... weird.",
                "I— what? Biological composition? Should I be worried about something?",
                "Ha! Is this a joke? I'm a person. Flesh and blood. You can see that.",
            };
            return humanReactions[rnd.Next(humanReactions.Length)];
        }

        // ════════════════════════════════════════════════════════════════
        //  ДЕФОЛТНЫЙ ОТВЕТ
        // ════════════════════════════════════════════════════════════════
        private static string GetDefaultAnswer(Character character)
        {
            var options = new List<string>();

            options.Add("Could you repeat that, officer?");
            options.Add("I'm not sure I understand the question.");
            options.Add("Is that relevant to my entry permit?");
            options.Add("Sorry, my mind was elsewhere. What did you say?");

            if (character is Robot)
            {
                if (character.Day >= 5)
                {
                    options.Add("Sorry, I zoned out. What was that?");
                    options.Add("I didn't catch that. Can you say it again?");
                    options.Add("I'm sorry, I'm a bit distracted today.");
                    options.Add("Hmm? One more time, please.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("Query unclear. Please rephrase.");
                    options.Add("That input is not recognized. Please try again.");
                }
                else
                {
                    options.Add("I'm not following. Could you be more specific?");
                    options.Add("I'm a bit distracted today. What was the question?");
                }
            }
            else if (character is Alien)
            {
                if (character.Day >= 5)
                {
                    options.Add("Hmm? Sorry, I was daydreaming. What was the question?");
                    options.Add("I didn't catch that. One more time?");
                    options.Add("My apologies, I missed that.");
                }
                else if (character.Day <= 2)
                {
                    options.Add("We... I do not comprehend. Please restate.");
                    options.Add("Your words are... confusing. What do you mean?");
                }
                else
                {
                    options.Add("I'm sorry, I didn't quite catch that.");
                    options.Add("Could you say that differently?");
                }
            }
            else // Human
            {
                options.Add("I'm sorry, what was that? I have a lot on my mind.");
                options.Add("Could you ask me something else?");
                options.Add("I'm not comfortable answering that.");
                options.Add("Why do you need to know that?");
                options.Add("Hmm? Sorry, I was daydreaming.");
                options.Add("I didn't get that. One more time?");
            }

            return options[rnd.Next(options.Count)];
        }

        // ════════════════════════════════════════════════════════════════
        //  ПРИВЕТСТВИЕ — с Дня 5 все три типа звучат одинаково
        // ════════════════════════════════════════════════════════════════
        public static string GenerateGreeting(Character character)
        {
            // После Дня 5 роботы и пришельцы звучат полностью как люди
            if (character.Day >= 5 && !(character is Human))
                return GenerateHumanGreeting();

            if (character is Robot)
                return GenerateRobotGreeting(character.Day);

            if (character is Alien)
                return GenerateAlienGreeting(character.Day);

            return GenerateHumanGreeting();
        }

        private static string GenerateRobotGreeting(int day)
        {
            if (day <= 2)
            {
                var robotEarly = new[]
                {
                    "Greetings. I am seeking entry for work purposes.",
                    "Hello. Here are my credentials.",
                    "Good day. I require passage to my designated workplace.",
                    "Salutations. My documents are prepared.",
                    "Hello, officer. I trust everything is in order.",
                    "Greetings. I am scheduled for entry at this time.",
                };
                return robotEarly[rnd.Next(robotEarly.Length)];
            }

            // День 3–4: начинают подражать, но с оговорками
            var robotMid = new[]
            {
                "Hi. Just... heading to work. Here are my papers.",
                "Good morning. I have all necessary paperwork.",
                "Hello. I am ready for inspection.",
                "Hello. Just heading to work.",
                "Morning. Is the line usually this long?",
                "Hello. Hope I didn't forget anything today.",
                "Good morning. I'm running a bit late, if that's okay.",
            };
            return robotMid[rnd.Next(robotMid.Length)];
        }

        private static string GenerateAlienGreeting(int day)
        {
            if (day <= 2)
            {
                var alienEarly = new[]
                {
                    "Hello, officer. I come in peace. Here are my papers.",
                    "Greetings. I seek entry for official business.",
                    "Good day. My documents should be correct.",
                    "Hello. I hope this won't take long.",
                    "Greetings, officer. We are... I am here for work.",
                };
                return alienEarly[rnd.Next(alienEarly.Length)];
            }

            // День 3–4: лучше, но иногда оговорки
            var alienMid = new[]
            {
                "Morning. Everything should be in order.",
                "Hello. I have my papers ready for you.",
                "Good day. May I enter?",
                "Hello. Just heading to work.",
                "Hi. I've got all my paperwork, officer.",
                "Morning. Is the line usually this long?",
            };
            return alienMid[rnd.Next(alienMid.Length)];
        }

        private static string GenerateHumanGreeting()
        {
            var human = new[]
            {
                "Morning, officer. Here are my papers.",
                "Hello. I'm hoping to get through quickly today.",
                "Good day. Everything should be in order.",
                "Hi. Ready for the inspection.",
                "Hey there. Let's get this over with.",
                "Morning. Got my documents right here.",
                "Hello. Just heading to work.",
                "Hi. I've got all my paperwork, officer.",
                "Good morning. I'm running a bit late, if that's okay.",
                "Hey. How's your shift going?",
                "Morning. Is the line usually this long?",
                "Hello. Hope I didn't forget anything today.",
                "Look, I'm in a bit of a hurry. Can we make this quick?",
                "Hi. Sorry, is there a faster queue?",
                "Morning. Coffee hasn't kicked in yet, bear with me.",
                "Hey. Same checkpoint as yesterday, right? I'm getting used to this.",
            };
            return human[rnd.Next(human.Length)];
        }

        // ════════════════════════════════════════════════════════════════
        //  ИМЯ
        // ════════════════════════════════════════════════════════════════
        private static string GetNameAnswer(Character character)
        {
            var rnd = new Random();
            if (character is Robot && character.Day <= 2)
            {
                return $"My designation is {character.Name}. It is on file.";
            }
            if (character is Alien && character.Day <= 3)
            {
                string[] a = {
                    $"I am called {character.Name}. The document confirms this.",
                    $"We — I am {character.Name}. It is registered.",
                };
                return a[rnd.Next(a.Length)];
            }
            string[] h = {
                $"{character.Name}. Same as on the badge.",
                $"It's {character.Name}. Is something wrong?",
                $"My name is {character.Name}. It's on the document.",
                $"{character.Name}. You can verify that."
            };
            return h[rnd.Next(h.Length)];
        }

        // ════════════════════════════════════════════════════════════════
        //  ПРОФЕССИЯ
        // ════════════════════════════════════════════════════════════════
        private static string GetOccupationAnswer(Character character)
        {
            var rnd = new Random();
            string occ = character.Occupation ?? "general work";
            if (character is Robot && character.Day <= 2)
                return $"I am assigned to {occ}. It is in my operational parameters.";
            string[] opts = {
                $"I work as {occ}. Have been for a while.",
                $"{occ}. It's all on the permit.",
                $"My job? {occ}. Nothing unusual.",
                $"I'm a {occ}. Why do you ask?"
            };
            return opts[rnd.Next(opts.Length)];
        }

        // ════════════════════════════════════════════════════════════════
        //  КУДА НАПРАВЛЯЕТСЯ
        // ════════════════════════════════════════════════════════════════
        private static string GetDestinationAnswer(Character character)
        {
            var rnd = new Random();
            string reason = character.ReasonToEnter ?? "my workstation";
            if (character is Robot && character.Day <= 2)
                return $"Destination: {reason}. Route is programmed and authorised.";
            string[] opts = {
                $"I'm heading to {reason}.",
                $"My destination is {reason}. It's on the pass.",
                $"Going to {reason} — I'm already late.",
                $"{reason}. Same as every day."
            };
            return opts[rnd.Next(opts.Length)];
        }

        // ════════════════════════════════════════════════════════════════
        //  ГРАЖДАНСТВО
        // ════════════════════════════════════════════════════════════════
        private static string GetCitizenshipAnswer(Character character)
        {
            var rnd = new Random();
            if (character is Alien && character.Day <= 4)
            {
                string[] a = {
                    "I am a registered visitor. All documentation is valid.",
                    "Non-human classification, registered. The permit is current.",
                    "Visitor status, properly registered. You can check the system.",
                };
                return a[rnd.Next(a.Length)];
            }
            if (character is Robot)
                return "Synthetic unit, registered and licensed. Serial on file.";
            string[] h = {
                "Colony citizen. Born here.",
                "Citizen, Sector B. Been here my whole life.",
                "I'm a registered resident. Everything is in order.",
                "Colony citizen. It's on the document."
            };
            return h[rnd.Next(h.Length)];
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            foreach (var key in keywords)
                if (text.Contains(key)) return true;
            return false;
        }
    }
}
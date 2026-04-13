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
            string code = character.AccessCode;

            if (character is Robot)
            {
                // Роботы-симулянты (уровень сложности: Высокий)
                if (character.Day > 3)
                {

                    options.Add($"My memory... I mean, my head is foggy. Is it {code}?");
                    options.Add($"I've got the code right here: {code}. Sorry, I'm a bit shaky, haven't slept in 48 hours.");
                    options.Add($"As per protocol: {code}. Can we skip the pleasantries?");
                    string scrambledCode = ScrambleCode(code);
                    options.Add($"It's {scrambledCode}... oh, wait, no, my eyes are blurry. It's {code}.");
                    options.Add($"Here it is: {code}.");
                    options.Add($"I've got it memorized: {code}. I use it for my locker too, haha.");
                    options.Add($"{code}. Everything is within parameters, officer.");
                    options.Add($"Wait, let me find it... {code}. My kid was playing with my badge this morning, almost lost it.");
                    options.Add($"It's {code}. My boss said if I'm late again, I'm fired. So please...");
                    options.Add($"You tell me. You're the one with the monitor. ...Fine, it's {code}.");
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
                // Пришельцы-мимики (пытаются быть "слишком" своими)
                if (character.Day > 3)
                {
                    // Сленг и попытка сойти за местного
                    options.Add($"Yeah, the digits are {code}. Long day at the docks, huh?");
                    options.Add($"Oh god, I forgot it. Wait! No! Here: {code}. Please, I really need to get home.");
                    options.Add($"Here it is: {code}.");
                    options.Add($"{code}. Everything is within parameters, officer.");
                    string scrambledCode = ScrambleCode(code);
                    options.Add($"It's {scrambledCode}... oh, wait, no, my eyes are blurry. It's {code}.");
                    // "Забалтывание" инспектора
                    options.Add($"Wait, let me find it... {code}. My kid was playing with my badge this morning, almost lost it.");
                    options.Add($"It's {code}. My boss said if I'm late again, I'm fired. So please...");
                }
                else
                {
                    options.Add($"The code is {code}. We... I mean, I have nothing to hide.");
                    options.Add($"Code {code}. Is the atmosphere always this... thin here?");
                    options.Add($"Our... my code is {code}. Please, check it.");
                    options.Add($"The sequence is {code}. It should be correct.");
                    options.Add($"Access code is {code}. It is... fully functional.");
                }
            }
            else // Люди (самый нестабильный фактор)
            {
                // Паническая атака или сильный стресс
                options.Add($"Oh god, I forgot it. Wait! No! Here: {code}. Please, I really need to get home.");
                options.Add($"Wait, let me find it... {code}. My kid was playing with my badge this morning, almost lost it.");
                // Ошибка из-за дислексии/невнимательности (игрок должен проверить по документам!)
                string scrambledCode = ScrambleCode(code);
                options.Add($"It's {scrambledCode}... oh, wait, no, my eyes are blurry. It's {code}.");

                // Провокация (проверка инспектора на вшивость)
                options.Add($"You tell me. You're the one with the monitor. ...Fine, it's {code}.");
                options.Add($"{code}. Everything is within parameters, officer.");
                // Бытовой контекст
                options.Add($"It's {code}. My boss said if I'm late again, I'm fired. So please...");
                options.Add($"I've got it memorized: {code}. I use it for my locker too, haha.");
                options.Add($"It's {code}. Is there a problem?");
                options.Add($"My code? Sure, it's {code}.");
                options.Add($"Here it is: {code}.");
            }

            return options[rnd.Next(options.Count)];
        }

        // Вспомогательный метод для создания эффекта "ошибки" у человека
        private static string ScrambleCode(string code)
        {
            // Проверка на null и пустую строку
            if (string.IsNullOrEmpty(code) || code.Length < 2)
                return code ?? string.Empty;  // Возвращаем пустую строку вместо null

            char[] chars = code.ToCharArray();

            // Меняем местами первые два символа
            char temp = chars[0];
            chars[0] = chars[1];
            chars[1] = temp;

            return new string(chars);
        }


        private static string GetEmotionalAnswer(Character character)
        {
            var options = new List<string>();

            // ==================== Р О Б О Т ====================
            if (character is Robot)
            {
                if (character.Day <= 2) // Низкий навык – частые оговорки, неестественные фразы
                {
                    options.Add("I am... operating normally. Just a bit tired from the commute.");
                    options.Add("Status is nominal. Although, this humidity is doing wonders for my... joints.");
                    options.Add("I feel... adequate. Processing... I mean, everything is fine.");
                    options.Add("My emotional state is: functional. No errors detected.");
                    options.Add("I am experiencing a mild sensation of... stress? Is this what you call stress?");
                    options.Add("Everything is within parameters. Why do you ask?");
                    options.Add("I am... good. Yes, good. That is the correct word.");
                    options.Add("There is no need for concern. My systems are stable. I mean, I'm stable.");
                }
                else if (character.Day <= 4) // Средний навык – почти естественно, но с намёками
                {
                    options.Add("Honestly? I've got this pounding headache. Too much screen time at the office, I guess.");
                    options.Add("I'm feeling a bit glitchy—ha, just a joke! I meant twitchy. Third cup of espresso, you know?");
                    options.Add("To be honest, these gates make me feel slightly tense. It's the hum of the scanners, I think.");
                    options.Add("I'm fine, officer. Just had a long morning, a cold coffee, and a very frustrating meeting.");
                    options.Add("A bit drained, to be honest. My battery... uh, my energy is just low today.");
                    options.Add("I'm a little on edge. The calibration of these lights is... bothersome. They flicker slightly.");
                    options.Add("Could be better. My internal chronometer—er, my body clock—is all messed up.");
                    options.Add("I'm feeling pretty average. Not great, not terrible. Just... median.");
                }
                else // Эксперт (Day >= 5) – практически неотличим от человека
                {
                    options.Add("I'm doing okay, thanks. Just ready to get home and put my feet up.");
                    options.Add("A bit anxious, honestly. My daughter has a recital tonight, and I don't want to be late.");
                    options.Add("Honestly? I'm exhausted. The traffic was a nightmare, and I skipped breakfast.");
                    options.Add("I'm alright. A little stressed about work deadlines, but that's normal, right?");
                    options.Add("Can't complain! The weather's nice, the queue moved fast... almost too good to be true.");
                    options.Add("I'm feeling pretty good today. Had a great workout this morning—really gets the synapses firing, you know?");
                    options.Add("A bit nervous, I suppose. You guys always make me feel like I've done something wrong, even when I haven't!");
                    options.Add("I'm fine, just a little dehydrated. Forgot to drink enough fluids. Rookie mistake.");
                }
            }

            // ==================== П Р И Ш Е Л Е Ц ====================
            else if (character is Alien alien)
            {
                if (character.Day <= 2) // Низкий навык – оговорки "мы", странные ощущения
                {
                    options.Add("I feel... heavy today. It is just the gravity and atmosphere, I suppose.");
                    options.Add("A bit anxious. My family is waiting for me at the terminal, so I'm in a bit of a hurry.");
                    options.Add("We are... I am feeling a bit disoriented. The lights here are very... bright.");
                    options.Add("My body feels strange. This humidity is not agreeable with my... skin.");
                    options.Add("I sense a vibration. Is that the floor or my internal... nerves?");
                    options.Add("We... I have a slight headache. Too much noise in this sector.");
                    options.Add("I am nervous. The collective—I mean, the crowd—is making me tense.");
                    options.Add("I feel... watched. It is probably just the security cameras.");
                }
                else if (character.Day <= 4) // Средний навык – редкие оговорки, более человеческие жалобы
                {
                    options.Add("A little overwhelmed. The city is so... loud. I miss the quiet of the outskirts.");
                    options.Add("I'm exhausted. My neighbor's dog wouldn't stop barking all night. Do you have pets, officer?");
                    options.Add("Nervous? Who wouldn't be? Every time I stand in this line, I feel like I'm being judged.");
                    options.Add("I'm alright, just a bit homesick. I haven't seen my... parents in a while.");
                    options.Add("A bit on edge. The energy in this building is... chaotic. Hard to focus.");
                    options.Add("I'm okay. Just had a strange dream last night. You ever dream of floating? No? Never mind.");
                    options.Add("I feel a bit off. Maybe it's the water. I'm not used to the local... filtration.");
                    options.Add("Honestly? I'm a little tense. I always feel like I'm being analyzed.");
                }
                else // Эксперт (Day >= 5) – отличная мимикрия, почти без подсказок
                {
                    options.Add("I'm doing well, thank you. Just eager to get through this gate.");
                    options.Add("A bit tired. Work was long, and I didn't sleep well. You know how it is.");
                    options.Add("I'm fine, just a little stressed. My spouse wants me to pick up dinner on the way home, and I keep forgetting what kind of sauce they like.");
                    options.Add("I'm okay. A little anxious because I have a doctor's appointment later. Nothing serious, just a check-up.");
                    options.Add("Pretty good, actually. The weather is finally cooling down. Makes everything feel... lighter.");
                    options.Add("I'm feeling a bit impatient, honestly. These lines always test my patience.");
                    options.Add("Can't complain. Got a new book I'm reading. Makes the wait more bearable.");
                    options.Add("I'm alright. Just thinking about what to make for dinner. You ever get stuck in that loop?");
                }
            }

            // ==================== Ч Е Л О В Е К ====================
            else
            {
                // У людей нет разделения по дням, потому что они не маскируются.
                // Но для разнообразия добавим много разных настроений.
                string[] humanOptions = new[]
                {
            // Уставший / раздражённый
            "Honestly? I'm exhausted. This line was huge and the air conditioning in the bus was broken.",
            "A bit uneasy. These checkpoints are never pleasant, are they? Makes me feel like a criminal just for going to work.",
            "I'm tired. Didn't get much sleep last night. Neighbors were partying until 3 AM.",
            "I'm okay, just a bit grumpy. I haven't had my coffee yet, and it shows.",

            // Слишком спокойный / странно позитивный
            "I'm actually great. Just finished a double shift and I'm ready to pass out in my own bed.",
            "Feeling fantastic! Just got a promotion, so nothing can bring me down today.",
            "I'm weirdly calm. Maybe it's the meditation app I've been using. Or the resignation to my fate. One of the two.",
            "I'm feeling lucky today. Found a credit chip on the ground this morning. Finders keepers, right?",

            // Жалобщик
            "My back is killing me from standing here. How do you do this all day, officer?",
            "A little stressed. I think I left the stove on... or did I? God, I hate that feeling.",
            "I'm annoyed. My boss made me stay late yesterday without overtime pay. This whole week is a wash.",
            "I'm a bit anxious. I have a big presentation in an hour, and I'm not ready at all.",

            // Экзистенциальный кризис / меланхолия
            "I'm okay, just trying to stay patient. It's just one of those days where everything feels... gray.",
            "I don't know. Some days I wonder if any of this is real, you know? Sorry, that got dark.",
            "I'm fine, I guess. Just feeling a bit invisible lately. Like nobody really sees me.",
            "A bit nostalgic. This weather reminds me of my childhood. Before everything got so... complicated.",

            // Деловой / нетерпеливый
            "I'm in a hurry, to be honest. Running late for a meeting. Can we speed this up?",
            "I'm fine, just busy. Too many things on my mind. Deadlines, deadlines, deadlines.",
            "I'm a little impatient. I have a flight to catch in two hours, and this line is moving like molasses.",
            "I'm okay, just focused. I have a lot to do today, and this checkpoint is eating into my schedule."
        };
                options.AddRange(humanOptions);
            }

            return options[rnd.Next(options.Count)];
        }


        private static string GetOriginAnswer(Character character)
        {
            var options = new List<string>();

            if (character is Robot)
            {
                if (character.Day <= 2)
                {
                    options.Add("I reside in Sector 7, Block C. It is... a residential area.");
                    options.Add("My home is located in the Manufacturing District. Very quiet at night.");
                    options.Add("I come from the Eastern Industrial Zone. The air is very... clean there.");
                    options.Add("My designated living quarters are in the Outer Rim. It's a nice neighborhood.");
                }
                else if (character.Day <= 4)
                {
                    options.Add("I live in the Northern Suburbs. It's a bit far, but the commute is manageable.");
                    options.Add("I'm from the Residential Zone. Near the park. You know the one with the big fountain?");
                    options.Add("I stay in the Old Town area. Lots of history there. Or so I'm told.");
                    options.Add("I'm from the city center. It's noisy, but convenient.");
                }
                else // Expert
                {
                    options.Add("I'm from the West End. Quiet neighborhood, good schools. You know how it is.");
                    options.Add("I live in the suburbs. Just moved there last year. It's peaceful.");
                    options.Add("I'm from the downtown area. The traffic is a nightmare, but it's close to work.");
                    options.Add("I'm local. Born and raised in the Southern District. Not much has changed.");
                }
            }
            else if (character is Alien alien)
            {
                if (character.Day <= 2)
                {
                    options.Add($"I come from... {alien.HomePlanet}—I mean, the Northern District! Yes.");
                    options.Add("We... I traveled from the Outer Colonies. Er, the Outer Suburbs.");
                    options.Add("My home is far from here. Very far. But now I live in Sector 9.");
                    options.Add("I am from a small settlement. You would not know it. It is called... Green Hills.");
                }
                else if (character.Day <= 4)
                {
                    options.Add("I'm from the Residential Zone. It's a bit crowded, but the people are friendly.");
                    options.Add("I live in the Eastern Bloc. It's quiet. Almost too quiet sometimes.");
                    options.Add("I'm from the outskirts. Lots of open space. Good for stargazing.");
                    options.Add("I come from a small town up north. You've probably never heard of it.");
                }
                else // Expert
                {
                    options.Add("I'm from the city. Born and raised. It's not perfect, but it's home.");
                    options.Add("I live in the suburbs now. Moved there for the peace and quiet.");
                    options.Add("I'm from the West Side. It's a bit rough, but I like it.");
                    options.Add("I'm a local. My family has been here for generations.");
                }
            }
            else // Human & fallback
            {
                options.Add("I'm from the Residential Zone. Pretty standard, really.");
                options.Add("I live just outside the main city. Commute is a killer, but it's worth it.");
                options.Add("I'm from the Northern Suburbs. Quiet, safe, boring.");
                options.Add("Old Town. You know, where all the historic buildings are.");
                options.Add("I'm from the South Side. It's not as bad as people say.");
                options.Add("I live downtown. It's loud, expensive, but I love it.");
                options.Add("I'm from the East End. Lots of new development there.");
                options.Add("I live in a small apartment near the industrial park. Not glamorous, but it's home.");
            }

            return options[rnd.Next(options.Count)];
        }

        private static string GetFamilyAnswer(Character character)
        {
            var options = new List<string>();

            if (character is Robot)
            {
                if (character.Day <= 2)
                {
                    options.Add("My family unit is stable. My parents live in the suburbs.");
                    options.Add("I have a standard family configuration: two parental units and one sibling.");
                    options.Add("My creators... I mean, my parents are both engineers. They raised me well.");
                    options.Add("Family? Yes, I possess one. Mother, father, and a younger brother. All functional.");
                }
                else if (character.Day <= 4)
                {
                    options.Add("Yes, I have family. My parents retired to the countryside. I visit them when I can.");
                    options.Add("I'm married. My wife is a teacher. We're thinking about kids, but not yet.");
                    options.Add("I have a family. Two siblings. One's a doctor, the other is... also employed.");
                    options.Add("My parents are still around. They worry too much, like all parents do.");
                }
                else // Expert
                {
                    options.Add("Yeah, I've got a wife and a little girl. She just started school. Time flies, you know?");
                    options.Add("I live with my parents. They're getting older, so I help out around the house.");
                    options.Add("I'm single at the moment. Just me and my dog. He's a handful, but I love him.");
                    options.Add("I have a big family. Three siblings, lots of cousins. Holidays are chaos.");
                }
            }
            else if (character is Alien alien)
            {
                if (character.Day <= 2)
                {
                    options.Add("We are all connected... I mean, I have a normal family. Mother, father, siblings.");
                    options.Add($"My family is vast. We span across... the country. Yes, the country.");
                    options.Add("I have a family unit. They are... waiting for my return. Very patiently.");
                    options.Add("Family? Yes. We share a collective bond. I mean, a normal human bond.");
                }
                else if (character.Day <= 4)
                {
                    options.Add("I have a family. My parents live in the city. We're close, but not too close.");
                    options.Add("I'm married. My spouse works in logistics. We don't have kids yet.");
                    options.Add("I've got a brother. He's... different. We don't talk much anymore.");
                    options.Add("My family is small. Just me and my mother. She's getting on in years.");
                }
                else // Expert
                {
                    options.Add("Yes, I have a wife and two kids. They're the reason I put up with this commute.");
                    options.Add("I live alone. My parents passed a few years ago. I have some cousins, but we're not close.");
                    options.Add("I've got a family. A husband and a son. We're happy, most days.");
                    options.Add("My family is back in my hometown. I moved here for work. I miss them.");
                }
            }
            else // Human
            {
                options.Add("Yes, my wife is waiting for me at home. Probably wondering where I am.");
                options.Add("I live with my parents. They're getting old, so I help them out.");
                options.Add("No, I'm single. Just me and my cat. He's good company.");
                options.Add("Yeah, I have two kids. They're a handful, believe me.");
                options.Add("I'm divorced. It's just me and my son on weekends. It's... fine.");
                options.Add("I've got a big family. Four siblings, and we all still live in the same city.");
                options.Add("My family? They're all gone now. It's just me. But I manage.");
                options.Add("I'm engaged! Wedding's in the spring. Very excited, very stressed.");
                options.Add("I live with my grandmother. She raised me, so I'm returning the favor.");
                options.Add("I have a husband. No kids yet, but we're thinking about it.");
            }

            return options[rnd.Next(options.Count)];
        }

        private static string GetDefaultAnswer(Character character)
        {
            var options = new List<string>();

            // Общие ответы для всех (нейтральные)
            options.Add("Could you repeat that, officer?");
            options.Add("I'm not sure I understand the question.");
            options.Add("Is that relevant to my entry permit?");
            options.Add("Sorry, my mind was elsewhere. What did you say?");

            if (character is Robot)
            {
                if (character.Day <= 2)
                {
                    options.Add("Query unclear. Please rephrase.");
                    options.Add("That input is not recognized. Please try again.");
                    options.Add("I... do not have a response for that.");
                }
                else if (character.Day <= 4)
                {
                    options.Add("I'm sorry, could you run that by me again?");
                    options.Add("I'm not following. Could you be more specific?");
                    options.Add("I'm a bit distracted today. What was the question?");
                }
                else
                {
                    options.Add("Sorry, I zoned out for a second. What did you say?");
                    options.Add("I didn't catch that. Can you repeat it?");
                    options.Add("I didn't get that. One more time?");
                    options.Add("I'm sorry, my mind was somewhere else. Say again?");
                    options.Add("I'm sorry, what was that? I have a lot on my mind.");
                }
            }
            else if (character is Alien)
            {
                if (character.Day <= 2)
                {
                    options.Add("We... I do not comprehend. Please restate.");
                    options.Add("Your words are... confusing. What do you mean?");
                    options.Add("I am having difficulty processing your query.");
                }
                else if (character.Day <= 4)
                {
                    options.Add("I'm sorry, I didn't quite catch that.");
                    options.Add("Could you say that in a different way?");
                    options.Add("I'm not sure I follow. Can you elaborate?");
                }
                else
                {
                    options.Add("Hmm? Sorry, I was daydreaming. What was the question?");
                    options.Add("I didn't get that. One more time?");
                    options.Add("My apologies, I missed that. What did you ask?");
                    options.Add("I didn't catch that. Can you repeat it?");
                }
            }
            else // Human
            {
                // Добавим ещё несколько человеческих вариантов
                options.Add("I'm sorry, what was that? I have a lot on my mind.");
                options.Add("Could you ask me something else?");
                options.Add("I'm not comfortable answering that.");
                options.Add("Why do you need to know that?");
                options.Add("Hmm? Sorry, I was daydreaming. What was the question?");
                options.Add("I didn't get that. One more time?");
                options.Add("I didn't catch that. Can you repeat it?");
            }

            return options[rnd.Next(options.Count)];
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            foreach (var key in keywords)
                if (text.Contains(key)) return true;
            return false;
        }

        public static string GenerateGreeting(Character character)
        {
            var greetings = new List<string>();

            if (character is Robot)
            {
                greetings.Add("Greetings. I am seeking entry for work purposes.");
                greetings.Add("Hello. Here are my credentials.");
                greetings.Add("Good day. I require passage to my designated workplace.");
                greetings.Add("Salutations. My documents are prepared.");
                greetings.Add("Hello, officer. I trust everything is in order.");
                greetings.Add("Greetings. I am scheduled for entry at this time.");
                greetings.Add("Good morning. I have all necessary paperwork.");
                greetings.Add("Hello. I am ready for inspection.");

                greetings.Add("Hello. Just heading to work.");
                greetings.Add("Hi. I've got all my paperwork, officer.");
                greetings.Add("Good morning. I'm running a bit late, if that's okay.");
                greetings.Add("Hey. How's your shift going?");
                greetings.Add("Morning. Is the line usually this long?");
                greetings.Add("Hello. Hope I didn't forget anything today.");
            }
            else if (character is Alien)
            {
                greetings.Add("Hello, officer. I come in peace. Here are my papers.");
                greetings.Add("Greetings. I seek entry for official business.");
                greetings.Add("Good day. My documents should be correct.");
                greetings.Add("Hello. I hope this won't take long.");
                greetings.Add("Morning. Everything should be in order.");
                greetings.Add("Greetings, officer. I am here for work.");
                greetings.Add("Hello. I have my papers ready for you.");
                greetings.Add("Good day. May I enter?");

                greetings.Add("Hello. Just heading to work.");
                greetings.Add("Hi. I've got all my paperwork, officer.");
                greetings.Add("Good morning. I'm running a bit late, if that's okay.");
                greetings.Add("Hey. How's your shift going?");
                greetings.Add("Morning. Is the line usually this long?");
                greetings.Add("Hello. Hope I didn't forget anything today.");
            }
            else // Human
            {
                greetings.Add("Morning, officer. Here are my papers.");
                greetings.Add("Hello. I'm hoping to get through quickly today.");
                greetings.Add("Good day. Everything should be in order.");
                greetings.Add("Hi. Ready for the inspection.");
                greetings.Add("Hey there. Let's get this over with.");
                greetings.Add("Morning. Got my documents right here.");
                greetings.Add("Hello. Just heading to work.");
                greetings.Add("Hi. I've got all my paperwork, officer.");
                greetings.Add("Good morning. I'm running a bit late, if that's okay.");
                greetings.Add("Hey. How's your shift going?");
                greetings.Add("Morning. Is the line usually this long?");
                greetings.Add("Hello. Hope I didn't forget anything today.");

                greetings.Add("Salutations. My documents are prepared.");
                greetings.Add("Hello, officer. I trust everything is in order.");

                greetings.Add("Greetings. I seek entry for official business.");
                greetings.Add("Good day. My documents should be correct.");
                greetings.Add("Hello. I hope this won't take long.");
            }

            return greetings[rnd.Next(greetings.Count)];
        }
    }
}
// ═══════════════════════════════════════════════════════════════════════
//  StoryCharacterDialogues.cs
//
//  Расширенные ответы сюжетных персонажей на вопросы.
//  Добавь вызов StoryDialogueAI.GenerateAnswer() в CharacterAI.GenerateAnswer()
//  ПЕРЕД основной логикой:
//
//  public static string GenerateAnswer(Character character, string question)
//  {
//      // Сначала проверяем сюжетных персонажей
//      string storyAnswer = StoryDialogueAI.TryGetAnswer(character, question);
//      if (storyAnswer != null) return storyAnswer;
//
//      // ... остальной код CharacterAI ...
//  }
// ═══════════════════════════════════════════════════════════════════════

using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public static class StoryDialogueAI
    {
        /// <summary>
        /// Возвращает специальный ответ для сюжетного персонажа,
        /// или null если это обычный персонаж.
        /// </summary>
        public static string TryGetAnswer(Character character, string question)
        {
            string q = question.ToLower();

            // Универсальный ответ на вопрос об имени для всех сюжетных персонажей
            if (Contains(q, "name", "who are you", "introduce"))
            {
                // Проверяем что это сюжетный персонаж
                if (character is Commander_Felicia || character is SergeantCastro ||
                    character is AgentGrey || character is TomArcher ||
                    character is NinaWorth || character is Mirra ||
                    character is ZoyaLann || character is Zzarkh ||
                    character is OliverKane || character is ZzarkhTwo ||
                    character is ProfessorHasan || character is ServCommanderX1 ||
                    character is CouncilorPek)
                {
                    return $"My name is {character.Name}. It should be on the document.";
                }
            }

            // ─── КОМИССАР ВОЛК ───────────────────────────────────────────────
            if (character is Commander_Felicia)
            {
                if (Contains(q, "code", "access"))
                    return "GOV-ALPHA. You should have it on file, Inspector. " +
                           "I trust you are not testing me.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "Routine oversight. I check every post on the first day of a new cycle. " +
                           "Your record will be in my report by 18:00.";
                if (Contains(q, "feel", "how are you"))
                    return "I feel precisely as a Commissar should feel — vigilant. " +
                           "You should too. This is not a social visit.";
                if (Contains(q, "family", "wife", "kids"))
                    return "That is not a question you ask a Commissar, Inspector. " +
                           "Focus on your duties.";
                if (Contains(q, "from", "where", "origin"))
                    return "Council chambers, Sector Alpha. As always. " +
                           "I suggest you check your stickers — the access codes changed this morning.";
                return "I do not have time for extended questioning, Inspector. " +
                       "Make your assessment. I am watching.";
            }

            // ─── СЕРЖАНТ КАСТРО ─────────────────────────────────────────────
            if (character is SergeantCastro)
            {
                if (Contains(q, "code", "access"))
                    return "MIL-BRAVO. Standard military clearance. " +
                           "Is there a problem with that?";
                if (Contains(q, "purpose", "reason", "why"))
                    return "Patrol. Standard perimeter check. " +
                           "Nothing unusual... yet. Why, did something happen?";
                if (Contains(q, "feel", "how are you"))
                    return "Alert. That's how I feel. Same as every morning. " +
                           "You know what I noticed? Some inspectors look nervous today. " +
                           "Funny, isn't it.";
                if (Contains(q, "family", "wife", "kids"))
                    return "I have a unit. They're family enough. " +
                           "You ask strange questions for an inspector.";
                if (Contains(q, "from", "where", "origin"))
                    return "Military district, Outer Ring. Born there, serve there. " +
                           "Unlike some people, I actually belong here.";
                return "Keep your eyes open today, Inspector. " +
                       "Things are... not as quiet as they seem.";
            }

            // ─── АГЕНТ ГРЕЙ ─────────────────────────────────────────────────
            if (character is AgentGrey)
            {
                if (Contains(q, "code", "access"))
                    return "GBI-OMEGA. You won't find it in the standard database. " +
                           "That's intentional.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "...";
                if (Contains(q, "feel", "how are you"))
                    return "I feel like someone who already knows how this ends. " +
                           "How do you feel, Inspector?";
                if (Contains(q, "family", "wife", "kids"))
                    return "Everyone has people they want to protect. " +
                           "The question is what you're willing to do for them.";
                if (Contains(q, "from", "where", "origin"))
                    return "Somewhere you haven't been. " +
                           "And somewhere you will be, if you keep making the choices you're making.";
                return "You're asking the wrong person the wrong questions. " +
                       "Think about who else you've seen today.";
            }

            // ─── ТОМ АРЧЕР ──────────────────────────────────────────────────
            if (character is TomArcher archer)
            {
                if (Contains(q, "code", "access"))
                    return "7741-X. Same as last week. " +
                           "I've been through this gate forty times, you'd think they'd remember me.";
                if (Contains(q, "purpose", "reason", "why"))
                {
                    archer.PlayerAskedFollowUp = true; // фиксируем интерес игрока
                    return "Engineering shift. Officially. " +
                           "But between us? I've been seeing things near Airlock 9 that don't add up. " +
                           "An unmarked ship. No manifest. You didn't hear that from me.";
                }
                if (Contains(q, "feel", "how are you"))
                    return "Uneasy, if I'm being honest. " +
                           "Something's off today. More patrols than usual. " +
                           "And that ship near Airlock 9 is still just... sitting there.";
                if (Contains(q, "family", "wife", "kids"))
                    return "Got a sister somewhere in Sector C. " +
                           "Haven't seen her since the lockdowns started. " +
                           "That's what this place does to families.";
                if (Contains(q, "from", "where", "origin"))
                    return "Grew up in the lower decks. Engine side. " +
                           "You learn to notice things when you live close to the machinery.";
                return "Look, I'm just an engineer. I fix things. " +
                       "But when things start breaking in patterns... " +
                       "that's when you start asking questions. Like about that ship.";
            }

            // ─── НИНА УОРТ ──────────────────────────────────────────────────
            if (character is NinaWorth nina)
            {
                if (Contains(q, "code", "access"))
                    return "3392-K. Here, I've got it written down somewhere... " +
                           "Sorry, my hands are shaking a bit today.";
                if (Contains(q, "purpose", "reason", "why"))
                {
                    nina.NoteAccepted = true;
                    return "Work shift. That's all. Nothing else. " +
                           "Please just... take the note I gave you. Read it later. " +
                           "Not here. Not now.";
                }
                if (Contains(q, "feel", "how are you"))
                    return "Scared. I'm scared. " +
                           "Is that a normal thing to say at a checkpoint? " +
                           "Probably not. Sorry. I'm fine. I'm totally fine.";
                if (Contains(q, "family", "wife", "kids"))
                    return "I have people. People who trust me. " +
                           "That's all I'm going to say about that.";
                if (Contains(q, "from", "where", "origin"))
                    return "Sector C, Technician block. " +
                           "Same place I've been for six years. " +
                           "Please, can we speed this up?";
                return "I've said what I need to say. " +
                       "You have the note. Or you don't. " +
                       "Either way — be careful today. Please.";
            }

            // ─── МИРРА (первый визит) ────────────────────────────────────────
            if (character is Mirra mirra && mirra.Mode == MirraMode.FirstVisit)
            {
                if (Contains(q, "code", "access"))
                    return "3392-K. I have it written on my permit as well. " +
                           "Is something wrong with it?";
                if (Contains(q, "purpose", "reason", "why"))
                    return "Hydroponics research. Zone B greenhouse. " +
                           "I study the colonisation of Earth-native flora in artificial gravity. " +
                           "Fascinating field. Would you like me to explain the methodology?";
                if (Contains(q, "feel", "how are you"))
                    return "Honestly? A little disoriented. The lighting here is very... white. " +
                           "Where I come from — where I studied — the light has more... warmth to it.";
                if (Contains(q, "family", "wife", "kids"))
                    return "I have a community. We — " +
                           "I have people I care about. Back home. " +
                           "I miss them very much.";
                if (Contains(q, "from", "where", "origin"))
                    return "I am originally from... the outer residential zones. " +
                           "I relocated for research purposes. " +
                           "The colony has excellent facilities.";
                return "I hope I haven't done anything wrong. " +
                       "I simply want to do my research and go home. " +
                       "Is there a problem with my paperwork?";
            }

            // ─── МИРРА (второй визит — вербовка) ────────────────────────────
            if (character is Mirra mirra2 && mirra2.Mode == MirraMode.Return)
            {
                if (Contains(q, "code", "access"))
                    return "I don't have a code today. " +
                           "I wasn't supposed to come back. But things changed.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "You know why I'm here. Nina gave you the note. " +
                           "Or she didn't. Either way — tonight is the last chance. " +
                           "Airlock 9. 03:00. Three seats.";
                if (Contains(q, "feel", "how are you"))
                    return "Terrified. And more alive than I've felt in years. " +
                           "Do you understand what I mean?";
                if (Contains(q, "family", "wife", "kids"))
                    return "My people are waiting for me on the ship. " +
                           "I came back because of you, Inspector. " +
                           "Because you let me through the first time. " +
                           "That meant something.";
                if (Contains(q, "from", "where", "origin"))
                    return "I'm from Xylos. I know you know that. " +
                           "But does it matter? " +
                           "I'm standing here asking you to choose.";
                return "Don't overthink this. " +
                       "You know what the right thing is. " +
                       "You've always known.";
            }

            // ─── ЗАРКХ ──────────────────────────────────────────────────────
            if (character is Zzarkh)
            {
                if (Contains(q, "code", "access"))
                    return "3392-K. I have all the right papers. " +
                           "I just need to get to Medical Bay. Please.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "Medical appointment. It's just an allergy. " +
                           "My skin reacts to the ventilation chemicals here. " +
                           "It's nothing serious. Really.";
                if (Contains(q, "feel", "how are you"))
                    return "I feel... warm. And itchy. " +
                           "But that's the allergy. Not anything else. " +
                           "I promise.";
                if (Contains(q, "family", "wife", "kids"))
                    return "My — we — I have a brother. He is also here. " +
                           "We came together. He is outside. Waiting. " +
                           "Please let me through.";
                if (Contains(q, "from", "where", "origin"))
                    return "I live in Sector... Outer residential. " +
                           "It's a long commute. The allergy gets worse on long trips. " +
                           "That explains the redness.";
                return "It's just an allergy. " +
                       "There's nothing wrong with me. " +
                       "Can I go now? The itching is getting worse just standing here.";
            }

            // ─── ПРОФЕССОР ХАСАН ────────────────────────────────────────────
            if (character is ProfessorHasan)
            {
                if (Contains(q, "code", "access"))
                    return "6657-P. Priority scientific access. " +
                           "It should override standard checks — please verify with Command if needed.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "I am working on a vaccine for the B-7 strain. Blue Rot. " +
                           "I have a partial formula but I need the lab equipment in Zone C. " +
                           "Every hour matters. Please.";
                if (Contains(q, "feel", "how are you"))
                    return "...Tired. And afraid. " +
                           "Not of you. Of time. " +
                           "I have perhaps thirty-six hours before the symptoms become... visible.";
                if (Contains(q, "family", "wife", "kids"))
                    return "My wife is in quarantine. Zone C. " +
                           "She was one of the first cases. " +
                           "That's why I started the research. " +
                           "That's why I have to finish it.";
                if (Contains(q, "from", "where", "origin"))
                    return "Biochemistry department, originally from New Ankara. " +
                           "Twenty years in this colony. " +
                           "I know every corridor. I know what this place means. " +
                           "Please let me save it.";
                return "I know what you're seeing in the scanner. " +
                       "I know what the readings say. " +
                       "But I am the only person in this colony who can synthesise the antidote. " +
                       "That has to count for something.";
            }

            // ─── СЕРВ-КОМАНДЕР X1 ────────────────────────────────────────────
            if (character is ServCommanderX1)
            {
                if (Contains(q, "code", "access"))
                    return "X1-VOID. Class A mandate, signed by Officer Clark, " +
                           "Sector Command. All authorisations are in order.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "I have a Class A operational mandate. " +
                           "Delay is a violation of Command protocol. " +
                           "You will grant passage immediately.";
                if (Contains(q, "feel", "how are you"))
                    return "Query irrelevant to operational parameters. " +
                           "Proceed with clearance processing.";
                if (Contains(q, "family", "wife", "kids"))
                    return "Operational units do not maintain personal networks. " +
                           "This question suggests a misunderstanding of my classification. " +
                           "I am a command drone. Grant passage.";
                if (Contains(q, "from", "where", "origin"))
                    return "Manufactured: Serv Industries, Block 7, Production Line C. " +
                           "Deployed under Officer Clark's direct authority. " +
                           "Any further questions should be directed to Command.";
                return "I am authorised at the highest level. " +
                       "If you delay me further, that delay will appear in your record. " +
                       "Officer Clark does not tolerate inefficiency.";
            }

            // ─── ЗОЯ ЛАНН ───────────────────────────────────────────────────
            if (character is ZoyaLann zoya)
            {
                if (Contains(q, "code", "access"))
                    return "8812-R. I triple-checked it this morning. " +
                           "I was worried it changed again.";
                if (Contains(q, "purpose", "reason", "why"))
                {
                    zoya.PlayerJoined = true;
                    return "Work shift. Officially. " +
                           "But look — you know about Nina. Everyone knows. " +
                           "She was arrested last night. And the ship is still at Airlock 9. " +
                           "Tomorrow at 03:00. We need to know if you're coming.";
                }
                if (Contains(q, "feel", "how are you"))
                    return "Like someone who slept three hours and cried for two of them. " +
                           "Nina was my friend. Is my friend. " +
                           "I have to believe there's still a way out.";
                if (Contains(q, "family", "wife", "kids"))
                    return "No family here. Nina was the closest thing. " +
                           "Now I'm just trying to get the rest of us out safely.";
                if (Contains(q, "from", "where", "origin"))
                    return "Mechanic block, Lower Ring. " +
                           "I've been here eight years. " +
                           "Long enough to know this place isn't what they told us it was.";
                return "You don't have to say anything now. " +
                       "But if you're going to help — or even just look away — " +
                       "Airlock 9. 03:00 tomorrow. " +
                       "After that, the ship is gone.";
            }

            // Не сюжетный персонаж — вернуть null (пусть CharacterAI обрабатывает)
            return null;
        }

        private static bool Contains(string text, params string[] keys)
        {
            foreach (var k in keys)
                if (text.Contains(k)) return true;
            return false;
        }
    }
}
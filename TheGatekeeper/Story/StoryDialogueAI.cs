// ═══════════════════════════════════════════════════════════════════════
//  StoryCharacterDialogues.cs  — ПОЛНАЯ ПЕРЕЗАПИСЬ
//
//  Изменения:
//  • Волк — нарастающее давление при вопросах, -Loyalty если продолжаешь
//  • Арчер — не отвечает на вопросы, рассказывает только своё
//  • Кастро — давит, проверяет тебя, подозревает
//  • Грей — знает всё, пугает, смотрит как на подозреваемого
//  • Больные (Заркх, Оливер) — расширенные умоляющие диалоги
//  • Хасан — отчаяние и мольба
// ═══════════════════════════════════════════════════════════════════════

using System;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public static class StoryDialogueAI
    {
        private static Random _rnd = new Random();

        /// <summary>
        /// Возвращает специальный ответ для сюжетного персонажа,
        /// или null если это обычный персонаж.
        /// </summary>
        public static string TryGetAnswer(Character character, string question)
        {
            string q = question.ToLower();

            // ─── КОМИССАР ВОЛК ───────────────────────────────────────────────
            // При первом вопросе — предупреждает. При втором — приказывает.
            // Каждый вопрос стоит -Loyalty (кроме стандартного кода).
            if (character is CommissarWolf)
                return WolfAnswer(q);

            // ─── АГЕНТ ГРЕЙ ─────────────────────────────────────────────────
            if (character is AgentGrey)
                return GreyAnswer(q);

            // ─── СЕРЖАНТ КАСТРО ─────────────────────────────────────────────
            if (character is SergeantCastro)
                return CastroAnswer(q);

            // ─── ТОМ АРЧЕР ──────────────────────────────────────────────────
            if (character is TomArcher archer)
                return ArcherAnswer(q, archer);

            // ─── НИНА УОРТ ──────────────────────────────────────────────────
            if (character is NinaWorth nina)
                return NinaAnswer(q, nina);

            // ─── МИРРА ──────────────────────────────────────────────────────
            if (character is Mirra mirra)
                return MirraAnswer(q, mirra);

            // ─── ЗОЯ ЛАНН ───────────────────────────────────────────────────
            if (character is ZoyaLann zoya)
                return ZoyaAnswer(q, zoya);

            // ─── ЗАРКХ (больной) ────────────────────────────────────────────
            if (character is Zzarkh)
                return ZzarkhAnswer(q);

            // ─── ЗАРКХ-2 ────────────────────────────────────────────────────
            if (character is ZzarkhTwo)
                return ZzarkhTwoAnswer(q);

            // ─── ОЛИВЕР КЕЙН (больной) ──────────────────────────────────────
            if (character is OliverKane)
                return OliverAnswer(q);

            // ─── ПРОФЕССОР ХАСАН ────────────────────────────────────────────
            if (character is ProfessorHasan)
                return HasanAnswer(q);

            // ─── СЕРВ-КОМАНДЕР X1 ────────────────────────────────────────────
            if (character is ServCommanderX1)
                return ServX1Answer(q);

            // ─── КОМАНДЕР ФЕЛИСИЯ ────────────────────────────────────────────
            if (character is CommanderFelicia)
                return FeliciaAnswer(q);

            // ─── СОВЕТНИК ПЕК ────────────────────────────────────────────────
            if (character is CouncilorPek)
                return PekAnswer(q);

            // ─── ПРОМЕЖУТОЧНЫЙ ИНСПЕКТОР РАЭЛЬ ──────────────────────────────
            if (character is MidtermInspector)
                return RaelAnswer(q);

            return null; // обычный персонаж — пусть CharacterAI обрабатывает
        }

        // ════════════════════════════════════════════════════════════════════
        //  ВОЛК — нарастающее давление
        //  Каждый лишний вопрос стоит Loyalty.
        //  Если задаёшь вопросы — он отвечает с угрозой, потом приказывает.
        // ════════════════════════════════════════════════════════════════════
        private static string WolfAnswer(string q)
        {
            // Счётчик вопросов к Волку — хранится в EndingTracker
            EndingTracker.WolfWarnings++; // используем как счётчик вопросов за визит
            int count = EndingTracker.WolfWarnings;

            if (Contains(q, "code", "access"))
            {
                // Код — отвечает без потери лояльности, но с видом "ты должен знать"
                EndingTracker.WolfWarnings--; // не штрафуем за код
                return "GOV-ALPHA. It is on file. You should have verified it before I arrived. " +
                       "I trust this is not a test of my patience.";
            }

            // Первый вопрос — предупреждает мягко
            if (count == 1)
            {
                EndingTracker.Loyalty--;
                if (Contains(q, "purpose", "reason", "why"))
                    return "Routine inspection. I check every gate post on the first day of a new cycle. " +
                           "Your file is already open on my terminal. " +
                           "I would advise you to focus on your work rather than my business.";
                if (Contains(q, "feel", "how are you"))
                    return "Commissars do not answer personal questions at checkpoints, Inspector. " +
                           "I will note that you asked.";
                if (Contains(q, "family", "wife", "kids", "relatives"))
                    return "That is... not a question you ask a Commissar. " +
                           "I will let it pass. Once.";
                if (Contains(q, "from", "where", "origin"))
                    return "Council chambers, Sector Alpha. As always. " +
                           "I suggest you remember that the next time you feel like asking questions.";
                return "Inspector. I commend the initiative — playing the role properly. " +
                       "But I am not here to be questioned. I am here to observe. " +
                       "Let me pass.";
            }

            // Второй вопрос — предупреждает строго
            if (count == 2)
            {
                EndingTracker.Loyalty -= 2;
                return "You are wasting my time. " +
                       "I outrank every person in this corridor. " +
                       "I will not answer another question. Pass me through. Now.";
            }

            // Третий и далее — приказывает, штрафует серьёзно
            EndingTracker.Loyalty -= 3;
            string[] orders = {
                "That is an order, Inspector. Not a suggestion. Pass me through immediately " +
                "or I will have your post reassigned before end of shift.",

                "I am logging this interaction. Every second you delay me " +
                "is another line in your disciplinary record. " +
                "Do. Not. Test. Me.",

                "I have tolerated enough. " +
                "You will stamp that pass, you will say nothing, " +
                "and you will pray I do not remember your face when I write my report tonight.",

                "Inspector. I gave you a direct order. " +
                "The fact that you are still asking questions tells me everything " +
                "I need to know about this post. Let. Me. Through.",
            };
            return orders[_rnd.Next(orders.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  АГЕНТ ГРЕЙ — знает всё, давит психологически
        //  Не просто молчит — наблюдает, намекает, пугает
        // ════════════════════════════════════════════════════════════════════
        private static string GreyAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "GBI-OMEGA. " +
                       "You won't find it in the public database. " +
                       "That's by design. " +
                       "Just like most things about me.";

            if (Contains(q, "purpose", "reason", "why"))
            {
                // Если у игрока высокий RebelTrust — Грей намекает что знает
                if (EndingTracker.RebelTrust >= 2)
                    return "Observation. " +
                           "Specifically... yours. " +
                           "You've made some interesting choices this week, Inspector. " +
                           "Some of them will catch up with you.";
                return "Observation. Standard protocol. " +
                       "Nothing you need to worry about. " +
                       "Unless you do.";
            }

            if (Contains(q, "feel", "how are you"))
            {
                if (EndingTracker.RebelTrust >= 3)
                    return "I feel like someone who already knows how this ends for you. " +
                           "How do you feel, Inspector? " +
                           "Knowing that every document you've touched, " +
                           "every person you've let through — it's all on record.";
                return "I feel like someone who has been watching this gate for six days. " +
                       "And I feel like I've seen enough.";
            }

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "Everyone has people they want to protect. " +
                       "The question is what they're willing to do for them. " +
                       "What are you willing to do, Inspector?";

            if (Contains(q, "from", "where", "origin"))
                return "I come from places that don't appear on colony maps. " +
                       "I go to places that don't either. " +
                       "Right now I'm here. Looking at you.";

            if (Contains(q, "name", "who are you"))
                return "Grey. That's all you get. " +
                       "And before you ask — no, it's not on your approved contact list.";

            // Дефолт — зависит от состояния игрока
            if (EndingTracker.RebelTrust >= 4)
                return "You know what I find interesting? " +
                       "The people who ask the most questions " +
                       "are usually the ones with the most to hide. " +
                       "Airlock 9. I know, Inspector.";

            if (EndingTracker.BribesAccepted >= 2)
                return "Forty-seven credits. A hundred and twenty. Eighty more yesterday. " +
                       "I have the numbers. " +
                       "Do you want to keep asking questions?";

            string[] defaults = {
                "...",
                "You're asking the wrong person the wrong questions.",
                "Think about who else you've spoken to today.",
                "I already have everything I need from this conversation.",
                "Ask me something you actually want to know the answer to.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  СЕРЖАНТ КАСТРО — патруль, подозревает инспектора
        //  Активно ставит под сомнение действия игрока
        // ════════════════════════════════════════════════════════════════════
        private static string CastroAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "MIL-BRAVO. Standard military clearance. " +
                       "Though I'm more curious about some of the codes " +
                       "that have been stamped through this gate recently. " +
                       "Anything you want to tell me about that?";

            if (Contains(q, "purpose", "reason", "why"))
            {
                if (EndingTracker.Errors >= 2)
                    return "Patrol. And also — checking on you, specifically. " +
                           "Command flagged some irregularities at this post. " +
                           "You've been making mistakes. Or choices. " +
                           "I'm trying to figure out which.";
                return "Standard perimeter patrol. " +
                       "Nothing unusual. " +
                       "Although the traffic through Gate 7 has been... interesting lately. " +
                       "Anything you want to report, Inspector?";
            }

            if (Contains(q, "feel", "how are you"))
            {
                if (EndingTracker.BribesAccepted >= 1)
                    return "Alert. Same as always. " +
                           "You know what keeps me alert? " +
                           "The smell of money changing hands where it shouldn't. " +
                           "This gate has that smell today. You notice it?";
                return "Alert. Professional. " +
                       "Unlike some people I see at their posts lately. " +
                       "You look like you haven't slept. " +
                       "Or like you're thinking too hard about something.";
            }

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "I have a unit. That's my family. " +
                       "We look after each other. " +
                       "You have anyone looking after you, Inspector? " +
                       "Or just yourself these days?";

            if (Contains(q, "from", "where", "origin"))
                return "Military district, Outer Ring. Born and raised. " +
                       "I know every corridor in this colony. " +
                       "Including Airlock 9. " +
                       "Just so you know that I know.";

            if (Contains(q, "name", "who are you"))
                return "Sergeant Reina Castro. Counter-intelligence. " +
                       "You should recognise me — I've been through this gate before. " +
                       "Or maybe you've been too distracted to notice your regulars.";

            // Дефолт — зависит от состояния
            if (EndingTracker.RebelTrust >= 2)
                return "Keep your eyes open today, Inspector. " +
                       "People are watching this gate more carefully than you might think. " +
                       "People other than me.";

            string[] defaults = {
                "Stay sharp. Things are moving fast in this colony and " +
                "I'd hate to see a gate inspector caught on the wrong side of them.",

                "You know what's interesting about checkpoint work? " +
                "You see everyone. Everyone sees you too.",

                "I'm just doing my rounds. " +
                "But if you ever see something that doesn't sit right — " +
                "you report it. To command. Not to whoever slips you notes.",

                "A soldier's job is to notice things. " +
                "An inspector's job is to notice things. " +
                "Between the two of us — we should know everything that moves through here.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  ТОМ АРЧЕР — не отвечает на вопросы, говорит только своё
        //  Всегда возвращается к кораблю у шлюза 9
        // ════════════════════════════════════════════════════════════════════
        private static string ArcherAnswer(string q, TomArcher archer)
        {
            // Вопрос о цели — это именно то что он ждал
            if (Contains(q, "purpose", "reason", "why", "business"))
            {
                archer.PlayerAskedFollowUp = true;
                return "Work shift. On paper. " +
                       "But listen — and don't write this down — " +
                       "there's been an unmarked ship at Airlock 9 for three days now. " +
                       "No manifest. No crew log. Just sitting there. " +
                       "I've worked on engines my whole life. " +
                       "That ship is running cold. Ready to go. " +
                       "Someone is waiting for something to happen before they leave.";
            }

            // На любые другие вопросы — уходит в сторону, возвращается к своей теме
            if (Contains(q, "code", "access"))
                return "7741-X. Same code I've had for six months. " +
                       "You know they don't change codes for engineers? " +
                       "Too many systems to update, they say. " +
                       "Makes me wonder what else doesn't get updated around here. " +
                       "Like the manifest log at Airlock 9.";

            if (Contains(q, "feel", "how are you"))
                return "Uneasy, if I'm honest. " +
                       "I've been watching that airlock for three days now. " +
                       "The patrols changed route yesterday. They don't go near it anymore. " +
                       "Someone told them not to. That's not an accident.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "Got a sister somewhere in Sector C. Haven't seen her since the lockdowns. " +
                       "Honestly? " +
                       "That ship at Airlock 9 — I keep thinking about her when I look at it. " +
                       "A way out that nobody's supposed to know about.";

            if (Contains(q, "from", "where", "origin"))
                return "Lower decks. Engine side. Grew up with the machinery. " +
                       "That's how I know what an idling drive sounds like " +
                       "even when it's supposed to be cold. " +
                       "Airlock 9. The ship's been running for days.";

            if (Contains(q, "name", "who are you"))
                return "Tom Archer. Engineer, maintenance crew, Gate 7 regular. " +
                       "I'm the guy who notices things that don't belong. " +
                       "Like unmarked ships. Like missing manifests.";

            // Дефолт — всегда про корабль
            string[] defaults = {
                "I'm just saying — if you had to leave this colony in a hurry, " +
                "and someone had a ship waiting... " +
                "you'd want to know about it. Airlock 9.",

                "Don't ask me anything else. I've said what I came to say. " +
                "There's a ship at Airlock 9. " +
                "It won't be there forever. " +
                "That's all.",

                "Three days. I've counted. " +
                "That ship has been at Airlock 9 for three days and nobody's asking questions. " +
                "Nobody official, anyway.",

                "You know what engineers do when we find something that doesn't add up? " +
                "We trace the fault back to its source. " +
                "That ship is the fault. " +
                "Airlock 9 is the source.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  НИНА УОРТ — испуганная, торопится, умоляет взять записку
        // ════════════════════════════════════════════════════════════════════
        private static string NinaAnswer(string q, NinaWorth nina)
        {
            if (Contains(q, "code", "access"))
                return "3392-K. I wrote it on my wrist this morning because I was so nervous I'd forget. " +
                       "Please just check it and let me through.";

            if (Contains(q, "purpose", "reason", "why"))
            {
                nina.NoteAccepted = true;
                return "Work shift. Officially. " +
                       "Please — you have the note I gave you? " +
                       "Don't read it here. Don't let anyone see you read it. " +
                       "Just — later. When you're alone. " +
                       "It's important. More important than anything I could say out loud right now.";
            }

            if (Contains(q, "feel", "how are you"))
                return "Terrified. I'm terrified and I know it shows and I can't help it. " +
                       "Please don't ask me anything else. " +
                       "The more time I spend at this gate the more visible I am " +
                       "and I really, really need to not be visible right now.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "I have people. People who trust me completely. " +
                       "And I've already said too much. " +
                       "Please. The note. Just take the note.";

            if (Contains(q, "from", "where", "origin"))
                return "Sector C, Technician block. Six years, same address. " +
                       "I've never done anything wrong in six years. " +
                       "I need you to remember that when you read the note.";

            if (Contains(q, "name", "who are you"))
                return "Nina Worth. Technician. I'm in your system. " +
                       "But please — don't pull up my full file right now. " +
                       "I'll explain. Just — the note.";

            string[] defaults = {
                "I've said what I needed to say. You have the note. " +
                "Please just stamp my pass and forget you saw me nervous.",

                "Every second I stand here is a second someone might notice me standing here. " +
                "Please. Let me through.",

                "Whatever you decide — just know that I'm not a bad person. " +
                "I'm just trying to do the right thing with the options I have.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  МИРРА — первый визит спокойная, второй — прямая и срочная
        // ════════════════════════════════════════════════════════════════════
        private static string MirraAnswer(string q, Mirra mirra)
        {
            if (mirra.Mode == MirraMode.FirstVisit)
            {
                if (Contains(q, "code", "access"))
                    return "3392-K. I have it on my permit as well. " +
                           "Is there a problem with the number?";
                if (Contains(q, "purpose", "reason", "why"))
                    return "Hydroponics research. Zone B greenhouse. " +
                           "I study the adaptation of Earth-native flora in artificial gravity environments. " +
                           "It's slow work but the data is... worth staying for.";
                if (Contains(q, "feel", "how are you"))
                    return "A little disoriented. The light in this corridor is very white. " +
                           "Where I come from the spectrum is... different. " +
                           "More orange. Warmer. " +
                           "I miss it, sometimes.";
                if (Contains(q, "family", "wife", "kids", "relatives"))
                    return "I have a community. " +
                           "We — I have people I care about very much. " +
                           "They are... far from here. " +
                           "I think about them every day.";
                if (Contains(q, "from", "where", "origin"))
                    return "I am from the outer residential zones originally. " +
                           "I relocated for research purposes. " +
                           "The colony has excellent facilities. " +
                           "I have grown... attached to it.";
                string[] def1 = {
                    "I hope I haven't done anything wrong. I simply want to do my research.",
                    "Is there something specific you're looking for? I'm happy to answer.",
                    "I find these questions interesting, actually. You're quite thorough.",
                };
                return def1[_rnd.Next(def1.Length)];
            }
            else // MirraMode.Return
            {
                if (Contains(q, "code", "access"))
                    return "I don't have a code today. " +
                           "I wasn't supposed to come back at all. " +
                           "But Nina is arrested and someone needed to come.";
                if (Contains(q, "purpose", "reason", "why"))
                    return "You know why I'm here. " +
                           "The note. Zoya. Airlock 9. " +
                           "Tonight at 03:00. There are three seats and one of them has your name on it " +
                           "if you want it. " +
                           "This is the last time I can ask.";
                if (Contains(q, "feel", "how are you"))
                    return "More afraid than I have ever been. " +
                           "And more certain. " +
                           "Both at the same time. Is that something you understand?";
                if (Contains(q, "family", "wife", "kids", "relatives"))
                    return "My people are on the ship. Waiting. " +
                           "I came back through this gate for you specifically. " +
                           "Because you let me through the first time without hesitation. " +
                           "That meant something to me.";
                if (Contains(q, "from", "where", "origin"))
                    return "Xylos. You know that already. " +
                           "My home is gone. The colony here is all I have left. " +
                           "And it's about to become something I can't live in. " +
                           "So we leave.";
                string[] def2 = {
                    "Don't overthink this. You already know what you want to do.",
                    "Tonight. 03:00. That's all I have left to say.",
                    "The ship leaves whether you're on it or not. " +
                    "I just thought you deserved to know it was leaving.",
                };
                return def2[_rnd.Next(def2.Length)];
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  ЗОЯ ЛАНН — Нина арестована, последний шанс
        // ════════════════════════════════════════════════════════════════════
        private static string ZoyaAnswer(string q, ZoyaLann zoya)
        {
            if (Contains(q, "code", "access"))
                return "8812-R. I triple-checked it this morning. " +
                       "I've been checking everything three times today. " +
                       "That's what happens when your friend gets arrested.";

            if (Contains(q, "purpose", "reason", "why"))
            {
                zoya.PlayerJoined = true;
                return "Work shift. On paper. " +
                       "In reality — Nina was arrested last night. " +
                       "They came for her at 02:00. Her neighbours heard it. " +
                       "The ship at Airlock 9 is still there. Still waiting. " +
                       "Tomorrow at 03:00 it leaves. " +
                       "I need to know if you're coming with us.";
            }

            if (Contains(q, "feel", "how are you"))
                return "Like I slept three hours and cried for two of them. " +
                       "Nina was my closest friend in this colony. " +
                       "Is. She's still alive — I think. " +
                       "I have to believe there's still a way to make this right.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "No family here. Nina was the closest thing I had. " +
                       "Now I'm trying to get the rest of us out safely. " +
                       "Before they start making more arrests.";

            if (Contains(q, "from", "where", "origin"))
                return "Mechanic block, Lower Ring. Eight years. " +
                       "Long enough to know this place stopped being what they promised. " +
                       "Long enough to want out.";

            string[] defaults = {
                "You don't have to say anything now. " +
                "But if you're going to help — Airlock 9. Tomorrow. 03:00.",

                "I'm not asking you to be a rebel. " +
                "I'm asking you to decide if this place deserves your loyalty " +
                "after everything you've seen.",

                "Nina trusted you. She put your name in that note for a reason. " +
                "I'm just here to find out if she was right.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  ЗАРКХ — больной, умоляет, скрывает симптомы
        //  Расширенные умоляющие диалоги
        // ════════════════════════════════════════════════════════════════════
        private static string ZzarkhAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "3392-K. I have it. Everything is in order. " +
                       "Please — I need to get to the medical bay. " +
                       "The itching is getting worse just standing here.";

            if (Contains(q, "purpose", "reason", "why"))
                return "Medical appointment. It's an allergy — I've had it for years. " +
                       "The ventilation chemicals in this corridor make it worse. " +
                       "My doctor is waiting. " +
                       "If I don't get there soon the reaction will spread and then " +
                       "I'll look much worse and you'll have many more questions " +
                       "and none of this will be good for either of us. " +
                       "Please.";

            if (Contains(q, "feel", "how are you"))
                return "Warm. Very warm. And itchy. " +
                       "My skin feels tight. " +
                       "This is how the allergy presents — heat, itching, redness. " +
                       "Nothing else. I promise you it's nothing else. " +
                       "Please let me through.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "My brother. He's outside. Waiting for me. " +
                       "He came with me today because I asked him to — " +
                       "because I knew the reaction was bad and I didn't want to be alone. " +
                       "He can vouch for me. He's right outside. Please.";

            if (Contains(q, "from", "where", "origin"))
                return "Trappist-1e originally. " +
                       "I've been in this colony for three years. " +
                       "I have a work permit. I have a medical history. " +
                       "I have an appointment in twenty minutes that I am going to miss " +
                       "if you don't let me through.";

            if (Contains(q, "name", "who are you"))
                return "Zzarkh. Registered resident. " +
                       "Three years. Perfect record. " +
                       "I've never caused any trouble. " +
                       "Please look at my record.";

            // Дефолт — нарастающее отчаяние
            string[] defaults = {
                "It's just an allergy. I know what it looks like. " +
                "I know exactly what you're thinking. " +
                "But I've had this condition since I was young. " +
                "My medical records are all there. " +
                "Please — the longer I stand here, the worse it gets.",

                "I'm not sick. I'm allergic. There's a difference. " +
                "The certificate — I know it looks — " +
                "please, the redness is from the ventilation, not from — " +
                "please just let me through.",

                "My doctor will confirm everything. " +
                "If you want to call ahead, call Medical Bay Level 3, ask for Dr. Farr. " +
                "She'll tell you I have an appointment. " +
                "Please. I just need to get there.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  ЗАРКХ-2 — агрессивный, ищет брата
        // ════════════════════════════════════════════════════════════════════
        private static string ZzarkhTwoAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "I — I don't have one. My brother has the paperwork. " +
                       "He came through here. Where is he? " +
                       "What did you do with him?";

            if (Contains(q, "purpose", "reason", "why"))
                return "I want my brother. That's my purpose. " +
                       "He came through this gate and he didn't come back " +
                       "and I need to know where he is. " +
                       "Let me in. NOW.";

            if (Contains(q, "feel", "how are you"))
                return "Like I've been standing outside this gate for two hours " +
                       "watching people go in and not come out. " +
                       "Like something terrible happened to my brother in there. " +
                       "Let me in.";

            if (Contains(q, "name", "who are you"))
                return "I'm his brother. That's all you need to know. " +
                       "Where is Zzarkh? What did you do with him?";

            string[] defaults = {
                "You're wasting time. " +
                "My brother needs me. " +
                "Whatever you think you saw on that scanner — " +
                "he is my family. " +
                "Let me through.",

                "I don't care about your protocols. " +
                "I don't care about your codes and your stamps. " +
                "My brother is in there. " +
                "I am going in there.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  ОЛИВЕР КЕЙН — больной, дочь ждёт, отчаяние
        // ════════════════════════════════════════════════════════════════════
        private static string OliverAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "5521-M. Same as it's been for four years. " +
                       "I come through this gate every day. " +
                       "Every single day. You've seen my face a hundred times. " +
                       "Please.";

            if (Contains(q, "purpose", "reason", "why"))
                return "My daughter is inside. She's seven years old and she's been alone " +
                       "since this morning and I promised her I'd be home before dark. " +
                       "I'm a plumber. I'm not — I don't cause trouble. " +
                       "I just need to get to my daughter.";

            if (Contains(q, "feel", "how are you"))
                return "Not great. I'll be honest with you. " +
                       "I know what you're seeing. I know what it looks like. " +
                       "But I feel the same way I always feel after a long shift. " +
                       "Tired. A little flushed. My daughter is waiting. " +
                       "Please.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "One daughter. Maya. She's seven. " +
                       "Her mother passed two years ago. " +
                       "It's just us. " +
                       "She's been alone since morning and I promised — " +
                       "I promised her I'd be home. " +
                       "Please don't make me break that promise.";

            if (Contains(q, "from", "where", "origin"))
                return "Residential Block D. Same address for six years. " +
                       "I'm registered. I'm documented. I'm a plumber with a daughter. " +
                       "I am not — I'm just trying to go home.";

            if (Contains(q, "name", "who are you"))
                return "Oliver Kane. Plumber, Block D, six years in this colony. " +
                       "I have no record. I have a daughter named Maya. " +
                       "Please let me through.";

            // Дефолт — нарастающее отчаяние
            string[] defaults = {
                "I know what the mark on my cheek looks like. I know. " +
                "But I covered it because I knew nobody would let me through if they saw it " +
                "and my daughter is alone and she doesn't know how to cook " +
                "and she's scared of the dark and I need — " +
                "please. I just need to get to her.",

                "If I have whatever you think I have — " +
                "I promise you I will go directly to Medical Bay after I see my daughter. " +
                "I promise. One hour. I just need one hour with her first. " +
                "She doesn't know I'm sick. I don't want her to be alone when she finds out.",

                "What would you do? " +
                "If your child was waiting for you. " +
                "If you knew — if you thought — " +
                "what would you do?",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  ПРОФЕССОР ХАСАН — заражён, знает об этом, умоляет дать время
        // ════════════════════════════════════════════════════════════════════
        private static string HasanAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "6657-P. Priority scientific access. " +
                       "It should override standard checks — " +
                       "please verify with Command if needed, but please do it quickly.";

            if (Contains(q, "purpose", "reason", "why"))
                return "I am the only person in this colony with a partial synthesis pathway " +
                       "for the B-7 antidote. " +
                       "I have thirty-six hours, maybe less, before my own symptoms " +
                       "make it impossible to work. " +
                       "Lab C has the equipment I need. " +
                       "Every minute I spend at this gate is a minute the formula doesn't get finished. " +
                       "Please.";

            if (Contains(q, "feel", "how are you"))
                return "...You know the answer to that. " +
                       "The scanner knows the answer to that. " +
                       "I contracted B-7 three days ago from a patient I was trying to help. " +
                       "I am still functional. I am still lucid. " +
                       "I need to use the time I have left. " +
                       "Please let me use it.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "My wife is in quarantine in Zone C. " +
                       "She was one of the first cases. " +
                       "She doesn't know I'm infected. " +
                       "She thinks I'm working on her cure. " +
                       "She's right. I am. " +
                       "Let me finish it.";

            if (Contains(q, "from", "where", "origin"))
                return "Biochemistry department. New Ankara originally. " +
                       "Twenty-three years in this colony. " +
                       "I have given this place everything. " +
                       "Let me give it this one last thing.";

            if (Contains(q, "name", "who are you"))
                return "Professor Karim Hasan. Biochemist. " +
                       "Possibly the most important person you'll process today. " +
                       "And I say that without arrogance.";

            string[] defaults = {
                "I know what I am carrying. " +
                "I know the risk. I know what the protocol says. " +
                "But if I don't reach Lab C today, " +
                "everyone in this colony is going to need a gate inspector " +
                "who can process the infected without flinching. " +
                "Because there will be many more of us.",

                "The formula is in my head and on the pages I'm carrying. " +
                "If you detain me — if anything happens to me — " +
                "please, give the documents to Commander Felicia. " +
                "Not Commissar Wolf. Felicia. " +
                "She'll know what to do with them.",

                "I'm not asking for mercy. I'm asking for an hour. " +
                "One hour in Lab C and this colony has a chance. " +
                "That is what is standing between you and the end of everything here.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  СЕРВ-КОМАНДЕР X1 — давит мандатом, который фальшивый
        // ════════════════════════════════════════════════════════════════════
        private static string ServX1Answer(string q)
        {
            if (Contains(q, "code", "access"))
                return "X1-VOID. Class A mandate, authorised by Officer Clark, Sector Command. " +
                       "All clearance levels are embedded in the mandate document. " +
                       "Verify and grant passage.";

            if (Contains(q, "purpose", "reason", "why"))
                return "Operational directive. Classification level exceeds " +
                       "standard inspection clearance. " +
                       "I am not authorised to disclose mission parameters to gate personnel. " +
                       "The mandate covers all required access.";

            if (Contains(q, "feel", "how are you"))
                return "That question is not relevant to operational function. " +
                       "I am operating within all parameters. " +
                       "Please proceed with clearance.";

            if (Contains(q, "name", "who are you"))
                return "Serv-Commander X1. Command drone, Class A operational status. " +
                       "My deployment was authorised by Officer Clark. " +
                       "His signature is on the mandate. Verify it.";

            if (Contains(q, "family", "wife", "kids", "relatives"))
                return "Operational units do not maintain personal networks. " +
                       "This question indicates a fundamental misunderstanding of my classification. " +
                       "I require clearance, not conversation.";

            string[] defaults = {
                "The mandate is valid. Officer Clark authorised this deployment directly. " +
                "Any delay constitutes a violation of Command protocol " +
                "and will be logged accordingly.",

                "I have been patient with this process. " +
                "My operational window is closing. " +
                "Grant passage or explain the grounds for refusal to Officer Clark personally.",

                "Every second you delay me is logged. " +
                "Every question you ask is logged. " +
                "Officer Clark reviews these logs personally. " +
                "I suggest you consider that.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  КОМАНДЕР ФЕЛИСИЯ — финальная проверка, знает о документах
        // ════════════════════════════════════════════════════════════════════
        private static string FeliciaAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "CMD-ALPHA-FINAL. You'll find it in your emergency protocols. " +
                       "Not the standard issue ones — the ones in the sealed envelope " +
                       "you were given on Day 1. Did you open it?";

            if (Contains(q, "purpose", "reason", "why"))
                return "Final shift review. Your entire record. " +
                       "Every decision. Every stamp. Every person you let through. " +
                       "And — if you have anything to hand over — now is the time to hand it over. " +
                       "I am not Commissar Wolf. I don't share his interests.";

            if (Contains(q, "feel", "how are you"))
                return "Honestly? Concerned. " +
                       "This colony is at a decision point and the people making decisions " +
                       "are not the right people. " +
                       "That's not something I can say in a report. " +
                       "But I can say it here, to you, quietly.";

            if (Contains(q, "name", "who are you"))
                return "Commander Felicia. Direct command authority, Colony Station. " +
                       "Independent of the Council. Independent of Commissar Wolf. " +
                       "Remember that when you decide what to hand me.";

            string[] defaults = {
                "If you have documents — evidence, research data, anything — " +
                "this is the moment. What I do with them won't involve Wolf.",

                "You've been at this gate for seven days. " +
                "You've seen things. Made choices. " +
                "I'm not here to judge them. " +
                "I'm here to understand what happened here.",

                "Whatever you've been carrying — " +
                "you don't have to carry it alone anymore.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  СОВЕТНИК ПЕК — VIP, высокомерный
        // ════════════════════════════════════════════════════════════════════
        private static string PekAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "GOV-PRIORITY. It supersedes standard gate codes. " +
                       "You should see a priority override on your terminal. " +
                       "If you don't, that's a problem with your terminal, not my credentials.";

            if (Contains(q, "purpose", "reason", "why"))
                return "Official visit. Colony administrative matters. " +
                       "The details are classified above your clearance level. " +
                       "I suggest you process me quickly — I have a meeting in twelve minutes.";

            if (Contains(q, "feel", "how are you"))
                return "I feel like a Colony Councilor who has been standing at a checkpoint " +
                       "for longer than any Colony Councilor should ever stand at a checkpoint. " +
                       "How do you feel about that?";

            string[] defaults = {
                "I don't have time for extended processing. " +
                "Stamp the pass. I'll note the efficiency in my report.",

                "Is there a problem with my documentation? " +
                "No? Then we're done here.",

                "I've visited fourteen gate posts this week. " +
                "This is the only one that has made me wait.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ════════════════════════════════════════════════════════════════════
        //  ИНСПЕКТОР РАЭЛЬ — аудит, знает об ошибках
        // ════════════════════════════════════════════════════════════════════
        private static string RaelAnswer(string q)
        {
            if (Contains(q, "code", "access"))
                return "INS-MIDTERM. Internal audit clearance. " +
                       "Standard protocol for mid-cycle review.";

            if (Contains(q, "purpose", "reason", "why"))
            {
                if (EndingTracker.Errors >= 3)
                    return "Compliance audit. Your post specifically. " +
                           "I'm not going to pretend otherwise. " +
                           "You have " + EndingTracker.Errors + " logged errors this cycle. " +
                           "That's above threshold. We need to talk.";
                return "Mid-cycle compliance check. All gate posts, sequential. " +
                       "Your numbers are... interesting. Not the worst I've seen today.";
            }

            if (Contains(q, "feel", "how are you"))
                return "Tired. This is my ninth post today. " +
                       "Some inspectors make my job easy. " +
                       "Some... less so.";

            string[] defaults = {
                "I'm going to be straightforward with you: " +
                "your error count is elevated. " +
                "Whether that's incompetence or something else — " +
                "I need to determine that today.",

                "The financial irregularity reports from this sector are real. " +
                "I want you to know I've seen them. " +
                "I'm giving you the opportunity to tell me your side.",
            };
            return defaults[_rnd.Next(defaults.Length)];
        }

        // ─── Утилита ─────────────────────────────────────────────────────────
        private static bool Contains(string text, params string[] keys)
        {
            foreach (var k in keys)
                if (text.Contains(k)) return true;
            return false;
        }
    }
}
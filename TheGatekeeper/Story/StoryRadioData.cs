// ═══════════════════════════════════════════════════════════════════════
//  StoryRadio.cs — сюжетные радиопередачи по дням
//  Замените содержимое _radioByDay в RadioOverlay (OverlayManager.cs)
//  на версию из этого файла.
//
//  КАК ПРИМЕНИТЬ:
//  В OverlayManager.cs найдите класс RadioOverlay и поле _radioByDay —
//  замените его на новый массив StoryRadioData.GetMessages(day).
//  Также замените в методе Show(int day) строку:
//      _lblMessages.Text = string.Join("\n\n", _radioByDay[dayIdx]);
//  на:
//      _lblMessages.Text = StoryRadioData.GetMessages(day);
// ═══════════════════════════════════════════════════════════════════════

namespace TheGatekeeper
{
    public static class StoryRadioData
    {
        public static string GetMessages(int day)
        {
            switch (day)
            {
                case 1:
                    return
                        "> [06:02] Command HQ: Shift commencing. Standard protocol.\n\n" +
                        "> [06:18] Medical Bay: Minor radiation spike detected at Gate 3.\n" +
                        "          Geiger units offline — scheduled maintenance.\n" +
                        "          DO NOT use dosimeter today. Data unreliable.\n\n" +
                        "> [06:45] Perimeter-2: Unidentified movement near outer wall.\n" +
                        "          Situation under control. Continue normal ops.\n\n" +
                        "> [07:10] Colony News: Today's forecast — pressure drop in\n" +
                        "          sectors A and B. Residents advised to stay indoors.\n\n" +
                        "> [08:03] Unknown signal: ██ ██ ...they look just like us...";

                case 2:
                    return
                        "> [06:05] Command HQ: Day 2. Quota increased to 4 subjects.\n\n" +
                        "> [06:22] Security: Last shift — two synthetics passed undetected.\n" +
                        "          Inspector reminded: check access codes FIRST.\n\n" +
                        "> [06:50] Colony News: Radiation levels normalized overnight.\n" +
                        "          Geiger units still offline — repair delayed.\n" +
                        "          Dosimeter readings REMAIN INVALID today.\n\n" +
                        "> [07:30] Medical Bay: Outbreak of flu-like symptoms in Sector C.\n" +
                        "          Possibly related to yesterday's spike. Under observation.\n\n" +
                        "> [08:33] Unknown: ...don't trust the documents. They fake them now...";

                case 3:
                    return
                        "> [06:01] Command HQ: BREACH ALERT — Sector 4 compromised.\n" +
                        "          Unknown number of synthetics inside the colony.\n\n" +
                        "> [06:18] Security: Villain unit confirmed active. No description yet.\n" +
                        "          All inspectors on HIGH ALERT.\n\n" +
                        "> [06:40] Colony News: Geiger repair completed. Dosimeter ONLINE.\n" +
                        "          Elevated background in corridor 7 — unrelated to breach.\n\n" +
                        "> [07:15] Medical Bay: Blue Rot (B-7) symptom reports incoming.\n" +
                        "          First confirmed case — Sector 3. Quarantine pending.\n\n" +
                        "> [07:44] Agent X [encrypted]: Trust. No. One.";

                case 4:
                    return
                        "> [06:03] Command HQ: Commissar Wolf issues Priority Directive #4.\n" +
                        "          All passes require double verification today.\n\n" +
                        "> [06:30] Colony News: A woman was detained last night trying to\n" +
                        "          pass unauthorized documents. Case under tribunal review.\n\n" +
                        "> [07:00] Medical Bay: B-7 case count: 3. Sector C under soft lockdown.\n" +
                        "          Symptoms: blue discolouration, subdermal patches.\n" +
                        "          Easily concealed with pigment creams. LOOK CAREFULLY.\n\n" +
                        "> [07:45] Perimeter-2: Unmarked ship spotted near Airlock 9.\n" +
                        "          No registration. Authorities notified.\n\n" +
                        "> [08:10] Unknown: ...the ship is still there. They're waiting...";

                case 5:
                    return
                        "> [06:00] Command HQ: Day 5. Synthetic mimicry at new high.\n" +
                        "          Biometrics UNRELIABLE. Trust dialogue only.\n\n" +
                        "> [06:25] Colony News: Serv-Command units deployed to outer sectors.\n" +
                        "          Some have reported drones attempting to enter civilian zones.\n" +
                        "          Verify ALL robot mandates — forgeries in circulation.\n\n" +
                        "> [07:10] Medical Bay: B-7 quarantine expanded. 11 confirmed cases.\n" +
                        "          Do NOT allow visibly symptomatic subjects past the gate.\n\n" +
                        "> [07:50] Security: Intel suggests a key rebel contact operates\n" +
                        "          near Gate inspection points. Identity unknown.\n\n" +
                        "> [08:20] [ENCRYPTED — PRIORITY]: Project VOID-VEIL entering phase 2.";

                case 6:
                    return
                        "> [06:01] Command HQ: Day 6. This is not a drill.\n" +
                        "          All sector gates reinforced. No exceptions.\n\n" +
                        "> [06:40] Colony News: Zzarkh-class alien sighted near Medical Bay.\n" +
                        "          Highly contagious — do not approach without biohazard protocol.\n\n" +
                        "> [07:05] Medical Bay: Professor Hasan requests emergency lab access.\n" +
                        "          Claims to be working on B-7 antidote.\n" +
                        "          Status: INFECTED. Clearance: PENDING REVIEW.\n\n" +
                        "> [07:30] Security: Serv-Legion units 1, 2 and 3 reported entering\n" +
                        "          the colony on falsified Class A mandates. All use same ID.\n\n" +
                        "> [08:00] Unknown: ...the cure and the plague walk side by side...";

                case 7:
                    return
                        "> [06:00] Command HQ: Final verification day before tribunal.\n" +
                        "          Agent Grey is monitoring ALL inspector decisions.\n\n" +
                        "> [06:30] Colony News: Nina Worth arrested early this morning.\n" +
                        "          Charged with document falsification and rebel contact.\n" +
                        "          Her associate Zoya Lann remains at large.\n\n" +
                        "> [07:00] Security: Airlock 9 flagged. Do not allow access.\n" +
                        "          Any subject mentioning airlock 9 — detain immediately.\n\n" +
                        "> [07:45] Medical Bay: B-7 containment holding — barely.\n" +
                        "          Hasan's partial formula unverified. Risky.\n\n" +
                        "> [08:15] [GHOST SIGNAL — unverified]: Mirra says tonight is the last\n" +
                        "          chance. Three seats on the ship. One for you, if you choose.";

                case 8:
                    return
                        "> [06:00] Command HQ: Maximum security protocol active.\n" +
                        "          Tribunal convenes at 18:00. Your record will be reviewed.\n\n" +
                        "> [06:20] Colony News: Three sectors under hard lockdown.\n" +
                        "          Serv-Legion units confirmed hostile — open fire authorised.\n\n" +
                        "> [07:00] Security: Rebel network partially dismantled.\n" +
                        "          One escape attempt thwarted at airlock 9. Suspects fled.\n\n" +
                        "> [07:30] Medical Bay: B-7 spreading. 34 confirmed. 9 critical.\n" +
                        "          Without antidote — full quarantine in 48 hours.\n\n" +
                        "> [08:00] Agent Grey [direct]: We know about the note.\n" +
                        "          Make the right choice today, Inspector.";

                case 9:
                    return
                        "> [06:00] Command HQ: PENULTIMATE DAY. Quota at maximum.\n" +
                        "          All errors will be reported to Commissar Wolf directly.\n\n" +
                        "> [06:30] Colony News: Curfew imposed colony-wide after 20:00.\n" +
                        "          Civilian unrest growing. Two protests broken up overnight.\n\n" +
                        "> [07:00] Security: Villain unit still unaccounted for.\n" +
                        "          Last sighting: Gate inspection area. Stay alert.\n\n" +
                        "> [07:20] Medical Bay: Hasan formula test results — inconclusive.\n" +
                        "          He has 24 hours to complete the antidote or colony seals.\n\n" +
                        "> [07:55] [GHOST SIGNAL]: Tomorrow is the last day.\n" +
                        "          The ship leaves at 03:00 regardless. With or without you.";

                case 10:
                    return
                        "> [06:00] Command HQ: FINAL DAY. Your shift record decides your fate.\n\n" +
                        "> [06:15] Colony News: Commissar Wolf's tribunal convenes at 20:00.\n" +
                        "          Inspector's decisions under full review.\n\n" +
                        "> [06:40] Security: Serv-Legion breach confirmed. Three units inside.\n" +
                        "          Colony control systems compromised. Acting on last orders.\n\n" +
                        "> [07:10] Medical Bay: Antidote synthesis — 80% complete.\n" +
                        "          If the lab holds, there is hope.\n\n" +
                        "> [07:45] Agent Grey [open channel]: The Inspector's choices\n" +
                        "          shaped what happens today. All of it.\n\n" +
                        "> [08:00] [FINAL GHOST SIGNAL]: Airlock 9. 03:00.\n" +
                        "          The stars don't care about your papers.";

                default:
                    return
                        "> [06:00] Command HQ: Standard shift protocol.\n\n" +
                        "> [06:30] Security: All units on standby.\n\n" +
                        "> [07:00] Colony News: No major incidents reported.";
            }
        }
    }
}

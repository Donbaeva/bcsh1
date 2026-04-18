using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper
{
    public class CharacterDoc
    {
        public string DocName { get; set; }
        public string DocOrigin { get; set; }
        public string DocPurpose { get; set; }
        public string DocOccupation { get; set; }
        public string DocFamily { get; set; }
        public string DocDestination { get; set; }
        public string DocAccessCode { get; set; }
        public string DocNationality { get; set; }
        public List<string> Amendments { get; } = new List<string>();
    }

    public partial class Form1
    {
        private CharacterDoc _currentDoc;
        private readonly HashSet<string> _askedQuestions = new HashSet<string>();

        // ═══════════════════════════════════════════════════════════
        //  ГЕНЕРАЦИЯ ДОКУМЕНТОВ
        // ═══════════════════════════════════════════════════════════
        internal void GenerateDocForCurrentCharacter()
        {
            if (currentCharacterData == null) return;
            _askedQuestions.Clear();
            var c = currentCharacterData;
            var rnd = new Random(c.Name.GetHashCode());
            _currentDoc = new CharacterDoc
            {
                DocName = c.Name,
                DocOrigin = DocOriginFor(c),
                DocPurpose = c.ReasonToEnter ?? "Work",
                DocOccupation = c.Occupation ?? "—",
                DocFamily = DocFamilyFor(c),
                DocDestination = DocDestinationFor(rnd),
                DocAccessCode = c.AccessCode ?? "—",
                DocNationality = DocNationalityFor(c),
            };
        }

        private string DocOriginFor(Character c)
        {
            string[] o = { "Residential Zone B","North District","Industrial Block 4",
                           "Colony West Wing","Outer Rim Settlement","Central Hub",
                           "Sector C — Lower","Port Facility 7" };
            return o[new Random(c.Name.GetHashCode()).Next(o.Length)];
        }
        private string DocFamilyFor(Character c)
        {
            string[] f = { "Single","Married, no children","Married, 2 children",
                           "Widowed","Registered partner","Single parent — 1 child" };
            return f[new Random(c.Name.GetHashCode() + 1).Next(f.Length)];
        }
        private string DocDestinationFor(Random rnd)
        {
            string[] d = { "Gate B — Work Sector","Medical Bay Level 3","Research Wing",
                           "Commerce Hub","Residential Block East","Administrative Floor 2",
                           "Engineering Bay 5","Docking Zone Alpha" };
            return d[rnd.Next(d.Length)];
        }
        private string DocNationalityFor(Character c)
        {
            if (c is Character.Alien) return "Non-human — Registered Visitor";
            if (c is Character.Robot) return "Synthetic Unit — Registered";
            string[] n = { "Colony Citizen — Sector A","Colony Citizen — Sector B",
                           "Colony Citizen — Sector C","Outer Settlement Resident",
                           "Visiting Worker — Permit Class 2" };
            return n[new Random(c.Name.GetHashCode() + 2).Next(n.Length)];
        }

        // ═══════════════════════════════════════════════════════════
        //  ПЛАВАЮЩЕЕ ОКНО ДОКУМЕНТОВ (index==7 в ShowOverlay)
        // ═══════════════════════════════════════════════════════════
        internal void ShowFloatingDocument()
        {
            if (_currentDoc == null)
            {
                StartTypingEffect("No subject on file. Documents appear when a subject arrives.");
                return;
            }
            foreach (Form f in Application.OpenForms)
                if (!(f is Form1) && f.Text == "SUBJECT PROFILE" && !f.IsDisposed)
                { f.BringToFront(); return; }

            var doc = new Form
            {
                Text = "SUBJECT PROFILE",
                Size = new Size(290, 400),
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(10, 13, 20),
                TopMost = true,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
            };
            var zone = ScaleRect(zoneRightScreen);
            doc.Location = new Point(zone.Left, zone.Bottom + 6);

            doc.Paint += (s, pe) => {
                var g = pe.Graphics;
                using (var p = new Pen(Color.FromArgb(160, 51, 102, 200), 1))
                    g.DrawRectangle(p, 0, 0, doc.Width - 1, doc.Height - 1);
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, doc.Width, 3),
                    Color.FromArgb(180, 51, 130, 220), Color.Transparent,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    g.FillRectangle(br, 0, 0, doc.Width, 3);
            };

            bool dragging = false; Point dragOffset = Point.Empty;
            var header = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(15, 25, 40) };
            var lblT = new Label
            {
                Text = "IDENTITY DOCUMENT",
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 160, 230),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            var btnX = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 80, 80),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => doc.Close();
            header.Controls.Add(lblT); header.Controls.Add(btnX);

            void AttachDrag(Control ctrl)
            {
                ctrl.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { dragging = true; dragOffset = e.Location; } };
                ctrl.MouseMove += (s, e) => { if (dragging) doc.Location = new Point(doc.Left + e.X - dragOffset.X, doc.Top + e.Y - dragOffset.Y); };
                ctrl.MouseUp += (s, e) => dragging = false;
            }
            AttachDrag(header); AttachDrag(lblT);

            var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(10, 6, 10, 6) };
            int by = 6;
            FloatField(body, "NAME", _currentDoc.DocName, ref by);
            FloatField(body, "ORIGIN", _currentDoc.DocOrigin, ref by);
            FloatField(body, "OCCUPATION", _currentDoc.DocOccupation, ref by);
            FloatField(body, "PURPOSE", _currentDoc.DocPurpose, ref by);
            FloatField(body, "DESTINATION", _currentDoc.DocDestination, ref by);
            FloatField(body, "FAMILY", _currentDoc.DocFamily, ref by);
            FloatField(body, "ACCESS CODE", _currentDoc.DocAccessCode, ref by);
            FloatField(body, "CITIZENSHIP", _currentDoc.DocNationality, ref by);

            if (_currentDoc.Amendments.Count > 0)
            {
                by += 4;
                body.Controls.Add(new Label { Location = new Point(8, by), Size = new Size(260, 1), BackColor = Color.FromArgb(80, 200, 160, 40) });
                by += 8;
                body.Controls.Add(new Label
                {
                    Text = "── AMENDMENTS",
                    Location = new Point(8, by),
                    Size = new Size(260, 14),
                    AutoSize = false,
                    ForeColor = Color.FromArgb(200, 160, 40),
                    Font = new Font("Consolas", 7f, FontStyle.Bold)
                });
                by += 18;
                foreach (var am in _currentDoc.Amendments)
                {
                    body.Controls.Add(new Label
                    {
                        Text = "• " + am,
                        Location = new Point(8, by),
                        Size = new Size(260, 28),
                        AutoSize = false,
                        ForeColor = Color.FromArgb(220, 180, 80),
                        Font = new Font("Consolas", 7.5f)
                    });
                    by += 28;
                }
            }
            doc.Controls.Add(body);
            doc.Controls.Add(header);
            doc.Show(this);
        }

        private void FloatField(Panel p, string label, string value, ref int y)
        {
            p.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(8, y),
                Size = new Size(260, 11),
                AutoSize = false,
                ForeColor = Color.FromArgb(70, 100, 150),
                Font = new Font("Consolas", 6.5f, FontStyle.Bold)
            });
            p.Controls.Add(new Label
            {
                Text = value ?? "—",
                Location = new Point(8, y + 11),
                Size = new Size(260, 15),
                AutoSize = false,
                ForeColor = Color.FromArgb(200, 215, 230),
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            });
            y += 32;
        }

        // ═══════════════════════════════════════════════════════════
        //  ПАНЕЛЬ ДОПРОСА — только вопросы
        // ═══════════════════════════════════════════════════════════
        internal void ShowInterrogationPanel()
        {
            if (currentCharacterData == null) return;
            if (_currentDoc == null) GenerateDocForCurrentCharacter();

            var panel = new Form
            {
                Text = "INTERROGATION",
                Size = new Size(310, 520),
                BackColor = Color.FromArgb(8, 11, 16),
                ForeColor = Color.FromArgb(200, 220, 240),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                Font = new Font("Consolas", 9)
            };
            panel.Paint += (s, pe) => {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(80, 51, 102, 170), 1))
                    g.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                using (var pen = new Pen(Color.FromArgb(160, 51, 130, 200), 2))
                {
                    int a = 14;
                    g.DrawLine(pen, 0, 0, a, 0); g.DrawLine(pen, 0, 0, 0, a);
                    g.DrawLine(pen, panel.Width - a, 0, panel.Width, 0);
                    g.DrawLine(pen, panel.Width, 0, panel.Width, a);
                    g.DrawLine(pen, 0, panel.Height - a, 0, panel.Height);
                    g.DrawLine(pen, 0, panel.Height, a, panel.Height);
                }
            };

            panel.Controls.Add(new Label
            {
                Text = "INTERROGATION // SELECT QUESTION",
                Location = new Point(16, 14),
                Size = new Size(278, 20),
                ForeColor = Color.FromArgb(100, 170, 240),
                Font = new Font("Consolas", 9, FontStyle.Bold)
            });
            panel.Controls.Add(new Label
            {
                Location = new Point(16, 36),
                Size = new Size(278, 1),
                BackColor = Color.FromArgb(40, 51, 102, 170)
            });

            var (questions, keys) = BuildQuestionList();
            int qy = 44;
            for (int i = 0; i < questions.Count; i++)
            {
                string q = questions[i], key = keys[i];
                bool already = _askedQuestions.Contains(key);
                var btn = new Button
                {
                    Text = q,
                    Location = new Point(16, qy),
                    Size = new Size(278, 34),
                    BackColor = already ? Color.FromArgb(20, 30, 20) : Color.FromArgb(22, 40, 58),
                    ForeColor = already ? Color.FromArgb(80, 100, 80) : Color.FromArgb(180, 210, 240),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 8, already ? FontStyle.Italic : FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    Tag = key
                };
                btn.FlatAppearance.BorderColor = already ? Color.FromArgb(30, 60, 30) : Color.FromArgb(40, 80, 130);
                btn.Click += (s, e) => { string k = (string)((Button)s).Tag; panel.Close(); HandleInterrogationQuestion(k); };
                panel.Controls.Add(btn);
                qy += 40;
            }

            var btnClose = new Button
            {
                Text = "✕  CLOSE",
                Location = new Point(16, panel.Height - 50),
                Size = new Size(278, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.FromArgb(100, 100, 120),
                Font = new Font("Consolas", 8),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(40, 40, 60);
            btnClose.Click += (s, e) => panel.Close();
            panel.Controls.Add(btnClose);
            panel.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) panel.Close(); };
            panel.ShowDialog(this);
        }

        private (List<string>, List<string>) BuildQuestionList()
        {
            var q = new List<string>(); var k = new List<string>();
            q.Add("What is your name?"); k.Add("name");
            q.Add("Where are you coming from?"); k.Add("origin");
            q.Add("What is your occupation?"); k.Add("occupation");
            q.Add("What is your purpose here?"); k.Add("purpose");
            q.Add("Where are you heading?"); k.Add("destination");
            q.Add("Do you have family?"); k.Add("family");
            q.Add("What is your access code?"); k.Add("code");
            q.Add("What is your citizenship?"); k.Add("nationality");
            q.Add("How are you feeling today?"); k.Add("feeling");
            if (huntModeActive) { q.Add("[HUNT] Describe your biology."); k.Add("biological"); }
            return (q, k);
        }

        // ═══════════════════════════════════════════════════════════
        //  ОБРАБОТКА ВОПРОСА
        // ═══════════════════════════════════════════════════════════
        private void HandleInterrogationQuestion(string key)
        {
            if (currentCharacterData == null || _currentDoc == null) return;
            if (_askedQuestions.Contains(key))
            {
                string[] annoyed ={ "I already answered that. Check your log.",
                    "We've been over this. Look at your records.",
                    "Are you serious? I told you already.",
                    "I answered that. Do you not take notes?",
                    "Again? It's in your log." };
                string ir = annoyed[new Random().Next(annoyed.Length)];
                AddToDialogueLog(currentCharacterData.Name, "[REPEATED] " + ir);
                StartTypingEffect(ir);
                return;
            }
            _askedQuestions.Add(key);
            string question = KeyToQuestion(key);
            string spoken = GetAnswerByKey(currentCharacterData, key);
            string docVal = GetDocValue(key);
            bool mismatch = HasMismatch(key, spoken, docVal);
            AddToDialogueLog("INSPECTOR", question);
            AddToDialogueLog(currentCharacterData.Name, spoken);
            StartTypingEffect(spoken);
            if (mismatch) ShowMismatchDialog(key, question, spoken, docVal);
        }

        private string KeyToQuestion(string key)
        {
            switch (key)
            {
                case "name": return "What is your name?";
                case "origin": return "Where are you coming from?";
                case "occupation": return "What is your occupation?";
                case "purpose": return "What is your purpose here?";
                case "destination": return "Where are you heading?";
                case "family": return "Do you have family?";
                case "code": return "What is your access code?";
                case "nationality": return "What is your citizenship?";
                case "feeling": return "How are you feeling today?";
                case "biological": return "Describe your biological composition.";
                default: return key;
            }
        }

        private string GetDocValue(string key)
        {
            if (_currentDoc == null) return "";
            switch (key)
            {
                case "name": return _currentDoc.DocName;
                case "origin": return _currentDoc.DocOrigin;
                case "occupation": return _currentDoc.DocOccupation;
                case "purpose": return _currentDoc.DocPurpose;
                case "destination": return _currentDoc.DocDestination;
                case "family": return _currentDoc.DocFamily;
                case "code": return _currentDoc.DocAccessCode;
                case "nationality": return _currentDoc.DocNationality;
                default: return "";
            }
        }

        private bool HasMismatch(string key, string spoken, string docVal)
        {
            switch (key)
            {
                case "code":
                    return !spoken.Contains(docVal ?? "") && docVal != "—";
                case "name":
                    if (currentCharacterData == null) return false;
                    foreach (var p in currentCharacterData.Name.Split(' '))
                        if (spoken.Contains(p)) return false;
                    return (currentCharacterData is Character.Robot || currentCharacterData is Character.Alien)
                          && currentCharacterData.Day <= 3;
                default:
                    bool syn = currentCharacterData is Character.Robot || currentCharacterData is Character.Alien;
                    return syn && (currentCharacterData?.Day <= 4) && new Random().Next(0, 4) == 0;
            }
        }

        private void ShowMismatchDialog(string key, string question, string spoken, string docVal)
        {
            var dlg = new Form
            {
                Size = new Size(500, 280),
                BackColor = Color.FromArgb(16, 8, 8),
                ForeColor = Color.FromArgb(220, 200, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                Font = new Font("Consolas", 9)
            };
            dlg.Paint += (s, pe) => {
                using (var pen = new Pen(Color.FromArgb(150, 180, 40, 40), 1))
                    pe.Graphics.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
            };
            dlg.Controls.Add(new Label
            {
                Text = $"⚠  DISCREPANCY — {key.ToUpper()}\n\nDocument says:  \"{docVal}\"\nSubject said:   \"{spoken.Substring(0, Math.Min(spoken.Length, 55))}...\"\n\nHow do you want to proceed?",
                Location = new Point(20, 16),
                Size = new Size(460, 110),
                ForeColor = Color.FromArgb(220, 180, 140),
                Font = new Font("Consolas", 9)
            });
            var btnC = new Button
            {
                Text = "CONFRONT — \"Why does your document say something different?\"",
                Location = new Point(20, 135),
                Size = new Size(460, 40),
                BackColor = Color.FromArgb(50, 20, 20),
                ForeColor = Color.FromArgb(220, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 8),
                Cursor = Cursors.Hand
            };
            btnC.FlatAppearance.BorderColor = Color.FromArgb(120, 60, 60);
            btnC.Click += (s, e) => { dlg.Close(); HandleConfront(key, docVal); };
            dlg.Controls.Add(btnC);
            var btnI = new Button
            {
                Text = "IGNORE — Continue without confronting",
                Location = new Point(20, 185),
                Size = new Size(460, 34),
                BackColor = Color.FromArgb(20, 28, 22),
                ForeColor = Color.FromArgb(100, 150, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 8),
                Cursor = Cursors.Hand
            };
            btnI.FlatAppearance.BorderColor = Color.FromArgb(40, 80, 40);
            btnI.Click += (s, e) => dlg.Close();
            dlg.Controls.Add(btnI);
            dlg.ShowDialog(this);
        }

        private void HandleConfront(string key, string docVal)
        {
            if (currentCharacterData == null) return;
            var rnd = new Random();
            bool isR = currentCharacterData is Character.Robot;
            bool isA = currentCharacterData is Character.Alien;
            string name = currentCharacterData.Name;
            string response; bool accepted = false;

            if (key == "name")
            {
                if (isR || isA)
                {
                    string[] e ={"I... go by a different name informally. The document is official.",
                        "It's a nickname. I should have clarified.","Registration error. The document is correct."};
                    response = e[rnd.Next(e.Length)];
                }
                else
                {
                    string[] h ={"I recently changed my name. The amendment paperwork is here.",
                        "I go by my middle name. Legal name is on the document.",
                        "I was married recently — still adjusting to the new name."};
                    response = h[rnd.Next(h.Length)];
                    accepted = true; _currentDoc.Amendments.Add($"Name: uses \"{name}\" informally");
                }
            }
            else if (key == "code")
            {
                string[] e ={"That's the old code — I memorised the wrong one.",
                    "Updated this morning, I didn't get the memo.",$"Here — it's {docVal}."};
                response = e[rnd.Next(e.Length)];
                if (!isR && !isA) { accepted = true; _currentDoc.Amendments.Add($"Code corrected: {docVal}"); }
            }
            else if (key == "origin")
            {
                string[] e ={"I meant where I started today, not my registered address.",
                    "I was passing through the transit zone — that's what I meant.",
                    "Address changed recently. The document shows my official origin."};
                response = e[rnd.Next(e.Length)];
                if (!isR && !isA) { accepted = true; _currentDoc.Amendments.Add($"Origin clarified: {docVal}"); }
            }
            else
            {
                string[] g ={"I may have misspoken. The document is correct.",
                    "That was a slip of the tongue. The record is accurate.",
                    "I was nervous. The document has the right information."};
                response = g[rnd.Next(g.Length)];
                if (!isR && !isA) { accepted = true; _currentDoc.Amendments.Add($"{key}: clarified"); }
            }

            AddToDialogueLog("INSPECTOR", "[CONFRONTED] Why does your document say something different?");
            AddToDialogueLog(name, response);
            StartTypingEffect(response);

            if (accepted)
            {
                string am = _currentDoc.Amendments[_currentDoc.Amendments.Count - 1];
                var notify = new Form
                {
                    Size = new Size(400, 120),
                    BackColor = Color.FromArgb(8, 20, 10),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.None
                };
                notify.Paint += (s, pe) => {
                    using (var pen = new Pen(Color.FromArgb(80, 0, 140, 60), 1))
                        pe.Graphics.DrawRectangle(pen, 0, 0, notify.Width - 1, notify.Height - 1);
                };
                notify.Controls.Add(new Label
                {
                    Text = $"📋 Amendment added:\n\"{am}\"",
                    Location = new Point(16, 18),
                    Size = new Size(368, 48),
                    ForeColor = Color.FromArgb(160, 220, 140),
                    Font = new Font("Consolas", 9)
                });
                var ok = new Button
                {
                    Text = "OK",
                    Location = new Point(150, 74),
                    Size = new Size(100, 28),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(100, 200, 100),
                    BackColor = Color.FromArgb(15, 40, 15),
                    Font = new Font("Consolas", 9)
                };
                ok.FlatAppearance.BorderColor = Color.FromArgb(40, 100, 40);
                ok.Click += (s, e) => notify.Close();
                notify.Controls.Add(ok);
                notify.ShowDialog(this);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  GetAnswerByKey
        // ═══════════════════════════════════════════════════════════
        private string GetAnswerByKey(Character c, string key)
        {
            switch (key)
            {
                case "name": return NameAnswer(c);
                case "origin": return CharacterAI.GenerateAnswer(c, "Where are you coming from?");
                case "occupation": return OccupationAnswer(c);
                case "purpose": return CharacterAI.GenerateAnswer(c, "What is your purpose here?");
                case "destination": return DestinationAnswer(c);
                case "family": return CharacterAI.GenerateAnswer(c, "Do you have family?");
                case "code": return CharacterAI.GenerateAnswer(c, "What is your access code?");
                case "nationality": return NationalityAnswer(c);
                case "feeling": return CharacterAI.GenerateAnswer(c, "How do you feel today?");
                case "biological": return CharacterAI.GenerateAnswer(c, "Describe your biological composition.");
                default: return CharacterAI.GenerateAnswer(c, key);
            }
        }

        private string NameAnswer(Character c)
        {
            var r = new Random();
            if (c is Character.Robot && c.Day <= 2)
            {
                string[] a = { $"My designation is {c.Name}.", $"I am registered as {c.Name}.", $"{c.Name}. It is... my name." };
                return a[r.Next(a.Length)];
            }
            if (c is Character.Alien && c.Day <= 3)
            {
                string[] a = { $"My name is {c.Name}. Registered under this name.", $"{c.Name}. It is on my papers.", $"We — I am called {c.Name}." };
                return a[r.Next(a.Length)];
            }
            string[] h = { $"{c.Name}. Is there a problem?", $"It's {c.Name}. Same as on the card.", $"My name? {c.Name}.", $"{c.Name}. Check the documents." };
            return h[r.Next(h.Length)];
        }

        private string OccupationAnswer(Character c)
        {
            var r = new Random(); string occ = c.Occupation ?? "general work";
            if (c is Character.Robot && c.Day <= 2) return $"I am assigned to {occ}. It is in my operational parameters.";
            string[] opts ={$"I work as {occ}.",$"{occ}. It's on the permit.",$"My job is {occ}.",$"I'm a {occ}."};
            return opts[r.Next(opts.Length)];
        }

        private string DestinationAnswer(Character c)
        {
            var r = new Random(); string d = _currentDoc?.DocDestination ?? c.ReasonToEnter ?? "my workstation";
            if (c is Character.Robot && c.Day <= 2) return $"Destination: {d}. Route is authorised.";
            string[] opts = { $"I'm heading to {d}.", $"My destination is {d}.", $"Going to {d} — I'm late.", $"{d}. Same as every day." };
            return opts[r.Next(opts.Length)];
        }

        private string NationalityAnswer(Character c)
        {
            var r = new Random();
            if (c is Character.Alien && c.Day <= 4)
            {
                string[] a ={"I am a registered visitor. All documentation is valid.",
                    "Non-human, registered. The permit is current.",
                    "Visitor status, properly registered."};
                return a[r.Next(a.Length)];
            }
            if (c is Character.Robot) return "Synthetic unit, registered and licensed.";
            string[] h = { "Colony citizen. Born and raised here.", "I'm a registered resident. Everything is in order.", "Colony citizen. It's on the document." };
            return h[r.Next(h.Length)];
        }
    }
}
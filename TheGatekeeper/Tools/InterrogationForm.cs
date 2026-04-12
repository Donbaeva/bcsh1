using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using TheGatekeeper.Models;

namespace TheGatekeeper.Tools
{
    public class InterrogationForm : Form
    {
        private Character character;
        private ComboBox cmbQuestions;
        private TextBox txtAnswer;
        private Button btnAsk;
        private Button btnClose;
        private Label lblCharacterName;
        private Label lblHint;
        private SoundPlayer soundPlayer;

        public InterrogationForm(Character character)
        {
            this.character = character;
            InitializeForm();
            LoadQuestions();
        }

        private void InitializeForm()
        {
            this.Text = "🎙️ INTERROGATION";
            this.Size = new Size(550, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 45);

            lblCharacterName = new Label
            {
                Text = $"INTERROGATION: {character.Name}",
                Location = new Point(20, 20),
                Size = new Size(510, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = GetColorByType(),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblHint = new Label
            {
                Text = "💡 Listen carefully. Humans show emotions, Synthetics have patterns.",
                Location = new Point(20, 65),
                Size = new Size(510, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            cmbQuestions = new ComboBox
            {
                Location = new Point(20, 140),
                Size = new Size(510, 30),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White
            };

            btnAsk = new Button
            {
                Text = "❓ ASK QUESTION",
                Location = new Point(20, 180),
                Size = new Size(510, 40),
                BackColor = Color.FromArgb(0, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnAsk.Click += BtnAsk_Click;

            txtAnswer = new TextBox
            {
                Location = new Point(20, 270),
                Size = new Size(510, 120),
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(25, 25, 40),
                ForeColor = Color.LightGreen,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(200, 420),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(70, 70, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblCharacterName);
            this.Controls.Add(lblHint);
            this.Controls.Add(cmbQuestions);
            this.Controls.Add(btnAsk);
            this.Controls.Add(txtAnswer);
            this.Controls.Add(btnClose);
        }

        private Color GetColorByType()
        {
            // Используем Species из твоего класса Character
            if (character.Species == "Human") return Color.LightGreen;
            if (character.Species == "Robot") return Color.Tomato;
            return Color.MediumPurple;
        }

        private void LoadQuestions()
        {
            cmbQuestions.Items.Add("What is your purpose of visit?");
            cmbQuestions.Items.Add("Tell me about your background.");
            cmbQuestions.Items.Add("Are you a synthetic entity?");
            cmbQuestions.SelectedIndex = 0;
        }

        private void BtnAsk_Click(object sender, EventArgs e)
        {
            string question = cmbQuestions.SelectedItem.ToString();
            string answer = "";
            string analysis = "";

            // Логика ответов на основе данных персонажа
            if (question.Contains("purpose"))
            {
                answer = character.ReasonToEnter;
            }
            else if (question.Contains("background"))
            {
                answer = $"I am a {character.Occupation} working on day {character.Day}.";
            }
            else
            {
                answer = character.Dialogue; // Используем общую фразу
            }

            // Анализ
            if (character.Species == "Robot")
                analysis = "\n\n🤖 [ANALYSIS: Delay in response 0.4ms. Rhythmic pattern detected.]";
            else if (character.Species == "Alien")
                analysis = "\n\n👽 [ANALYSIS: Harmonic distortion in vocal cords. Non-terrestrial origin.]";
            else
                analysis = "\n\n✅ [ANALYSIS: Bio-signature confirmed. Emotional stress detected.]";

            txtAnswer.Text = $"Q: {question}\n\nA: {answer}{analysis}";

            // Звук (только если файлы .wav!)
            PlaySound("talk.wav");
        }

        private void PlaySound(string fileName)
        {
            try
            {
                string path = System.IO.Path.Combine(Application.StartupPath, "Audio", fileName);
                if (System.IO.File.Exists(path))
                {
                    soundPlayer?.Stop();
                    soundPlayer = new SoundPlayer(path);
                    soundPlayer.Play();
                }
            }
            catch { }
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace TheGatekeeper
{
    public partial class DaySummaryForm : Form
    {
        public bool ContinueToNextDay { get; private set; } = false;

        public DaySummaryForm(int day, int score, int health, int checkedCount, int quota,
                              List<(Models.Character Char, string Decision)> decisions)
        {
            this.Text = $"DAY {day} REPORT";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(10, 12, 18);
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
                {
                    ContinueToNextDay = true;
                    this.Close();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    ContinueToNextDay = false;
                    this.Close();
                }
            };

            // Основная панель с рамкой в стиле игры
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            mainPanel.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                using (var pen = new Pen(Color.FromArgb(180, 51, 102, 170), 2))
                    g.DrawRectangle(pen, 10, 10, mainPanel.Width - 20, mainPanel.Height - 20);
            };
            this.Controls.Add(mainPanel);

            // Заголовок
            Label title = new Label
            {
                Text = $"═══ DAY {day} COMPLETE ═══",
                Font = new Font("Consolas", 18, FontStyle.Bold),
                ForeColor = Color.Cyan,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            mainPanel.Controls.Add(title);

            // Статистика
            Label stats = new Label
            {
                Text = $"SCORE: {score} pts\n" +
                       $"HEALTH: {health}/3\n" +
                       $"SUBJECTS CHECKED: {checkedCount}/{quota}",
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.LightGray,
                Location = new Point(20, 70),
                AutoSize = true
            };
            mainPanel.Controls.Add(stats);

            // Разделитель
            Label divider = new Label
            {
                Text = new string('─', 60),
                Font = new Font("Consolas", 10),
                ForeColor = Color.FromArgb(51, 102, 170),
                Location = new Point(20, 140),
                AutoSize = true
            };
            mainPanel.Controls.Add(divider);

            // Список решений
            Label decisionsLabel = new Label
            {
                Text = "YOUR DECISIONS:",
                Font = new Font("Consolas", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(20, 170),
                AutoSize = true
            };
            mainPanel.Controls.Add(decisionsLabel);

            int yPos = 200;
            foreach (var tuple in decisions)
            {
                var character = tuple.Item1;
                var decision = tuple.Item2;

                Color decisionColor;
                switch (decision)
                {
                    case "ROBOT": decisionColor = Color.Red; break;
                    case "ALIEN": decisionColor = Color.DodgerBlue; break;
                    case "HUMAN": decisionColor = Color.Lime; break;
                    default: decisionColor = Color.Gray; break;
                }
   

            string actualType = character.Species;

                Label entry = new Label
                {
                    Text = $"{character.Name,-20}  →  {decision,-6}  (actual: {actualType})",
                    Font = new Font("Consolas", 10),
                    ForeColor = decisionColor,
                    Location = new Point(40, yPos),
                    AutoSize = true
                };
                mainPanel.Controls.Add(entry);
                yPos += 25;
            }

            // Подсказка
            Label hint = new Label
            {
                Text = "\nPress SPACE or ENTER to continue\nPress ESC to exit to menu",
                Font = new Font("Consolas", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 150),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 60
            };
            mainPanel.Controls.Add(hint);
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheGatekeeper
{
    public class ChecklistForm : Form
    {
        public ChecklistForm()
        {
            this.Size = new Size(250, 350);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 230, 140); // Цвет стикера (Khaki)
            this.StartPosition = FormStartPosition.CenterParent;

            Label title = new Label
            {
                Text = "SUBJECT VERIFICATION",
                Font = new Font("Consolas", 10, FontStyle.Bold),
                Bounds = new Rectangle(10, 10, 230, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            string[] checks = {
                "Access Code Match",
                "Natural Speech",
                "Normal Body Temp",
                "Human Pronouns (I/Me)",
                "Emotional Response"
            };

            int y = 40;
            foreach (var text in checks)
            {
                CheckBox cb = new CheckBox
                {
                    Text = text,
                    Bounds = new Rectangle(20, y, 210, 25),
                    Font = new Font("Consolas", 9),
                    FlatStyle = FlatStyle.Flat
                };
                this.Controls.Add(cb);
                y += 30;
            }

            Button btnClose = new Button
            {
                Text = "OK",
                Bounds = new Rectangle(85, 290, 80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 190, 100)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(title);
            this.Controls.Add(btnClose);

            // Рисуем рамочку
            this.Paint += (s, e) => e.Graphics.DrawRectangle(Pens.DarkGoldenrod, 0, 0, Width - 1, Height - 1);
        }
    }
}
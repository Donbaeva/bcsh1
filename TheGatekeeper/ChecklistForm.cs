using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheGatekeeper
{
    public class ChecklistForm : Form
    {
        private ProgressBar pbSuspicion;
        private Label lblVerdict;

        public ChecklistForm()
        {
            this.Text = "FIELD VERIFICATION";
            this.Size = new Size(350, 450);
            this.BackColor = Color.FromArgb(30, 35, 40);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            int y = 20;
            string[] checks = {
                "Access Code Mismatch",
                "Abnormal Temperature",
                "Irregular Pulse",
                "Speech Anomaly (We/Us)",
                "Synthetic Eye Reflection"
            };

            foreach (var text in checks)
            {
                CheckBox cb = new CheckBox
                {
                    Text = text,
                    Location = new Point(20, y),
                    Size = new Size(280, 30),
                    Font = new Font("Consolas", 10f)
                };
                cb.CheckedChanged += UpdateSuspicion;
                this.Controls.Add(cb);
                y += 40;
            }

            // Шкала вероятности
            Label lblProb = new Label { Text = "THREAT PROBABILITY:", Location = new Point(20, y + 20), Size = new Size(300, 20) };
            pbSuspicion = new ProgressBar { Location = new Point(20, y + 45), Size = new Size(290, 20), Maximum = 100 };
            lblVerdict = new Label
            {
                Text = "STATUS: CLEAR",
                Location = new Point(20, y + 75),
                Size = new Size(300, 30),
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.Lime
            };

            this.Controls.AddRange(new Control[] { lblProb, pbSuspicion, lblVerdict });
        }

        private void UpdateSuspicion(object sender, EventArgs e)
        {
            int checkedCount = 0;
            foreach (Control c in this.Controls)
                if (c is CheckBox cb && cb.Checked) checkedCount++;

            int probability = checkedCount * 20; // 5 пунктов по 20%
            pbSuspicion.Value = probability;

            if (probability == 0) { lblVerdict.Text = "STATUS: CLEAR"; lblVerdict.ForeColor = Color.Lime; }
            else if (probability <= 40) { lblVerdict.Text = "STATUS: SUSPICIOUS"; lblVerdict.ForeColor = Color.Yellow; }
            else { lblVerdict.Text = "STATUS: HIGH THREAT"; lblVerdict.ForeColor = Color.Red; }
        }
    }
}
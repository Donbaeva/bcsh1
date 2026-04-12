using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace TheGatekeeper.Tools
{
    public class PulseToolForm : Form
    {
        private Timer animationTimer;
        private int currentPulse;
        private Label lblPulseValue;
        private ProgressBar pulseBar;
        private Label lblResult;

        public PulseToolForm(int pulse, string characterType)
        {
            currentPulse = pulse;
            InitializeForm();
            ShowResult(pulse, characterType);
            AnimatePulse();
        }

        private void InitializeForm()
        {
            this.Text = "💓 Pulse Meter";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 45);

            Label lblTitle = new Label
            {
                Text = "PULSE MEASUREMENT",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Cyan,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblPulseValue = new Label
            {
                Location = new Point(20, 70),
                Size = new Size(360, 50),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "---"
            };

            pulseBar = new ProgressBar
            {
                Location = new Point(50, 140),
                Size = new Size(300, 30),
                Minimum = 0,
                Maximum = 200
            };

            lblResult = new Label
            {
                Location = new Point(20, 190),
                Size = new Size(360, 80),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGreen,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(140, 280),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 70, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblPulseValue);
            this.Controls.Add(pulseBar);
            this.Controls.Add(lblResult);
            this.Controls.Add(btnClose);
        }

        private void ShowResult(int pulse, string characterType)
        {
            if (characterType == "Robot")
            {
                lblResult.Text = "🤖 ROBOT DETECTED!\nNo heartbeat. Classification: non-human.";
                lblResult.ForeColor = Color.Red;
            }
            else if (pulse == 0)
            {
                lblResult.Text = "⚠️ NO HEARTBEAT!\nNot a normal human profile. Further inspection required.";
                lblResult.ForeColor = Color.Orange;
            }
            else if (pulse < 60)
            {
                lblResult.Text = "⚠️ LOW PULSE\nPossible medical condition.";
                lblResult.ForeColor = Color.Yellow;
            }
            else if (pulse > 100)
            {
                lblResult.Text = "⚠️ HIGH PULSE\nStress or non-human physiology?";
                lblResult.ForeColor = Color.Yellow;
            }
            else
            {
                lblResult.Text = "✅ PULSE NORMAL\nConsistent with human baseline.";
                lblResult.ForeColor = Color.LightGreen;
            }
        }

        private void AnimatePulse()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 50;
            int step = 0;

            animationTimer.Tick += (s, e) =>
            {
                if (currentPulse == 0)
                {
                    lblPulseValue.Text = "0 BPM";
                    lblPulseValue.ForeColor = Color.Red;
                    pulseBar.Value = 0;
                    animationTimer.Stop();
                }
                else
                {
                    int displayPulse = currentPulse + (step % 20) - 10;
                    displayPulse = Math.Max(0, Math.Min(200, displayPulse));
                    lblPulseValue.Text = $"{displayPulse} BPM";
                    pulseBar.Value = displayPulse;
                    step += 2;

                    if (displayPulse < 60) lblPulseValue.ForeColor = Color.Orange;
                    else if (displayPulse > 100) lblPulseValue.ForeColor = Color.Yellow;
                    else lblPulseValue.ForeColor = Color.LightGreen;
                }
            };
            animationTimer.Start();
        }
    }

    public class RadiationToolForm : Form
    {
        public RadiationToolForm(int radiation, string characterType)
        {
            this.Text = "📡 Radiation Detector";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 45);

            Label lblTitle = new Label
            {
                Text = "RADIATION MEASUREMENT",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblRadValue = new Label
            {
                Text = $"{radiation} mSv",
                Location = new Point(20, 70),
                Size = new Size(360, 50),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = characterType == "Alien" ? Color.Violet : (radiation > 20 ? Color.Red : Color.LightGreen),
                TextAlign = ContentAlignment.MiddleCenter
            };

            ProgressBar radBar = new ProgressBar
            {
                Location = new Point(50, 140),
                Size = new Size(300, 30),
                Minimum = 0,
                Maximum = 100,
                Value = Math.Min(100, radiation)
            };

            Label lblResult = new Label
            {
                Location = new Point(20, 190),
                Size = new Size(360, 80),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (characterType == "Alien")
            {
                lblResult.Text = "👽 EXTRATERRESTRIAL SIGNATURE DETECTED!\nSend to the lab.";
                lblResult.ForeColor = Color.Violet;
            }
            else if (radiation > 20)
            {
                lblResult.Text = "⚠️ CRITICAL RADIATION LEVEL!\nNon-standard human profile. Isolation recommended.";
                lblResult.ForeColor = Color.Red;
            }
            else if (radiation > 10)
            {
                lblResult.Text = "⚠️ ELEVATED RADIATION\nContamination or non-human origin?";
                lblResult.ForeColor = Color.Orange;
            }
            else
            {
                lblResult.Text = "✅ RADIATION NORMAL\nClear to proceed.";
                lblResult.ForeColor = Color.LightGreen;
            }

            Button btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(140, 280),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 70, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblRadValue);
            this.Controls.Add(radBar);
            this.Controls.Add(lblResult);
            this.Controls.Add(btnClose);
        }
    }

    public class VoiceToolForm : Form
    {
        public VoiceToolForm(string voicePattern, string characterType)
        {
            this.Text = "🎤 Voice Analyzer";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 45);

            Label lblTitle = new Label
            {
                Text = "VOICE ANALYSIS",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Cyan,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblPattern = new Label
            {
                Text = $"Pattern: {voicePattern}",
                Location = new Point(20, 80),
                Size = new Size(360, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblResult = new Label
            {
                Location = new Point(20, 150),
                Size = new Size(360, 60),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (characterType == "Robot")
            {
                lblResult.Text = "🤖 Synthetic harmonics detected.\nClassification: ROBOT.";
                lblResult.ForeColor = Color.Red;
            }
            else if (characterType == "Alien")
            {
                lblResult.Text = "👽 Unknown modulation signature.\nClassification: ALIEN.";
                lblResult.ForeColor = Color.Violet;
            }
            else
            {
                lblResult.Text = "✅ Natural modulation detected.\nLikely HUMAN.";
                lblResult.ForeColor = Color.LightGreen;
            }

            Button btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(140, 230),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 70, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblPattern);
            this.Controls.Add(lblResult);
            this.Controls.Add(btnClose);
        }
    }

    public class FingerprintToolForm : Form
    {
        public FingerprintToolForm(string fingerprint, string characterType)
        {
            this.Text = "🖐️ Fingerprint Scan";
            this.Size = new Size(450, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 45);

            Label lblTitle = new Label
            {
                Text = "FINGERPRINT SCAN",
                Location = new Point(20, 20),
                Size = new Size(410, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Cyan,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblFingerprint = new Label
            {
                Text = $"Type: {fingerprint}",
                Location = new Point(20, 70),
                Size = new Size(410, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblResult = new Label
            {
                Location = new Point(20, 120),
                Size = new Size(410, 100),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (characterType == "Robot")
            {
                lblResult.Text = "🤖 Artificial ridge patterns detected.\nClassification: ROBOT.";
                lblResult.ForeColor = Color.Red;
            }
            else if (characterType == "Alien")
            {
                lblResult.Text = "👽 No match in database.\nClassification: ALIEN.";
                lblResult.ForeColor = Color.Violet;
            }
            else
            {
                lblResult.Text = "✅ Match found in database.\nLikely HUMAN.";
                lblResult.ForeColor = Color.LightGreen;
            }

            Button btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(165, 300),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 70, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblFingerprint);
            this.Controls.Add(lblResult);
            this.Controls.Add(btnClose);
        }
    }
}
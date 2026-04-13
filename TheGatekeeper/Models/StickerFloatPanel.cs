using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheGatekeeper
{
    public class StickerFloatPanel : Form
    {
        private const int SW = 240;
        private const int SH = 200;
        private bool _dragging;
        private Point _dragOffset;
        private Form1 _owner;

        public StickerFloatPanel(string title, string content, Point startPos, Form1 owner)
        {
            _owner = owner;
            this.Text = title; // Используем для идентификации
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(SW, SH);
            this.BackColor = Color.FromArgb(255, 255, 140);
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(startPos.X - 20, startPos.Y - 20);

            // Заголовок
            Panel header = new Panel { Dock = DockStyle.Top, Height = 25, BackColor = Color.FromArgb(30, 0, 0, 0) };

            Label lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 70, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            // Кнопка закрытия (Крестик)
            Button btnClose = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 25,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 50, 30),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 0, 0);
            btnClose.Click += (s, e) => this.Close();

            header.Controls.Add(lblTitle);
            header.Controls.Add(btnClose);

            // Текстовое поле со скроллом
            RichTextBox rtb = new RichTextBox
            {
                Text = content,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 255, 140),
                ForeColor = Color.FromArgb(40, 30, 0),
                Font = new Font("Comic Sans MS", 9.5f),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Padding = new Padding(8)
            };

            // Перетаскивание за заголовок и текст
            AssignDrag(header);
            AssignDrag(lblTitle);
            AssignDrag(rtb);

            this.Controls.Add(rtb);
            this.Controls.Add(header);

            this.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(150, 150, 50), 2), 0, 0, Width - 1, Height - 1);
            };
        }

        private void AssignDrag(Control c)
        {
            c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { _dragging = true; _dragOffset = e.Location; } };
            c.MouseMove += (s, e) => { if (_dragging) this.Location = new Point(this.Left + e.X - _dragOffset.X, this.Top + e.Y - _dragOffset.Y); };
            c.MouseUp += (s, e) => _dragging = false;
        }
    }
}
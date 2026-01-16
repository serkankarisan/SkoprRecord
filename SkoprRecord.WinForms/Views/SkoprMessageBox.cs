using SkoprRecord.WinForms.Controls;

namespace SkoprRecord.WinForms.Views
{
    public partial class SkoprMessageBox : Form
    {
        private CustomTitleBar customTitleBar = null!;
        private Label lblMessage = null!;
        private Button btn1 = null!;
        private Button btn2 = null!;
        private Panel pnlButtons = null!;
        private PictureBox picIcon = null!;

        public SkoprMessageBox()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(1); // Border effect

            // Gerekirse kenarlık çizimi veya iç panel kullanımı buraya eklenebilir
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using (var msgBox = new SkoprMessageBox())
            {
                msgBox.customTitleBar.Title = caption;
                msgBox.lblMessage.Text = text;
                msgBox.SetupButtons(buttons);
                msgBox.SetupIcon(icon);
                return msgBox.ShowDialog();
            }
        }

        private void SetupButtons(MessageBoxButtons buttons)
        {
            // Butonları sıfırla
            btn1.Visible = false;
            btn2.Visible = false;

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    SetupButton(btn1, "Tamam", DialogResult.OK, true);
                    break;
                case MessageBoxButtons.YesNo:
                    SetupButton(btn1, "Evet", DialogResult.Yes, true);
                    SetupButton(btn2, "Hayır", DialogResult.No, false);
                    break;
                case MessageBoxButtons.OKCancel:
                    SetupButton(btn1, "Tamam", DialogResult.OK, true);
                    SetupButton(btn2, "İptal", DialogResult.Cancel, false);
                    break;
            }

            // Düzeni yeniden hesapla
            RecalculateButtonLayout();
        }

        private void RecalculateButtonLayout()
        {
            if (pnlButtons == null || btn1 == null || btn2 == null) return;

            int spacing = 15;
            int startY = 12;
            int panelWidth = pnlButtons.Width;

            if (btn2.Visible)
            {
                // İki butonlu düzen
                int totalWidth = btn1.Width + spacing + btn2.Width;
                int startX = (panelWidth - totalWidth) / 2;

                btn1.Location = new Point(startX, startY);
                btn2.Location = new Point(startX + btn1.Width + spacing, startY);
            }
            else
            {
                // Tek butonlu düzen
                btn1.Location = new Point((panelWidth - btn1.Width) / 2, startY);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            RecalculateButtonLayout();
        }

        private void SetupButton(Button btn, string text, DialogResult result, bool isPrimary)
        {
            btn.Text = text;
            btn.DialogResult = result;
            btn.Visible = true;

            if (isPrimary)
            {
                btn.BackColor = Color.FromArgb(255, 85, 85); // Kırmızı vurgu
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = Color.FromArgb(50, 50, 70);
                btn.ForeColor = Color.LightGray;
            }
        }

        private void SetupIcon(MessageBoxIcon icon)
        {
            // İkon desteği ileride eklenebilir. Şimdilik sade tutuluyor.
            // Simgeler veya çizimler eklenebilir.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Kenarlık çiz
            using (var pen = new Pen(Color.FromArgb(60, 60, 80), 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void InitializeComponent()
        {
            this.customTitleBar = new SkoprRecord.WinForms.Controls.CustomTitleBar();
            this.lblMessage = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btn1 = new System.Windows.Forms.Button();
            this.btn2 = new System.Windows.Forms.Button();
            this.picIcon = new System.Windows.Forms.PictureBox();

            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // 
            // customTitleBar
            // 
            this.customTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(40)))));
            this.customTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.customTitleBar.Name = "customTitleBar";
            this.customTitleBar.ShowClose = true;
            this.customTitleBar.ShowMinimize = false;
            this.customTitleBar.ShowSettings = false;
            this.customTitleBar.Size = new System.Drawing.Size(400, 40);
            this.customTitleBar.TabIndex = 0;
            this.customTitleBar.CloseRequested += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btn1);
            this.pnlButtons.Controls.Add(this.btn2);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 140);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(400, 60);
            this.pnlButtons.TabIndex = 1;
            this.pnlButtons.Resize += (s, e) => RecalculateButtonLayout();

            // 
            // btn1
            // 
            this.btn1.FlatStyle = FlatStyle.Flat;
            this.btn1.FlatAppearance.BorderSize = 0;
            this.btn1.Size = new Size(100, 35);
            this.btn1.Cursor = Cursors.Hand;
            this.btn1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // 
            // btn2
            // 
            this.btn2.FlatStyle = FlatStyle.Flat;
            this.btn2.FlatAppearance.BorderSize = 0;
            this.btn2.Size = new Size(100, 35);
            this.btn2.Cursor = Cursors.Hand;
            this.btn2.Font = new Font("Segoe UI", 10F);

            // 
            // lblMessage
            // 
            this.lblMessage.Dock = DockStyle.Fill;
            this.lblMessage.ForeColor = Color.White;
            this.lblMessage.Font = new Font("Segoe UI", 10F);
            this.lblMessage.Location = new Point(20, 40);
            this.lblMessage.Padding = new Padding(20);
            this.lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            this.lblMessage.Size = new Size(360, 100);

            // 
            // SkoprMessageBox
            // 
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.customTitleBar);
            this.StartPosition = FormStartPosition.CenterParent;

            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

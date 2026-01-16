namespace SkoprRecord.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlMain = new System.Windows.Forms.Panel();
            this.customTitleBar = new SkoprRecord.WinForms.Controls.CustomTitleBar();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.btnStartScreen = new System.Windows.Forms.Button();
            this.btnStartAudio = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.pnlAudioSettings = new System.Windows.Forms.Panel();
            this.chkSystemAudio = new System.Windows.Forms.CheckBox();
            this.chkMicrophone = new System.Windows.Forms.CheckBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.pnlMain.SuspendLayout();
            this.pnlControls.SuspendLayout();
            this.pnlAudioSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.pnlMain.Controls.Add(this.customTitleBar);
            this.pnlMain.Controls.Add(this.btnSettings);
            this.pnlMain.Controls.Add(this.pnlAudioSettings);
            this.pnlMain.Controls.Add(this.pnlControls);
            this.pnlMain.Controls.Add(this.lblStatus);
            this.pnlMain.Controls.Add(this.lblTime);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(1); // Kenarlık genişliği
            this.pnlMain.Size = new System.Drawing.Size(480, 420); // Başlık için yükseklik artırıldı
            this.pnlMain.TabIndex = 0;
            // 
            // customTitleBar
            // 
            this.customTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(40)))));
            this.customTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.customTitleBar.Location = new System.Drawing.Point(1, 1);
            this.customTitleBar.Name = "customTitleBar";
            this.customTitleBar.ShowClose = true;
            this.customTitleBar.ShowMinimize = true;
            this.customTitleBar.ShowSettings = true;
            this.customTitleBar.Size = new System.Drawing.Size(478, 40);
            this.customTitleBar.TabIndex = 5;
            this.customTitleBar.Title = "SKOPR KAYDET";
            this.customTitleBar.SettingsClick += new System.EventHandler(this.btnSettings_Click);
            // 
            // lblTime
            // 
            this.lblTime.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTime.Font = new System.Drawing.Font("Consolas", 42F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTime.ForeColor = System.Drawing.Color.White;
            this.lblTime.Location = new System.Drawing.Point(20, 110);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(440, 70);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "00:00:00";
            this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(127)))));
            this.lblStatus.Location = new System.Drawing.Point(20, 70);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(440, 30);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "● Hazır";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlControls
            // 
            this.pnlControls.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlControls.Controls.Add(this.btnStartScreen);
            this.pnlControls.Controls.Add(this.btnStartAudio);
            this.pnlControls.Controls.Add(this.btnStop);
            this.pnlControls.Location = new System.Drawing.Point(40, 280);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(400, 60);
            this.pnlControls.TabIndex = 2;
            // 
            // btnStartScreen
            // 
            this.btnStartScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnStartScreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartScreen.FlatAppearance.BorderSize = 0;
            this.btnStartScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartScreen.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStartScreen.ForeColor = System.Drawing.Color.White;
            this.btnStartScreen.Location = new System.Drawing.Point(0, 0);
            this.btnStartScreen.Name = "btnStartScreen";
            this.btnStartScreen.Size = new System.Drawing.Size(130, 50);
            this.btnStartScreen.TabIndex = 0;
            this.btnStartScreen.Text = "EKRAN KAYDI";
            this.btnStartScreen.UseVisualStyleBackColor = false;
            this.btnStartScreen.Click += new System.EventHandler(this.btnStartScreen_Click);
            // 
            // btnStartAudio
            // 
            this.btnStartAudio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnStartAudio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartAudio.FlatAppearance.BorderSize = 0;
            this.btnStartAudio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartAudio.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStartAudio.ForeColor = System.Drawing.Color.White;
            this.btnStartAudio.Location = new System.Drawing.Point(140, 0);
            this.btnStartAudio.Name = "btnStartAudio";
            this.btnStartAudio.Size = new System.Drawing.Size(130, 50);
            this.btnStartAudio.TabIndex = 1;
            this.btnStartAudio.Text = "SES KAYDI";
            this.btnStartAudio.UseVisualStyleBackColor = false;
            this.btnStartAudio.Click += new System.EventHandler(this.btnStartAudio_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(55)))));
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Enabled = false;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(280, 0);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 50);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "DURDUR";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // pnlAudioSettings
            // 
            this.pnlAudioSettings.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlAudioSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(55)))));
            this.pnlAudioSettings.Controls.Add(this.chkSystemAudio);
            this.pnlAudioSettings.Controls.Add(this.chkMicrophone);
            this.pnlAudioSettings.Location = new System.Drawing.Point(90, 210);
            this.pnlAudioSettings.Name = "pnlAudioSettings";
            this.pnlAudioSettings.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlAudioSettings.Size = new System.Drawing.Size(300, 50);
            this.pnlAudioSettings.TabIndex = 3;
            // 
            // chkSystemAudio
            // 
            this.chkSystemAudio.AutoSize = true;
            this.chkSystemAudio.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkSystemAudio.ForeColor = System.Drawing.Color.White;
            this.chkSystemAudio.Location = new System.Drawing.Point(15, 13);
            this.chkSystemAudio.Name = "chkSystemAudio";
            this.chkSystemAudio.Size = new System.Drawing.Size(112, 23);
            this.chkSystemAudio.TabIndex = 0;
            this.chkSystemAudio.Text = "🔊 Sistem Sesi";
            this.chkSystemAudio.UseVisualStyleBackColor = true;
            this.chkSystemAudio.CheckedChanged += new System.EventHandler(this.chkSystemAudio_CheckedChanged);
            // 
            // chkMicrophone
            // 
            this.chkMicrophone.AutoSize = true;
            this.chkMicrophone.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkMicrophone.ForeColor = System.Drawing.Color.White;
            this.chkMicrophone.Location = new System.Drawing.Point(170, 13);
            this.chkMicrophone.Name = "chkMicrophone";
            this.chkMicrophone.Size = new System.Drawing.Size(103, 23);
            this.chkMicrophone.TabIndex = 1;
            this.chkMicrophone.Text = "🎤 Mikrofon";
            this.chkMicrophone.UseVisualStyleBackColor = true;
            this.chkMicrophone.CheckedChanged += new System.EventHandler(this.chkMicrophone_CheckedChanged);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(0, 0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(0, 0);
            this.btnSettings.TabIndex = 6;
            this.btnSettings.Visible = false; 
            // 
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 420);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; // Borderless
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Skopr Kaydet";
            this.pnlMain.ResumeLayout(false);
            this.pnlControls.ResumeLayout(false);
            this.pnlAudioSettings.ResumeLayout(false);
            this.pnlAudioSettings.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private SkoprRecord.WinForms.Controls.CustomTitleBar customTitleBar;
        private Label lblTime;
        private Label lblStatus;
        private Panel pnlControls;
        private Button btnStartScreen;
        private Button btnStartAudio;
        private Button btnStop;
        private Panel pnlAudioSettings;
        private CheckBox chkSystemAudio;
        private CheckBox chkMicrophone;
        private Button btnSettings;
    }
}

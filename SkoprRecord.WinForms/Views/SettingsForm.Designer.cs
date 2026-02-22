namespace SkoprRecord.WinForms.Views
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlMain = new System.Windows.Forms.Panel();
            this.customTitleBar = new SkoprRecord.WinForms.Controls.CustomTitleBar();
            this.pnlNavigation = new System.Windows.Forms.Panel();
            this.btnNavRecording = new System.Windows.Forms.Button();
            this.btnNavAudio = new System.Windows.Forms.Button();
            this.btnNavGeneral = new System.Windows.Forms.Button();
            this.pnlTabRecording = new System.Windows.Forms.Panel();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblFps = new System.Windows.Forms.Label();
            this.numFps = new System.Windows.Forms.NumericUpDown();
            this.lblBitrate = new System.Windows.Forms.Label();
            this.numBitrate = new System.Windows.Forms.NumericUpDown();
            this.pnlTabAudio = new System.Windows.Forms.Panel();
            this.chkCaptureSystemAudio = new System.Windows.Forms.CheckBox();
            this.chkCaptureMicrophone = new System.Windows.Forms.CheckBox();
            this.pnlTabGeneral = new System.Windows.Forms.Panel();
            this.chkStartInTray = new System.Windows.Forms.CheckBox();
            this.chkShowNotifications = new System.Windows.Forms.CheckBox();
            this.chkConfirmSaveOnStop = new System.Windows.Forms.CheckBox();
            this.lblHotkeys = new System.Windows.Forms.Label();
            this.lblHotkeyScreen = new System.Windows.Forms.Label();
            this.txtHotkeyScreen = new System.Windows.Forms.TextBox();
            this.lblHotkeyAudio = new System.Windows.Forms.Label();
            this.txtHotkeyAudio = new System.Windows.Forms.TextBox();
            this.lblHotkeyStop = new System.Windows.Forms.Label();
            this.txtHotkeyStop = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlMain.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlTabRecording.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBitrate)).BeginInit();
            this.pnlTabAudio.SuspendLayout();
            this.pnlTabGeneral.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.pnlMain.Controls.Add(this.pnlTabRecording);
            this.pnlMain.Controls.Add(this.pnlTabAudio);
            this.pnlMain.Controls.Add(this.pnlTabGeneral);
            this.pnlMain.Controls.Add(this.pnlNavigation);
            this.pnlMain.Controls.Add(this.customTitleBar);
            this.pnlMain.Controls.Add(this.pnlButtons);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(0);
            this.pnlMain.Size = new System.Drawing.Size(500, 450);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlNavigation
            // 
            this.pnlNavigation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(40)))));
            this.pnlNavigation.Controls.Add(this.btnNavGeneral);
            this.pnlNavigation.Controls.Add(this.btnNavAudio);
            this.pnlNavigation.Controls.Add(this.btnNavRecording);
            this.pnlNavigation.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNavigation.Location = new System.Drawing.Point(0, 40);
            this.pnlNavigation.Name = "pnlNavigation";
            this.pnlNavigation.Size = new System.Drawing.Size(500, 45);
            this.pnlNavigation.TabIndex = 3;
            // 
            // btnNavRecording
            // 
            this.btnNavRecording.FlatAppearance.BorderSize = 0;
            this.btnNavRecording.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.btnNavRecording.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavRecording.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavRecording.ForeColor = System.Drawing.Color.White;
            this.btnNavRecording.Location = new System.Drawing.Point(0, 0);
            this.btnNavRecording.Name = "btnNavRecording";
            this.btnNavRecording.Size = new System.Drawing.Size(120, 45);
            this.btnNavRecording.TabIndex = 0;
            this.btnNavRecording.Text = "Kayıt";
            this.btnNavRecording.UseVisualStyleBackColor = true;
            this.btnNavRecording.Click += new System.EventHandler(this.btnNav_Click);
            // 
            // btnNavAudio
            // 
            this.btnNavAudio.FlatAppearance.BorderSize = 0;
            this.btnNavAudio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.btnNavAudio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavAudio.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnNavAudio.ForeColor = System.Drawing.Color.LightGray;
            this.btnNavAudio.Location = new System.Drawing.Point(120, 0);
            this.btnNavAudio.Name = "btnNavAudio";
            this.btnNavAudio.Size = new System.Drawing.Size(120, 45);
            this.btnNavAudio.TabIndex = 1;
            this.btnNavAudio.Text = "Ses";
            this.btnNavAudio.UseVisualStyleBackColor = true;
            this.btnNavAudio.Click += new System.EventHandler(this.btnNav_Click);
            // 
            // btnNavGeneral
            // 
            this.btnNavGeneral.FlatAppearance.BorderSize = 0;
            this.btnNavGeneral.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.btnNavGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavGeneral.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnNavGeneral.ForeColor = System.Drawing.Color.LightGray;
            this.btnNavGeneral.Location = new System.Drawing.Point(240, 0);
            this.btnNavGeneral.Name = "btnNavGeneral";
            this.btnNavGeneral.Size = new System.Drawing.Size(120, 45);
            this.btnNavGeneral.TabIndex = 2;
            this.btnNavGeneral.Text = "Genel";
            this.btnNavGeneral.UseVisualStyleBackColor = true;
            this.btnNavGeneral.Click += new System.EventHandler(this.btnNav_Click);
            // 
            // customTitleBar
            // 
            this.customTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(40)))));
            this.customTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.customTitleBar.Location = new System.Drawing.Point(0, 0);
            this.customTitleBar.Name = "customTitleBar";
            this.customTitleBar.ShowClose = true;
            this.customTitleBar.ShowMinimize = false;
            this.customTitleBar.ShowSettings = false;
            this.customTitleBar.Size = new System.Drawing.Size(500, 40);
            this.customTitleBar.TabIndex = 2;
            this.customTitleBar.Title = "Ayarlar";
            this.customTitleBar.CloseRequested += new System.EventHandler(this.customTitleBar_CloseRequested);
            // 
            // pnlTabRecording
            // 
            this.pnlTabRecording.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.pnlTabRecording.Controls.Add(this.numBitrate);
            this.pnlTabRecording.Controls.Add(this.lblBitrate);
            this.pnlTabRecording.Controls.Add(this.numFps);
            this.pnlTabRecording.Controls.Add(this.lblFps);
            this.pnlTabRecording.Controls.Add(this.btnBrowse);
            this.pnlTabRecording.Controls.Add(this.txtOutputFolder);
            this.pnlTabRecording.Controls.Add(this.lblOutputFolder);
            this.pnlTabRecording.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabRecording.Location = new System.Drawing.Point(0, 85);
            this.pnlTabRecording.Name = "pnlTabRecording";
            this.pnlTabRecording.Padding = new System.Windows.Forms.Padding(20);
            this.pnlTabRecording.Size = new System.Drawing.Size(500, 305);
            this.pnlTabRecording.TabIndex = 4;
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.ForeColor = System.Drawing.Color.White;
            this.lblOutputFolder.Location = new System.Drawing.Point(20, 20);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(95, 19);
            this.lblOutputFolder.TabIndex = 0;
            this.lblOutputFolder.Text = "Kayıt Klasörü:";
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.txtOutputFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOutputFolder.ForeColor = System.Drawing.Color.White;
            this.txtOutputFolder.Location = new System.Drawing.Point(20, 45);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(320, 25);
            this.txtOutputFolder.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(350, 45);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(80, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Gözat...";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblFps
            // 
            this.lblFps.AutoSize = true;
            this.lblFps.ForeColor = System.Drawing.Color.White;
            this.lblFps.Location = new System.Drawing.Point(20, 90);
            this.lblFps.Name = "lblFps";
            this.lblFps.Size = new System.Drawing.Size(113, 19);
            this.lblFps.TabIndex = 3;
            this.lblFps.Text = "FPS (Kare Hızı):";
            // 
            // numFps
            // 
            this.numFps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.numFps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numFps.ForeColor = System.Drawing.Color.White;
            this.numFps.Location = new System.Drawing.Point(20, 115);
            this.numFps.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            this.numFps.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
            this.numFps.Name = "numFps";
            this.numFps.Size = new System.Drawing.Size(120, 25);
            this.numFps.TabIndex = 4;
            this.numFps.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblBitrate
            // 
            this.lblBitrate.AutoSize = true;
            this.lblBitrate.ForeColor = System.Drawing.Color.White;
            this.lblBitrate.Location = new System.Drawing.Point(20, 160);
            this.lblBitrate.Name = "lblBitrate";
            this.lblBitrate.Size = new System.Drawing.Size(115, 19);
            this.lblBitrate.TabIndex = 5;
            this.lblBitrate.Text = "Bitrate (Kbps):";
            // 
            // numBitrate
            // 
            this.numBitrate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.numBitrate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numBitrate.ForeColor = System.Drawing.Color.White;
            this.numBitrate.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numBitrate.Location = new System.Drawing.Point(20, 185);
            this.numBitrate.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
            this.numBitrate.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numBitrate.Name = "numBitrate";
            this.numBitrate.Size = new System.Drawing.Size(120, 25);
            this.numBitrate.TabIndex = 6;
            this.numBitrate.Value = new decimal(new int[] { 8000, 0, 0, 0 });
            // 
            // pnlTabAudio
            // 
            this.pnlTabAudio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.pnlTabAudio.Controls.Add(this.chkCaptureMicrophone);
            this.pnlTabAudio.Controls.Add(this.chkCaptureSystemAudio);
            this.pnlTabAudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabAudio.Location = new System.Drawing.Point(0, 85);
            this.pnlTabAudio.Name = "pnlTabAudio";
            this.pnlTabAudio.Padding = new System.Windows.Forms.Padding(20);
            this.pnlTabAudio.Size = new System.Drawing.Size(500, 305);
            this.pnlTabAudio.TabIndex = 5;
            this.pnlTabAudio.Visible = false;
            // 
            // chkCaptureSystemAudio
            // 
            this.chkCaptureSystemAudio.AutoSize = true;
            this.chkCaptureSystemAudio.ForeColor = System.Drawing.Color.White;
            this.chkCaptureSystemAudio.Location = new System.Drawing.Point(20, 20);
            this.chkCaptureSystemAudio.Name = "chkCaptureSystemAudio";
            this.chkCaptureSystemAudio.Size = new System.Drawing.Size(176, 23);
            this.chkCaptureSystemAudio.TabIndex = 0;
            this.chkCaptureSystemAudio.Text = "Sistem Sesini Kaydet";
            this.chkCaptureSystemAudio.UseVisualStyleBackColor = true;
            // 
            // chkCaptureMicrophone
            // 
            this.chkCaptureMicrophone.AutoSize = true;
            this.chkCaptureMicrophone.ForeColor = System.Drawing.Color.White;
            this.chkCaptureMicrophone.Location = new System.Drawing.Point(20, 60);
            this.chkCaptureMicrophone.Name = "chkCaptureMicrophone";
            this.chkCaptureMicrophone.Size = new System.Drawing.Size(157, 23);
            this.chkCaptureMicrophone.TabIndex = 1;
            this.chkCaptureMicrophone.Text = "Mikrofonu Kaydet";
            this.chkCaptureMicrophone.UseVisualStyleBackColor = true;
            // 
            // pnlTabGeneral
            // 
            this.pnlTabGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.pnlTabGeneral.Controls.Add(this.chkConfirmSaveOnStop);
            this.pnlTabGeneral.Controls.Add(this.chkShowNotifications);
            this.pnlTabGeneral.Controls.Add(this.chkStartInTray);
            this.pnlTabGeneral.Controls.Add(this.lblHotkeys);
            this.pnlTabGeneral.Controls.Add(this.lblHotkeyScreen);
            this.pnlTabGeneral.Controls.Add(this.txtHotkeyScreen);
            this.pnlTabGeneral.Controls.Add(this.lblHotkeyAudio);
            this.pnlTabGeneral.Controls.Add(this.txtHotkeyAudio);
            this.pnlTabGeneral.Controls.Add(this.lblHotkeyStop);
            this.pnlTabGeneral.Controls.Add(this.txtHotkeyStop);
            this.pnlTabGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabGeneral.Location = new System.Drawing.Point(0, 85);
            this.pnlTabGeneral.Name = "pnlTabGeneral";
            this.pnlTabGeneral.Padding = new System.Windows.Forms.Padding(20);
            this.pnlTabGeneral.Size = new System.Drawing.Size(500, 305);
            this.pnlTabGeneral.TabIndex = 6;
            this.pnlTabGeneral.Visible = false;
            // 
            // chkStartInTray
            // 
            this.chkStartInTray.AutoSize = true;
            this.chkStartInTray.ForeColor = System.Drawing.Color.White;
            this.chkStartInTray.Location = new System.Drawing.Point(20, 20);
            this.chkStartInTray.Name = "chkStartInTray";
            this.chkStartInTray.Size = new System.Drawing.Size(214, 23);
            this.chkStartInTray.TabIndex = 0;
            this.chkStartInTray.Text = "Sistem Tepsisinde Başlat";
            this.chkStartInTray.UseVisualStyleBackColor = true;
            // 
            // chkShowNotifications
            // 
            this.chkShowNotifications.AutoSize = true;
            this.chkShowNotifications.ForeColor = System.Drawing.Color.White;
            this.chkShowNotifications.Location = new System.Drawing.Point(20, 60);
            this.chkShowNotifications.Name = "chkShowNotifications";
            this.chkShowNotifications.Size = new System.Drawing.Size(172, 23);
            this.chkShowNotifications.TabIndex = 1;
            this.chkShowNotifications.Text = "Bildirimleri Göster";
            this.chkShowNotifications.UseVisualStyleBackColor = true;
            // 
            // chkConfirmSaveOnStop
            // 
            this.chkConfirmSaveOnStop.AutoSize = true;
            this.chkConfirmSaveOnStop.ForeColor = System.Drawing.Color.White;
            this.chkConfirmSaveOnStop.Location = new System.Drawing.Point(20, 100);
            this.chkConfirmSaveOnStop.Name = "chkConfirmSaveOnStop";
            this.chkConfirmSaveOnStop.Size = new System.Drawing.Size(261, 23);
            this.chkConfirmSaveOnStop.TabIndex = 2;
            this.chkConfirmSaveOnStop.Text = "Durdurulduğunda Kaydetmeyi Onayla";
            this.chkConfirmSaveOnStop.UseVisualStyleBackColor = true;
            // 
            // lblHotkeys
            // 
            this.lblHotkeys.AutoSize = true;
            this.lblHotkeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblHotkeys.Location = new System.Drawing.Point(20, 130);
            this.lblHotkeys.Name = "lblHotkeys";
            this.lblHotkeys.Size = new System.Drawing.Size(120, 19);
            this.lblHotkeys.TabIndex = 3;
            this.lblHotkeys.Text = "Kısayol Tuşları:";
            // 
            // lblHotkeyScreen
            // 
            this.lblHotkeyScreen.AutoSize = true;
            this.lblHotkeyScreen.ForeColor = System.Drawing.Color.White;
            this.lblHotkeyScreen.Location = new System.Drawing.Point(20, 160);
            this.lblHotkeyScreen.Name = "lblHotkeyScreen";
            this.lblHotkeyScreen.Size = new System.Drawing.Size(80, 19);
            this.lblHotkeyScreen.TabIndex = 4;
            this.lblHotkeyScreen.Text = "Ekran Kaydı:";
            // 
            // txtHotkeyScreen
            // 
            this.txtHotkeyScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.txtHotkeyScreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHotkeyScreen.ForeColor = System.Drawing.Color.White;
            this.txtHotkeyScreen.Location = new System.Drawing.Point(120, 158);
            this.txtHotkeyScreen.Name = "txtHotkeyScreen";
            this.txtHotkeyScreen.ReadOnly = true;
            this.txtHotkeyScreen.Size = new System.Drawing.Size(160, 25);
            this.txtHotkeyScreen.TabIndex = 5;
            this.txtHotkeyScreen.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HotkeyTextBox_KeyDown);
            // 
            // lblHotkeyAudio
            // 
            this.lblHotkeyAudio.AutoSize = true;
            this.lblHotkeyAudio.ForeColor = System.Drawing.Color.White;
            this.lblHotkeyAudio.Location = new System.Drawing.Point(20, 195);
            this.lblHotkeyAudio.Name = "lblHotkeyAudio";
            this.lblHotkeyAudio.Size = new System.Drawing.Size(70, 19);
            this.lblHotkeyAudio.TabIndex = 6;
            this.lblHotkeyAudio.Text = "Ses Kaydı:";
            // 
            // txtHotkeyAudio
            // 
            this.txtHotkeyAudio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.txtHotkeyAudio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHotkeyAudio.ForeColor = System.Drawing.Color.White;
            this.txtHotkeyAudio.Location = new System.Drawing.Point(120, 193);
            this.txtHotkeyAudio.Name = "txtHotkeyAudio";
            this.txtHotkeyAudio.ReadOnly = true;
            this.txtHotkeyAudio.Size = new System.Drawing.Size(160, 25);
            this.txtHotkeyAudio.TabIndex = 7;
            this.txtHotkeyAudio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HotkeyTextBox_KeyDown);
            // 
            // lblHotkeyStop
            // 
            this.lblHotkeyStop.AutoSize = true;
            this.lblHotkeyStop.ForeColor = System.Drawing.Color.White;
            this.lblHotkeyStop.Location = new System.Drawing.Point(20, 230);
            this.lblHotkeyStop.Name = "lblHotkeyStop";
            this.lblHotkeyStop.Size = new System.Drawing.Size(60, 19);
            this.lblHotkeyStop.TabIndex = 8;
            this.lblHotkeyStop.Text = "Durdur:";
            // 
            // txtHotkeyStop
            // 
            this.txtHotkeyStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.txtHotkeyStop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHotkeyStop.ForeColor = System.Drawing.Color.White;
            this.txtHotkeyStop.Location = new System.Drawing.Point(120, 228);
            this.txtHotkeyStop.Name = "txtHotkeyStop";
            this.txtHotkeyStop.ReadOnly = true;
            this.txtHotkeyStop.Size = new System.Drawing.Size(160, 25);
            this.txtHotkeyStop.TabIndex = 9;
            this.txtHotkeyStop.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HotkeyTextBox_KeyDown);
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 390);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(500, 60);
            this.pnlButtons.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(240, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 40);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Kaydet";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(55)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(350, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 40);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "İptal";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 450);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ayarlar";
            this.pnlMain.ResumeLayout(false);
            this.pnlTabRecording.ResumeLayout(false);
            this.pnlTabRecording.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBitrate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBitrate)).EndInit();
            this.pnlTabAudio.ResumeLayout(false);
            this.pnlTabAudio.PerformLayout();
            this.pnlTabGeneral.ResumeLayout(false);
            this.pnlTabGeneral.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private SkoprRecord.WinForms.Controls.CustomTitleBar customTitleBar;
        private Panel pnlNavigation;
        private Button btnNavRecording;
        private Button btnNavAudio;
        private Button btnNavGeneral;
        private Panel pnlTabRecording;
        private Label lblOutputFolder;
        private TextBox txtOutputFolder;
        private Button btnBrowse;
        private Label lblFps;
        private NumericUpDown numFps;
        private Label lblBitrate;
        private NumericUpDown numBitrate;
        private Panel pnlTabAudio;
        private CheckBox chkCaptureSystemAudio;
        private CheckBox chkCaptureMicrophone;
        private Panel pnlTabGeneral;
        private CheckBox chkStartInTray;
        private CheckBox chkShowNotifications;
        private CheckBox chkConfirmSaveOnStop;
        private Label lblHotkeys;
        private Label lblHotkeyScreen;
        private TextBox txtHotkeyScreen;
        private Label lblHotkeyAudio;
        private TextBox txtHotkeyAudio;
        private Label lblHotkeyStop;
        private TextBox txtHotkeyStop;
        private Panel pnlButtons;
        private Button btnSave;
        private Button btnCancel;
    }
}

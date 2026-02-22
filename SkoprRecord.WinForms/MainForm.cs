using Serilog;
using SkoprRecord.Application.Helpers;
using SkoprRecord.Application.Interfaces;
using SkoprRecord.Application.Services;
using SkoprRecord.Domain.Models;
using SkoprRecord.WinForms.Services;
using SkoprRecord.WinForms.Views;

namespace SkoprRecord.WinForms;

/// <summary>
/// Ana form - Kullanıcı arayüzü ve kayıt kontrollerini yönetir.
/// </summary>
public partial class MainForm : Form
{
    private readonly IRecorderController _controller;
    private readonly SettingsService _settingsService;
    private RecordingSettings _settings;
    private System.Windows.Forms.Timer _timer = null!;
    private static Icon? _appIcon; // Statik tutarak handle'ın ve stream'in ölmesini engelliyoruz
    private TimeSpan _elapsedTime;
    private NotifyIcon? _trayIcon;
    private GlobalHotkeyService? _hotkeyService;
    private ToolStripMenuItem? _startScreenItem;
    private ToolStripMenuItem? _startAudioItem;
    private ToolStripMenuItem? _stopRecordingItem;
    private ToolStripMenuItem? _systemAudioItem;
    private ToolStripMenuItem? _microphoneItem;
    private bool _isExiting = false;
    private bool _hasShownTrayNotification = false;
    private bool _allowExplicitShow = false;

    public MainForm(IRecorderController controller, SettingsService settingsService)
    {
        _controller = controller;
        _settingsService = settingsService;
        _settings = _settingsService.Load();

        InitializeComponent();
        LoadAppIcon();
        InitializeTimer();
        SetupTrayIcon();
        SetupButtonStyles(); // Buton stillerini ayarla
        SetupEventHandlers();
        SetupGlobalHotkey();
        customTitleBar.CloseRequested += CustomTitleBar_CloseRequested;

        // Yükleme ve yeniden boyutlandırma sırasında butonların ortalanmasını sağla
        this.Load += (s, e) => CenterButtons();
        pnlControls.Resize += (s, e) => CenterButtons();

        UpdateUI();

        // Eğer tepside başlatma seçili DEĞİLSE, görünürlüğe en baştan izin ver.
        if (!_settings.StartInTray)
        {
            _allowExplicitShow = true;
        }

        // FFmpeg kontrolü arka planda
        _ = Task.Run(async () => await CheckAndInstallFfmpegAsync());
    }

    /// <summary>
    /// Pencere görünürlüğünü kontrol eder. Tepside başlatma ayarı için override edildi.
    /// </summary>
    protected override void SetVisibleCore(bool value)
    {
        // Eğer göstermeye çalışılıyorsa (value=true) AMA açıkça izin verilmemişse engelle
        if (value && !_allowExplicitShow)
        {
            if (!IsHandleCreated) CreateHandle();
            value = false;

            // StartInTray aktifse, kullanıcıya uygulamanın çalıştığını bildirmek için 
            // genel bildirim ayarından bağımsız olarak bilgi ver.
            if (!_hasShownTrayNotification)
            {
                _hasShownTrayNotification = true;
                _trayIcon?.ShowBalloonTip(2000, "Skopr Kaydet", "Uygulama arka planda hazır.", ToolTipIcon.Info);
            }
        }
        base.SetVisibleCore(value);
    }

    /// <summary>
    /// Form kenarlığını özel renk ile çizer.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
            Color.FromArgb(60, 60, 80), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(60, 60, 80), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(60, 60, 80), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(60, 60, 80), 1, ButtonBorderStyle.Solid);
    }

    private async void CustomTitleBar_CloseRequested(object? sender, EventArgs e)
    {
        if (_isExiting) return;

        // Kayıt devam ediyorsa onay iste (btnStop enabled ise kayıt var demektir)
        if (btnStop.Enabled)
        {
            _isExiting = true;
            var result = Views.SkoprMessageBox.Show(
                "Kayıt devam ediyor. Kaydı durdurup uygulamayı kapatmak istiyor musunuz?",
                "Kayıt Devam Ediyor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Kaydı durdur ve dosya yolunu al
                var filePath = await _controller.StopRecordingAsync(suppressEvent: true);

                // Eğer dosya kaydedildiyse ve ayar aktifse, kaydetme penceresini manuel göster
                if (!string.IsNullOrEmpty(filePath) && _settings.ConfirmSaveOnStop)
                {
                    HandleManualSave(filePath);
                }

                System.Windows.Forms.Application.Exit();
            }
            else
            {
                _isExiting = false;
            }
        }
        else
        {
            System.Windows.Forms.Application.Exit();
        }
    }

    /// <summary>
    /// Zamanlayıcıyı başlatır. Her 100ms'de bir kayıt süresini günceller.
    /// </summary>
    private void InitializeTimer()
    {
        _timer = new System.Windows.Forms.Timer();
        _timer.Interval = 100; // 100ms
        _timer.Tick += Timer_Tick;
    }

    /// <summary>
    /// Uygulama ikonunu Assets klasöründen yükler.
    /// </summary>
    /// <remarks>
    /// İkon bulunamazsa hata loglanır ama uygulama çalışmaya devam eder.
    /// </remarks>
    private void LoadAppIcon()
    {
        if (_appIcon != null)
        {
            this.Icon = _appIcon;
            return;
        }

        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // Not: Ad alanı ProjeAdı.KlasörAdı.DosyaAdı şeklindedir
            var resourceName = "SkoprRecord.WinForms.Assets.app_icon.ico";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    
                    // Çoklu çözünürlük desteği ile yükle
                    _appIcon = new Icon(ms);
                    this.Icon = _appIcon;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Uygulama ikonu yüklenemedi.");
        }
    }

    /// <summary>
    /// RecorderController event'lerine abone olur (RecordingStarted, RecordingEnded).
    /// </summary>
    private void SetupEventHandlers()
    {
        _controller.RecordingStarted += OnRecordingStarted;
        _controller.RecordingEnded += OnRecordingEnded;
        _controller.Settings = _settings;

        this.FormClosing += MainForm_FormClosing;
        this.Resize += MainForm_Resize;
    }

    /// <summary>
    /// Sistem tepsisi (System Tray) ikonunu ve sağ tık menüsünü oluşturur.
    /// </summary>
    /// <remarks>
    /// Menü öğeleri: Ekran Kaydı, Ses Kaydı, Ayarlar, Çıkış.
    /// Ses ayarları (Sistem Sesi, Mikrofon) dinamik olarak güncellenir.
    /// </remarks>
    private void SetupTrayIcon()
    {
        // İkonu LoadAppIcon üzerinden veya statikten al
        if (_appIcon == null) LoadAppIcon();
        
        _trayIcon = new NotifyIcon
        {
            Icon = _appIcon ?? this.Icon,
            Text = "Skopr Kaydet",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();

        var showItem = new ToolStripMenuItem("📺 Göster");
        showItem.Click += (s, e) => ShowForm();
        contextMenu.Items.Add(showItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        _startScreenItem = new ToolStripMenuItem("🔴 Ekran Kaydı Başlat");
        _startScreenItem.Click += (s, e) => StartScreenRecording();
        contextMenu.Items.Add(_startScreenItem);

        _startAudioItem = new ToolStripMenuItem("🎵 Ses Kaydı Başlat");
        _startAudioItem.Click += (s, e) => StartAudioRecording();
        contextMenu.Items.Add(_startAudioItem);

        _stopRecordingItem = new ToolStripMenuItem("⏹️ Kaydı Durdur");
        _stopRecordingItem.Click += (s, e) => StopRecording();
        _stopRecordingItem.Visible = false;
        contextMenu.Items.Add(_stopRecordingItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        _systemAudioItem = new ToolStripMenuItem(_settings.CaptureSystemAudio ? "🔊 Sistem Sesi: Aktif" : "🔊 Sistem Sesi: Kapalı");
        _systemAudioItem.Click += (s, e) =>
        {
            _settings.CaptureSystemAudio = !_settings.CaptureSystemAudio;
            _settingsService.Save(_settings);
            _controller.Settings = _settings;
            UpdateUI();
            UpdateTrayMenuAudioItems();
        };
        contextMenu.Items.Add(_systemAudioItem);

        _microphoneItem = new ToolStripMenuItem(_settings.CaptureMicrophone ? "🎤 Mikrofon: Aktif" : "🎤 Mikrofon: Kapalı");
        _microphoneItem.Click += (s, e) =>
        {
            _settings.CaptureMicrophone = !_settings.CaptureMicrophone;
            _settingsService.Save(_settings);
            _controller.Settings = _settings;
            UpdateUI();
            UpdateTrayMenuAudioItems();
        };
        contextMenu.Items.Add(_microphoneItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var settingsItem = new ToolStripMenuItem("⚙️ Ayarlar");
        settingsItem.Click += (s, e) => OpenSettings();
        contextMenu.Items.Add(settingsItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("❌ Çıkış");
        exitItem.Click += async (s, e) =>
        {
            if (_isExiting) return;

            if (_controller.CurrentState == Domain.Enums.RecorderState.Recording)
            {
                _isExiting = true;
                var result = Views.SkoprMessageBox.Show(
                    "Kayıt devam ediyor. Kaydı durdurup uygulamayı kapatmak istiyor musunuz?",
                    "Kayıt Devam Ediyor",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Kaydı durdur ve dosya yolunu al (Event tetiklemeyi bastır, manuel yöneteceğiz)
                    var filePath = await _controller.StopRecordingAsync(suppressEvent: true);

                    // Eğer dosya kaydedildiyse ve ayar aktifse, kaydetme penceresini manuel göster
                    if (!string.IsNullOrEmpty(filePath) && _settings.ConfirmSaveOnStop)
                    {
                        HandleManualSave(filePath);
                    }
                }
                else
                {
                    _isExiting = false;
                    return;
                }
            }

            _settingsService.Save(_settings);
            _trayIcon?.Dispose();
            System.Windows.Forms.Application.Exit();
        };
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.MouseDoubleClick += (s, e) => { if (e.Button == MouseButtons.Left) ShowForm(); };
    }

    /// <summary>
    /// Global kısayol tuşlarını (Ayarlardan gelen) kaydeder.
    /// </summary>
    /// <remarks>
    /// Başarısız olursa hata loglanır ama uygulama çalışmaya devam eder.
    /// </remarks>
    private void SetupGlobalHotkey()
    {
        try
        {
            if (_hotkeyService != null)
            {
                _hotkeyService.Dispose();
            }

            _hotkeyService = new GlobalHotkeyService(
                this, 
                _settings.HotkeyScreenMods, _settings.HotkeyScreenKey,
                _settings.HotkeyAudioMods, _settings.HotkeyAudioKey,
                _settings.HotkeyStopMods, _settings.HotkeyStopKey
            );

            _hotkeyService.ScreenRecordingRequested += OnScreenRecordingRequested;
            _hotkeyService.AudioRecordingRequested += OnAudioRecordingRequested;
            _hotkeyService.StopRecordingRequested += OnStopRecordingRequested;

            if (_hotkeyService.Register())
            {
                Log.Information("Global hotkeys kaydedildi.");
            }
            else
            {
                Log.Warning("Global hotkeys kaydedilemedi.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Global hotkey kurulumu başarısız.");
        }
    }

    private void OnScreenRecordingRequested(object? sender, EventArgs e)
    {
        if (InvokeRequired) { Invoke(() => OnScreenRecordingRequested(sender, e)); return; }
        if (_controller.CurrentState != Domain.Enums.RecorderState.Recording) StartScreenRecording();
    }

    private void OnAudioRecordingRequested(object? sender, EventArgs e)
    {
        if (InvokeRequired) { Invoke(() => OnAudioRecordingRequested(sender, e)); return; }
        if (_controller.CurrentState != Domain.Enums.RecorderState.Recording) StartAudioRecording();
    }

    private void OnStopRecordingRequested(object? sender, EventArgs e)
    {
        if (InvokeRequired) { Invoke(() => OnStopRecordingRequested(sender, e)); return; }
        if (_controller.CurrentState == Domain.Enums.RecorderState.Recording) StopRecording();
    }

    /// <summary>
    /// Tray menüsündeki ses ayarları metinlerini günceller.
    /// </summary>
    private void UpdateTrayMenuAudioItems()
    {
        if (_systemAudioItem != null)
        {
            _systemAudioItem.Text = _settings.CaptureSystemAudio ? "🔊 Sistem Sesi: Aktif" : "🔊 Sistem Sesi: Kapalı";
        }

        if (_microphoneItem != null)
        {
            _microphoneItem.Text = _settings.CaptureMicrophone ? "🎤 Mikrofon: Aktif" : "🎤 Mikrofon: Kapalı";
        }
    }

    /// <summary>
    /// Tray menüsündeki kayıt başlat/durdur butonlarının durumunu günceller.
    /// </summary>
    /// <param name="isRecording">Kayıt aktif mi?</param>
    private void UpdateTrayMenuState(bool isRecording)
    {
        if (_startScreenItem != null)
        {
            _startScreenItem.Enabled = !isRecording;
            _startScreenItem.Text = isRecording ? "🔴 Ekran Kaydı (Kayıt Devam Ediyor)" : "🔴 Ekran Kaydı Başlat";
        }

        if (_startAudioItem != null)
        {
            _startAudioItem.Enabled = !isRecording;
            _startAudioItem.Text = isRecording ? "🎵 Ses Kaydı (Kayıt Devam Ediyor)" : "🎵 Ses Kaydı Başlat";
        }

        if (_stopRecordingItem != null)
        {
            _stopRecordingItem.Visible = isRecording;
        }
    }

    /// <summary>
    /// Formu görünür hale getirir ve öne çıkarır.
    /// </summary>
    private void ShowForm()
    {
        _allowExplicitShow = true; // Görünürlüğe izin ver
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide();
            if (_settings.ShowNotifications)
            {
                // _trayIcon?.ShowBalloonTip(2000, "Skopr Kaydet", "Uygulama arka planda çalışıyor.", ToolTipIcon.Info);
                // Resize sırasında zırt pırt bildirim çıkmasın, sadece initial veya explicit durumlarda çıksın
            }
        }
    }

    /// <summary>
    /// Form yüklendiğinde çalışır. 
    /// </summary>
    private void MainForm_Load(object? sender, EventArgs e)
    {
        // StartInTray mantığı SetVisibleCore içine taşındı.
    }

    private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Kayıt devam ediyorsa onay iste
        if (_controller.CurrentState == Domain.Enums.RecorderState.Recording)
        {
            e.Cancel = true; // Kapatmayı geçici olarak iptal et

            var result = Views.SkoprMessageBox.Show(
                "Kayıt devam ediyor. Kaydı durdurup uygulamayı kapatmak istiyor musunuz?",
                "Kayıt Devam Ediyor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Kaydı durdur ve dosya yolunu al
                var filePath = await _controller.StopRecordingAsync(suppressEvent: true);

                // Eğer dosya kaydedildiyse ve ayar aktifse, kaydetme penceresini manuel göster
                if (!string.IsNullOrEmpty(filePath) && _settings.ConfirmSaveOnStop)
                {
                    HandleManualSave(filePath);
                }

                // Şimdi gerçekten kapat
                _settingsService.Save(_settings);
                _trayIcon?.Dispose();
                System.Windows.Forms.Application.Exit();
            }
            // Hayır denirse hiçbir şey yapma (form açık kalır)
        }
        else if (e.CloseReason == CloseReason.UserClosing)
        {
            // Normal kapatma (X butonu) - tepsiye küçült
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }
    }

    /// <summary>
    /// Zamanlayıcı her tetiklendiğinde (100ms) kayıt süresini günceller.
    /// </summary>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        _elapsedTime = _elapsedTime.Add(TimeSpan.FromMilliseconds(100));
        lblTime.Text = _elapsedTime.ToString(@"hh\:mm\:ss");
    }

    /// <summary>
    /// Kayıt durumuna göre UI elemanı görünürlüğü, renk ve metinlerini günceller.
    /// </summary>
    /// <remarks>
    /// Kayıt aktifken: Başlat butonları gizlenir, Durdur butonu kırmızı olur.
    /// Kayıt yokken: Durdur butonu gizlenir, Başlat butonları görünür.
    /// </remarks>
    private void UpdateUI()
    {
        bool isRecording = _controller.CurrentState == Domain.Enums.RecorderState.Recording;

        // Görünürlük ve Aktiflik Durumu
        btnStartScreen.Visible = !isRecording;
        btnStartAudio.Visible = !isRecording;
        btnStop.Visible = isRecording;
        btnStop.Enabled = isRecording;
        chkSystemAudio.Enabled = !isRecording;
        chkMicrophone.Enabled = !isRecording;

        lblStatus.Text = isRecording ? "Kayıt yapılıyor..." : "Hazır";
        lblStatus.ForeColor = isRecording ? Color.FromArgb(255, 85, 85) : Color.FromArgb(0, 255, 127); // Kırmızı / Bahar Yeşili

        // Durdur butonu vurgusu ve düzeni
        if (isRecording)
        {
            btnStop.BackColor = Color.FromArgb(255, 60, 60); // Kırmızı
            btnStop.ForeColor = Color.White;
            btnStop.Text = "KAYDI DURDUR";
            btnStop.Width = 160; // Uzun metin için daha geniş
        }
        else
        {
            btnStop.BackColor = Color.FromArgb(35, 35, 50); // Koyu
            btnStop.ForeColor = Color.DimGray;
            btnStop.Text = "DURDUR";
            btnStop.Width = 120; // Standart genişlik
        }

        chkSystemAudio.Checked = _settings.CaptureSystemAudio;
        chkMicrophone.Checked = _settings.CaptureMicrophone;

        CenterButtons();
    }

    /// <summary>
    /// Görünür butonları pnlControls içinde yatay olarak ortalar.
    /// </summary>
    /// <remarks>
    /// Form yüklenirken ve yeniden boyutlandırıldığında otomatik çağrılır.
    /// </remarks>
    private void CenterButtons()
    {
        // Simge durumundaysa veya görünür değilse hesaplama yapma
        if (this.WindowState == FormWindowState.Minimized || !this.Visible) return;

        var visibleButtons = new List<Button>();
        if (btnStartScreen.Visible) visibleButtons.Add(btnStartScreen);
        if (btnStartAudio.Visible) visibleButtons.Add(btnStartAudio);
        if (btnStop.Visible) visibleButtons.Add(btnStop);

        if (visibleButtons.Count == 0) return;

        int gap = 20; // Daha iyi görünüm için biraz daha geniş boşluk
        int totalWidth = visibleButtons.Sum(b => b.Width) + (visibleButtons.Count - 1) * gap;
        int startX = (pnlControls.Width - totalWidth) / 2;

        // Negatif olmamasını sağla (beklenmez ama güvenlik için)
        if (startX < 0) startX = 0;

        int currentX = startX;
        foreach (var btn in visibleButtons)
        {
            btn.Location = new Point(currentX, (pnlControls.Height - btn.Height) / 2);
            currentX += btn.Width + gap;
        }
    }

    /// <summary>
    /// Butonlara hover (fare üzerine gelince) efekti ekler.
    /// </summary>
    private void SetupButtonStyles()
    {
        // Hover (üzerine gelme) efektleri
        btnStartScreen.FlatStyle = FlatStyle.Flat;
        btnStartScreen.FlatAppearance.BorderSize = 0;
        btnStartScreen.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 100); // Açık Kırmızı

        btnStartAudio.FlatStyle = FlatStyle.Flat;
        btnStartAudio.FlatAppearance.BorderSize = 0;
        btnStartAudio.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 160, 255); // Açık Mavi

        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.FlatAppearance.BorderSize = 0;
        // Durdur butonu hover efekti Enabled durumuna göre yönetilir
    }

    /// <summary>
    /// Kayıt başladığında çağrılır. Zamanlayıcıyı başlatır ve bildirim gösterir.
    /// </summary>
    private void OnRecordingStarted(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnRecordingStarted(sender, e));
            return;
        }

        _elapsedTime = TimeSpan.Zero;
        _timer.Start();
        UpdateUI();
        UpdateTrayMenuState(true);
        this.WindowState = FormWindowState.Minimized;

        if (_settings.ShowNotifications)
        {
            string title = _controller.Settings.IsAudioOnly ? "Ses Kaydı Başladı" : "Kayıt Başladı";
            string text = _controller.Settings.IsAudioOnly ? "Ses kaydı devam ediyor." : "Ekran kaydı devam ediyor.";
            _trayIcon?.ShowBalloonTip(2000, title, text, ToolTipIcon.Info);
        }
    }

    /// <summary>
    /// Kayıt bittiğinde çağrılır. Dosya kaydetme işlemlerini yönetir.
    /// </summary>
    /// <param name="sender">Event kaynağı</param>
    /// <param name="filePath">Kaydedilen dosyanın yolu</param>
    /// <remarks>
    /// ConfirmSaveOnStop ayarı aktifse SaveFileDialog gösterir.
    /// Kullanıcı iptal ederse dosyayı siler.
    /// </remarks>
    private void OnRecordingEnded(object? sender, string filePath)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnRecordingEnded(sender, filePath));
            return;
        }

        _timer.Stop();
        UpdateUI();
        UpdateTrayMenuState(false);
        ShowForm();

        if (!File.Exists(filePath))
        {
            Views.SkoprMessageBox.Show("Kayıt dosyası oluşturulamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_settings.ConfirmSaveOnStop)
        {
            using var saveDialog = new SaveFileDialog
            {
                FileName = Path.GetFileName(filePath),
                DefaultExt = Path.GetExtension(filePath),
                Filter = Path.GetExtension(filePath).ToLowerInvariant() == ".mp3"
                    ? "MP3 Audio (*.mp3)|*.mp3"
                    : "MPEG-4 Video (*.mp4)|*.mp4",
                InitialDirectory = Path.GetDirectoryName(filePath) ?? _settings.OutputFolder
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string destinationPath = saveDialog.FileName;
                if (!string.Equals(filePath, destinationPath, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (File.Exists(destinationPath)) File.Delete(destinationPath);
                        File.Move(filePath, destinationPath);
                        filePath = destinationPath;
                    }
                    catch (Exception ex)
                    {
                        Views.SkoprMessageBox.Show($"Dosya taşınırken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (_settings.ShowNotifications)
                {
                    _trayIcon?.ShowBalloonTip(2000, "Tamamlandı", $"Kaydedildi: {filePath}", ToolTipIcon.Info);
                }
            }
            else
            {
                try { File.Delete(filePath); } catch { }
            }
        }
        else if (_settings.ShowNotifications)
        {
            _trayIcon?.ShowBalloonTip(2000, "Tamamlandı", $"Kaydedildi: {filePath}", ToolTipIcon.Info);
        }
    }

    /// <summary>
    /// Ekran kaydını başlatır (video + ses).
    /// </summary>
    private async void StartScreenRecording()
    {
        try
        {
            await _controller.StartRecordingAsync(audioOnly: false);
        }
        catch (Exception ex)
        {
            Views.SkoprMessageBox.Show($"Kayıt başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log.Error(ex, "Ekran kaydı başlatılamadı.");
        }
    }

    /// <summary>
    /// Sadece ses kaydını başlatır (MP3).
    /// </summary>
    private async void StartAudioRecording()
    {
        try
        {
            await _controller.StartRecordingAsync(audioOnly: true);
        }
        catch (Exception ex)
        {
            Views.SkoprMessageBox.Show($"Ses kaydı başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log.Error(ex, "Ses kaydı başlatılamadı.");
        }
    }

    /// <summary>
    /// Devam eden kaydı durdurur.
    /// </summary>
    private async void StopRecording()
    {
        try
        {
            await _controller.StopRecordingAsync();
        }
        catch (Exception ex)
        {
            Views.SkoprMessageBox.Show($"Kayıt durdurulamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log.Error(ex, "Kayıt durdurulamadı.");
        }
    }

    /// <summary>
    /// Çıkış sırasında manuel kaydetme işlemi için yardımcı metod.
    /// Event mekanizması çıkışta güvenilir olmadığı için doğrudan çağrılır.
    /// </summary>
    private void HandleManualSave(string filePath)
    {
        try
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var filter = ext == ".mp3" ? "MP3 Audio (*.mp3)|*.mp3" : "MPEG-4 Video (*.mp4)|*.mp4";

            using var saveDialog = new SaveFileDialog
            {
                FileName = Path.GetFileName(filePath),
                DefaultExt = ext,
                Filter = filter,
                InitialDirectory = Path.GetDirectoryName(filePath) ?? _settings.OutputFolder
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string destinationPath = saveDialog.FileName;
                if (!string.Equals(filePath, destinationPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(destinationPath)) File.Delete(destinationPath);
                    File.Move(filePath, destinationPath);
                }
            }
            else
            {
                try { File.Delete(filePath); } catch { }
            }
        }
        catch { /* Çıkış sırasında hata olursa yoksay */ }
    }

    /// <summary>
    /// Ayarlar formunu açar.
    /// </summary>
    private void OpenSettings()
    {
        var settingsClone = _settings.Clone();
        using var settingsForm = new SettingsForm(settingsClone);

        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            // Kopya üzerindeki değişiklikleri asıl ayar nesnesine uygula
            _settings = settingsClone;
            _settingsService.Save(_settings);
            _controller.Settings = _settings;
            SetupGlobalHotkey(); // Kısayol tuşlarını güncelle
            UpdateUI();
        }
    }

    /// <summary>
    /// FFmpeg kurulumunu kontrol eder ve gerekirse yükler.
    /// </summary>
    private async Task CheckAndInstallFfmpegAsync()
    {
        try
        {
            if (!FfmpegHelper.IsInstalled())
            {
                Log.Warning("FFmpeg bulunamadı, kullanıcıya indirme seçeneği sunuluyor.");

                await Task.Delay(1000); // UI'ın yüklenmesini bekle

                Invoke(() =>
                {
                    var result = Views.SkoprMessageBox.Show(
                        "FFmpeg bulunamadı. Video kaydı için FFmpeg gereklidir.\n\n" +
                        "Şimdi otomatik olarak indirilsin mi? (yaklaşık 100MB)",
                        "FFmpeg Gerekli",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        _ = Task.Run(async () =>
                        {
                            var progress = new Progress<string>(msg => Log.Information($"FFmpeg: {msg}"));
                            var success = await FfmpegHelper.DownloadAndInstallAsync(progress);

                            Invoke(() =>
                            {
                                if (!success)
                                {
                                    Views.SkoprMessageBox.Show(
                                        "FFmpeg indirilemedi. Lütfen manuel olarak kurun:\n" +
                                        "https://ffmpeg.org/download.html",
                                        "Hata",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                                }
                                else
                                {
                                    Log.Information("FFmpeg başarıyla indirildi ve kuruldu.");
                                    Views.SkoprMessageBox.Show("FFmpeg başarıyla kuruldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            });
                        });
                    }
                });
            }
            else
            {
                Log.Information("FFmpeg zaten kurulu.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "FFmpeg kontrolü sırasında hata oluştu.");
        }
    }

    // Event handler'lar - Buton tıklamalarını ilgili metodlara yönlendirir
    private void btnStartScreen_Click(object sender, EventArgs e) => StartScreenRecording();
    private void btnStartAudio_Click(object sender, EventArgs e) => StartAudioRecording();
    private void btnStop_Click(object sender, EventArgs e) => StopRecording();
    private void btnSettings_Click(object sender, EventArgs e) => OpenSettings();

    private void chkSystemAudio_CheckedChanged(object sender, EventArgs e)
    {
        _settings.CaptureSystemAudio = chkSystemAudio.Checked;
        _settingsService.Save(_settings);
        _controller.Settings = _settings;
    }

    private void chkMicrophone_CheckedChanged(object sender, EventArgs e)
    {
        _settings.CaptureMicrophone = chkMicrophone.Checked;
        _settingsService.Save(_settings);
        _controller.Settings = _settings;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hotkeyService?.Dispose();
            _timer?.Dispose();
            _trayIcon?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}

using Hardcodet.Wpf.TaskbarNotification;
using SkoprRecord.App.Services;
using SkoprRecord.App.ViewModels;
using SkoprRecord.App.Views;
using SkoprRecord.Application.Services;
using SkoprRecord.Domain.Enums;
using SkoprRecord.Domain.Models;
using System.Windows;
using System.Windows.Input;

namespace SkoprRecord.App;

/// <summary>
/// Uygulamanın ana penceresi. Kullanıcı arayüzü olaylarını, sistem tepsisi (tray) etkileşimlerini
/// ve global kısayol tuşlarını yönetir.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly SettingsService _settingsService;
    private GlobalHotkeyService? _hotkeyService;
    private RecordingSettings _settings;
    private TaskbarIcon? _trayIcon;
    private System.Windows.Controls.MenuItem? _startStopScreenItem;
    private System.Windows.Controls.MenuItem? _startStopAudioItem;
    private System.Windows.Controls.MenuItem? _stopRecordingItem;
    private System.Windows.Controls.MenuItem? _systemAudioMenuItem;
    private System.Windows.Controls.MenuItem? _micMenuItem;
    private bool _isExiting = false;

    public MainWindow(MainWindowViewModel viewModel, SettingsService settingsService)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        _viewModel = viewModel;
        _settingsService = settingsService;
        DataContext = viewModel;

        // Ayarları yükle ve ViewModel'e aktar
        _settings = _settingsService.Load();
        _viewModel.CaptureSystemAudio = _settings.CaptureSystemAudio;
        _viewModel.CaptureMicrophone = _settings.CaptureMicrophone;

        // viewModel.Controller.Settings = _settings is enough.
        _viewModel.Controller.Settings = _settings;

        // Sistem tepsisi (System Tray) kurulumu
        SetupTrayIcon();

        // Window event'lerine abone ol
        this.Closing += MainWindow_Closing;
        this.StateChanged += OnStateChanged;
        Loaded += OnLoaded;
        Closed += OnClosed;
        MouseLeftButtonDown += OnMouseLeftButtonDown;

    }

    /// <summary>
    /// ViewModel üzerindeki ses ayarları değiştikçe kayıt ayarlarını günceler ve diske yazar.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.CaptureSystemAudio))
        {
            _settings.CaptureSystemAudio = _viewModel.CaptureSystemAudio;
            _viewModel.Controller.Settings = _settings;
            _settingsService.Save(_settings);
            UpdateAudioMenuItemsDisplay();
        }
        else if (e.PropertyName == nameof(_viewModel.CaptureMicrophone))
        {
            _settings.CaptureMicrophone = _viewModel.CaptureMicrophone;
            _viewModel.Controller.Settings = _settings;
            _settingsService.Save(_settings);
            UpdateAudioMenuItemsDisplay();
        }

    }

    /// <summary>
    /// Tray menüsündeki ses ayarları metinlerini günceller.
    /// </summary>
    private void UpdateAudioMenuItemsDisplay()
    {
        if (_systemAudioMenuItem != null)
        {
            _systemAudioMenuItem.Header = _settings.CaptureSystemAudio ? "🔊 Sistem Sesi: Aktif" : "🔇 Sistem Sesi: Kapalı";
        }
        if (_micMenuItem != null)
        {
            _micMenuItem.Header = _settings.CaptureMicrophone ? "🎤 Mikrofon: Aktif" : "🎤 Mikrofon: Kapalı";
        }
    }

    /// <summary>
    /// Pencere simge durumuna küçültüldüğünde tepsiye saklar.
    /// </summary>
    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            if (_settings.ShowNotifications)
            {
                _trayIcon?.ShowBalloonTip("Skopr Record", "Uygulama arka planda çalışıyor. Ctrl+Shift+R ile kayıt başlatabilirsiniz.", BalloonIcon.Info);
            }
        }
    }

    /// <summary>
    /// Pencere kapatılırken kayıt kontrolü yapar.
    /// </summary>
    private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // If recording is active, ask for confirmation
        if (_viewModel.Controller.CurrentState == RecorderState.Recording)
        {
            e.Cancel = true; // Cancel the close temporarily

            if (_isExiting) return; // Prevent multiple dialogs
            _isExiting = true;

            var result = Views.SkoprMessageBox.Show(
                "Kayıt devam ediyor. Kaydı durdurup uygulamayı kapatmak istiyor musunuz?",
                "Kayıt Devam Ediyor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Kaydı durdur ve dosya yolunu al
                var filePath = await _viewModel.Controller.StopRecordingAsync(suppressEvent: true);

                // Eğer dosya kaydedildiyse ve ayar aktifse, kaydetme penceresini manuel göster
                if (!string.IsNullOrEmpty(filePath) && _settings.ConfirmSaveOnStop)
                {
                    HandleManualSave(filePath);
                }

                // Şimdi gerçekten kapat
                _settingsService.Save(_settings);
                _trayIcon?.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                _isExiting = false;
            }
        }
        else if (WindowState != WindowState.Minimized)
        {
            // Normal close (X button) when not recording - minimize to tray
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }

    /// <summary>
    /// Sistem tepsisi (Tray) simgesini ve sağ tık menüsünü oluşturur.
    /// </summary>
    private void SetupTrayIcon()
    {
        var iconUri = new Uri("pack://application:,,,/Assets/app_icon.ico");
        var iconStream = System.Windows.Application.GetResourceStream(iconUri)?.Stream;

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Skopr Record",
            Visibility = Visibility.Visible
        };

        if (iconStream != null) _trayIcon.Icon = new System.Drawing.Icon(iconStream);

        var contextMenu = new System.Windows.Controls.ContextMenu();

        var showItem = new System.Windows.Controls.MenuItem { Header = "📺 Göster" };
        showItem.Click += (s, e) => ShowWindow();
        contextMenu.Items.Add(showItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        _startStopScreenItem = new System.Windows.Controls.MenuItem { Header = "🔴 Ekran Kaydı Başlat" };
        _startStopScreenItem.Click += (s, e) =>
        {
            if (_viewModel.StartScreenRecordingCommand.CanExecute(null)) _viewModel.StartScreenRecordingCommand.Execute(null);
        };
        contextMenu.Items.Add(_startStopScreenItem);

        _startStopAudioItem = new System.Windows.Controls.MenuItem { Header = "🎵 Ses Kaydı Başlat" };
        _startStopAudioItem.Click += (s, e) =>
        {
            if (_viewModel.StartAudioRecordingCommand.CanExecute(null)) _viewModel.StartAudioRecordingCommand.Execute(null);
        };
        contextMenu.Items.Add(_startStopAudioItem);

        _stopRecordingItem = new System.Windows.Controls.MenuItem { Header = "⏹️ Kaydı Durdur", Visibility = Visibility.Collapsed };
        _stopRecordingItem.Click += (s, e) =>
        {
            if (_viewModel.StopRecordingCommand.CanExecute(null)) _viewModel.StopRecordingCommand.Execute(null);
        };
        contextMenu.Items.Add(_stopRecordingItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        // Hızlı ses kanalı kontrolleri
        _systemAudioMenuItem = new System.Windows.Controls.MenuItem { Header = _settings.CaptureSystemAudio ? "🔊 Sistem Sesi: Aktif" : "🔇 Sistem Sesi: Kapalı" };
        _systemAudioMenuItem.Click += (s, e) => _viewModel.CaptureSystemAudio = !_viewModel.CaptureSystemAudio;
        contextMenu.Items.Add(_systemAudioMenuItem);

        _micMenuItem = new System.Windows.Controls.MenuItem { Header = _settings.CaptureMicrophone ? "🎤 Mikrofon: Aktif" : "🎤 Mikrofon: Kapalı" };
        _micMenuItem.Click += (s, e) => _viewModel.CaptureMicrophone = !_viewModel.CaptureMicrophone;
        contextMenu.Items.Add(_micMenuItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var settingsItem = new System.Windows.Controls.MenuItem { Header = "⚙️ Ayarlar" };
        settingsItem.Click += (s, e) => OpenSettings();
        contextMenu.Items.Add(settingsItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "❌ Çıkış" };
        exitItem.Click += async (s, e) =>
        {
            if (_isExiting) return;

            // Kayıt devam ediyorsa onay iste
            if (_viewModel.Controller.CurrentState == RecorderState.Recording)
            {
                _isExiting = true;
                var result = Views.SkoprMessageBox.Show(
                    "Kayıt devam ediyor. Kaydı durdurup uygulamayı kapatmak istiyor musunuz?",
                    "Kayıt Devam Ediyor",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Kaydı durdur ve dosya yolunu al (Event tetiklemeyi bastır, manuel yöneteceğiz)
                    var filePath = await _viewModel.Controller.StopRecordingAsync(suppressEvent: true);

                    // Eğer dosya kaydedildiyse ve ayar aktifse, kaydetme penceresini manuel göster
                    if (!string.IsNullOrEmpty(filePath) && _settings.ConfirmSaveOnStop)
                    {
                        HandleManualSave(filePath);
                    }

                    _settingsService.Save(_settings);
                    _trayIcon?.Dispose();
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    _isExiting = false;
                }
            }
            else
            {
                _settingsService.Save(_settings);
                _trayIcon?.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        };
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenu = contextMenu;
        _trayIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();
    }

    // Bu method artık kullanılmıyor, lambda içinde halledildi.

    /// <summary>
    /// Kayıt durumuna göre tepsi simgesindeki metinleri günceller.
    /// </summary>
    private void UpdateTrayMenuState(bool isRecording)
    {
        if (isRecording)
        {
            if (_startStopScreenItem != null)
            {
                _startStopScreenItem.IsEnabled = false;
                _startStopScreenItem.Header = "🔴 Ekran Kaydı (Kayıt Devam Ediyor)";
            }
            if (_startStopAudioItem != null)
            {
                _startStopAudioItem.IsEnabled = false;
                _startStopAudioItem.Header = "🎵 Ses Kaydı (Kayıt Devam Ediyor)";
            }
            if (_stopRecordingItem != null)
            {
                _stopRecordingItem.Visibility = Visibility.Visible;
            }
        }
        else
        {
            if (_startStopScreenItem != null)
            {
                _startStopScreenItem.IsEnabled = true;
                _startStopScreenItem.Header = "🔴 Ekran Kaydı Başlat";
            }
            if (_startStopAudioItem != null)
            {
                _startStopAudioItem.IsEnabled = true;
                _startStopAudioItem.Header = "🎵 Ses Kaydı Başlat";
            }
            if (_stopRecordingItem != null)
            {
                _stopRecordingItem.Visibility = Visibility.Collapsed;
            }
        }

        if (_trayIcon != null) _trayIcon.ToolTipText = isRecording ? "Skopr Record - Kayıt yapılıyor..." : "Skopr Record";
    }

    private void OpenSettings()
    {
        ShowWindow();
        SettingsButton_Click(this, new RoutedEventArgs());
    }

    /// <summary>
    /// Pencereyi normal boyuta getirir ve öne çıkarır.
    /// </summary>
    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    /// <summary>
    /// Pencereyi sürüklemek için sol tık olayını işler.
    /// </summary>
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }



    /// <summary>
    /// Pencere yüklendiğinde servisleri başlatır ve gerekirse tepsiye küçültür.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetupGlobalHotkey();

        _viewModel.Controller.RecordingStarted += OnRecordingStarted;
        _viewModel.Controller.RecordingEnded += OnRecordingEnded;

        if (_settings.StartInTray)
        {
            WindowState = WindowState.Minimized;
            Hide();
        }
    }

    /// <summary>
    /// Kayıt başladığında arayüzü gizler ve bildirim gösterir.
    /// </summary>
    private void OnRecordingStarted(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            Hide();
            UpdateTrayMenuState(true);
            if (_settings.ShowNotifications)
            {
                string title = _viewModel.Controller.Settings.IsAudioOnly ? "Ses Kaydı Başladı" : "Kayıt Başladı";
                string body = _viewModel.Controller.Settings.IsAudioOnly
                    ? "Ses kaydı devam ediyor. Durdurmak için Ctrl+Shift+R basın."
                    : "Ekran kaydı devam ediyor. Durdurmak için Ctrl+Shift+R basın.";
                _trayIcon?.ShowBalloonTip(title, body, BalloonIcon.Info);
            }
        });
    }

    /// <summary>
    /// Kayıt bittiğinde dosyayı kaydetme veya farklı kaydetme işlemlerini yönetir.
    /// </summary>
    private void OnRecordingEnded(object? sender, string filePath)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrayMenuState(false);
            ShowWindow();

            if (!System.IO.File.Exists(filePath))
            {
                Views.SkoprMessageBox.Show("Kayıt dosyası oluşturulamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_settings.ConfirmSaveOnStop)
            {
                var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                var filter = ext == ".mp3" ? "MP3 Audio (*.mp3)|*.mp3" : "MPEG-4 Video (*.mp4)|*.mp4";

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = System.IO.Path.GetFileName(filePath),
                    DefaultExt = ext,
                    Filter = filter,
                    InitialDirectory = System.IO.Path.GetDirectoryName(filePath) ?? _settings.OutputFolder
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string destinationPath = saveFileDialog.FileName;
                    if (!string.Equals(filePath, destinationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                            System.IO.File.Move(filePath, destinationPath);
                        }
                        catch (Exception ex) { MessageBox.Show($"Dosya taşınırken hata: {ex.Message}"); }
                    }
                    if (_settings.ShowNotifications) _trayIcon?.ShowBalloonTip("Tamamlandı", $"Kaydedildi: {destinationPath}", BalloonIcon.Info);
                }
                else { try { System.IO.File.Delete(filePath); } catch { } }
            }
            else if (_settings.ShowNotifications)
            {
                _trayIcon?.ShowBalloonTip("Tamamlandı", $"Kaydedildi: {filePath}", BalloonIcon.Info);
            }
        });
    }

    /// <summary>
    /// Global kısayolları kaydeder/günceller.
    /// </summary>
    private void SetupGlobalHotkey()
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
    }

    private void OnScreenRecordingRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_viewModel.Controller.CurrentState != RecorderState.Recording && _viewModel.StartScreenRecordingCommand.CanExecute(null))
                _viewModel.StartScreenRecordingCommand.Execute(null);
        });
    }

    private void OnAudioRecordingRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_viewModel.Controller.CurrentState != RecorderState.Recording && _viewModel.StartAudioRecordingCommand.CanExecute(null))
                _viewModel.StartAudioRecordingCommand.Execute(null);
        });
    }

    private void OnStopRecordingRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_viewModel.Controller.CurrentState == RecorderState.Recording && _viewModel.StopRecordingCommand.CanExecute(null))
                _viewModel.StopRecordingCommand.Execute(null);
        });
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
    }

    /// <summary>
    /// Küçültme (-) butonuna tıklandığında pencereyi simge durumuna getirir.
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Kapat (X) butonuna tıklandığında çıkış sürecini başlatır.
    /// </summary>
    private async void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isExiting) return;

        // Kayıt devam ediyorsa onay iste
        if (_viewModel.Controller.CurrentState == RecorderState.Recording)
        {
            _isExiting = true;
            var result = Views.SkoprMessageBox.Show(
                "Kayıt devam ediyor. Kaydı durdurup uygulamayı kapatmak istiyor musunuz?",
                "Kayıt Devam Ediyor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Kaydı durdur ve dosya yolunu al
                var filePath = await _viewModel.Controller.StopRecordingAsync(suppressEvent: true);

                // Eğer dosya kaydedildiyse ve ayar aktifse, kaydetme penceresini manuel göster
                if (!string.IsNullOrEmpty(filePath) && _settings.ConfirmSaveOnStop)
                {
                    HandleManualSave(filePath);
                }

                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                _isExiting = false;
            }
        }
        else
        {
            System.Windows.Application.Current.Shutdown();
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
            var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            var filter = ext == ".mp3" ? "MP3 Audio (*.mp3)|*.mp3" : "MPEG-4 Video (*.mp4)|*.mp4";

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = System.IO.Path.GetFileName(filePath),
                DefaultExt = ext,
                Filter = filter,
                InitialDirectory = System.IO.Path.GetDirectoryName(filePath) ?? _settings.OutputFolder
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string destinationPath = saveFileDialog.FileName;
                if (!string.Equals(filePath, destinationPath, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                        System.IO.File.Move(filePath, destinationPath);
                    }
                    catch (Exception ex) { MessageBox.Show($"Dosya taşınırken hata: {ex.Message}"); }
                }
            }
            else
            {
                try { System.IO.File.Delete(filePath); } catch { }
            }
        }
        catch { /* Çıkış sırasında hata olursa yoksay */ }
    }



    /// <summary>
    /// Ayarlar butonuna tıklandığında ayarlar penceresini açar.
    /// </summary>
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsClone = _settings.Clone();
        var settingsWindow = new SettingsWindow(settingsClone);
        settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        if (settingsWindow.ShowDialog() == true)
        {
            _settings = settingsClone;
            _settingsService.Save(_settings);
            _viewModel.CaptureSystemAudio = _settings.CaptureSystemAudio;
            _viewModel.CaptureMicrophone = _settings.CaptureMicrophone;
            _viewModel.Controller.Settings = _settings;
            SetupGlobalHotkey(); // Kısayol tuşlarını güncelle
        }
    }
}

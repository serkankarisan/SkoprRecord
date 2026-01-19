using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Input;
using SkoprRecord.App.Services;
using SkoprRecord.App.ViewModels;
using SkoprRecord.App.Views;
using SkoprRecord.Application.Services;
using SkoprRecord.Domain.Models;

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
    private System.Windows.Controls.MenuItem? _systemAudioMenuItem;
    private System.Windows.Controls.MenuItem? _micMenuItem;

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

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        Loaded += OnLoaded;
        Closed += OnClosed;
        StateChanged += OnStateChanged;
        MouseLeftButtonDown += OnMouseLeftButtonDown;

        // Sistem tepsisi (System Tray) kurulumu
        SetupTrayIcon();
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
        _startStopScreenItem.Click += (s,e) => 
        {
            if (_viewModel.StartScreenRecordingCommand.CanExecute(null)) _viewModel.StartScreenRecordingCommand.Execute(null);
        };
        contextMenu.Items.Add(_startStopScreenItem);

        _startStopAudioItem = new System.Windows.Controls.MenuItem { Header = "🎵 Ses Kaydı Başlat" };
        _startStopAudioItem.Click += (s,e) =>
        {
             if (_viewModel.StartAudioRecordingCommand.CanExecute(null)) _viewModel.StartAudioRecordingCommand.Execute(null);
        };
        contextMenu.Items.Add(_startStopAudioItem);

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
        exitItem.Click += (s, e) =>
        {
            _settingsService.Save(_settings);
            _trayIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
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
        // Kayıt sırasında "Başlat" butonlarını "Durdur" a çeviriyoruz. 
        // Ekran ve Ses butonlarını tek bir "Durdur" butonuna çevirmek veya disable etmek gerek.
        // Basitlik için: Kayıt varsa ikisini de gizleyip "Durdur" ekleyebiliriz ama mevcut objeler üzerinden gidelim.
        
        if (isRecording)
        {
             if (_startStopScreenItem != null) _startStopScreenItem.Visibility = Visibility.Collapsed;
             if (_startStopAudioItem != null) _startStopAudioItem.Header = "⏹️ Kaydı Durdur";
             if (_startStopAudioItem != null) _startStopAudioItem.Click -= OnStopClick; // Avoid double sub
             if (_startStopAudioItem != null) _startStopAudioItem.Click += OnStopClick;
             // Audio item'ı geçici durdurma butonu olarak kullanalım.
        }
        else
        {
             if (_startStopScreenItem != null) { _startStopScreenItem.Visibility = Visibility.Visible; _startStopScreenItem.Header = "🔴 Ekran Kaydı Başlat"; }
             if (_startStopAudioItem != null) { _startStopAudioItem.Visibility = Visibility.Visible; _startStopAudioItem.Header = "🎵 Ses Kaydı Başlat"; }
             // Click eventlerini resetlemek gerekir ama lambda ile ekledik.
             // Daha temiz bir yapı kuralım: SetupTrayIcon'u her durum değişiminde yenilemek yerine, 
             // menü durumunu yönetmek daha doğru. Ancak şimdilik basitçe tooltip güncelleyelim.
        }

        if (_trayIcon != null) _trayIcon.ToolTipText = isRecording ? "Skopr Record - Kayıt yapılıyor..." : "Skopr Record";
    }

    private void OnStopClick(object sender, RoutedEventArgs e)
    {
         if (_viewModel.StopRecordingCommand.CanExecute(null)) _viewModel.StopRecordingCommand.Execute(null);
    }

    private void OpenSettings()
    {
        ShowWindow();
        SettingsButton_Click(this, new RoutedEventArgs());
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hotkeyService = new GlobalHotkeyService(this);
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

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
                _trayIcon?.ShowBalloonTip("Kayıt Başladı", "Ekran kaydı devam ediyor. Durdurmak için Ctrl+Shift+R basın.", BalloonIcon.Info);
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
                MessageBox.Show("Kayıt dosyası oluşturulamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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

    /// <summary> Global kısayol tuşuna basıldığında kaydı başlatır (Varsayılan: Ekran Kaydı) veya durdurur. </summary>
    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_viewModel.StopRecordingCommand.CanExecute(null)) 
            {
                _viewModel.StopRecordingCommand.Execute(null);
            }
            else if (_viewModel.StartScreenRecordingCommand.CanExecute(null)) 
            {
                _viewModel.StartScreenRecordingCommand.Execute(null);
            }
        });
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsClone = _settings.Clone();
        var settingsWindow = new SettingsWindow(settingsClone);
        // Explicitly center and show independently to avoid anchoring to MainWindow's offset
        settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        if (settingsWindow.ShowDialog() == true)
        {
            _settings = settingsClone;
            _settingsService.Save(_settings);
            _viewModel.CaptureSystemAudio = _settings.CaptureSystemAudio;
            _viewModel.CaptureMicrophone = _settings.CaptureMicrophone;
            _viewModel.Controller.Settings = _settings;
        }
    }
}

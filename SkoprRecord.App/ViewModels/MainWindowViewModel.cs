using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SkoprRecord.App.Services;
using SkoprRecord.App.Views;
using SkoprRecord.Application.Interfaces;
using SkoprRecord.Domain.Enums;
using SkoprRecord.Application.Helpers;

namespace SkoprRecord.App.ViewModels;


/// <summary>
/// Ana pencerenin veri bağlama (data binding) ve kullanıcı etkileşimi mantığını yöneten ViewModel.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IRecorderController _controller;

    /// <summary> Kayıt kontrolcüsüne erişim sağlar. </summary>
    public IRecorderController Controller => _controller;

    private DispatcherTimer? _timer;
    private DateTime _recordingStartTime;

    /// <summary> Arayüzde gösterilen durum metni (Hazır, Kaydediyor vb.). </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartScreenRecordingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartAudioRecordingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopRecordingCommand))]
    private string _statusText = "Hazır";

    /// <summary> Olası hata mesajlarını ekrana yansıtmak için kullanılır. </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary> Kayıt süresini (00:00:00) ekranda gösterir. </summary>
    [ObservableProperty]
    private string _elapsedTime = "00:00:00";

    /// <summary> Sistem sesinin kaydedilip edilmeyeceği ayarı. </summary>
    [ObservableProperty]
    private bool _captureSystemAudio = true;

    /// <summary> Mikrofondan ses alınıp alınmayacağı ayarı. </summary>
    [ObservableProperty]
    private bool _captureMicrophone = false;

    /// <summary> Kullanılabilir monitörlerin listesi. </summary>
    [ObservableProperty]
    private List<DisplayInfo> _availableMonitors = new();

    /// <summary> Seçili monitör. </summary>
    [ObservableProperty]
    private DisplayInfo? _selectedMonitor;

    partial void OnSelectedMonitorChanged(DisplayInfo? value)
    {
        if (value != null)
        {
            _controller.Settings.SelectedMonitorHandle = value.Handle;
        }
    }

    /// <summary> Monitör seçimi aktif mi? (Kayıt modundan bağımsız, idle iken aktif) </summary>
    public bool CanSelectMonitor => CanChangeSettings;

    /// <summary> Monitör seçim alanının görünürlüğü. Tek monitör varsa gizlenir. </summary>
    [ObservableProperty]
    private System.Windows.Visibility _monitorSelectionVisibility = System.Windows.Visibility.Collapsed;

    // -----------------------------

    public MainWindowViewModel(IRecorderController controller)
    {
        _controller = controller;
        _controller.StateChanged += OnStateChanged;
        
        // Mevcut ayarları yükle
        _captureSystemAudio = _controller.Settings.CaptureSystemAudio;
        _captureMicrophone = _controller.Settings.CaptureMicrophone;

        UpdateStatus();

        // Monitörleri listele
        RefreshMonitors();

        // Kayıt süresi sayacı kurulumu
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
    }

    private void RefreshMonitors()
    {
        var monitors = MonitorEnumerationHelper.GetMonitors();

        if (monitors.Count > 1)
        {
            MonitorSelectionVisibility = System.Windows.Visibility.Visible;
            AvailableMonitors = monitors;
            
            // Varsayılan olarak Birincil Ekranı seç
            var primaryMonitor = AvailableMonitors.FirstOrDefault(m => m.IsPrimary);
            SelectedMonitor = primaryMonitor ?? AvailableMonitors.First();
        }
        else
        {
            // Tek monitör varsa gizle ve onu seç
            MonitorSelectionVisibility = System.Windows.Visibility.Collapsed;
            AvailableMonitors = monitors;
            SelectedMonitor = monitors.FirstOrDefault();
        }
    }

    /// <summary>
    /// Her saniye tetiklenerek kayıt süresini günceller.
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.Now - _recordingStartTime;
        ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
    }

    /// <summary>
    /// Kayıt durumuna göre Timer'ı başlatır veya durdurur.
    /// </summary>
    private void OnStateChanged(object? sender, RecorderState e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateStatus();

            if (e == RecorderState.Recording)
            {
                _recordingStartTime = DateTime.Now;
                ElapsedTime = "00:00:00";
                _timer?.Start();
            }
            else
            {
                _timer?.Stop();
            }
        });
    }

    /// <summary>
    /// Durum metnini ve butonların tıklanabilirlik durumlarını günceller.
    /// </summary>
    private void UpdateStatus()
    {
        StatusText = _controller.CurrentState switch
        {
            RecorderState.Idle => "Hazır",
            RecorderState.Starting => "Başlatılıyor...",
            RecorderState.Recording => "Kaydediyor",
            RecorderState.Stopping => "Durduruluyor...",
            RecorderState.Error => "Hata",
            _ => _controller.CurrentState.ToString()
        };

        OnPropertyChanged(nameof(StatusColorBrush));
        OnPropertyChanged(nameof(CanChangeSettings));
        OnPropertyChanged(nameof(CanSelectMonitor));
        StartScreenRecordingCommand.NotifyCanExecuteChanged();
        StartAudioRecordingCommand.NotifyCanExecuteChanged();
        StopRecordingCommand.NotifyCanExecuteChanged();
    }

    /// <summary> Duruma göre arayüzdeki ikon rengini belirler. </summary>
    public System.Windows.Media.Brush StatusColorBrush
    {
        get
        {
            return _controller.CurrentState switch
            {
                RecorderState.Recording => System.Windows.Media.Brushes.Red,
                RecorderState.Starting => System.Windows.Media.Brushes.Orange,
                RecorderState.Error => System.Windows.Media.Brushes.DarkRed,
                _ => System.Windows.Media.Brushes.Gray
            };
        }
    }

    /// <summary> Kayıt sırasında bazı ayarların değiştirilmesini engellemek için kontrol. </summary>
    public bool CanChangeSettings => _controller.CurrentState == RecorderState.Idle;

    /// <summary>
    /// Ekran kaydını başlatan komut.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartScreenRecording()
    {
        try
        {
            ErrorMessage = "";
            await _controller.StartRecordingAsync(audioOnly: false);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Başlatma Hatası: {ex.Message}";
        }
    }

    /// <summary>
    /// Sadece ses kaydını başlatan komut.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAudioRecording()
    {
        try
        {
            ErrorMessage = "";
            await _controller.StartRecordingAsync(audioOnly: true);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ses Kaydı Başlatma Hatası: {ex.Message}";
        }
    }

    private bool CanStart() => _controller.CurrentState == RecorderState.Idle || _controller.CurrentState == RecorderState.Error;

    /// <summary>
    /// Kayıt işlemini durduran komut.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopRecording()
    {
        try
        {
            await _controller.StopRecordingAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Durdurma Hatası: {ex.Message}";
        }
    }

    private bool CanStop() => _controller.CurrentState == RecorderState.Recording;
}


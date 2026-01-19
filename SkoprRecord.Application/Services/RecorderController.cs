using SkoprRecord.Application.Helpers;
using SkoprRecord.Application.Interfaces;
using SkoprRecord.Domain.Enums;
using SkoprRecord.Domain.Interfaces;
using SkoprRecord.Domain.Models;
using System.Diagnostics;

namespace SkoprRecord.Application.Services;

/// <summary>
/// Kayıt sürecini yöneten ana kontrolcü sınıfı. 
/// Görüntü yakalama, video kodlama ve ses kaydı bileşenlerini birleştirir.
/// </summary>
public class RecorderController : IRecorderController
{
    private readonly ICaptureSource _captureSource;
    private readonly IVideoEncoder _videoEncoder;
    private readonly FileNamingService _fileNamingService;
    private readonly IAudioRecorder _audioRecorder;

    private RecorderState _currentState = RecorderState.Idle;
    private int _frameCount = 0;
    private DateTime _startTime;
    private string? _currentOutputPath;

    /// <summary> Kayıt durumu değiştiğinde tetiklenir. </summary>
    public event EventHandler<RecorderState>? StateChanged;

    /// <summary> Kayıt fiilen başladığında tetiklenir. </summary>
    public event EventHandler? RecordingStarted;

    /// <summary> Kayıt bittiğinde ve nihai video dosyası oluşturulduğunda tetiklenir. </summary>
    public event EventHandler<string>? RecordingEnded;

    /// <summary> Kayıt ayarları. </summary>
    public RecordingSettings Settings { get; set; } = new();

    /// <summary> Anlık kayıt durumu. </summary>
    public RecorderState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                StateChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary> Tamamlanan son kaydın dosya yolu. </summary>
    public string? LastRecordingPath => _currentOutputPath;

    /// <summary> Sistem sesinin anlık sessize alınıp alınmadığı (Ses kaydedicisine iletilir). </summary>
    public bool IsSystemAudioMuted
    {
        get => _audioRecorder.IsSystemAudioMuted;
        set => _audioRecorder.IsSystemAudioMuted = value;
    }

    /// <summary> Mikrofonun anlık sessize alınıp alınmadığı (Ses kaydedicisine iletilir). </summary>
    public bool IsMicMuted
    {
        get => _audioRecorder.IsMicMuted;
        set => _audioRecorder.IsMicMuted = value;
    }

    public RecorderController(
        ICaptureSource captureSource,
        IVideoEncoder videoEncoder,
        FileNamingService fileNamingService,
        IAudioRecorder audioRecorder)
    {
        _captureSource = captureSource;
        _videoEncoder = videoEncoder;
        _fileNamingService = fileNamingService;
        _audioRecorder = audioRecorder;

        _captureSource.FrameArrived += OnFrameArrived;
    }

    /// <summary>
    /// Kayıt işlemini başlatır. Donanım ve dosyaları hazırlar.
    /// </summary>
    /// <summary>
    /// Kayıt işlemini başlatır. Donanım ve dosyaları hazırlar.
    /// </summary>
    /// <summary>
    /// Kayıt işlemini başlatır. Donanım ve dosyaları hazırlar.
    /// </summary>
    /// <param name="audioOnly">Sadece ses kaydı mı yapılacak?</param>
    public async Task StartRecordingAsync(bool audioOnly)
    {
        if (CurrentState != RecorderState.Idle && CurrentState != RecorderState.Error)
            return;

        try
        {
            // Modu güncelle
            Settings.IsAudioOnly = audioOnly;

            CurrentState = RecorderState.Starting;
            _frameCount = 0;
            _startTime = DateTime.Now;

            // Çıktı klasörünü belirle
            var outputFolder = !string.IsNullOrEmpty(Settings.OutputFolder)
                ? Settings.OutputFolder
                : _fileNamingService.OutputFolder;

            if (!System.IO.Directory.Exists(outputFolder))
            {
                System.IO.Directory.CreateDirectory(outputFolder);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            // Sadece ses için .mp3, video için .mp4 uzantısı kullan
            var ext = Settings.IsAudioOnly ? ".mp3" : ".mp4";
            _currentOutputPath = System.IO.Path.Combine(outputFolder, $"Kayit_{timestamp}{ext}");

            // Sadece ses kaydı modu
            if (Settings.IsAudioOnly)
            {
                // Ses kaydını başlat (en az biri açık olmalı)
                if (Settings.CaptureSystemAudio || Settings.CaptureMicrophone)
                {
                    _audioRecorder.StartRecording(Settings.CaptureSystemAudio, Settings.CaptureMicrophone);
                    CurrentState = RecorderState.Recording;
                    RecordingStarted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    throw new Exception("En az bir ses kaynağı (Sistem veya Mikrofon) seçili olmalıdır.");
                }
            }
            // Video + Ses modu
            else
            {
                // Video kodlayıcıyı hazırla
                _videoEncoder.CaptureSystemAudio = Settings.CaptureSystemAudio;
                _videoEncoder.CaptureMicrophone = Settings.CaptureMicrophone;

                _videoEncoder.Initialize(
                    _currentOutputPath,
                    Settings.Width,
                    Settings.Height,
                    Settings.Fps
                );

                // Ses kaydını başlat (ayarlar aktifse)
                if (Settings.CaptureSystemAudio || Settings.CaptureMicrophone)
                {
                    _audioRecorder.StartRecording(Settings.CaptureSystemAudio, Settings.CaptureMicrophone);
                }

                // Görüntü yakalamayı başlat (Seçili monitör ile)
                var targetMonitor = Settings.SelectedMonitorHandle;

                await _captureSource.StartCaptureAsync(targetMonitor);

                CurrentState = RecorderState.Recording;
                RecordingStarted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception)
        {
            CurrentState = RecorderState.Error;
            throw;
        }
    }

    /// <summary>
    /// Kayıt işlemini durdurur, kaynakları serbest bırakır ve dosya birleştirmelerini yapar.
    /// </summary>
    /// <summary>
    /// Kayıt işlemini durdurur, kaynakları serbest bırakır ve dosya birleştirmelerini yapar.
    /// </summary>
    public async Task<string?> StopRecordingAsync(bool suppressEvent = false)
    {
        if (CurrentState != RecorderState.Recording) return null;

        try
        {
            CurrentState = RecorderState.Stopping;

            // Durdurma işlemi mantığı
            // Kaynakları durdur ve bekle

            // Ses kaydını durdur ve geçici dosyaları al
            var audioResult = _audioRecorder.StopRecording();
            var audioFiles = new System.Collections.Generic.List<string>();
            if (audioResult.SystemAudioPath != null) audioFiles.Add(audioResult.SystemAudioPath);
            if (audioResult.MicAudioPath != null) audioFiles.Add(audioResult.MicAudioPath);

            if (Settings.IsAudioOnly)
            {
                // -- SADECE SES --
                // WAV'ları FFmpeg ile MP3'e veya tek dosya WAV'a dönüştür/birleştir.
                // Basitlik için mixleyip MP3 yapalım.
                if (_currentOutputPath != null && audioFiles.Count > 0)
                {
                    await ConvertToMp3Async(audioFiles, _currentOutputPath);
                }
            }
            else
            {
                // -- VIDEO MODU --
                // Görüntü yakalamayı durdur
                await _captureSource.StopCaptureAsync();

                // Video ve ses dosyalarını FFmpeg ile birleştir
                await _videoEncoder.FinalizeAsync(audioFiles.ToArray());
            }

            if (_currentOutputPath != null && System.IO.File.Exists(_currentOutputPath))
            {
                if (!suppressEvent)
                {
                    RecordingEnded?.Invoke(this, _currentOutputPath);
                }
                return _currentOutputPath; // Dosya yolunu dön
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Kayıt durdurma hatası: {ex.Message}");
            return null;
        }
        finally
        {
            CurrentState = RecorderState.Idle;
        }
    }

    private async Task ConvertToMp3Async(List<string> inputs, string output)
    {
        // FFmpeg komutu: -i input1 -i input2 -filter_complex amix=inputs=2:duration=longest output.mp3
        var ffmpeg = FfmpegHelper.GetFfmpegPath();
        if (ffmpeg == null) return;

        var args = "";
        foreach (var inp in inputs) args += $"-i \"{inp}\" ";

        if (inputs.Count > 1)
        {
            args += $"-filter_complex amix=inputs={inputs.Count}:duration=longest ";
        }

        args += $"\"{output}\" -y";

        var psi = new ProcessStartInfo
        {
            FileName = ffmpeg,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        if (proc != null)
        {
            await proc.WaitForExitAsync();
        }

        // Geçici wavları temizle
        foreach (var f in inputs)
        {
            try { System.IO.File.Delete(f); } catch { }
        }
    }

    /// <summary>
    /// Yakalama kaynağından yeni bir kare geldiğinde çağrılır.
    /// </summary>
    private void OnFrameArrived(object? sender, object frame)
    {
        if (CurrentState != RecorderState.Recording)
        {
            if (frame is IDisposable d) d.Dispose();
            return;
        }

        _frameCount++;

        try
        {
            // Zaman damgasını hesapla ve encoder'a gönder
            var elapsed = DateTime.Now - _startTime;
            _videoEncoder.EncodeFrame(frame, elapsed);
        }
        catch (Exception)
        {
            // Frame işleme hataları burada yutulur veya loglanır
        }
        finally
        {
            // Frame her durumda dispose edilmelidir
            if (frame is IDisposable d) d.Dispose();
        }
    }
}


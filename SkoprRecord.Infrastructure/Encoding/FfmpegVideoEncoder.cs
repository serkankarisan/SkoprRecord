using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkoprRecord.Domain.Interfaces;
using SkoprRecord.Domain.Models;
using SkoprRecord.Application.Helpers;

namespace SkoprRecord.Infrastructure.Encoding;

/// <summary>
/// FFmpeg tabanlı video kodlayıcı sınıfı.
/// Yakalanan video karelerini standart giriş (stdin) üzerinden FFmpeg işlemine aktarır.
/// Kayıt bitiminde video ve sesleri birleştirerek nihai MP4 dosyasını oluşturur.
/// </summary>
public class FfmpegVideoEncoder : IVideoEncoder
{
    private Process? _ffmpegProcess;
    private Stream? _stdin;
    private int _width;
    private int _height;
    private bool _isRecording;
    private readonly object _lock = new();
    private string? _outputPath;
    private string? _tempVideoPath;

    /// <summary> Sistem sesi kaydedilsin mi? </summary>
    public bool CaptureSystemAudio { get; set; } = true;

    /// <summary> Mikrofon sesi kaydedilsin mi? </summary>
    public bool CaptureMicrophone { get; set; } = false;

    /// <summary>
    /// Kodlayıcıyı yapılandırır ve FFmpeg işlemini arka planda başlatır.
    /// </summary>
    public void Initialize(string outputPath, int width, int height, int fps)
    {
        _outputPath = outputPath;
        _width = width;
        _height = height;

        // Kayıt sırasında kilitlenmeleri önlemek için önce geçici bir video doyası oluşturulur
        _tempVideoPath = Path.Combine(Path.GetTempPath(), $"rec_{Guid.NewGuid()}.mp4");

        // FFmpeg yolunu kontrol et
        var ffmpegPath = FfmpegHelper.GetFfmpegPath();
        if (string.IsNullOrEmpty(ffmpegPath))
        {
            throw new InvalidOperationException("FFmpeg bulunamadı!");
        }

        // FFmpeg komut satırı argümanlarını hazırla
        var args = BuildFfmpegArgs(_tempVideoPath, width, height, fps);

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            _ffmpegProcess = Process.Start(startInfo);
            if (_ffmpegProcess == null)
            {
                throw new Exception("FFmpeg başlatılamadı.");
            }

            _stdin = _ffmpegProcess.StandardInput.BaseStream;
            _isRecording = true;

            // Hata çıktılarını oku
            _ffmpegProcess.ErrorDataReceived += (s, e) => { };
            _ffmpegProcess.BeginErrorReadLine();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// FFmpeg için gerekli başlangıç komutlarını oluşturur.
    /// </summary>
    private string BuildFfmpegArgs(string outputPath, int width, int height, int fps)
    {
        // Video girişi: stdin üzerinden ham BGRA verisi alıyoruz
        var args = $"-y -f rawvideo -pix_fmt bgra -s {width}x{height} -r {fps} -i - ";

        // Video kodlama ayarları (libx264, ultrafast preset en düşük gecikme için)
        args += "-c:v libx264 -preset ultrafast -crf 23 -pix_fmt yuv420p ";

        args += $"\"{outputPath}\"";
        return args;
    }

    /// <summary>
    /// Gelen ham bayt verisini FFmpeg'in standart girişine yazar.
    /// </summary>
    public void EncodeFrame(object frameObj, TimeSpan timestamp)
    {
        if (!_isRecording || _stdin == null)
        {
            return;
        }

        if (frameObj is byte[] bytes && bytes.Length > 0)
        {
            try
            {
                lock (_lock)
                {
                    if (_stdin.CanWrite)
                    {
                        _stdin.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception)
            {
                // Yazma hatası durumunda işlem devam eder
            }
        }
    }

    /// <summary>
    /// Kodlamayı bitirir, FFmpeg işlemini kapatır ve varsa sesleri birleştirir (muxing).
    /// </summary>
    public async Task FinalizeAsync(string[]? audioFiles = null)
    {
        _isRecording = false;

        // Stdin'i kapatarak FFmpeg'e verinin bittiğini bildir
        if (_stdin != null)
        {
            try
            {
                lock (_lock)
                {
                    _stdin.Flush();
                    _stdin.Close();
                }
            }
            catch { }
            _stdin = null;
        }

        // İşlemin bitmesini bekle
        if (_ffmpegProcess != null)
        {
            try
            {
                await _ffmpegProcess.WaitForExitAsync();
            }
            catch { }

            _ffmpegProcess.Dispose();
            _ffmpegProcess = null;
        }

        // Üretilen video dosyasını nihai konuma taşı ve sesle birleştir
        if (!string.IsNullOrEmpty(_tempVideoPath) && File.Exists(_tempVideoPath))
        {
            var fileInfo = new FileInfo(_tempVideoPath);

            if (fileInfo.Length > 10 * 1024) // Minimum geçerli boyut kontrolü (10KB)
            {
                if (audioFiles != null && audioFiles.Length > 0)
                {
                    await MuxAudioAsync(_tempVideoPath, audioFiles, _outputPath!);
                }
                else
                {
                    if (File.Exists(_outputPath)) File.Delete(_outputPath);
                    File.Move(_tempVideoPath, _outputPath!);
                }
            }
            else
            {
                try { File.Delete(_tempVideoPath); } catch { }
            }
        }
    }

    /// <summary>
    /// FFmpeg kullanarak video ve harici ses dosyalarını tek bir MP4'te birleştirir.
    /// </summary>
    private async Task MuxAudioAsync(string tempVideoPath, string[] audioFiles, string finalOutputPath)
    {
        var ffmpegPath = FfmpegHelper.GetFfmpegPath();
        if (string.IsNullOrEmpty(ffmpegPath)) return;

        string inputs = $"-i \"{tempVideoPath}\" ";
        string filter = "";
        string maps = "-map 0:v ";

        int audioCount = 0;
        foreach (var audio in audioFiles)
        {
            if (File.Exists(audio))
            {
                inputs += $"-i \"{audio}\" ";
                audioCount++;
            }
        }

        if (audioCount == 0)
        {
            if (File.Exists(finalOutputPath)) File.Delete(finalOutputPath);
            File.Move(tempVideoPath, finalOutputPath);
            return;
        }

        if (audioCount == 1)
        {
            maps += "-map 1:a ";
        }
        else if (audioCount > 1)
        {
            string inputTags = "";
            for (int i = 1; i <= audioCount; i++)
            {
                inputTags += $"[{i}:a]";
            }
            // Ses kanallarını tek bir yayında birleştir
            filter = $"-filter_complex \"{inputTags}amix=inputs={audioCount}[aout]\" ";
            maps += "-map \"[aout]\" ";
        }

        string muxedTempPath = Path.Combine(Path.GetTempPath(), $"muxed_{Guid.NewGuid()}.mp4");
        string args = $"-y {inputs} {filter} {maps} -c:v copy -c:a aac -b:a 192k -shortest \"{muxedTempPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && File.Exists(muxedTempPath))
            {
                if (File.Exists(finalOutputPath)) File.Delete(finalOutputPath);
                File.Move(muxedTempPath, finalOutputPath);

                // Geçici dosyaları temizle
                try
                {
                    File.Delete(tempVideoPath);
                    foreach (var a in audioFiles) if (File.Exists(a)) File.Delete(a);
                }
                catch { }
            }
            else
            {
                // Mux başarısızsa sadece videoyu koru
                if (File.Exists(finalOutputPath)) File.Delete(finalOutputPath);
                File.Move(tempVideoPath, finalOutputPath);
            }
        }
    }

    public void Dispose()
    {
        FinalizeAsync().GetAwaiter().GetResult();
    }
}


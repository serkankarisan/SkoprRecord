using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkoprRecord.Domain.Interfaces;
using SkoprRecord.Domain.Models;

namespace SkoprRecord.Infrastructure.Encoding;

/// <summary>
/// FFmpeg bulunamadığında veya test senaryolarında kullanılan sahte video kodlayıcı.
/// Dosya üretmez, sadece gelen verileri tüketiyormuş gibi davranır.
/// </summary>
public class DummyVideoEncoder : IVideoEncoder
{
    private int _frameCount = 0;

    /// <summary> Sistem sesi kaydedilsin mi? </summary>
    public bool CaptureSystemAudio { get; set; }

    /// <summary> Mikrofon sesi kaydedilsin mi? </summary>
    public bool CaptureMicrophone { get; set; }

    /// <summary> sahte başlatma işlemi yapıldığında log düşer. </summary>
    public void Initialize(string outputPath, int width, int height, int fps)
    {
        _frameCount = 0;
    }

    /// <summary>
    /// Kareleri sink eder (yok sayar). Frame IDisposable ise dispose eder.
    /// </summary>
    public void EncodeFrame(object frame, TimeSpan timestamp)
    {
        _frameCount++;
        if (frame is IDisposable d)
        {
            d.Dispose();
        }
    }

    /// <summary>
    /// Kodlamayı sonlandırmış gibi davranır.
    /// </summary>
    public Task FinalizeAsync(string[]? audioFiles = null)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Temizlenecek kaynak yok
    }
}


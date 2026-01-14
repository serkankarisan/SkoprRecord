using SkoprRecord.Domain.Models;

namespace SkoprRecord.Domain.Interfaces;

/// <summary>
/// Ses yakalama kaynağı arayüzü.
/// Hem sistem sesi (loopback) hem de mikrofon yakalama sınıfları için temel oluşturur.
/// </summary>
public interface IAudioCaptureSource : IDisposable
{
    /// <summary>
    /// Yeni ses verisi geldiğinde tetiklenir.
    /// </summary>
    event EventHandler<AudioDataEventArgs> DataAvailable;

    /// <summary>
    /// Yakalanan sesin format bilgisi.
    /// </summary>
    AudioFormat Format { get; }

    /// <summary>
    /// Ses yakalamayı başlatır.
    /// </summary>
    Task StartCaptureAsync();

    /// <summary>
    /// Ses yakalamayı durdurur.
    /// </summary>
    Task StopCaptureAsync();
}

/// <summary>
/// Ses verisi olay argümanları. Ham bayt dizisini içerir.
/// </summary>
public class AudioDataEventArgs : EventArgs
{
    /// <summary> Ses verisi tamponu. </summary>
    public byte[] Buffer { get; }

    /// <summary> Tamponda yazılı olan geçerli bayt sayısı. </summary>
    public int BytesRecorded { get; }

    public AudioDataEventArgs(byte[] buffer, int bytesRecorded)
    {
        Buffer = buffer;
        BytesRecorded = bytesRecorded;
    }
}

/// <summary>
/// Ses formatı özelliklerini tanımlayan sınıf.
/// </summary>
public class AudioFormat
{
    /// <summary> Örnekleme hızı (Hz). Varsayılan 44100. </summary>
    public int SampleRate { get; set; } = 44100;

    /// <summary> Kanal sayısı (1=Mono, 2=Stereo). </summary>
    public int Channels { get; set; } = 2;

    /// <summary> Örnek başına bit derinliği. Varsayılan 16. </summary>
    public int BitsPerSample { get; set; } = 16;
}


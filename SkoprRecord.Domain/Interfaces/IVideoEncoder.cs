namespace SkoprRecord.Domain.Interfaces;

/// <summary>
/// Video kodlama (encoding) işlemlerini yöneten arayüz.
/// Yakalanan kareleri ve sesleri birleştirerek nihai video dosyasını oluşturur.
/// </summary>
public interface IVideoEncoder : IDisposable
{
    /// <summary> Sistem sesinin yakalanıp yakalanmayacağı bilgisi. </summary>
    bool CaptureSystemAudio { get; set; }

    /// <summary> Mikrofon sesinin yakalanıp yakalanmayacağı bilgisi. </summary>
    bool CaptureMicrophone { get; set; }

    /// <summary>
    /// Kodlayıcıyı yapılandırır.
    /// </summary>
    /// <param name="outputPath">Video dosyasının kaydedileceği yol.</param>
    /// <param name="width">Video genişliği.</param>
    /// <param name="height">Video yüksekliği.</param>
    /// <param name="fps">Saniyedeki kare sayısı.</param>
    void Initialize(string outputPath, int width, int height, int fps);

    /// <summary>
    /// Bir video karesini kodlama kuyruğuna ekler.
    /// </summary>
    /// <param name="frame">Kodlanacak kare nesnesi (Direct3D yüzeyi vb.).</param>
    /// <param name="timestamp">Karenin zaman damgası.</param>
    void EncodeFrame(object frame, TimeSpan timestamp);

    /// <summary>
    /// Kodlama işlemini bitirir ve dosyayı kapatır.
    /// Varsa ses dosyalarını video ile birleştirir (muxing).
    /// </summary>
    /// <param name="audioFiles">Videoya eklenecek geçici ses dosyaları listesi.</param>
    Task FinalizeAsync(string[]? audioFiles = null);
}


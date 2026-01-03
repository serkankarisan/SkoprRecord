namespace SkoprRecord.Domain.Models;

/// <summary>
/// Ses kaydı tamamlandığında oluşturulan geçici dosya yollarını tutan sınıf.
/// </summary>
public class AudioRecordingResult
{
    /// <summary> Geçici sistem sesi (loopback) dosyasının yolu. </summary>
    public string? SystemAudioPath { get; set; }

    /// <summary> Geçici mikrofon sesi dosyasının yolu. </summary>
    public string? MicAudioPath { get; set; }
}


namespace SkoprRecord.Domain.Enums;

/// <summary>
/// Kaydedicinin çalışma durumlarını temsil eden enum.
/// </summary>
public enum RecorderState
{
    /// <summary> Boşta, kayıt yapılmıyor. </summary>
    Idle,
    /// <summary> Kayıt başlatılıyor. </summary>
    Starting,
    /// <summary> Kayıt aktif olarak devam ediyor. </summary>
    Recording,
    /// <summary> Kayıt durduruluyor ve dosyalar işleniyor. </summary>
    Stopping,
    /// <summary> Bir hata oluştu. </summary>
    Error
}


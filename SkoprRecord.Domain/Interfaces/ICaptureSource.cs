namespace SkoprRecord.Domain.Interfaces;

/// <summary>
/// Ekran yakalama kaynağı arayüzü.
/// Windows Graphics Capture veya diğer yöntemler için temel oluşturur.
/// </summary>
public interface ICaptureSource : IDisposable
{
    /// <summary>
    /// Yeni bir video karesi (frame) geldiğinde tetiklenir.
    /// Event argümanı olarak ham frame verisini (platforma bağlı nesne) döner.
    /// </summary>
    event EventHandler<object> FrameArrived;

    /// <summary>
    /// Ekran yakalamayı başlatır.
    /// </summary>
    /// <param name="monitorHandle">Yakalanacak monitörün handle değeri (IntPtr.Zero ise varsayılan/birincil).</param>
    Task StartCaptureAsync(IntPtr monitorHandle = default);

    /// <summary>
    /// Ekran yakalamayı durdurur.
    /// </summary>
    Task StopCaptureAsync();
}


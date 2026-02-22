namespace SkoprRecord.Domain.Models;

/// <summary>
/// Uygulamanın kayıt ve genel ayarlarını tutan model sınıfı.
/// JSON olarak serileştirilip diske kaydedilebilir.
/// </summary>
public class RecordingSettings
{
    // --- Video Ayarları ---

    /// <summary> Kayıt genişliği (varsayılan 1920). </summary>
    public int Width { get; set; } = 1920;

    /// <summary> Kayıt yüksekliği (varsayılan 1080). </summary>
    public int Height { get; set; } = 1080;

    /// <summary> Saniyedeki kare sayısı (Varsayılan 30). </summary>
    public int Fps { get; set; } = 30;

    /// <summary> Video bit hızı (bps). </summary>
    public int Bitrate { get; set; } = 4_000_000;

    /// <summary> Videoların kaydedileceği klasör yolu. </summary>
    public string OutputFolder { get; set; } = "";

    // --- Ses Ayarları ---

    /// <summary> Sistem sesinin kaydedilip kaydedilmeyeceği. </summary>
    public bool CaptureSystemAudio { get; set; } = true;

    /// <summary> Mikrofon sesinin kaydedilip kaydedilmeyeceği. </summary>
    public bool CaptureMicrophone { get; set; } = false;

    /// <summary> Ses örnekleme hızı (örneğin 44100). </summary>
    public int AudioSampleRate { get; set; } = 44100;

    /// <summary> Ses kanalı sayısı (1=Mono, 2=Stereo). </summary>
    public int AudioChannels { get; set; } = 2;

    /// <summary> Sadece ses kaydı yapılsın (video devre dışı). (Geçici, kaydedilmez) </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsAudioOnly { get; set; } = false;

    /// <summary> Seçili monitörün handle değeri (Serileştirilmez). </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public IntPtr SelectedMonitorHandle { get; set; } = IntPtr.Zero;

    // --- Genel Uygulama Ayarları ---

    /// <summary> Kayıt durdurulduğunda farklı kaydet diyaloğu gösterilsin mi? </summary>
    public bool ConfirmSaveOnStop { get; set; } = true;

    /// <summary> Uygulama bildirimleri (Balloon tips) gösterilsin mi? </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary> Uygulama başlangıçta sistem tepsisinde (tray) mi başlasın? </summary>
    public bool StartInTray { get; set; } = false;

    // --- Kısayol (Hotkey) Ayarları ---

    /// <summary> Ekran kaydı başlatma kısayolu için değiştirici tuşlar (Örn: Alt + Shift -> 1 | 4 = 5) </summary>
    public uint HotkeyScreenMods { get; set; } = 0x0001 | 0x0004; // Alt (1) + Shift (4)

    /// <summary> Ekran kaydı başlatma kısayolu tuşu (Örn: R) </summary>
    public uint HotkeyScreenKey { get; set; } = 0x52; // R tuşu

    /// <summary> Ses kaydı başlatma kısayolu için değiştirici tuşlar </summary>
    public uint HotkeyAudioMods { get; set; } = 0x0001 | 0x0004; // Alt + Shift

    /// <summary> Ses kaydı başlatma kısayolu tuşu (Örn: A) </summary>
    public uint HotkeyAudioKey { get; set; } = 0x41; // A tuşu

    /// <summary> Kaydı durdurma kısayolu için değiştirici tuşlar </summary>
    public uint HotkeyStopMods { get; set; } = 0x0001 | 0x0004; // Alt + Shift

    /// <summary> Kaydı durdurma kısayolu tuşu (Örn: S) </summary>
    public uint HotkeyStopKey { get; set; } = 0x53; // S tuşu

    /// <summary>
    /// Ayar nesnesinin yüzeysel bir kopyasını oluşturur.
    /// </summary>
    public RecordingSettings Clone()
    {
        return (RecordingSettings)this.MemberwiseClone();
    }
}


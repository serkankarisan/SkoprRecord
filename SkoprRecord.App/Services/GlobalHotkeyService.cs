using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SkoprRecord.App.Services;

/// <summary>
/// Küresel kısayol tuşu (Global Hotkey) yönetimi servisi.
/// Uygulama simge durumundayken veya odağı yokken bile tuş kombinasyonlarını yakalar.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID_SCREEN = 9001;
    private const int HOTKEY_ID_AUDIO = 9002;
    private const int HOTKEY_ID_STOP = 9003;

    private readonly IntPtr _windowHandle;
    private HwndSource? _source;

    private readonly uint _modScreen, _keyScreen;
    private readonly uint _modAudio, _keyAudio;
    private readonly uint _modStop, _keyStop;

    /// <summary> Ekran kaydı kısayolu tuşuna basıldığında tetiklenir. </summary>
    public event EventHandler? ScreenRecordingRequested;
    
    /// <summary> Ses kaydı kısayolu tuşuna basıldığında tetiklenir. </summary>
    public event EventHandler? AudioRecordingRequested;
    
    /// <summary> Kaydı durdurma kısayolu tuşuna basıldığında tetiklenir. </summary>
    public event EventHandler? StopRecordingRequested;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>
    /// Pencereye özel global kısayolları (Ayarlardan gelen) atar.
    /// </summary>
    public GlobalHotkeyService(Window window, uint modScreen, uint keyScreen, uint modAudio, uint keyAudio, uint modStop, uint keyStop)
    {
        _modScreen = modScreen; _keyScreen = keyScreen;
        _modAudio = modAudio;   _keyAudio = keyAudio;
        _modStop = modStop;     _keyStop = keyStop;
        _windowHandle = new WindowInteropHelper(window).Handle;

        if (_windowHandle == IntPtr.Zero)
        {
            window.Loaded += (s, e) =>
            {
                var handle = new WindowInteropHelper(window).Handle;
                RegisterHotkeys(handle);
            };
        }
        else
        {
            RegisterHotkeys(_windowHandle);
        }
    }

    /// <summary>
    /// Belirtilen pencere handle'ı üzerinden Win32 API ile tuş kombinasyonlarını kaydeder.
    /// </summary>
    private void RegisterHotkeys(IntPtr handle)
    {
        _source = HwndSource.FromHwnd(handle);
        _source?.AddHook(HwndHook);

        RegisterHotKey(handle, HOTKEY_ID_SCREEN, (int)_modScreen, (int)_keyScreen);
        RegisterHotKey(handle, HOTKEY_ID_AUDIO, (int)_modAudio, (int)_keyAudio);
        RegisterHotKey(handle, HOTKEY_ID_STOP, (int)_modStop, (int)_keyStop);
    }

    /// <summary>
    /// Windows mesaj kuyruğunu dinleyen kanca (Hook). WM_HOTKEY sinyallerini yakalar.
    /// </summary>
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (id == HOTKEY_ID_SCREEN) ScreenRecordingRequested?.Invoke(this, EventArgs.Empty);
            else if (id == HOTKEY_ID_AUDIO) AudioRecordingRequested?.Invoke(this, EventArgs.Empty);
            else if (id == HOTKEY_ID_STOP) StopRecordingRequested?.Invoke(this, EventArgs.Empty);
            
            handled = (id == HOTKEY_ID_SCREEN || id == HOTKEY_ID_AUDIO || id == HOTKEY_ID_STOP);
        }
        return IntPtr.Zero;
    }

    /// <summary>
    /// Kayıtlı kısayol tuşlarını çözer ve mesaj kancasını kaldırır.
    /// </summary>
    public void Dispose()
    {
        _source?.RemoveHook(HwndHook);
        UnregisterHotKey(_windowHandle, HOTKEY_ID_SCREEN);
        UnregisterHotKey(_windowHandle, HOTKEY_ID_AUDIO);
        UnregisterHotKey(_windowHandle, HOTKEY_ID_STOP);
    }
}


using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace SkoprRecord.App.Services;

/// <summary>
/// Küresel kısayol tuşu (Global Hotkey) yönetimi servisi.
/// Uygulama simge durumundayken veya odağı yokken bile tuş kombinasyonlarını yakalar.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;

    private readonly int _hotkeyId;
    private readonly IntPtr _windowHandle;
    private HwndSource? _source;

    /// <summary> Tanımlanan kısayol tuşuna basıldığında tetiklenir. </summary>
    public event EventHandler? HotkeyPressed;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>
    /// Pencereye özel bir global kısayol atar. Varsayılan olarak Ctrl+Shift+R ('R' = 0x52).
    /// </summary>
    public GlobalHotkeyService(Window window, int virtualKey = 0x52)
    {
        _hotkeyId = GetHashCode();
        _windowHandle = new WindowInteropHelper(window).Handle;

        if (_windowHandle == IntPtr.Zero)
        {
            window.Loaded += (s, e) =>
            {
                var handle = new WindowInteropHelper(window).Handle;
                RegisterHotkeys(handle, virtualKey);
            };
        }
        else
        {
            RegisterHotkeys(_windowHandle, virtualKey);
        }
    }

    /// <summary>
    /// Belirtilen pencere handle'ı üzerinden Win32 API ile tuş kombinasyonunu kaydeder.
    /// </summary>
    private void RegisterHotkeys(IntPtr handle, int virtualKey)
    {
        _source = HwndSource.FromHwnd(handle);
        _source?.AddHook(HwndHook);

        // Ctrl + Shift + R
        RegisterHotKey(handle, _hotkeyId, MOD_CONTROL | MOD_SHIFT, virtualKey);
    }

    /// <summary>
    /// Windows mesaj kuyruğunu dinleyen kanca (Hook). WM_HOTKEY sinyallerini yakalar.
    /// </summary>
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    /// <summary>
    /// Kayıtlı kısayol tuşunu çözer ve mesaj kancasını kaldırır.
    /// </summary>
    public void Dispose()
    {
        _source?.RemoveHook(HwndHook);
        UnregisterHotKey(_windowHandle, _hotkeyId);
    }
}


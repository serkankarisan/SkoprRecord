using System.Runtime.InteropServices;

namespace SkoprRecord.WinForms.Services;

/// <summary>
/// Global hotkey (kısayol tuşu) kaydı ve yönetimi için servis.
/// Ctrl+Shift+R kombinasyonu ile kayıt başlatma/durdurma.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 9000;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Form _form;
    private readonly uint _modifiers;
    private readonly uint _key;
    private bool _isRegistered;

    public event EventHandler? HotkeyPressed;

    /// <summary>
    /// Global hotkey servisi oluşturur.
    /// </summary>
    /// <param name="form">Hotkey mesajlarını alacak form</param>
    /// <param name="virtualKey">Virtual key code (varsayılan: R = 0x52)</param>
    public GlobalHotkeyService(Form form, uint virtualKey = 0x52)
    {
        _form = form;
        _key = virtualKey;
        _modifiers = 0x0002 | 0x0004; // MOD_CONTROL | MOD_SHIFT

        // Form'un WndProc'unu override etmek için NativeWindow kullanıyoruz
        var messageFilter = new HotkeyMessageFilter(this);
        System.Windows.Forms.Application.AddMessageFilter(messageFilter);
    }

    /// <summary>
    /// Kısayol tuşunu sisteme kaydeder.
    /// </summary>
    /// <returns>Başarılıysa true.</returns>
    public bool Register()
    {
        if (_isRegistered) return true;

        _isRegistered = RegisterHotKey(_form.Handle, HOTKEY_ID, _modifiers, _key);
        return _isRegistered;
    }

    /// <summary>
    /// Kısayol tuşu kaydını sistemden siler.
    /// </summary>
    public void Unregister()
    {
        if (!_isRegistered) return;

        UnregisterHotKey(_form.Handle, HOTKEY_ID);
        _isRegistered = false;
    }

    /// <summary>
    /// Kısayol tuşu tetiklendiğinde event fırlatır.
    /// </summary>
    internal void OnHotkeyPressed()
    {
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Kaynakları temizler ve kısayol kaydını siler.
    /// </summary>
    public void Dispose()
    {
        Unregister();
    }

    private class HotkeyMessageFilter : IMessageFilter
    {
        private readonly GlobalHotkeyService _service;

        public HotkeyMessageFilter(GlobalHotkeyService service)
        {
            _service = service;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                _service.OnHotkeyPressed();
                return true;
            }
            return false;
        }
    }
}

using System.Runtime.InteropServices;

namespace SkoprRecord.WinForms.Services;

/// <summary>
/// Global hotkey (kısayol tuşu) kaydı ve yönetimi için servis.
/// Ctrl+Shift+R kombinasyonu ile kayıt başlatma/durdurma.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID_SCREEN = 9001;
    private const int HOTKEY_ID_AUDIO = 9002;
    private const int HOTKEY_ID_STOP = 9003;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Form _form;
    private readonly uint _modScreen, _keyScreen;
    private readonly uint _modAudio, _keyAudio;
    private readonly uint _modStop, _keyStop;
    
    private bool _isRegistered;
    private readonly HotkeyMessageFilter _messageFilter;

    public event EventHandler? ScreenRecordingRequested;
    public event EventHandler? AudioRecordingRequested;
    public event EventHandler? StopRecordingRequested;

    /// <summary>
    /// Global hotkey servisi oluşturur.
    /// </summary>
    public GlobalHotkeyService(Form form, uint modScreen, uint keyScreen, uint modAudio, uint keyAudio, uint modStop, uint keyStop)
    {
        _form = form;
        _modScreen = modScreen;    _keyScreen = keyScreen;
        _modAudio = modAudio;      _keyAudio = keyAudio;
        _modStop = modStop;        _keyStop = keyStop;

        // Form'un WndProc'unu override etmek için NativeWindow kullanıyoruz
        _messageFilter = new HotkeyMessageFilter(this);
        System.Windows.Forms.Application.AddMessageFilter(_messageFilter);
    }

    /// <summary>
    /// Kısayol tuşunu sisteme kaydeder.
    /// </summary>
    /// <returns>Başarılıysa true.</returns>
    public bool Register()
    {
        if (_isRegistered) return true;

        bool success = true;
        success &= RegisterHotKey(_form.Handle, HOTKEY_ID_SCREEN, _modScreen, _keyScreen);
        success &= RegisterHotKey(_form.Handle, HOTKEY_ID_AUDIO, _modAudio, _keyAudio);
        success &= RegisterHotKey(_form.Handle, HOTKEY_ID_STOP, _modStop, _keyStop);
        
        _isRegistered = success;
        return _isRegistered;
    }

    /// <summary>
    /// Kısayol tuşu kaydını sistemden siler.
    /// </summary>
    public void Unregister()
    {
        if (!_isRegistered) return;

        UnregisterHotKey(_form.Handle, HOTKEY_ID_SCREEN);
        UnregisterHotKey(_form.Handle, HOTKEY_ID_AUDIO);
        UnregisterHotKey(_form.Handle, HOTKEY_ID_STOP);
        _isRegistered = false;
    }

    /// <summary>
    /// Kısayol tuşu tetiklendiğinde event fırlatır.
    /// </summary>
    internal void OnHotkeyPressed(int id)
    {
        if (id == HOTKEY_ID_SCREEN) ScreenRecordingRequested?.Invoke(this, EventArgs.Empty);
        else if (id == HOTKEY_ID_AUDIO) AudioRecordingRequested?.Invoke(this, EventArgs.Empty);
        else if (id == HOTKEY_ID_STOP) StopRecordingRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Kaynakları temizler ve kısayol kaydını siler.
    /// </summary>
    public void Dispose()
    {
        System.Windows.Forms.Application.RemoveMessageFilter(_messageFilter);
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
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == HOTKEY_ID_SCREEN || id == HOTKEY_ID_AUDIO || id == HOTKEY_ID_STOP)
                {
                    _service.OnHotkeyPressed(id);
                    return true;
                }
            }
            return false;
        }
    }
}

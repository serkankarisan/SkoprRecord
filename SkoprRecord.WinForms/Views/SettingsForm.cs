using SkoprRecord.Domain.Models;

namespace SkoprRecord.WinForms.Views;

/// <summary>
/// Ayarlar formu - WPF SettingsWindow ile aynı özelliklere sahip
/// </summary>
public partial class SettingsForm : Form
{
    private readonly RecordingSettings _settings;

    public SettingsForm(RecordingSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        LoadAppIcon();

        // Görsel ayırıcı ekle
        var separator = new Panel
        {
            Height = 1,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(60, 60, 80)
        };
        pnlMain.Controls.Add(separator);

        // Dock sırasını ayarla (WinForms Docking için Z-Order yönetimi):
        // 0: Başlık Çubuğu (En Üst)
        // 1: Ayırıcı (Başlık Altı)
        // 2: Navigasyon (Ayırıcı Altı)
        // 3... İçerik
        pnlMain.Controls.SetChildIndex(customTitleBar, 0);
        pnlMain.Controls.SetChildIndex(separator, 1);
        pnlMain.Controls.SetChildIndex(pnlNavigation, 2);

        LoadSettings();
        UpdateNavButtons();
    }

    /// <summary>
    /// Mevcut ayarları forma yükler.
    /// </summary>
    private void LoadSettings()
    {
        // Kayıt Ayarları
        txtOutputFolder.Text = _settings.OutputFolder;
        numFps.Value = _settings.Fps;
        numBitrate.Value = _settings.Bitrate / 1000; // Convert to Kbps

        // Ses Ayarları
        chkCaptureSystemAudio.Checked = _settings.CaptureSystemAudio;
        chkCaptureMicrophone.Checked = _settings.CaptureMicrophone;

        // Genel Ayarlar
        chkStartInTray.Checked = _settings.StartInTray;
        chkShowNotifications.Checked = _settings.ShowNotifications;
        chkConfirmSaveOnStop.Checked = _settings.ConfirmSaveOnStop;

        // Kısayol Ayarları (Mevcut ayarları TextBox'lara yükle)
        txtHotkeyScreen.Text = FormatHotkey(_settings.HotkeyScreenMods, _settings.HotkeyScreenKey);
        txtHotkeyAudio.Text = FormatHotkey(_settings.HotkeyAudioMods, _settings.HotkeyAudioKey);
        txtHotkeyStop.Text = FormatHotkey(_settings.HotkeyStopMods, _settings.HotkeyStopKey);
    }

    /// <summary>
    /// Kısayol değiştirici ve tuş kombinasyonunu string formunda dönüştürür.
    /// </summary>
    private string FormatHotkey(uint modifiers, uint key)
    {
        string modStr = "";
        if ((modifiers & 0x0001) != 0) modStr += "Alt+";
        if ((modifiers & 0x0002) != 0) modStr += "Ctrl+";
        if ((modifiers & 0x0004) != 0) modStr += "Shift+";

        Keys keyCode = (Keys)key;
        string keyStr = keyCode.ToString();

        // Rakamlar
        if (keyStr.StartsWith("D") && keyStr.Length == 2 && char.IsDigit(keyStr[1]))
            keyStr = keyStr.Substring(1);

        return modStr + keyStr;
    }

    /// <summary>
    /// Formdaki değerleri ayar nesnesine kaydeder.
    /// </summary>
    private void SaveSettings()
    {
        _settings.OutputFolder = txtOutputFolder.Text;
        _settings.Fps = (int)numFps.Value;
        _settings.Bitrate = (int)numBitrate.Value * 1000; // Convert from Kbps to bps

        _settings.CaptureSystemAudio = chkCaptureSystemAudio.Checked;
        _settings.CaptureMicrophone = chkCaptureMicrophone.Checked;

        _settings.StartInTray = chkStartInTray.Checked;
        _settings.ShowNotifications = chkShowNotifications.Checked;
        _settings.ConfirmSaveOnStop = chkConfirmSaveOnStop.Checked;

        // Hotkeys are saved immediately into _settings via HotkeyTextBox_KeyDown
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Kayıt klasörünü seçin",
            SelectedPath = _settings.OutputFolder,
            ShowNewFolderButton = true
        };

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            txtOutputFolder.Text = folderDialog.SelectedPath;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        SaveSettings();
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private void btnNav_Click(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            // Panel değiştirme
            pnlTabRecording.Visible = (btn == btnNavRecording);
            pnlTabAudio.Visible = (btn == btnNavAudio);
            pnlTabGeneral.Visible = (btn == btnNavGeneral);

            // Buton stillerini güncelle
            UpdateNavButtons();
        }
    }

    /// <summary>
    /// Navigasyon butonlarının görünümünü seçili sekmeye göre günceller.
    /// </summary>
    private void UpdateNavButtons()
    {
        var buttons = new[] { btnNavRecording, btnNavAudio, btnNavGeneral };

        // Determine current selection
        int selectedIndex = 0;
        if (pnlTabAudio.Visible) selectedIndex = 1;
        if (pnlTabGeneral.Visible) selectedIndex = 2;

        for (int i = 0; i < buttons.Length; i++)
        {
            bool isSelected = selectedIndex == i;
            buttons[i].Font = new Font("Segoe UI", 10F, isSelected ? FontStyle.Bold : FontStyle.Regular);
            buttons[i].ForeColor = isSelected ? Color.White : Color.LightGray;
            buttons[i].BackColor = isSelected ? Color.FromArgb(40, 40, 60) : Color.Transparent;
        }
    }

    private void customTitleBar_CloseRequested(object? sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    /// <summary>
    /// Kısayol TextBox'larına tıklandığında tuş kombinasyonunu yakalar ve ayarlar nesnesine kaydeder.
    /// </summary>
    private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox txtBox) return;

        // Engellemek istediğimiz tuşlar (Sadece Modifier basılmışsa yoksay)
        if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.Menu)
        {
            e.SuppressKeyPress = true;
            return;
        }

        uint modifiers = 0;
        string modStr = "";

        if (e.Alt) { modifiers |= 0x0001; modStr += "Alt+"; }
        if (e.Control) { modifiers |= 0x0002; modStr += "Ctrl+"; }
        if (e.Shift) { modifiers |= 0x0004; modStr += "Shift+"; }

        uint key = (uint)e.KeyCode;
        string keyStr = e.KeyCode.ToString();

        // Rakamlar için D1, D2 vb. yerine sayısal karşılığı
        if (keyStr.StartsWith("D") && keyStr.Length == 2 && char.IsDigit(keyStr[1]))
        {
            keyStr = keyStr.Substring(1);
        }

        string fullStr = modStr + keyStr;
        txtBox.Text = fullStr;

        if (txtBox == txtHotkeyScreen)
        {
            _settings.HotkeyScreenMods = modifiers;
            _settings.HotkeyScreenKey = key;
        }
        else if (txtBox == txtHotkeyAudio)
        {
            _settings.HotkeyAudioMods = modifiers;
            _settings.HotkeyAudioKey = key;
        }
        else if (txtBox == txtHotkeyStop)
        {
            _settings.HotkeyStopMods = modifiers;
            _settings.HotkeyStopKey = key;
        }

        e.SuppressKeyPress = true; // Sisteme gitmesini engelle (Bip sesini vb önler)
    }

    /// <summary>
    /// Uygulama ikonunu yükler.
    /// </summary>
    private void LoadAppIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "app_icon.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }
        }
        catch { }
    }
}

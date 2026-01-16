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

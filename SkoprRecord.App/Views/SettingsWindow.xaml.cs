using Microsoft.Win32;
using SkoprRecord.Domain.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SkoprRecord.App.Views;

/// <summary>
/// Uygulama ayarlarının kullanıcı tarafından değiştirildiği pencere.
/// Video, ses ve genel uygulama tercihlerini yönetir.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary> Düzenlenen ayar nesnesi. </summary>
    public RecordingSettings Settings { get; private set; }

    /// <summary> Ayarların kaydedilip edilmediği bilgisi. </summary>
    public bool IsSaved { get; private set; }

    public SettingsWindow(RecordingSettings currentSettings)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Settings = currentSettings;
        LoadSettings();
    }

    /// <summary>
    /// Mevcut ayarları arayüzdeki kontrollerle (ComboBox, TextBox vb.) eşleştirir.
    /// </summary>
    private void LoadSettings()
    {
        // FPS Ayarı
        FpsCombo.SelectedIndex = Settings.Fps switch
        {
            24 => 0,
            30 => 1,
            60 => 2,
            _ => 1
        };

        // Kalite/Bitrate Ayarı
        QualityCombo.SelectedIndex = Settings.Bitrate switch
        {
            2_000_000 => 0,
            4_000_000 => 1,
            8_000_000 => 2,
            16_000_000 => 3,
            _ => 1
        };

        // Kayıt Klasörü
        OutputPathTextBox.Text = string.IsNullOrEmpty(Settings.OutputFolder)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "SkoprRecord")
            : Settings.OutputFolder;

        // Ses Kanalları
        SystemAudioCheck.IsChecked = Settings.CaptureSystemAudio;
        MicrophoneCheck.IsChecked = Settings.CaptureMicrophone;

        // Genel Tercihler
        ConfirmSaveCheck.IsChecked = Settings.ConfirmSaveOnStop;
        ShowNotificationsCheck.IsChecked = Settings.ShowNotifications;
        StartInTrayCheck.IsChecked = Settings.StartInTray;

        // Kısayollar
        ScreenHotkeyTextBox.Text = FormatHotkey(Settings.HotkeyScreenMods, Settings.HotkeyScreenKey);
        AudioHotkeyTextBox.Text = FormatHotkey(Settings.HotkeyAudioMods, Settings.HotkeyAudioKey);
        StopHotkeyTextBox.Text = FormatHotkey(Settings.HotkeyStopMods, Settings.HotkeyStopKey);
    }

    private string FormatHotkey(uint modifiers, uint key)
    {
        string modStr = "";
        if ((modifiers & 0x0001) != 0) modStr += "Alt+";
        if ((modifiers & 0x0002) != 0) modStr += "Ctrl+";
        if ((modifiers & 0x0004) != 0) modStr += "Shift+";

        // Convert virtual key to string (Using WinForms Keys enum equivalent for simplicity in display, though WPF uses KeyInterop)
        System.Windows.Forms.Keys keyCode = (System.Windows.Forms.Keys)key;
        string keyStr = keyCode.ToString();

        // Rakamlar
        if (keyStr.StartsWith("D") && keyStr.Length == 2 && char.IsDigit(keyStr[1]))
            keyStr = keyStr.Substring(1);

        return modStr + keyStr;
    }

    /// <summary>
    /// Arayüzdeki değerleri ayar nesnesine geri yazar.
    /// </summary>
    private void SaveSettings()
    {
        Settings.Fps = FpsCombo.SelectedIndex switch
        {
            0 => 24,
            1 => 30,
            2 => 60,
            _ => 30
        };

        Settings.Bitrate = QualityCombo.SelectedIndex switch
        {
            0 => 2_000_000,
            1 => 4_000_000,
            2 => 8_000_000,
            3 => 16_000_000,
            _ => 4_000_000
        };

        Settings.OutputFolder = OutputPathTextBox.Text;
        Settings.CaptureSystemAudio = SystemAudioCheck.IsChecked == true;
        Settings.CaptureMicrophone = MicrophoneCheck.IsChecked == true;
        Settings.ConfirmSaveOnStop = ConfirmSaveCheck.IsChecked == true;
        Settings.ShowNotifications = ShowNotificationsCheck.IsChecked == true;
        Settings.StartInTray = StartInTrayCheck.IsChecked == true;

        // Hotkeys are saved immediately into Settings via HotkeyTextBox_PreviewKeyDown
        IsSaved = true;
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not TextBox txtBox) return;

        // Sadece modifier tuşlar basılmışsa engelle
        if (e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift ||
            e.Key == System.Windows.Input.Key.LeftCtrl || e.Key == System.Windows.Input.Key.RightCtrl ||
            e.Key == System.Windows.Input.Key.LeftAlt || e.Key == System.Windows.Input.Key.RightAlt ||
            e.Key == System.Windows.Input.Key.System) // Alt is often System
        {
            e.Handled = true;
            return;
        }

        uint modifiers = 0;
        string modStr = "";

        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt)) { modifiers |= 0x0001; modStr += "Alt+"; }
        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control)) { modifiers |= 0x0002; modStr += "Ctrl+"; }
        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift)) { modifiers |= 0x0004; modStr += "Shift+"; }

        // Convert WPF Key to Win32 Virtual Key Code
        int virtualKey = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key);
        uint key = (uint)virtualKey;
        
        System.Windows.Forms.Keys keyCode = (System.Windows.Forms.Keys)virtualKey;
        string keyStr = keyCode.ToString();

        // Rakamlar
        if (keyStr.StartsWith("D") && keyStr.Length == 2 && char.IsDigit(keyStr[1]))
        {
            keyStr = keyStr.Substring(1);
        }

        txtBox.Text = modStr + keyStr;

        if (txtBox == ScreenHotkeyTextBox)
        {
            Settings.HotkeyScreenMods = modifiers;
            Settings.HotkeyScreenKey = key;
        }
        else if (txtBox == AudioHotkeyTextBox)
        {
            Settings.HotkeyAudioMods = modifiers;
            Settings.HotkeyAudioKey = key;
        }
        else if (txtBox == StopHotkeyTextBox)
        {
            Settings.HotkeyStopMods = modifiers;
            Settings.HotkeyStopKey = key;
        }

        e.Handled = true;
    }

    /// <summary>
    /// Kayıt klasörü seçmek için klasör seçme diyaloğunu açar.
    /// </summary>
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Kayıt Klasörü Seçin"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputPathTextBox.Text = dialog.FolderName;
        }
    }

    /// <summary>
    /// Ayarları onaylar ve pencereyi başarılı sonucuyla kapatır.
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsSaved = false;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        IsSaved = false;
        Close();
    }
}


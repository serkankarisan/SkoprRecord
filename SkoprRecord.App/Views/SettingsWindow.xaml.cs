using Microsoft.Win32;
using SkoprRecord.Domain.Models;
using System.IO;
using System.Windows;

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

        IsSaved = true;
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


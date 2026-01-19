using System.Text.RegularExpressions;
using System.Windows;

namespace SkoprRecord.App.Views;

public partial class DownloadProgressWindow : Window
{
    public DownloadProgressWindow()
    {
        InitializeComponent();
    }

    public void UpdateProgress(string message)
    {
        // Mesajı güncelle
        StatusText.Text = message;

        // Yüzde bilgisini ayıkla (Örn: "İndiriliyor: %45")
        var match = Regex.Match(message, @"%(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int percent))
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = percent;
            DetailText.Text = $"{percent}% Tamamlandı";
        }
        else
        {
            // Yüzde yoksa (örn: "Arşiv çıkartılıyor...") indeterminate moduna geç
            if (!message.Contains("%"))
            {
                ProgressBar.IsIndeterminate = true;
                DetailText.Text = "";
            }
        }
    }
}

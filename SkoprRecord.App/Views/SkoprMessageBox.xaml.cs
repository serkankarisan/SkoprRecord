using System.Windows;

namespace SkoprRecord.App.Views;

/// <summary>
/// Koyu tema uyumlu özel MessageBox penceresi
/// </summary>
public partial class SkoprMessageBox : Window
{
    private MessageBoxResult _result = MessageBoxResult.Cancel;

    public SkoprMessageBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Mesaj kutusunu gösterir ve sonucu döner.
    /// </summary>
    /// <param name="message">Gösterilecek mesaj.</param>
    /// <param name="title">Pencere başlığı.</param>
    /// <param name="buttons">Gösterilecek butonlar (OK, YesNo, OKCancel).</param>
    /// <param name="icon">İkon türü (Kullanılmıyor, şablon uyumluluğu için).</param>
    /// <returns>Kullanıcının seçimi (OK, Cancel, Yes, No).</returns>
    public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
    {
        var msgBox = new SkoprMessageBox();
        msgBox.TitleText.Text = title;
        msgBox.MessageText.Text = message;
        msgBox.SetupButtons(buttons);
        msgBox.ShowDialog();
        return msgBox._result;
    }

    /// <summary>
    /// İstenen buton tipine göre arayüzü ayarlar.
    /// </summary>
    private void SetupButtons(MessageBoxButton buttons)
    {
        switch (buttons)
        {
            case MessageBoxButton.OK:
                Button1.Content = "Tamam";
                Button1.Tag = MessageBoxResult.OK;
                Button2.Visibility = Visibility.Collapsed;
                break;

            case MessageBoxButton.YesNo:
                Button1.Content = "Evet";
                Button1.Tag = MessageBoxResult.Yes;
                Button1.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 85, 85));
                Button1.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                Button1.FontWeight = FontWeights.Bold;

                Button2.Content = "Hayır";
                Button2.Tag = MessageBoxResult.No;
                Button2.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 70));
                Button2.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 176, 176));
                Button2.FontWeight = FontWeights.Normal;
                Button2.Visibility = Visibility.Visible;
                break;

            case MessageBoxButton.OKCancel:
                Button1.Content = "İptal";
                Button1.Tag = MessageBoxResult.Cancel;
                Button1.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 70));
                Button1.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 176, 176));
                Button1.FontWeight = FontWeights.Normal;

                Button2.Content = "Tamam";
                Button2.Tag = MessageBoxResult.OK;
                Button2.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 85, 85));
                Button2.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                Button2.FontWeight = FontWeights.Bold;
                Button2.Visibility = Visibility.Visible;
                break;
        }
    }

    private void Button1_Click(object sender, RoutedEventArgs e)
    {
        _result = (MessageBoxResult)(Button1.Tag ?? MessageBoxResult.OK);
        Close();
    }

    private void Button2_Click(object sender, RoutedEventArgs e)
    {
        _result = (MessageBoxResult)(Button2.Tag ?? MessageBoxResult.Cancel);
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _result = MessageBoxResult.Cancel;
        Close();
    }
}

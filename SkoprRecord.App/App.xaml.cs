using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;
using SkoprRecord.App.Services;
using SkoprRecord.App.ViewModels;
using SkoprRecord.App.Views;
using SkoprRecord.Application.Interfaces;
using SkoprRecord.Application.Services;
using SkoprRecord.Domain.Interfaces;
using SkoprRecord.Infrastructure.Audio;
using SkoprRecord.Infrastructure.Capture;
using SkoprRecord.Infrastructure.Encoding;
using SkoprRecord.Application.Helpers;

namespace SkoprRecord.App;

/// <summary>
/// Uygulamanın ana giriş noktası (Composition Root).
/// Bağımlılık yönetimi (DI), günlükleme (logging) ve pencere yönetimini başlatır.
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Uygulama başlatıldığında tetiklenen olay.
    /// Günlükleme sistemini kurar, bağımlılıkları kaydeder ve ana pencereyi açar.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // 1. Günlükleme Sistemini Kur (Serilog)
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkoprRecord");
        if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
        var logPath = Path.Combine(appDataPath, "logs", "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Skopr Record başlatılıyor...");
        Log.Information("Log dosyası: {Path}", logPath);

        // 2. FFmpeg Gereksinim Kontrolü
        if (!FfmpegHelper.IsInstalled())
        {
            var result = MessageBox.Show(
                "FFmpeg bulunamadı. Video kaydı için FFmpeg gereklidir.\n\n" +
                "Şimdi otomatik olarak indirilsin mi? (yaklaşık 100MB)",
                "FFmpeg Gerekli",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var downloadWindow = new DownloadProgressWindow();
                downloadWindow.Show();

                var progress = new Progress<string>(msg => 
                {
                    Log.Information($"FFmpeg: {msg}");
                    downloadWindow.UpdateProgress(msg);
                });

                var success = await FfmpegHelper.DownloadAndInstallAsync(progress);
                
                downloadWindow.Close();

                if (!success)
                {
                    MessageBox.Show(
                        "FFmpeg indirilemedi. Lütfen manuel olarak kurun:\n" +
                        "https://ffmpeg.org/download.html",
                        "Hata",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        // 3. Bağımlılık Enjeksiyonu (DI) Yapılandırması
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // 4. Ana Pencere Başlatma Mantığı
        var settingsService = _serviceProvider.GetRequiredService<SettingsService>();
        var settings = settingsService.Load();

        Log.Information("Başlangıç ayarları: StartInTray={StartInTray}", settings.StartInTray);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        if (settings.StartInTray)
        {
            // Tepsi modunda başlat (Pencereyi gizle)
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.Show();
        }
        else
        {
            mainWindow.Show();
        }

        Log.Information("Uygulama başarıyla başlatıldı.");
    }

    /// <summary>
    /// Servislerin ve bağımlılıkların DI container üzerine kaydedildiği metod.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Loglama servisi
        services.AddLogging(builder => builder.AddSerilog());

        // Çekirdek Servisler
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ICaptureSource, WgcCaptureSource>();

        // FFmpeg varsa gerçek kodlayıcıyı, yoksa uyarı sınıflarını yükle
        if (FfmpegHelper.IsInstalled())
        {
            services.AddSingleton<IVideoEncoder, FfmpegVideoEncoder>();
            Log.Information("FFmpeg encoder aktif edildi.");
        }
        else
        {
            services.AddSingleton<IVideoEncoder, DummyVideoEncoder>();
            Log.Warning("FFmpeg bulunamadı, geçici kodlayıcı kullanılıyor.");
        }

        services.AddSingleton<FileNamingService>();
        services.AddSingleton<IAudioRecorder, AudioRecorder>();
        services.AddSingleton<IRecorderController, RecorderController>();

        // ViewModel ve View kayıtları
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }

    /// <summary>
    /// Uygulama kapatılırken kaynakları temizler ve logları boşaltır.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Uygulama kapatılıyor.");
        if (_serviceProvider is IDisposable d) d.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}


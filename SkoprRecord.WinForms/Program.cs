using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SkoprRecord.Application.Helpers;
using SkoprRecord.Application.Interfaces;
using SkoprRecord.Application.Services;
using SkoprRecord.Domain.Interfaces;
using SkoprRecord.Infrastructure.Audio;
using SkoprRecord.Infrastructure.Capture;
using SkoprRecord.Infrastructure.Encoding;

namespace SkoprRecord.WinForms;

/// <summary>
/// Uygulamanın ana giriş noktası (Composition Root).
/// Bağımlılık yönetimi (DI), günlükleme (logging) ve form yönetimini başlatır.
/// </summary>
static class Program
{
    /// <summary>
    /// Uygulamanın ana giriş noktası.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // 1. Günlükleme Sistemini Kur (Serilog)
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkoprRecord");
        if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
        var logPath = Path.Combine(appDataPath, "logs", "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Skopr Record (WinForms) başlatılıyor...");
        Log.Information("Log dosyası: {Path}", logPath);

        // 2. WinForms Konfigürasyonu
        ApplicationConfiguration.Initialize();

        // 3. Bağımlılık Enjeksiyonu (DI) Yapılandırması
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // 4. Ana Form Başlatma
        try
        {
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            System.Windows.Forms.Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Uygulama başlatılırken hata oluştu.");
            Views.SkoprMessageBox.Show($"Uygulama başlatılırken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.Information("Uygulama kapatılıyor.");
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Servislerin ve bağımlılıkların DI container üzerine kaydedildiği metod.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
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

        // Form kayıtları
        services.AddTransient<MainForm>();
    }
}
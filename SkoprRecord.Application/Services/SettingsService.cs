using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SkoprRecord.Domain.Models;

namespace SkoprRecord.Application.Services;

/// <summary>
/// Uygulama ayarlarının diske kaydedilmesi ve diskten okunması işlemlerini yöneten servis.
/// Ayarlar '%AppData%\SkoprRecord\settings.json' yolunda saklanır.
/// </summary>
public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SkoprRecord",
        "settings.json"
    );

    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Ayarları diskten yükler. Dosya yoksa veya hata oluşursa varsayılan ayarları döner.
    /// </summary>
    /// <returns>Yüklenen kayıt ayarları.</returns>
    public RecordingSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<RecordingSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (settings != null)
                {
                    _logger.LogInformation("Ayarlar yüklendi: {@Settings}", settings);
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar yüklenirken hata oluştu: {Path}", SettingsPath);
        }

        _logger.LogWarning("Ayarlar dosyası bulunamadı veya yüklenemedi, varsayılan ayarlar dönülüyor.");
        return new RecordingSettings();
    }

    /// <summary>
    /// Verilen ayarları JSON formatında diske kaydeder.
    /// </summary>
    /// <param name="settings">Kaydedilecek ayarlar nesnesi.</param>
    public void Save(RecordingSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsPath, json);
            _logger.LogInformation("Ayarlar kaydedildi: {Path}", SettingsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar kaydedilirken hata oluştu: {Path}", SettingsPath);
        }
    }
}


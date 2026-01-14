using System.Diagnostics;
using System.IO.Compression;

namespace SkoprRecord.Application.Helpers;

/// <summary>
/// FFmpeg aracının sistemde bulunması, indirilmesi ve kurulması işlemlerini yöneten yardımcı sınıf.
/// </summary>
public static class FfmpegHelper
{
    private const string FFMPEG_DOWNLOAD_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";

    // Uygulamanın LocalAppData klasöründeki FFmpeg dizini
    private static readonly string FfmpegFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SkoprRecord", "ffmpeg"
    );

    /// <summary> Yerel FFmpeg çalıştırılabilir dosyasının tam yolu. </summary>
    public static string FfmpegPath => Path.Combine(FfmpegFolder, "bin", "ffmpeg.exe");

    /// <summary>
    /// FFmpeg'in sistemde (PATH'te) veya uygulama klasöründe yüklü olup olmadığını kontrol eder.
    /// </summary>
    public static bool IsInstalled()
    {
        // 1. Sistem PATH çevre değişkeninde ara
        if (IsInPath())
            return true;

        // 2. Uygulamanın kendi klasöründe ara
        if (File.Exists(FfmpegPath))
            return true;

        return false;
    }

    /// <summary>
    /// Komut satırı üzerinden FFmpeg'in erişilebilir olup olmadığını test eder.
    /// </summary>
    private static bool IsInPath()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Çalıştırılabilir FFmpeg dosyasının yolunu döner.
    /// </summary>
    /// <returns>Bulunursa dosya yolu veya 'ffmpeg' komutu, bulunamazsa null.</returns>
    public static string? GetFfmpegPath()
    {
        if (IsInPath())
            return "ffmpeg";

        if (File.Exists(FfmpegPath))
            return FfmpegPath;

        return null;
    }

    /// <summary>
    /// FFmpeg aracını belirtilen URL'den indirir, zipten çıkarır ve ilgili klasöre kurar.
    /// </summary>
    /// <param name="progress">İlerleme durumunu raporlamak için opsiyonel arayüz.</param>
    public static async Task<bool> DownloadAndInstallAsync(IProgress<string>? progress = null)
    {
        try
        {
            progress?.Report("FFmpeg indiriliyor...");

            Directory.CreateDirectory(FfmpegFolder);
            var zipPath = Path.Combine(FfmpegFolder, "ffmpeg.zip");

            // HTTP istemcisi ile indir
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                progress?.Report("İndirme başlatılıyor (yaklaşık 100MB)...");

                var response = await client.GetAsync(FFMPEG_DOWNLOAD_URL, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var downloadedBytes = 0L;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        var percent = (int)((downloadedBytes * 100) / totalBytes);
                        progress?.Report($"İndiriliyor: %{percent}");
                    }
                }
            }

            progress?.Report("Arşiv çıkartılıyor...");
            ZipFile.ExtractToDirectory(zipPath, FfmpegFolder, overwriteFiles: true);

            // İndirilen paketin içindeki bin klasörünü ayıkla
            var extractedDirs = Directory.GetDirectories(FfmpegFolder, "ffmpeg-*");
            if (extractedDirs.Length > 0)
            {
                var sourceDir = extractedDirs[0];
                var sourceBin = Path.Combine(sourceDir, "bin");
                var destBin = Path.Combine(FfmpegFolder, "bin");

                if (Directory.Exists(sourceBin))
                {
                    if (Directory.Exists(destBin))
                        Directory.Delete(destBin, true);

                    Directory.Move(sourceBin, destBin);
                }
                Directory.Delete(sourceDir, true);
            }

            File.Delete(zipPath);
            progress?.Report("FFmpeg kurulumu tamamlandı!");

            return File.Exists(FfmpegPath);
        }
        catch (Exception ex)
        {
            progress?.Report($"Kurulum hatası: {ex.Message}");
            return false;
        }
    }
}

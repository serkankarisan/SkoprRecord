namespace SkoprRecord.Application.Services;

/// <summary>
/// Kayıt dosyası isimlendirme servisi.
/// Tarih-saat bazlı benzersiz dosya isimleri oluşturur.
/// </summary>
public class FileNamingService
{
    private readonly string _outputFolder;

    /// <summary>
    /// Yeni bir FileNamingService nesnesi oluşturur.
    /// </summary>
    /// <param name="outputFolder">Varsayılan çıktı klasörü. Belirtilmezse 'Videolarım/SkoprRecord' kullanılır.</param>
    public FileNamingService(string? outputFolder = null)
    {
        _outputFolder = outputFolder ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SkoprRecord"
        );

        // Klasörün var olduğundan emin ol
        if (!Directory.Exists(_outputFolder))
        {
            Directory.CreateDirectory(_outputFolder);
        }
    }

    /// <summary>
    /// Mevcut tarih ve saate göre yeni bir MP4 dosya yolu oluşturur.
    /// </summary>
    /// <returns>Oluşturulan tam dosya yolu.</returns>
    public string GenerateFilePath()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var fileName = $"Kayit_{timestamp}.mp4";
        return Path.Combine(_outputFolder, fileName);
    }

    /// <summary> Kayıtların tutulduğu ana klasör yolu. </summary>
    public string OutputFolder => _outputFolder;
}


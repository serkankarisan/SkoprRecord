using System;
using System.Threading.Tasks;
using SkoprRecord.Domain.Enums;
using SkoprRecord.Domain.Models;

namespace SkoprRecord.Application.Interfaces;

/// <summary>
/// Tüm kayıt sürecini (video + ses) orkestre eden kontrolcü arayüzü.
/// </summary>
public interface IRecorderController
{
    /// <summary> Kaydedicinin anlık durumu. </summary>
    RecorderState CurrentState { get; }

    /// <summary> Durum değiştiğinde tetiklenen olay. </summary>
    event EventHandler<RecorderState> StateChanged;

    /// <summary> Aktif kayıt ayarları. </summary>
    RecordingSettings Settings { get; set; }

    /// <summary>
    /// Ekran ve ses kaydını başlatır.
    /// </summary>
    /// <param name="audioOnly">Eğer true ise sadece ses kaydeder.</param>
    Task StartRecordingAsync(bool audioOnly);

    /// <summary>
    /// Kaydı durdurur ve dosyaları birleştirme işlemini başlatır.
    /// </summary>
    Task StopRecordingAsync();

    /// <summary> Kayıt başarıyla başladığında tetiklenir. </summary>
    event EventHandler? RecordingStarted;

    /// <summary> Kayıt bittiğinde ve dosya hazır olduğunda tetiklenir. Parametre olarak dosya yolunu döner. </summary>
    event EventHandler<string>? RecordingEnded;

    /// <summary> Kayıt sırasında sistem sesini anlık sessize al/aç. </summary>
    bool IsSystemAudioMuted { get; set; }

    /// <summary> Kayıt sırasında mikrofonu anlık sessize al/aç. </summary>
    bool IsMicMuted { get; set; }
}


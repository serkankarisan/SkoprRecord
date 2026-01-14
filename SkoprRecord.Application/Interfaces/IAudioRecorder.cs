using System.Threading.Tasks;
using SkoprRecord.Domain.Models;

namespace SkoprRecord.Application.Interfaces;

/// <summary>
/// Ses kayıt işlemlerini (sistem ve mikrofon) yöneten servis arayüzü.
/// </summary>
public interface IAudioRecorder
{
    /// <summary>
    /// Ses kaydını başlatır.
    /// </summary>
    /// <param name="captureSystem">Sistem sesinin kaydedilip kaydedilmeyeceği.</param>
    /// <param name="captureMic">Mikrofon sesinin kaydedilip kaydedilmeyeceği.</param>
    void StartRecording(bool captureSystem, bool captureMic);

    /// <summary>
    /// Ses kaydını durdurur ve geçici dosya yollarını döner.
    /// </summary>
    /// <returns>Kayıt sonuçları (dosya yolları).</returns>
    AudioRecordingResult StopRecording();

    /// <summary> Sistem sesinin çalışma anında sessize alınıp alınmadığı. </summary>
    bool IsSystemAudioMuted { get; set; }

    /// <summary> Mikrofonun çalışma anında sessize alınıp alınmadığı. </summary>
    bool IsMicMuted { get; set; }

    /// <summary> Kayıt işleminin aktif olup olmadığı. </summary>
    bool IsRecording { get; }
}


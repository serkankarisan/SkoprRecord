using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using SkoprRecord.Application.Interfaces;
using SkoprRecord.Domain.Models;

namespace SkoprRecord.Infrastructure.Audio;

/// <summary>
/// NAudio kütüphanesini kullanarak ses kayıt işlemlerini gerçekleştiren sınıf.
/// WASAPI Loopback (Sistem Sesi) ve WaveIn (Mikrofon) teknolojilerini kullanır.
/// </summary>
public class AudioRecorder : IAudioRecorder, IDisposable
{
    private WasapiLoopbackCapture? _systemCapture;
    private WaveInEvent? _micCapture;
    private WaveFileWriter? _systemWriter;
    private WaveFileWriter? _micWriter;
    private string? _systemAudioPath;
    private string? _micAudioPath;

    private bool _isSystemAudioMuted;
    private bool _isMicMuted;
    private bool _isRecording;

    /// <summary> Kayıt işleminin devam edip etmediği. </summary>
    public bool IsRecording => _isRecording;

    /// <summary> Sistem sesinin sessize alınıp alınmadığı. </summary>
    public bool IsSystemAudioMuted
    {
        get => _isSystemAudioMuted;
        set => _isSystemAudioMuted = value;
    }

    /// <summary> Mikrofonun sessize alınıp alınmadığı. </summary>
    public bool IsMicMuted
    {
        get => _isMicMuted;
        set => _isMicMuted = value;
    }

    /// <summary>
    /// Ses kaydını başlatır. Sistem ve mikrofon için ayrı ayrı yakalama birimleri (capture unit) oluşturur.
    /// Kayıtlar geçici (temp) dizininde WAV dosyası olarak tutulur.
    /// </summary>
    public void StartRecording(bool captureSystem, bool captureMic)
    {
        _isRecording = true;
        _isSystemAudioMuted = false;
        _isMicMuted = false;

        // 1. Sistem Sesi (WASAPI Loopback)
        if (captureSystem)
        {
            try
            {
                _systemAudioPath = Path.Combine(Path.GetTempPath(), $"sys_{Guid.NewGuid()}.wav");
                _systemCapture = new WasapiLoopbackCapture();
                _systemWriter = new WaveFileWriter(_systemAudioPath, _systemCapture.WaveFormat);

                _systemCapture.DataAvailable += (s, e) =>
                {
                    if (!_isSystemAudioMuted && _systemWriter != null)
                    {
                        _systemWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    }
                    else if (_isSystemAudioMuted && _systemWriter != null)
                    {
                        // Sessiz durumdayken boş veri yazarak video senkronizasyonunu koruyoruz
                        var silence = new byte[e.BytesRecorded];
                        _systemWriter.Write(silence, 0, silence.Length);
                    }
                };

                _systemCapture.RecordingStopped += (s, e) =>
                {
                    _systemWriter?.Dispose();
                    _systemWriter = null;
                };

                _systemCapture.StartRecording();
            }
            catch (Exception)
            {
                _systemCapture?.Dispose();
                _systemCapture = null;
            }
        }

        // 2. Mikrofon (WaveIn)
        if (captureMic)
        {
            try
            {
                _micAudioPath = Path.Combine(Path.GetTempPath(), $"mic_{Guid.NewGuid()}.wav");
                _micCapture = new WaveInEvent();
                _micWriter = new WaveFileWriter(_micAudioPath, _micCapture.WaveFormat);

                _micCapture.DataAvailable += (s, e) =>
                {
                    if (!_isMicMuted && _micWriter != null)
                    {
                        _micWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    }
                    else if (_isMicMuted && _micWriter != null)
                    {
                        // Sessiz durumdayken boş veri yaz
                        var silence = new byte[e.BytesRecorded];
                        _micWriter.Write(silence, 0, silence.Length);
                    }
                };

                _micCapture.RecordingStopped += (s, e) =>
                {
                    _micWriter?.Dispose();
                    _micWriter = null;
                };

                _micCapture.StartRecording();
            }
            catch (Exception)
            {
                _micCapture?.Dispose();
                _micCapture = null;
            }
        }
    }

    /// <summary>
    /// Kayıtları durdurur ve oluşturulan geçici dosya yollarını döner.
    /// </summary>
    public AudioRecordingResult StopRecording()
    {
        _isRecording = false;

        // Sistem sesi yakalamayı durdur
        if (_systemCapture != null)
        {
            _systemCapture.StopRecording();
            _systemCapture.Dispose();
            _systemCapture = null;
        }

        // Mikrofon yakalamayı durdur
        if (_micCapture != null)
        {
            _micCapture.StopRecording();
            _micCapture.Dispose();
            _micCapture = null;
        }

        // Yazıcıları güvenle kapat
        if (_systemWriter != null)
        {
            _systemWriter.Flush();
            _systemWriter.Dispose();
            _systemWriter = null;
        }

        if (_micWriter != null)
        {
            _micWriter.Flush();
            _micWriter.Dispose();
            _micWriter = null;
        }

        return new AudioRecordingResult
        {
            SystemAudioPath = _systemAudioPath,
            MicAudioPath = _micAudioPath
        };
    }

    public void Dispose()
    {
        StopRecording();
    }
}


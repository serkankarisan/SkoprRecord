# Skopr Ekran Kaydet (Skopr Screen Record) 🎥 (WPF & WinForms)

**Skopr Screen Record**, Windows için geliştirilmiş, yüksek performanslı ve modern arayüzlü bir ekran kayıt uygulamasıdır. Windows Graphics Capture API (WGC) ve FFmpeg teknolojilerini kullanarak düşük gecikmeli, yüksek kaliteli video ve ses kaydı sunar.

## ✨ Özellikler

- 🚀 **Yüksek Performans**: Windows Graphics Capture API ile donanım ivmeli ekran yakalama.
- 🔊 **Çoklu Ses Kaydı**: Hem sistem sesini hem de mikrofonu aynı anda veya ayrı ayrı kaydedebilme.
- 🎵 **Sadece Ses Kaydı**: Ekran görüntüsü olmadan sadece ses kaydı (MP3) yapabilme özelliği.
- ⚙️ **Esnek Ayarlar**: Özelleştirilebilir FPS (24, 30, 60), Bitrate ve çıktı klasörü seçenekleri.
- ⌨️ **Global Kısayol**: `Ctrl + Shift + R` ile uygulamaya odaklanmadan kaydı başlatma/durdurma.
- 📥 **Otomatik FFmpeg**: Sistemde FFmpeg yoksa uygulama içinden otomatik indirme ve kurulum.
- 📂 **Sistem Tepsisi (Tray)**: Uygulamayı sistem tepsisinde gizli çalıştırabilme, ses kanallarını hızlıca açıp/kapatabilme ve oradan yönetebilme.
- 📄 **Modern Arayüz**: WPF ve Material Design esintili, kullanımı kolay arayüz.

## 🛠️ Kullanılan Teknolojiler

- **Dil**: C# 12 / .NET 8
- **Arayüz**: WPF ve Windows Forms (WinForms)
- **Ekran Yakalama**: Windows Graphics Capture (WGC)
- **Video Kodlama**: FFmpeg (libx264)
- **Ses İşleme**: NAudio (WASAPI Loopback & WaveIn)
- **MVVM**: CommunityToolkit.Mvvm
- **Loglama**: Serilog
- **Grafik**: Vortice.Windows (Direct3D11 Interop)

## 📁 Proje Yapısı

Proje Clean Architecture prensiplerine uygun olarak katmanlara ayrılmıştır:

- **SkoprRecord.App**: WPF kullanıcı arayüzü, ViewModels.
- **SkoprRecord.WinForms**: Windows Forms kullanıcı arayüzü ve entegrasyonu.
- **SkoprRecord.Application**: İş mantığı (Business Logic), servis arayüzleri ve kontrolörler.
- **SkoprRecord.Domain**: Temel modeller, enumlar ve en alt düzey arayüzler.
- **SkoprRecord.Infrastructure**: WGC yakalama, FFmpeg kodlama ve NAudio ses kayıt uygulamaları.

## 🚀 Başlangıç

### Gereksinimler
- Windows 10 sürüm 1903 (veya daha yeni)
- .NET 8 SDK

### Kurulum
1. Repoyu klonlayın:
   ```bash
   git clone https://github.com/serkankarisan/SkoprRecord.git
   ```
2. Proje dizinine gidin:
   ```bash
   cd SkoprRecord
   ```
3. Uygulamayı derleyin ve çalıştırın:
   ```bash
   dotnet run --project SkoprRecord.App
   # Veya WinForms versiyonu için:
   dotnet run --project SkoprRecord.WinForms
   ```

> [!NOTE]
> Uygulama ilk açılışta FFmpeg'in eksik olduğunu fark edecek ve indirmek isteyip istemediğinizi soracaktır. Onay verdiğinizde FFmpeg otomatik olarak kurulacaktır.

## 📝 Lisans
Bu proje MIT lisansı ile lisanslanmıştır.

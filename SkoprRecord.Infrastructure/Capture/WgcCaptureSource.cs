using SkoprRecord.Domain.Interfaces;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using WinRT;

namespace SkoprRecord.Infrastructure.Capture;

/// <summary>
/// Windows Graphics Capture (WGC) API kullanarak ekran görüntüsü yakalayan sınıf.
/// Windows 10 (1903+) ve Windows 11 için optimize edilmiştir.
/// </summary>
public class WgcCaptureSource : ICaptureSource
{
    private GraphicsCaptureItem? _item;
    private Direct3D11CaptureFramePool? _framePool;
    private GraphicsCaptureSession? _session;
    private IDirect3DDevice? _winrtDevice;
    private int _width;
    private int _height;

    /// <summary> Yeni bir görüntü karesi hazır olduğunda tetiklenir. </summary>
    public event EventHandler<object>? FrameArrived;

    /// <summary>
    /// Ekran yakalamayı başlatır. Ana monitörü yakalar.
    /// </summary>
    /// <summary>
    /// Ekran yakalamayı başlatır.
    /// </summary>
    /// <param name="monitorHandle">Yakalanacak monitörün handle değeri. 0 ise birincil monitör seçilir.</param>
    public async Task StartCaptureAsync(IntPtr monitorHandle = default)
    {
        // Direct3D cihazını oluştur
        _winrtDevice = D3D11Helper.CreateDevice();

        // Eğer belirli bir monitör handle'ı verilmediyse birincil monitörü bul
        if (monitorHandle == IntPtr.Zero)
        {
            monitorHandle = MonitorFromWindow(IntPtr.Zero, MONITOR_DEFAULTTOPRIMARY);
        }

        // Yakalama öğesini oluştur
        _item = CaptureHelper.CreateItemForMonitor(monitorHandle);

        if (_item == null) throw new InvalidOperationException("Grafik yakalama öğesi oluşturulamadı.");

        _width = _item.Size.Width;
        _height = _item.Size.Height;

        // Kare havuzunu (Frame Pool) oluştur
        _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            _winrtDevice,
            Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
            2,
            _item.Size);

        _framePool.FrameArrived += OnFrameArrived;

        // Yakalama oturumunu başlat
        _session = _framePool.CreateCaptureSession(_item);
        _session.StartCapture();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Sistemden yeni kare geldiğinde tetiklenen olay işleyici.
    /// Gelen kareyi SoftwareBitmap üzerinden ham bayt dizisine dönüştürür.
    /// </summary>
    private async void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
    {
        using var frame = sender.TryGetNextFrame();
        if (frame == null) return;

        try
        {
            var surface = frame.Surface;
            if (surface == null) return;

            // Yüzeyi (Surface) SoftwareBitmap'e dönüştürerek işlemciye aktar
            var softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(surface, BitmapAlphaMode.Premultiplied);

            if (softwareBitmap == null) return;

            // Format kontrolü (BGRA8 bekleniyor)
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            // Piksel verisini çıkar
            var buffer = new byte[softwareBitmap.PixelWidth * softwareBitmap.PixelHeight * 4];
            softwareBitmap.CopyToBuffer(buffer.AsBuffer());

            softwareBitmap.Dispose();

            // Bayt dizisini kodlayıcıya gönder
            if (buffer.Length > 0)
            {
                FrameArrived?.Invoke(this, buffer);
            }
        }
        catch (Exception)
        {
            // Hata durumunda kare atlanır
        }
    }

    /// <summary>
    /// Yakalama oturumunu ve kaynakları kapatır.
    /// </summary>
    public Task StopCaptureAsync()
    {
        _session?.Dispose();
        _session = null;

        _framePool?.Dispose();
        _framePool = null;

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopCaptureAsync();
        if (_winrtDevice is IDisposable d) d.Dispose();
    }

    // --- Win32 Interop ---

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    private const uint MONITOR_DEFAULTTOPRIMARY = 1;
}

/// <summary>
/// Direct3D11 cihaz yönetimi için yardımcı sınıf.
/// </summary>
public static class D3D11Helper
{
    [DllImport("d3d11.dll", EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice", SetLastError = true)]
    private static extern uint CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

    /// <summary>
    /// Yakalama API'si için gerekli olan hibrit (WinRT + Native) D3D cihazını oluşturur.
    /// </summary>
    public static IDirect3DDevice CreateDevice()
    {
        FeatureLevel[] featureLevels = new[]
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0
        };

        ID3D11Device? device = null;
        ID3D11DeviceContext? context = null;

        // Donanım ivmeli cihaz oluşturmayı dene
        var result = D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            featureLevels,
            out device,
            out FeatureLevel actualLevel,
            out context
        );

        if (result.Failure || device == null)
        {
            // Başarısız olursa WARP (yazılımsal) modunu dene
            result = D3D11.D3D11CreateDevice(
                null,
                DriverType.Warp,
                DeviceCreationFlags.BgraSupport,
                featureLevels,
                out device,
                out actualLevel,
                out context
            );

            if (result.Failure || device == null)
            {
                throw new Exception("D3D11 cihazı oluşturulamadı.");
            }
        }

        context?.Dispose();

        try
        {
            // DXGI arayüzü üzerinden WinRT uyumlu IDirect3DDevice oluştur
            using var dxgiDevice = device.QueryInterface<IDXGIDevice>();

            var hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.NativePointer, out var pGraphicsDevice);
            if (hr != 0) throw new Exception("D3D11-DXGI köprüsü kurulamadı.");

            var graphicsDevice = MarshalInterface<IDirect3DDevice>.FromAbi(pGraphicsDevice);
            Marshal.Release(pGraphicsDevice);

            return graphicsDevice!;
        }
        finally
        {
            device?.Dispose();
        }
    }
}

/// <summary>
/// Monitör ve pencerelerden GraphicsCaptureItem oluşturmak için WinRT interop yardımcıları.
/// </summary>
public static class CaptureHelper
{
    private static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        int CreateForWindow([In] IntPtr window, [In] ref Guid iid, out IntPtr result);
        int CreateForMonitor([In] IntPtr monitor, [In] ref Guid iid, out IntPtr result);
    }

    /// <summary>
    /// Verilen monitör handle'ı için bir yakalama öğesi (Capture Item) oluşturur.
    /// </summary>
    public static GraphicsCaptureItem CreateItemForMonitor(IntPtr hMonitor)
    {
        IntPtr hString = IntPtr.Zero;
        try
        {
            int hrString = WindowsCreateString("Windows.Graphics.Capture.GraphicsCaptureItem", (uint)"Windows.Graphics.Capture.GraphicsCaptureItem".Length, out hString);
            if (hrString < 0) Marshal.ThrowExceptionForHR(hrString);

            Guid interopIID = typeof(IGraphicsCaptureItemInterop).GUID;
            int hrFactory = RoGetActivationFactory(hString, ref interopIID, out var factoryPtr);
            if (hrFactory < 0) Marshal.ThrowExceptionForHR(hrFactory);

            var interop = (IGraphicsCaptureItemInterop)Marshal.GetObjectForIUnknown(factoryPtr);
            Marshal.Release(factoryPtr);

            Guid itemAuth = GraphicsCaptureItemGuid;
            int hrItem = interop.CreateForMonitor(hMonitor, ref itemAuth, out var itemPtr);
            if (hrItem < 0) Marshal.ThrowExceptionForHR(hrItem);

            var item = MarshalInterface<GraphicsCaptureItem>.FromAbi(itemPtr);
            Marshal.Release(itemPtr);

            return item;
        }
        finally
        {
            if (hString != IntPtr.Zero) WindowsDeleteString(hString);
        }
    }

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);

    [DllImport("combase.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int WindowsCreateString(string sourceString, uint length, out IntPtr hstring);

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int WindowsDeleteString(IntPtr hstring);
}


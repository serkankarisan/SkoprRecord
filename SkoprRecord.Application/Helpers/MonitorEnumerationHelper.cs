using System.Runtime.InteropServices;

namespace SkoprRecord.Application.Helpers;

public class DisplayInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public IntPtr Handle { get; set; }
    public bool IsPrimary { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public override string ToString()
    {
        return $"{DeviceName} {(IsPrimary ? "[Ana Ekran]" : "")} ({Width}x{Height})";
    }
}

public static class MonitorEnumerationHelper
{
    private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfoEx
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    private const uint MONITORINFOF_PRIMARY = 1;
    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(Point pt, uint dwFlags);

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    public static IntPtr GetMonitorForCursor()
    {
        if (GetCursorPos(out var point))
        {
            return MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
        }
        return IntPtr.Zero;
    }

    public static List<DisplayInfo> GetMonitors()
    {
        var monitors = new List<DisplayInfo>();

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                var mi = new MonitorInfoEx();
                mi.Size = Marshal.SizeOf(mi);

                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    monitors.Add(new DisplayInfo
                    {
                        Handle = hMonitor,
                        DeviceName = mi.DeviceName,
                        IsPrimary = (mi.Flags & MONITORINFOF_PRIMARY) != 0,
                        Width = mi.Monitor.Right - mi.Monitor.Left,
                        Height = mi.Monitor.Bottom - mi.Monitor.Top
                    });
                }
                return true;
            },
            IntPtr.Zero);

        return monitors;
    }
}

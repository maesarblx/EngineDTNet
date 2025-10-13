using System.Runtime.InteropServices;

// эту хуйн мне написал чатгпт 🤑🤑 / chatgpt wrote this shit 🤑🤑

public class MonitorInfoData
{
    public IntPtr MonitorHandle;
    public string? DeviceName;
    public int Left, Top, Right, Bottom;
    public int Width => Right - Left;
    public int Height => Bottom - Top;

    public override string ToString() =>
        $"{DeviceName} ({MonitorHandle}): {Width}x{Height} px @ ({Left},{Top})";
}

public static class MonitorUtils
{
    private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    public static List<MonitorInfoData> GetAllMonitors()
    {
        var list = new List<MonitorInfoData>();

        bool MonitorEnumCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            var mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            if (GetMonitorInfo(hMonitor, ref mi))
            {
                string device = mi.szDevice ?? string.Empty;
                int zeroIdx = device.IndexOf('\0');
                if (zeroIdx >= 0) device = device.Substring(0, zeroIdx);

                list.Add(new MonitorInfoData
                {
                    MonitorHandle = hMonitor,
                    DeviceName = device,
                    Left = mi.rcMonitor.left,
                    Top = mi.rcMonitor.top,
                    Right = mi.rcMonitor.right,
                    Bottom = mi.rcMonitor.bottom
                });
            }
            return true; // продолжать перечисление
        }

        if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumCallback, IntPtr.Zero))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        return list;
    }
}

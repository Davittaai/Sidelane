using System.Runtime.InteropServices;

namespace Sidelane.Services;

/// <summary>Best-effort native dark title bar. Does not replace standard Windows window controls.</summary>
public static class WindowThemeService
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int size);

    public static void RequestDarkTitleBar(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;
        try
        {
            var dark = 1;
            if (DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int)) != 0)
                DwmSetWindowAttribute(hwnd, 19, ref dark, sizeof(int));
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or System.Runtime.InteropServices.COMException)
        { /* Older Windows versions use the system title bar instead. */ }
    }
}

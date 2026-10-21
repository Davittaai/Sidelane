// Alt-Tab window enumeration and focus routine adapted from Quick Apps Overlay (MIT).
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Sidelane.Services;

public sealed record WindowEntry(IntPtr Handle, string Title, string Process);
public sealed class WindowService
{
    public IntPtr OwnHandle { get; set; }

    public static void ShowExistingInstance()
    {
        var hwnd = NativeMethods.FindWindow(null, "Sidelane");
        if (hwnd == IntPtr.Zero) return;
        NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
        NativeMethods.SetForegroundWindow(hwnd);
    }
    public IReadOnlyList<WindowEntry> Enumerate()
    {
        var result = new List<WindowEntry>();
        NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (hwnd == OwnHandle || !NativeMethods.IsWindowVisible(hwnd)) return true;
            var flags = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            if ((flags & NativeMethods.WS_EX_TOOLWINDOW) != 0 && (flags & NativeMethods.WS_EX_APPWINDOW) == 0) return true;
            if ((flags & NativeMethods.WS_EX_NOACTIVATE) != 0) return true;
            if (NativeMethods.GetWindow(hwnd, NativeMethods.GW_OWNER) != IntPtr.Zero) return true;
            if (NativeMethods.DwmGetWindowAttribute(hwnd, NativeMethods.DWMWA_CLOAKED, out var cloaked, sizeof(int)) == 0 && cloaked != 0) return true;
            var size = NativeMethods.GetWindowTextLength(hwnd);
            if (size == 0) return true;
            var title = new StringBuilder(size + 1);
            NativeMethods.GetWindowText(hwnd, title, title.Capacity);
            NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
            string process;
            try { using var details = Process.GetProcessById((int)pid); process = details.ProcessName; }
            catch { return true; }
            result.Add(new WindowEntry(hwnd, title.ToString(), process));
            return true;
        }, IntPtr.Zero);
        return result;
    }

    // Foreground observation reads only window handles and geometry — never a browser tab's content.
    public static bool IsFocused(IntPtr hwnd) => hwnd != IntPtr.Zero && NativeMethods.GetForegroundWindow() == hwnd;

    public static bool IsForegroundFullscreen(IntPtr ownHandle)
    {
        var foreground = NativeMethods.GetForegroundWindow();
        if (foreground == IntPtr.Zero || foreground == ownHandle ||
            !NativeMethods.IsWindowVisible(foreground) ||
            !NativeMethods.GetWindowRect(foreground, out var bounds)) return false;
        // Fullscreen on monitor B should not hide a sidebar on monitor A.
        if (System.Windows.Forms.Screen.FromHandle(foreground).DeviceName !=
            System.Windows.Forms.Screen.FromHandle(ownHandle).DeviceName) return false;
        var screen = System.Windows.Forms.Screen.FromHandle(foreground).Bounds;
        // Compare against monitor bounds (not working area): maximized windows must NOT
        // trigger fullscreen detection simply because they cover the usable desktop.
        const int tolerance = 5;
        return Math.Abs(bounds.Left - screen.Left) <= tolerance &&
               Math.Abs(bounds.Top - screen.Top) <= tolerance &&
               Math.Abs(bounds.Right - screen.Right) <= tolerance &&
               Math.Abs(bounds.Bottom - screen.Bottom) <= tolerance;
    }

    public static bool Matches(WindowEntry entry, string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return false;
        return query.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(term => entry.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        entry.Process.Equals(term.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? term[..^4] : term, StringComparison.OrdinalIgnoreCase));
    }

    public static void Focus(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;
        if (NativeMethods.IsIconic(hwnd)) NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
        var foreground = NativeMethods.GetForegroundWindow();
        var thread = NativeMethods.GetWindowThreadProcessId(foreground, out _);
        var self = NativeMethods.GetCurrentThreadId();
        var attached = thread != 0 && thread != self && NativeMethods.AttachThreadInput(thread, self, true);
        try { NativeMethods.SetForegroundWindow(hwnd); }
        finally { if (attached) NativeMethods.AttachThreadInput(thread, self, false); }
    }
}

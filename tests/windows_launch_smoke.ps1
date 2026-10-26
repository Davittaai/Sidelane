# A native Windows process/UI-window smoke gate for the portable Sidelane build.
# Interactive controls, audio and DPI still require manual desktop QA.
param([string]$ExePath = 'out/Sidelane.exe')
$ErrorActionPreference = 'Stop'
$exe = (Resolve-Path -LiteralPath $ExePath).Path
$workDir = Split-Path -Parent $exe

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class SidelaneNativeProbe {
    private delegate bool EnumCallback(IntPtr hWnd, IntPtr value);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumCallback callback, IntPtr value);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] private static extern int GetWindowTextLength(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    public static int FindWindowWidth(int processId) {
        int width = 0;
        EnumWindows((hwnd, _) => {
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid != (uint)processId || !IsWindowVisible(hwnd) || GetWindowTextLength(hwnd) == 0) return true;
            if (!GetWindowRect(hwnd, out Rect rect)) return true;
            width = rect.Right - rect.Left;
            return false;
        }, IntPtr.Zero);
        return width;
    }
}
'@

Write-Host "Launching $exe for native startup and top-level window verification"
$process = Start-Process -FilePath $exe -WorkingDirectory $workDir -ArgumentList "--smoke-test" -PassThru
try {
    Start-Sleep -Seconds 6
    $process.Refresh()
    if ($process.HasExited) { throw "App exited during startup: $($process.ExitCode)" }
    $windowWidth = [SidelaneNativeProbe]::FindWindowWidth($process.Id)
    # A collapsed 54-DIP rail may be wider in physical pixels under DPI scaling.
    if ($windowWidth -lt 40 -or $windowWidth -gt 180) {
        throw "Expected visible narrow rail after startup, got $windowWidth physical pixels"
    }
    Write-Host "PASS native UI rail: $windowWidth device pixels"

    # Launching again must not produce a second persistent sidebar process.
    $second = Start-Process -FilePath $exe -WorkingDirectory $workDir -PassThru
    try {
        if (-not $second.WaitForExit(9000)) { throw 'Duplicate instance remained running' }
        $process.Refresh()
        if ($process.HasExited) { throw 'Existing instance exited when a duplicate was launched' }
        Write-Host "PASS single instance: existing PID $($process.Id) is still running"
    }
    finally {
        $second.Refresh()
        if (-not $second.HasExited) { Stop-Process -Id $second.Id -Force -ErrorAction SilentlyContinue }
        $second.Dispose()
    }

    $live = Get-Process -Id $process.Id -ErrorAction Stop
    Write-Host ("PASS Windows launch: PID {0}, RAM {1:N1} MiB" -f $live.Id, ($live.WorkingSet64 / 1MB))
}
finally {
    $process.Refresh()
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $process.WaitForExit(5000) | Out-Null
    }
    $process.Dispose()
}

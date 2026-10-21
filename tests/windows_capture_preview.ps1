# Best-effort screenshot of the real WPF app on the isolated GitHub Windows runner.
# This is NOT physical-device or full interactive acceptance testing.
param([string]$ExePath = 'out/Sidelane.exe')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class SidelanePointer {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
}
'@

$exe = (Resolve-Path -LiteralPath $ExePath).Path
$proc = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -PassThru
try {
    Start-Sleep -Seconds 4
    $proc.Refresh()
    if ($proc.HasExited) { throw "Sidelane exited during screenshot startup" }
    # On a normal 100%-scaled fresh runner, the rail starts at (28, 100).
    # Moving the cursor may expand it; this event is best-effort on CI desktops.
    [SidelanePointer]::SetCursorPos(40, 125) | Out-Null
    Start-Sleep -Seconds 2
    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $width = [Math]::Min(1100, $bounds.Width)
    $height = [Math]::Min(900, $bounds.Height)
    $image = New-Object System.Drawing.Bitmap($width, $height)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($image)
        try { $graphics.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $image.Size) }
        finally { $graphics.Dispose() }
        New-Item -ItemType Directory -Force qa-artifacts | Out-Null
        $image.Save('qa-artifacts/native-wpf-ci.png', [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Captured real Windows CI desktop: ${width}x${height} (not a device QA signoff)"
    }
    finally { $image.Dispose() }
}
finally {
    $proc.Refresh()
    if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    $proc.Dispose()
}
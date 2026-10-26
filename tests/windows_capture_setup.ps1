# Best-effort actual Windows WPF Setup capture for visual regression review.
param([string]$ExePath = 'out/Sidelane.exe')
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$exe = (Resolve-Path -LiteralPath $ExePath).Path
$proc = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -ArgumentList "--setup" -PassThru
try {
    Start-Sleep -Seconds 6
    $proc.Refresh()
    if ($proc.HasExited) { throw "Setup smoke process exited." }
    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $width = [Math]::Min(1100, $bounds.Width)
    $height = [Math]::Min(850, $bounds.Height)
    $image = [System.Drawing.Bitmap]::new($width, $height)
    try {
        $g = [System.Drawing.Graphics]::FromImage($image)
        try { $g.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $image.Size) }
        finally { $g.Dispose() }
        New-Item -ItemType Directory -Force qa-artifacts | Out-Null
        $image.Save('qa-artifacts/native-wpf-setup.png', [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Captured native Setup window on GitHub Windows runner."
    }
    finally { $image.Dispose() }
}
finally {
    $proc.Refresh()
    if (!$proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    $proc.Dispose()
}

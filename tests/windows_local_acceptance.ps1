param(
    [Parameter(Mandatory=$true)][string]$ExePath,
    [int]$IdleSeconds = 60,
    [string]$OutputDirectory = "qa-local"
)
$ErrorActionPreference = "Stop"
if ($IdleSeconds -lt 15 -or $IdleSeconds -gt 600) { throw "IdleSeconds must be 15 to 600." }
$exe = (Resolve-Path $ExePath).Path
if (-not $exe.EndsWith(".exe", [StringComparison]::OrdinalIgnoreCase)) { throw "Expected a Windows executable" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$output = (Resolve-Path $OutputDirectory).Path
$started = Get-Date
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$process = Start-Process -FilePath $exe -PassThru
$watch.Stop()
try {
    Start-Sleep -Seconds 5
    $process.Refresh()
    if ($process.HasExited) { throw "Sidelane exited during startup (code $($process.ExitCode))." }
    $samples = @()
    for ($i = 0; $i -lt $IdleSeconds; $i += 5) {
        Start-Sleep -Seconds ([Math]::Min(5, $IdleSeconds - $i))
        $process.Refresh()
        if ($process.HasExited) { throw "Sidelane exited during idle observation." }
        $samples += [PSCustomObject]@{
            elapsedSeconds = [Math]::Min($i + 5, $IdleSeconds)
            workingSetMiB = [Math]::Round($process.WorkingSet64 / 1MB, 1)
            privateMiB = [Math]::Round($process.PrivateMemorySize64 / 1MB, 1)
            totalCpuSeconds = [Math]::Round($process.TotalProcessorTime.TotalSeconds, 3)
        }
    }
    $report = [ordered]@{
        app = "Sidelane"
        environment = "local-windows-qa"
        sampledAt = (Get-Date).ToString("o")
        windowsVersion = [Environment]::OSVersion.VersionString
        machineArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
        startupProcessCreationMs = [Math]::Round($watch.Elapsed.TotalMilliseconds)
        note = "Process creation timing is not visual readiness or true cold startup."
        sha256 = (Get-FileHash -Algorithm SHA256 $exe).Hash.ToLowerInvariant()
        samples = $samples
        manualQa = [ordered]@{
            expandAndCollapse = "not-tested"
            keyboardAndHotkey = "not-tested"
            trayAndExit = "not-tested"
            mediaControls = "not-tested"
            secondMonitorAndDpi = "not-tested"
            fullscreen = "not-tested"
            taskAndNotePrivacy = "not-tested"
        }
    }
    $path = Join-Path $output "sidelane-qa.json"
    $report | ConvertTo-Json -Depth 8 | Set-Content -Path $path -Encoding utf8
    Write-Host "Saved local-only diagnostic report: $path"
    Write-Host "No task, note, window title, browser history or chat text was collected."
} finally {
    if (-not $process.HasExited) {
        Write-Host "Close Sidelane normally from its tray menu after completing manual checks."
    }
}

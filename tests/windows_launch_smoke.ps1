# Native Windows smoke gate: checks that the published WPF binary can start
# and remain alive on the Windows CI runner. Does NOT substitute for manual GUI QA.
param([string]$ExePath = 'out/Sidelane.exe')
$ErrorActionPreference = 'Stop'
$exe = (Resolve-Path -LiteralPath $ExePath).Path
$workDir = Split-Path -Parent $exe
Write-Host "Launching $exe for a Windows process-startup smoke test"
$process = Start-Process -FilePath $exe -WorkingDirectory $workDir -PassThru
try {
    Start-Sleep -Seconds 6
    $process.Refresh()
    if ($process.HasExited) {
        throw "Sidelane exited during startup (exit code: $($process.ExitCode))."
    }
    $running = Get-Process -Id $process.Id -ErrorAction Stop
    Write-Host ("PASS native Windows launch: PID {0}, RAM {1:N1} MiB" -f $running.Id, ($running.WorkingSet64 / 1MB))
}
finally {
    $process.Refresh()
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $process.WaitForExit(5000) | Out-Null
    }
    $process.Dispose()
}

$ErrorActionPreference = "Stop"
$hostPath = (Resolve-Path "out/Sidelane.NativeHost.exe").Path
$p = New-Object System.Diagnostics.Process
$p.StartInfo.FileName = $hostPath
$p.StartInfo.UseShellExecute = $false
$p.StartInfo.RedirectStandardInput = $true
$p.StartInfo.RedirectStandardOutput = $true
$p.StartInfo.CreateNoWindow = $true
if (!$p.Start()) { throw "Native host did not start." }
try {
    $json = '{"provider":"ChatGPT","state":"generating"}'
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $length = [BitConverter]::GetBytes([int]$bytes.Length)
    $p.StandardInput.BaseStream.Write($length, 0, 4)
    $p.StandardInput.BaseStream.Write($bytes, 0, $bytes.Length)
    $p.StandardInput.BaseStream.Flush()
    $p.StandardInput.Close()
    $header = New-Object byte[] 4
    $p.StandardOutput.BaseStream.ReadExactly($header)
    $responseLength = [BitConverter]::ToInt32($header, 0)
    if ($responseLength -lt 1 -or $responseLength -gt 128) { throw "Bad native response length." }
    $response = New-Object byte[] $responseLength
    $p.StandardOutput.BaseStream.ReadExactly($response)
    $body = [System.Text.Encoding]::UTF8.GetString($response) | ConvertFrom-Json
    if ($body.ok -ne $false) { throw "Expected false without a running Sidelane pipe server." }
    if (!$p.WaitForExit(5000)) { throw "Native host did not exit when stdin closed." }
    if ($p.ExitCode -ne 0) { throw "Native host returned exit code $($p.ExitCode)" }
    Write-Host "PASS native messaging binary framing, input validation and safe no-app response"
} finally {
    if (!$p.HasExited) { $p.Kill() }
    $p.Dispose()
}

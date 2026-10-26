param([Parameter(Mandatory=$true)][ValidatePattern('^[a-p]{32}$')][string]$ExtensionId)
$ErrorActionPreference = "Stop"
$exe = Join-Path $PSScriptRoot "..\Sidelane.NativeHost.exe"
if (!(Test-Path $exe)) { throw "Native host executable not found beside the installed Sidelane app." }
$manifest = Join-Path $PSScriptRoot "com.sidelane.activity.json"
@{
    name = "com.sidelane.activity"
    description = "Sidelane optional local AI status bridge"
    path = (Resolve-Path $exe).Path
    type = "stdio"
    allowed_origins = @("chrome-extension://$ExtensionId/")
} | ConvertTo-Json -Depth 4 | Set-Content -Path $manifest -Encoding utf8
$key = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.sidelane.activity"
New-Item -Path $key -Force | Out-Null
Set-Item -Path $key -Value $manifest
Write-Host "Registered Sidelane companion for extension ID $ExtensionId. No admin rights used."

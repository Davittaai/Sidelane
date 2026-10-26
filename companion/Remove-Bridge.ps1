$key = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.sidelane.activity"
if (Test-Path $key) { Remove-Item $key -Force }
Write-Host "Sidelane optional browser bridge unregistered."

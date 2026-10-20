# Execute from the extracted Sidelane root on Windows 10/11 with .NET 10 SDK installed.
$ErrorActionPreference = 'Stop'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install .NET 10 SDK from https://dotnet.microsoft.com/en-us/download/dotnet/10.0' }
if (-not (Test-Path 'src/Sidelane/Sidelane.csproj')) { throw 'Run from the extracted Sidelane project root.' }
& dotnet run --project tests/Sidelane.CoreTests/Sidelane.CoreTests.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Pure .NET checks failed.' }
& dotnet build src/Sidelane/Sidelane.csproj -c Release -warnaserror
if ($LASTEXITCODE -ne 0) { throw 'WPF build failed.' }
& dotnet publish src/Sidelane/Sidelane.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o release
if ($LASTEXITCODE -ne 0) { throw 'Portable build failed.' }
if (-not (Test-Path 'release/Sidelane.exe')) { throw 'Expected Windows executable not found.' }
Write-Host 'Checks and publish succeeded. Complete docs/WINDOWS-QA.md manual smoke tests before public release.'

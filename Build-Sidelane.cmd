@echo off
setlocal
cd /d "%~dp0"
title Sidelane v1.2 - Windows portable build

echo.
echo    Sidelane v1.2 - build and package Windows executable
echo    ---------------------------------------------------
echo.
where dotnet >nul 2>&1
if errorlevel 1 goto needsdk

echo [1/4] Running Sidelane core regression tests...
dotnet run --project "tests\Sidelane.CoreTests\Sidelane.CoreTests.csproj" -c Release
if errorlevel 1 goto failed

echo [2/4] Compiling portable Windows executable...
dotnet publish "src\Sidelane\Sidelane.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "release"
if errorlevel 1 goto failed
if not exist "release\Sidelane.exe" goto failed

echo [3/4] Compiling optional Chrome native messaging host...
dotnet publish "src\Sidelane.NativeHost\Sidelane.NativeHost.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "release-nativehost"
if errorlevel 1 goto failed
copy /Y "release-nativehost\Sidelane.NativeHost.exe" "release\Sidelane.NativeHost.exe" >nul
if errorlevel 1 goto failed
xcopy "companion" "release\companion" /E /I /Y >nul
if errorlevel 1 goto failed

echo [4/4] Windows executable and optional companion created in release\
echo This executable is unsigned and has not passed hands-on Windows QA.
echo See docs\WINDOWS-QA.md before distributing publicly.
explorer "release"
exit /b 0

:needsdk
echo ERROR: .NET 10 SDK is not installed.
echo Install it at https://dotnet.microsoft.com/en-us/download/dotnet/10.0
pause
exit /b 2

:failed
echo.
echo BUILD FAILED. Copy the errors above to report a problem.
pause
exit /b 1

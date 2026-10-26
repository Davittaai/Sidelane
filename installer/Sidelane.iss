#define AppVersion "2.0.0"
[Setup]
SourceDir=..
AppId={{D9309A9A-B081-467B-B096-1187F1AECC06}
AppName=Sidelane
AppVersion={#AppVersion}
SetupIconFile=src\Sidelane\Assets\sidelane.ico
AppPublisher=Davit Naskidashvili
AppPublisherURL=https://github.com/Davittaai/Sidelane
AppSupportURL=https://github.com/Davittaai/Sidelane/issues
AppUpdatesURL=https://github.com/Davittaai/Sidelane/releases
DefaultDirName={localappdata}\Programs\Sidelane
DefaultGroupName=Sidelane
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\Sidelane.exe
OutputDir=dist
OutputBaseFilename=SidelaneSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
DisableDirPage=no
LicenseFile=LICENSE

[Files]
Source: "out\Sidelane.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "out\Sidelane.NativeHost.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "out\companion\*"; DestDir: "{app}\companion"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "out\README-START.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "out\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "out\THIRD_PARTY_NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion

[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Icons]
Name: "{userprograms}\Sidelane"; Filename: "{app}\Sidelane.exe"
Name: "{userdesktop}\Sidelane"; Filename: "{app}\Sidelane.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Sidelane.exe"; Description: "Launch Sidelane"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C reg delete HKCU\Software\Google\Chrome\NativeMessagingHosts\com.sidelane.activity /f"; Flags: runhidden
Filename: "{cmd}"; Parameters: "/C reg delete HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v Sidelane /f"; Flags: runhidden

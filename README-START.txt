Sidelane v2.0 beta 1 - Windows x64 AI workspace

Installer: Run SidelaneSetup.exe to add Start menu and Uninstall support,
or unzip the portable build and run Sidelane.exe.
The app checks official stable GitHub releases daily, offers a notification,
and never installs a replacement without your approval.

1. Extract the entire ZIP to a folder you control (for example, C:\Tools\Sidelane).
2. Run Sidelane.exe. It is self-contained: no separate .NET runtime installation is needed.
3. Use first-run Quick Setup to select tools, browser, colors and installed apps.
   Later reopen via the tray icon > Quick setup.
4. Open Preferences for advanced controls, including a different hotkey or disabling it.
   Hover over the narrow rail to expand it.
5. You can export saved preferences or import a backup from Preferences.
6. Enable Windows sign-in launch only if you want it.
7. Exit via the system-tray icon or right-click the sidebar > Exit Sidelane.

Privacy: Only your appearance and shortcut preferences are saved locally at
%APPDATA%\Sidelane\settings.json. Temporary tasks and manually set AI statuses
are never stored or uploaded. An optional, separately enabled Chrome extension can
report coarse observed generating/open/stopped status but cannot identify task text.

The executable is unsigned. Verify the ZIP checksum in Sidelane-win-x64.sha256.
Windows SmartScreen may warn about an unknown publisher; only run files you trust.

Source and issues: https://github.com/Davittaai/Sidelane
License: MIT. Third-party attributions included in THIRD_PARTY_NOTICES.md.

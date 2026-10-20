# Windows verification checklist

Complete this on a real Windows 10/11 machine before public v0.1 release.

- [ ] `dotnet restore` and `dotnet build -c Release -warnaserror` succeed on a clean machine.
- [ ] App starts as a 54px rail; expanded panel aligns to left/right correctly.
- [ ] Hover expands and leaving the overlay collapses, unless it contains focused input.
- [ ] App detection reports open vs not detected (never invented AI execution status).
- [ ] Click a tracked VS Code window -> foreground; click an offline AI item -> web browser link.
- [ ] Right-click app row -> manual working/waiting/ready labels, resets on restart.
- [ ] Spotify playback session shows actual metadata if Spotify exposes an SMTC session; play/pause and skip work where supported.
- [ ] Browser/alternate media without SMTC doesn't crash the sidebar.
- [ ] Add 3 tasks, check one, delete one, restart -> no tasks remain.
- [ ] Choose accent/background colors and font; restart -> preferences persist.
- [ ] Set background JPG/PNG; restart -> image applied; remove file -> graceful fallback.
- [ ] Add custom app shortcut and matching process; ensure custom URL must be http(s).
- [ ] Drag unlocked bar; lock & attempt drag; toggle left/right/free placement.
- [ ] Test 100%, 125%, 150% display scaling; two monitors if available.
- [ ] Windows tray menu toggle/settings/exit and Ctrl+Alt+Space global hotkey.
- [ ] No private chat/task content appears in `%APPDATA%/Sidelane/settings.json`.
- [ ] Built self-contained EXE opens on clean Windows without .NET installed.
- [ ] Assess unsigned executable Windows SmartScreen warning; document release note.

Report each result as pass/fail/blocked with OS build, DPI, and release SHA.

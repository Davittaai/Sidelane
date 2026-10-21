# Sidelane real-device Windows acceptance

This checklist is for the actual WPF executable (not the HTML concept). Record Windows build, DPI scaling, monitor arrangement, exe SHA256, CPU/RAM methodology, and pass/fail evidence. Do not mark checks complete from CI alone.

## Install and reliability
- [ ] Extract the published Windows x64 ZIP onto a Windows 10/11 system without a separate .NET runtime; launch `Sidelane.exe`.
- [ ] The rail appears visible; app icon, tray menu and Ctrl+Alt+Space work. Document any hotkey conflict.
- [ ] Launch `Sidelane.exe` again: the same instance comes to foreground, without creating another persistent process.
- [ ] Close and relaunch repeatedly; no crash, orphan tray icon, or stuck background instance.
- [ ] Test hidden/visible transitions and verify CPU settles while hidden.

## Interactions and content
- [ ] Hover expands the rail, moving off collapses after a short delay. When keyboard focus is inside, it does not close prematurely.
- [ ] Add, complete, delete and clear temporary tasks; none survive process exit.
- [ ] Set working/waiting/ready on an app, add an activity note, hover the compact rail and verify note is readable and accurate.
- [ ] After restart, temporary activity notes and statuses are absent; inspect `%APPDATA%\Sidelane\settings.json` for zero task/note content.
- [ ] A matching app window focuses correctly; a missing window opens a safe configured http(s) link.
- [ ] Create/edit/remove a shortcut; duplicate names, malformed link and overlong inputs are handled.
- [ ] Test Spotify and another SMTC player for title, artist and supported controls, and then test with no player.
- [ ] Test keyboard navigation, focus indicator, Escape, settings dialog, tray exit and screen reader names.

## Position and fullscreen
- [ ] Drag rail while unlocked, lock it, and verify drag is disabled; settings survive relaunch.
- [ ] Test left/right/free on one monitor at 100% and 125% scaling; verify no edge clipping.
- [ ] Test a second monitor with different DPI if possible, including negative X coordinate layouts.
- [ ] Test a fullscreen app on the same monitor: Sidelane stays behind it.
- [ ] Test a fullscreen app on another monitor: Sidelane remains usable on its own monitor.
- [ ] Test Windows sign-in autostart and undoing the setting.

## Release documentation
- [ ] Take native running-app screenshots; publish without misleading concept-only images.
- [ ] Record cold-start time, after-5-minute idle working-set memory and CPU with methodology; do not report single CI-start value as idle.
- [ ] Inspect network connections and saved files; verify no collection of conversation text or telemetry.
- [ ] Log Windows SmartScreen behavior accurately; unsigned build may warn.
- [ ] Fix confirmed defects with regression tests; rerun all gates before stable update.

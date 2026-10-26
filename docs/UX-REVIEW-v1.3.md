# Sidelane v1.3 UX engineering review

The feature audit originated from firsthand feedback about sizing, awkward color controls, browser-only shortcuts, invisible activity states, missing animation/icon, and absent taskbar presence.

## Decision log

- Keep native WPF, no Electron and no major theme framework. Lightweight patterns from MIT-licensed WPF UI and PixiEditor ColorPicker were reviewed. The built-in Windows ColorDialog provides unrestricted custom colors with no NuGet/runtime additions.
- Add a per-shortcut **launch mode**, per-shortcut **browser**, optional Chromium `--app=` window, and a chosen `.exe` or `.lnk` target. Avoid shell command concatenation by using ProcessStartInfo.ArgumentList and full local path validation.
- Add explicit **AI provider mapping**, with Auto resolving from destination host where possible. The Chrome extension only reports Open, Generating, and Recently stopped. It cannot derive task descriptions from private chat content.
- Provide obvious editable manual task labels next to AI shortcuts so users can annotate what they are working on without extending browser permissions.
- Make the UI more compact and reveal search only when there are more than six shortcuts. Always ensure tasks remain reachable without the full-size panel.
- Keep animations subtle (under 250ms entrance; looping three bars only during generating), with an explicit disable switch.
- Make Sidelane visible on the taskbar when open by default, while retaining tray controls and an option to hide the taskbar button.
- Refresh branding with a distinctive custom Sidelane icon; avoid external font requirements.

## QA gates
1. 70+ core tests (exact count dictated by CI logs), static XAML, JavaScript, PowerShell checks.
2. Warning-as-error native WPF compile, Windows launch smoke, media failure fallback.
3. Installer and uninstaller smoke, checksums, opt-in browser host protocol.
4. Inspect actual WPF screenshot at reference desktop size for clipping and taskbar presence.
5. **Manual follow-up:** mixed DPI, keyboard-only navigation, mouse hover/resize, taskbar minimize/restore, installed Chrome/Edge/Firefox/Brave, installed Store app .lnk shortcut, real Chrome chat statuses.

Do not mark item 5 complete based solely on automated tests.

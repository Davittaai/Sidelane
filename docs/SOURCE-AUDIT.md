# Sidelane v1.2 source audit

## Core decisions
- Keep .NET 10 WPF and Win32 instead of Electron, WebView or heavy frameworks.
- No backend, cloud account, task history, analytics, or transcript storage.
- Avoid persistent AI state unless permissioned browser companion sends a short signal.
- Separate **manual status** from **observed browser status** in labels.

## Optimizations applied
- Cache process names by PID during window scans, avoiding repeated GetProcessById calls.
- Decode wallpapers to 800 px and reuse a single frozen bitmap until path/mtime changes.
- Expire stale AI signals without rebuilding shortcut controls on every 4-second refresh.
- Avoid including private browser window titles in rail tooltips.
- Clamp dimensions loaded from settings JSON to documented bounds and screen work area.
- Continue using native media APIs rather than Spotify web API or polling external endpoints.

## Limitations and follow-up evidence
- Native UI at multiple DPI/scaling factors, Chrome live UI signals and Spotify control accuracy require interactive testing.
- Installer and native-host build are validated in hosted Windows CI. Real Windows SmartScreen prompts vary by device and are not controlled by CI.
- The unsigned setup and ZIP must be downloaded from the official Sidelane GitHub release.
- Clean-machine first-run test, memory-after-idle benchmark, keyboard/screen-reader test and optional browser companion pairing require user-provided Windows evidence.

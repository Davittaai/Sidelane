# Sidelane v1.0.0 - first public Windows release

A minimal, locally running Windows sidebar for switching between apps, controlling available system media, and jotting down disposable tasks.

## Features

- Compact rail, hover expansion, drag-and-lock positioning, left/right/free placement
- Configurable shortcuts for AI tools, browsers and desktop applications
- Foreground-window indication and manual **Working / Waiting / Ready** status labels
- System media session song metadata and player controls where Windows supports them
- Temporary scratchpad, configurable font/accent/surface/background, optional launch on login
- No account, analytics, tracking, conversation reading or cloud backend

## Install

Download `Sidelane-win-x64.zip` and `Sidelane-win-x64.sha256`. Verify the checksum, extract the ZIP and double-click `Sidelane.exe` on a Windows x64 PC. The package is self-contained; no .NET runtime is required. Read `README-START.txt` in the ZIP for controls and privacy details.

## Verification and limitations

CI has verified the core regression suite, WPF compilation with warnings as errors, self-contained packaging, checksum creation and a **process startup smoke test on a Windows runner**. That automated launch is **not** equivalent to hands-on GUI QA. Media transport, fullscreen coexistence, multi-monitor/DPI behavior, and Windows SmartScreen need additional device testing. Please report any defects in GitHub Issues and include your Windows version and scaling.

Sidelane **does not automatically detect whether ChatGPT, Grok or Claude is generating**. AI status labels are manual, and app detection only observes Windows application titles/process matches. Task contents are in-memory only; UI configuration is stored locally.

This executable is unsigned, so Windows SmartScreen may present an unknown publisher warning. Review the public source and verify the SHA256 checksum. Do not bypass warnings for downloads you don't trust.

Credits and licenses: MIT licensed. Win32 window switching adapted with attribution from [Quick Apps Overlay](https://github.com/vhvb1989/quick-apps-overlay).

# Sidelane — portfolio description

**Role:** Independent developer and product designer

**Stack:** C# / .NET 8, WPF, Win32 interop, Windows Media Control, XAML, GitHub Actions

**Problem:** Switching constantly between AI assistants, coding tools, music and small tasks breaks concentration. Full productivity dashboards are often too bulky for this use case.

**Solution:** Sidelane is a minimal desktop sidebar that stays over regular windows, expands on hover and exposes app shortcuts, detected windows, a small task scratchpad and system-media transport controls. Users can configure appearance, placement and their own apps without creating an account.

**Engineering highlights:**
- Adapted MIT-licensed window enumeration and focus routines from Quick Apps Overlay, with attribution.
- Built customizable native WPF layouts and a frameless, always-on-top Windows overlay.
- Split Win32 window-management and Windows media-session logic into isolated services.
- Kept scratchpad tasks and manual status labels in RAM while saving only appearance, geometry and app preferences to local JSON.
- Added conservative HTTP(S)-only external destination validation and a Windows portable-build workflow.

**Accurate status (as of initial source drop):** Source and interactive visual prototype prepared; structural checks performed. The desktop executable still requires a successful Windows build and manual overlay testing before it can be described as released or production-ready.

**Next development milestone:** Windows CI build, real-machine smoke test, then optional browser extension for genuine AI processing/ready states. No chat text collection is planned.

## What changed in the v0.3 code

Screen-aware WPF placement with pure-function geometry tests, real foreground-window highlighting, fullscreen-friendly behavior, Windows media capability checks, optional launch-at-login, ephemeral progress tracking, and release checksum generation. These demonstrate Win32 interop, UI architecture, user privacy, and release engineering.

A screenshot of the browser prototype must always be labeled a *concept preview* until real Windows runtime captures exist.

## v0.4 portfolio engineering additions

Proactive review of third-party references without wholesale dependency adoption; refactored topmost state transitions and removed a perpetual Win32 z-order poll; improved multi-monitor geometry; isolated pure image-input validation from native bitmap decoding; implemented transactional settings updates with rollback when the registry or disk fails; protected Win32 process-enumeration resources; strengthened keyboard/exit affordances. 32 platform-independent regression cases are prepared, awaiting Windows CI. Screenshots remain clearly labeled concepts, not runtime proof.

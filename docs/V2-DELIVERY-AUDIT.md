# Sidelane v2: what changed, what was weak, and what is actually tested

Date: 2026-10-26. This document separates implementation from acceptance.

## v1.3 baseline

The previous releases delivered a native .NET 10 WPF collapsible rail, media controls via Windows sessions, temporary scratchpad, manual AI task notes, adjustable dimensions and theme palettes, per-app browser or EXE/shortcut selection, optional experimental Chrome generating/open signals, taskbar/tray controls, per-user installer, checksummed downloads and consent-based GitHub updates.

The major problem was *not missing technology*, but first-use friction. With no onboarding, first launch showed an unexplained rail; the default VS Code shortcut could not launch unless an actual application was already open, and Ctrl+Alt+Space was registered without warning if another application owned it. Real observed AI task names have **never** been supported; only manual notes can carry a task description.

## v2 actions

| Weakness | v2 decision | How it is checked |
|---|---|---|
| Abrupt first launch | A branded first-run WPF setup guides tool selection, browser, theme and taskbar setting | Fresh-vs-existing profile unit tests; native setup screenshot |
| Existing users may fear configuration loss | Upgrade does not invoke first-run on existing settings; tray Quick Setup requires confirmation | Fresh/existing profile tests, manual acceptance |
| Unconfigured installed AI apps | Local conservative Start Menu shortcut discovery with exact-name matching, never running the program | Matched/missing shortcut tests and live Windows test needed |
| VS Code shortcut has no valid browser fallback | Uncheck VS Code in setup if no local shortcut found | Native setup review and optional device validation |
| Hotkey collisions were silent | User can select Ctrl+Alt+Space, Ctrl+Shift+Space, or disable; detect Windows registration conflict | Pure policy tests and Windows real-device check |
| Fragmented visual themes | Derive main rail and media card surfaces/borders from user-selected dark background | Theme regression tests, native screenshot visual inspection |
| Background resource footprint | Keep Win32/WPF, in-memory task status, and no Electron/cloud layer | Measure actual RAM after 5 minutes on device, not merely CI launch |

## Deferred despite the v2 name

- **Automatic AI task transcription:** not available. AI browser connector only observes a few attributes and never reads prompt/response content.
- **Trustworthy task completion:** disappearance of a provider Stop button is not proof of successful completion.
- **Live cross-browser AI detection:** current opt-in connector is Chrome-only; selected Edge/Firefox/Brave browsers remain valid launch destinations but do not provide activity signals without a separate integration.
- **Automatic app scanning of every machine:** conservative Start Menu shortcuts only, deliberately not a full PowerToys replacement.
- **Real stable-release acceptance:** high-DPI monitors, multiple windows and browsers, Spotify sessions, keyboard accessibility and 5-minute idle memory require physical Windows validation.
- **Code signing:** installer is unsigned; SmartScreen may warn.

## Release gates

1. Core unit tests with no regressions.
2. XML/XAML event validation, native Windows warnings-as-errors compiler, WPF smoke and Chrome native-host protocol test.
3. Installer build and uninstall checks, ZIP and installer SHA256.
4. Native screenshot of first-run setup and compact rail with visual review.
5. User laptop/device validation before the first v2 stable tag.

**Never conflate hosted CI with universal compatibility.**

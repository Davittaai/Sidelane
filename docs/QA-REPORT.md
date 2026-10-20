# Sidelane v0.2 — three-pass QA report

## Pass 1: correctness and preferences

- Fixed new/edited/deleted shortcut save edge cases.
- Normalized malformed preference JSON, including invalid colors, shortcut arrays and URL fields.
- Added no-credentials URL policy, temp-file cleanup, and an injectable settings directory for tests.
- Updated media-session selection to prefer active audio.
- Expanded the prepared .NET core suite to **15 tests**. These tests are not claimed as executed until a .NET SDK runner completes them.

## Pass 2: interaction and appearance

- Added right-rail orientation persistence when a sidebar is dragged into free placement.
- Added a 240ms hover-exit delay, Escape-to-collapse, and close-when-deactivated handling.
- Added visible per-task remove buttons, media button unavailable states, four quick accent presets.
- Parsed WPF markup and checked every XAML handler reference against code-behind.

## Pass 3: QA, presentation, and release safety

- Repeated static checks on all WPF XAML files plus required license, contributor and privacy files: **pass**.
- Chromium design-preview test: settings, color, right placement, task add/check/remove, media demo, collapse/re-expand: **pass**, 0 uncaught JavaScript errors.
- Preview screenshot visually inspected for alignment and legibility. Preview is **sample data only**.
- GitHub Actions workflow now has read-only build permissions and a separate release job with scoped write permission.
- Additional contributor, security, changelog and release-gate documentation added.

## Not verified — mandatory release gates

- No .NET SDK is installed in this environment. **C# project did not compile here** and the 15 prepared .NET tests were not executed.
- No real Windows machine was available. WPF overlay operation, media controls, focus switching, DPI handling, hotkey and installer were not tested.
- GitHub Actions must run on the published repository before any stable release claim.
- Source is a portfolio **candidate**, not a verified production Windows executable.

See `docs/WINDOWS-QA.md` and `docs/RELEASE-GATES.md` for final human checks.

---

## v0.3 source QA — independent follow-up

**Implemented in source:** focused-app detection; full-screen-aware topmost behavior; screen-aware placement with WPF DPI conversion; task completion count; capability-aware media buttons; optional HKCU launch at login; improved Unicode Win32 window enumeration.

**Static verification actually executed:** `python tests/static_check.py` — passed XML parsing, declared XAML handler references, required-file checks.

**Design preview verification actually executed:** `python tests/preview_smoke.py` — passed settings, accent, right-edge placement, scratchpad, media demo controls, collapse/re-expand, 0 uncaught JavaScript page errors. This preview does **not** test the native WPF app.

**Prepared for Windows at v0.3:** 21 pure .NET checks and `.github/workflows/windows.yml` builds, SHA256 checksum and artifact packaging. **NOT executed here:** C# compilation, .NET checks, Windows media playback controls, system tray, startup registry integration, fullscreen transitions, mixed-DPI/multi-monitor movement, click-to-focus, and Windows SmartScreen behavior. No production release claim is warranted.

**Environment limitation:** current container has no .NET SDK or Windows runtime and cannot resolve the .NET SDK download host. Publishing also requires a new GitHub repository (not provisioned via the available connector).

---

## v0.4 independent source audit — October 2026

**Actually executed here:** `python tests/static_check.py` verified WPF XAML XML structure and declared events, including the new app-filter text event. `python tests/preview_smoke.py` exercised search filtering, settings, colors, placement, temporary tasks, media simulation, and collapse/expand in Chromium, with zero page errors. Source diff whitespace validated; ZIP and Git snapshot integrity checked.

**Authored but not executed:** 32 .NET core regression cases including temporary preferences, duplicate names, missing/oversized/unrecognized images, placement orientation, invalid destinations, and failed-write rollback. No Windows/.NET SDK in this runtime, so native WPF compile, media-session integration, Windows topmost behavior, tray, hotkey and multi-monitor DPI cases remain unverified. No tested Windows EXE is claimed.

**Dependency choices:** Reference implementations reviewed include Quick Apps Overlay (MIT, existing attributed source), WPF UI (MIT, not adopted) and ModernWPF (MIT, not adopted). No new dependency or third-party code was copied in v0.4.

---

## v0.5 interface redesign QA, October 19, 2026

- Inspected v0.4 OverlayWindow, SettingsWindow, styles, interaction handlers, preference model, preview, and tests before changes.
- Replaced overlay chrome, app-row styling, fixed icon buttons, media layout, section typography, and settings layout in the actual WPF XAML.
- Updated C# row creation, display states, app count, disabled media labels, background-image opacity, placeholder visibility, and contrast-friendly defaults.
- Rebuilt the browser preview to correspond to the WPF sections and settings rather than a separate promotional layout.
- Visually inspected expanded-left, expanded-right, settings-open, and collapsed-rail screenshots, correcting multi-line row status and clipping.
- `python tests/static_check.py`: passed XML parsing for all three WPF XAML files and handler references. This is **not** C# compilation.
- `python tests/preview_smoke.py`: passed settings, colors, right position, tasks, manual state, player simulation, keyboard quick-filter, collapse/expand, zero JS page errors.
- Additional Playwright layout verification: at 1440×900, 1366×768, 1024×768, the panel and Preferences fit the viewport (smaller screens may scroll internal content).
- Default theme WCAG contrast calculation: primary text 15:1, secondary text 6.8:1 against base surface. A contrast check rejects too-bright custom surface colors. Five pure .NET policy tests were added but not executed here.
- **Not run:** dotnet restore, C# compiler, .NET suite, Windows shell, WPF render, screen reader or hardware behavior. No new third-party code copied, and no new runtime dependency added.

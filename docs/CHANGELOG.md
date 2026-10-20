# Version 1.0 source release candidate (2026-10-19)

- Updated Windows WPF and core checks to .NET 10 LTS.
- Added one-click `Build-Sidelane.cmd` compile and executable launcher.
- Fixed Unicode shortcut icon truncation to preserve emoji grapheme clusters.
- Added explicit release status and tightened separate-repository publishing automation.
- Native compilation and Windows QA still pending.

# Changes

## v0.5 · Visual restraint and personalization

- Replaced loud section labels, multi-colored app cards, and decorative microcopy with a compact, consistent desktop utility layout.
- New monochrome WPF vector utility icons, restrained single-accent scheme, and consistent typography/spacing.
- Media transport controls moved into a compact inline player, preserving existing session APIs.
- App rows now carry focused/open/manual state explicitly, with quieter rail state indicators.
- Updated Preferences with clearer grouping and four usable color swatches and an accessibility guard for dark surface colors; retained custom accent, surface, font, background image, opacity, placement, and app configuration.
- Dimmed optional wallpaper so text stays readable; added app-search/task-input hints and subtle keyboard-focus styling.
- Rebuilt interactive HTML preview to match the WPF layout; added left/right, compact rail, settings, keyboard, and manual-status demonstrations.
- No new runtime dependency, browser extension, background service, or data storage.

## v0.4.0 — focused code audit (source candidate)

- Removed the aggressive repeated `SetWindowPos` timer and unused interop declarations. Fullscreen support now changes the WPF topmost property only on transitions.
- Removed duplicate initial window height assignment and reduced unchanged window-list UI rebuilds.
- Improved free-mode monitor-edge expansion direction and added geometry regression cases.
- Added app-slot search in the native expanded panel and matched the browser concept preview.
- Added bounded local PNG/JPEG/BMP image validation and memory-conscious bitmap decoding.
- Switched settings editing to a detached copy and commit-on-success file persistence, with startup-registry rollback when saving fails.
- Added duplicate-name cleanup to user-edited JSON, bounded image paths, and safer preference saves from the overlay.
- Closed leaked `Process` handles during periodic window enumeration; added an exit context menu and a safe tray icon fallback.
- Expanded the **prepared** .NET core regression suite from 21 to 32 checks. Native WPF build and .NET tests have **not** been executed here.
- Reviewed WPF UI / ModernWPF (MIT) for design patterns, but intentionally added no runtime dependencies.

**Release status:** Static and browser concept checks pass. A Windows compile, .NET regression run and device smoke test are still required.

# Changelog

## v0.2.0 candidate — October 2026

- Hardened malformed-settings recovery, editable app slots, and link validation.
- Preserved the rail orientation when dragging a right-aligned sidebar into free placement.
- Reduced accidental collapses with a hover-exit delay and Escape shortcut.
- Added accessible task deletion buttons, color presets, and disabled media controls when no session exists.
- Made the media adapter prefer the currently playing session instead of a paused Spotify session.
- Expanded pure .NET core checks and improved GitHub Actions permission boundaries.
- Added contributor/privacy documentation and clearer release gates.

**Release status:** Source candidate only; a successful Windows build and device smoke test remain required.

## v0.3.0 — reliability and ergonomics (source candidate)

- Active/focused app indicator using foreground Windows handles only.
- Fullscreen-aware always-on-top behavior (opt-out available).
- Multi-monitor positioning service with pure geometry regression cases.
- More accurate media transport controls based on Windows playback capabilities.
- Scratchpad completed/total count; task history remains ephemeral.
- Optional per-user login startup, off by default, with Windows Run key.
- Unicode-safe Win32 window title length and tighter test coverage.
- Windows compilation and on-device smoke testing are still required.

- Kept label opacity independent of translucent surfaces for readability; repaired collapsed right-rail browser preview clipping and settings flyout interaction.

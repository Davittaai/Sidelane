# Sidelane v1.3 beta: desktop-first UX

This is a **public beta**, not a fully field-tested stable release.

## What's changed
- Smaller default desktop overlay, 46px rail, 320px width, 560px height; resizable rail, width and height in Preferences.
- Refined spacing and edge styling, searchable app slots when more than six are configured, and a visible temporary scratchpad at the compact size.
- Four paired color palettes, native Windows color pickers, live theme preview, saved accent and background hex fields.
- Shortcut-specific **Auto**, **Browser**, and **App** launch options. Browser: default, Chrome, Edge, Firefox or Brave, plus supported Chromium standalone web-app windows. App: choose local EXE or Windows shortcut (.lnk), including shortcuts to Store apps. No unexpected web fallback from App mode.
- Browser companion reports **Open / Generating / Recently stopped** for supported, opted-in Chrome tabs; repeated signals are aggregated across tabs. Rename shortcuts and assign explicit AI provider if needed. Shows no fictitious progress percentages and reads no prompt/response content.
- One-click activity-note button beside each app to manually describe a task. Notes remain RAM-only and clear on exit.
- Loading/entrance motion and short three-bar animation for browser-observed generation; animated effects can be disabled.
- Custom app icon is used in the executable, taskbar and the sidebar rail. Taskbar visibility can be enabled/disabled.
- Safer per-shortcut launch policy, color contrast checks, WPF layout, and launch/AI provider regression tests.

## Install
Download SidelaneSetup.exe or the portable ZIP from this tagged GitHub release. Both are self-contained x64 Windows packages and have SHA256 sidecars. Unsigned setup may trigger SmartScreen. Existing preferences persist in AppData; activity notes and tasks do not.

## Acceptance limitations
- Provider UI attributes change and the experimental companion requires manual Chrome extension opt-in + native host registration. It **does not** work on arbitrary Firefox/Edge tabs or detect what a chat is writing automatically.
- User-supplied note labels are distinct from observed status. Browser titles, prompts and replies are not collected.
- GitHub CI compilation, launch, installer and screenshot are not substitutes for testing actual user's browser profiles, animations, mixed-DPI monitors, Spotify playback or Windows accessibility tools.

# Sidelane v1.1 beta — desktop hardening preview

An early preview for testers of the small, local-first Windows sidebar. The existing v1.0.0 remains the stable download until real-device acceptance is complete.

## What changed since v1.0.0

- **Temporary per-app activity notes:** right-click a configured app, select **Set activity note**, type up to 96 characters, and see it in the app row and collapsed-rail tooltip. Notes are in memory only and clear on exit.
- **Reliable relaunch:** starting the executable again brings the existing sidebar forward instead of silently exiting.
- **Monitor awareness:** fullscreen activity on another monitor will not suppress the sidebar on its own screen.
- **Modal editing:** settings and activity-note dialogs keep the sidebar expanded until the editor closes.
- **Lower background work:** when the sidebar is hidden it skips unnecessary UI/media refreshes, updating when shown again.
- **Stronger Windows QA:** 44 portable core tests, warning-free WPF build, actual visible 54-pixel rail check, and single-instance launch check in GitHub Actions.

## Install

Extract `Sidelane-win-x64.zip`, double-click `Sidelane.exe`, and use the tray icon to show/hide or exit. This is a self-contained Windows x64 build with no external .NET runtime requirement. Verify the matching SHA256 file before use. It is unsigned; Windows may warn about an unknown publisher.

## Important limitations

This is a **beta release**, not a real-device-tested stable release. Windows CI tests do not exercise Spotify, manual hovering, multi-DPI setups or fullscreen interactions in a personal desktop session. Report those results against [M2](https://github.com/Davittaai/Sidelane/issues/2).

Sidelane does **not** automatically infer whether ChatGPT, Claude, Grok or Codex is generating. Window-open and focused states are observed from Windows; activity notes and Working/Waiting/Ready are user-entered. A consented, optional integration is tracked separately as [M3](https://github.com/Davittaai/Sidelane/issues/4). No chat/task content is saved or sent to servers; preferences are saved locally.

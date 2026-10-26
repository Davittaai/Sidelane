# Sidelane product completion plan

Status: **v1.0.0 stable, v1.2.0-beta.1 engineering preview published; real-device acceptance pending** (2026-10-26).

## Product definition

Sidelane is a free, open-source, self-contained Windows 10/11 (x64) floating sidebar. The default narrow rail opens links or existing app windows. Hover reveals local system media, short-lived task checklists and human-entered activity notes. Appearance settings persist; private session content does not. No cloud account, analytics or external AI credentials.

**Do not claim autonomous AI task/progress detection.** Window-open and foreground indicators are measured; activity/status descriptions are user-entered. Any future automated integration must be opt-in and proven reliable per service.

## P0: Release correctness and safety

- [x] MIT-compatible upstream notice and attribution
- [x] GitHub source and repeatable Windows CI build
- [x] Standalone self-contained Windows x64 build and SHA256 release asset
- [x] 38/38 existing portable core tests, compiler warnings-as-errors, Windows process startup smoke (v1.0.0)
- [x] Native activity note editor with 96-character cap, session-only memory and matching hover tooltip (v1.1 source)
- [x] Session data isolated from saved settings, with dedicated core tests (v1.1 source)
- [x] Second launch focuses an existing sidebar instead of silently doing nothing (v1.1 source)
- [x] Do not suppress a sidebar on monitor A just because monitor B is fullscreen (v1.1 source)
- [x] Skip hidden overlay window/media polling and refresh on reappearance (v1.1 source)
- [x] Improve Windows CI smoke to validate visible collapsed rail and single-instance policy (v1.1 source)
- [x] **Windows CI was green for the v1.1 beta source changes**, including core tests and startup smoke (2026-10-21). Subsequent commits require fresh checks.
- [ ] **Real Windows laptop UI acceptance.** Hover, focus, move, pin, tray, media, settings, startup, fullscreen, second monitor, DPI and alt-tab.
- [ ] Confirm no residual session notes/tasks on app relaunch, no user content in AppData files, and no unexpected outbound network requests.

## P1: Everyday usability

- [ ] Collect real running WPF screenshots at 100%, 125% and 150% scaling (do not substitute HTML concept screenshots).
- [ ] Try 30-minute workflow with 3+ tracked apps, media playing, and five disposable tasks. Document bugs.
- [ ] Test Spotify and at least one other SMTC-capable media app; test absence of media too.
- [ ] Confirm keyboard-only navigation, Escape, context menus, hotkey collision behavior, focus return, and screen-reader names.
- [ ] Verify Windows sign-in autostart and the signed-off placement on a second monitor.
- [ ] Measure **idle** CPU, working set and cold-start time on a representative laptop with test method documented; a six-second CI working set is not idle performance.

## P2: Distribution and documentation

- [x] Public source, license, SECURITY, contribution guide, release notes, SHA256 file, portable ZIP
- [ ] Update native screenshots from completed Windows QA; replace concept preview for marketing only after verified screenshots exist.
- [ ] Test clean-machine unzip/start with no .NET SDK/runtime installed.
- [ ] Review Windows SmartScreen / unsigned publisher experience; signing is optional and costs money, so document warnings instead of claiming signing.
- [ ] Tag and publish the next stable version only after native validation. CI artifacts may be shared as **pre-release builds** meanwhile.
- [ ] Make GitHub repo inviting: feature GIF from actual WPF, short demo video, meaningful tags/topics, precise limitations, troubleshooting, first-time user instructions.

## P3: Optional AI auto-state connector, separate consented scope

- [ ] Feasibility spike for opt-in browser extension / native messaging for one provider, with **no chat text collection**.
- [ ] Show only truthful generating/completed/unknown signals; unknown when unsupported and never guess progress percentages.
- [ ] Explicit privacy review, extension permissions, browser compatibility and service changes before promising support for ChatGPT, Grok or Claude.
- [ ] Keep extension entirely optional; the sidebar must work without it.

## Release gate

Automated CI success means the binary **builds and starts on GitHub Windows**, not that its visual desktop workflow has passed. Close M2 only when the real-device checklist is signed off with OS build, monitors, scaling, screenshots, and pass/fail evidence. Do not silently broaden the no-saving requirement: appearance preferences persist, task/activity contents must not.

## October 22 execution update

- Fixed preview publisher's missing `HEAD_BRANCH` environment variable in `.github/workflows/release-preview.yml`. The previous publish-preview workflow had failed, despite a successful Windows build.
- Added `tests/windows_local_acceptance.ps1` for opt-in, **local-only** executable SHA256, Windows environment and resource measurements. It deliberately omits task text, notes, window titles, browser URLs, and chat content; it does **not** perform automated GUI validation.
- To use it on a physical Windows device: `pwsh -File tests/windows_local_acceptance.ps1 -ExePath ./release/Sidelane.exe -IdleSeconds 300` from the project directory (or supply the actual extracted EXE path). Review the JSON in `qa-local/`; do not commit the report without inspecting it.
- **Release blockers remain:** genuine manual 100/125/150% DPI, second monitor/fullscreen, keyboard and media QA, privacy verification with file/network inspection, real WPF screenshots, and clean-machine install/SmartScreen experience.
- Stable v1.1 promotion must not occur solely because CI is green. The existing v1.1 beta is the testing candidate.

## October 26 engineering milestone

- [x] Configurable rail width 54–76 DIP, panel width 340–520 DIP and height 420–900 DIP; normalized settings.
- [x] Per-user Windows installer/uninstaller, verified by hosted Windows install/uninstall smoke.
- [x] Daily stable-release checks and user-approved installer download with SHA256 verification against official GitHub release.
- [x] Optional Chrome native messaging companion for ChatGPT, Claude and Grok with sender checks, strict state-only payload schema and same-user named-pipe IPC.
- [x] Optimize wallpaper decode/cache, process enumeration and redundant browser status rebuilds; no private browser tab title in tooltips.
- [x] Publish [v1.2.0-beta.1](https://github.com/Davittaai/Sidelane/releases/tag/v1.2.0-beta.1) from [56-test Windows CI](https://github.com/Davittaai/Sidelane/actions/runs/40290976240).
- [ ] Confirm live Chrome provider detection on all three sites, actual user's mixed-DPI Windows acceptance, idle RAM/CPU, and an end-to-end newer-stable-update installation.
**Beta is not the same as complete stable release.**

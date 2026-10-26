# Sidelane milestones

Updated: 2026-10-26

## M0: Publish source and make contributions reproducible

- [x] Publish WPF source, local test harness, preview, build scripts and licenses in a dedicated GitHub repository
- [x] Minimize heavy design screenshots without changing runtime behavior
- [x] Validate XAML structure and browser-preview interactions locally
- [x] Record CI failures and the successful Windows build in [CI results](CI-RESULTS.md)

## M1: Verified Windows portable alpha (current priority)

**Goal:** A downloadable Windows application built from the published source. **Automated release complete with v1.0.0 GitHub assets. Native device QA remains open.**

Acceptance criteria:

1. ✅ GitHub Actions `Windows build` ran successfully on a clean Windows hosted runner.
2. ✅ The `Sidelane.CoreTests` check passed **38/38**, recorded in the logs.
3. ✅ WPF compilation completed with no errors under `-warnaserror`.
4. ✅ The workflow uploaded `Sidelane-win-x64.zip` and its matching SHA256 sidecar. [Successful run](https://github.com/Davittaai/Sidelane/actions/runs/39376540695).
5. ✅ A native Windows runner launched `Sidelane.exe` successfully (process smoke only, not visual QA).
6. ✅ A permanent [v1.0.0 release](https://github.com/Davittaai/Sidelane/releases/tag/v1.0.0) includes compiled ZIP and SHA256 sidecar.
7. A human executes [Windows QA](WINDOWS-QA.md) on real Windows: launch/exit, moving between monitors, fullscreen coexistence, window focus, audio, settings persistence, and no task-data persistence.
8. Complete Windows GUI acceptance tests and record evidence in the dedicated tracking issue. CI smoke does not assert UI correctness.

## M2: Real-device QA and product hardening (next priority)

Capture screenshots of actual running WPF UI, test Spotify/system-media transport, DPI/multi-monitor drag, hotkey/foreground switching, autostart and fullscreen behavior on at least one Windows laptop; measure idle memory and CPU. Fix any defects and publish v1.0.1 if required.

## M3: AI activity integration, privacy-first

Prototype an optional browser companion that sends only explicit state indicators over local IPC. It must never read/store conversation content, use unknown remote endpoints, or claim reliable completion detection until implemented and tested.

## M4: Public portfolio readiness

Add genuine Windows application screenshots, one short demonstration video, first-time setup experience, reproducible builds, and a simple issue template for bug reports. Measure cold-start latency and idle RAM/CPU on a low-powered Windows laptop.

## M5: Installer, resizing, update service, optional Chrome bridge

**Engineering beta published:** [v1.2.0-beta.1](https://github.com/Davittaai/Sidelane/releases/tag/v1.2.0-beta.1). Rail/panel dimensions, Inno setup/uninstall, verified user-consent update downloads, and opt-in native Chrome bridge implemented. The selected Windows CI passed **56 core tests**, native host protocol checks and installer smoke. See [issue #7](https://github.com/Davittaai/Sidelane/issues/7). Provider-specific live-browser detection, physical Windows QA, and end-to-end update acceptance remain unverified.

## Product constraints

Keep no cloud backend, no accounts, no task history, accessible keyboard navigation, a discreet collapse behavior, and a reasonable idle footprint. Avoid adding Electron or a heavy UI package without evidence of benefit.

# Sidelane milestones

Updated: 2026-10-20

## M0: Publish source and make contributions reproducible

- [x] Publish WPF source, local test harness, preview, build scripts and licenses in a dedicated GitHub repository
- [x] Minimize heavy design screenshots without changing runtime behavior
- [x] Validate XAML structure and browser-preview interactions locally
- [ ] Record results of the first Windows CI run (build cannot be validated by the Linux packaging environment)

## M1: Verified Windows portable alpha (current priority)

**Goal:** A downloadable `Sidelane-win-x64.zip` from GitHub Actions that contains a real Windows `Sidelane.exe` built from the published commit.

Acceptance criteria:

1. GitHub Actions `Windows build` runs successfully on a clean Windows hosted runner.
2. The `Sidelane.CoreTests` check passes, with its test count visible in the log.
3. WPF compilation has no errors under `-warnaserror`.
4. The workflow outputs `Sidelane-win-x64.zip` and matching SHA256 sidecar as a build artifact.
5. A human executes [Windows QA](WINDOWS-QA.md) on real Windows: launch/exit, moving between monitors, fullscreen coexistence, window focus, audio, settings persistence, and no task-data persistence.
6. Publish the first public release *only after* native QA. Do not make the stable `v1.0.0` tag before that point.

## M2: AI activity integration, privacy-first

Prototype an optional browser companion that sends only explicit state indicators over local IPC. It must never read/store conversation content, use unknown remote endpoints, or claim reliable completion detection until implemented and tested.

## M3: Public portfolio readiness

Add genuine Windows application screenshots, one short demonstration video, first-time setup experience, reproducible builds, and a simple issue template for bug reports. Measure cold-start latency and idle RAM/CPU on a low-powered Windows laptop.

## Product constraints

Keep no cloud backend, no accounts, no task history, accessible keyboard navigation, a discreet collapse behavior, and a reasonable idle footprint. Avoid adding Electron or a heavy UI package without evidence of benefit.

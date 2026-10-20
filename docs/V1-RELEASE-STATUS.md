# Sidelane v1.0 build status

**Updated 20 October 2026**

**Sidelane v1.0.0 is publicly released** with a self-contained Windows x64 ZIP and a SHA256 checksum: https://github.com/Davittaai/Sidelane/releases/tag/v1.0.0. The Windows CI passed native process startup smoke, but hands-on GUI testing on a personal desktop has **not** been completed.

## Verified

- [x] Public source, MIT license, third-party notices, build/test scripts and design concept checked into GitHub.
- [x] Static markup and handler checks on Linux and Windows runner.
- [x] Browser concept interactions checked independently; not a WPF runtime test.
- [x] .NET core regression suite passed: **38/38**.
- [x] .NET 10 WPF build with warnings as errors passed.
- [x] Self-contained `win-x64` publish succeeded.
- [x] GitHub Actions artifact ZIP contains `Sidelane.exe` and SHA256 sidecar.
- [x] Outer and inner artifacts verified against archive integrity checks and hash; `Sidelane.exe` has valid MZ/PE headers.
- [x] Native Windows executable started and remained running in Windows GitHub Actions smoke gate.
- [x] Public v1.0.0 Release assets published with checksum after CI success.

Evidence: [successful Windows CI](https://github.com/Davittaai/Sidelane/actions/runs/39376540695), commit `874cbd5`, [portable artifact](https://github.com/Davittaai/Sidelane/actions/runs/39376540695/artifacts/12243850722).

## Follow-up hands-on device verification

- [ ] Install/extract and launch on a real Windows 11 PC, without the .NET SDK.
- [ ] Test exit/notification tray, shortcut selection, hotkey, drag, hover and collapse.
- [ ] Test media session controls against installed Spotify or another compatible media app.
- [ ] Test fullscreen windows, scaling and multi-monitor placement.
- [ ] Confirm temporary tasks disappear on exit but preferences persist.
- [ ] Inspect actual native WPF screenshots and app resource usage.
- [x] Published v1.0.0 assets. Additional hands-on QA remains tracked as a follow-up, not falsely certified.

**Unsigned app warning:** Windows SmartScreen may flag this new unsigned application. Do not bypass security warnings on untrusted binaries; compare downloads with their SHA256 checksum and build from source when preferred.

Latest release workflow: https://github.com/Davittaai/Sidelane/actions/runs/39454362476.

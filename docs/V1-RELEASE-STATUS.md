# Sidelane v1.0: honest release status

**19 October 2026** — this is the version 1.0 source distribution, NOT a verified Windows executable release.

## Implemented
- Portable 54px rail that expands on hover, relocatable and configurable.
- Window/title/process matching and one-click application switching.
- Windows system media metadata and playback controls, when exposed by the media player.
- In-memory-only temporary task list and manual AI progress/status annotations.
- Adjustable placement, font, color presets, opacity, and local background image.
- Local JSON settings persistence, user-configured startup option, fullscreen suppression.
- .NET 10 LTS targeting and a one-click Windows build script.
- GitHub Actions Windows build, download artifact, and tag-triggered release configuration.

## Actually verified in this environment
- XML parse and event-handler references using `python tests/static_check.py`.
- Interactive browser design study using `python tests/preview_smoke.py`.
- Source archive file listing, checksums, and Git bundle validation.

## Still not verified
- Native WPF compilation, .NET regression test execution, single-file Windows publish.
- Multi-monitor Windows behavior, app focus, system media transport controls, tray and hotkey.
- SmartScreen behavior of unsigned executable.
- Public GitHub repository and release artifact publication.

## Finish release on Windows
1. On a supported Windows PC, install the official .NET 10 SDK from https://dotnet.microsoft.com/en-us/download/dotnet/10.0 if missing.
2. Extract the source ZIP and double-click `Build-Sidelane.cmd`.
3. Run `release/Sidelane.exe` and complete the cases in `docs/WINDOWS-QA.md`.
4. Run `publish-to-github.ps1` from PowerShell in the source root, after GitHub CLI authentication, to create `Davittaai/Sidelane`.
5. Confirm the GitHub Actions Windows build and download its artifact.
6. Only after native testing, tag `v1.0.0` to publish a GitHub release ZIP and checksum.

Do not distribute a build under the word 'tested' until these checks pass.

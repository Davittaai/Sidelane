# Sidelane release gates

Current engineering beta: **v1.2.0-beta.1**, 2026-10-26.

## Automated gates completed

- [x] Source published with MIT attribution and contributor/security documentation
- [x] 56/56 .NET core checks on the selected beta commit
- [x] Static XAML/event checks, Chrome JS syntax checks, extension manifest and PowerShell parser checks
- [x] Native WPF warning-as-error Windows build and process startup smoke
- [x] Native host console framed-protocol smoke test (no-app fails closed)
- [x] Inno per-user setup compilation and silent installation/uninstallation
- [x] Portable ZIP + setup file, each with independent SHA256 sidecar, released as beta
- [x] Optional local Chrome bridge documented; no account/chat collection built into protocol

[Successful Windows CI](https://github.com/Davittaai/Sidelane/actions/runs/40290976240) · [v1.2 beta release](https://github.com/Davittaai/Sidelane/releases/tag/v1.2.0-beta.1)

## Stable release blockers

- [ ] Real Windows laptop end-to-end acceptance at 100%, 125%, and 150% DPI / dual monitors
- [ ] Media controls with Spotify and another supported player
- [ ] Test native messaging pairing with real Chrome and three provider accounts; confirm Stop-control compatibility
- [ ] Observe permissions and outbound traffic; verify no prompt/response contents saved or sent by companion
- [ ] Test the user-visible download, SHA256 verification, consent and update-install flow with a newer stable release
- [ ] Clean-machine installer, SmartScreen, autostart and uninstall check outside GitHub CI
- [ ] Genuine demonstration recording and measured 5-minute idle resource usage

Do not call the optional browser bridge generally compatible or mark stable v1.2 without these tests. The v1.0.0 stable release remains available.

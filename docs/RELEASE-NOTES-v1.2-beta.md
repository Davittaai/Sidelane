# Sidelane v1.2 beta

This is a **pre-release for testing**, not a fully validated stable version.

## Added
- Adjustable collapsed rail width (54–76 DIP), expanded width (340–520 DIP), and panel height (420–900 DIP), persisted in Preferences.
- Per-user SidelaneSetup.exe (Inno Setup), Start Menu shortcut, optional desktop shortcut, and Windows uninstall entry; portable ZIP stays available.
- Automatic checks of the official stable GitHub release feed, with user consent and SHA256 verification before downloading and launching an official setup. No silent replacement.
- Optional **experimental** Chrome companion for ChatGPT, Claude, Grok via an explicitly registered native messaging host. Only ephemeral status signals are accepted; no prompts, replies or history are collected.
- Performance changes: deduplicated process queries, 800 px cached background decode, one-time AI signal expiry refresh, reduced tooltip exposure.

## Validation
CI checks WPF build, core regressions, native Windows launch, host publish, installer compilation, silent install and silent uninstall. Real-device multi-monitor/Spotify/Chrome integration must still be completed before stable sign-off.

## Limitations
- Automatic AI detection depends on volatile browser UI Stop controls and may display Unknown when unsupported. No task progress percentages.
- Browser companion requires manual unpacked extension permission and one-time registration; it is not auto-installed.
- App and installer are unsigned, and Windows SmartScreen may warn.
- New release checks contact only `api.github.com`. Update downloads are verified against GitHub-published SHA256 before opening the installer.

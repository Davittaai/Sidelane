# Sidelane v2.0.0-beta.1 acceptance

## Engineering release candidate
Selected from merged PR #12 on 2026-10-26. This branch exists only to trigger verified Windows CI, packaged ZIP/installer and the checksum-verified GitHub prerelease publisher.

Key features: native guided first-run, local app discovery, explicit browser-vs-installed-app routing, customizable global hotkey/collision warning, keyboard filtering, crisp artwork, and coherent theme cards.

Before publishing, require: all core regression checks, WPF warnings-as-errors compile, native process and singleton startup, native messaging protocol check, Chrome JS/manifest validation, actual first-run WPF screenshot, Inno setup compilation, silent setup/uninstall, SHA256 sidecars.

**Still manual before stable v2:** live Chrome-provider activity, installed apps including Microsoft Store, media playback, keyboard and DPI, 5-minute idle RAM and CPU, installer upgrade path. See docs/V2-DELIVERY-AUDIT.md and issue #13.

Do not describe this beta as stable, signed, or capable of reading AI task details.

# Sidelane v2.0.0 Beta 1 — Your AI workspace, ready from first launch

**Beta release.** The v1.3 lineup was functional but demanded manual setup. v2 changes the first-run experience and reliability while keeping the native Windows application small and local-first.

## New

- A dedicated welcome screen for selecting your AI tools, default browser, theme and taskbar visibility before you start. Existing users are not forced through onboarding after an upgrade.
- Local-only discovery of recognized Start Menu shortcuts for ChatGPT, Claude, Grok and VS Code; detection does **not** run or inspect apps, and unknown apps are not automatically added.
- Quick Setup available again from the tray. A confirmation protects existing custom configurations.
- Configurable system hotkey: Ctrl+Alt+Space, Ctrl+Shift+Space or Disabled. Windows shortcut registration collisions are reported rather than silently ignored.
- Better coordinated theme colors for sidebar background, rail, dividers, media card and loading indicator.
- Existing v1.3 features remain: compact panel sizing, four theme presets/custom color picker, per-tool launch routing, temporary task notes, system media controls and verified user-approved release updates.
- Installer and portable ZIP, each checked with GitHub-published SHA256.

## Privacy

Profiles and shortcut configuration are local; temporary task names are **not saved**. A separately installed Chrome companion can observe 'Open', 'Generating' and 'Recently stopped' on supported provider pages. It cannot read or prove what tasks are being worked on. No account, chat upload, analytics or backend was added in v2.

## Known limitations

The optional Chrome companion is experimental and not automatically installed. Unsigned Windows setup may prompt SmartScreen. Cross-monitor DPI, real provider UI, media sessions and extended idle resource usage must be validated on real devices before labeling a stable v2 release.

[Repository](https://github.com/Davittaai/Sidelane) · [v2 gap and QA audit](docs/V2-DELIVERY-AUDIT.md)

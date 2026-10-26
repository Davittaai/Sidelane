# M7 upgrade / remove / add audit

**Date:** 2026-10-26 · **Basis:** Sidelane v1.3.0-beta.1 code review, existing GitHub issues, Microsoft PowerToys Command Palette and Flow Launcher feature patterns.

**Product positioning:** Sidelane is a compact desktop companion for AI multitasking. It should not become another comprehensive app/file launcher or cloud dashboard.

## Upgrade now

| Priority | Area | Finding | Action / acceptance |
|---|---|---|---|
| P0 | Robust launch | Auto refused a configured website when the local EXE/shortcut path was stale | Fix fallback, keep strict App behavior, add regression tests |
| P0 | Recovery | No first-party way to export/import shortcuts and themes across devices | File-based import/export with confirmation and validation, no session notes |
| P1 | Theme integrity | Main background follows custom color, but rail/divider and loading bar accent were fixed | Derive rail/divider colors from surface, preserve contrast, color the loader |
| P1 | Release QA | Native startup/installer CI passes, but no real device/Spotify/Chrome integration sign-off | Repeat acceptance on a Windows laptop before stable version |
| P1 | Input feedback | Invalid executable selection, missing browser and hotkey conflicts need actionable errors | Show what failed and route directly to Preferences |
| P1 | Idle performance | Hosted Windows runner measured ~137 MiB shortly after start, not idle RAM | Run 5-min realistic idle CPU/working-set benchmark before optimizing |

## Add when tested

- **First-run onboarding:** discover Start Menu app shortcuts locally, choose launch behavior and recommended browser without manually finding EXEs. Zero cloud integration.
- **Customizable hotkey with collision warning:** do not silently register a conflicting shortcut, and expose a disable toggle.
- **Connection status:** explain that Chrome companion is optional and that unknown is not 'idle' or 'done'; test per provider.
- **Portable profile move:** import/export feature should remain human-controlled and should never persist transient task/note contents.
- **Reduced motion/keyboard focus:** honor Windows motion preferences, escape behavior and screen-reader names.

## Remove or simplify

- Retire the legacy v0.5 HTML-only simulated UI as the **primary QA reference**. The actual WPF screenshots/tests now govern releases. Archive the concept rather than silently presenting it as the current app.
- Stop duplicating old release claims across README, docs/PRODUCT-COMPLETION.md and multiple beta notes. Keep current README and versioned release notes canonical; treat historic QA records as archived.
- Do **not** add another theme framework, an Electron runtime, a plugin marketplace, a cloud login, online telemetry, or fake AI completion percentages.
- Keep optional browser integration isolated: never elevate experimental UI signals to trusted task metadata without consent and real provider validation.

## Research references

- [Microsoft PowerToys Command Palette Settings](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/settings): configurable activation, compact mode, fullscreen handling, appearance and motion controls.
- [PowerToys 0.101 release](https://devblogs.microsoft.com/commandline/powertoys-0-101-is-here-window-hopper-insider-updates-and-command-palette-compact-mode/): compact launcher UX and per-app window switching.
- [Flow Launcher](https://github.com/Flow-Launcher/Flow.Launcher): keyboard-driven tool switching and extensibility; scope is deliberately much broader than Sidelane's.

**Shipping policy:** no stable v1.4/v1.3.1 release until code compiles with warnings-as-errors, automated regressions and installer smoke pass. Native Windows and live-browser acceptance remain separate evidence gates.

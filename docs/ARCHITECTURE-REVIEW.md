# v0.4 architecture and upstream review

## Upstream projects considered

- **Quick Apps Overlay** (`vhvb1989/quick-apps-overlay`): MIT, existing attributed Win32 enumeration/focus foundation. Kept only code relevant to native window switching. Preserve `THIRD_PARTY_NOTICES.md`.
- **WPF UI** (`lepoco/wpfui`): MIT, full Fluent WPF component framework. Reviewed interface and styling ideas. **Not added**: the narrow overlay needs only a handful of controls, and a new dependency increases deployment/maintenance surface.
- **ModernWPF** (`Kinnara/ModernWpf`): MIT, extensive Fluent-style WPF controls. **Not added** for the same scope/size reason. Its maintained 1.x preview track may be useful if Sidelane becomes a larger desktop settings app.
- **Zebar** (`glzr-io/zebar`): customizable webview widget system. Observed conceptually; no code copied. Sidelane remains a native single-purpose overlay rather than a widget host.

## What was removed or simplified

1. Removed 850 ms topmost enforcement timer and direct `SetWindowPos` calls. Focus polling now updates topmost on fullscreen state change, without constantly fighting other windows.
2. Removed duplicate initial window height calculation and an unconditional periodic app-row rebuild when the enumerated window list has not changed.
3. Removed unused Win32 topmost constants and cleaned up process handles that were allocated during window enumeration.
4. Reduced settings mutation risk: editor works on a detached copy. A failed JSON write does not replace in-memory preferences. Optional autostart registry change is rolled back on file-save failures.
5. Added a small app search textbox instead of adopting a search framework or third-party control library.
6. Separated pure image-input validation from Windows-specific decoding so that input constraints can be unit tested cross-platform. Large/unsupported images no longer get passed to WPF for decoding.

## Deliberate boundaries

- No analytics, browser scraping, chat contents, cloud authentication, or automatic AI activity claims.
- No additions to the NuGet dependency graph.
- Scratchpad and manual status labels stay in RAM, not settings JSON.
- No executable release until native Windows CI and hands-on device smoke tests pass.

## Key source ownership

`OverlayWindow` composes UI only; `ScreenPlacement` encapsulates testable geometry; `SettingsService` owns JSON persistence and normalization; `ImageFilePolicy` validates images without native dependencies; `BackgroundImagePolicy` performs WPF decoding only; `WindowService` enumerates and activates actual Windows windows; `MediaService` exposes OS-provided media transport controls.

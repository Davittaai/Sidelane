# Design and launch reference review

Reviewed MIT-licensed projects while planning Sidelane v1.3:
- [WPF UI](https://github.com/lepoco/wpfui), Fluent layout, navigation and spacing patterns (MIT).
- [PixiEditor ColorPicker](https://github.com/PixiEditor/ColorPicker), HSV picker and accessible color preview patterns (MIT).
- [BrowserPicker](https://github.com/mortenn/BrowserPicker), explicitly configured browser routing principles (check upstream LICENSE before any direct reuse).

The implementation uses WPF built-in controls and native Windows APIs, borrowing the UX patterns, **not vendoring entire UI frameworks**. This keeps the binary lean, avoids theme conflicts, and supports Windows 10. Browser process launch validation uses an original isolated `LaunchPolicy` service with structured `ArgumentList` to avoid shell command injection. No third-party code fragments were copied without attribution.

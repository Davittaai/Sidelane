# Sidelane design system, v0.5

The primary interaction is **glance -> switch -> leave**. Sidelane is not a second dashboard. A quiet 54px rail should remain legible when collapsed, and the 360px panel should contain only three areas: Apps, Listening, and Scratchpad.

## UI decisions

- **Native-feeling typography:** Segoe UI first, 15px window name, 13px section names, 12px row labels, and 10px supporting status. No decorative editorial headers.
- **One accent:** sage (`#B9D8BF`) is used for the logo, focused/manual state, primary settings action, and selection. Users can override it with a hex value or choose Blue, Amber, or Lilac.
- **Calm neutral surfaces:** `#191C20` panel, `#202226` rail, `#25282D` media surface, `#34373C` subtle dividers. Only the compact media player needs a container.
- **Hierarchy through spacing:** minimal separators between three functional areas; apps are unboxed 48px rows with a muted, explicit second line.
- **Restrained interactions:** row hover uses a tonal background, no glowing cards, bouncing icons, decorative gradients, or always-on pulsing states.
- **Contrast at the default theme:** primary `#F0F0EE` against `#191C20` is about **15:1**, muted `#A0A3AA` against `#191C20` is about **6.8:1** by the WCAG relative-luminance formula. The Settings screen now accepts custom dark surfaces only when they preserve at least 4.5:1 contrast for secondary text. Full light mode is not yet supported.
- **Accessible intent:** descriptive tooltips/automation labels, visible focus outlines, disabled media controls, system fonts, Escape-to-collapse, native checkbox/input focus, and browser-preview reduced-motion support.
- **Honest states:** automatic `Open` / `In focus` comes from Windows; Working, Waiting, Ready are always explicitly labeled `manual`. The browser preview is simulated.
- **Privacy-first:** task contents and manual statuses only live in memory. Appearance/preferences persist locally; no cloud telemetry.

## Visual sources reviewed

- [Microsoft Fluent Windows design overview](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/design/index.md): platform familiarity, useful controls, predictable layout and focus states.
- [Microsoft Fluent 2 layout](https://fluent2.microsoft.design/layout): logical groups using consistent proximity and whitespace instead of excess divider lines.
- [Raycast extension UI guidelines](https://github.com/raycast/extensions/blob/main/docs/basics/prepare-an-extension-for-store.md): concise, consistent actions and preferences discoverability. Inspiration only, no Raycast assets or code adopted.
- [Microsoft Fluent UI tokens](https://github.com/microsoft/fluentui): systematic roles for spacing/color and semantic foregrounds. No dependency added.

**Implementation note:** All icons in the fixed controls are inlined WPF vector geometry or custom XAML shapes; no icon font or downloaded third-party pack is required. User app symbols remain editable. No third-party UI code was copied into this redesign.

## Release caveats

- The HTML is an **interactive design study** with sample data, not a compiled WPF application.
- The WPF markup and C# code were edited together. Markup parsing and handler checks pass on Linux, but actual WPF build, Windows media integration, mixed DPI, and accessibility UI Automation inspection still require the Windows CI/hardware gate.

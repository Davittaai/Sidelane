# Sidelane 1.3 beta QA candidate

Based on merged PR #8 (compact UI, native app and browser launch routing, provider status clarity, taskbar and icon, subtle motion, improved preferences).

Automated CI required: 73+ core tests, WPF warnings-as-errors build, native host message framing, Windows rail launch, actual UI screenshot, installer compilation/install/uninstall, ZIP and installer SHA256.

Manual acceptance outstanding: mixed-DPI multi-monitor, Chrome/Claude/Grok live browser providers, packaged Windows Store apps via .lnk, Spotify, screen readers, taskbar interactions and user-chosen default/specific browser on a real Windows laptop.

The browser extension is opt-in and **does not read conversation content**. Users enter task labels themselves; generating/idle signals are browser-control observations. No invented percentages.

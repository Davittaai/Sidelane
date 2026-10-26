# v1.3 Beta 2 release gate

This branch delivers M7 reliability and personalization upgrades following PR #10:
- recover Auto launch when the chosen installed app path is missing; preserve strict App mode
- apply custom background palettes to rail/dividers and loader accent
- export/import settings locally with confirmation and no temporary task data
- open app picker in the local Start Menu folder

Release only after passing .NET core tests, warnings-as-errors WPF build, native Windows process startup, native messaging host framing and silent installer install/uninstall; provide SHA256 for ZIP and setup. Real-device AI/Spotify/DPI testing remains outstanding. Beta is not stable.

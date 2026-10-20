# Interactive interface study

Open `index.html` in a browser to explore the v0.5 Sidelane visual design. Hover or click the rail to expand; filter app slots, add temporary tasks, open Preferences, change colors or placement, and right-click an app row to set its **manual** status.

All application labels and playback metadata are **simulated**. This is not a wrapper for the Windows native UI, nor does the preview connect to local system media. The actual desktop app lives under `src/Sidelane/`.

Run `python tests/preview_smoke.py` to exercise the preview in Chromium with Playwright.

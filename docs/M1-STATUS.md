# M1, Windows portable release gate

Verification sequence:

1. The core regression harness runs on a Windows GitHub Actions runner.
2. WPF source must compile with warnings treated as errors.
3. A self-contained native Windows x64 executable is published.
4. The Windows CI runner launches the executable and checks that it stays running for at least six seconds. This is only a process startup smoke test, not a visual inspection.
5. The ZIP is validated and its SHA256 is generated and uploaded.
6. Only a **successful** Windows workflow on the controlled `release/v1.0.0` branch triggers the release publisher.
7. The release publisher downloads that precise successful run's assets and verifies the checksum before creating GitHub Release `v1.0.0`.

Human acceptance checks that **cannot** be certified by Windows CI: actual fullscreen layering, hover comfort, window switching reliability across apps, media transport against real Spotify sessions, multi-monitor scaling, startup-on-login persistence and visual screenshots of WPF on a user's desktop. Track these in `docs/WINDOWS-QA.md` and do not conflate them with the automated smoke.

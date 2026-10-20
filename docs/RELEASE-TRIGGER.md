# Sidelane v1.0.0 release trigger

This branch intentionally selects commit **88a4fc2** for the v1 release pipeline.

The release publisher runs only after a successful Windows build on this branch. Required checks: core tests, warning-free WPF build, Windows process startup smoke, self-contained packaging, and SHA256 verification.

Automated smoke is not a substitute for manual GUI QA. See [docs/WINDOWS-QA.md](../WINDOWS-QA.md) and [release notes](./RELEASE-NOTES-v1.md).

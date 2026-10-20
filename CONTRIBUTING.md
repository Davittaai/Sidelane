# Contributing to Sidelane

Sidelane is a small, Windows-first desktop utility. Please keep PRs focused and lightweight.

## Setup

- Windows 10 (19041+) or 11
- .NET 8 SDK
- `dotnet run --project src/Sidelane/Sidelane.csproj`
- `dotnet run --project tests/Sidelane.CoreTests/Sidelane.CoreTests.csproj`

## Changes

1. Create a feature branch, explain the user-facing change and its privacy impact.
2. Prefer built-in .NET/Win32 APIs over unreviewed dependencies and background services.
3. Run `dotnet build src/Sidelane/Sidelane.csproj -c Release -warnaserror` on Windows.
4. Test the overlay over a maximized application, on both edges, and from the system tray.
5. Preserve the MIT attribution in `THIRD_PARTY_NOTICES.md`.

Do not add telemetry, stored task contents, browser scraping, or AI model output monitoring without a clearly documented opt-in/privacy design review.

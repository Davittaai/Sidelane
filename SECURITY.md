# Security & privacy

Report suspected vulnerabilities privately through GitHub's **Report a vulnerability** feature when enabled, or contact the maintainer directly. Avoid posting exploit or private user data in public issues.

Sidelane does not read prompt content, record keystrokes, inject into browser pages, or upload user settings. Preferences are stored locally in `%APPDATA%\Sidelane\settings.json`; scratchpad tasks and manually entered AI status labels are kept only in RAM.

Configured shortcut URLs are only launched on explicit click and are restricted to HTTP/HTTPS without embedded credentials. Windows media information comes from the system media-session interface. The Windows executable is currently unsigned; review the source and build it locally if trust is required.

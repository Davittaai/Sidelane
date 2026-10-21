# Optional AI status integration feasibility (research phase)

Updated: 2026-10-21. This is **research, not a shipped integration**.

## User goal

Users want to see whether a selected ChatGPT, Claude, or Grok tab is currently generating a response. Today Sidelane can observe a matching open/focused Windows app and allows manual statuses and session-only notes. It **cannot** reliably see browser tab generation states without extra permission. It must not claim percentages or task text it does not know.

## Evidence and compatibility

Chrome's MV3 extensions support opt-in site access and an `activeTab`-based permission model. Content scripts may observe UI controls on an authorized page; they cannot directly invoke native messaging. A service worker acts as the bridge. Chrome native messaging requires a locally installed and explicitly registered host with a specific extension ID on its `allowed_origins` list. User consent and explicit installation are necessary.

Reference implementations and documentation:
- Chrome official native messaging: https://developer.chrome.com/docs/extensions/develop/concepts/native-messaging
- Chrome official permissions: https://developer.chrome.com/docs/extensions/develop/concepts/declare-permissions
- Chrome public sample host: https://github.com/GoogleChrome/chrome-extensions-samples/tree/main/api-samples/nativeMessaging

## Safest proposed architecture

1. Keep the desktop app fully functional without an extension.
2. Create an **optional** MV3 extension that requests permission for exactly one supported provider after a clear user action.
3. In its content script, inspect only whether a documented UI *control indicating generation* exists. **Do not access message bodies, prompts, replies, network payloads, cookies, or titles.**
4. Pass provider ID plus `generating` / `completed` / `unknown` through the extension worker to a native messaging host registered for that extension ID only.
5. The host validates length, origin and schema before reporting the signal to the existing desktop process over a private local IPC channel. It should not expose an unauthenticated web service or persist signals.
6. The sidebar applies a short time-to-live and falls back to `unknown` when signals cease or permission is revoked. Manual notes stay separate from automatic signals.
7. No analytics, telemetry, account tokens, transcripts or histories.

## Acceptance before implementation or release

- [ ] Confirm with on-device observation that *one* provider exposes a stable, accurately named generation indicator. Never guess selectors from an outdated example.
- [ ] Implement explicit permissions, worker bridge and Windows host in a separate experimental package.
- [ ] Test on the supported Chrome versions with active, background, closed and permission-revoked tabs.
- [ ] Validate no user content is read, logged, saved, transmitted or added to crash reports.
- [ ] Confirm Windows host registration/uninstallation does not require administrator permission and cleans up safely.
- [ ] Validate UI unknown-state semantics and extension absence.
- [ ] Conduct a license/security review and user opt-in documentation before enabling it by default.

## Decision

**Do not ship speculative automatic AI status detection in stable v1.** A manual activity note is honest and immediately useful. Research suggests an opt-in local bridge is technically feasible, but provider UI signals are brittle and demand real-browser validation. Track implementation in issue #4 after the native desktop QA milestone is accepted.
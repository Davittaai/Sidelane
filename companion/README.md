# Optional browser activity companion (experimental)

This feature is **not required** for Sidelane, and is not yet validated on all live provider interfaces. It reports only **Generating**, **Recently stopped**, or **Unknown**. It cannot measure percentage progress, read task names, or prove a response completed successfully.

## Enable in Google Chrome

1. Install Sidelane v1.2 beta using `SidelaneSetup.exe` or the extracted portable ZIP.
2. In Chrome open `chrome://extensions`, enable Developer mode, click **Load unpacked** and choose the `companion/chrome` folder installed beside Sidelane.
3. Copy the extension's 32-character ID (letters a to p).
4. In PowerShell, from the Sidelane installation folder, run:
   `powershell -NoProfile -File ".\companion\Register-Bridge.ps1" -ExtensionId YOUR_EXTENSION_ID`
5. Restart Chrome. Open ChatGPT, Claude, or Grok and trigger a response. The sidebar displays **Generating · browser** only when the corresponding page exposes a recognizable Stop control.
6. Remove the extension and run `companion/Remove-Bridge.ps1` to disable. Uninstalling Sidelane also removes its per-user native-messaging registration.

No admin access is required. Chrome requests access only to the three listed sites and native messaging. The extension inspects button attributes, **not** messages, prompts, replies, cookies, or traffic. The background worker validates the sender origin before sending {provider,state}; the native host validates these values again, and sends them via a same-user Windows named pipe. No signals are written to disk, and signals expire after 12 seconds.

**Limitations:** Provider HTML changes can break detection. Disappearance of a Stop button is shown as 'Recently stopped', not 'Completed', because users may have cancelled. Multiple tabs and browser restarts require manual testing. Until such tests pass, keep this companion experimental and opt-in.

Chrome messaging API: https://developer.chrome.com/docs/extensions/develop/concepts/native-messaging

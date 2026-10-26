"use strict";
const hosts = Object.freeze({
  "chatgpt.com": "ChatGPT",
  "claude.ai": "Claude",
  "grok.com": "Grok"
});
let port = null;
function getPort() {
  if (port) return port;
  port = chrome.runtime.connectNative("com.sidelane.activity");
  port.onDisconnect.addListener(() => { port = null; });
  return port;
}
chrome.runtime.onMessage.addListener((message, sender) => {
  // Never accept provider names from page scripts or unverified senders.
  if (!sender.tab || !sender.url || !message || message.type !== "status") return;
  let url;
  try { url = new URL(sender.url); } catch { return; }
  const provider = hosts[url.hostname];
  if (url.protocol !== "https:" || !provider) return;
  if (!["generating", "unknown", "stopped"].includes(message.state)) return;
  getPort().postMessage({provider, state: message.state});
});

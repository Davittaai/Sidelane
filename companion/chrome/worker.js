"use strict";
// Status is aggregated across tabs rather than letting an idle tab overwrite an active one.
// Content, prompts and replies are never read or transmitted.
const hosts = Object.freeze({ "chatgpt.com":"ChatGPT", "claude.ai":"Claude", "grok.com":"Grok" });
const tabs = new Map();
const last = new Map();
let port = null;
function getPort() {
  if (port) return port;
  port = chrome.runtime.connectNative("com.sidelane.activity");
  port.onDisconnect.addListener(() => { port = null; });
  return port;
}
function publish(provider) {
  const now = Date.now();
  for (const [id, status] of tabs) if (now - status.time > 12000) tabs.delete(id);
  const records = [...tabs.values()].filter(x => x.provider === provider);
  const state = records.some(x => x.state === "generating") ? "generating"
    : records.some(x => x.state === "stopped") ? "stopped"
    : records.length ? "open" : "unknown";
  const previous = last.get(provider);
  if (previous && previous.state === state && now - previous.time < 3000) return;
  last.set(provider, {state,time:now});
  try { getPort().postMessage({provider, state}); } catch { port = null; }
}
chrome.runtime.onMessage.addListener((message, sender) => {
  if (!sender.tab || !Number.isInteger(sender.tab.id) || !sender.url ||
      !message || message.type !== "status") return;
  let url;
  try { url = new URL(sender.url); } catch { return; }
  const provider = hosts[url.hostname];
  if (url.protocol !== "https:" || !provider ||
      !["generating","open","stopped","unknown"].includes(message.state)) return;
  tabs.set(sender.tab.id, {provider, state:message.state, time:Date.now()});
  publish(provider);
});
chrome.tabs.onRemoved.addListener(tabId => {
  const value = tabs.get(tabId);
  if (!value) return;
  tabs.delete(tabId);
  publish(value.provider);
});
chrome.tabs.onUpdated.addListener((tabId, changes) => {
  if (changes.status !== "loading") return;
  const value = tabs.get(tabId);
  if (!value) return;
  tabs.delete(tabId);
  publish(value.provider);
});

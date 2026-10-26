"use strict";
// An intentionally narrow heuristic. It inspects only control attributes,
// never prompt text, responses, cookies, conversation titles or network requests.
// Browser UI changes can make an observation unknown.
let previous = "unknown";
let lastSent = 0;
let scheduled = false;
function detect() {
  const buttons = document.querySelectorAll("button[aria-label],button[data-testid]");
  for (const button of buttons) {
    const label = (button.getAttribute("aria-label") || "").toLowerCase();
    const testId = (button.getAttribute("data-testid") || "").toLowerCase();
    if (testId === "stop-button" || /^stop (generating|response|generation|streaming)$/.test(label)) return "generating";
  }
  return "unknown";
}
function observe() {
  scheduled = false;
  const state = detect();
  const now = Date.now();
  // A disappearing Stop control is suggestive, not proof that a request completed.
  const signal = previous === "generating" && state === "unknown" ? "stopped" : state;
  if (signal !== previous || now - lastSent >= 4000) {
    try { chrome.runtime.sendMessage({type: "status", state: signal}); } catch {}
    lastSent = now;
  }
  previous = state;
}
function schedule() {
  if (scheduled) return;
  scheduled = true;
  setTimeout(observe, 250);
}
const mutation = new MutationObserver(schedule);
mutation.observe(document.documentElement, {childList: true, subtree: true, attributes: true,
  attributeFilter: ["aria-label", "data-testid"]});
setInterval(observe, 4000);
observe();

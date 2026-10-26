using System.Diagnostics;
using System.IO;
using Sidelane.Models;

namespace Sidelane.Services;

/// <summary>Explicit per-shortcut launch policy; never interprets URLs as shell commands.</summary>
public static class LaunchPolicy
{
    public static readonly string[] Modes = ["Auto", "Browser", "App"];
    public static readonly string[] Browsers = ["Default", "Chrome", "Edge", "Firefox", "Brave"];

    public static bool IsValidMode(string? mode) => mode is "Auto" or "Browser" or "App";
    public static bool IsValidBrowser(string? browser) =>
        browser is "Default" or "Chrome" or "Edge" or "Firefox" or "Brave";

    public static ProcessStartInfo? Create(ShortcutSettings shortcut, Func<string, string?> browserResolver)
    {
        var mode = IsValidMode(shortcut.LaunchMode) ? shortcut.LaunchMode : "Auto";
        // App mode is strict: never surprise the user by opening a browser.
        // Auto mode may fall back to the website if the local app moved or was uninstalled.
        if (mode is "App" or "Auto")
        {
            var path = shortcut.AppPath ?? "";
            var valid = Path.IsPathFullyQualified(path) &&
                (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)) &&
                File.Exists(path);
            if (valid)
                return new ProcessStartInfo(path)
                    { UseShellExecute = path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) };
            if (mode == "App") return null;
        }
        if (!LinkPolicy.TryGetWebTarget(shortcut.Url, out var uri) || uri is null) return null;
        var browser = IsValidBrowser(shortcut.Browser) ? shortcut.Browser : "Default";
        if (browser == "Default")
            return new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true };
        var path = browserResolver(browser);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        var start = new ProcessStartInfo(path) { UseShellExecute = false };
        // Explicit argument tokens, never string concatenation to a shell.
        if (shortcut.BrowserAppWindow && browser is "Chrome" or "Edge" or "Brave")
            start.ArgumentList.Add("--app=" + uri.AbsoluteUri);
        else
            start.ArgumentList.Add(uri.AbsoluteUri);
        return start;
    }
}

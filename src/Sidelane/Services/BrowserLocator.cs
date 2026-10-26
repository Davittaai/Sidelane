using System.IO;
using Microsoft.Win32;

namespace Sidelane.Services;

/// <summary>Find installed browsers from Windows App Paths first, then conventional per-user paths.</summary>
public static class BrowserLocator
{
    private static string? FindRegistered(string exeName)
    {
        foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            using var key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exeName);
            var path = key?.GetValue(null) as string;
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) return path;
        }
        return null;
    }

    public static string? Locate(string browser)
    {
        var exe = browser switch
        {
            "Chrome" => "chrome.exe",
            "Edge" => "msedge.exe",
            "Firefox" => "firefox.exe",
            "Brave" => "brave.exe",
            _ => null
        };
        if (exe is null) return null;
        var registered = FindRegistered(exe);
        if (registered is not null) return registered;
        var roots = new[] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) };
        var relative = browser switch
        {
            "Chrome" => @"Google\Chrome\Application\chrome.exe",
            "Edge" => @"Microsoft\Edge\Application\msedge.exe",
            "Firefox" => @"Mozilla Firefox\firefox.exe",
            "Brave" => @"BraveSoftware\Brave-Browser\Application\brave.exe",
            _ => ""
        };
        foreach (var root in roots)
        {
            var candidate = Path.Combine(root, relative);
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }
}

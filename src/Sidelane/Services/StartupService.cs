using Microsoft.Win32;

namespace Sidelane.Services;

/// <summary>
/// Optional, reversible per-user startup; no scheduled tasks, services or admin access.
/// </summary>
public static class StartupService
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string EntryName = "Sidelane";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: false);
        return key?.GetValue(EntryName) is string value && value.Length > 0;
    }

    public static void SetEnabled(bool enable)
    {
        if (!enable)
        {
            using var existing = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true);
            existing?.DeleteValue(EntryName, throwOnMissingValue: false);
            return;
        }
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath, writable: true);
        if (key is null) throw new InvalidOperationException("Windows startup settings are inaccessible.");
        var exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe) || !exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Start the built Sidelane.exe before enabling login startup.");
        key.SetValue(EntryName, $"\"{exe}\"", RegistryValueKind.String);
    }
}

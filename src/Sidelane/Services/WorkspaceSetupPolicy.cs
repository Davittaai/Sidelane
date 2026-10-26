using System.IO;
using Sidelane.Models;

namespace Sidelane.Services;

/// <summary>Side-effect-free first-run planning. Users choose whether any detected app can launch.</summary>
public static class WorkspaceSetupPolicy
{
    public static AppSettings Configure(AppSettings profile,
        IEnumerable<string> selectedNames,
        string? selectedBrowser,
        bool preferInstalled,
        IReadOnlyDictionary<string, string> installed,
        string accent,
        string background,
        bool showOnTaskbar)
    {
        var selected = selectedNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var browser = LaunchPolicy.IsValidBrowser(selectedBrowser) ? selectedBrowser! : "Default";
        var shortcuts = new List<ShortcutSettings>();
        foreach (var item in ShortcutSettings.Defaults().Where(x => selected.Contains(x.Name)))
        {
            item.Browser = browser;
            if (preferInstalled && installed.TryGetValue(item.Name, out var path) &&
                Path.IsPathFullyQualified(path) && File.Exists(path) &&
                path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                item.AppPath = path;
                item.LaunchMode = "App";
            }
            else if (LinkPolicy.TryGetWebTarget(item.Url, out _))
                item.LaunchMode = "Browser";
            else
                continue; // A desktop-only tool without a real executable would be a dead shortcut.
            shortcuts.Add(item);
        }
        profile.Shortcuts = shortcuts;
        profile.Accent = SettingsService.ValidColor(accent, "#B9D8BF");
        profile.Background = ThemeContrastPolicy.SupportsDarkUi(background) ? background : "#191C20";
        profile.ShowTaskbarButton = showOnTaskbar;
        return profile;
    }
}

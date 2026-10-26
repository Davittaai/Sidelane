using System.IO;

namespace Sidelane.Services;

/// <summary>Conservative, local-only discovery of familiar Start Menu shortcuts.</summary>
public static class LocalAppDiscovery
{
    private static readonly Dictionary<string, string[]> Hints = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ChatGPT"] = ["ChatGPT"],
        ["Claude"] = ["Claude"],
        ["Grok"] = ["Grok"],
        ["VS Code"] = ["Visual Studio Code", "VS Code", "Code"]
    };

    public static string? Find(string application, IEnumerable<string> roots)
    {
        if (!Hints.TryGetValue(application, out var hints)) return null;
        var matches = new List<string>();
        foreach (var root in roots)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) continue;
            try
            {
                // Never inspect target contents or execute a shortcut during discovery.
                var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
                foreach (var entry in Directory.EnumerateFiles(root, "*.lnk", options).Take(1200))
                {
                    var title = Path.GetFileNameWithoutExtension(entry);
                    if (hints.Any(h => title.Equals(h, StringComparison.OrdinalIgnoreCase)))
                        matches.Add(entry);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        return matches.OrderBy(p => p.Length).FirstOrDefault();
    }

    public static IReadOnlyDictionary<string, string> FindKnownApps()
    {
        var folders = new[] {
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms)
        };
        return Hints.Keys.Select(name => (Name: name, Path: Find(name, folders)))
            .Where(x => x.Path is not null)
            .ToDictionary(x => x.Name, x => x.Path!, StringComparer.OrdinalIgnoreCase);
    }
}

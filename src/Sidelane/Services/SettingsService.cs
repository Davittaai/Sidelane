using System.IO;
using System.Globalization;
using System.Text.Json;
using Sidelane.Models;

namespace Sidelane.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public string DirectoryPath { get; }
    private string FilePath => Path.Combine(DirectoryPath, "settings.json");
    public AppSettings Current { get; private set; } = new();

    public SettingsService(string? directoryPath = null)
    {
        DirectoryPath = directoryPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sidelane");
        Load();
    }

    public void Load()
    {
        try
        {
            Current = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings()
                : new AppSettings();
        }
        catch (JsonException) { Current = new AppSettings(); }
        catch (IOException) { Current = new AppSettings(); }
        catch (UnauthorizedAccessException) { Current = new AppSettings(); }
        Normalize(Current);
    }

    private const long MaxImportBytes = 256 * 1024;

    /// <summary>Export only saved preferences; scratchpad tasks and activity notes are never included.</summary>
    public void Export(string destination)
    {
        var temp = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(Current, JsonOptions));
            File.Move(temp, destination, true);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }

    /// <summary>Import a Sidelane preferences file after validating it; live settings stay unchanged on failure.</summary>
    public void Import(string path)
    {
        var file = new FileInfo(path);
        if (!file.Exists || file.Length is < 2 or > MaxImportBytes)
            throw new InvalidDataException("The settings file is missing or exceeds 256 KB.");
        var content = File.ReadAllText(path);
        using var document = JsonDocument.Parse(content);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            !document.RootElement.TryGetProperty("Shortcuts", out var shortcuts) ||
            shortcuts.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("This is not a Sidelane settings export.");
        var replacement = JsonSerializer.Deserialize<AppSettings>(content)
            ?? throw new InvalidDataException("Could not read the settings.");
        Replace(replacement);
    }

    public AppSettings EditableCopy() =>
        JsonSerializer.Deserialize<AppSettings>(JsonSerializer.Serialize(Current)) ?? new AppSettings();

    // The UI does not receive the new settings until their atomic write succeeds.
    public void Replace(AppSettings replacement)
    {
        Normalize(replacement);
        Write(replacement);
        Current = replacement;
    }

    public void Save()
    {
        Normalize(Current);
        Write(Current);
    }

    private void Write(AppSettings candidate)
    {
        Directory.CreateDirectory(DirectoryPath);
        var temp = Path.Combine(DirectoryPath, $"settings-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(candidate, JsonOptions));
            File.Move(temp, FilePath, true);
        }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); }
            catch (IOException) { /* The temporary file can safely be removed later. */ }
        }
    }

    private static void Normalize(AppSettings settings)
    {
        settings.Shortcuts ??= ShortcutSettings.Defaults();
        settings.Edge = settings.Edge is "Left" or "Right" or "Free" ? settings.Edge : "Left";
        if (settings.Edge == "Right") settings.RailOnRight = true;
        else if (settings.Edge == "Left") settings.RailOnRight = false;
        settings.Opacity = double.IsFinite(settings.Opacity) ? Math.Clamp(settings.Opacity, .65, 1) : 1.0;
        settings.RailWidth = ScreenPlacement.NormalizeRailWidth(settings.RailWidth);
        settings.PanelWidth = ScreenPlacement.NormalizePanelWidth(settings.PanelWidth, settings.RailWidth);
        settings.PanelHeight = ScreenPlacement.NormalizePanelHeight(settings.PanelHeight);
        settings.Left = double.IsFinite(settings.Left) ? settings.Left : 28;
        settings.Top = double.IsFinite(settings.Top) ? settings.Top : 100;
        settings.Accent = ValidColor(settings.Accent, "#B9D8BF");
        settings.Background = ValidColor(settings.Background, "#191C20");
        if (!ThemeContrastPolicy.SupportsDarkUi(settings.Background)) settings.Background = "#191C20";
        settings.Font = settings.Font is "Segoe UI" or "Consolas" or "Arial" or "Georgia" ? settings.Font : "Segoe UI";
        settings.BackgroundImage ??= "";
        if (settings.BackgroundImage.Length > 1024) settings.BackgroundImage = "";
        // The JSON file is user-editable; never trust its string lengths or protocols.
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        settings.Shortcuts = settings.Shortcuts
            .Where(s => s is not null && !string.IsNullOrWhiteSpace(s.Name))
            .Select(s => new ShortcutSettings
            {
                Name = s.Name.Trim()[..Math.Min(s.Name.Trim().Length, 28)],
                Symbol = string.IsNullOrWhiteSpace(s.Symbol) ? "◈" : TruncateSymbols(s.Symbol),
                Match = (s.Match ?? "")[..Math.Min((s.Match ?? "").Length, 100)],
                Url = LinkPolicy.TryGetWebTarget(s.Url, out var uri) ? uri!.ToString() : "",
                StatusProvider = s.StatusProvider is "Auto" or "None" or "ChatGPT" or "Claude" or "Grok" ? s.StatusProvider : "Auto",
                LaunchMode = LaunchPolicy.IsValidMode(s.LaunchMode) ? s.LaunchMode : "Auto",
                Browser = LaunchPolicy.IsValidBrowser(s.Browser) ? s.Browser : "Default",
                BrowserAppWindow = s.BrowserAppWindow,
                AppPath = (s.AppPath ?? "").Length <= 1024 && System.IO.Path.IsPathFullyQualified(s.AppPath ?? "") &&
                    ((s.AppPath ?? "").EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                     (s.AppPath ?? "").EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)) ? (s.AppPath ?? "") : ""
            })
            .Where(s => names.Add(s.Name))
            .Take(12).ToList();
    }

    // Prevent splitting UTF-16 surrogate pairs in emoji-based shortcut icons.
    private static string TruncateSymbols(string value)
    {
        var text = new StringInfo(value);
        return text.SubstringByTextElements(0, Math.Min(text.LengthInTextElements, 2));
    }

    public static string ValidColor(string? candidate, string fallback)
    {
        if (candidate?.Length != 7 || candidate[0] != '#') return fallback;
        return candidate.Skip(1).All(Uri.IsHexDigit) ? candidate.ToUpperInvariant() : fallback;
    }
}

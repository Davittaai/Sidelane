using System.Text.Json;
using Sidelane.Models;
using Sidelane.Services;

var cases = new List<(string Name, Func<bool> Run)>
{
    ("browser is explicitly selectable", () => LaunchPolicy.IsValidBrowser("Firefox") && !LaunchPolicy.IsValidBrowser("curl")),
    ("launch mode is validated", () => LaunchPolicy.IsValidMode("App") && !LaunchPolicy.IsValidMode("Powershell")),
    ("default web opens with operating system handler", () =>
    {
        var plan = LaunchPolicy.Create(new ShortcutSettings { Name = "AI", LaunchMode = "Browser",
            Url = "https://chatgpt.com/", Browser = "Default" }, _ => null);
        return plan is { UseShellExecute: true } && plan.FileName == "https://chatgpt.com/";
    }),
    ("explicit browser cannot silently fall back", () =>
        LaunchPolicy.Create(new ShortcutSettings { Name = "AI", LaunchMode = "Browser",
            Browser = "Firefox", Url = "https://claude.ai" }, _ => null) is null),
    ("shell command URL blocked", () =>
        LaunchPolicy.Create(new ShortcutSettings { Url = "javascript:alert(1)" }, _ => null) is null),
    ("app mode does not open website without executable", () =>
        LaunchPolicy.Create(new ShortcutSettings { LaunchMode = "App", Url = "https://chatgpt.com" }, _ => null) is null),
    ("observed open-tab signal validated", () => AiSignalPolicy.Parse("{\"provider\":\"Claude\",\"state\":\"open\"}")?.State == "open"),
    ("extra transcript fields rejected even when short", () => AiSignalPolicy.Parse("{\"provider\":\"Grok\",\"state\":\"open\",\"prompt\":\"hi\"}") is null),
    ("new compact defaults", () => new AppSettings().RailWidth == 46 && new AppSettings().PanelWidth == 320 &&
        new AppSettings().PanelHeight == 560 && new AppSettings().ShowTaskbarButton),
    ("resizing clamps hostile preferences", () => InSandbox(root =>
    {
        File.WriteAllText(Path.Combine(root, "settings.json"), "{\"RailWidth\":900,\"PanelWidth\":40,\"PanelHeight\":5000}");
        var p = new SettingsService(root).Current;
        return p.RailWidth == 76 && p.PanelWidth == 300 && p.PanelHeight == 900;
    })),
    ("custom rail placement respects configured width", () =>
        ScreenPlacement.EdgeRailLeft("Right", 0, 0, 1920, railWidth: 72) == 1836),
    ("custom expanded size stays anchored", () =>
        ScreenPlacement.WindowLeft(1836, true, true, 0, 1920, railWidth: 72, expandedWidth: 480) == 1428),
    ("AI signal explicitly marks stopped, not completed", () =>
        AiSignalPolicy.Parse("{\"provider\":\"Claude\",\"state\":\"stopped\"}") is { State: "stopped" }),
    ("AI signal denies transcript-bearing extras by length", () =>
        AiSignalPolicy.Parse("{\"provider\":\"Grok\",\"state\":\"generating\",\"prompt\":\"" +
            new string('p', 300) + "\"}") is null),
    ("AI signal accepts only known providers and states", () =>
        AiSignalPolicy.Parse("{\"provider\":\"ChatGPT\",\"state\":\"generating\"}") is { Provider: "ChatGPT" }),
    ("AI signal refuses invalid browser payload", () =>
        AiSignalPolicy.Parse("{\"provider\":\"Other\",\"state\":\"generating\"}") is null),
    ("AI signal rejects prompt and reply payloads", () =>
        AiSignalPolicy.Parse(new string('x', 300)) is null),
    ("official update feed rejects spoofed release URLs", () =>
        UpdateService.Parse("{\"tag_name\":\"v1.9.0\",\"html_url\":\"https://evil.example/release\"}", new Version(1, 2)) is null),
    ("official update feed accepts newer stable release", () =>
        UpdateService.Parse("{\"tag_name\":\"v1.9.0\",\"html_url\":\"https://github.com/Davittaai/Sidelane/releases/tag/v1.9.0\"}", new Version(1, 2)) is not null),
    ("right edge anchors narrow rail", () => Math.Abs(ScreenPlacement.EdgeRailLeft("Right", 80, 0, 1920) - 1862) < .001),
    ("right rail expands to left", () => ScreenPlacement.WindowLeft(1854, true, true, 0, 1920) == 1580),
    ("collapsed right rail maintains anchor", () => ScreenPlacement.WindowLeft(1854, false, true, 0, 1920) == 1854),
    ("multi-monitor negative coordinates supported", () => ScreenPlacement.WindowLeft(-1270, true, false, -1280, 0) == -1270),
    ("free placement opens inward near right edge", () => ScreenPlacement.RailOnRightForFree(1850, 0, 1920)),
    ("free placement opens inward near left edge", () => !ScreenPlacement.RailOnRightForFree(12, 0, 1920)),
    ("free placement supports negative coordinates", () => ScreenPlacement.RailOnRightForFree(-90, -1280, 0)),
    ("offscreen positions clamped", () => ScreenPlacement.WindowTop(9000, 500, -200, 700) == 196),
    ("valid accent normalized", () => SettingsService.ValidColor("#ffab12", "wrong") == "#FFAB12"),
    ("invalid short hex rejected", () => SettingsService.ValidColor("#123", "fallback") == "fallback"),
    ("invalid hex characters rejected", () => SettingsService.ValidColor("#G23456", "fallback") == "fallback"),
    ("default dark surface meets secondary text contrast", () => ThemeContrastPolicy.SupportsDarkUi("#191C20")),
    ("dark color customization remains available", () => ThemeContrastPolicy.SupportsDarkUi("#202D37")),
    ("white surface rejected for a dark-only UI", () => !ThemeContrastPolicy.SupportsDarkUi("#FFFFFF")),
    ("broken hex cannot bypass contrast check", () => !ThemeContrastPolicy.SupportsDarkUi("#KKKKKK")),
    ("too-light JSON surface is normalized", () => InSandbox(root =>
    {
        File.WriteAllText(Path.Combine(root, "settings.json"), "{\"Background\":\"#FFFFFF\"}");
        return new SettingsService(root).Current.Background == "#191C20";
    })),
    ("https destination accepted", () => LinkPolicy.TryGetWebTarget("https://chatgpt.com", out _)),
    ("local http destination accepted", () => LinkPolicy.TryGetWebTarget("http://localhost:1234", out _)),
    ("javascript destination rejected", () => !LinkPolicy.TryGetWebTarget("javascript:alert(1)", out _)),
    ("file destination rejected", () => !LinkPolicy.TryGetWebTarget("file:///C:/Windows", out _)),
    ("embedded credentials rejected", () => !LinkPolicy.TryGetWebTarget("https://user:pass@example.com", out _)),
    ("overlong image path rejected", () => !ImageFilePolicy.IsSupported(new string('x', 1200) + ".png")),
    ("missing image rejected", () => !ImageFilePolicy.IsSupported(Path.Combine(Path.GetTempPath(), "nonexistent-sidelane-picture.png"))),
    ("unsupported image type rejected", () => InSandbox(root =>
    {
        var path = Path.Combine(root, "background.webp"); File.WriteAllText(path, "invalid");
        return !ImageFilePolicy.IsSupported(path);
    })),
    ("oversized images rejected without decoding", () => InSandbox(root =>
    {
        var path = Path.Combine(root, "huge.png");
        using (var output = File.Create(path)) output.SetLength(ImageFilePolicy.MaxImageBytes + 1);
        return !ImageFilePolicy.IsSupported(path);
    })),
    ("empty destination rejected", () => !LinkPolicy.TryGetWebTarget("", out _)),
    ("new settings defaults are non-invasive", () => new AppSettings().RespectFullscreen &&
        !new AppSettings().StartupAtLogin),
    ("default sidebar is opaque for readability", () => new AppSettings().Opacity == 1.0),
    ("custom translucency remains available", () => InSandbox(root =>
    {
        var prefs = new AppSettings { Opacity = .75 };
        File.WriteAllText(Path.Combine(root, "settings.json"), JsonSerializer.Serialize(prefs));
        return Math.Abs(new SettingsService(root).Current.Opacity - .75) < .001;
    })),
    ("default app slots present", () => ShortcutSettings.Defaults().Count == 5),
    ("temporary tasks absent from settings model", () => typeof(AppSettings).GetProperties()
        .All(p => p.Name != "Tasks" && p.Name != "ManualStatuses")),
    ("settings roundtrip and no transient state persisted", () => InSandbox(root =>
    {
        var service = new SettingsService(root);
        service.Current.Accent = "#11aa44";
        service.Current.Edge = "Right";
        service.Save();
        var loaded = new SettingsService(root);
        var json = File.ReadAllText(Path.Combine(root, "settings.json"));
        return loaded.Current.Accent == "#11AA44" && loaded.Current.Edge == "Right" &&
            !json.Contains("Tasks", StringComparison.OrdinalIgnoreCase) &&
            !File.Exists(Path.Combine(root, "settings.json.tmp"));
    })),
    ("settings copy keeps editor changes isolated", () => InSandbox(root =>
    {
        var service = new SettingsService(root);
        var draft = service.EditableCopy();
        draft.Accent = "#F0F0F0";
        draft.Shortcuts.Clear();
        return service.Current.Accent == "#B9D8BF" && service.Current.Shortcuts.Count == 5;
    })),
    ("replacement is committed on disk and in memory", () => InSandbox(root =>
    {
        var service = new SettingsService(root);
        var draft = service.EditableCopy(); draft.Accent = "#20ff88";
        service.Replace(draft);
        return service.Current.Accent == "#20FF88" && new SettingsService(root).Current.Accent == "#20FF88";
    })),
    ("replacement failure leaves live settings unchanged", () => InSandbox(root =>
    {
        var service = new SettingsService(root);
        Directory.CreateDirectory(Path.Combine(root, "settings.json"));
        var draft = service.EditableCopy(); draft.Accent = "#F0F0F0";
        try { service.Replace(draft); return false; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return service.Current.Accent == "#B9D8BF" &&
            !Directory.EnumerateFiles(root, "settings-*.tmp").Any(); }
    })),
    ("damaged preferences recover without crashing", () => InSandbox(root =>
    {
        File.WriteAllText(Path.Combine(root, "settings.json"), "{not json");
        return new SettingsService(root).Current.Shortcuts.Count == 5;
    })),
    ("nullable hostile settings normalized", () => InSandbox(root =>
    {
        File.WriteAllText(Path.Combine(root, "settings.json"), """
            {"Accent":"#BADHEX","Background":"not-a-color","Edge":"Unsafe","Opacity":-12,
             "Shortcuts":[null,{"Name":"  Test  ","Symbol":"😀😀😀","Url":"javascript:alert(1)","Match":"calc"}]}
            """);
        var s = new SettingsService(root).Current;
        return s.Edge == "Left" && s.Accent == "#B9D8BF" && s.Background == "#191C20" &&
               s.Opacity >= .65 && s.Shortcuts.Count == 1 && s.Shortcuts[0].Name == "Test" &&
               s.Shortcuts[0].Url.Length == 0;
    })),
    ("Unicode app icons remain valid when truncated", () => InSandbox(root =>
    {
        var input = new AppSettings { Shortcuts = new List<ShortcutSettings>
            { new() { Name = "Emoji", Symbol = "😀😀😀" } } };
        File.WriteAllText(Path.Combine(root,"settings.json"), JsonSerializer.Serialize(input));
        return new SettingsService(root).Current.Shortcuts[0].Symbol == "😀😀";
    })),
    ("duplicate app names normalized", () => InSandbox(root =>
    {
        File.WriteAllText(Path.Combine(root,"settings.json"),
            "{\"Shortcuts\":[{\"Name\":\" Claude \"},{\"Name\":\"claude\"},{\"Name\":\"ChatGPT\"}]}");
        var apps = new SettingsService(root).Current.Shortcuts;
        return apps.Count == 2 && apps[0].Name == "Claude";
    })),
    ("session activity notes are trimmed", () =>
    {
        var store = new SessionActivityStore();
        store.SetNote("ChatGPT", "  Shipping Sidelane v1.1  ");
        return store.Get("ChatGPT").Note == "Shipping Sidelane v1.1";
    }),
    ("session activity notes are limited to 96 characters", () =>
    {
        var store = new SessionActivityStore();
        store.SetNote("Claude", new string('x', 400));
        return store.Get("Claude").Note.Length == 96;
    }),
    ("session status only accepts known states", () =>
    {
        var store = new SessionActivityStore();
        store.SetStatus("Grok", "working");
        var accepted = store.Get("Grok").Status == "working";
        store.SetStatus("Grok", "unverified");
        return accepted && store.Get("Grok").Status.Length == 0;
    }),
    ("session activity names are case-insensitive", () =>
    {
        var store = new SessionActivityStore();
        store.SetNote("ChatGPT", "Running QA");
        return store.Get("chatgpt").Note == "Running QA";
    }),
    ("clearing a session note preserves status", () =>
    {
        var store = new SessionActivityStore();
        store.SetStatus("ChatGPT", "waiting");
        store.SetNote("ChatGPT", "Waiting for tests");
        store.SetNote("ChatGPT", "");
        return store.Get("ChatGPT").Status == "waiting" && store.Get("ChatGPT").Note.Length == 0;
    }),
    ("session activity is not shared between instances or settings", () =>
    {
        var first = new SessionActivityStore();
        first.SetStatus("ChatGPT", "working");
        first.SetNote("ChatGPT", "Secret temporary task");
        var clean = new SessionActivityStore();
        return clean.Get("ChatGPT").Note == "" && clean.Get("ChatGPT").Status == "" &&
            !System.Text.Json.JsonSerializer.Serialize(new AppSettings()).Contains("Secret temporary task");
    }),
    ("malformed preference list capped at twelve", () => InSandbox(root =>
    {
        var input = new AppSettings { Shortcuts = Enumerable.Range(1, 20)
            .Select(i => new ShortcutSettings { Name = $"Item {i}" }).ToList() };
        File.WriteAllText(Path.Combine(root,"settings.json"),JsonSerializer.Serialize(input));
        return new SettingsService(root).Current.Shortcuts.Count == 12;
    })),
};

var failed = 0;
foreach (var (name, run) in cases)
{
    bool ok;
    try { ok = run(); }
    catch (Exception ex) { Console.WriteLine($"ERROR: {name} — {ex.Message}"); ok = false; }
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}: {name}");
    if (!ok) failed++;
}
Console.WriteLine($"{cases.Count - failed}/{cases.Count} passed");
if (failed > 0) Environment.Exit(1);

static bool InSandbox(Func<string, bool> test)
{
    var path = Path.Combine(Path.GetTempPath(), "SidelaneChecks", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    try { return test(path); }
    finally { Directory.Delete(path, recursive: true); }
}

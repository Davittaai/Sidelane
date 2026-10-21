using System.Text.Json;
using Sidelane.Models;
using Sidelane.Services;

var cases = new List<(string Name, Func<bool> Run)>
{
    ("right edge anchors narrow rail", () => Math.Abs(ScreenPlacement.EdgeRailLeft("Right", 80, 0, 1920) - 1854) < .001),
    ("right rail expands to left", () => ScreenPlacement.WindowLeft(1854, true, true, 0, 1920) == 1548),
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

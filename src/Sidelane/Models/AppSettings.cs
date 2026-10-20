namespace Sidelane.Models;

public sealed class AppSettings
{
    public double Left { get; set; } = 28;
    public double Top { get; set; } = 100;
    public bool Locked { get; set; }
    public bool AutoCollapse { get; set; } = true;
    public bool RespectFullscreen { get; set; } = true;
    public bool StartupAtLogin { get; set; } = false;
    public bool StartCollapsed { get; set; } = true;
    public string Edge { get; set; } = "Left";
    // Remembers which side of the panel holds the rail when dragged to a free position.
    public bool RailOnRight { get; set; }
    public double Opacity { get; set; } = 0.96;
    public string Accent { get; set; } = "#B9D8BF";
    public string Background { get; set; } = "#191C20";
    public string Font { get; set; } = "Segoe UI";
    public string BackgroundImage { get; set; } = "";
    public List<ShortcutSettings> Shortcuts { get; set; } = ShortcutSettings.Defaults();
}

public sealed class ShortcutSettings
{
    public string Name { get; set; } = "";
    public string Symbol { get; set; } = "◈";
    public string Url { get; set; } = "";
    public string Match { get; set; } = "";

    public static List<ShortcutSettings> Defaults() => new()
    {
        new() { Name="ChatGPT", Symbol="✳", Url="https://chatgpt.com", Match="chatgpt" },
        new() { Name="Claude", Symbol="✺", Url="https://claude.ai", Match="claude" },
        new() { Name="Grok", Symbol="✧", Url="https://grok.com", Match="grok" },
        new() { Name="Codex", Symbol="⌘", Url="https://chatgpt.com/codex", Match="codex" },
        new() { Name="VS Code", Symbol="⌁", Match="code.exe|visual studio code" },
    };
}

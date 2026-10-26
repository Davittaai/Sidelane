namespace Sidelane.Services;

/// <summary>Small vetted global shortcut set; no keyboard hooks or key recording.</summary>
public static class HotkeyPolicy
{
    public const string Default = "Ctrl+Alt+Space";
    private const uint Alt = 1, Control = 2, Shift = 4, NoRepeat = 0x4000, Space = 0x20;
    public static readonly string[] Choices = [Default, "Ctrl+Shift+Space", "Disabled"];
    public static string Normalize(string? value) => Choices.Contains(value) ? value! : Default;
    public static bool TryGet(string? value, out uint modifiers, out uint key)
    {
        modifiers = 0;
        key = 0;
        switch (Normalize(value))
        {
            case Default:
                modifiers = Control | Alt | NoRepeat;
                key = Space;
                return true;
            case "Ctrl+Shift+Space":
                modifiers = Control | Shift | NoRepeat;
                key = Space;
                return true;
            default:
                return false;
        }
    }
}

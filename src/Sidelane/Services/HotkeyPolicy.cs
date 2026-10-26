namespace Sidelane.Services;

/// <summary>Small vetted global shortcut set; no keyboard hooks or key recording.</summary>
public static class HotkeyPolicy
{
    public const string Default = "Ctrl+Alt+Space";
    public static readonly string[] Choices = [Default, "Ctrl+Shift+Space", "Disabled"];
    public static string Normalize(string? value) => Choices.Contains(value) ? value! : Default;
    public static bool TryGet(string? value, out uint modifiers, out uint key)
    {
        modifiers = 0;
        key = 0;
        switch (Normalize(value))
        {
            case Default:
                modifiers = NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT;
                key = NativeMethods.VK_SPACE;
                return true;
            case "Ctrl+Shift+Space":
                modifiers = NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT | NativeMethods.MOD_NOREPEAT;
                key = NativeMethods.VK_SPACE;
                return true;
            default:
                return false;
        }
    }
}

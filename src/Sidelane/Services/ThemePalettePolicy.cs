namespace Sidelane.Services;

/// <summary>Derives rail and divider tones from a user-selected dark surface.</summary>
public static class ThemePalettePolicy
{
    public static string Rail(string surface)
    {
        var source = SettingsService.ValidColor(surface, "#191C20");
        var lighter = Blend(source, "#FFFFFF", .055);
        // Keep the small rail's fixed muted text readable at the edge of the allowed palette.
        return ThemeContrastPolicy.SupportsDarkUi(lighter) ? lighter : source;
    }

    public static string Divider(string surface) =>
        Blend(SettingsService.ValidColor(surface, "#191C20"), "#FFFFFF", .17);

    public static string Blend(string foreground, string background, double amount)
    {
        var a = SettingsService.ValidColor(foreground, "#191C20");
        var b = SettingsService.ValidColor(background, "#FFFFFF");
        var t = double.IsFinite(amount) ? Math.Clamp(amount, 0, 1) : 0;
        static int Channel(string hex, int index) =>
            Convert.ToInt32(hex.Substring(index, 2), 16);
        static int Mix(int a, int b, double t) =>
            (int)Math.Clamp(Math.Round(a + (b - a) * t), 0, 255);
        return $"#{Mix(Channel(a, 1), Channel(b, 1), t):X2}" +
               $"{Mix(Channel(a, 3), Channel(b, 3), t):X2}" +
               $"{Mix(Channel(a, 5), Channel(b, 5), t):X2}";
    }
}

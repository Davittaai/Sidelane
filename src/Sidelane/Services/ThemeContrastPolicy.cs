namespace Sidelane.Services;

/// <summary>
/// Sidelane currently has a dark-only foreground palette. Reject surface colors
/// that would make its secondary text unreadable rather than pretending that
/// arbitrary light backgrounds are supported.
/// </summary>
public static class ThemeContrastPolicy
{
    private const string SecondaryText = "#A0A3AA";

    public static bool SupportsDarkUi(string? surface)
    {
        if (surface is null || surface.Length != 7 || surface[0] != '#' ||
            !surface.Skip(1).All(Uri.IsHexDigit)) return false;
        var text = RelativeLuminance(SecondaryText);
        var background = RelativeLuminance(surface);
        return text > background && (text + .05) / (background + .05) >= 4.5;
    }

    private static double RelativeLuminance(string hex)
    {
        static double Linear(int channel)
        {
            var value = channel / 255.0;
            return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
        }
        var r = Linear(Convert.ToInt32(hex.Substring(1, 2), 16));
        var g = Linear(Convert.ToInt32(hex.Substring(3, 2), 16));
        var b = Linear(Convert.ToInt32(hex.Substring(5, 2), 16));
        return .2126 * r + .7152 * g + .0722 * b;
    }
}

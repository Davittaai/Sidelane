namespace Sidelane.Services;

// Values are WPF device-independent pixels. "RailLeft" always identifies the
// left edge of the narrow strip, regardless of which side expands.
public static class ScreenPlacement
{
    public const double RailWidth = 46;
    public const double ExpandedWidth = 320;
    public static double NormalizeRailWidth(double value) => double.IsFinite(value) ? Math.Clamp(value, 42, 76) : RailWidth;
    public static double NormalizePanelWidth(double value, double railWidth = RailWidth) => double.IsFinite(value) ? Math.Clamp(value, Math.Max(300, railWidth + 220), 520) : ExpandedWidth;
    public static double NormalizePanelHeight(double value) => double.IsFinite(value) ? Math.Clamp(value, 360, 900) : 560;

    public static double EdgeRailLeft(string edge, double savedRailLeft,
        double workLeft, double workRight, double margin = 12, double railWidth = RailWidth) => edge switch
    {
        "Left" => workLeft + margin,
        "Right" => workRight - railWidth - margin,
        _ => savedRailLeft
    };

    // In free mode expand away from the nearest edge when possible. Users
    // dragging the rail across the monitor should never lose the icon strip.
    public static bool RailOnRightForFree(double railLeft, double workLeft, double workRight, double railWidth = RailWidth, double expandedWidth = ExpandedWidth)
    {
        var roomLeft = railLeft - workLeft;
        var roomRight = workRight - (railLeft + railWidth);
        return roomRight < expandedWidth - railWidth && roomLeft >= roomRight;
    }

    public static double WindowLeft(double railLeft, bool expand, bool railOnRight,
        double workLeft, double workRight, double margin = 4, double railWidth = RailWidth, double expandedWidth = ExpandedWidth)
    {
        var width = expand ? expandedWidth : railWidth;
        var desired = railLeft - (expand && railOnRight ? expandedWidth - railWidth : 0);
        var max = Math.Max(workLeft + margin, workRight - width - margin);
        return Math.Clamp(desired, workLeft + margin, max);
    }

    public static double WindowTop(double savedTop, double height,
        double workTop, double workBottom, double margin = 4)
        => Math.Clamp(savedTop, workTop + margin,
            Math.Max(workTop + margin, workBottom - height - margin));
}

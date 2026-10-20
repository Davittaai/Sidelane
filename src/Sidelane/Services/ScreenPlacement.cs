namespace Sidelane.Services;

// Values are WPF device-independent pixels. "RailLeft" always identifies the
// left edge of the narrow strip, regardless of which side expands.
public static class ScreenPlacement
{
    public const double RailWidth = 54;
    public const double ExpandedWidth = 360;

    public static double EdgeRailLeft(string edge, double savedRailLeft,
        double workLeft, double workRight, double margin = 12) => edge switch
    {
        "Left" => workLeft + margin,
        "Right" => workRight - RailWidth - margin,
        _ => savedRailLeft
    };

    // In free mode expand away from the nearest edge when possible. Users
    // dragging the rail across the monitor should never lose the icon strip.
    public static bool RailOnRightForFree(double railLeft, double workLeft, double workRight)
    {
        var roomLeft = railLeft - workLeft;
        var roomRight = workRight - (railLeft + RailWidth);
        return roomRight < ExpandedWidth - RailWidth && roomLeft >= roomRight;
    }

    public static double WindowLeft(double railLeft, bool expand, bool railOnRight,
        double workLeft, double workRight, double margin = 4)
    {
        var width = expand ? ExpandedWidth : RailWidth;
        var desired = railLeft - (expand && railOnRight ? ExpandedWidth - RailWidth : 0);
        var max = Math.Max(workLeft + margin, workRight - width - margin);
        return Math.Clamp(desired, workLeft + margin, max);
    }

    public static double WindowTop(double savedTop, double height,
        double workTop, double workBottom, double margin = 4)
        => Math.Clamp(savedTop, workTop + margin,
            Math.Max(workTop + margin, workBottom - height - margin));
}

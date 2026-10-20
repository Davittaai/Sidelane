using System.IO;

namespace Sidelane.Services;

public static class ImageFilePolicy
{
    public const long MaxImageBytes = 12 * 1024 * 1024;
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp" };

    public static bool IsSupported(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 1024) return false;
        try
        {
            var file = new FileInfo(path);
            return SupportedExtensions.Contains(file.Extension) &&
                   file.Exists && file.Length is > 0 and <= MaxImageBytes;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        { return false; }
    }
}

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sidelane.Services;

public static class BackgroundImagePolicy
{
    private static readonly object Gate = new();
    private static string? _path;
    private static DateTime _stamp;
    private static BitmapImage? _bitmap;

    public static ImageBrush? TryCreateBrush(string? path)
    {
        if (!ImageFilePolicy.IsSupported(path)) return null;
        try
        {
            lock (Gate)
            {
                var stamp = File.GetLastWriteTimeUtc(path!);
                if (_bitmap is null || _path != path || _stamp != stamp)
                {
                    using var stream = File.OpenRead(path!);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 800; // sidebar renders at 520 DIP or less
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    _bitmap = image;
                    _path = path;
                    _stamp = stamp;
                }
                return new ImageBrush(_bitmap) { Stretch = Stretch.UniformToFill, Opacity = .13 };
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or InvalidOperationException or FormatException or System.Runtime.InteropServices.COMException)
        { return null; }
    }
}

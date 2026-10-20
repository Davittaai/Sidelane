using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sidelane.Services;

public static class BackgroundImagePolicy
{
    public static ImageBrush? TryCreateBrush(string? path)
    {
        if (!ImageFilePolicy.IsSupported(path)) return null;
        try
        {
            using var stream = File.OpenRead(path!);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 1400;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill, Opacity = .13 };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or InvalidOperationException or FormatException or System.Runtime.InteropServices.COMException)
        { return null; }
    }
}

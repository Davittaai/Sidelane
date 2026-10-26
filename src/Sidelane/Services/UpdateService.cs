using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Sidelane.Services;

public sealed record SidelaneRelease(Version Version, Uri Page, Uri? Installer = null, Uri? Checksum = null);

/// <summary>Checks the official GitHub stable release feed. It never executes downloaded code.</summary>
public static class UpdateService
{
    private const string Endpoint = "https://api.github.com/repos/Davittaai/Sidelane/releases/latest";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static async Task<SidelaneRelease?> GetNewReleaseAsync(Version current, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        req.Headers.UserAgent.Add(new ProductInfoHeaderValue("Sidelane", "1.2"));
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        using var result = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        result.EnsureSuccessStatusCode();
        return Parse(await result.Content.ReadAsStringAsync(ct), current);
    }

    public static SidelaneRelease? Parse(string json, Version current)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("draft", out var draft) && draft.GetBoolean()) return null;
        if (root.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) return null;
        if (!root.TryGetProperty("tag_name", out var tag)) return null;
        var value = tag.GetString()?.TrimStart('v', 'V');
        if (!Version.TryParse(value, out var latest) || latest <= current) return null;
        if (!root.TryGetProperty("html_url", out var page) ||
            !Uri.TryCreate(page.GetString(), UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || uri.Host != "github.com" ||
            uri.AbsolutePath != $"/Davittaai/Sidelane/releases/tag/{tag.GetString()}")
            return null;

        Uri? installer = null, checksum = null;
        if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
            foreach (var asset in assets.EnumerateArray())
            {
                if (!asset.TryGetProperty("name", out var name) ||
                    !asset.TryGetProperty("browser_download_url", out var link)) continue;
                var file = name.GetString();
                if (file is not ("SidelaneSetup.exe" or "SidelaneSetup.sha256")) continue;
                var exact = $"https://github.com/Davittaai/Sidelane/releases/download/{tag.GetString()}/{file}";
                if (link.GetString() != exact || !Uri.TryCreate(exact, UriKind.Absolute, out var candidate)) continue;
                if (file == "SidelaneSetup.exe") installer = candidate; else checksum = candidate;
            }
        return new SidelaneRelease(latest, uri, installer, checksum);
    }
}

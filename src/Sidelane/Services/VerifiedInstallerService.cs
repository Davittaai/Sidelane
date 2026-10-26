using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Sidelane.Services;

/// <summary>Downloads only the official release installer and verifies its SHA256 before execution.</summary>
public static class VerifiedInstallerService
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromMinutes(3) };
    private const long MaxBytes = 160L * 1024 * 1024;

    public static async Task<string> DownloadAsync(SidelaneRelease release, CancellationToken ct = default)
    {
        if (release.Installer is null || release.Checksum is null)
            throw new InvalidOperationException("No verified installer is available for this release.");
        var hashText = await Client.GetStringAsync(release.Checksum, ct);
        var match = Regex.Match(hashText, @"^([a-fA-F0-9]{64})\s+SidelaneSetup\.exe\s*$");
        if (!match.Success) throw new InvalidDataException("Release checksum has an invalid format.");

        var outputPath = Path.Combine(Path.GetTempPath(), $"SidelaneSetup-{Guid.NewGuid():N}.exe");
        try
        {
            using var response = await Client.GetAsync(release.Installer,
                HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > MaxBytes)
                throw new InvalidDataException("Installer exceeds the safety limit.");
            await using (var output = File.Create(outputPath))
            await using (var input = await response.Content.ReadAsStreamAsync(ct))
            {
                var buffer = new byte[65536];
                long total = 0;
                int count;
                while ((count = await input.ReadAsync(buffer, ct)) != 0)
                {
                    total += count;
                    if (total > MaxBytes) throw new InvalidDataException("Installer exceeds the safety limit.");
                    await output.WriteAsync(buffer.AsMemory(0, count), ct);
                }
            }
            await using var verify = File.OpenRead(outputPath);
            if (verify.Length < 1024) throw new InvalidDataException("Installer file is too small.");
            var actual = Convert.ToHexString(await SHA256.HashDataAsync(verify, ct));
            if (!actual.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Installer failed official release checksum verification.");
            return outputPath;
        }
        catch
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
            throw;
        }
    }
}

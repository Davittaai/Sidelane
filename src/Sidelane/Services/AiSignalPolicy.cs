using System.Text.Json;

namespace Sidelane.Services;

/// <summary>Only allow small state signals from the opt-in Chrome native bridge.</summary>
public static class AiSignalPolicy
{
    public static (string Provider, string State)? Parse(string? json)
    {
        if (string.IsNullOrEmpty(json) || json.Length > 256) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object || root.EnumerateObject().Count() != 2) return null;
            if (root.EnumerateObject().Any(x => x.Name is not ("provider" or "state"))) return null;
            var provider = root.GetProperty("provider").GetString();
            var state = root.GetProperty("state").GetString();
            if (provider is not ("ChatGPT" or "Claude" or "Grok")) return null;
            if (state is not ("generating" or "open" or "unknown" or "stopped")) return null;
            return (provider, state);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException) { return null; }
    }
}

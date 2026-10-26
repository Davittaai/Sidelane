using Sidelane.Models;

namespace Sidelane.Services;

/// <summary>Explicit or inferred provider mapping, independent of the user's shortcut display name.</summary>
public static class AiProviderPolicy
{
    public static string Resolve(ShortcutSettings shortcut)
    {
        if (shortcut.StatusProvider == "None") return "";
        if (shortcut.StatusProvider is "ChatGPT" or "Claude" or "Grok")
            return shortcut.StatusProvider;
        if (LinkPolicy.TryGetWebTarget(shortcut.Url, out var uri) && uri is not null)
        {
            if (uri.Host.Equals("chatgpt.com", StringComparison.OrdinalIgnoreCase)) return "ChatGPT";
            if (uri.Host.Equals("claude.ai", StringComparison.OrdinalIgnoreCase)) return "Claude";
            if (uri.Host.Equals("grok.com", StringComparison.OrdinalIgnoreCase)) return "Grok";
        }
        return shortcut.Name switch
        {
            "ChatGPT" => "ChatGPT",
            "Claude" => "Claude",
            "Grok" => "Grok",
            _ => ""
        };
    }
}

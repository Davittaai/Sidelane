namespace Sidelane.Services;

public static class LinkPolicy
{
    // Deliberately disallow file:, javascript:, shell:, and arbitrary protocols.
    public static bool TryGetWebTarget(string? input, out Uri? target)
    {
        target = null;
        if (string.IsNullOrWhiteSpace(input) ||
            !Uri.TryCreate(input.Trim(), UriKind.Absolute, out var candidate) ||
            candidate.Scheme is not ("https" or "http") ||
            string.IsNullOrWhiteSpace(candidate.Host) ||
            !string.IsNullOrEmpty(candidate.UserInfo)) return false;
        target = candidate;
        return true;
    }
}

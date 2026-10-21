namespace Sidelane.Services;

/// <summary>
/// Temporary per-app activity status and work description. This class deliberately
/// has no serialization or file I/O: notes disappear when Sidelane exits.
/// </summary>
public sealed class SessionActivityStore
{
    public sealed record Activity(string Status, string Note);
    private readonly Dictionary<string, Activity> _states = new(StringComparer.OrdinalIgnoreCase);

    public Activity Get(string name) => _states.TryGetValue(name, out var state)
        ? state : new Activity("", "");

    public void SetStatus(string name, string status)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var next = status is "working" or "waiting" or "ready" ? status : "";
        Update(name, Get(name) with { Status = next });
    }

    public void SetNote(string name, string? note)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        // Keep hover text compact and make notes strictly session-only.
        var next = (note ?? "").Trim();
        if (next.Length > 96) next = next[..96];
        Update(name, Get(name) with { Note = next });
    }

    private void Update(string name, Activity value)
    {
        if (value.Status.Length == 0 && value.Note.Length == 0)
            _states.Remove(name);
        else
            _states[name] = value;
    }
}

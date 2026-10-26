using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Sidelane.Services;

/// <summary>Per-user, in-memory browser activity signals. No disk, network listener or message bodies.</summary>
public sealed class AiStatusServer : IDisposable
{
    public const string PipeName = "Sidelane.BrowserState.V1";
    private readonly CancellationTokenSource _stop = new();
    private readonly Dictionary<string, (string State, DateTimeOffset Until)> _states = new();
    private readonly object _gate = new();
    public event Action? Updated;
    public bool WasUsed { get; private set; }
    private Task? _runner;

    public void Start() => _runner ??= Task.Run(ListenAsync);

    private async Task ListenAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await pipe.WaitForConnectionAsync(_stop.Token);
                using var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 256, leaveOpen: true);
                // The native host sends one small JSON line per message; refuse large lines.
                var line = await reader.ReadLineAsync(_stop.Token);
                if (line is not null && AiSignalPolicy.Parse(line) is { } signal)
                {
                    lock (_gate) _states[signal.Provider] = (signal.State, DateTimeOffset.UtcNow.AddSeconds(12));
                    WasUsed = true;
                    Updated?.Invoke();
                }
            }
            catch (OperationCanceledException) { break; }
            catch (IOException) { /* Browser host may disconnect during navigation. */ }
        }
    }

    public string Get(string provider)
    {
        lock (_gate)
        {
            if (!_states.TryGetValue(provider, out var status) || status.Until <= DateTimeOffset.UtcNow)
                return "";
            return status.State switch
            {
                "generating" => "Generating · browser",
                "stopped" => "Recently stopped · browser",
                _ => ""
            };
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _stop.Dispose();
    }
}

using Windows.Media.Control;

namespace Sidelane.Services;

public sealed record MediaInfo(string Title, string Artist, bool Playing, bool Available,
    bool CanPrevious = false, bool CanToggle = false, bool CanNext = false);

public sealed class MediaService
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private GlobalSystemMediaTransportControlsSession? _session;

    public async Task<MediaInfo> ReadAsync()
    {
        try
        {
            _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var sessions = _manager.GetSessions();
            // Prefer the playing session; a paused Spotify instance must not hide active audio.
            _session = sessions.FirstOrDefault(s => s.GetPlaybackInfo()?.PlaybackStatus ==
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                ?? _manager.GetCurrentSession()
                ?? sessions.FirstOrDefault();
            if (_session is null) return new("No active media", "Open Spotify or another player", false, false);
            var props = await _session.TryGetMediaPropertiesAsync();
            var info = _session.GetPlaybackInfo();
            var playing = info?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            var controls = info?.Controls;
            return new(string.IsNullOrWhiteSpace(props.Title) ? "Unknown track" : props.Title,
                props.Artist ?? "", playing, true,
                controls?.IsPreviousEnabled == true,
                controls?.IsPlayPauseToggleEnabled == true || controls?.IsPlayEnabled == true || controls?.IsPauseEnabled == true,
                controls?.IsNextEnabled == true);
        }
        catch (Exception)
        {
            _session = null;
            return new("Media unavailable", "Windows media session not exposed", false, false);
        }
    }

    public async Task ControlAsync(string command)
    {
        if (_session is null) return;
        try
        {
            switch (command)
            {
                case "playpause":
                    var info = _session.GetPlaybackInfo();
                    if (info?.Controls?.IsPlayPauseToggleEnabled == true)
                        await _session.TryTogglePlayPauseAsync();
                    else if (info?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing &&
                             info.Controls?.IsPauseEnabled == true)
                        await _session.TryPauseAsync();
                    else if (info?.Controls?.IsPlayEnabled == true)
                        await _session.TryPlayAsync();
                    break;
                case "next": await _session.TrySkipNextAsync(); break;
                case "previous": await _session.TrySkipPreviousAsync(); break;
            }
        }
        catch (Exception) { /* Player may reject external transport commands. */ }
    }
}

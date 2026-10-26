using System.Net.Http;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using Sidelane.Services;

namespace Sidelane;

public partial class App : System.Windows.Application
{
    private Mutex? _mutex;
    private System.Windows.Forms.NotifyIcon? _tray;
    private OverlayWindow? _overlay;
    private SettingsService? _settings;
    private bool _settingsOpen;
    private readonly CancellationTokenSource _updateStop = new();
    private Uri? _availableUpdate;
    private SidelaneRelease? _latestRelease;
    private bool _updating;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _mutex = new Mutex(true, "Local\\Sidelane.Overlay.Singleton", out var first);
        if (!first)
        {
            // A second launch should bring the existing instance back, not appear broken.
            WindowService.ShowExistingInstance();
            Shutdown();
            return;
        }
        _settings = new SettingsService();
        _overlay = new OverlayWindow(_settings);
        _overlay.SettingsRequested += ShowSettings;
        _overlay.ExitRequested += ExitApp;
        MainWindow = _overlay;
        _overlay.Show();
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Show / hide sidebar", null, (_, _) => Dispatcher.Invoke(() => _overlay.ToggleVisibility()));
        menu.Items.Add("Settings", null, (_, _) => Dispatcher.Invoke(ShowSettings));
        menu.Items.Add("Check for updates", null, (_, _) => Dispatcher.Invoke(async () => await CheckForUpdatesAsync(true)));
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Exit Sidelane", null, (_, _) => Dispatcher.Invoke(ExitApp));
        _tray = new System.Windows.Forms.NotifyIcon
        {
            Icon = TryGetAppIcon(),
            Text = "Sidelane | Local activity companion",
            Visible = true, ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => Dispatcher.Invoke(() => _overlay.ToggleVisibility());
        _tray.BalloonTipClicked += (_, _) => Dispatcher.BeginInvoke(async () => await OfferUpdateAsync());
        _ = UpdateLoopAsync();
    }

    private static System.Drawing.Icon TryGetAppIcon()
    {
        try { return Environment.ProcessPath is { } path
            ? System.Drawing.Icon.ExtractAssociatedIcon(path) ?? SystemIcons.Application
            : SystemIcons.Application; }
        catch (Exception) { return SystemIcons.Application; }
    }

    private static void OpenUpdatePage(Uri uri)
    {
        try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
        catch (System.ComponentModel.Win32Exception) { }
    }

    private async Task UpdateLoopAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), _updateStop.Token);
            while (!_updateStop.IsCancellationRequested)
            {
                await CheckForUpdatesAsync(false);
                await Task.Delay(TimeSpan.FromHours(24), _updateStop.Token);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task OfferUpdateAsync()
    {
        var release = _latestRelease;
        if (release is null || _updating) return;
        if (release.Installer is null || release.Checksum is null)
        { OpenUpdatePage(release.Page); return; }
        if (MessageBox.Show($"Download and verify Sidelane {release.Version} installer from the official GitHub release? The setup runs only with your approval.",
            "Sidelane update", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _updating = true;
        try
        {
            var path = await VerifiedInstallerService.DownloadAsync(release, _updateStop.Token);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            ExitApp(); // release installation files before setup replaces them
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or HttpRequestException or TaskCanceledException or System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            MessageBox.Show("Update could not be verified or started. Use the official release page instead.",
                "Sidelane update", MessageBoxButton.OK, MessageBoxImage.Warning);
            OpenUpdatePage(release.Page);
        }
        finally { _updating = false; }
    }

    private async Task CheckForUpdatesAsync(bool interactive)
    {
        try
        {
            var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
            var info = await UpdateService.GetNewReleaseAsync(current, _updateStop.Token);
            if (info is not null)
            {
                _latestRelease = info;
                if (interactive) { await OfferUpdateAsync(); return; }
                if (info.Page != _availableUpdate)
                {
                    _availableUpdate = info.Page;
                    _tray?.ShowBalloonTip(7000, "Sidelane update available",
                        $"Version {info.Version} is ready. Click to view the official release.",
                        System.Windows.Forms.ToolTipIcon.Info);
                }

            }
            else if (interactive)
                System.Windows.MessageBox.Show("You're on the latest stable release.",
                    "Sidelane updates", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            if (interactive) System.Windows.MessageBox.Show("Could not check GitHub releases.",
                "Sidelane updates", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ShowSettings()
    {
        if (_overlay is null || _settings is null) return;
        if (_settingsOpen) return;
        _settingsOpen = true;
        _overlay.HoldExpandedForDialog(true);
        try
        {
            var dialog = new SettingsWindow(_settings) { Owner = _overlay };
            if (dialog.ShowDialog() == true) _overlay.ApplyAppearance();
        }
        finally { _overlay.HoldExpandedForDialog(false); _settingsOpen = false; }
    }

    private void ExitApp()
    {
        _updateStop.Cancel();
        _overlay?.DisposeNative();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
        try { _mutex?.ReleaseMutex(); } catch (ApplicationException) { }
        _mutex?.Dispose();
        _updateStop.Dispose();
        base.OnExit(e);
    }
}

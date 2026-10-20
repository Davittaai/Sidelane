using System.Drawing;
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

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _mutex = new Mutex(true, "Local\\Sidelane.Overlay.Singleton", out var first);
        if (!first) { Shutdown(); return; }
        _settings = new SettingsService();
        _overlay = new OverlayWindow(_settings);
        _overlay.SettingsRequested += ShowSettings;
        _overlay.ExitRequested += Exit;
        MainWindow = _overlay;
        _overlay.Show();
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Show / hide sidebar", null, (_, _) => Dispatcher.Invoke(() => _overlay.ToggleVisibility()));
        menu.Items.Add("Settings", null, (_, _) => Dispatcher.Invoke(ShowSettings));
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Exit Sidelane", null, (_, _) => Dispatcher.Invoke(Exit));
        _tray = new System.Windows.Forms.NotifyIcon
        {
            Icon = TryGetAppIcon(),
            Text = "Sidelane | Local activity companion",
            Visible = true, ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => Dispatcher.Invoke(() => _overlay.ToggleVisibility());
    }

    private static System.Drawing.Icon TryGetAppIcon()
    {
        try { return Environment.ProcessPath is { } path
            ? System.Drawing.Icon.ExtractAssociatedIcon(path) ?? SystemIcons.Application
            : SystemIcons.Application; }
        catch (Exception) { return SystemIcons.Application; }
    }

    private void ShowSettings()
    {
        if (_overlay is null || _settings is null) return;
        if (_settingsOpen) return;
        _settingsOpen = true;
        try
        {
            var dialog = new SettingsWindow(_settings) { Owner = _overlay };
            if (dialog.ShowDialog() == true) _overlay.ApplyAppearance();
        }
        finally { _settingsOpen = false; }
    }

    private void Exit()
    {
        _overlay?.DisposeNative();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
        try { _mutex?.ReleaseMutex(); } catch (ApplicationException) { }
        _mutex?.Dispose();
        base.OnExit(e);
    }
}

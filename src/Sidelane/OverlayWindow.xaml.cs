using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Sidelane.Models;
using Sidelane.Services;

using CheckBox = System.Windows.Controls.CheckBox;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Button = System.Windows.Controls.Button;
using Brushes = System.Windows.Media.Brushes;
using ColorConverter = System.Windows.Media.ColorConverter;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using FontFamily = System.Windows.Media.FontFamily;

namespace Sidelane;

public partial class OverlayWindow : Window
{
    private double RailWidth => _settings.Current.RailWidth;
    private double ExpandedWidth => _settings.Current.PanelWidth;
    private readonly SettingsService _settings;
    private readonly WindowService _windows = new();
    private readonly MediaService _media = new();
    private readonly List<CheckBox> _tasks = new();
    private readonly SessionActivityStore _activities = new();
    private readonly AiStatusServer _ai = new();
    private readonly DispatcherTimer _refresh = new() { Interval = TimeSpan.FromSeconds(4) };
    private readonly DispatcherTimer _focusRefresh = new() { Interval = TimeSpan.FromMilliseconds(1000) };
    private readonly DispatcherTimer _collapseDelay = new() { Interval = TimeSpan.FromMilliseconds(240) };
    private IntPtr _hwnd;
    private bool _expanded;
    private bool _closing;
    private bool _refreshing;
    private bool _modalOpen;
    private IntPtr _lastFocused;
    private bool? _lastFullscreen;
    private IReadOnlyList<WindowEntry> _available = Array.Empty<WindowEntry>();

    public event Action? SettingsRequested;
    public event Action? ExitRequested;

    public OverlayWindow(SettingsService settings)
    {
        _settings = settings;
        InitializeComponent();
        _ai.Updated += () => Dispatcher.BeginInvoke(() => { if (!_closing) RebuildApps(); });
        // Restore the rail near its saved monitor before the first HWND is created.
        // Otherwise Windows starts it on the primary monitor and edge snapping
        // can silently discard the saved secondary-monitor position.
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = settings.Current.Left;
        Top = settings.Current.Top;
        _refresh.Tick += async (_, _) => await RefreshAsync();
        _focusRefresh.Tick += (_, _) =>
        {
            if (!IsVisible) return; // Hidden overlays should not rebuild their UI every second.
            var focused = NativeMethods.GetForegroundWindow();
            if (focused != _lastFocused) { _lastFocused = focused; RebuildApps(); }
            UpdateAlwaysOnTop();
        };
        _collapseDelay.Tick += (_, _) =>
        {
            _collapseDelay.Stop();
            if (!_modalOpen && _settings.Current.AutoCollapse && !IsMouseOver && !IsKeyboardFocusWithin)
                SetExpanded(false);
        };

    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        _windows.OwnHandle = _hwnd;
        HwndSource.FromHwnd(_hwnd)?.AddHook(WndProc);
        NativeMethods.RegisterHotKey(_hwnd, NativeMethods.HOTKEY_ID,
            NativeMethods.MOD_ALT | NativeMethods.MOD_CONTROL | NativeMethods.MOD_NOREPEAT,
            NativeMethods.VK_SPACE);
        // Do not relocate a saved secondary-monitor position back to the primary screen.
        Height = Math.Max(300, Math.Min(_settings.Current.PanelHeight, GetWorkArea().Height - 24));
        ApplyAppearance();
        SetExpanded(!_settings.Current.StartCollapsed);
        _ai.Start();
        ShowInTaskbar = _settings.Current.ShowTaskbarButton;
        if (_settings.Current.EnableAnimations) PlayEntrance();
        _refresh.Start();
        _focusRefresh.Start();
        UpdateAlwaysOnTop();
        await RefreshAsync();
    }

    // Only change Z-order when fullscreen state changes. Continuous SetWindowPos
    // requests would steal attention from other desktop workflows.
    private void UpdateAlwaysOnTop()
    {
        if (_hwnd == IntPtr.Zero || !IsVisible) return;
        var fullscreen = _settings.Current.RespectFullscreen &&
                         WindowService.IsForegroundFullscreen(_hwnd);
        if (_lastFullscreen == fullscreen) return;
        _lastFullscreen = fullscreen;
        Topmost = !fullscreen;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == NativeMethods.HOTKEY_ID)
        {
            Show();
            SetExpanded(true);
            Activate();
            _ = RefreshAsync();
            handled = true;
        }
        return IntPtr.Zero;
    }

    private Brush AccentBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.Current.Accent));

    private Rect GetWorkArea()
    {
        var screen = System.Windows.Forms.Screen.FromHandle(_hwnd);
        var bounds = screen.WorkingArea;
        // Screen gives device pixels; WPF layout uses device-independent pixels.
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
                        ?? Matrix.Identity;
        var a = transform.Transform(new System.Windows.Point(bounds.Left, bounds.Top));
        var b = transform.Transform(new System.Windows.Point(bounds.Right, bounds.Bottom));
        return new Rect(a, b);
    }

    public void ApplyAppearance()
    {
        var s = _settings.Current;
        FontFamily = new FontFamily(s.Font);
        ShowInTaskbar = s.ShowTaskbarButton;
        // Opacity applies to surfaces, not labels: keep text and icons legible.
        Surface.Opacity = 1;
        Surface.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(s.Background))
        { Opacity = s.Opacity };
        RailArea.Background = new SolidColorBrush(Color.FromRgb(32, 34, 38))
        { Opacity = s.Opacity };
        var wallpaper = BackgroundImagePolicy.TryCreateBrush(s.BackgroundImage);
        // Optional wallpaper should never compete with content readability.
        if (wallpaper is not null) wallpaper.Opacity = 0.13;
        WallPaperLayer.Background = (Brush?)wallpaper ?? Brushes.Transparent;
        PrivacyDot.Fill = AccentBrush;
        // The branded icon keeps its own colors independently of the selected accent.
        PinButton.Foreground = s.Locked ? AccentBrush : new SolidColorBrush(Color.FromRgb(160,170,187));
        var work = GetWorkArea();
        Height = Math.Max(300, Math.Min(s.PanelHeight, work.Height - 24));
        s.Left = ScreenPlacement.EdgeRailLeft(s.Edge, s.Left, work.Left, work.Right, railWidth: RailWidth);
        SetExpanded(_expanded);
        _lastFullscreen = null;
        UpdateAlwaysOnTop();
        RebuildApps();
    }

    private void PlayEntrance()
    {
        Surface.Opacity = 0;
        Surface.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(230))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
    }

    private void AnimateExpansion()
    {
        var offset = new TranslateTransform(10, 0);
        ExpandedPane.RenderTransform = offset;
        ExpandedPane.Opacity = 0;
        offset.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(165))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        ExpandedPane.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(170)));
    }

    private void SetExpanded(bool expand)
    {
        var wasExpanded = _expanded;
        _expanded = expand;
        var settings = _settings.Current;
        var area = GetWorkArea();
        if (settings.Edge == "Free")
            settings.RailOnRight = ScreenPlacement.RailOnRightForFree(settings.Left, area.Left, area.Right, RailWidth, ExpandedWidth);
        var railOnRight = expand && settings.RailOnRight;
        RailArea.Width = RailWidth;
        FirstColumn.Width = new GridLength(railOnRight ? ExpandedWidth - RailWidth : RailWidth);
        SecondColumn.Width = new GridLength(railOnRight ? RailWidth : ExpandedWidth - RailWidth);
        Grid.SetColumn(RailArea, railOnRight ? 1 : 0);
        Grid.SetColumn(ExpandedPane, railOnRight ? 0 : 1);
        Width = expand ? ExpandedWidth : RailWidth;
        // Keep the 54px rail anchored to the same screen coordinate in both modes.
        var railLeft = ScreenPlacement.EdgeRailLeft(_settings.Current.Edge, _settings.Current.Left,
            area.Left, area.Right, railWidth: RailWidth);
        Left = ScreenPlacement.WindowLeft(railLeft, expand, _settings.Current.RailOnRight,
            area.Left, area.Right, railWidth: RailWidth, expandedWidth: ExpandedWidth);
        Top = ScreenPlacement.WindowTop(_settings.Current.Top, Height, area.Top, area.Bottom);
        ExpandedPane.Visibility = expand ? Visibility.Visible : Visibility.Collapsed;
        LogoButton.ToolTip = expand ? "Collapse" : "Expand";
        if (expand && !wasExpanded && settings.EnableAnimations) AnimateExpansion();
        else { ExpandedPane.BeginAnimation(OpacityProperty, null); ExpandedPane.Opacity = 1; }
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        _collapseDelay.Stop();
        if (_settings.Current.AutoCollapse && !_expanded) SetExpanded(true);
    }

    private void Window_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!_modalOpen && _settings.Current.AutoCollapse && _expanded) _collapseDelay.Start();
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        if (!_modalOpen && _settings.Current.AutoCollapse && _expanded && !IsMouseOver)
            _collapseDelay.Start();
    }

    // Keep an owned dialog's parent expanded until the editor closes.
    public void HoldExpandedForDialog(bool hold)
    {
        _modalOpen = hold;
        if (hold) { _collapseDelay.Stop(); SetExpanded(true); }
        else if (_settings.Current.AutoCollapse && !IsMouseOver && !IsKeyboardFocusWithin)
            _collapseDelay.Start();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { SetExpanded(false); e.Handled = true; }
    }

    private void Logo_Click(object sender, RoutedEventArgs e) => SetExpanded(!_expanded);
    private void Collapse_Click(object sender, RoutedEventArgs e) => SetExpanded(false);
    private void Settings_Click(object sender, RoutedEventArgs e) => SettingsRequested?.Invoke();
    private void Hide_Click(object sender, RoutedEventArgs e) => Hide();
    private void Exit_Click(object sender, RoutedEventArgs e) => ExitRequested?.Invoke();

    private void Pin_Click(object sender, RoutedEventArgs e)
    {
        var previous = _settings.Current.Locked;
        _settings.Current.Locked = !previous;
        if (!TrySavePreferences()) _settings.Current.Locked = previous;
        ApplyAppearance();
    }

    private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_settings.Current.Locked) return;
        try
        {
            DragMove();
            // Dragging switches to an absolute free position; the app retains that placement.
            _settings.Current.Left = Left + (_settings.Current.RailOnRight && _expanded ? ExpandedWidth - RailWidth : 0);
            _settings.Current.Top = Top;
            _settings.Current.Edge = "Free";
            TrySavePreferences();
            SetExpanded(_expanded);
        }
        catch (InvalidOperationException) { }
        e.Handled = true;
    }

    private bool TrySavePreferences()
    {
        try { _settings.Save(); return true; }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            LogoButton.ToolTip = "Could not save position or settings. Check access to your AppData folder.";
            return false;
        }
    }

    private async Task RefreshAsync()
    {
        if (_refreshing || _closing || !IsVisible) return;
        _refreshing = true;
        try
        {
            var current = _windows.Enumerate();
            if (!_available.SequenceEqual(current))
            {
                _available = current;
                RebuildApps();
            }
            if (_ai.PruneExpired()) RebuildApps(); // update once when a signal expires
            var media = await _media.ReadAsync();
            if (_closing) return;
            SongTitle.Text = media.Title;
            SongArtist.Text = media.Artist;
            PlayPause.Content = media.Playing ? "Ⅱ" : "▶";
            MediaProvider.Text = media.Available ? "System media" : "Not connected";
            PlayPause.IsEnabled = media.CanToggle;
            PreviousTrack.IsEnabled = media.CanPrevious;
            NextTrack.IsEnabled = media.CanNext;
            LoadingBar.Visibility = Visibility.Collapsed;
        }
        catch (Exception)
        {
            // Transient failures (closing windows, revoked media sessions) are recoverable.
            SongTitle.Text = "Temporarily unavailable";
            SongArtist.Text = "Retrying automatically";
            PlayPause.IsEnabled = false;
            PreviousTrack.IsEnabled = false;
            NextTrack.IsEnabled = false;
            LoadingBar.Visibility = Visibility.Collapsed;
        }
        finally { _refreshing = false; }
    }

    private void EditActivityNote(string name)
    {
        var previous = _activities.Get(name).Note;
        HoldExpandedForDialog(true);
        string? response;
        try { response = ActivityNoteWindow.Prompt(this, name, previous); }
        finally { HoldExpandedForDialog(false); }
        if (response is null) return;
        _activities.SetNote(name, response);
        RebuildApps();
    }

    private static string ToStatusName(string value) => value switch
    {
        "working" => "Working · manual",
        "waiting" => "Waiting · manual",
        "ready" => "Ready · manual",
        _ => ""
    };

    private void RebuildApps()
    {
        if (AppRows is null || RailItems is null) return;
        AppRows.Items.Clear();
        RailItems.Items.Clear();
        var visible = 0;
        var filter = SearchApps?.Text.Trim() ?? "";
        foreach (var item in _settings.Current.Shortcuts)
        {
            var target = _available.FirstOrDefault(w => WindowService.Matches(w, item.Match));
            if (target is not null) visible++;
            var activity = _activities.Get(item.Name);
            var manual = activity.Status.Length > 0 ? ToStatusName(activity.Status) : null;
            var observed = _ai.Get(item.Name);
            var generating = observed.StartsWith("Generating", StringComparison.Ordinal);
            var isFocused = target is not null && WindowService.IsFocused(target.Handle);
            var state = manual ?? (observed.Length > 0 ? observed : isFocused ? "In focus" :
                target is not null ? "Open" :
                LinkPolicy.TryGetWebTarget(item.Url, out _) ? "Open website" : "Not open");
            if (activity.Note.Length > 0)
                state = generating ? observed + " · " + activity.Note : activity.Note;
            var dotColor = manual is not null || observed.Length > 0 || activity.Note.Length > 0 || isFocused ? AccentBrush :
                (target is not null ? new SolidColorBrush(Color.FromRgb(127,135,139)) :
                new SolidColorBrush(Color.FromRgb(91,96,104)));

            var btn = new Button
            {
                Style = (Style)FindResource("AppRowButton"),
                Height = 48, Margin = new Thickness(0,0,0,2),
                ToolTip = BuildShortcutTooltip(item.Name, state, target?.Title, manual)
            };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            var ico = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(44,47,53)), CornerRadius = new CornerRadius(9),
                Width = 32, Height = 32, Child = new TextBlock
                {
                    Text = item.Symbol, FontSize = 17, Foreground = isFocused ? AccentBrush : new SolidColorBrush(Color.FromRgb(203,207,209)),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Grid.SetColumn(ico, 0); grid.Children.Add(ico);
            var labels = new StackPanel { Margin = new Thickness(9,2,0,0) };
            labels.Children.Add(new TextBlock { Text = item.Name, FontWeight = FontWeights.Medium, FontSize = 12.3, Foreground = new SolidColorBrush(Color.FromRgb(236,237,235)), TextTrimming = TextTrimming.CharacterEllipsis });
            labels.Children.Add(new TextBlock { Text = state, FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(148,152,158)), Margin = new Thickness(0,3,0,0), TextTrimming = TextTrimming.CharacterEllipsis });
            Grid.SetColumn(labels, 1); grid.Children.Add(labels);
            var dot = new Ellipse { Width = 6, Height = 6, Fill = dotColor, VerticalAlignment = VerticalAlignment.Center };
            if (generating)
            {
                var wave = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
                for (var i = 0; i < 3; i++)
                {
                    var bar = new System.Windows.Shapes.Rectangle { Width = 2.5, Height = 12, RadiusX = 1.2, RadiusY = 1.2,
                        Fill = AccentBrush, Margin = new Thickness(1.3, 0, 1.3, 0),
                        RenderTransformOrigin = new System.Windows.Point(.5, .5),
                        RenderTransform = new ScaleTransform(1, .38) };
                    if (_settings.Current.EnableAnimations)
                        bar.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty,
                            new DoubleAnimation(.3, 1, TimeSpan.FromMilliseconds(470 + i * 95))
                            { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
                    wave.Children.Add(bar);
                }
                Grid.SetColumn(wave, 2);
                grid.Children.Add(wave);
            }
            else
            {
                Grid.SetColumn(dot, 2); grid.Children.Add(dot);
            }
            btn.Content = grid;
            btn.Click += (_, _) => ActivateShortcut(item);
            var context = new ContextMenu();
            foreach (var pair in new[] { ("Mark working", "working"), ("Mark waiting", "waiting"), ("Mark ready", "ready"), ("Clear manual status", "") })
            {
                var menu = new MenuItem { Header = pair.Item1 };
                menu.Click += (_, _) => { _activities.SetStatus(item.Name, pair.Item2); RebuildApps(); };
                context.Items.Add(menu);
            }
            context.Items.Add(new Separator());
            var editNote = new MenuItem { Header = "Set activity note..." };
            editNote.Click += (_, _) => EditActivityNote(item.Name);
            context.Items.Add(editNote);
            var clearNote = new MenuItem { Header = "Clear activity note", IsEnabled = activity.Note.Length > 0 };
            clearNote.Click += (_, _) => { _activities.SetNote(item.Name, ""); RebuildApps(); };
            context.Items.Add(clearNote);
            btn.ContextMenu = context;
            if (filter.Length == 0 || item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                state.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(btn, 0);
                row.Children.Add(btn);
                var noteButton = new Button
                {
                    Content = "✎",
                    ToolTip = "Set what " + item.Name + " is working on (manual note)",
                    Style = (Style)FindResource("IconButton"),
                    Width = 25,
                    Height = 31,
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                noteButton.Click += (_, _) => EditActivityNote(item.Name);
                Grid.SetColumn(noteButton, 1);
                row.Children.Add(noteButton);
                AppRows.Items.Add(row);
            }

            if (RailItems.Items.Count < 6)
            {
                var rail = new Button
                {
                    Content = item.Symbol, FontSize = 17, Width = 33, Height = 34,
                    Foreground = isFocused ? AccentBrush : new SolidColorBrush(Color.FromRgb(170,175,178)),
                    Style = (Style)FindResource("IconButton"),
                    ToolTip = BuildShortcutTooltip(item.Name, state, target?.Title, manual), Margin = new Thickness(0,0,0,4),
                    Background = isFocused ? new SolidColorBrush(Color.FromRgb(47,55,53)) : Brushes.Transparent
                };
                rail.Click += (_, _) => ActivateShortcut(item);
                RailItems.Items.Add(rail);
            }
        }
        AppsCount.Text = $"{visible} desktop";
        StatusSummary.Text = _settings.Current.Shortcuts.Count + " shortcuts · " + _settings.Current.Shortcuts.Count(i => _ai.Get(i.Name).StartsWith("Generating", StringComparison.Ordinal)) + " working";
    }

    private static string BuildShortcutTooltip(string appName, string state, string? windowTitle, string? manualStatus)
    {
        var tooltip = appName + "\n" + state;
        if (manualStatus is not null && !state.Equals(manualStatus, StringComparison.Ordinal))
            tooltip += "\n" + manualStatus;
        // Avoid exposing private browser conversation titles in app tooltips.
        return tooltip;
    }

    private void SearchApps_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SearchHint is not null) SearchHint.Visibility = string.IsNullOrEmpty(SearchApps.Text) ? Visibility.Visible : Visibility.Collapsed;
        RebuildApps();
    }

    private void NewTask_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (TaskHint is not null) TaskHint.Visibility = string.IsNullOrEmpty(NewTask.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ActivateShortcut(ShortcutSettings shortcut)
    {
        if (shortcut.LaunchMode is "Auto" or "App")
        {
            var existing = _available.FirstOrDefault(w => WindowService.Matches(w, shortcut.Match));
            if (existing is not null) { WindowService.Focus(existing.Handle); return; }
        }
        var plan = LaunchPolicy.Create(shortcut, BrowserLocator.Locate);
        if (plan is null)
        {
            System.Windows.MessageBox.Show("No usable launch target found. In Preferences, choose a browser or select the installed app executable.", 
                "Sidelane shortcut", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try { Process.Start(plan); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or System.IO.IOException)
        {
            System.Windows.MessageBox.Show("Couldn't open the selected app or browser. Check its installation and shortcut settings.", 
                "Sidelane shortcut", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Prev_Click(object sender, RoutedEventArgs e) { await _media.ControlAsync("previous"); await RefreshAsync(); }
    private async void Play_Click(object sender, RoutedEventArgs e) { await _media.ControlAsync("playpause"); await RefreshAsync(); }
    private async void Next_Click(object sender, RoutedEventArgs e) { await _media.ControlAsync("next"); await RefreshAsync(); }

    private void NewTask_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { AddTask_Click(sender, new RoutedEventArgs()); e.Handled = true; }
    }

    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var label = NewTask.Text.Trim();
        if (label.Length == 0 || _tasks.Count >= 30) return;
        var cb = new CheckBox { Content = label, Style = (Style)FindResource("TaskCheck"),
            VerticalAlignment = VerticalAlignment.Center };
        var row = new Grid { Margin = new Thickness(0,0,0,3) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(cb);
        var remove = new Button { Content = "×", ToolTip = "Remove task", Width = 27, Height = 27,
            Style = (Style)FindResource("IconButton"), FontSize = 15, Margin = new Thickness(5,0,0,0) };
        Grid.SetColumn(remove, 1);
        row.Children.Add(remove);
        void DeleteTask() { _tasks.Remove(cb); TaskList.Items.Remove(row); UpdateTaskCount(); }
        remove.Click += (_, _) => DeleteTask();
        var menu = new ContextMenu();
        var delete = new MenuItem { Header = "Remove task" };
        delete.Click += (_, _) => DeleteTask();
        menu.Items.Add(delete);
        cb.ContextMenu = menu;
        cb.Checked += (_, _) => UpdateTaskCount();
        cb.Unchecked += (_, _) => UpdateTaskCount();
        _tasks.Add(cb);
        TaskList.Items.Add(row);
        UpdateTaskCount();
        NewTask.Clear();
    }

    private void UpdateTaskCount()
    {
        if (ScratchpadTitle is not null)
            ScratchpadTitle.Text = _tasks.Count == 0 ? "Scratchpad" : $"Scratchpad  ·  {_tasks.Count(t => t.IsChecked == true)}/{_tasks.Count}";
    }

    public void ToggleVisibility()
    {
        if (IsVisible) Hide();
        else { Show(); _lastFullscreen = null; SetExpanded(true); UpdateAlwaysOnTop(); Activate(); _ = RefreshAsync(); }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!_closing) { e.Cancel = true; Hide(); }
    }

    public void DisposeNative()
    {
        _refresh.Stop(); _focusRefresh.Stop(); _collapseDelay.Stop();
        _ai.Dispose();
        if (_hwnd != IntPtr.Zero) NativeMethods.UnregisterHotKey(_hwnd, NativeMethods.HOTKEY_ID);
        _closing = true;
        Close();
    }
}

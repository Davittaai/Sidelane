using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Win32;
using Sidelane.Models;
using Sidelane.Services;

using ComboBox = System.Windows.Controls.ComboBox;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using ColorDialog = System.Windows.Forms.ColorDialog;
using DrawingColor = System.Drawing.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using MediaColor = System.Windows.Media.Color;

namespace Sidelane;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settings;
    private readonly ObservableCollection<ShortcutSettings> _shortcuts;

    public SettingsWindow(SettingsService settings)
    {
        _settings = settings;
        InitializeComponent();
        SourceInitialized += (_, _) => WindowThemeService.RequestDarkTitleBar(new WindowInteropHelper(this).Handle);
        var s = settings.Current;
        AccentInput.Text = s.Accent;
        BackgroundInput.Text = s.Background;
        ImagePath.Text = s.BackgroundImage;
        OpacityInput.Value = s.Opacity;
        ShowTaskbarInput.IsChecked = s.ShowTaskbarButton;
        AnimationsInput.IsChecked = s.EnableAnimations;
        RailWidthInput.Value = s.RailWidth;
        PanelWidthInput.Value = s.PanelWidth;
        PanelHeightInput.Value = s.PanelHeight;
        AutoCollapseInput.IsChecked = s.AutoCollapse;
        RespectFullscreenInput.IsChecked = s.RespectFullscreen;
        StartupAtLoginInput.IsChecked = StartupService.IsEnabled();
        StartCollapsedInput.IsChecked = s.StartCollapsed;
        LockPositionInput.IsChecked = s.Locked;
        SelectCombo(FontCombo, s.Font);
        SelectCombo(EdgeCombo, s.Edge);
        _shortcuts = new ObservableCollection<ShortcutSettings>(s.Shortcuts.Select(x => new ShortcutSettings
        { Name = x.Name, Symbol = x.Symbol, Url = x.Url, Match = x.Match,
          StatusProvider = x.StatusProvider, LaunchMode = x.LaunchMode, Browser = x.Browser, BrowserAppWindow = x.BrowserAppWindow, AppPath = x.AppPath }));
        ShortcutList.ItemsSource = _shortcuts;
        if (_shortcuts.Count > 0) ShortcutList.SelectedIndex = 0;
    }

    private void AppearanceNav_Click(object sender, RoutedEventArgs e) => AppearanceSection.BringIntoView();
    private void LayoutNav_Click(object sender, RoutedEventArgs e) => LayoutSection.BringIntoView();
    private void AppsNav_Click(object sender, RoutedEventArgs e) => AppsSection.BringIntoView();

    private static void SelectCombo(ComboBox box, string value)
    {
        foreach (ComboBoxItem item in box.Items)
            if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            { box.SelectedItem = item; return; }
        box.SelectedIndex = 0;
    }

    private void Size_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RailWidthValue is not null) RailWidthValue.Text = $"{RailWidthInput.Value:0} px";
        if (PanelWidthValue is not null) PanelWidthValue.Text = $"{PanelWidthInput.Value:0} px";
        if (PanelHeightValue is not null) PanelHeightValue.Text = $"{PanelHeightInput.Value:0} px";
    }

    private void ResetSize_Click(object sender, RoutedEventArgs e)
    {
        RailWidthInput.Value = ScreenPlacement.RailWidth;
        PanelWidthInput.Value = ScreenPlacement.ExpandedWidth;
        PanelHeightInput.Value = 560;
    }

    private void Opacity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityValue is not null) OpacityValue.Text = $"{e.NewValue:P0}";
    }

    private void Palette_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string token })
        {
            var pair = token.Split('|');
            if (pair.Length == 2) { AccentInput.Text = pair[0]; BackgroundInput.Text = pair[1]; }
        }
    }

    private void ColorInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (AccentPreview is null || AccentInput is null) return;
        var value = SettingsService.ValidColor(AccentInput.Text, "");
        if (value.Length == 7)
            AccentPreview.Fill = new SolidColorBrush((MediaColor)System.Windows.Media.ColorConverter.ConvertFromString(value));
        var background = SettingsService.ValidColor(BackgroundInput?.Text, "");
        if (ThemePreview is not null && background.Length == 7 && ThemeContrastPolicy.SupportsDarkUi(background))
            ThemePreview.Background = new SolidColorBrush((MediaColor)System.Windows.Media.ColorConverter.ConvertFromString(background));
    }

    private void PickAccent_Click(object sender, RoutedEventArgs e) => PickColor(AccentInput);
    private void PickBackground_Click(object sender, RoutedEventArgs e) => PickColor(BackgroundInput);

    private static void PickColor(System.Windows.Controls.TextBox target)
    {
        using var dialog = new ColorDialog { FullOpen = true, AnyColor = true };
        var hex = SettingsService.ValidColor(target.Text, "#191C20");
        dialog.Color = DrawingColorTranslator(hex);
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            target.Text = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
    }

    private static DrawingColor DrawingColorTranslator(string hex) =>
        DrawingColor.FromArgb(Convert.ToInt32(hex.Substring(1, 2), 16),
            Convert.ToInt32(hex.Substring(3, 2), 16),
            Convert.ToInt32(hex.Substring(5, 2), 16));

    private void BrowseApp_Click(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog {
            Filter = "Windows applications and shortcuts|*.exe;*.lnk",
            CheckFileExists = true,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            RestoreDirectory = true
        };
        if (picker.ShowDialog() == true) AppPathInput.Text = picker.FileName;
    }

    private void LaunchMode_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (BrowserFields is null || AppFields is null) return;
        var mode = (LaunchModeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Auto";
        BrowserFields.Visibility = mode == "App" ? Visibility.Collapsed : Visibility.Visible;
        AppFields.Visibility = mode == "Browser" ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*" };
        if (picker.ShowDialog() == true) ImagePath.Text = picker.FileName;
    }

    private void ShortcutList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ShortcutList.SelectedItem is not ShortcutSettings selected) return;
        NameInput.Text = selected.Name;
        SymbolInput.Text = selected.Symbol;
        MatchInput.Text = selected.Match;
        UrlInput.Text = selected.Url;
        AppPathInput.Text = selected.AppPath;
        BrowserAppWindowInput.IsChecked = selected.BrowserAppWindow;
        SelectCombo(LaunchModeCombo, selected.LaunchMode);
        SelectCombo(StatusProviderCombo, selected.StatusProvider);
        SelectCombo(BrowserCombo, selected.Browser);
    }

    private void NewShortcut_Click(object sender, RoutedEventArgs e)
    {
        ShortcutList.SelectedIndex = -1;
        NameInput.Clear(); SymbolInput.Text = "◈"; MatchInput.Clear(); UrlInput.Clear();
        AppPathInput.Clear(); BrowserAppWindowInput.IsChecked = false;
        SelectCombo(LaunchModeCombo, "Auto"); SelectCombo(BrowserCombo, "Default");
        SelectCombo(StatusProviderCombo, "Auto");
        NameInput.Focus();
    }

    private void ApplyShortcut_Click(object sender, RoutedEventArgs e) => TryApplyShortcut();

    private bool TryApplyShortcut()
    {
        Validation.Text = "";
        var name = NameInput.Text.Trim();
        if (name.Length == 0) { Validation.Text = "App name is required."; return false; }
        var url = UrlInput.Text.Trim();
        var mode = (LaunchModeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Auto";
        var browser = (BrowserCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Default";
        var appPath = AppPathInput.Text.Trim();
        if (url.Length > 0 && !LinkPolicy.TryGetWebTarget(url, out _))
        { Validation.Text = "Use a valid http(s) URL."; return false; }
        if (mode == "App" && (appPath.Length == 0 || !System.IO.File.Exists(appPath) ||
            !(appPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
              appPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))))
        { Validation.Text = "Choose an installed .exe or Windows shortcut (.lnk) for App mode."; return false; }
        if (mode == "Browser" && url.Length == 0)
        { Validation.Text = "Provide a website URL for Browser mode."; return false; }
        if (ShortcutList.SelectedItem is not ShortcutSettings && _shortcuts.Count >= 12)
        { Validation.Text = "Maximum 12 app slots."; return false; }
        if (_shortcuts.Any(x => x != ShortcutList.SelectedItem &&
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        { Validation.Text = "Choose a different name for this app."; return false; }
        var replacement = new ShortcutSettings { Name = name, Symbol = string.IsNullOrWhiteSpace(SymbolInput.Text) ? "◈" : SymbolInput.Text,
            Match = MatchInput.Text.Trim(), Url = url,
            StatusProvider = (StatusProviderCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Auto",
            LaunchMode = mode, Browser = browser, BrowserAppWindow = BrowserAppWindowInput.IsChecked == true,
            AppPath = appPath };
        var index = ShortcutList.SelectedIndex;
        if (index < 0) { _shortcuts.Add(replacement); ShortcutList.SelectedItem = replacement; }
        else { _shortcuts[index] = replacement; ShortcutList.SelectedIndex = index; }
        return true;
    }

    private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (ShortcutList.SelectedItem is ShortcutSettings chosen)
        {
            _shortcuts.Remove(chosen);
            ShortcutList.SelectedIndex = -1;
            NameInput.Clear(); SymbolInput.Text = "◈"; MatchInput.Clear(); UrlInput.Clear();
        }
    }

    private void ExportSettings_Click(object sender, RoutedEventArgs e)
    {
        Validation.Text = "";
        var dialog = new SaveFileDialog
        {
            Title = "Export saved Sidelane preferences",
            Filter = "Sidelane preferences (*.json)|*.json",
            FileName = "sidelane-settings.json",
            AddExtension = true,
            OverwritePrompt = true
        };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            _settings.Export(dialog.FileName);
            Validation.Text = "Saved preferences exported. Review shortcut URLs and local paths before sharing.";
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            Validation.Text = "Export failed: " + ex.Message;
        }
    }

    private void ImportSettings_Click(object sender, RoutedEventArgs e)
    {
        Validation.Text = "";
        var dialog = new OpenFileDialog
        {
            Title = "Import Sidelane preferences",
            Filter = "Sidelane preferences (*.json)|*.json",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != true) return;
        var decision = System.Windows.MessageBox.Show(
            "Replace your current saved Sidelane appearance and shortcuts with the selected file? Temporary tasks will remain unchanged until exit.",
            "Import preferences", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (decision != MessageBoxResult.Yes) return;
        try
        {
            _settings.Import(dialog.FileName);
            DialogResult = true; // Apply imported settings immediately in the existing overlay.
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException
            or System.Text.Json.JsonException or InvalidDataException)
        {
            Validation.Text = "Import rejected: " + ex.Message;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Validation.Text = "";
        // Save applies an in-progress editor row too, including newly created shortcuts.
        var changed = ShortcutList.SelectedItem is ShortcutSettings chosen
            ? NameInput.Text.Trim() != chosen.Name || SymbolInput.Text != chosen.Symbol ||
              MatchInput.Text.Trim() != chosen.Match || UrlInput.Text.Trim() != chosen.Url ||
              AppPathInput.Text.Trim() != chosen.AppPath ||
              (StatusProviderCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() != chosen.StatusProvider ||
              (LaunchModeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() != chosen.LaunchMode ||
              (BrowserCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() != chosen.Browser ||
              (BrowserAppWindowInput.IsChecked == true) != chosen.BrowserAppWindow
            : !string.IsNullOrWhiteSpace(NameInput.Text);
        if (changed && !TryApplyShortcut()) return;
        var validAccent = SettingsService.ValidColor(AccentInput.Text, "");
        var validBackground = SettingsService.ValidColor(BackgroundInput.Text, "");
        if (validAccent.Length == 0 || validBackground.Length == 0)
        { Validation.Text = "Colors must be #RRGGBB."; return; }
        if (!ThemeContrastPolicy.SupportsDarkUi(validBackground))
        { Validation.Text = "Choose a darker surface color so text stays readable."; return; }
        if (!string.IsNullOrWhiteSpace(ImagePath.Text) && !ImageFilePolicy.IsSupported(ImagePath.Text.Trim()))
        { Validation.Text = "Choose a PNG, JPG or BMP file smaller than 12 MB, or clear the image field."; return; }
        var s = _settings.EditableCopy();
        s.Accent = validAccent;
        s.Background = validBackground;
        s.Font = (FontCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Segoe UI";
        s.Edge = (EdgeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Left";
        s.RailWidth = RailWidthInput.Value;
        s.PanelWidth = PanelWidthInput.Value;
        s.PanelHeight = PanelHeightInput.Value;
        s.Opacity = OpacityInput.Value;
        s.AutoCollapse = AutoCollapseInput.IsChecked == true;
        s.RespectFullscreen = RespectFullscreenInput.IsChecked == true;
        s.StartCollapsed = StartCollapsedInput.IsChecked == true;
        s.ShowTaskbarButton = ShowTaskbarInput.IsChecked == true;
        s.EnableAnimations = AnimationsInput.IsChecked == true;
        s.Locked = LockPositionInput.IsChecked == true;
        s.BackgroundImage = ImagePath.Text.Trim();
        s.Shortcuts = _shortcuts.Select(x => new ShortcutSettings
        { Name = x.Name, Symbol = x.Symbol, Match = x.Match, Url = x.Url,
          StatusProvider = x.StatusProvider, LaunchMode = x.LaunchMode, Browser = x.Browser, BrowserAppWindow = x.BrowserAppWindow,
          AppPath = x.AppPath }).ToList();
        var autoStart = StartupAtLoginInput.IsChecked == true;
        var originallyEnabled = StartupService.IsEnabled();
        try
        {
            // Registry and JSON are separate stores. Revert the registry if saving
            // the settings fails, and do not mutate live settings on failure.
            if (autoStart != originallyEnabled) StartupService.SetEnabled(autoStart);
            s.StartupAtLogin = autoStart;
            _settings.Replace(s);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            try { if (autoStart != originallyEnabled) StartupService.SetEnabled(originallyEnabled); }
            catch (Exception) { /* Report the original failure without masking it. */ }
            Validation.Text = "Couldn't apply settings: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

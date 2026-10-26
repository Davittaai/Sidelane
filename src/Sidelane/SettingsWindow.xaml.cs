using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sidelane.Models;
using Sidelane.Services;

using ComboBox = System.Windows.Controls.ComboBox;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Sidelane;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settings;
    private readonly ObservableCollection<ShortcutSettings> _shortcuts;

    public SettingsWindow(SettingsService settings)
    {
        _settings = settings;
        InitializeComponent();
        var s = settings.Current;
        AccentInput.Text = s.Accent;
        BackgroundInput.Text = s.Background;
        ImagePath.Text = s.BackgroundImage;
        OpacityInput.Value = s.Opacity;
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
        { Name = x.Name, Symbol = x.Symbol, Url = x.Url, Match = x.Match }));
        ShortcutList.ItemsSource = _shortcuts;
        if (_shortcuts.Count > 0) ShortcutList.SelectedIndex = 0;
    }

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
        PanelHeightInput.Value = 724;
    }

    private void Opacity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityValue is not null) OpacityValue.Text = $"{e.NewValue:P0}";
    }

    private void Palette_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string color }) AccentInput.Text = color;
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
    }

    private void NewShortcut_Click(object sender, RoutedEventArgs e)
    {
        ShortcutList.SelectedIndex = -1;
        NameInput.Clear(); SymbolInput.Text = "◈"; MatchInput.Clear(); UrlInput.Clear();
        NameInput.Focus();
    }

    private void ApplyShortcut_Click(object sender, RoutedEventArgs e) => TryApplyShortcut();

    private bool TryApplyShortcut()
    {
        Validation.Text = "";
        var name = NameInput.Text.Trim();
        if (name.Length == 0) { Validation.Text = "App name is required."; return false; }
        var url = UrlInput.Text.Trim();
        if (url.Length > 0 && !LinkPolicy.TryGetWebTarget(url, out _))
        { Validation.Text = "Use a valid http(s) URL."; return false; }
        if (ShortcutList.SelectedItem is not ShortcutSettings && _shortcuts.Count >= 12)
        { Validation.Text = "Maximum 12 app slots."; return false; }
        if (_shortcuts.Any(x => x != ShortcutList.SelectedItem &&
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        { Validation.Text = "Choose a different name for this app."; return false; }
        var replacement = new ShortcutSettings { Name = name, Symbol = string.IsNullOrWhiteSpace(SymbolInput.Text) ? "◈" : SymbolInput.Text,
            Match = MatchInput.Text.Trim(), Url = url };
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

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Validation.Text = "";
        // Save applies an in-progress editor row too, including newly created shortcuts.
        var changed = ShortcutList.SelectedItem is ShortcutSettings chosen
            ? NameInput.Text.Trim() != chosen.Name || SymbolInput.Text != chosen.Symbol ||
              MatchInput.Text.Trim() != chosen.Match || UrlInput.Text.Trim() != chosen.Url
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
        s.Locked = LockPositionInput.IsChecked == true;
        s.BackgroundImage = ImagePath.Text.Trim();
        s.Shortcuts = _shortcuts.Select(x => new ShortcutSettings
        { Name = x.Name, Symbol = x.Symbol, Match = x.Match, Url = x.Url }).ToList();
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

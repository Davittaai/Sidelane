using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Sidelane.Models;
using Sidelane.Services;

namespace Sidelane;

/// <summary>Private, permissionless first-run workspace chooser. Nothing is launched during discovery.</summary>
public partial class SetupWindow : Window
{
    private readonly SettingsService _settings;
    private IReadOnlyDictionary<string, string> _installed = new Dictionary<string, string>();

    public SetupWindow(SettingsService settings)
    {
        _settings = settings;
        InitializeComponent();
        SourceInitialized += (_, _) => WindowThemeService.RequestDarkTitleBar(new WindowInteropHelper(this).Handle);
        Loaded += async (_, _) => await DetectAsync();
    }

    private async Task DetectAsync()
    {
        try
        {
            var discovered = await Task.Run(LocalAppDiscovery.FindKnownApps);
            if (!IsLoaded) return;
            _installed = discovered;
            if (!discovered.ContainsKey("VS Code"))
            {
                VSCodeCheck.IsChecked = false;
                VSCodeCheck.IsEnabled = false;
                VSCodeCheck.ToolTip = "No VS Code Start Menu shortcut found. Add it manually later in Preferences.";
            }
            InstalledInfo.Text = discovered.Count > 0
                ? "Found local shortcuts: " + string.Join(", ", discovered.Keys.OrderBy(x => x)) +
                    ". These will be used only if you select the option above."
                : "No matching local shortcuts found. AI tools will use the selected browser.";
            var browsers = new[] { "Chrome", "Edge", "Firefox", "Brave" }
                .Where(x => BrowserLocator.Locate(x) is not null).ToList();
            foreach (var browser in browsers) BrowserChoice.Items.Add(new ComboBoxItem { Content = browser });
            BrowserInfo.Text = browsers.Count + " browser" + (browsers.Count == 1 ? "" : "s") + " detected";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            InstalledInfo.Text = "App discovery unavailable. Browser shortcuts will still work.";
            BrowserInfo.Text = "System default";
        }
    }

    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        Validation.Text = "";
        var choices = new[] {
            ("ChatGPT", ChatGPTCheck.IsChecked == true),
            ("Claude", ClaudeCheck.IsChecked == true),
            ("Grok", GrokCheck.IsChecked == true),
            ("Codex", CodexCheck.IsChecked == true),
            ("VS Code", VSCodeCheck.IsChecked == true)
        };
        var selected = choices.Where(x => x.Item2).Select(x => x.Item1).ToHashSet();
        if (selected.Count == 0)
        {
            Validation.Text = "Choose at least one tool, or use defaults.";
            return;
        }
        var browser = (BrowserChoice.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Default";
        var theme = new[] { SageTheme, SlateTheme, SandTheme, IrisTheme }
            .FirstOrDefault(x => x.IsChecked == true)?.Tag?.ToString()?.Split('|');
        var settings = WorkspaceSetupPolicy.Configure(_settings.EditableCopy(), selected, browser,
            PreferAppsCheck.IsChecked == true, _installed,
            theme is { Length: 2 } ? theme[0] : "#B9D8BF",
            theme is { Length: 2 } ? theme[1] : "#191C20",
            TaskbarCheck.IsChecked == true);
        if (settings.Shortcuts.Count == 0)
        {
            Validation.Text = "None of the selected tools has a working launch target. Choose a browser-based tool.";
            return;
        }
        try
        {
            _settings.Replace(settings);
            DialogResult = true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Validation.Text = "Couldn't save your workspace: " + ex.Message;
        }
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var defaults = WorkspaceSetupPolicy.Configure(_settings.EditableCopy(),
                ShortcutSettings.Defaults().Select(x => x.Name),
                "Default", true, _installed, "#B9D8BF", "#191C20", true);
            _settings.Replace(defaults);
            DialogResult = true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Validation.Text = "Couldn't save defaults: " + ex.Message;
        }
    }
}

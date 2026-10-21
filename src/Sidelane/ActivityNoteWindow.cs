using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;

namespace Sidelane;

/// <summary>Keyboard-friendly, modal editor for a non-persistent activity note.</summary>
public sealed class ActivityNoteWindow : Window
{
    private readonly TextBox _input;
    private ActivityNoteWindow(Window owner, string appName, string current)
    {
        Owner = owner;
        Title = "Activity note · " + appName;
        Width = 370;
        Height = 205;
        MinWidth = 320;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        Background = new SolidColorBrush(Color.FromRgb(26, 29, 33));
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");

        var layout = new DockPanel { Margin = new Thickness(20) };
        var label = new TextBlock
        {
            Text = "What is " + appName + " working on?",
            Foreground = new SolidColorBrush(Color.FromRgb(234, 238, 235)),
            FontSize = 13, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        DockPanel.SetDock(label, Dock.Top);
        layout.Children.Add(label);

        var help = new TextBlock
        {
            Text = "Temporary. Clears when Sidelane exits (96 characters max).",
            Foreground = new SolidColorBrush(Color.FromRgb(150, 155, 160)),
            FontSize = 10, Margin = new Thickness(0, 8, 0, 10)
        };
        DockPanel.SetDock(help, Dock.Bottom);
        var controls = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var cancel = new Button { Content = "Cancel", Width = 76, Height = 31, Margin = new Thickness(0, 0, 8, 0) };
        cancel.Click += (_, _) => { DialogResult = false; };
        var save = new Button { Content = "Save note", Width = 95, Height = 31,
            Background = new SolidColorBrush(Color.FromRgb(185, 216, 191)), Foreground = Brushes.Black };
        save.Click += (_, _) => { DialogResult = true; };
        controls.Children.Add(cancel); controls.Children.Add(save);
        DockPanel.SetDock(controls, Dock.Bottom);
        layout.Children.Add(controls);
        layout.Children.Add(help);

        _input = new TextBox
        {
            Text = current, MaxLength = 96, Height = 34, FontSize = 12,
            Padding = new Thickness(9, 5, 9, 5),
            Background = new SolidColorBrush(Color.FromRgb(39, 42, 48)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 75, 82))
        };
        _input.KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter) { DialogResult = true; e.Handled = true; }
            else if (e.Key == System.Windows.Input.Key.Escape) { DialogResult = false; e.Handled = true; }
        };
        layout.Children.Add(_input);
        Content = layout;
        Loaded += (_, _) => { _input.Focus(); _input.SelectAll(); };
    }

    public static string? Prompt(Window owner, string appName, string current)
    {
        var window = new ActivityNoteWindow(owner, appName, current);
        return window.ShowDialog() == true ? window._input.Text.Trim() : null;
    }
}

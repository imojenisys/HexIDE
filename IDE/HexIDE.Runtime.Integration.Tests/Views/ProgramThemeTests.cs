using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.VisualTree;
using HexIDE.IDE;
using HexIDE.Runtime.BuiltinControls;
using HexIDE.Runtime.Components;

namespace HexIDE.Runtime.Integration.Tests.Views;

// Interim appearance policy, hexide-io/HexIDE#909; platform skins (#1137) replace it.
public class ProgramThemeTests
{
    [AvaloniaFact]
    public void Shown_form_and_its_controls_and_popups_stay_light_under_a_dark_application()
    {
        var app = Application.Current!;
        var previous = app.RequestedThemeVariant;
        var form = FormComponentClass.Instance.InstantiateWindow(new ComponentInstance(FormComponentClass.Instance, "Form1"));
        var combo = new VBComboBox { ItemsSource = new[] { "one", "two" }, Width = 150 };
        var scroll = new VBScrollBar();
        var menu = new ContextMenu { ItemsSource = new[] { new MenuItem { Header = "probe" } } };
        combo.ContextMenu = menu;
        form.Content = new StackPanel { Children = { combo, scroll } };
        try
        {
            app.RequestedThemeVariant = ThemeVariant.Dark;
            form.Show();
            form.UpdateLayout();
            combo.IsDropDownOpen = true;
            form.UpdateLayout();
            var popup = combo.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.Should().BeTrue();
            form.ActualThemeVariant.Should().Be(ThemeVariant.Light);
            scroll.ActualThemeVariant.Should().Be(ThemeVariant.Light);
            popup.Child!.ActualThemeVariant.Should().Be(ThemeVariant.Light);
            menu.Open(combo);
            menu.ActualThemeVariant.Should().Be(ThemeVariant.Light);
        }
        finally
        {
            menu.Close();
            combo.IsDropDownOpen = false;
            form.Close();
            app.RequestedThemeVariant = previous;
        }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Standalone_program_boxes_stay_light(bool input, bool visibleOwner)
    {
        var app = Application.Current!;
        var previous = app.RequestedThemeVariant;
        var owner = new Window();
        Window? shown = null;
        using var opened = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            if (window.Content is Runtime.Dialogs.MessageBox or Runtime.Dialogs.InputBox) shown = window;
        });
        try
        {
            app.RequestedThemeVariant = ThemeVariant.Dark;
            if (visibleOwner) owner.Show();
            var library = new StandaloneStandardLib(owner);
            Task pending = input ? library.InputBox("probe", "caption", "")
                : library.MsgBox("probe", "caption", MessageBoxButtons.Ok, MessageBoxIcon.None);
            // Hidden-owner boxes are unowned replacement windows.
            shown.Should().NotBeNull();
            shown!.ActualThemeVariant.Should().Be(ThemeVariant.Light);
            shown.Close();
            await pending;
        }
        finally
        {
            shown?.Close();
            owner.Close();
            app.RequestedThemeVariant = previous;
        }
    }

}

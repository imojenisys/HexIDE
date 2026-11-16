using System.Linq;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;
using HexIDE.VisualDesigner;
using HexIDE.VisualDesigner.Views;

namespace HexIDE.Integration.Tests.Views;

public class DesignerKeyboardNavigationTests
{
    private const string Frm = """
        VERSION 5.00
        Begin VB.Form Form1
           Caption         =   "Form1"
           ClientHeight    =   7200
           ClientLeft      =   60
           ClientTop       =   450
           ClientWidth     =   9600
           Begin VB.CommandButton Command1
              TabIndex = 2
           End
           Begin VB.TextBox Text1
              TabIndex = 0
           End
           Begin VB.CheckBox Check1
              TabIndex = 1
           End
        End
        Attribute VB_Name = "Form1"
        """;

    private sealed class NullSink : IDeserializeErrorSink { public void LogError(string _) { } }

    private static (FormEditViewModel vm, FormEditView view, Window window) Designed()
    {
        var form = new FormDeserializer().Deserialize(new ProjectDefinition(VBProjectType.EXE, "P"), Frm, new NullSink())!;
        var vm = new FormEditViewModel(null!, Substitute.For<IEventBus>(), Substitute.For<HexIDE.Projects.IProjectService>(),
            null!, null!, Substitute.For<ISettingsService>(), Substitute.For<ILocalizationService>(), new Vb6ProjectFileFormat()).Initialize(form);
        var view = new FormEditView { DataContext = vm };
        var window = new Window { Width = 1000, Height = 800, Content = view };
        var app = new App();
        app.LoadMarkup();
        var resources = app.Resources;
        app.Resources = new ResourceDictionary();
        window.Resources.MergedDictionaries.Add(resources);
        window.Show();
        var caption = view.GetVisualDescendants().OfType<HexIDE.Controls.MDICaptionButtons>().Single();
        window.TryFindResource(typeof(HexIDE.Controls.MDICaptionButtons), out var captionTheme).Should().BeTrue();
        caption.Theme = (Avalonia.Styling.ControlTheme)captionTheme!;
        caption.Theme.Should().NotBeNull();
        caption.ApplyStyling();
        caption.ApplyTemplate();
        caption.Template.Should().NotBeNull();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (vm, view, window);
    }

    // vb6.exe SP6 measurements: https://github.com/hexide-io/HexIDE/issues/3399
    [AvaloniaTheory]
    [InlineData(null, Key.Tab, KeyModifiers.None, "Text1")]
    [InlineData("Text1", Key.Tab, KeyModifiers.None, "Check1")]
    [InlineData("Check1", Key.Tab, KeyModifiers.None, "Command1")]
    [InlineData("Command1", Key.Tab, KeyModifiers.None, "Text1")]
    [InlineData("Text1", Key.Tab, KeyModifiers.Shift, "Command1")]
    [InlineData("Command1", Key.Tab, KeyModifiers.Shift, "Check1")]
    [InlineData("Check1", Key.Tab, KeyModifiers.Shift, "Text1")]
    [InlineData("Text1", Key.Escape, KeyModifiers.None, null)]
    [InlineData(null, Key.Tab, KeyModifiers.Shift, "Command1")]
    public void Designer_keys_follow_the_measured_selection_table(string? from, Key key, KeyModifiers modifiers, string? expected)
    {
        var (vm, view, window) = Designed();
        try
        {
            vm.SelectedComponent = from is null ? vm.Form : vm.Components.Single(c => c.Name == from);
            var form = view.FindControl<Control>("FormContainer")!;
            if (from is null)
                form.Focus();
            else
                view.FindControl<HexIDE.Controls.ControlsContainer>("ControlsContainer")!
                    .ContainerFromIndex(vm.Components.IndexOf(vm.SelectedComponent!))!.Focus();
            var physicalKey = key == Key.Tab ? PhysicalKey.Tab : PhysicalKey.Escape;
            var rawModifiers = modifiers == KeyModifiers.Shift ? RawInputModifiers.Shift : RawInputModifiers.None;
            window.KeyPressQwerty(physicalKey, rawModifiers);
            window.KeyReleaseQwerty(physicalKey, rawModifiers);
            Dispatcher.UIThread.RunJobs();
            vm.SelectedComponent.Should().Be(expected is null ? vm.Form : vm.Components.Single(c => c.Name == expected));
            window.FocusManager!.GetFocusedElement().Should().BeSameAs(form);
            vm.IsModified.Should().BeFalse();
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Designer_caption_buttons_are_not_tab_stops()
    {
        var (_, view, window) = Designed();
        try
        {
            var buttons = view.GetVisualDescendants().OfType<HexIDE.Controls.MDICaptionButtons>().Single().GetVisualDescendants()
                .OfType<Button>().ToArray();
            buttons.Should().HaveCount(3);
            buttons.Should().OnlyContain(b => !b.Focusable);
        }
        finally { window.Close(); }
    }
}

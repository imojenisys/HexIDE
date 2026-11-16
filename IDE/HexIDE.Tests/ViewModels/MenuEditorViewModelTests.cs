using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using HexIDE.Events;
using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.Serialization;
using HexIDE.VisualDesigner;

namespace HexIDE.Tests.ViewModels;

// Measured sequence: https://github.com/hexide-io/HexIDE/issues/4141 (2026-11-16 oracle comment).
public class MenuEditorViewModelTests
{
    [Fact]
    public void Empty_editor_selects_a_blank_with_enabled_and_visible_checked()
    {
        var editor = new MenuEditorViewModel();
        editor.FlatMenu.Should().ContainSingle().Which.Should().BeSameAs(editor.SelectedMenu);
        editor.SelectedMenu!.Caption.Should().BeEmpty();
        editor.SelectedMenu.IsEnable.Should().BeTrue();
        editor.SelectedMenu.IsVisible.Should().BeTrue();
        editor.SelectedMenu.IsChecked.Should().BeFalse();
        editor.SelectedMenu.IsWindowsList.Should().BeFalse();
    }

    [Fact]
    public void Next_follows_the_measured_table_and_resets_the_trailing_indent()
    {
        var editor = new MenuEditorViewModel();
        editor.SelectedMenu.Should().NotBeNull();
        editor.SelectedMenu!.Caption = "&File";
        Rows(editor, ["&File", ""], [0, 0], 0);
        editor.Next();
        Rows(editor, ["&File", ""], [0, 0], 1);
        editor.MoveRight();
        Rows(editor, ["&File", ""], [0, 1], 1);
        editor.SelectedMenu!.Caption = "&Open";
        Rows(editor, ["&File", "&Open", ""], [0, 1, 0], 1);
        editor.Next();
        Rows(editor, ["&File", "&Open", ""], [0, 1, 1], 2);
        editor.SelectedMenu!.Caption = "&Save";
        Rows(editor, ["&File", "&Open", "&Save", ""], [0, 1, 1, 0], 2);
        editor.Next();
        Rows(editor, ["&File", "&Open", "&Save", ""], [0, 1, 1, 1], 3);
        editor.Next();
        Rows(editor, ["&File", "&Open", "&Save", ""], [0, 1, 1, 1], 0);
        editor.SelectedMenu = editor.FlatMenu[2];
        editor.MoveLeft();
        editor.Next();
        Rows(editor, ["&File", "&Open", "&Save", ""], [0, 1, 0, 0], 3);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Accepting_a_fresh_editor_writes_only_the_typed_item(bool typeCaption)
    {
        var windows = Substitute.For<IWindowManager>();
        var project = TestHelpers.CreateProjectWithForm("P", "Form1");
        var form = project.Forms.First();
        using var designer = new FormEditViewModel(null!, new EventBus(), Substitute.For<IProjectService>(),
            Substitute.For<IEditorService>(), windows, Substitute.For<ISettingsService>(),
            Substitute.For<ILocalizationService>(), new Vb6ProjectFileFormat()).Initialize(form);
        windows.ShowDialog(Arg.Any<IDialog>()).Returns(call =>
        {
            var editor = (MenuEditorViewModel)call[0];
            var theme = new Avalonia.Themes.Simple.SimpleTheme();
            var icons = new ResourceInclude(new Uri("avares://HexIDE/"))
            {
                Source = new Uri("avares://HexIDE/Themes/IconGeometry.axaml")
            };
            Application.Current!.Styles.Add(theme);
            Application.Current.Resources.MergedDictionaries.Add(icons);
            var view = new MenuEditorView { DataContext = editor };
            var window = new Window { Content = view, Width = 600, Height = 450 };
            try
            {
                window.Show();
                window.UpdateLayout();
                var fields = view.GetVisualDescendants().OfType<TextBox>().ToArray();
                if (typeCaption)
                {
                    fields[0].Focus();
                    window.KeyTextInput("&File");
                    fields[1].Focus();
                    window.KeyTextInput("mnuFile");
                }
                bool? accepted = null;
                editor.CloseRequested += result => accepted = result;
                var ok = view.GetVisualDescendants().OfType<Button>().First();
                ((IInvokeProvider)new ButtonAutomationPeer(ok)).Invoke();
                return Task.FromResult(accepted == true);
            }
            finally
            {
                window.Close();
                Application.Current.Resources.MergedDictionaries.Remove(icons);
                Application.Current.Styles.Remove(theme);
            }
        });

        await designer.EditMenu();

        var menus = form.Components.Where(c => c.BaseClass == MenuComponentClass.Instance).ToArray();
        menus.Should().HaveCount(typeCaption ? 1 : 0);
        var serialized = new FormSerializer().Serialize(form, "Form1.frm").frmText;
        serialized.Split("Begin VB.Menu").Should().HaveCount(typeCaption ? 2 : 1);
        if (typeCaption)
        {
            serialized.Should().Contain("Begin VB.Menu mnuFile");
            menus[0].GetPropertyOrDefault(VBProperties.CaptionProperty).Should().Be("&File");
            menus[0].GetPropertyOrDefault(VBProperties.NameProperty).Should().Be("mnuFile");
        }
    }

    [Theory]
    [InlineData("Delete")]
    [InlineData("MoveUp")]
    [InlineData("MoveDown")]
    public void Trailing_blank_stays_available_after_tree_operations(string operation)
    {
        var editor = new MenuEditorViewModel();
        editor.SelectedMenu.Should().NotBeNull();
        editor.SelectedMenu!.Caption = "File";
        editor.Next();
        switch (operation)
        {
            case "Delete": editor.Delete(); break;
            case "MoveUp": editor.MoveUp(); break;
            case "MoveDown": editor.MoveDown(); break;
        }
        Rows(editor, ["File", ""], [0, 0], 1);
    }

    private static void Rows(MenuEditorViewModel editor, string[] captions, int[] indents, int selected)
    {
        editor.FlatMenu.Select(m => m.Caption).Should().Equal(captions);
        editor.FlatMenu.Select(m => m.Indent).Should().Equal(indents);
        editor.SelectedMenu.Should().BeSameAs(editor.FlatMenu[selected]);
    }
}

using System;
using System.Linq;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HexIDE.Forms.ViewModels.Options;
using HexIDE.Forms.Views.Options;
using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Tools;
using HexIDE.VisualDesigner;

namespace HexIDE.Integration.Tests.Views;

public class TemplatedItemAccessibleNameTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    public void LanguagePage_ItemsAreNamedByDisplayName(int comboIndex)
    {
        var settings = Substitute.For<ISettingsService>();
        settings.ActiveLanguage.Returns("en");
        var languages = Substitute.For<ILanguageSwitchService>();
        languages.AvailableLanguages.Returns([
            new LanguageInfo("system", "System default", "ltr"),
            new LanguageInfo("ar", "Arabic", "rtl"), new LanguageInfo("en", "English", "ltr")]);
        languages.RegionsFor("en").Returns([new RegionInfo("en-GB", "United Kingdom")]);
        var vm = new LanguagePageViewModel(settings, languages, new LocalizationService(), () => { });
        var view = new LanguagePageView { DataContext = vm };
        CheckCombo(view, comboIndex, item => item switch
        {
            LanguageInfo language => language.DisplayName,
            RegionInfo region => region.DisplayName,
            _ => throw new InvalidOperationException(),
        });
    }

    [AvaloniaFact]
    public void MenuEditor_ItemsAreNamedByCaption()
    {
        var vm = new MenuEditorViewModel();
        vm.FlatMenu.Add(new MenuViewModel { Caption = "File" });
        vm.FlatMenu.Add(new MenuViewModel { Caption = "Open", Indent = 1 });
        var view = new MenuEditorView { DataContext = vm };
        var window = new Window { Content = view, Width = 600, Height = 600 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var list = view.GetVisualDescendants().OfType<ListBox>().Single();
            for (var i = 0; i < vm.FlatMenu.Count; i++)
                ControlAutomationPeer.CreatePeerForElement(list.ContainerFromIndex(i)!).GetName()
                    .Should().Be(vm.FlatMenu[i].Caption);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ColorPalette_ItemsAreNamedByColorDescription()
    {
        var vm = new ColorPaletteToolViewModel(Substitute.For<IDocumentDockService>(), new LocalizationService());
        var view = new ColorPaletteToolView { DataContext = vm };
        var window = new Window { Content = view, Width = 400, Height = 600 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var list = view.GetVisualDescendants().OfType<ListBox>().Single();
            for (var i = 0; i < vm.Palette.Count; i++)
                ControlAutomationPeer.CreatePeerForElement(list.ContainerFromIndex(i)!).GetName()
                    .Should().Be(HexIDE.Converters.VBColorDescriptionConverter.Instance.Convert(
                        vm.Palette[i].Color, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture)?.ToString());
        }
        finally { window.Close(); }
    }

    private static void CheckCombo(Control view, int index, Func<object, string> expected)
    {
        var window = new Window { Content = view, Width = 600, Height = 500 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            AssertComboNames(view.GetVisualDescendants().OfType<ComboBox>().ElementAt(index), expected);
        }
        finally { window.Close(); }
    }

    private static void AssertComboNames(ComboBox combo, Func<object, string> expected)
    {
        combo.ItemCount.Should().BeGreaterThan(0);
        for (var i = 0; i < combo.ItemCount; i++)
        {
            combo.SelectedIndex = i;
            combo.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            var container = combo.ContainerFromIndex(i);
            container.Should().NotBeNull();
            ControlAutomationPeer.CreatePeerForElement(container!).GetName().Should().Be(expected(combo.Items[i]!));
            combo.IsDropDownOpen = false;
        }
    }
}

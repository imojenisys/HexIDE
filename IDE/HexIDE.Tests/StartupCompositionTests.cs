namespace HexIDE.Tests;

public class StartupCompositionTests : CompositionTestBase
{
    [AvaloniaFact]
    public void DesignerFactoryUsesRuntimeConstructorAndSharedToolbox()
    {
        _ = Composition.Root;
        var factory = typeof(EditorService).GetField("formEditViewModelFactory",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(Composition.EditorService).Should().BeOfType<Func<HexIDE.VisualDesigner.FormEditViewModel>>().Subject;

        using var first = factory();
        using var second = factory();
        first.Should().NotBeSameAs(second);
        first.ToolsBoxToolViewModel.Should().BeSameAs(Composition.ToolBoxViewModel);
        second.ToolsBoxToolViewModel.Should().BeSameAs(Composition.ToolBoxViewModel);
        first.UndoStack.Should().NotBeNull();
        second.UndoStack.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void ResolvesEveryStartupRootInApplicationOrder()
    {
        // Keep the order in App.OnFrameworkInitializationCompleted: a different first root can hide a cycle.
        Composition.ThemeService.Should().NotBeNull();
        Composition.SettingsService.Should().NotBeNull();
        Composition.KeymapService.Should().NotBeNull();
        Composition.LocalizationService.Should().NotBeNull();
        Composition.Root.Should().NotBeNull();
        Composition.LanguageServerMessages.Should().NotBeNull();
        Composition.LspClient.Should().NotBeNull();
        try
        {
            Composition.FileWatcherService.Should().NotBeNull();
            Composition.Recovery.Should().NotBeNull();
            Composition.WindowStateService.Should().NotBeNull();

            // DesktopStartupHook also resolves the add-in host and, in Debug, the server context.
            Composition.AddinLoader.Should().NotBeNull();
            Composition.HexIdeHost.Should().NotBeNull();
            Composition.ProjectManager.Should().NotBeNull();
            Composition.DocumentDockService.Should().NotBeNull();
            Composition.EditorService.Should().NotBeNull();
            Composition.ProjectRunnerService.Should().NotBeNull();
            Composition.ProjectService.Should().NotBeNull();
            Composition.EventBus.Should().NotBeNull();
            Composition.BookmarkService.Should().NotBeNull();
            Composition.BreakpointService.Should().NotBeNull();
            Composition.DebugController.Should().NotBeNull();
            Composition.ToolBoxViewModel.Should().NotBeNull();
            Composition.ProfileService.Should().NotBeNull();
            Composition.AddinProjectTemplateService.Should().NotBeNull();
            Composition.LanguageSwitchService.Should().NotBeNull();
            Composition.Capture.Should().NotBeNull();
            Composition.Pseudonyms.Should().NotBeNull();
            Composition.ProjectFileFormat.Should().NotBeNull();
            Composition.ReadOnlyRegionReader.Should().NotBeNull();
            Composition.ReadOnlyRegionRules.Should().NotBeNull();
            Composition.ArchiveProjects.Should().NotBeNull();
        }
        finally
        {
            Composition.AddinLoader.Dispose();
            Composition.FileWatcherService.Dispose();
        }
    }
}

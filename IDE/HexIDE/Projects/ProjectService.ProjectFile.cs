using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HexIDE.IDE;

namespace HexIDE.Projects;

public partial class ProjectService
{
    // The save transaction for the future editable .vbp tab. Kept internal until the tab's dirty/close
    // protection is connected: exposing editing before then would allow a project save to overwrite it.
    // False means refusal or Reload; Cancel throws, as the other save paths do.
    internal Task<bool> ApplyProjectFileText(ProjectDefinition project, string text, byte[] originalBytes,
        Func<Task<bool>> confirmReplacingModel) => WithSaveRefusals(async () =>
    {
        if (project.FormFormat is not null || project.ProjectFileWriter is not null
            || ProjectFileSource.ChosenPath(project) is not { } path
            || !path.EndsWith(".vbp", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only a saved VB6 project file can apply project text.");

        var errors = new DeserializeErrorSink(localization);
        var proposed = await format.ReadProjectTextAsync(path, text, project.TextEncoding, HostAnsi, errors);
        if (errors.Errors.Count > 0)
        {
            await windowManager.MessageBox(localization.GetString("Str.ProjectLoad.Heading") + "\n"
                + string.Join("\n", errors.Errors), icon: MessageBoxIcon.Warning);
            return false;
        }

        if (DiffersFromRead(path, originalBytes) && await ChooseProjectFileReload(path))
        {
            await ReopenFromDisk(project, path);
            return false;
        }

        eventBus.Publish(new HexIDE.Events.ApplyAllUnsavedChangesEvent());
        if (IsDirty(project) && !await confirmReplacingModel())
            throw new OperationCanceledException();

        // Reopen asks about the members before this callback can write. In particular, Cancel there
        // must not leave a new project file on disk beside the old, still-open project model.
        await ReopenFromDisk(project, path, () => AtomicWriteText(path, text, proposed.ProjectFileBytes));
        return true;
    });

    private static bool DiffersFromRead(string path, byte[] originalBytes)
    {
        try { return !File.ReadAllBytes(path).AsSpan().SequenceEqual(originalBytes); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return true; }
    }

    /// <summary>
    /// Renders a project's own file. Every render of one goes through here (#3698): its type's writer where it has one, and
    /// VB6's format otherwise. <c>ContributedProjectFileTests</c> fails the build if VB6's format renders a project file
    /// anywhere else, so a project an add-in opened never has its file written as a <c>.vbp</c>.
    /// </summary>
    /// <remarks>
    /// A writer that failed when the project opened left no baseline to compare with, so its file isn't written that session:
    /// each render after that is the same refusal, with the add-in's words.
    /// </remarks>
    private string RenderProjectFile(ProjectDefinition project, string path, Func<object, string?>? pathOf = null,
        bool includeRelatedDocuments = true)
    {
        if (project.ProjectFileWriter is not { } writer)
            return format.RenderProject(project, path, pathOf, includeRelatedDocuments);
        if (projectFilesRefusedAtOpen.TryGetValue(project, out var refusal))
            throw refusal;
        return writer.RenderProject(project, path, pathOf);
    }

    private readonly ConditionalWeakTable<ProjectDefinition, RenderRefusal> projectFilesRefusedAtOpen = new();
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaEdit.Document;
using HexIDE.Bookmarks;
using HexIDE.Controls;
using HexIDE.Events;
using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Lsp;
using HexIDE.Lsp.Messages;
using HexIDE.Projects;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;
using HexIDE.Utils;
using PropertyChanged.SourceGenerator;
using R3;
using Serilog;

namespace HexIDE.Forms.ViewModels;

public partial class CodeEditorViewModel : BaseEditorWindowViewModel, ISearchableDocument
{
    private readonly IWindowManager windowManager;
    private readonly HexIDE.Debugging.IRunScope runScope;
    private readonly IEditorService editorService;
    private readonly IProjectService projectService;
    private readonly IEventBus eventBus;
    private readonly ILspClient lspClient;
    private readonly ISettingsService settingsService;
    private readonly IStatusBarService statusBarService;
    private readonly IBookmarkService bookmarkService;
    private readonly HexIDE.Debugging.IBreakpointService breakpointService;
    private readonly HexIDE.Runtime.Debugging.IDebugController debugController;
    private readonly ILocalizationService localization;
    protected override string ComputeTitle()
    {
        var code = localization.GetString("Str.Document.CodeSuffix");
        return moduleDefinition is not null
            ? $"{moduleDefinition.Owner.Name} - {moduleDefinition.Name} ({code})"
            : $"{formDefinition?.Owner.Name} - {formDefinition?.Name} ({code})";
    }
    public override object? Icon { get; } = HexIDE.Utils.IconFactory.Themed("Geo.Code");

    private TextDocument document = new TextDocument();
    private FormDefinition? formDefinition;
    private ModuleDefinition? moduleDefinition;

    public TextDocument Document => document;

    [Notify] private int caretOffset;
    [Notify] private int selectionStart;
    [Notify] private int selectionLength;

    public event Action? FocusWindowRequest;
    public event Action<IReadOnlyList<LspMarker>>? MarkersChanged;

    /// <summary>
    /// A language server for this document came up, or went away. Raised on the UI thread.
    /// </summary>
    /// <remarks>
    /// <b>Exists for folding, which has no other signal.</b> The view asks for folding ranges when it
    /// attaches, and on the first document of a session that is before any server has answered
    /// <c>initialize</c> — the registry then has no started claimant advertising the capability, answers
    /// with an empty set, and nothing asks again until the text changes. So a module opened and read
    /// without being typed into never folded at all (hexide-io/HexIDE#446).
    ///
    /// <para>
    /// Deliberately the server's state rather than a published diagnostic, which is what the carried-file
    /// editor keys on. A server may advertise <c>foldingRangeProvider</c> and publish nothing — it is
    /// entitled to — and on that server a publish would never arrive to hang the retry on.
    /// </para>
    /// </remarks>
    public event Action? LanguageServerStateChanged;

    /// <summary>Held so the subscription can be dropped, and so it is only ever taken once.</summary>
    private EventHandler? languageServerStateHandler;

    /// <summary>
    /// The most recent diagnostics, kept so a view attaching later can catch up.
    ///
    /// <para>
    /// <see cref="MarkersChanged"/> is a notification, not a state: a view that subscribes after the last
    /// publication sees nothing until the next one. That is not hypothetical — moving this document to
    /// another dock re-materialises the view, and neither source of diagnostics has any reason to publish
    /// again for a document that has not changed, so the squiggles would simply not come back
    /// (hexide-io/HexIDE#270).
    /// </para>
    ///
    /// <para>
    /// It matters more here than in the carried-file editor, because a second source feeds this channel:
    /// compiling with the real VB6 toolchain injects its errors through it, and a build is a much rarer
    /// event to have to wait for again than a keystroke.
    /// </para>
    /// </summary>
    public IReadOnlyList<LspMarker> Markers { get; private set; } = [];

    public IEventBus EventBus => eventBus;
    public ISettingsService Settings => settingsService;
    public IStatusBarService StatusBar => statusBarService;
    public IBookmarkService BookmarkService => bookmarkService;
    public HexIDE.Debugging.IBreakpointService BreakpointService => breakpointService;
    public HexIDE.Runtime.Debugging.IDebugController DebugController => debugController;

    /// <summary>True while the project is running OR paused in the debugger — the window in which a code edit
    /// triggers the VB6 "reset your project?" prompt (the interpreter can't hot-patch a running program). Read off
    /// the debug controller (active between its run-start Reset and its run-end Stop) rather than
    /// IProjectRunnerService, which would close a DI cycle via the editor factory.</summary>
    public bool IsProjectRunning => debugController.IsSessionActive;

    /// <summary>VB6-faithful Edit-and-Continue affordance: the interpreter can't apply an edit to a running program
    /// live, so editing while running/paused pops VB6's own reset prompt. Yes → request a project reset (the edit
    /// then stands); No → the edit is left cancelled and the run continues. Returns true if a reset was requested.
    /// The reset goes through the event bus (ProjectRunnerService handles EndProjectRequestedEvent) to avoid a
    /// direct dependency on the runner, which would cycle.</summary>
    public async Task<bool> ConfirmResetWhileRunningAsync()
    {
        var result = await windowManager.MessageBox(
            localization.GetString("Str.ProjectRunner.EditWhileRunningConfirm"),
            buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning);
        if (result != MessageBoxResult.Yes)
            return false;
        eventBus.Publish(new EndProjectRequestedEvent());
        return true;
    }
    public FormDefinition? FormDefinition => formDefinition;
    public ModuleDefinition? ModuleDefinition => moduleDefinition;

    /// <summary>
    /// What this editor's document <em>is</em> — the key everything per-document hangs on.
    /// </summary>
    /// <remarks>
    /// Fixed when the editor is initialized and never recomputed, because the definition it names never
    /// changes: a rename, a first save or a Save As all leave the same object in place. The gutters are built
    /// from this and the F9 and Ctrl+F2 commands read the same value, which is what stops the two
    /// disagreeing after a rename (#269).
    /// </remarks>
    public DocumentIdentity Identity =>
        OpenDocument ?? throw new InvalidOperationException(
            "A code editor has no document until Initialize has been called with a form or a module.");

    /// <inheritdoc/>
    public override DocumentIdentity? OpenDocument => identity;

    private DocumentIdentity? identity;

    /// <summary>The project currently running, or null. Read live, never captured.</summary>
    public ProjectDefinition? RunningProject => runScope.RunningProject;

    /// <summary>
    /// True when the underlying file cannot be written back faithfully, so a code edit would be discarded
    /// at save time.
    ///
    /// This covers a form's *code*, not just its layout: the code lives inside the .frm, so refusing to
    /// save the form discards code edits too. Gating only the designer would leave the more likely loss
    /// — someone typing a procedure — completely unprotected.
    /// </summary>
    public bool IsReadOnly => formDefinition is { CanSaveFaithfully: false };

    public string? ReadOnlyReason => formDefinition?.UnfaithfulSaveReason;

    public ObservableCollection<string> ObjectNames    { get; } = new();
    public ObservableCollection<string> ProcedureNames { get; } = new();

    [Notify] private string? selectedObject;
    [Notify] private string? selectedProcedure;

    private const string GeneralObject   = "(General)";
    private const string DeclarationsProc = "(Declarations)";

    private DocumentSymbol[]? _symbols;

    /// <summary>
    /// This document's conversation with the language layer, shared with the carried-file editor.
    ///
    /// <para>
    /// Created in <see cref="Initialize(FormDefinition)"/> rather than the constructor, because the URI it
    /// is named by is not knowable until a definition has been supplied — a constructor-time session would
    /// name every editor <c>vb6://form/untitled</c>.
    /// </para>
    /// </summary>
    private LspDocumentSession? session;

    public CodeEditorViewModel(IWindowManager windowManager,
        IEditorService editorService,
        IProjectService projectService,
        IEventBus eventBus,
        ILspClient lspClient,
        ISettingsService settingsService,
        IStatusBarService statusBarService,
        IBookmarkService bookmarkService,
        HexIDE.Debugging.IBreakpointService breakpointService,
        HexIDE.Runtime.Debugging.IDebugController debugController,
        HexIDE.Debugging.IRunScope runScope,
        ILocalizationService localization)
    {
        this.windowManager = windowManager;
        this.editorService = editorService;
        this.projectService = projectService;
        this.eventBus = eventBus;
        this.lspClient = lspClient;
        this.settingsService = settingsService;
        this.bookmarkService = bookmarkService;
        this.breakpointService = breakpointService;
        this.debugController = debugController;
        this.runScope = runScope;
        this.statusBarService = statusBarService;
        this.localization = localization;

        // Refresh the tab title (its "(Code)" suffix is localized) when the language changes. Unsubscribe on Dispose
        // (tab close) — a raw `+=` kept every closed code-editor VM (each holding a full document buffer) reachable
        // from the singleton localization service, so none were collected and a language switch replayed on all of them.
        Action onLanguageChanged = () => Title = ComputeTitle();
        localization.LanguageChanged += onLanguageChanged;
        AutoDispose(new ActionDisposable(() => localization.LanguageChanged -= onLanguageChanged));

        AutoDispose(this.eventBus.Subscribe<CreateOrNavigateToSubEvent>(e =>
        {
            if (e.Form == formDefinition)
            {
                Log.Debug("CodeEditorViewModel: Handling CreateOrNavigateToSubEvent({SubName}) in {FormName}",
                    e.Sub, formDefinition?.Name);
                var sub = Document.IndexOf($"Sub {e.Sub}", 0, Document.TextLength, StringComparison.OrdinalIgnoreCase);
                if (sub != -1)
                {
                    Log.Debug("CodeEditorViewModel: Found 'Sub {SubName}' at offset {Offset}, navigating", e.Sub, sub);
                    var nextNewLineIndex = Document.IndexOf("\n", sub, Document.TextLength - sub, StringComparison.OrdinalIgnoreCase);
                    CaretOffset = nextNewLineIndex == -1 ? sub : nextNewLineIndex + 1;
                }
                else
                {
                    Log.Debug("CodeEditorViewModel: 'Sub {SubName}' not found, creating event stub", e.Sub);
                    AddProcedureViewModel vm = new AddProcedureViewModel();
                    vm.IsPublic = true;
                    vm.IsSub = true;
                    vm.Name = e.Sub;
                    var code = vm.GenerateCode();
                    InsertAtEnd(code.beginCode, code.endCode);
                }
                FocusWindowRequest?.Invoke();
            }
        }));
        AutoDispose(this.eventBus.Subscribe<ApplyAllUnsavedChangesEvent>(e =>
        {
            // Marshalled, because failing here is INVISIBLE and corrupting. Document.Text throws
            // "Call from invalid thread" off the UI thread, EventBus logs the exception and moves to the
            // next handler, and the save that published this event then serializes the model's previous
            // code as though the editor had been flushed. The caller is told the file was written; it was,
            // with the wrong contents. An MCP write tool did exactly this. (#334)
            //
            // Invoke rather than Post: the publisher writes the model to disk on the very next statement,
            // so a flush that has merely been queued is a flush that did not happen. Inlines when already
            // on the UI thread, which every in-app caller is.
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                Flush();
            else
                Avalonia.Threading.Dispatcher.UIThread.Invoke(Flush);

            void Flush()
            {
                // Split at the prefix, not the whole buffer: the model keeps holding the code section, and
                // writing the composed text back would put the designer block into Code, where the
                // interpreter compiles it as VB.
                var body = BufferBody;
                formDefinition?.UpdateCode(body);
                if (moduleDefinition is not null)
                    moduleDefinition.UpdateCode(body);
            }
        }));
        AutoDispose(this.eventBus.Subscribe<DocumentSavedEvent>(e =>
        {
            // Matched by reference against this editor's own document, as every other event here is.
            //
            // EITHER half matches, and that is safe rather than a duplication risk. A UserControl or
            // PropertyPage is one file whose two halves are both set by Initialize(ModuleDefinition)
            // (#152), so the event names both — but a save publishes ONE event, this handler runs once,
            // and the session's URI is fixed, so both halves matching produces exactly one announcement
            // under exactly one URI. Ordering the checks to prefer the module would read as though it
            // prevented something; it prevents nothing, and mutation testing says so.

            var mine = (e.Module is not null && ReferenceEquals(e.Module, moduleDefinition))
                    || (e.Form is not null && ReferenceEquals(e.Form, formDefinition));

            // A save is also the commonest way a document's NAME changes: a first save gives it a file,
            // and saving a project into another directory repoints every one of them. So reconcile the
            // session's name before announcing the save, and announce the save under the new name -- the
            // other order tells a server about a write to a document it has just been told to forget.
            if (mine && session is { } open) ReconcileThenAnnounceSaveAsync(open).ListenErrors();
        }));

        AutoDispose(this.eventBus.Subscribe<FormUnloadedEvent>(e =>
        {
            if (e.Form == formDefinition)
                RequestClose();
        }));
        AutoDispose(new ActionDisposable(() =>
        {
            var body = BufferBody;   // the same split as the flush above, for the same reason
            formDefinition?.UpdateCode(body);
            if (moduleDefinition is not null)
                moduleDefinition.UpdateCode(body);
            // Disposed from HERE rather than AutoDispose'd from Initialize. Dispose walks its
            // disposables in REVERSE registration order, so a session registered later would close the
            // document BEFORE the buffer above was written back to the definition.
            session?.Dispose();
        }));
    }

    /// <summary>
    /// The text this buffer carries in front of the document's code, and splits at on flush.
    /// </summary>
    /// <remarks>
    /// <b>Stored rather than recomputed, because the buffer's prefix and the model's diverge.</b> Renaming
    /// a module changes the header the model would render — <c>Attribute VB_Name = "Utilities"</c> is
    /// longer than <c>= "Mod1"</c> — while the buffer still holds the old one until something replaces
    /// both together. Splitting at the model's current length would then cut into the body.
    /// </remarks>
    private string bufferPrefix = "";

    /// <summary>
    /// The document's code, without the header the buffer shows in front of it.
    /// </summary>
    /// <remarks>
    /// <b>Every reader that wants "the code" goes through this, and every writer through
    /// <see cref="ReplaceBody"/>.</b> Since #273 task 3.2 the buffer is the whole file, so the two are no
    /// longer the same string, and a caller that assigns <c>Document.Text</c> a bare body would silently
    /// destroy the header — the next flush splits at the prefix's length and would take the first lines of
    /// the body with it.
    /// </remarks>
    public string BufferBody => FormCodeText.BodyOf(Document.Text, bufferPrefix);

    /// <summary>Replaces the code, leaving the header the buffer shows in front of it untouched.</summary>
    public void ReplaceBody(string body) => Document.Text = bufferPrefix + body;

    public CodeEditorViewModel Initialize(FormDefinition formElement)
    {
        this.formDefinition = formElement;
        this.identity = DocumentIdentity.For(formElement);
        // Both subscriptions also reconcile the wire name. A document with no file is named
        // untitled:<Project>/<Name>, so BOTH its own rename and its project's change what servers should
        // call it, with no save involved at all -- and after #489 that is the ordinary state of a new
        // document rather than a corner case. A document that HAS a file is unaffected: its name is its
        // path, RenameAsync compares before acting, and a no-op rename costs one comparison.
        AutoDispose(formElement.ObservePropertyChanged(x => x.Name)
            .Subscribe(_ => { Title = ComputeTitle(); ReconcileWireName(); }));
        AutoDispose(formElement.Owner.ObservePropertyChanged(x => x.Name)
            .Subscribe(_ => { Title = ComputeTitle(); ReconcileWireName(); }));
        // The whole file, not the code section (#273 task 3.2). One line number now means the same
        // thing to the editor, a language server, the interpreter and the debugger.
        bufferPrefix = FormCodeText.Prefix(formElement);
        Document.Text = bufferPrefix + formElement.Code;

        PopulateObjectNames();

        OpenToLanguageLayer();

        Title = ComputeTitle();
        return this;
    }

    public CodeEditorViewModel Initialize(ModuleDefinition moduleElement)
    {
        this.moduleDefinition = moduleElement;
        // BOTH halves, because a UserControl or PropertyPage HAS both and they are one file.
        //
        // This used to set only the module, while the designer's View Code went through
        // Initialize(FormDefinition) and set only the form part. Two tabs could then stand open over one
        // .ctl, each flushing to its own buffer — and the two save paths read different ones (SaveModule
        // serializes module.Code, SerializeFormToFile serializes formPart.Code). Code typed in one tab was
        // written by neither the other's save nor reported by IsDirty, which reads module.Code. It went
        // silently missing. (#152)
        //
        // Null for a .bas or .cls, which have no designer half — so everything downstream that tests
        // formDefinition stays correct for them without a kind check.
        this.formDefinition = moduleElement.FormPart;
        this.identity = DocumentIdentity.For(moduleElement);
        // Both subscriptions also reconcile the wire name. A document with no file is named
        // untitled:<Project>/<Name>, so BOTH its own rename and its project's change what servers should
        // call it, with no save involved at all -- and after #489 that is the ordinary state of a new
        // document rather than a corner case. A document that HAS a file is unaffected: its name is its
        // path, RenameAsync compares before acting, and a no-op rename costs one comparison.
        AutoDispose(moduleElement.ObservePropertyChanged(x => x.Name)
            .Subscribe(_ => { Title = ComputeTitle(); ReconcileWireName(); }));
        AutoDispose(moduleElement.Owner.ObservePropertyChanged(x => x.Name)
            .Subscribe(_ => { Title = ComputeTitle(); ReconcileWireName(); }));
        // As above. For a .ctl/.pag the prefix comes from the FormPart's designer half, and the body is
        // the MODULE's code -- which is the pairing the save path writes for those kinds too.
        bufferPrefix = FormCodeText.Prefix(moduleElement);
        Document.Text = bufferPrefix + moduleElement.Code;

        // A module with a designer half lists its controls like a form's editor does; one without falls
        // back to "(General)" alone, which is what this used to hardcode.
        PopulateObjectNames();

        OpenToLanguageLayer();

        Title = ComputeTitle();
        return this;
    }

    /// <summary>
    /// Hands this document to the language layer.
    ///
    /// <para>
    /// Called from <c>Initialize</c>, after the definition is assigned and after <c>Document.Text</c> is
    /// loaded, and from nowhere else. Both are load-bearing: the URI is not knowable before the first, and
    /// starting before the second would open the document with an empty buffer <em>and</em> turn the load
    /// assignment itself into a spurious change notification, because the session hooks
    /// <c>TextChanged</c> as it starts.
    /// </para>
    /// </summary>
    private void OpenToLanguageLayer()
    {
        // GetDocumentUri(), not a form-or-module expression written out again: a UserControl or
        // PropertyPage sets BOTH definition fields (#152) and the module must win. One rule, one place.
        // isProjectMember: true -- this window opens forms, modules, classes, UserControls and
        // PropertyPages, and every one of them is a member. That is what gates a `.cls` away from a LaTeX
        // server claiming the same extension (#279); the carried-file editor states the opposite.
        session = new LspDocumentSession(
            lspClient, Document, DocumentWireName.For(Identity), isProjectMember: true);

        // Forwarded into this class's own event rather than re-exposed as a pass-through. The view
        // subscribes once when it attaches and never replays, so a subscription that landed on the session
        // object instead would die with it — silently, and permanently blank.
        session.MarkersChanged += markers =>
        {
            Markers = markers;
            MarkersChanged?.Invoke(markers);
        };

        // The same piggyback as before: a fresh diagnostic set means the server has evidently just re-read
        // the document, which is the cheapest signal that its symbols are worth asking for again.
        session.DiagnosticsApplied += () => _ = RefreshSymbolsAsync();

        session.Start();

        if (languageServerStateHandler is null)
        {
            // Posted rather than raised inline: this arrives on whichever thread the connection is
            // running on, and every subscriber is a view.
            languageServerStateHandler = (_, _) =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() => LanguageServerStateChanged?.Invoke());
            lspClient.StateChanged += languageServerStateHandler;
            AutoDispose(new ActionDisposable(() =>
            {
                if (languageServerStateHandler is { } handler) lspClient.StateChanged -= handler;
            }));
        }
    }

    /// <summary>
    /// Replaces the editor buffer with <paramref name="newCode"/> after the file watcher reloaded the
    /// underlying file from disk. Preserves the caret position best-effort. The <c>Document.Text</c>
    /// assignment raises <c>TextChanged</c>, which debounces a didChange to the LSP server so diagnostics
    /// refresh — no explicit LSP call is needed. Must be called on the UI thread.
    /// </summary>
    internal void ReloadFrom(string newCode)
    {
        if (string.Equals(Document.Text, newCode, StringComparison.Ordinal))
            return;
        var caret = CaretOffset;
        Document.Text = newCode;
        CaretOffset = Math.Clamp(caret, 0, Document.TextLength);
    }

    private void PopulateObjectNames()
    {
        ObjectNames.Clear();
        ObjectNames.Add(GeneralObject);
        // Selected before the early return, not after the loop: a .bas or .cls has no designer half and
        // takes that return, and it still needs "(General)" selected rather than left null.
        SelectedObject = GeneralObject;
        if (formDefinition is null) return;
        foreach (var component in formDefinition.Components)
        {
            var name = component.GetPropertyOrDefault(VBProperties.NameProperty);
            if (name is { Length: > 0 })
                ObjectNames.Add(name);
        }
    }

    /// <summary>
    /// Brings the session's name back into agreement with the document, then tells servers it was saved.
    /// </summary>
    /// <remarks>
    /// Compared against <c>DocumentWireName.For(Identity)</c> rather than against anything on the event.
    /// The event matches EITHER half of a UserControl, and the code window's Save repoints the form part's
    /// path alone (#474) while the identity and the project file both use the module's — so reading the
    /// path off the event would re-open a UserControl under the wrong name, and only sometimes.
    /// </remarks>
    /// <summary>
    /// Re-announces this document under its current name, if that has changed. Does nothing otherwise.
    /// </summary>
    private void ReconcileWireName()
    {
        if (session is { } open) open.RenameAsync(DocumentWireName.For(Identity)).ListenErrors();
    }

    private async Task ReconcileThenAnnounceSaveAsync(LspDocumentSession open)
    {
        await open.RenameAsync(DocumentWireName.For(Identity));
        await open.NotifySavedAsync();
    }

    /// <summary>
    /// Cancels any pending debounce and immediately syncs the current document text to the LSP server.
    /// Call this before requests that depend on the server having up-to-date source (e.g. signatureHelp).
    /// </summary>
    internal Task FlushDocumentAsync(CancellationToken ct = default)
        => session?.FlushAsync(ct) ?? Task.CompletedTask;

    private async Task RefreshSymbolsAsync()
    {
        if (!lspClient.IsRunning) return;
        if (LiveDocumentUri is not { } uri) return;
        var symbols = await lspClient.RequestDocumentSymbolsAsync(uri);
        _symbols = symbols;
        Avalonia.Threading.Dispatcher.UIThread.Post(() => RefreshProcedureNames());
    }

    private void RefreshProcedureNames()
    {
        var current = SelectedProcedure;
        ProcedureNames.Clear();

        if (SelectedObject == GeneralObject || SelectedObject is null)
        {
            ProcedureNames.Add(DeclarationsProc);
            foreach (var s in FlattenedSymbols())
                if (s.Kind is SymbolKind.Method or SymbolKind.Function or SymbolKind.Property)
                    ProcedureNames.Add(s.Name);
        }
        else
        {
            // Show events for the selected form/control
            var component = FindComponent(SelectedObject);
            if (component is not null)
                foreach (var ev in component.BaseClass.Events)
                    ProcedureNames.Add(ev.Name);
        }

        SelectedProcedure = (current is not null && ProcedureNames.IndexOf(current) >= 0) ? current : null;
    }

    private ComponentInstance? FindComponent(string name)
    {
        if (formDefinition is null) return null;
        foreach (var c in formDefinition.Components)
            if (c.GetPropertyOrDefault(VBProperties.NameProperty) == name)
                return c;
        return null;
    }

    private void OnSelectedObjectChanged(string? oldValue, string? newValue)
    {
        RefreshProcedureNames();
    }

    /// <summary>
    /// Every symbol the server reported, nesting included.
    /// </summary>
    /// <remarks>
    /// <c>textDocument/documentSymbol</c> answers with a tree, and servers differ on how deep they make it.
    /// The bundled VB6 server returns procedures at the top level; a server that reports the module as one
    /// symbol containing its procedures is equally conformant, and reading only the top level of that
    /// answer yields a procedure dropdown with the module's name in it and nothing else.
    /// </remarks>
    private IEnumerable<DocumentSymbol> FlattenedSymbols() =>
        _symbols is null ? [] : _symbols.SelectMany(s => s.Flatten());

    /// <summary>
    /// The name this document's LIVE session is known by, or null when no session is open.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Read off the session, not minted per request.</b> It used to call
    /// <c>DocumentWireName.For(Identity)</c> on every request, which reads the path live — so from the
    /// moment a document was first saved, every lifecycle notification still said <c>untitled:</c> while
    /// every request said <c>file:</c>. The bundled server answers an unknown URI with an empty array and
    /// no error, so hover, completion, folding, Go To Definition, rename and formatting simply went quiet,
    /// and the procedure dropdown emptied on the next diagnostics tick. Nothing logged, nothing thrown.
    /// </para>
    /// <para>
    /// Null when nothing is open, and the callers below answer emptily rather than asking. A request naming
    /// a document no server was told about is not a question with a wrong answer; it is a question nobody
    /// can be expected to have an answer to, and sending it invites exactly the silence above.
    /// </para>
    /// <para>
    /// Make EXE is why this must not be recomputed even when a session IS open: it repoints every
    /// document's path into a temporary folder and puts it back afterwards, so a name derived from the path
    /// mid-build would name a document the server never opened, twice.
    /// </para>
    /// </remarks>
    private string? LiveDocumentUri => session is { IsOpen: true } open ? open.Uri : null;

    public void SaveForm() => SaveWithFormattingAsync(() => projectService.SaveForm(formDefinition!, false)).ListenErrors();
    public void SaveModule() => SaveWithFormattingAsync(() => projectService.SaveModule(moduleDefinition!, false)).ListenErrors();

    private async Task SaveWithFormattingAsync(Func<Task> saveAction)
    {
        if (settingsService.FormatOnSave)
            await ApplyFormattingToDocumentAsync();
        await saveAction();
    }

    /// <summary>
    /// Requests formatting from the LSP server and applies edits to the document.
    /// Called automatically before save (keyword casing + indentation).
    /// </summary>
    private async Task ApplyFormattingToDocumentAsync()
    {
        if (!lspClient.IsRunning) return;
        try
        {
            await FlushDocumentAsync();
            var edits = await RequestFormattingAsync();
            if (edits.Length == 0) return;

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Sort edits in reverse order to preserve offsets
                var sorted = new List<TextEdit>(edits);
                sorted.Sort((a, b) =>
                {
                    int cmp = b.Range.Start.Line.CompareTo(a.Range.Start.Line);
                    return cmp != 0 ? cmp : b.Range.Start.Character.CompareTo(a.Range.Start.Character);
                });

                var doc = Document;
                doc.BeginUpdate();
                try
                {
                    foreach (var te in sorted)
                    {
                        var startLine = doc.GetLineByNumber(te.Range.Start.Line + 1);
                        var endLine   = doc.GetLineByNumber(te.Range.End.Line + 1);
                        int startOff  = Math.Min(startLine.Offset + te.Range.Start.Character, startLine.EndOffset);
                        int endOff    = Math.Min(endLine.Offset + te.Range.End.Character, endLine.EndOffset);
                        doc.Replace(startOff, endOff - startOff, te.NewText);
                    }
                }
                finally
                {
                    doc.EndUpdate();
                }
            });
        }
        catch (Exception ex)
        {
            Log.Debug("[save-format] {ErrorMessage}", ex.Message);
        }
    }

    public Task<HoverResult?> RequestHoverAsync(Position position, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestHoverAsync(uri, position, ct) : Task.FromResult<HoverResult?>(null);

    public Task<FoldingRange[]> RequestFoldingRangesAsync(CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestFoldingRangesAsync(uri, ct) : Task.FromResult<FoldingRange[]>([]);

    public Task<CompletionItem[]> RequestCompletionAsync(Position position, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestCompletionAsync(uri, position, ct) : Task.FromResult<CompletionItem[]>([]);

    public Task<SignatureHelp?> RequestSignatureHelpAsync(Position position, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestSignatureHelpAsync(uri, position, ct) : Task.FromResult<SignatureHelp?>(null);

    public Task<Location[]?> RequestDefinitionAsync(Position position, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestDefinitionAsync(uri, position, ct) : Task.FromResult<Location[]?>(null);

    public Task<Location[]?> RequestDeclarationAsync(Position position, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestDeclarationAsync(uri, position, ct) : Task.FromResult<Location[]?>(null);

    public Task<DocumentHighlight[]?> RequestDocumentHighlightAsync(Position position, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestDocumentHighlightAsync(uri, position, ct) : Task.FromResult<DocumentHighlight[]?>(null);

    public Task<WorkspaceEdit?> RequestRenameAsync(Position position, string newName, CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestRenameAsync(uri, position, newName, ct) : Task.FromResult<WorkspaceEdit?>(null);

    public Task<string?> ShowInputBoxAsync(string prompt, string title, string defaultText)
        => windowManager.InputBox(prompt, title, defaultText);

    public Task<TextEdit[]> RequestFormattingAsync(CancellationToken ct = default)
        => LiveDocumentUri is { } uri ? lspClient.RequestFormattingAsync(uri, ct) : Task.FromResult<TextEdit[]>([]);

    /// <summary>
    /// Exposes the document URI for cross-file definition navigation and for the rename edit lookup.
    /// </summary>
    /// <remarks>
    /// Falls back to what a session WOULD be opened under when none is open, because both callers are
    /// comparing a server's answer against "this document" and a null would read as "not this one" —
    /// which, for a reply that can only have been about this document, is the wrong answer rather than a
    /// cautious one. The requests themselves are gated; a comparison is not a request.
    /// </remarks>
    public string GetDocumentUriPublic() => LiveDocumentUri ?? DocumentWireName.For(Identity);

    /// <summary>
    /// Opens the document a server's <c>Location</c> names and puts the caret on it.
    /// </summary>
    /// <remarks>
    /// <b>This used to be a no-op, justified by a comment about the BUNDLED server.</b> It said the server
    /// "currently only returns symbols from the same file" — true of ours, and a statement about one
    /// backend rather than about the protocol. A foreign server that answers properly stops it being true
    /// without anything here changing, and the failure is silent: the definition is found, returned, and
    /// dropped.
    /// </remarks>
    public void NavigateToUri(string uri, int line, int col)
    {
        // Line and column arrive one-based from the caller, which has already converted from the
        // protocol's zero-based Position.
        if (!editorService.NavigateTo(uri, line, col))
            Log.Debug("[definition] Nothing loaded answers to {Uri}", uri);
    }

    private void OnSelectedProcedureChanged(string? oldValue, string? newValue)
    {
        if (newValue is null or DeclarationsProc) return;

        if (SelectedObject == GeneralObject || SelectedObject is null)
        {
            // Navigate to existing proc by symbol range
            // Searched over the same flattened view the dropdown was filled from. Looking only at the top
            // level here would offer a name and then decline to navigate to it.
            var sym = FlattenedSymbols().FirstOrDefault(s => s.Name == newValue);
            if (sym is null) return;
            var line = sym.SelectionRange.Start.Line + 1;
            if (line >= 1 && line <= Document.LineCount)
                CaretOffset = Document.GetLineByNumber(line).Offset;
        }
        else
        {
            // Event handler: find or generate Sub ObjectName_EventName(...)
            var handlerName = $"{SelectedObject}_{newValue}";
            var searchText  = $"Sub {handlerName}";
            var idx = Document.IndexOf(searchText, 0, Document.TextLength, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var line = Document.GetLineByOffset(idx);
                CaretOffset = line.EndOffset;
            }
            else
            {
                InsertEventHandlerStub(SelectedObject, newValue);
            }
        }

        FocusWindowRequest?.Invoke();
    }

    private void InsertEventHandlerStub(string objectName, string eventName)
    {
        var component = FindComponent(objectName);
        EventClass? ev = null;
        if (component is not null)
            foreach (var e in component.BaseClass.Events)
                if (e.Name == eventName) { ev = e; break; }

        var sb = new StringBuilder();
        sb.Append($"Private Sub {objectName}_{eventName}(");
        if (ev is not null)
        {
            for (int i = 0; i < ev.Arguments.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append($"{ev.Arguments[i].DefaultName} As {ev.Arguments[i].Type}");
            }
        }
        sb.AppendLine(")");
        sb.AppendLine();
        sb.Append($"End Sub");

        InsertAtEnd(sb.ToString(), string.Empty);
    }

    public void SaveFormAs() => SaveWithFormattingAsync(() => projectService.SaveForm(formDefinition!, true)).ListenErrors();

    public void ViewCode() => editorService.EditCode(formDefinition);

    public void ViewObject() => editorService.EditForm(formDefinition);

    public async Task AddProcedure()
    {
        var vm = new AddProcedureViewModel();
        if (!await windowManager.ShowDialog(vm))
            return;

        var code = vm.GenerateCode();
        InsertAtEnd(code.beginCode, code.endCode);
    }

    private void InsertAtEnd(string beginCode, string endCode)
    {
        var textLen = Document.TextLength;
        if (textLen >= 2)
        {
            var end = Document.GetText(textLen - 2, 2);
            if (end != "\n\n")
                Document.Insert(textLen, "\n\n");
            else if (end[1] == '\n')
                Document.Insert(textLen, "\n");
        }

        Document.Insert(Document.TextLength, beginCode);
        var offset = Document.TextLength;
        Document.Insert(Document.TextLength, endCode);
        CaretOffset = offset;
        FocusWindowRequest?.Invoke();
    }
}
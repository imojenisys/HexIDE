using System;
using System.IO;
using Avalonia.Headless.XUnit;
using HexIDE.Bookmarks;
using HexIDE.Debugging;
using HexIDE.Events;
using HexIDE.Forms.ViewModels;
using HexIDE.IDE;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;

namespace HexIDE.Tests.IDE;

/// <summary>
/// The header the code window shows is kept in step with the document, and the document's marks are moved
/// when it changes height (hexide-io/HexIDE#273 tasks 3.3, 3.3a and 3.3b).
/// </summary>
/// <remarks>
/// <b>Most of this deliberately runs with nothing open.</b> Breakpoints and bookmarks are bare integers held
/// per document rather than anchors in a buffer, so the case that matters — and the one 3.3b exists for — is
/// a header that changes on a document with no code window at all. A refresher tested only through an open
/// editor would pass while leaving every closed document's marks pointing at the wrong statements.
/// </remarks>
public class HeaderRefresherTests
{
    private static readonly ProjectDefinition Project = new(VBProjectType.EXE, "P");

    private sealed class NullSink : IDeserializeErrorSink
    {
        public static readonly NullSink Instance = new();
        public void LogError(string _) { }
    }

    private readonly EventBus bus = new();
    private readonly BreakpointService breakpoints = new();
    private readonly BookmarkService bookmarks = new();
    private readonly IDocumentDockService dock = Substitute.For<IDocumentDockService>();

    private HeaderRefresher CreateSut()
    {
        dock.OpenDocuments.Returns(new List<BaseEditorWindowViewModel>());
        return new HeaderRefresher(bus, dock, breakpoints, bookmarks);
    }

    private const string OneLabel = """
        VERSION 5.00
        Begin VB.Form Form1
           Begin VB.Label Label1
              Caption         =   "Hi"
              Left            =   120
              Top             =   120
              Width           =   1000
              Height          =   240
           End
        End
        Attribute VB_Name = "Form1"
        Option Explicit
        """;

    private static FormDefinition Deserialize(string source)
    {
        var form = new FormDeserializer().Deserialize(Project, source, NullSink.Instance)!;
        form.AbsolutePath = Path.Combine(Path.GetTempPath(), "hexide-header-refresher", "Form1.frm");
        return form;
    }

    private static ComponentInstance AButton(string name) =>
        new(CommandButtonComponentClass.Instance, name);

    // -- The arithmetic ----------------------------------------------------------------------------

    [Fact]
    public void MarksBelowTheHeaderMoveByTheDifference()
    {
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);
        // Line 12 of the buffer is Option Explicit: the header is 10 lines and the code section opens with
        // its Attribute VB_Name.
        breakpoints.SetDocument(document, [12]);
        bookmarks.SetBookmarks(document, [11]);

        CreateSut().ShiftMarks(document, Header(10), Header(13));

        breakpoints.GetBreakpoints(document).Should().Equal(15);
        bookmarks.GetBookmarks(document).Should().Equal(14);
    }

    [Fact]
    public void AShorterHeaderMovesThemBack()
    {
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);
        breakpoints.SetDocument(document, [12, 20]);

        CreateSut().ShiftMarks(document, Header(10), Header(8));

        breakpoints.GetBreakpoints(document).Should().Equal(10, 18);
    }

    [Fact]
    public void AMarkInsideTheHeaderStaysWhereItIs()
    {
        // The header is being replaced by another header, so a mark on its line 3 is still on its line 3.
        // Task 3.7 stops a mark being set there at all; until then this is the honest answer rather than a
        // clamp that would silently pile every header mark onto one line.
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);
        breakpoints.SetDocument(document, [3, 10, 11]);

        CreateSut().ShiftMarks(document, Header(10), Header(14));

        breakpoints.GetBreakpoints(document).Should().Equal(3, 10, 15);
    }

    [Fact]
    public void TheTwoStoresDisagreeAboutTheBaseAndTheShiftRespectsThat()
    {
        // Breakpoints are 1-based and bookmarks 0-based, both stated to callers on the shipped automation
        // tools and both written raw into the same .user.hexproj. The same source line is therefore a
        // different integer in each store, and a shift that used one rule for both would move one of them
        // off by one -- onto the wrong statement, which for a breakpoint means it never fires.
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);
        // One mark on the header's LAST line and one on the code's FIRST, in each store, because only a
        // mark at the boundary can tell the two rules apart -- and a test without one passes whichever
        // rule the code uses.
        breakpoints.SetDocument(document, [10, 11]);   // 1-based: last header line, first code line
        bookmarks.SetBookmarks(document, [9, 10]);     // 0-based: the same two lines

        CreateSut().ShiftMarks(document, Header(10), Header(12));

        breakpoints.GetBreakpoints(document).Should().Equal(10, 13);
        bookmarks.GetBookmarks(document).Should().Equal(9, 12);
    }

    [Fact]
    public void AHeaderOfTheSameHeightMovesNothingAndSaysNothing()
    {
        // Not merely "the numbers are unchanged": both stores raise a change event from every mutator, and
        // the gutters repaint, the sidecar saves and a live run is re-pushed off those events. A no-op that
        // still announced would do all three on every nudge of a control.
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);
        breakpoints.SetDocument(document, [12]);
        var announced = 0;
        breakpoints.BreakpointsChanged += _ => announced++;

        CreateSut().ShiftMarks(document, Header(10), "x" + Header(10));

        breakpoints.GetBreakpoints(document).Should().Equal(12);
        announced.Should().Be(0);
    }

    [Fact]
    public void LineEndingsDoNotDecideTheCount()
    {
        // Counted by '\n', never by Environment.NewLine. A .frm is CRLF wherever it came from, but the
        // buffer is also composed from text HexIDE produced, and build-ide runs on ubuntu-latest -- a count
        // that asked the host what a line ending is would move every mark on one machine and not the other.
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);
        breakpoints.SetDocument(document, [12]);

        CreateSut().ShiftMarks(document, "a\nb\n", "a\r\nb\r\nc\r\n");

        breakpoints.GetBreakpoints(document).Should().Equal(13);
    }

    private static string Header(int lines) => string.Concat(Enumerable.Repeat("x\r\n", lines));

    // -- The gates ---------------------------------------------------------------------------------

    [Fact]
    public void AFormThatCannotBeSavedFaithfullyIsNeverReRendered()
    {
        // FormSerializer carries no fidelity check of its own -- SerializeFormToFile refuses BEFORE calling
        // it -- so without this gate a refresh would put a flattened menu hierarchy in the code window as
        // though it were the file, for exactly the forms whose save is refused to stop that reaching disk.
        var form = Deserialize(OneLabel);
        var asRead = form.DesignerText;
        form.MarkUnfaithfulToSave(UnfaithfulSaveCause.NestedContainers, "a menu was flattened");
        form.UpdateComponents([.. form.Components, AButton("Command1")]);

        CreateSut().LayoutChanged(form);

        form.DesignerText.Should().Be(asRead, "the buffer keeps showing the file the save refusal protects");
    }

    [Fact]
    public void AFormWithNoFileIsNotReRenderedYet()
    {
        // Deferred to task 3.4, which settles what a fileless form's companion references are called. Left
        // alone rather than rendered against a guessed name: the .frx citations are written from it.
        var form = Deserialize(OneLabel);
        form.AbsolutePath = null;
        var asRead = form.DesignerText;
        form.UpdateComponents([.. form.Components, AButton("Command1")]);

        CreateSut().LayoutChanged(form);

        form.DesignerText.Should().Be(asRead);
    }

    // -- End to end, with nothing open -------------------------------------------------------------

    [Fact]
    public void ACommitReRendersTheHeaderAndMovesTheMarksWithNoCodeWindowOpen()
    {
        // The clause 3.3b exists for. Nothing here is open: the marks live in the stores, keyed by the
        // document's identity, and a header that grew has to move them there or they stay pointing at
        // whatever statement used to be on that line.
        var sut = CreateSut();
        var form = Deserialize(OneLabel);
        var document = DocumentIdentity.For(form);

        // Settle the header on what a render produces, so the second call measures the control rather than
        // the difference between the file's formatting and the serializer's.
        sut.LayoutChanged(form);
        var before = form.DesignerText!;
        var firstBodyLine = before.Count(c => c == '\n') + 1;
        breakpoints.SetDocument(document, [firstBodyLine + 1]);

        form.UpdateComponents([.. form.Components, AButton("Command1")]);
        sut.LayoutChanged(form);

        var grewBy = form.DesignerText!.Count(c => c == '\n') - before.Count(c => c == '\n');
        grewBy.Should().BeGreaterThan(0, "a control adds at least its Begin and its End");
        breakpoints.GetBreakpoints(document).Should().Equal(firstBodyLine + 1 + grewBy);
    }

    [Fact]
    public void TheNotificationOnTheBusIsWhatDrivesIt()
    {
        // The four paths that can commit a layout change do not know about this class, and must not have to:
        // the designer's undo stack, the menu editor, the colour palette and automation with no designer
        // open all publish the one event instead.
        var sut = CreateSut();
        var form = Deserialize(OneLabel);
        sut.LayoutChanged(form);
        var before = form.DesignerText!;
        form.UpdateComponents([.. form.Components, AButton("Command1")]);

        bus.Publish(new FormLayoutChangedEvent(form));

        form.DesignerText.Should().NotBe(before);
        form.DesignerText.Should().Contain("Command1");
    }

    [Fact]
    public void TheRenderedHeaderIsTheDesignerHalfAndStopsAtTheRootEnd()
    {
        // The composition invariant the whole phase rests on: prefix + Code is the file. If the render ran
        // past the root End it would put VERSION and Begin VB.Form into the code window twice, and the
        // second copy would be compiled as VB.
        var sut = CreateSut();
        var form = Deserialize(OneLabel);
        form.UpdateComponents([.. form.Components, AButton("Command1")]);

        sut.LayoutChanged(form);

        // The Command1 clause is what stops this passing vacuously: every assertion below is equally true
        // of the text as READ, so without it the test would go green with the render deleted.
        form.DesignerText.Should().Contain("Begin VB.CommandButton Command1");
        form.DesignerText.Should().EndWith("End\r\n");
        form.DesignerText.Should().NotContain("Option Explicit");
        FormCodeText.WholeFile(form).Should().Be(form.DesignerText + form.Code);
    }

    // -- With a window open ------------------------------------------------------------------------

    [Fact]
    public void ANewHeaderIsRecordedEvenWhenItIsTheOnlyThingThatChanged()
    {
        // ApplyHeader is what a save calls with the header it just wrote. The serializer is a reproduction
        // rather than a byte-faithful copy, so this fires on the first save of a file HexIDE did not write
        // -- which is the case the invariant is worded for: after a save the buffer is the text that save
        // wrote, not the text it was opened with.
        var form = Deserialize(OneLabel);

        CreateSut().ApplyHeader(form, "VERSION 5.00\r\nBegin VB.Form Form1\r\nEnd\r\n");

        form.DesignerText.Should().Be("VERSION 5.00\r\nBegin VB.Form Form1\r\nEnd\r\n");
    }

    [AvaloniaFact]
    public void AnOpenCodeWindowIsReHeadedWhereItStands()
    {
        // The other half of the same operation, and the only part a developer sees. The refresher finds the
        // window by the document's IDENTITY rather than by a name or a path, because both of those change
        // under it -- renaming the form is one of the things that moves the header.
        var form = Deserialize(OneLabel);
        var editor = NewCodeEditor().Initialize(form);
        dock.OpenDocuments.Returns(new List<BaseEditorWindowViewModel> { editor });
        var sut = new HeaderRefresher(bus, dock, breakpoints, bookmarks);

        sut.ApplyHeader(form, "VERSION 5.00\r\nBegin VB.Form Form1\r\nEnd\r\n");

        editor.Document.Text.Should().Be("VERSION 5.00\r\nBegin VB.Form Form1\r\nEnd\r\n" + form.Code);
        editor.BufferBody.Should().Be(form.Code);
        editor.Dispose();
    }

    /// <summary>A code editor with every collaborator stubbed: only the buffer is under test here.</summary>
    private static CodeEditorViewModel NewCodeEditor()
    {
        var eventBus = Substitute.For<IEventBus>();
        eventBus.Subscribe<CreateOrNavigateToSubEvent>(Arg.Any<Action<CreateOrNavigateToSubEvent>>())
            .Returns(Substitute.For<IDisposable>());
        eventBus.Subscribe<ApplyAllUnsavedChangesEvent>(Arg.Any<Action<ApplyAllUnsavedChangesEvent>>())
            .Returns(Substitute.For<IDisposable>());
        eventBus.Subscribe<FormUnloadedEvent>(Arg.Any<Action<FormUnloadedEvent>>())
            .Returns(Substitute.For<IDisposable>());
        return new CodeEditorViewModel(
            Substitute.For<IWindowManager>(),
            Substitute.For<IEditorService>(),
            Substitute.For<HexIDE.Projects.IProjectService>(),
            eventBus,
            Substitute.For<HexIDE.Lsp.ILspClient>(),
            Substitute.For<ISettingsService>(),
            Substitute.For<IStatusBarService>(),
            Substitute.For<IBookmarkService>(),
            Substitute.For<IBreakpointService>(),
            Substitute.For<HexIDE.Runtime.Debugging.IDebugController>(),
            Substitute.For<IRunScope>(),
            Substitute.For<HexIDE.Localization.ILocalizationService>());
    }
}

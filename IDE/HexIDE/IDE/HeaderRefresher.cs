using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HexIDE.Bookmarks;
using HexIDE.Debugging;
using HexIDE.Events;
using HexIDE.Forms.ViewModels;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;
using Serilog;

namespace HexIDE.IDE;

/// <summary>
/// The one implementation of <see cref="IHeaderRefresher"/>. The contract, and why the seam is where it is,
/// are on the interface; what is here is how each trigger reaches it.
/// </summary>
public sealed class HeaderRefresher : IHeaderRefresher, IDisposable
{
    private readonly IDocumentDockService dock;
    private readonly IBreakpointService breakpoints;
    private readonly IBookmarkService bookmarks;
    private readonly IDisposable subscription;

    public HeaderRefresher(
        IEventBus eventBus,
        IDocumentDockService dock,
        IBreakpointService breakpoints,
        IBookmarkService bookmarks)
    {
        this.dock = dock;
        this.breakpoints = breakpoints;
        this.bookmarks = bookmarks;
        subscription = eventBus.Subscribe<FormLayoutChangedEvent>(e => LayoutChanged(e.Form));
    }

    public void Dispose() => subscription.Dispose();

    /// <summary>
    /// Trigger one: a committed change to a form's designer half. Re-renders the designer text and makes it
    /// the header, when the render moved and when the form is one HexIDE may render at all.
    /// </summary>
    /// <remarks>
    /// <b>Two gates, and neither is defensive tidiness.</b>
    /// <list type="number">
    /// <item><description>A form HexIDE cannot reproduce is never re-rendered. <c>FormSerializer</c> carries
    /// no fidelity check of its own — <c>SerializeFormToFile</c> refuses <em>before</em> calling it — so a
    /// refresh without this gate would put a flattened menu hierarchy in the code window as though it were
    /// the file, for exactly the forms whose save is refused to stop that reaching disk. The developer's
    /// window would then disagree with both the file and the refusal.</description></item>
    /// <item><description>A form with no file yet is left alone until task 3.4 settles what its companion
    /// references should be called. Read from the <em>identity</em>, not from
    /// <c>FormDefinition.AbsolutePath</c>: a UserControl's two halves diverge (#474) and the module's is the
    /// one the project file names.</description></item>
    /// </list>
    /// Both log at Debug, because "the header did not refresh" is otherwise indistinguishable from "nothing
    /// was raised".
    /// </remarks>
    public void LayoutChanged(FormDefinition form)
    {
        var identity = DocumentIdentity.For(form);

        if (!form.CanSaveFaithfully)
        {
            Log.Debug("HeaderRefresher: not re-rendering {Document} — {Reason}",
                identity.Display, form.UnfaithfulSaveReason);
            return;
        }

        if (identity.AbsolutePath is not { } path)
        {
            Log.Debug("HeaderRefresher: not re-rendering {Document} — it has no file yet", identity.Display);
            return;
        }

        ApplyHeader(form, new FormSerializer().SerializeDesignerText(form, Path.GetFileName(path)));
    }

    /// <summary>
    /// Makes <paramref name="header"/> the document's header: recorded on the model, the marks below it
    /// moved, and an open buffer re-headed in place. Does nothing when it is the header already.
    /// </summary>
    /// <remarks>
    /// Also trigger two. A save passes the header it just wrote rather than letting this re-render, because
    /// the two can differ: the render a save makes is the one that reached disk, and a second render taken
    /// afterwards would be of a model that may have moved in between. It is called before the save is
    /// announced so that a server reading the buffer and a server reading the file agree about line one.
    /// </remarks>
    public void ApplyHeader(FormDefinition form, string header)
    {
        var identity = DocumentIdentity.For(form);
        var current = FormCodeText.Prefix(form);

        // The model half only when the model actually moves, because the marks move with it and a shift of
        // zero must not announce itself to two stores, three gutters and the sidecar.
        if (!string.Equals(current, header, StringComparison.Ordinal))
        {
            form.RecordDesignerText(header);
            ShiftMarks(identity, current, header);
        }

        // The buffer half unconditionally, and that is not belt-and-braces. An open window can hold a header
        // the model does not -- an undo puts the previous one back in the buffer while the model keeps the
        // current one -- and gating this on the model having moved would leave that window showing a header
        // for a form that no longer looks like it, permanently: the next render equals what is recorded, so
        // nothing would ever repair it. RefreshPrefix is a no-op when the two already agree.
        EditorFor(identity)?.RefreshPrefix(header);
    }

    /// <summary>
    /// Moves a document's breakpoints and bookmarks by the change in the header's line count.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>In the stores, not in a gutter.</b> Both gutters read their store live and repaint on its change
    /// event, the sidecar saves from it and the debugger is re-pushed from it, so a shift written here
    /// reaches all four — and reaches a document nobody has open, which a gutter could not.
    /// </para>
    /// <para>
    /// <b>A mark inside the header stays where it is.</b> The header is being replaced by another header, so
    /// a mark on line 3 of it is still on line 3 of it. Only marks below the old header move, and the
    /// arithmetic makes that automatic: a mark at or past the first body line lands at or past the new one.
    /// Task 3.7 stops a mark being set in the header at all; until then this is the honest answer rather
    /// than a clamp.
    /// </para>
    /// <para>
    /// The two stores disagree about the base — breakpoints are 1-based and bookmarks 0-based, both stated
    /// to callers on the shipped automation tools — so the comparison differs by one between them and the
    /// shift does not.
    /// </para>
    /// </remarks>
    public void ShiftMarks(DocumentIdentity document, string oldHeader, string newHeader)
    {
        var oldLines = LineCount(oldHeader);
        var delta = LineCount(newHeader) - oldLines;
        if (delta == 0)
            return;

        var marks = breakpoints.GetBreakpoints(document);
        if (marks.Count > 0)
            breakpoints.SetDocument(document, Shift(marks, oldLines + 1, delta));

        var marked = bookmarks.GetBookmarks(document);
        if (marked.Count > 0)
            bookmarks.SetBookmarks(document, Shift(marked, oldLines, delta));
    }

    /// <summary>Every line at or after <paramref name="firstBodyLine"/> moved by <paramref name="delta"/>.</summary>
    private static IEnumerable<int> Shift(IReadOnlyList<int> lines, int firstBodyLine, int delta) =>
        lines.Select(line => line >= firstBodyLine ? line + delta : line).ToList();

    /// <summary>
    /// The number of lines a header occupies, which is the 0-based line the body starts on.
    /// </summary>
    /// <remarks>
    /// Counts <c>'\n'</c> rather than splitting on <c>Environment.NewLine</c>. A <c>.frm</c> is a
    /// Windows-native format and is CRLF-terminated wherever it came from, but the buffer is also composed
    /// from text HexIDE itself produced, and <c>build-ide</c> runs on <c>ubuntu-latest</c> — a count that
    /// asked the host what a line ending is would answer differently there and move every mark on the
    /// wrong machine.
    /// </remarks>
    private static int LineCount(string header) => header.Count(c => c == '\n');

    private CodeEditorViewModel? EditorFor(DocumentIdentity document) =>
        dock.OpenDocuments.OfType<CodeEditorViewModel>()
            .FirstOrDefault(editor => editor.OpenDocument == document);
}

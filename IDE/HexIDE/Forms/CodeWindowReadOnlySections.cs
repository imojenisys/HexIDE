using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using HexIDE.Runtime.Serialization;

namespace HexIDE.Forms;

/// <summary>
/// What typing may change in a code window: nothing at all while the document is held read-only as a whole,
/// and otherwise everything but its header and its members' attribute runs (hexide-io/HexIDE#273 task 3.7).
/// </summary>
/// <remarks>
/// <para>
/// <b>One provider carries both gates, because the editor has room for only one.</b> Binding
/// <c>TextEditor.IsReadOnly</c> replaces the text area's section provider outright, so a form held
/// read-only as a whole would lose the region rules the moment it became editable again, and vice versa.
/// Both verdicts are read live on every call, so a reload that flips either one takes effect at the next
/// keystroke with nothing to re-install.
/// </para>
/// <para>
/// <b>The whole-document gate is not a region.</b> It refuses typing and nothing else. Breakpoints and Find
/// ask <see cref="ViewModels.CodeEditorViewModel.IsReadOnlyRegion"/>, which never consults it.
/// </para>
/// <para>
/// <b>A deletion can only be narrowed here, never widened.</b> AvaloniaEdit's
/// <c>TextArea.GetDeletableSegments</c> throws if a provider returns anything outside the span it was asked
/// about. So "deleting a member takes its attribute run with it" is expressed as <i>not carving the run
/// out</i> when the deletion already spans the run and the line it describes. Deleting the declaration line
/// alone cannot reach the run below it through this seam; the guarded write path owns that case.
/// </para>
/// </remarks>
internal sealed class CodeWindowReadOnlySections : TextSegmentReadOnlySectionProvider<TextSegment>
{
    private readonly TextDocument document;
    private readonly Func<bool> wholeDocumentReadOnly;
    private readonly Func<IReadOnlyList<TextRegion>> regions;

    public CodeWindowReadOnlySections(TextDocument document, Func<bool> wholeDocumentReadOnly,
        Func<IReadOnlyList<TextRegion>> regions)
        : base(document)
    {
        this.document = document;
        this.wholeDocumentReadOnly = wholeDocumentReadOnly;
        this.regions = regions;
    }

    /// <summary>
    /// False inside a region, including at its first character — the stock provider allows insertion at
    /// both edges of a segment, which would let text land above the top of the file or between a
    /// declaration and its attributes. True at a region's end, which is the start of the next line.
    /// </summary>
    public override bool CanInsert(int offset)
    {
        if (wholeDocumentReadOnly())
            return false;
        foreach (var region in regions())
            if (region.Touches(offset, 0))
                return false;
        return true;
    }

    /// <summary>
    /// <paramref name="segment"/> minus every protected span it overlaps, in order.
    /// </summary>
    /// <remarks>
    /// A member's protected span starts at the terminator of the line it describes, not at the run itself:
    /// deleting that terminator would join the first attribute line onto the declaration. The span is
    /// dropped altogether when the deletion covers the whole run and the whole line above it.
    /// </remarks>
    public override IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
    {
        if (wholeDocumentReadOnly())
            return [];

        var result = new List<ISegment>();
        var cursor = segment.Offset;
        var end = segment.EndOffset;
        foreach (var span in ProtectedSpans(segment))
        {
            if (span.End <= cursor) continue;
            if (span.Start >= end) break;
            if (span.Start > cursor)
                result.Add(new SimpleSegment(cursor, span.Start - cursor));
            cursor = Math.Max(cursor, span.End);
        }
        if (cursor < end)
            result.Add(new SimpleSegment(cursor, end - cursor));
        return result;
    }

    private IEnumerable<(int Start, int End)> ProtectedSpans(ISegment segment)
    {
        foreach (var region in regions())
        {
            var described = region.Start > 0 ? document.GetLineByOffset(region.Start).PreviousLine : null;
            if (described is null)
            {
                // The header, or a run with no line above it to describe.
                yield return (region.Start, region.End);
                continue;
            }
            if (segment.Offset <= described.Offset && segment.EndOffset >= region.End)
                continue;
            yield return (described.EndOffset, region.End);
        }
    }
}

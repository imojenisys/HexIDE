using System.Collections.Generic;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using HexIDE.Lsp.Messages;
using HexIDE.Runtime.Serialization;

namespace HexIDE.Controls;

/// <summary>
/// The folds a code window shows: a language server's, converted from <see cref="FoldingRange"/>, and the ones
/// the editor makes itself, merged into one list a <see cref="FoldingManager"/> accepts.
/// </summary>
/// <remarks>
/// The collapsed label for each fold is the trimmed text of the fold's first line.
/// </remarks>
public static class Vb6FoldingAdapter
{
    /// <summary>Applies a server's folds and nothing else: for a document with no header of its own.</summary>
    public static void Apply(FoldingManager manager, TextDocument document, FoldingRange[] ranges) =>
        manager.UpdateFoldings(Merge([], FromServer(document, ranges)), -1);

    /// <summary>A server's folds as the folding manager takes them, in the server's order.</summary>
    /// <remarks>
    /// A fold must span lines: one that starts and ends on the same line, or names a line the document does
    /// not have, is skipped.
    /// </remarks>
    public static List<NewFolding> FromServer(TextDocument document, FoldingRange[] ranges)
    {
        var folds = new List<NewFolding>(ranges.Length);
        foreach (var r in ranges)
        {
            var startLine = r.StartLine + 1; // LSP is 0-based; AvaloniaEdit is 1-based
            var endLine   = r.EndLine   + 1;

            if (startLine < 1 || endLine > document.LineCount || endLine <= startLine)
                continue;

            var endDocLine = document.GetLineByNumber(endLine);
            folds.Add(Spanning(document, startLine, endDocLine.Offset + endDocLine.Length));
        }
        return folds;
    }

    /// <summary>
    /// The header's fold: from the top of the file to the end of the header's last line, or null when the
    /// header is shorter than two lines.
    /// </summary>
    /// <param name="regions">The buffer's read-only regions, header first; see <see cref="ReadOnlyRegions.Of"/>.</param>
    /// <remarks>
    /// <para>
    /// <b>The fold stops before the header's last line break</b>, so the line after the header, the first line
    /// of code, stays a line of its own and folded the header reads as one line above the code.
    /// </para>
    /// <para>
    /// <b>A one-line header is not folded.</b> A fold hides text inside the lines it spans and leaves the first
    /// line's label in their place; on one line that label is the line itself, so folding it changes nothing a
    /// developer can see. A <c>.bas</c> whose header is its <c>Attribute VB_Name</c> line is that case.
    /// </para>
    /// </remarks>
    public static NewFolding? Header(TextDocument document, IReadOnlyList<TextRegion> regions)
    {
        // The header is the region nothing anchors, and is always first; a member's run is anchored to the line above.
        if (regions.Count == 0 || regions[0].Anchor is not null)
            return null;

        var header = regions[0];
        var last = document.GetLineByOffset(header.End > header.Start ? header.End - 1 : header.Start);
        if (last.LineNumber < 2)
            return null;
        return Spanning(document, 1, last.Offset + last.Length);
    }

    /// <summary>
    /// The editor's own folds and a server's, as one list the folding manager accepts.
    /// </summary>
    /// <param name="own">
    /// The folds the editor makes itself, which a server's answer never displaces: the header's, and from
    /// phase 4 of hexide-io/HexIDE#273 each member's attribute run.
    /// </param>
    /// <param name="server">A server's folds, possibly none.</param>
    /// <remarks>
    /// <para>
    /// <b>Merged into every application, an empty or absent server answer included.</b> Applying a server's
    /// answer replaces every fold the manager holds, so a header fold applied once would vanish the next time
    /// a server answered, or answered nothing.
    /// </para>
    /// <para>
    /// <b>A server fold that partly overlaps one of the editor's own is dropped</b>, because two folds that
    /// cross cannot both be collapsed. One that sits wholly inside it or wholly contains it is kept, as any
    /// nested fold is.
    /// </para>
    /// <para>
    /// <b>So is one that starts where one of the editor's own starts</b>, whatever its length. The folding
    /// manager matches the folds it is given to the sections it holds by start offset alone, so of two that
    /// start together each can take the other's section, and with it whether it is folded: measured, a server
    /// fold listed first took the header's section, and the header's fold came back expanded. A server fold
    /// identical to one of the editor's own is a duplicate, and dropping it loses nothing.
    /// </para>
    /// <para>
    /// <b>Sorted by start, the longer first where two start together</b>: the manager throws on a list that is
    /// not sorted by start, and the outer of two nested folds has to precede the inner. A zero-length fold is
    /// dropped, which the manager does silently anyway.
    /// </para>
    /// </remarks>
    public static List<NewFolding> Merge(IReadOnlyList<NewFolding> own, IEnumerable<NewFolding> server)
    {
        var merged = new List<NewFolding>();
        foreach (var fold in own)
        {
            if (fold.EndOffset > fold.StartOffset)
                merged.Add(fold);
        }
        foreach (var fold in server)
        {
            if (fold.EndOffset <= fold.StartOffset)
                continue;
            var crosses = false;
            foreach (var mine in own)
            {
                if (fold.StartOffset == mine.StartOffset || Crosses(fold, mine))
                {
                    crosses = true;
                    break;
                }
            }
            if (!crosses)
                merged.Add(fold);
        }
        merged.Sort((a, b) => a.StartOffset != b.StartOffset
            ? a.StartOffset.CompareTo(b.StartOffset)
            : b.EndOffset.CompareTo(a.EndOffset));
        return merged;
    }

    /// <summary>True when the two folds overlap without one containing the other.</summary>
    private static bool Crosses(NewFolding a, NewFolding b) =>
        a.StartOffset < b.EndOffset && b.StartOffset < a.EndOffset
        && !(a.StartOffset <= b.StartOffset && a.EndOffset >= b.EndOffset)
        && !(b.StartOffset <= a.StartOffset && b.EndOffset >= a.EndOffset);

    private static NewFolding Spanning(TextDocument document, int startLine, int endOffset)
    {
        var firstLine = document.GetLineByNumber(startLine);
        var label = document.GetText(firstLine.Offset, firstLine.Length).Trim();
        if (string.IsNullOrEmpty(label))
            label = "...";
        return new NewFolding(firstLine.Offset, endOffset) { Name = label };
    }
}

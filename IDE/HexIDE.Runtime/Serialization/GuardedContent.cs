using System;
using System.Collections.Generic;

namespace HexIDE.Runtime.Serialization;

/// <summary>What replacing a document's content came to.</summary>
/// <param name="NewText">The whole buffer to put in place, or null when the replacement was refused.</param>
/// <param name="Refusal">Why it was refused, in words a caller can act on; null when it was not.</param>
/// <param name="HeaderKept">
/// True when the incoming text carried no header of its own, or only part of it, so the document's header was
/// put in front of it. A caller that reports on its write can say so.
/// </param>
public readonly record struct ContentReplacement(string? NewText, string? Refusal, bool HeaderKept);

/// <summary>
/// Replacing a whole document's content without letting the replacement change its header: the rule the
/// add-in <c>SetContent</c> and automation <c>set_file_content</c> rows of the design record share
/// (hexide-io/HexIDE#273 task 3.9).
/// </summary>
/// <remarks>
/// <para>
/// <b>Three shapes are accepted, and one is refused.</b> A caller may send the whole file with its header
/// unchanged; the code alone; or the code preceded by the part of the header that sits in it — a form's
/// <c>Attribute</c> lines, which is what a form's code section has always begun with and what reading one
/// back has always returned. In each case the document's own header is what ends up in front. A header that
/// differs from the document's is refused, because the header is the IDE's to change, from its model.
/// </para>
/// <para>
/// <b>"Unchanged" compares line text, not terminators.</b> A caller that sends <c>\n</c> for a
/// <c>\r\n</c> document has not changed its header, and the header written back is the document's own, byte
/// for byte.
/// </para>
/// </remarks>
public static class GuardedContent
{
    /// <summary>
    /// The buffer that replacing <paramref name="current"/>'s content with <paramref name="incoming"/> gives,
    /// or the reason it is refused.
    /// </summary>
    /// <param name="current">The document's whole buffer as it stands.</param>
    /// <param name="prefixLength">The length of the header the buffer was composed with; see <see cref="ReadOnlyRegions.Of"/>.</param>
    /// <param name="incoming">What the caller wants the document to hold.</param>
    public static ContentReplacement Replace(string current, int prefixLength, string incoming)
    {
        var header = current[..ReadOnlyRegions.HeaderEnd(current, prefixLength)];
        var incomingHeaderEnd = ReadOnlyRegions.HeaderEnd(incoming, 0);
        if (incomingHeaderEnd == 0)
            return new ContentReplacement(header + incoming, null, HeaderKept: header.Length > 0);

        // Blank lines in front of the incoming header are the caller's formatting, not a header line.
        var theirs = LinesOf(incoming[..incomingHeaderEnd]);
        while (theirs.Count > 0 && theirs[0].Trim().Length == 0)
            theirs.RemoveAt(0);
        var ours = LinesOf(header);
        if (theirs.Count > ours.Count || !EndsWith(ours, theirs))
            return new ContentReplacement(null,
                "The content would change the document's header (its designer block, class header or leading "
                + "Attribute lines), which only the IDE changes. Send the code alone, or the whole file with its "
                + "header exactly as the document has it.",
                HeaderKept: false);

        return new ContentReplacement(header + incoming[incomingHeaderEnd..], null,
            HeaderKept: theirs.Count < ours.Count);
    }

    private static List<string> LinesOf(string text)
    {
        var lines = new List<string>();
        foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
            lines.Add(line);
        if (lines.Count > 0 && lines[^1].Length == 0)
            lines.RemoveAt(lines.Count - 1);
        return lines;
    }

    private static bool EndsWith(List<string> whole, List<string> tail)
    {
        var skip = whole.Count - tail.Count;
        for (var i = 0; i < tail.Count; i++)
        {
            if (!string.Equals(whole[skip + i], tail[i], StringComparison.Ordinal))
                return false;
        }
        return true;
    }
}

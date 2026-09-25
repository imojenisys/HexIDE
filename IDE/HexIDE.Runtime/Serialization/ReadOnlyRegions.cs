using System;
using System.Collections.Generic;

namespace HexIDE.Runtime.Serialization;

/// <summary>
/// A span of a code window's buffer that only the IDE may change: <c>[Start, End)</c>, whole lines, with
/// <see cref="End"/> just past the last line's terminator.
/// </summary>
/// <param name="Start">The offset of the region's first character, which is always the start of a line.</param>
/// <param name="End">The offset just past the region's last line, terminator included.</param>
/// <param name="OpenEnded">
/// True when the region's last line is the last line of the buffer and has no terminator. Text inserted at
/// <see cref="End"/> would then join that line rather than start a new one, so it counts as inside.
/// </param>
/// <param name="Anchor">
/// For a member's attribute run, where the line it describes ends: the start of that line's terminator.
/// Null for the header, which describes nothing above it. See <see cref="Changes"/>.
/// </param>
public readonly record struct TextRegion(int Start, int End, bool OpenEnded = false, int? Anchor = null)
{
    /// <summary>
    /// True when <paramref name="length"/> characters at <paramref name="offset"/> lie in this region, or an
    /// insertion there would land in it.
    /// </summary>
    /// <remarks>
    /// An insertion (<paramref name="length"/> 0) at the region's first character counts as inside, because
    /// it pushes the region away from the line above it: for the header that is text above the top of the
    /// file, and for a member's attribute run it separates the run from the declaration it describes. An
    /// insertion at <see cref="End"/> is the start of the next line and does not count, unless the region is
    /// <see cref="OpenEnded"/>.
    /// <para>
    /// This is membership: whether a line or a span is <em>in</em> the region. Whether an <em>edit</em> may
    /// be made is <see cref="Changes"/>, which is stricter.
    /// </para>
    /// </remarks>
    public bool Touches(int offset, int length) =>
        length > 0
            ? offset < End && offset + length > Start
            : offset >= Start && (offset < End || (OpenEnded && offset == End));

    /// <summary>Where an edit starts to change this region: <see cref="Anchor"/>, or <see cref="Start"/>.</summary>
    public int EditStart => Anchor ?? Start;

    /// <summary>
    /// True when replacing <paramref name="length"/> characters at <paramref name="offset"/> would change
    /// this region: <see cref="Touches"/>, and also any replacement that removes the line break between a
    /// member's attribute run and the line it describes.
    /// </summary>
    /// <remarks>
    /// That line break is outside the run, because the run is whole lines and the break ends the line above.
    /// But removing it joins the first attribute line onto the declaration, and the run is then no run at all:
    /// the line no longer starts with <c>Attribute</c>, and the file no longer compiles. Typing is refused it by
    /// the code window's section provider (#273 task 3.7), which starts a member's protected span at the same
    /// <see cref="EditStart"/>. Every other writer asks this (task 3.11, found by review: a Replace of
    /// <c>Currency\r\n</c> with <c>Currency</c> made exactly that join).
    /// <para>
    /// An insertion is unchanged: text typed at the end of a declaration lands before its line break and
    /// leaves the run where it is.
    /// </para>
    /// </remarks>
    public bool Changes(int offset, int length) =>
        length > 0
            ? offset < End && offset + length > EditStart
            : Touches(offset, 0);
}

/// <summary>
/// Where the read-only regions of a code window's buffer are: the file's header, and each run of attribute
/// lines describing a member (hexide-io/HexIDE#273 phase 3).
/// </summary>
/// <remarks>
/// <para>
/// <b>A read-only region is the header or a member's attribute run, and nothing else.</b> A form the IDE
/// cannot reproduce is held read-only <em>as a whole</em>, and that is a different gate. It must not make the
/// form's code lines part of a region, or a developer could not set a breakpoint anywhere in it and Find
/// would match nothing. So nothing here asks whether a document is faithful.
/// </para>
/// <para>
/// <b>The header is one rule for every kind: the top of the file through the last line of the leading
/// <c>Attribute</c> run.</b> It is not the buffer's prefix, and conflating the two corrupts a file. For a
/// <c>.bas</c> or <c>.cls</c> the prefix already ends after the attribute run. For a <c>.frm</c>,
/// <c>.ctl</c> or <c>.pag</c> it ends at the designer block's closing <c>End</c>, and the attribute run is
/// the first lines of the code, so the region straddles the split.
/// </para>
/// <para>
/// <b>Where the model split nothing off, the header is found from the text.</b> An unparseable
/// <c>.ctl</c>/<c>.pag</c>, or a <c>.bas</c>/<c>.cls</c> whose first line is blank (hexide-io/HexIDE#472),
/// keeps its whole file in the code. Its prefix is then empty, but the buffer still opens with a header.
/// </para>
/// </remarks>
public static class ReadOnlyRegions
{
    /// <summary>
    /// The read-only regions of <paramref name="text"/>, in order: the header first when there is one, then
    /// every member attribute run.
    /// </summary>
    /// <param name="text">The whole buffer.</param>
    /// <param name="prefixLength">
    /// The length of the header the buffer was composed with, or 0 when the model split nothing off.
    /// </param>
    public static IReadOnlyList<TextRegion> Of(string text, int prefixLength)
    {
        var lines = LinesOf(text);
        var regions = new List<TextRegion>();

        var headerLines = HeaderLineCount(text, lines, prefixLength);
        if (headerLines > 0)
            regions.Add(RegionOf(lines, 0, headerLines, text.Length));

        var i = headerLines;
        while (i < lines.Count)
        {
            if (!IsMemberAttribute(text, lines[i]))
            {
                i++;
                continue;
            }
            var first = i;
            while (i < lines.Count && IsMemberAttribute(text, lines[i]))
                i++;
            // The run hangs from the line above it, which is the member it describes. A run on the first line
            // would be the header's, so there always is one; the guard is for a text no rule here produces.
            regions.Add(RegionOf(lines, first, i, text.Length) with
            {
                Anchor = first > 0 ? lines[first - 1].ContentEnd : null,
            });
        }
        return regions;
    }

    /// <summary>
    /// The offset just past the header, which is where the developer's code starts. 0 when there is no header.
    /// </summary>
    public static int HeaderEnd(string text, int prefixLength)
    {
        var lines = LinesOf(text);
        var headerLines = HeaderLineCount(text, lines, prefixLength);
        return headerLines == 0 ? 0 : lines[headerLines - 1].Next;
    }

    /// <summary>
    /// The names the header's designer block declares: the name on each <c>Begin</c> line, which is the form's
    /// own, a control's or a menu's. Compared ignoring case, as VB6 compares names. Empty when there is none.
    /// </summary>
    /// <remarks>
    /// For rename (hexide-io/HexIDE#273 task 3.10). Such a name's declaration is in the header, so a rename
    /// that keeps off the header would leave the code naming a control that no longer has that name; the code
    /// window refuses it, and the bundled server declines it by the same rule (<c>VbProtectedRegions</c>).
    /// </remarks>
    public static IReadOnlySet<string> DeclaredNames(string text, int prefixLength)
    {
        var lines = LinesOf(text);
        var headerLines = HeaderLineCount(text, lines, prefixLength);

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headerLines; i++)
        {
            var line = Trimmed(text, lines[i]);
            if (!StartsWithKeyword(line, "Begin"))
                continue;
            var rest = line["Begin".Length..].TrimStart();
            var afterType = rest.IndexOfAny(' ', '\t');
            if (afterType < 0)
                continue;
            var name = rest[afterType..].Trim();
            if (!name.IsEmpty)
                names.Add(name.ToString());
        }
        return names;
    }

    /// <summary>
    /// True when replacing <paramref name="length"/> characters at <paramref name="offset"/> would change any
    /// of <paramref name="regions"/>.
    /// </summary>
    public static bool Touches(IReadOnlyList<TextRegion> regions, int offset, int length)
    {
        foreach (var region in regions)
        {
            if (region.Touches(offset, length))
                return true;
        }
        return false;
    }

    /// <summary>
    /// True when replacing <paramref name="length"/> characters at <paramref name="offset"/> would change any
    /// of <paramref name="regions"/>, joining a member's attribute run onto its declaration included. The
    /// question every writer asks; see <see cref="TextRegion.Changes"/>.
    /// </summary>
    public static bool Changes(IReadOnlyList<TextRegion> regions, int offset, int length)
    {
        foreach (var region in regions)
        {
            if (region.Changes(offset, length))
                return true;
        }
        return false;
    }

    /// <summary>One line of the buffer: where its text starts and ends, and where the next line starts.</summary>
    private readonly record struct Line(int Start, int ContentEnd, int Next);

    private static List<Line> LinesOf(string text)
    {
        var lines = new List<Line>();
        var pos = 0;
        while (pos < text.Length)
        {
            var newline = text.IndexOf('\n', pos);
            var next = newline < 0 ? text.Length : newline + 1;
            var end = newline < 0 ? text.Length : newline;
            if (end > pos && text[end - 1] == '\r')
                end--;
            lines.Add(new Line(pos, end, next));
            pos = next;
        }
        return lines;
    }

    private static ReadOnlySpan<char> Trimmed(string text, Line line) =>
        text.AsSpan(line.Start, line.ContentEnd - line.Start).Trim();

    private static TextRegion RegionOf(List<Line> lines, int firstLine, int endLine, int textLength)
    {
        var last = lines[endLine - 1];
        return new TextRegion(lines[firstLine].Start, last.Next, OpenEnded: last.ContentEnd == textLength);
    }

    private static int HeaderLineCount(string text, List<Line> lines, int prefixLength)
    {
        if (prefixLength > 0 && prefixLength <= text.Length)
        {
            var prefixLines = 0;
            while (prefixLines < lines.Count && lines[prefixLines].Next <= prefixLength)
                prefixLines++;
            return prefixLines + AttributeRunLength(text, lines, prefixLines);
        }
        return HeaderLinesFromText(text, lines);
    }

    /// <summary>
    /// The header of a buffer the model split nothing off from: a <c>VERSION</c> line and the class or
    /// designer block after it, then the leading attribute run, when the text has them.
    /// </summary>
    /// <remarks>
    /// The designer block is walked by nesting, because a <c>.frm</c>'s controls are nested
    /// <c>Begin</c>/<c>End</c> blocks and its fonts and images nested <c>BeginProperty</c>/<c>EndProperty</c>
    /// ones. A text that opens with none of this has no header, and blank lines alone are not one.
    /// </remarks>
    private static int HeaderLinesFromText(string text, List<Line> lines)
    {
        var i = 0;
        while (i < lines.Count && Trimmed(text, lines[i]).IsEmpty)
            i++;

        var sawBlock = false;
        if (i < lines.Count && StartsWithKeyword(Trimmed(text, lines[i]), "VERSION"))
        {
            sawBlock = true;
            i++;
            if (i < lines.Count && Trimmed(text, lines[i]).Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                while (i < lines.Count && !Trimmed(text, lines[i]).Equals("END", StringComparison.OrdinalIgnoreCase))
                    i++;
                if (i < lines.Count)
                    i++;
            }
            else
            {
                while (i < lines.Count && StartsWithKeyword(Trimmed(text, lines[i]), "Object"))
                    i++;
                if (i < lines.Count && StartsWithKeyword(Trimmed(text, lines[i]), "Begin"))
                {
                    var depth = 0;
                    do
                    {
                        var line = Trimmed(text, lines[i]);
                        if (StartsWithKeyword(line, "Begin") || line.StartsWith("BeginProperty", StringComparison.OrdinalIgnoreCase))
                            depth++;
                        else if (line.Equals("End", StringComparison.OrdinalIgnoreCase) || line.Equals("EndProperty", StringComparison.OrdinalIgnoreCase))
                            depth--;
                        i++;
                    }
                    while (i < lines.Count && depth > 0);
                }
            }
        }

        var run = AttributeRunLength(text, lines, i);
        return sawBlock || run > 0 ? i + run : 0;
    }

    /// <summary>
    /// The number of lines, from <paramref name="from"/>, up to and including the last line of the
    /// <c>Attribute</c> run that starts there. Blank lines inside or in front of the run count; blank lines
    /// after it do not, and nor do blank lines with no run after them.
    /// </summary>
    private static int AttributeRunLength(string text, List<Line> lines, int from)
    {
        var lastAttribute = -1;
        for (var i = from; i < lines.Count; i++)
        {
            var line = Trimmed(text, lines[i]);
            if (line.IsEmpty)
                continue;
            if (!StartsWithKeyword(line, "Attribute"))
                break;
            lastAttribute = i;
        }
        return lastAttribute < 0 ? 0 : lastAttribute + 1 - from;
    }

    /// <summary>
    /// True for <c>Attribute Name.VB_Something = …</c>, and false for anything that merely begins with the
    /// word — an assignment to a variable called <c>Attribute</c>, or an array of that name.
    /// </summary>
    private static bool IsMemberAttribute(string text, Line line)
    {
        var rest = Trimmed(text, line);
        if (!StartsWithKeyword(rest, "Attribute"))
            return false;
        rest = rest["Attribute".Length..].TrimStart();

        var name = 0;
        while (name < rest.Length && (char.IsLetterOrDigit(rest[name]) || rest[name] is '_' or '.'))
            name++;
        if (name == 0 || !char.IsLetter(rest[0]))
            return false;

        rest = rest[name..].TrimStart();
        return rest.Length > 0 && rest[0] == '=';
    }

    private static bool StartsWithKeyword(ReadOnlySpan<char> line, string keyword) =>
        line.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)
        && line.Length > keyword.Length
        && char.IsWhiteSpace(line[keyword.Length]);
}

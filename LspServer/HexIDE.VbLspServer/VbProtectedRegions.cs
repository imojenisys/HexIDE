// SPDX-License-Identifier: MIT
// Copyright (C) 2026 The HexIDE Authors

namespace HexIDE.VbLspServer;

/// <summary>
/// The lines of a VB6 file that no answer of this server may change or point into: the file's header, and
/// every member's <c>Attribute</c> lines (hexide-io/HexIDE#273 task 3.10).
/// </summary>
/// <remarks>
/// <para>
/// <b>One helper, read by every answer that could reach them</b> — diagnostics, formatting, rename and
/// highlight — so the four cannot drift apart. Each of them used to know nothing about a header, because
/// until the client started sending the whole file there was none to know about.
/// </para>
/// <para>
/// <b>The header</b> is the top of the file through the last line of the leading <c>Attribute</c> run: a
/// <c>VERSION</c> line, then a class's <c>BEGIN … END</c> block or a form's <c>Object</c> lines and designer
/// block, then the attribute run. A text with none of this has no header. <b>A member's attribute lines</b>
/// are <c>Attribute Name.VB_Something = …</c>, wherever they appear below the header.
/// </para>
/// <para>
/// <b>This is the IDE's rule, ported rather than shared.</b> The IDE's own copy is
/// <c>IDE/HexIDE.Runtime/Serialization/ReadOnlyRegions.cs</c>, and the two halves of the repository do not
/// reference each other, for the same reasons the two grammar copies are separate. The server only ever has
/// the text, so this is that file's text-only branch. <c>BundledServerRespectsTheIdesRegionsTests</c> in
/// <c>HexIDE.Tests</c> drives the built server against the IDE's copy over the corpus, which is what keeps
/// the two in step.
/// </para>
/// <para>
/// <b>Lines, not offsets</b>, because every answer here speaks LSP positions. Lines split on <c>\n</c> with a
/// trailing <c>\r</c> stripped, exactly as <see cref="VbTextHelpers.FindAllOccurrences"/> splits them, so a
/// line number means the same thing to both.
/// </para>
/// </remarks>
internal sealed class VbProtectedRegions
{
    private readonly bool[] protectedLines;
    private readonly HashSet<string> declaredNames;

    private VbProtectedRegions(bool[] protectedLines, int headerLineCount, HashSet<string> declaredNames)
    {
        this.protectedLines = protectedLines;
        this.declaredNames = declaredNames;
        HeaderLineCount = headerLineCount;
    }

    /// <summary>The number of lines the header occupies, from line 0. 0 when the text has no header.</summary>
    public int HeaderLineCount { get; }

    /// <summary>The protected lines of <paramref name="source"/>.</summary>
    public static VbProtectedRegions Of(string source)
    {
        var lines = LinesOf(source);
        var header = HeaderLinesFromText(lines);

        var marks = new bool[lines.Count];
        for (var i = 0; i < lines.Count; i++)
            marks[i] = i < header || IsMemberAttribute(lines[i]);
        return new VbProtectedRegions(marks, header, DeclaredNames(lines, header));
    }

    /// <summary>
    /// True when the header's designer block declares <paramref name="word"/>: the name on a <c>Begin</c> line,
    /// which is the form's own, a control's or a menu's. Compared ignoring case, as VB6 compares names.
    /// </summary>
    /// <remarks>
    /// For rename. Such a name's declaration is in the header, so a rename that keeps off the header would
    /// leave the code referring to a control that no longer has that name. The IDE refuses the same rename
    /// by the same rule (<c>ReadOnlyRegions.DeclaredNames</c>).
    /// </remarks>
    public bool Declares(string word) => declaredNames.Contains(word);

    /// <summary>True when <paramref name="line"/> (0-based) is in the header or is a member's attribute line.</summary>
    public bool IsProtected(int line) => line >= 0 && line < protectedLines.Length && protectedLines[line];

    /// <summary>True when any line <paramref name="range"/> covers is protected.</summary>
    /// <remarks>
    /// A range ending at character 0 of a line stops at the end of the line before it, so it does not cover
    /// the line it ends on.
    /// </remarks>
    public bool Touches(LspRange range)
    {
        var last = range.End.Character == 0 && range.End.Line > range.Start.Line ? range.End.Line - 1 : range.End.Line;
        for (var line = range.Start.Line; line <= last; line++)
        {
            if (IsProtected(line))
                return true;
        }
        return false;
    }

    /// <summary>
    /// True when <paramref name="occurrence"/> of <paramref name="word"/> is the member name in
    /// <c>Attribute word.VB_…</c>: the one place inside a protected line that a rename changes.
    /// </summary>
    /// <remarks>
    /// When <c>Total</c> is renamed to <c>GrandTotal</c>, <c>Attribute Total.VB_Description</c> has to follow,
    /// or the description is left describing a member that no longer exists. The IDE lets exactly this edit
    /// through its own guard for the same reason (<c>CodeEditorViewModel.IsOwnAttributeQualifier</c>).
    /// </remarks>
    public static bool IsOwnQualifier(string lineText, LspRange occurrence, string word)
    {
        var start = occurrence.Start.Character;
        var end = occurrence.End.Character;
        if (occurrence.Start.Line != occurrence.End.Line || end - start != word.Length
            || end >= lineText.Length || lineText[end] != '.')
            return false;
        return lineText.AsSpan(0, start).Trim().Equals("Attribute", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// <paramref name="source"/> with every protected line emptied and every line break kept, so each line of
    /// code keeps the line and column it had.
    /// </summary>
    /// <remarks>
    /// For the parse that stands in when the grammar cannot read a protected line
    /// (<c>VbDiagnosticsProvider.ParseSource</c>). <paramref name="source"/> must be the text these regions
    /// were computed from.
    /// </remarks>
    public string Blank(string source)
    {
        var result = new System.Text.StringBuilder(source.Length);
        var line = 0;
        var pos = 0;
        while (pos < source.Length)
        {
            var newline = source.IndexOf('\n', pos);
            var next = newline < 0 ? source.Length : newline + 1;
            var end = newline < 0 ? source.Length : newline;
            if (end > pos && source[end - 1] == '\r')
                end--;

            if (!IsProtected(line))
                result.Append(source, pos, end - pos);
            result.Append(source, end, next - end);

            pos = next;
            line++;
        }
        return result.ToString();
    }

    // ── The rule, ported from ReadOnlyRegions' text-only branch ──────────────────────────────────────────

    private static HashSet<string> DeclaredNames(List<string> lines, int headerLines)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headerLines; i++)
        {
            var line = lines[i].AsSpan().Trim();
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

    private static List<string> LinesOf(string source)
    {
        var lines = new List<string>();
        var pos = 0;
        while (pos < source.Length)
        {
            var newline = source.IndexOf('\n', pos);
            var end = newline < 0 ? source.Length : newline;
            var next = newline < 0 ? source.Length : newline + 1;
            if (end > pos && source[end - 1] == '\r')
                end--;
            lines.Add(source[pos..end]);
            pos = next;
        }
        return lines;
    }

    /// <summary>
    /// A <c>VERSION</c> line and the class or designer block after it, then the leading attribute run.
    /// </summary>
    /// <remarks>
    /// The designer block is walked by nesting, because a form's controls are nested <c>Begin</c>/<c>End</c>
    /// blocks and its fonts and images nested <c>BeginProperty</c>/<c>EndProperty</c> ones.
    /// </remarks>
    private static int HeaderLinesFromText(List<string> lines)
    {
        var i = 0;
        while (i < lines.Count && lines[i].AsSpan().Trim().IsEmpty)
            i++;

        var sawBlock = false;
        if (i < lines.Count && StartsWithKeyword(lines[i].AsSpan().Trim(), "VERSION"))
        {
            sawBlock = true;
            i++;
            if (i < lines.Count && lines[i].AsSpan().Trim().Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                while (i < lines.Count && !lines[i].AsSpan().Trim().Equals("END", StringComparison.OrdinalIgnoreCase))
                    i++;
                if (i < lines.Count)
                    i++;
            }
            else
            {
                while (i < lines.Count && StartsWithKeyword(lines[i].AsSpan().Trim(), "Object"))
                    i++;
                if (i < lines.Count && StartsWithKeyword(lines[i].AsSpan().Trim(), "Begin"))
                {
                    var depth = 0;
                    do
                    {
                        var line = lines[i].AsSpan().Trim();
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

        var run = AttributeRunLength(lines, i);
        return sawBlock || run > 0 ? i + run : 0;
    }

    /// <summary>
    /// The number of lines from <paramref name="from"/> up to and including the last line of the
    /// <c>Attribute</c> run that starts there. Blank lines inside or in front of the run count; blank lines
    /// after it do not.
    /// </summary>
    private static int AttributeRunLength(List<string> lines, int from)
    {
        var lastAttribute = -1;
        for (var i = from; i < lines.Count; i++)
        {
            var line = lines[i].AsSpan().Trim();
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
    private static bool IsMemberAttribute(string text)
    {
        var rest = text.AsSpan().Trim();
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

using System;
using System.Collections.Generic;
using HexIDE.Runtime.ProjectElements;

namespace HexIDE.Runtime.Serialization;

/// <summary>
/// The text of a form's <b>code section</b> — everything a <c>.frm</c> holds after its designer block.
/// </summary>
/// <remarks>
/// <para>A form's code section opens with an <c>Attribute VB_*</c> block that VB6 hides. HexIDE's editor
/// <i>does</i> show it — measured in the running IDE on 2026-09-20, where it is the opening lines of
/// the code window, syntax-coloured as ordinary code and freely editable. <c>VB_Name</c> is load-bearing: it is the
/// form's identity. Being on screen is not the same as being written back: anyone composing "the code" of a
/// form writes the part they came to write, and a straight replacement then deletes that block with no
/// warning — which happened, and reached a commit before <c>git diff</c> caught it.</para>
///
/// <para>The mirror mistake is passing a whole <c>.frm</c> instead, which puts <c>VERSION</c> /
/// <c>Begin VB.Form</c> into the code where it is compiled as VB.</para>
/// </remarks>
public static class FormCodeText
{
    /// <summary>
    /// True when this is a whole <c>.frm</c> file rather than the code section of one.
    /// </summary>
    /// <remarks>
    /// Both signals are required. A lone <c>VERSION</c> line is not proof — a form's code could plausibly
    /// begin with an identifier of that name — while <c>VERSION</c> followed by a <c>Begin</c> block is the
    /// shape of the file and of nothing else.
    /// </remarks>
    public static bool LooksLikeFormFile(string content)
    {
        var sawVersion = false;
        foreach (var raw in Lines(content))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (!sawVersion)
            {
                if (!line.StartsWith("VERSION", StringComparison.OrdinalIgnoreCase)) return false;
                sawVersion = true;
                continue;
            }
            return line.StartsWith("Begin ", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    /// <summary>
    /// The leading <c>Attribute</c> block of a code section, with its line endings, or <c>""</c> if absent.
    /// </summary>
    /// <remarks>
    /// Leading blank lines are carried into the block so restoring it cannot introduce or lose one. Only a
    /// LEADING run counts: an <c>Attribute</c> line further down belongs to a procedure and is not part of
    /// the file's identity.
    /// </remarks>
    public static string AttributeBlock(string code)
    {
        var end = 0;
        var pos = 0;
        foreach (var raw in Lines(code))
        {
            var lineLength = raw.Length + 1;   // the split ate one '\n'
            var line = raw.Trim();
            if (line.Length == 0) { pos += lineLength; continue; }
            if (!line.StartsWith("Attribute ", StringComparison.OrdinalIgnoreCase)) break;
            pos += lineLength;
            end = pos;
        }
        return end == 0 ? "" : code[..Math.Min(end, code.Length)];
    }

    /// <summary>
    /// <paramref name="incoming"/> with <paramref name="existing"/>'s attribute block restored when the
    /// incoming text has none of its own. Returns <paramref name="incoming"/> unchanged otherwise.
    /// </summary>
    /// <remarks>
    /// Preserving rather than refusing, because omitting the block is what a caller does when they mean
    /// exactly what they said — "replace the code" — and the block is not code. The alternative, silently
    /// accepting a body that destroys the form's identity, is the one behaviour that should not exist.
    /// </remarks>
    public static string PreserveAttributes(string incoming, string existing)
    {
        if (AttributeBlock(incoming).Length > 0) return incoming;

        var block = AttributeBlock(existing);
        return block.Length == 0 ? incoming : block + incoming;
    }

    /// <summary>
    /// The whole file this form represents: its designer half, then its code section.
    /// </summary>
    /// <remarks>
    /// <b>The composition phase 3 of hexide-io/HexIDE#273 rests on, and the invariant is exact</b>: for a
    /// form read from disk and not since modified, this returns the file byte for byte, terminators
    /// included. Both halves are slices of the text that was read.
    ///
    /// <para>
    /// A form HexIDE created has no designer text, so this is its code alone — which is also right, because
    /// there is no file yet for it to differ from. The same is true of a <c>.ctl</c> or <c>.pag</c> whose
    /// designer block could not be parsed: nothing was split off, so nothing is put back.
    /// </para>
    ///
    /// <para>
    /// Deliberately NOT a re-render from <c>Components</c>. A re-render is what a save produces and it is a
    /// reproduction, not the text: the <c>VERSION</c> line comes from a literal and the block's
    /// <c>Begin</c>/<c>End</c> are rebuilt at a computed indent. Composing from a render would show the
    /// developer a file subtly unlike the one on disk.
    /// </para>
    /// </remarks>
    public static string WholeFile(FormDefinition form) => Prefix(form) + form.Code;

    /// <summary>
    /// What the code window puts in front of this form's code: its designer half, or nothing.
    /// </summary>
    /// <remarks>
    /// <b>The prefix is not the protected region, and conflating them corrupts a file in either
    /// direction.</b> The region read-only protection covers is the top of the file through the last line
    /// of the leading <c>Attribute</c> run, which for a form straddles this boundary — part of it is the
    /// prefix and part of it is the first lines of <c>Code</c>. Prepending the attribute run here would
    /// show it twice; splitting after it would take <c>VB_Name</c> out of <c>Code</c> and write a form
    /// without one. They answer different questions and are computed separately.
    /// </remarks>
    public static string Prefix(FormDefinition form) => form.DesignerText ?? "";

    /// <summary>What the code window puts in front of this module's code.</summary>
    /// <remarks>
    /// A <c>.ctl</c> or <c>.pag</c> takes its designer half from the <c>FormPart</c>; everything else takes
    /// the module header, whose null-versus-empty rule is argued at
    /// <see cref="ModuleFileFormat.BufferHeader"/>.
    /// </remarks>
    public static string Prefix(ModuleDefinition module) =>
        ModuleFileFormat.HandlesHeader(module.Kind)
            ? ModuleFileFormat.BufferHeader(module.Name, module.Kind, module.OriginalHeader)
            : module.FormPart?.DesignerText ?? "";

    /// <summary>
    /// The body of a composed buffer: everything after the prefix it was composed with.
    /// </summary>
    /// <remarks>
    /// <b>Split by the prefix's LENGTH, and by the prefix the buffer actually carries</b> rather than the
    /// one the model would produce now. The two diverge the moment a document is renamed — a module called
    /// <c>Utilities</c> has a longer <c>VB_Name</c> line than one called <c>Mod1</c> — and splitting at the
    /// model's current length would then cut into the body or leave part of the header in it. The buffer's
    /// own prefix is authoritative until something refreshes both together.
    /// </remarks>
    public static string BodyOf(string buffer, string prefix) =>
        prefix.Length > 0 && buffer.Length >= prefix.Length ? buffer[prefix.Length..] : buffer;

    /// <summary>
    /// The designer half of a rendered form file: everything <c>FormSerializer</c> wrote before the code it
    /// was given.
    /// </summary>
    /// <remarks>
    /// <b>The inverse of the render, taken by length, exactly as <see cref="BodyOf"/> takes the body.</b>
    /// The serializer appends the code verbatim as its last write, with no separator in front of it, so the
    /// remainder is the designer half and is the same span <c>FormDeserializer</c> records when it reads a
    /// file. <c>DesignerHalfIsTheRenderMinusTheCode</c> pins that, because a separator introduced there
    /// later would move this split silently and put a stray line into the code window on every save.
    /// </remarks>
    public static string DesignerHalfOf(string rendered, string code) =>
        code.Length > 0 && rendered.Length >= code.Length ? rendered[..^code.Length] : rendered;

    /// <summary>
    /// The whole file this module represents, whichever kind it is.
    /// </summary>
    /// <remarks>
    /// Two different shapes behind one question, which is the point of having it in one place. A
    /// <c>.bas</c> or <c>.cls</c> composes through <see cref="ModuleFileFormat.ToFileContent"/>, so it picks
    /// up the preserved header or the canonical literal. A <c>.ctl</c> or <c>.pag</c> does not: its header
    /// is a designer block, <see cref="ModuleFileFormat.HandlesHeader"/> is false for those kinds and
    /// <c>ToFileContent</c> would hand the body straight back, so it composes from the <c>FormPart</c> that
    /// carries the designer text with the MODULE's code beside it — which is the half the save path writes
    /// for those kinds too.
    /// </remarks>
    public static string WholeFile(ModuleDefinition module) => Prefix(module) + module.Code;

    private static IEnumerable<string> Lines(string text)
    {
        foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
            yield return line;
    }
}

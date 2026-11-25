using HexIDE.Runtime.ProjectElements;

namespace HexIDE.Language;

/// <summary>Length-based splits and names for composed code buffers.</summary>
internal static class CodeBufferText
{
    public static int LineBreaks(string text) => SourceLines.Count(text);

    public static string BodyOf(string buffer, string prefix) =>
        prefix.Length > 0 && buffer.Length >= prefix.Length ? buffer[prefix.Length..] : buffer;

    public static string DesignerHalfOf(string rendered, string code) =>
        code.Length > 0 && rendered.Length >= code.Length ? rendered[..^code.Length] : rendered;

    public static string? RenderFileNameFor(FormDefinition form)
    {
        var identity = DocumentIdentity.For(form);
        if (identity.AbsolutePath is { } path)
            return System.IO.Path.GetFileName(path);
        return identity.Name.Length == 0 ? null : identity.Name + identity.Extension;
    }
}

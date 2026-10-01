namespace HexIDE.Addins;

public enum AddinDocumentKind { Form, Module, UserControl, Other }

public enum AddinDiagnosticSeverity { Error = 1, Warning = 2, Information = 3, Hint = 4 }

/// <param name="Content">
/// The document's code, without the header the code window shows above it. <b>Not yet the text every position
/// on this surface counts in</b>: those count from the top of the file, header included, so a line found by
/// reading this is out by the header's length. Content becomes the whole file in hexide-io/HexIDE#273 task
/// 3.19, with its own contract note, rather than changing here as a side effect.
/// </param>
/// <param name="Project">
/// The project holding this document, by name. Trailing and optional so an add-in built against an earlier
/// version keeps compiling, on the rule this file already records for a diagnostic's Code and Source: the
/// IDE builds these records and add-ins read them, so nothing outside the IDE constructs one positionally.
/// </param>
public record AddinDocument(string FileName, string FilePath, string Content, AddinDocumentKind Kind,
    string? Project = null);

/// <param name="Project">
/// The project holding this document, by name. Trailing and optional so an add-in built against an earlier
/// version keeps compiling, on the rule this file already records for a diagnostic's Code and Source: the
/// IDE builds these records and add-ins read them, so nothing outside the IDE constructs one positionally.
/// </param>
/// <remarks>
/// Positions are 1-based, and lines count from the top of the file with its header included (a form's designer
/// block, a class's header and its Attribute lines), as the code window numbers them.
/// </remarks>
public record AddinSelection(
    string FileName,
    string SelectedText,
    int StartLine, int StartColumn,
    int EndLine, int EndColumn,
    string? Project = null);

/// <summary>
/// A text replacement. All positions are 1-based, and lines count from the top of the file with its header
/// included, as the code window numbers them.
/// </summary>
public record AddinTextEdit(
    int StartLine, int StartColumn,
    int EndLine, int EndColumn,
    string NewText);

/// <param name="Project">
/// The project holding this document, by name. Trailing and optional so an add-in built against an earlier
/// version keeps compiling, on the rule this file already records for a diagnostic's Code and Source: the
/// IDE builds these records and add-ins read them, so nothing outside the IDE constructs one positionally.
/// </param>
public record AddinFileInfo(string FileName, string FilePath, AddinDocumentKind Kind, string? Project = null);

public record AddinProjectInfo(string ProjectName, string ProjectPath, IReadOnlyList<AddinFileInfo> Files);

/// <remarks>
/// <b><c>Code</c> and <c>Source</c> are trailing and optional deliberately.</b> This type is built by the
/// IDE and read by add-ins, so adding to the end leaves every existing add-in compiling and running
/// unchanged, while an add-in that wants the rule that fired — or which of several servers reported it —
/// can now ask (hexide-io/HexIDE#426).
/// <para>
/// <c>Line</c> and <c>Column</c> are 1-based, and the line counts from the top of the file with its header
/// included, as the code window numbers it.
/// </para>
/// </remarks>
public record AddinDiagnostic(
    string FileName,
    int Line, int Column,
    string Message,
    AddinDiagnosticSeverity Severity,
    string? Code = null,
    string? Source = null,
    string? Project = null);

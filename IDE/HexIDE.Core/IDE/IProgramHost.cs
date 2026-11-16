using System.Threading.Tasks;

namespace HexIDE.IDE;

/// <summary>The host's dialogs and failure texts for a running program (#4066).</summary>
public interface IProgramHost
{
    Task<VbFileDialogAnswer?> FileDialog(VbFileDialogRequest request);
    Task<bool> PrintDialog();
    Task<int?> ColorDialog(int initial);
    Task<VbFontChoice?> FontDialog(VbFontChoice initial);

    Task<MessageBoxResult> MsgBox(string text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxLayout layout);
    Task<string?> InputBox(string prompt, string? title, string defaultText);

    string StartupFormFailedToLoad(string formName, string description);
    string NotImplementedYet { get; }
}

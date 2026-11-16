using System;
using System.Threading.Tasks;

namespace HexIDE.IDE;

public sealed record HostedFormRequest(string Caption, double Width, double Height, double Left, double Top);

public enum HostedFormState { Normal, Minimized, Maximized, FullScreen }

/// <summary>The host's window for one program form (#4068). Only this form's own window crosses the seam.</summary>
public interface IHostedForm
{
    object Window { get; }
    void SetContent(object content);
    Task Show(bool blockBehind);
    void Hide();
    void Close();
    void Activate();
    bool IsShown { get; }
    bool IsHeld { get; set; }
    double Width { get; set; }
    double Height { get; set; }
    (double Width, double Height, HostedFormState State) Extent { get; }
    event Action? Resized;
    event Action? SizeSet;
    event Action? Shown;
    event Action? Activated;
    event Action? HeldPressed;
    // True refuses the close, as UnloadFromCloseBox does.
    Func<bool>? Closing { get; set; }
}

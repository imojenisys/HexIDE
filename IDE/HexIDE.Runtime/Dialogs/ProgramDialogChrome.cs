using System;
using Avalonia.Controls;

namespace HexIDE.Runtime.Dialogs;

internal static class ProgramDialogChrome
{
    // System MessageBox styles measured in #4099; own only the caption buttons and modal frame.
    internal static (uint Style, uint ExStyle) WindowsStyles(uint style, uint exStyle) =>
        (style & ~0x00030000u, exStyle | 0x00000001u);

    internal static void Apply(Window window)
    {
        // An embedded dialog does not own its host window's caption.
        if (window.Content is not MessageBox { IsProgramDialog: true } and not InputBox { IsProgramDialog: true })
            return;
        if (OperatingSystem.IsWindows())
            Win32Properties.AddWindowStylesCallback(window, WindowsStyles);
        window.CanMinimize = false;
        window.CanMaximize = false;
    }
}

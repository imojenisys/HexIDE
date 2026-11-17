using Avalonia.Controls;
using Avalonia.Styling;

namespace HexIDE.Runtime.AvaloniaInterop;

public static class ProgramAppearance
{
    // Interim counterpart of the designer's Light scope; platform skins (#1137) replace this (#909).
    public static void Apply(Window window) => window.RequestedThemeVariant = ThemeVariant.Light;
}

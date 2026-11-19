using System.Runtime.InteropServices;
using Avalonia.Media;

namespace HexIDE.Runtime.BuiltinControls;

internal static partial class ToolbarCaptionMetrics
{
    internal static (Typeface Typeface, double Size) MessageFont()
    {
        // NONCLIENTMETRICSW contains five 92-byte LOGFONTW records; lfMessageFont is last.
        var metrics = new byte[504];
        BitConverter.GetBytes(metrics.Length).CopyTo(metrics, 0);
        // Query at 96 DPI: these extents feed Avalonia DIPs, not device pixels (#4802).
        if (!SystemParametersInfoForDpi(0x29, (uint)metrics.Length, metrics, 0, 96))
            throw new Win32Exception();
        var face = System.Text.Encoding.Unicode.GetString(metrics, 436, 64).TrimEnd('\0');
        return (new Typeface(face, metrics[428] != 0 ? FontStyle.Italic : FontStyle.Normal,
            (FontWeight)BitConverter.ToInt32(metrics, 424)), Math.Abs(BitConverter.ToInt32(metrics, 408)));
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfoForDpi(uint action, uint parameter, [In, Out] byte[] metrics, uint flags, uint dpi);

    // #2334: GDI's whole-string extent differs from summed Skia advances.
    internal static double Measure(string caption, Typeface typeface, double size)
    {
        var dc = CreateCompatibleDC(IntPtr.Zero);
        if (dc == IntPtr.Zero) throw new Win32Exception();
        IntPtr font = IntPtr.Zero, previous = IntPtr.Zero;
        try
        {
            font = CreateFontW(-(int)Math.Round(size), 0, 0, 0, (int)typeface.Weight,
                typeface.Style == FontStyle.Italic ? 1u : 0u, 0, 0, 1, 0, 0, 0, 0,
                typeface.FontFamily.Name);
            if (font == IntPtr.Zero) throw new Win32Exception();
            previous = SelectObject(dc, font);
            if (previous == IntPtr.Zero || previous == new IntPtr(-1))
                throw new Win32Exception();
            if (!GetTextExtentPoint32W(dc, caption, caption.Length, out var extent))
                throw new Win32Exception();
            return extent.Width;
        }
        finally
        {
            if (previous != IntPtr.Zero && previous != new IntPtr(-1)) SelectObject(dc, previous);
            if (font != IntPtr.Zero) DeleteObject(font);
            DeleteDC(dc);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Extent { public int Width; public int Height; }

    [LibraryImport("gdi32.dll", SetLastError = true)]
    private static partial IntPtr CreateCompatibleDC(IntPtr dc);
    [LibraryImport("gdi32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr CreateFontW(int height, int width, int escapement, int orientation,
        int weight, uint italic, uint underline, uint strikeOut, uint charSet, uint outputPrecision,
        uint clipPrecision, uint quality, uint pitchAndFamily, string face);
    [LibraryImport("gdi32.dll", SetLastError = true)]
    private static partial IntPtr SelectObject(IntPtr dc, IntPtr value);
    [LibraryImport("gdi32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetTextExtentPoint32W(IntPtr dc, string text, int count, out Extent extent);
    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(IntPtr value);
    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(IntPtr dc);
}

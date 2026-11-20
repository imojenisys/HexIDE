using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace HexIDE.Runtime.BuiltinControls;

/// <summary>The control mirrored by a running form's native menu item.</summary>
public static class NativeMenuSource
{
    private static readonly ConditionalWeakTable<NativeMenuItemBase, Control> sources = new();

    public static Control? Of(NativeMenuItemBase item) => sources.TryGetValue(item, out var source) ? source : null;

    internal static void Register(NativeMenuItemBase item, Control source) => sources.Add(item, source);
    internal static void Remove(NativeMenuItemBase item) => sources.Remove(item);
}

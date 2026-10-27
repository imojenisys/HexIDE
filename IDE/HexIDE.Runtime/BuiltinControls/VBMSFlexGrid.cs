using Avalonia.Controls;
using HexIDE.Runtime.Components.InBox;

namespace HexIDE.Runtime.BuiltinControls;

// Runtime state only; input and drawing are later slices of #1099.
public sealed class VBMSFlexGrid : Control
{
    internal FlexGridModel Model { get; } = new();
}

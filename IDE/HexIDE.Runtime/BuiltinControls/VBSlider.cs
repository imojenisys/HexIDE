using Avalonia.Controls;
using HexIDE.Runtime.AvaloniaInterop;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.Components.InBox;
using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime.BuiltinControls;

// The object model only (#1091 A); input and drawing follow in part B.
public sealed class VBSlider : Control
{
    internal long Min, Max = 10, Value, SmallChange = 1, LargeChange = 5, TickFrequency = 1;
    internal long TickStyle, Orientation, TextPosition, BorderStyle, OLEDropMode;
    internal long SelectionStart, SelectionLength;
    internal bool SelectRange;
    internal string Text = "";

    public VBSlider() => Focusable = true;

    internal long SelStart => SelectRange ? SelectionStart : Value;
    internal long SelLength => SelectRange ? SelectionLength : 0;
    internal long GetNumTicks => Max <= Min || TickFrequency == 0 ? 2 : (Max - Min - 1) / TickFrequency + 2;

    internal void SetValue(long value, bool change)
    {
        var clamped = System.Math.Max(Min, System.Math.Min(Max, value));
        if (Value == clamped)
            return;
        Value = clamped;
        if (change)
            this.ExecuteSub(SliderComponentClass.ChangeEvent);
    }

    internal void SetRange(long number, bool minimum)
    {
        if (minimum ? number >= Max : number <= Min)
            throw new VBRunTimeException(VBStandardError.InvalidPropertyValue);
        if (minimum)
            Min = number;
        else
        {
            Max = number;
            if (SelectRange)
                SelectionLength = System.Math.Min(SelectionLength, Max - SelectionStart);
        }
        SetValue(Value, false);
    }
}

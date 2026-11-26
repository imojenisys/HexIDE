using HexIDE.Controls;
using HexIDE.Runtime.BuiltinTypes;

namespace HexIDE.Integration.Tests.Controls;

/// <summary>
/// The Properties window uses the names VB6 writes when saving a form with each of the 17 MousePointer values.
/// See the oracle's mousepointers-names-and-pointers.md (#120).
/// </summary>
public class MousePointerValuesTests
{
    [AvaloniaFact]
    public void The_Properties_window_offers_all_seventeen_values_by_VB6s_names()
    {
        new PropertyEnumBox { PropertyType = typeof(VBCursorType) }.Options!.Select(o => o.Text).Should().Equal(
            "0 - Default", "1 - Arrow", "2 - Cross", "3 - I-Beam", "4 - Icon", "5 - Size", "6 - Size NE SW", "7 - Size N S",
            "8 - Size NW SE", "9 - Size W E", "10 - Up Arrow", "11 - Hourglass", "12 - No Drop", "13 - Arrow and Hourglass",
            "14 - Arrow and Question", "15 - Size All", "99 - Custom");
    }
}

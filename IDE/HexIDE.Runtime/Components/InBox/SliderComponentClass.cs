using Avalonia.Controls;
using HexIDE.Runtime.BuiltinControls;
using static HexIDE.Runtime.Components.VBProperties;

namespace HexIDE.Runtime.Components.InBox;

public sealed class SliderComponentClass : InBoxComponentClass
{
    private SliderComponentClass() : base([TabIndexProperty, TabStopProperty, ToolTipTextProperty], [],
        [ChangeEvent, ScrollEvent, ClickEvent]) { }

    public static readonly EventClass ChangeEvent = new("Change");
    public static readonly EventClass ScrollEvent = new("Scroll");
    public static SliderComponentClass Instance { get; } = new();
    public override string Name => "Slider";
    public override string ControlClassName => "Slider";
    public override IEnumerable<RuntimeLibrary> Libraries => [ControlLibraries.MSComctlLib];
    public override (int Width, int Height) AddedSize => (1500, 675);
    public override (int X, int Y) DefaultExtent => throw new NotImplementedException("Slider missing extents are not measured (#1091).");
    public override bool CanSaveFaithfully => false;

    // Persistence and user events remain for parts C and B respectively.
    public override IReadOnlyList<InBoxMember> Members { get; } =
        OwnMembers("Min", "Max", "Value", "SmallChange", "LargeChange", "TickStyle", "TickFrequency", "GetNumTicks",
            "Orientation", "SelectRange", "SelStart", "SelLength", "Text", "TextPosition", "BorderStyle", "Enabled",
            "MousePointer", "OLEDropMode")
        .Concat(new[] { "ClearSel", "Refresh" }.Select(n => new InBoxMember(n, MemberKind.Method, MemberOrigin.Version6, false, true)))
        .Concat(new[] { "hWnd", "MouseIcon" }.Select(n => new InBoxMember(n, MemberKind.Property, MemberOrigin.Version6, false, false)))
        .Append(new("OLEDrag", MemberKind.Method, MemberOrigin.Version6, false, false))
        .Concat(new[] { "Change", "Scroll", "Click", "KeyDown", "KeyPress", "KeyUp", "MouseDown", "MouseMove", "MouseUp",
            "OLEStartDrag", "OLEGiveFeedback", "OLESetData", "OLECompleteDrag", "OLEDragOver", "OLEDragDrop" }
            .Select(n => new InBoxMember(n, MemberKind.Event, MemberOrigin.Version6, false, n == "Change"))).ToArray();

    private static IEnumerable<InBoxMember> OwnMembers(params string[] names) =>
        names.Select(n => new InBoxMember(n, MemberKind.Property, MemberOrigin.Version6, n is not ("Text" or "GetNumTicks"), true));

    protected override Control InstantiateInternal(ComponentInstance instance)
    {
        if (LibraryOf(instance) != ControlLibraries.MSComctlLib)
            throw new NotImplementedException("ComctlLib Slider behaviour is not implemented (#1091): unmeasured.");
        if (instance.UnknownRawProperties.Any(p => p.Name is not ("_ExtentX" or "_ExtentY" or "_Version")))
            throw new NotImplementedException("Loading Slider properties is not implemented until #1091 C.");
        var slider = new VBSlider();
        AttachedProperties.SetMousePointer(slider, BuiltinTypes.VBCursorType.Default);
        return slider;
    }

    public override Control InstantiateForDesigner(ComponentInstance instance) => new Control();
}

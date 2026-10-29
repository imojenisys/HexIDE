using Avalonia.Media;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.Components.InBox;
using HexIDE.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HexIDE.VisualDesigner;

public partial class ComponentToolViewModel : ObservableObject
{
    private readonly ToolBoxToolViewModel parent;

    public ComponentToolViewModel(ToolBoxToolViewModel parent, ComponentBaseClass baseClass, IImage icon)
    {
        this.parent = parent;
        BaseClass = baseClass;
        Icon = icon;
        Name = baseClass.Name;
    }

    public ComponentToolViewModel(ToolBoxToolViewModel parent, string name, IImage icon)
    {
        this.parent = parent;
        Icon = icon;
        Name = name;
    }

    public ComponentToolViewModel(ToolBoxToolViewModel parent, IComponentClass component, IImage icon)
    {
        this.parent = parent;
        BaseClass = component as ComponentBaseClass;
        Icon = icon;
        Name = component.Name;
    }

    public ComponentBaseClass? BaseClass { get; }

    /// <summary>The library an in-box control is placed from, which its type name and its form's header name (#1103).</summary>
    public RuntimeLibrary? Library { get; init; }

    /// <summary>The type name a control placed from this button takes: <c>ComctlLib.ProgressBar</c> from COMCTL32.OCX's.</summary>
    public string? VBTypeName =>
        Library is { } library && BaseClass is InBoxComponentClass inBox ? library.TypeName(inBox.ControlClassName) : BaseClass?.VBTypeName;
    public IImage Icon { get; }
    public string Name { get; }

    /// <summary>
    /// The button's name for a screen reader and the automation tree: its control's class, <c>CommandButton</c> from
    /// <c>VB.CommandButton</c>, the name code uses, where <see cref="Name"/> is the prefix a new control's name takes
    /// (#2751). A caller looking for the CommandButton found nothing. The tip still shows <see cref="Name"/> until
    /// VB6's own is measured.
    /// </summary>
    public string AutomationName => VBTypeName is { } typeName && typeName.LastIndexOf('.') is var dot and >= 0
        ? typeName[(dot + 1)..]
        : Name;

    public bool IsSelected
    {
        get => parent.SelectedComponent == this;
        set
        {
            if (value)
                parent.SelectedComponent = this;
            else
                parent.SelectedComponent = parent.Components[0];
        }
    }

    public void RaiseIsSelected() => OnPropertyChanged(nameof(IsSelected));
}
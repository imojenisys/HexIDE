using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Runtime.Components;
using HexIDE.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace HexIDE.VisualDesigner;

public partial class MenuEditorViewModel : ObservableObject, IDialog
{
    [Notify] private MenuViewModel? selectedMenu;

    public ObservableCollection<MenuViewModel> FlatMenu { get; } = new();

    public List<MenuViewModel> Deleted { get; } = new();

    private MenuViewModel blankMenu = null!;
    private readonly List<(MenuViewModel Menu, int Indent)> originalItems = new();
    internal bool HasLayoutChanges => !ItemsToApply.Select(menu => (menu, menu.Indent)).SequenceEqual(originalItems);
    internal bool HasChanges => HasLayoutChanges || ItemsToApply.Any(menu => menu.HasChanges);
    internal IEnumerable<MenuViewModel> ItemsToApply => FlatMenu.Where(menu => menu != blankMenu);

    public MenuEditorViewModel()
    {
        AddBlankRow();
        SelectedMenu = blankMenu;
    }

    public MenuEditorViewModel(FormEditViewModel form, ILocalizationService? localization = null)
    {
        Title = localization?.GetString("Str.Dialog.MenuEditor.Title") ?? Title;

        void AddRecur(ComponentInstance menu, int level)
        {
            var vm = new MenuViewModel(menu);
            vm.Indent = level;
            FlatMenu.Add(vm);
            originalItems.Add((vm, level));
            if (menu.GetPropertyOrDefault(MenuComponentClass.SubItemsProperty) is { } subItems)
            {
                foreach (var subItem in subItems)
                    AddRecur(subItem, level + 1);
            }
        }
        foreach (var menu in form.TopLevelMenu)
            AddRecur(menu.Instance, 0);
        AddBlankRow();
        SelectedMenu = blankMenu;
    }

    private void AddBlankRow()
    {
        blankMenu = new MenuViewModel();
        blankMenu.PropertyChanged += OnBlankCaptionChanged;
        FlatMenu.Add(blankMenu);
    }

    private void OnBlankCaptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MenuViewModel.Caption) || string.IsNullOrEmpty(blankMenu.Caption))
            return;

        // The caption promotes this row; the next blank starts at level zero until Next reaches it (#4141).
        blankMenu.PropertyChanged -= OnBlankCaptionChanged;
        AddBlankRow();
    }

    public void Accept()
    {
        CloseRequested?.Invoke(true);
    }

    public void Cancel()
    {
        CloseRequested?.Invoke(false);
    }

    public void Next()
    {
        if (selectedMenu == null)
            return;

        var indexOf = FlatMenu.IndexOf(selectedMenu) + 1;
        if (indexOf >= FlatMenu.Count)
            indexOf = 0;

        var next = FlatMenu[indexOf];
        if (next == blankMenu)
            next.Indent = selectedMenu.Indent;
        SelectedMenu = next;
    }

    public void Insert()
    {
        var indexOf = selectedMenu == null ? Math.Max(0, FlatMenu.Count - 1) : FlatMenu.IndexOf(selectedMenu) + 1;
        indexOf = Math.Min(indexOf, FlatMenu.Count - 1);
        var newMenu = new MenuViewModel();
        FlatMenu.Insert(indexOf, newMenu);
        newMenu.Indent = selectedMenu?.Indent ?? 0;
        SelectedMenu = newMenu;
    }

    public void Delete()
    {
        if (selectedMenu == null || selectedMenu == blankMenu)
            return;

        var indexOf = FlatMenu.IndexOf(selectedMenu);
        FlatMenu.RemoveAt(indexOf);
        indexOf = Math.Min(indexOf, FlatMenu.Count - 1);
        Deleted.Add(selectedMenu);
        if (indexOf >= 0)
            SelectedMenu = FlatMenu[indexOf];
        else
            SelectedMenu = null;
    }

    public void MoveLeft()
    {
        if (selectedMenu == null)
            return;

        selectedMenu.Indent = Math.Max(0, selectedMenu.Indent - 1);
    }

    public void MoveRight()
    {
        if (selectedMenu == null)
            return;

        selectedMenu.Indent = selectedMenu.Indent + 1;
    }

    public void MoveUp()
    {
        if (selectedMenu == null || selectedMenu == blankMenu)
            return;

        var indexOf = FlatMenu.IndexOf(selectedMenu);
        if (indexOf > 0)
        {
            FlatMenu.Move(indexOf, indexOf - 1);
        }
    }

    public void MoveDown()
    {
        if (selectedMenu == null)
            return;

        var indexOf = FlatMenu.IndexOf(selectedMenu);
        if (indexOf < FlatMenu.Count - 2)
        {
            FlatMenu.Move(indexOf, indexOf + 1);
        }
    }

    public string Title { get; } = "Menu Editor";
    public bool CanResize => false;
    public event Action<bool>? CloseRequested;
}

public partial class MenuViewModel : ObservableObject
{
    public ComponentInstance? Menu { get; }
    [Notify] private string caption;
    [Notify] private string name;
    [Notify] private bool isChecked;
    [Notify] private bool isEnable = true;
    [Notify] private bool isVisible = true;
    [Notify] private bool isWindowsList;
    [Notify] private int indent;

    public MenuViewModel()
    {
        caption = "";
        name = "";
    }

    public MenuViewModel(ComponentInstance menu)
    {
        Menu = menu;
        caption = menu.GetPropertyOrDefault(VBProperties.CaptionProperty) ?? "";
        name = menu.GetPropertyOrDefault(VBProperties.NameProperty) ?? "";
        isChecked = menu.GetPropertyOrDefault(VBProperties.CheckedProperty);
        isEnable = menu.GetPropertyOrDefault(VBProperties.EnabledProperty);
        isVisible = menu.GetPropertyOrDefault(VBProperties.VisibleProperty);
        isWindowsList = menu.GetPropertyOrDefault(VBProperties.WindowListProperty);
    }

    internal bool HasChanges => Menu is null
        || caption != (Menu.GetPropertyOrDefault(VBProperties.CaptionProperty) ?? "")
        || name != (Menu.GetPropertyOrDefault(VBProperties.NameProperty) ?? "")
        || isEnable != Menu.GetPropertyOrDefault(VBProperties.EnabledProperty)
        || isVisible != Menu.GetPropertyOrDefault(VBProperties.VisibleProperty)
        || isChecked != Menu.GetPropertyOrDefault(VBProperties.CheckedProperty)
        || isWindowsList != Menu.GetPropertyOrDefault(VBProperties.WindowListProperty);

    public void Apply(ComponentInstance menu)
    {
        void SetIfChanged<T>(PropertyClass<T> property, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(menu.GetPropertyOrDefault(property), value))
                menu.SetProperty(property, value);
        }

        SetIfChanged(VBProperties.CaptionProperty, caption);
        SetIfChanged(VBProperties.NameProperty, name);
        SetIfChanged(VBProperties.EnabledProperty, isEnable);
        SetIfChanged(VBProperties.VisibleProperty, isVisible);
        SetIfChanged(VBProperties.CheckedProperty, isChecked);
        SetIfChanged(VBProperties.WindowListProperty, isWindowsList);
    }
}

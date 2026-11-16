using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using HexIDE.IDE;
using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime;

/// <summary>The runtime's event root inside a single-view host window (#4068).</summary>
public sealed class HostedFormRoot : Decorator, IModuleExecutionRoot, IFormView
{
    private readonly IHostedForm hosted;
    public VBWindowContext Context { get; }

    public HostedFormRoot(IHostedForm hosted, IBasicStandardLibrary standardLibrary)
    {
        this.hosted = hosted;
        Context = new VBWindowContext(standardLibrary) { Window = (Control)hosted.Window };
        hosted.SetContent(this);
        hosted.Resized += () => Context.WindowResized();
        hosted.SizeSet += () => Context.WindowResized(sizeSet: true);
        hosted.Shown += () => Context.Shown();
        hosted.Activated += () =>
        {
            if (HeldBy is null)
                Context.Program?.Activated(Context);
        };
        hosted.HeldPressed += () => HeldBy?.ActivateForm();
        hosted.Closing = Context.UnloadFromCloseBox;
    }

    public void SetContent(Control content) => Child = content;
    public Task ShowInHost(bool blockBehind) => hosted.Show(blockBehind);
    public void ShowForm() => hosted.Show(false);
    public void HideForm() => hosted.Hide();
    public void CloseForm() => hosted.Close();
    public void ActivateForm() => hosted.Activate();
    public bool IsShown => hosted.IsShown;
    private IFormView? heldBy;
    public IFormView? HeldBy
    {
        get => heldBy;
        set { heldBy = value; hosted.IsHeld = value is not null; }
    }
    public (Size Size, WindowState State) Extent
    {
        get
        {
            var extent = hosted.Extent;
            return (new Size(extent.Width, extent.Height), (WindowState)extent.State);
        }
    }
    // Optional event paths retain the interface defaults used by the former MDI root.
    public void ExecuteSub(string name, IReadOnlyList<Vb6Value>? args = null) => Context.ExecuteSub(name, args);
    public bool Handles(string name) => Context.Handles(name);
    public void ExecuteEvent(string name, Func<IReadOnlyList<Vb6Value>> args, EventCoalescing coalescing = EventCoalescing.None, Func<bool>? when = null) =>
        Context.ExecuteEvent(name, args, coalescing, when);
    public void ExecuteTimer(string name, IReadOnlyList<Vb6Value>? args = null) => Context.ExecuteTimer(name, args);
    public Task<bool> UnloadAsync(int mode) => Context.UnloadAsync(mode);
}

/// <summary>Resolve a form's runtime root from its own window, on desktop or in a host (#4068).</summary>
public static class FormRoot
{
    public static IFormView? View(object? window) => window as IFormView
        ?? (window as ContentControl)?.Content as HostedFormRoot;

    public static IModuleExecutionRoot? Execution(object? window) => window as IModuleExecutionRoot
        ?? (window as ContentControl)?.Content as HostedFormRoot;
}

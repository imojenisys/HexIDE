using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HexIDE.IDE;
using HexIDE.Runtime.BuiltinTypes;
using HexIDE.Runtime.Interpreter;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;
using MessageBox = HexIDE.Runtime.Dialogs.MessageBox;

namespace HexIDE.Runtime.Integration.Tests.Views;

// VB6 SP6, native and p-code: docs/vb6-fidelity-oracle/rgb-qbcolor-and-msgboxs-style.md,
// "A message box is modal to the whole program" (#834). Timer scheduling is separate (#5491).
public class MessageBoxProgramModalityTests
{
    private sealed class Sink : IDeserializeErrorSink
    {
        public void LogError(string error) => throw new InvalidOperationException(error);
    }

    private sealed class Run : IDisposable
    {
        private readonly CancellationTokenSource cancel = new();
        private readonly IDisposable watch;
        internal readonly List<string> Printed = [];
        internal readonly List<Exception> Errors = [];
        internal readonly List<Window> Boxes = [];
        internal readonly VBFormRuntime Owner;
        internal VBFormRuntime Other => (VBFormRuntime)Owner.Context.Program!.LoadedForms[1].FormWindow!;
        internal readonly Task Running;

        internal Run()
        {
            var project = new ProjectDefinition(VBProjectType.EXE, "P");
            foreach (var name in new[] { "Form1", "Form2" })
            {
                var form = new FormDeserializer().Deserialize(project, $$"""
                    VERSION 5.00
                    Begin VB.Form {{name}}
                       Caption = "{{name}}"
                       ClientWidth = 4500
                       ClientHeight = 3000
                    End
                    Attribute VB_Name = "{{name}}"
                    Private Sub Form_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
                        Debug.Print "{{name}} down"
                    End Sub
                    Private Sub Form_Click()
                        Debug.Print "{{name}} click"
                    End Sub
                    Public Sub HideSelf()
                        Me.Visible = False
                    End Sub
                    Public Sub ShowSelf()
                        Me.Visible = True
                    End Sub
                    """.ReplaceLineEndings("\r\n"), new Sink())!;
                project.AddForm(form);
            }
            watch = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window.Content is MessageBox)
                    Boxes.Add(window);
            });
            VBDebugConsole.Output += Printed.Add;
            VBWindowContext.Faulted += Faulted;
            VBWindowContext.RunTimeError += RunTimeError;
            Running = VBLoader.RunForm(project.Forms[0], cancel.Token, out Owner);
            Pump();
            Code("Form2.Show").GetAwaiter().GetResult();
            Pump();
            Owner.Context.Program!.LoadedForms.Should().HaveCount(2);
        }

        private void Faulted(VBWindowContext _, Exception error) => Errors.Add(error);
        private void RunTimeError(VBWindowContext _, VBRunTimeException error) => Errors.Add(error);

        internal async Task Code(string code)
        {
            var context = Owner.Context;
            await new StatementExecutor(context.Interpreter!, context.InstanceEnv, context.FormInstance!.ClassDef)
                .Visit(context.Interpreter!.TryParseStatement(code)!);
            Pump();
        }

        internal Task OpenBox()
        {
            var done = new TaskCompletionSource();
            Owner.Events.Raise("probe", async () =>
            {
                try
                {
                    await Code("MsgBox \"probe\"");
                    done.SetResult();
                }
                catch (Exception error) { done.SetException(error); }
            });
            Pump();
            return done.Task;
        }

        internal Dictionary<Window, List<(bool Enabled, int Children)>> RecordNativeEnablement()
        {
            var writes = new Dictionary<Window, List<(bool Enabled, int Children)>>();
            var program = Owner.Context.Program!;
            program.Interpreter.BuiltIns.HoldFormsForMessageBox = () =>
                new Dialogs.MessageBoxFormHold(program.LoadedForms, (window, enabled) =>
                {
                    if (!writes.TryGetValue(window, out var changes))
                        writes[window] = changes = [];
                    changes.Add((enabled, window.OwnedWindows.Count));
                    Dialogs.MessageBoxFormHold.ApplyNativeEnabled(window, enabled);
                });
            return writes;
        }

        internal void Answer(int index)
        {
            var box = (MessageBox)Boxes[index].Content!;
            var button = box.GetVisualDescendants().OfType<Button>().Single();
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Pump();
        }

        public void Dispose()
        {
            cancel.Cancel();
            Pump();
            foreach (var box in Boxes)
                box.Close();
            Pump();
            watch.Dispose();
            VBWindowContext.RunTimeError -= RunTimeError;
            VBWindowContext.Faulted -= Faulted;
            VBDebugConsole.Output -= Printed.Add;
            cancel.Dispose();
        }
    }

    private static void Pump()
    {
        var context = System.Threading.ExecutionContext.Capture();
        try { Dispatcher.UIThread.RunJobs(); }
        finally { if (context is not null) System.Threading.ExecutionContext.Restore(context); }
    }

    private static void Click(VBFormRuntime window)
    {
        var point = new Point(80, 70);
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Pump();
    }

    [AvaloniaTheory]
    [InlineData(1.0, "visible")]
    [InlineData(1.5, "visible")]
    [InlineData(1.0, "hidden before")]
    [InlineData(1.5, "hidden before")]
    [InlineData(1.0, "hidden during")]
    [InlineData(1.5, "hidden during")]
    public async Task A_waiting_box_blocks_clicks_on_both_forms_until_answered(double scaling, string visibility)
    {
        using var run = new Run();
        run.Owner.SetRenderScaling(scaling);
        run.Other.SetRenderScaling(scaling);
        if (visibility == "hidden before")
            await run.Code("Form1.HideSelf");
        var waiting = run.OpenBox();
        Pump();
        if (visibility == "hidden during")
            await run.Code("Form1.HideSelf");
        if (visibility != "visible")
            await run.Code("Form1.ShowSelf");

        waiting.IsCompleted.Should().BeFalse();
        Click(run.Other);
        Click(run.Owner);
        run.Printed.Should().BeEmpty();
        // The pump already holds handlers behind the box on main. Effective enablement and the log after answering
        // distinguish discarded input from events queued to run when the waiting statement returns.
        var otherEnabled = run.Other.IsEffectivelyEnabled;
        var ownerEnabled = run.Owner.IsEffectivelyEnabled;
        run.Answer(run.Boxes.Count - 1);
        await waiting;
        run.Printed.Should().BeEmpty("clicks made while the box waited must not run later either");
        otherEnabled.Should().BeFalse();
        ownerEnabled.Should().BeFalse();

        Click(run.Owner);
        Click(run.Other);
        run.Printed.Should().Equal("Form1 down", "Form1 click", "Form2 down", "Form2 click");
        run.Errors.Should().BeEmpty();
    }

    // The issue explicitly requires the program's own assignments to survive; their getters during the wait are
    // deliberately not pinned, since that VB6 value is unmeasured.
    [AvaloniaTheory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task An_Enabled_assignment_while_waiting_survives_the_answer(bool before, bool after)
    {
        using var run = new Run();
        await run.Code($"Form2.Enabled = {before}");
        var waiting = run.OpenBox();
        await run.Code($"Form2.Enabled = {after}");
        run.Other.IsEffectivelyEnabled.Should().BeFalse("program code cannot release the box's hold");
        run.Answer(0);
        await waiting;
        run.Other.IsEnabled.Should().Be(after);
        run.Other.IsEffectivelyEnabled.Should().Be(after);
        Click(run.Other);
        run.Printed.Should().Equal(after ? ["Form2 down", "Form2 click"] : []);
        run.Errors.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void With_no_box_both_forms_take_real_clicks()
    {
        using var run = new Run();
        Click(run.Owner);
        Click(run.Other);
        run.Printed.Should().Equal("Form1 down", "Form1 click", "Form2 down", "Form2 click");
        run.Errors.Should().BeEmpty();
    }

    [AvaloniaFact]
    public async Task A_form_disabled_by_code_stays_disabled_after_the_answer()
    {
        using var run = new Run();
        await run.Code("Form2.Enabled = False");
        var waiting = run.OpenBox();
        Pump();
        run.Answer(0);
        await waiting;
        run.Other.IsEnabled.Should().BeFalse();
        Click(run.Owner);
        Click(run.Other);
        run.Printed.Should().Equal("Form1 down", "Form1 click");
        run.Errors.Should().BeEmpty();
    }

    // Counting overlapping boxes is #834's explicit design decision, not a VB6 measurement. Open them directly
    // through the shared built-in: Timer handlers can't open a second box while the event pump holds them.
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Forms_stay_disabled_until_the_last_box_is_answered(int first)
    {
        using var run = new Run();
        var native = run.RecordNativeEnablement();
        var builtins = run.Owner.Context.Interpreter!.BuiltIns;
        var one = builtins.EvaluateBuiltInFunction("MsgBox", [new Vb6Value("one")]);
        var two = builtins.EvaluateBuiltInFunction("MsgBox", [new Vb6Value("two")]);
        Pump();
        run.Answer(first);
        run.Other.IsEffectivelyEnabled.Should().BeFalse();
        run.Owner.IsEffectivelyEnabled.Should().BeFalse();
        native.Should().ContainKeys(run.Owner, run.Other);
        native[run.Owner].Last().Enabled.Should().BeFalse();
        native[run.Other].Last().Enabled.Should().BeFalse();
        run.Answer(1 - first);
        await Task.WhenAll(one, two);
        run.Owner.IsEffectivelyEnabled.Should().BeTrue();
        run.Other.IsEffectivelyEnabled.Should().BeTrue();
        native[run.Owner].Last().Should().Be((true, 0), "restoration follows Avalonia's owned-dialog cleanup");
        native[run.Other].Last().Enabled.Should().BeTrue();
        Click(run.Owner);
        Click(run.Other);
        run.Printed.Should().Equal("Form1 down", "Form1 click", "Form2 down", "Form2 click");
        run.Errors.Should().BeEmpty();
    }

    // Headless SetEnabled is a no-op. Observe the real native bridge's calls on Linux, including the dispatcher
    // turn after Closed removes the owned dialog; actual window-system enablement remains a live check.
    [AvaloniaTheory]
    [InlineData("visible", "")]
    [InlineData("hidden before", "")]
    [InlineData("hidden during", "")]
    [InlineData("visible", "Form1")]
    [InlineData("visible", "Form2")]
    public async Task Native_windows_are_held_through_visibility_changes_and_restored_after_dialog_cleanup(
        string visibility, string disabledForm)
    {
        using var run = new Run();
        var native = run.RecordNativeEnablement();
        if (disabledForm.Length > 0)
            await run.Code($"{disabledForm}.Enabled = False");
        if (visibility == "hidden before")
            await run.Code("Form1.HideSelf");
        var waiting = run.OpenBox();
        native.Should().ContainKeys([run.Owner, run.Other], "both native windows must receive the hold");
        var ownerWrites = native[run.Owner].Count;
        if (visibility == "hidden during")
            await run.Code("Form1.HideSelf");
        if (visibility != "visible")
        {
            await run.Code("Form1.ShowSelf");
            native[run.Owner].Count.Should().BeGreaterThan(ownerWrites, "showing must reapply the native hold");
            run.Owner.OwnedWindows.Should().BeEmpty("the box moved to an unowned window");
        }

        waiting.IsCompleted.Should().BeFalse();
        native[run.Owner].Last().Enabled.Should().BeFalse();
        native[run.Other].Last().Enabled.Should().BeFalse();
        run.Answer(run.Boxes.Count - 1);
        await waiting;
        native[run.Owner].Last().Should().Be((disabledForm != "Form1", 0),
            "Avalonia must have removed the closed dialog before the last native restoration");
        native[run.Other].Last().Should().Be((disabledForm != "Form2", 0));
        run.Owner.IsEnabled.Should().Be(disabledForm != "Form1");
        run.Other.IsEnabled.Should().Be(disabledForm != "Form2");
        run.Errors.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void A_hosted_forms_root_also_holds_input_without_changing_its_Enabled_setting()
    {
        using var run = new Run();
        var window = new Window { Width = 300, Height = 200 };
        var host = Substitute.For<IHostedForm>();
        host.Window.Returns(window);
        host.When(h => h.SetContent(Arg.Any<object>())).Do(call => window.Content = call.Arg<object>());
        var root = new HostedFormRoot(host, new StandaloneStandardLib(window, "P"));
        var canvas = new Canvas { Background = Brushes.White };
        root.SetContent(canvas);
        var presses = 0;
        canvas.PointerPressed += (_, _) => presses++;
        var form = new VbObject(run.Owner.Context.FormInstance!.ClassDef, new()) { FormWindow = window };
        try
        {
            window.Show();
            Pump();
            using (new Dialogs.MessageBoxFormHold([form]))
            {
                window.MouseDown(new Point(50, 50), MouseButton.Left);
                window.MouseUp(new Point(50, 50), MouseButton.Left);
                presses.Should().Be(0);
                root.IsEffectivelyEnabled.Should().BeFalse();
                root.IsEnabled.Should().BeTrue("the hold leaves the program's setting alone");
            }
            window.MouseDown(new Point(80, 70), MouseButton.Left);
            window.MouseUp(new Point(80, 70), MouseButton.Left);
            presses.Should().Be(1);
            root.IsEffectivelyEnabled.Should().BeTrue();
        }
        finally { window.Close(); }
    }
}

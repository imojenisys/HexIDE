using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using HexIDE.Automation;

namespace HexIDE.Integration.Tests.Automation;

/// <summary>
/// <c>hover</c> with <c>stay</c> leaves the pointer where it went, so what is drawn only under the pointer, a flat toolbar
/// button's hot image or any <c>:pointerover</c> look, and an opened tip, can be snapshotted (#2562). A click raises its
/// events on the control and never sets pointer-over, and a hover's end takes the pointer out of the window, so nothing
/// else leaves it there.
/// </summary>
public class HoverStayTests
{
    private static Window Show(Control content)
    {
        var window = new Window { Content = content, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Button TippedButton(string content, string tip)
    {
        var button = new Button { Content = content, Width = 120, Height = 40 };
        ToolTip.SetTip(button, tip);
        ToolTip.SetShowDelay(button, 0);
        return button;
    }

    private static async Task Settle(int ms)
    {
        var end = Environment.TickCount64 + ms;
        while (Environment.TickCount64 < end) { await Task.Delay(20); Dispatcher.UIThread.RunJobs(); }
    }

    // Send through the platform callback, before pointer-over preprocessing, without touching the OS cursor (#2666).
    private static Action<TopLevel, Point> RealMouse()
    {
        var device = typeof(MouseDevice).GetConstructors().Single().Invoke(
            [new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true)]);
        return (window, point) =>
        {
            var root = typeof(TopLevel).GetProperty("InputRoot",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(window);
            var input = (Action<RawInputEventArgs>)typeof(Avalonia.Platform.ITopLevelImpl).GetProperty("Input")!
                .GetValue(window.PlatformImpl)!;
            var constructor = typeof(RawPointerEventArgs).GetConstructors().Single(c =>
                c.GetParameters() is [_, _, _, _, { ParameterType: var p }, _] && p == typeof(Point));
            input((RawInputEventArgs)constructor.Invoke(
                [device, (ulong)Environment.TickCount64, root, RawPointerEventType.Move, point, RawInputModifiers.None]));
        };
    }

    [AvaloniaTheory]
    [InlineData(true, false, 1.0, false)]
    [InlineData(true, false, 1.5, false)]
    [InlineData(true, true, 1.5, false)]
    [InlineData(false, false, 1.5, false)]
    [InlineData(true, false, 1.5, true)]
    public async Task Real_mouse_restatements_preserve_stay_only_at_a_known_unchanged_position(bool known, bool moved, double scaling, bool nativeWayland)
    {
        NativeWayland.ForTest = _ => nativeWayland;
        var button = TippedButton("Add", "Add Form");
        var other = TippedButton("Other", "Other tip");
        other.Name = "Other";
        var window = Show(new StackPanel { Children = { button, other } });
        window.SetRenderScaling(scaling);
        var real = RealMouse();
        try
        {
            if (known) real(window, new Point(200, 60));
            UiAutomationDriver.Hover(button, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(button);
            var point = button.TranslatePoint(landing!.Value.Point, window)!.Value;
            using var watch = new UiAutomationDriver.ForeignPointerWatch(window, point);
            real(window, new Point(moved ? 210 : 200, 60));
            using var restatement = new UiAutomationDriver.ForeignPointerWatch(window, point);
            real(window, new Point(moved ? 210 : 200, 60));
            await Settle(100);

            var preserved = known && !moved && !nativeWayland;
            restatement.Moved.Should().Be(watch.Moved,
                "a genuine move ends the stay, so its next restatement must reach the input manager too");
            button.IsPointerOver.Should().Be(preserved);
            if (preserved)
            {
                ToolTip.GetIsOpen(button).Should().BeTrue();
                watch.Moved.Should().BeNull();
            }
            else watch.Moved.Should().StartWith($"the real mouse moved the pointer to ({(moved ? 210 : 200)}, 60) in the window")
                .And.EndWith("during the watch, and the tip went with it, as it would for a person: one resting over the window is "
                    + "sent such moves as windows change around it. Move it off the window and hover again");
        }
        finally { UiAutomationDriver.EndStayingHover(window); window.Close(); NativeWayland.ForTest = null; }
    }

    [AvaloniaFact]
    public async Task A_restatement_in_a_new_popup_uses_the_same_screen_position()
    {
        var button = TippedButton("Add", "Add Form");
        var popup = new Avalonia.Controls.Primitives.Popup
        {
            PlacementTarget = button, Child = new Border { Width = 80, Height = 40 },
        };
        var window = PopupBoundsTests.NativePopupWindow(1.5);
        window.Width = 400;
        window.Height = 300;
        window.Content = new StackPanel { Children = { button, popup } };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        var real = RealMouse();
        try
        {
            real(window, new Point(10, 10));
            var screen = window.PointToScreen(new Point(10, 10));
            UiAutomationDriver.Hover(button, null, null, null, out _).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(button);
            popup.IsOpen = true;
            await Settle(50);
            var root = TopLevel.GetTopLevel(popup.Child!)!;
            root.Should().BeOfType<Avalonia.Controls.Primitives.PopupRoot>();
            real(root, root.PointToClient(screen));
            await Settle(100);
            button.IsPointerOver.Should().BeTrue();
            ToolTip.GetIsOpen(button).Should().BeTrue();
        }
        finally { popup.IsOpen = false; UiAutomationDriver.EndStayingHover(window); window.Close(); }
    }

    [AvaloniaFact]
    public async Task A_hover_left_by_stay_keeps_the_control_under_the_pointer_with_its_tip_open()
    {
        var button = TippedButton("Add", "Add Form");
        var window = Show(button);
        try
        {
            UiAutomationDriver.Hover(button, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);

            UiAutomationDriver.StayHover(landing!.Value.Receiver);
            await Settle(100);
            var said = UiAutomationDriver.StayedNote(landing.Value.Receiver, moved: null, saidAlready: false);

            said.Should().Be("; the pointer stays there and 'Button' reads as under it (isPointerOver), so take_snapshot "
                             + "shows its pointer-over look and any tip, until the next click, drag or hover, in this window "
                             + "or another, or the real mouse's next move to a new position over it");
            button.IsPointerOver.Should().BeTrue();
            ToolTip.GetIsOpen(button).Should().BeTrue("the tip a staying hover opened stays open for a snapshot");
            UiAutomationDriver.Inspect(button, "Window/Button").IsPointerOver.Should().BeTrue(
                "inspect_element reads it back");
        }
        finally { window.Close(); }
    }

    // A pointer is over one window at a time, and two windows both read as under it after a staying hover in each (#2886).
    [AvaloniaFact]
    public async Task A_hover_in_another_window_ends_a_staying_hover_and_says_so()
    {
        var first = TippedButton("One", "First tip");
        var second = TippedButton("Two", "Second tip");
        var firstWindow = Show(first);
        firstWindow.Title = "Hourglass form";
        var secondWindow = Show(second);
        try
        {
            UiAutomationDriver.Hover(first, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(landing!.Value.Receiver);
            await Settle(100);
            first.IsPointerOver.Should().BeTrue("the first staying hover is what the second has to end");

            var reply = UiAutomationDriver.Hover(second, null, null, null, out _);
            await Settle(100);

            reply.Success.Should().BeTrue(reply.Error);
            reply.Detail.Should().Contain("; the pointer left 'Hourglass form'");
            first.IsPointerOver.Should().BeFalse("the pointer has gone to the other window");
            second.IsPointerOver.Should().BeTrue();
        }
        finally
        {
            firstWindow.Close();
            secondWindow.Close();
        }
    }

    // A click takes the pointer to its own window as a hover does, and two windows read as under it otherwise (#3424).
    [AvaloniaFact]
    public async Task A_click_in_another_window_ends_a_staying_hover_and_says_so()
    {
        var first = TippedButton("One", "First tip");
        var second = TippedButton("Two", "Second tip");
        var firstWindow = Show(first);
        firstWindow.Title = "Hourglass form";
        var secondWindow = Show(second);
        try
        {
            UiAutomationDriver.Hover(first, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(landing!.Value.Receiver);
            await Settle(100);
            first.IsPointerOver.Should().BeTrue("the staying hover is what the click has to end");

            var reply = await UiAutomationDriver.FinishAsync(UiAutomationDriver.Interact(second, "click", null));
            await Settle(100);

            reply.Success.Should().BeTrue(reply.Error);
            reply.Detail.Should().Contain("; the pointer left 'Hourglass form'");
            first.IsPointerOver.Should().BeFalse("the pointer has gone to the other window");
        }
        finally
        {
            UiAutomationDriver.EndHover(second);
            firstWindow.Close();
            secondWindow.Close();
        }
    }

    [AvaloniaFact]
    public async Task A_hover_with_no_staying_hover_elsewhere_says_nothing_of_another_window()
    {
        var button = TippedButton("Only", "Tip");
        var window = Show(button);
        try
        {
            var reply = UiAutomationDriver.Hover(button, null, null, null, out _);
            await Settle(50);

            reply.Detail.Should().NotContain("the pointer left");
        }
        finally { window.Close(); }
    }

    // The snapshot draws its owner's popups over the window, and the dump lists them under it; a tip was neither, so a
    // snapshot after a staying hover showed the hot look and not the tip the reply said was open.
    [AvaloniaFact]
    public async Task A_tip_a_staying_hover_left_open_is_one_of_its_owners_popups_and_the_dump_lists_its_text()
    {
        var button = TippedButton("Add", "Add Form");
        var window = Show(button);
        try
        {
            UiAutomationDriver.Hover(button, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(landing!.Value.Receiver);

            UiAutomationDriver.AttachedPopupsOfPublic(button).Should().ContainSingle()
                .Which.Child.Should().BeOfType<ToolTip>();
            var dump = UiAutomationDriver.Dump(window, "Window", 10, interactiveOnly: false);
            UiAutomationDriver.Find(dump, "Add Form", 10).Matches.Should().NotBeEmpty();
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task A_hover_that_does_not_stay_leaves_nothing_under_the_pointer()
    {
        var button = TippedButton("Add", "Add Form");
        var window = Show(button);
        try
        {
            UiAutomationDriver.Hover(button, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            button.IsPointerOver.Should().BeTrue("the move went through the window's input");

            UiAutomationDriver.EndHover(landing!.Value.Receiver);
            await Settle(100);

            button.IsPointerOver.Should().BeFalse();
            ToolTip.GetIsOpen(button).Should().BeFalse();
            UiAutomationDriver.Inspect(button, "Window/Button").IsPointerOver.Should().BeFalse();
        }
        finally { window.Close(); }
    }

    // A click raises its press and release on the control, which takes nothing out from under the pointer: without the
    // staying hover's end, the first button kept its pointer-over look and its tip over the second.
    [AvaloniaFact]
    public async Task The_next_click_in_the_window_ends_a_staying_hover_first()
    {
        var hovered = TippedButton("Add", "Add Form");
        var clicked = new Button { Content = "Other", Width = 120, Height = 40 };
        var window = Show(new StackPanel { Children = { hovered, clicked } });
        try
        {
            UiAutomationDriver.Hover(hovered, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(landing!.Value.Receiver);

            var outcome = await UiAutomationDriver.InteractAsync(clicked, "click", null);
            await Settle(100);

            outcome.Success.Should().BeTrue(outcome.Error);
            hovered.IsPointerOver.Should().BeFalse("the click's pointer left what the hover was over");
            ToolTip.GetIsOpen(hovered).Should().BeFalse("a tip left open would cover what the click was aimed at");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task The_next_hover_in_the_window_ends_a_staying_hover_and_can_stay_itself()
    {
        var first = TippedButton("Add", "Add Form");
        var second = TippedButton("Remove", "Remove Form");
        var window = Show(new StackPanel { Children = { first, second } });
        try
        {
            UiAutomationDriver.Hover(first, null, null, null, out var landing).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(landing!.Value.Receiver);

            UiAutomationDriver.Hover(second, null, null, null, out var next).Success.Should().BeTrue();
            await Settle(100);
            UiAutomationDriver.StayHover(next!.Value.Receiver);
            await Settle(100);

            first.IsPointerOver.Should().BeFalse();
            ToolTip.GetIsOpen(first).Should().BeFalse();
            second.IsPointerOver.Should().BeTrue();
            ToolTip.GetIsOpen(second).Should().BeTrue();
        }
        finally { window.Close(); }
    }
}

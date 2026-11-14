using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using HexIDE.Automation;
using HexIDE.Desktop.Server;

namespace HexIDE.Integration.Tests.Automation;

public class WindowChangeTests
{
    [AvaloniaTheory]
    [InlineData(1400, 900, 1282, 700)]
    [InlineData(1100, 620, 1100, 620)]
    public async Task Layout_follows_the_platform_client_size(int width, int height, int actualWidth, int actualHeight)
    {
        var backing = new Window();
        var inner = backing.PlatformImpl!;
        Window? window = null;
        var resized = (Action<Size, WindowResizeReason>)null!;
        var platform = PopupBoundsTests.ForwardingPlatform<IWindowImpl>.Wrap((method, args) =>
        {
            if (method.Name == "Resize")
            {
                var requested = (Size)args![0]!;
                var allowed = new Size(Math.Min(requested.Width, 1282), Math.Min(requested.Height, 700));
                args[0] = allowed;
                // Native backends can report the same capped size again. Avalonia then leaves Width/Height alone.
                if (allowed != requested && allowed == window?.ClientSize)
                    resized(allowed, WindowResizeReason.Layout);
            }
            return method.Invoke(inner, args);
        });
        var factory = new Factory();
        var document = new Document { Title = "Code" };
        var documents = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>(document), ActiveDockable = document };
        var diagnostics = new Tool { Title = "Diagnostics" };
        var bottom = new ToolDock
        {
            Proportion = 0.2, Alignment = Alignment.Bottom,
            VisibleDockables = factory.CreateList<IDockable>(diagnostics), ActiveDockable = diagnostics,
        };
        var rows = new ProportionalDock
        {
            Orientation = Dock.Model.Core.Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents, new ProportionalDockSplitter(), bottom),
        };
        var layout = new RootDock { VisibleDockables = factory.CreateList<IDockable>(rows), ActiveDockable = rows };
        factory.InitLayout(layout);
        var root = new DockControl { Layout = layout };
        window = new Window(platform) { Width = 1282, Height = 700, Content = root };
        var app = Application.Current!;
        var lifetime = typeof(Application).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(field => field.FieldType == typeof(IApplicationLifetime));
        var previousLifetime = lifetime.GetValue(app);
        var desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        desktop.MainWindow.Returns(window);
        desktop.Windows.Returns(new[] { window });
        lifetime.SetValue(app, desktop);
        window.Show();
        resized = (Action<Size, WindowResizeReason>)typeof(ITopLevelImpl)
            .GetProperty("Resized", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(inner)!;
        window.WindowState = WindowState.Maximized;
        try
        {
            var reply = await new HexIdeTools(null!).SetWindowStateAsync("Normal", width: width, height: height,
                ct: TestContext.Current.CancellationToken);
            reply.Success.Should().BeTrue(reply.Error);
            reply.Window!.Width.Should().Be(actualWidth);
            reply.Window.Height.Should().Be(actualHeight);
            Dispatcher.UIThread.RunJobs();
            using var assertions = new AwesomeAssertions.Execution.AssertionScope();
            window.ClientSize.Should().Be(new Size(actualWidth, actualHeight));
            window.Width.Should().Be(window.ClientSize.Width);
            window.Height.Should().Be(window.ClientSize.Height);
            root.Bounds.Size.Should().Be(window.ClientSize);
            var pane = root.GetVisualDescendants().OfType<ToolChromeControl>()
                .Single(control => ReferenceEquals(control.DataContext, bottom));
            var paneTop = pane.TranslatePoint(default, window)!.Value.Y;
            paneTop.Should().BeGreaterThanOrEqualTo(0);
            (paneTop + pane.Bounds.Height).Should().BeLessThanOrEqualTo(reply.Window.Height);
            pane.Bounds.Height.Should().BeGreaterThan(0);
            var note = reply.Note;
            if (width == actualWidth && height == actualHeight)
                note.Should().BeNull();
            else
                note.Should().Contain("1400×900").And.Contain("1282×700")
                    .And.Contain("x/y/width/height").And.Contain("working area");

            // A later native/user resize must still own layout.
            resized(new Size(900, 500), WindowResizeReason.User);
            Dispatcher.UIThread.RunJobs();
            root.Bounds.Size.Should().Be(window.ClientSize);
        }
        finally { window.Close(); lifetime.SetValue(app, previousLifetime); }
    }

    [AvaloniaFact]
    public async Task Wait_reads_confirmation_after_a_later_dispatcher_post()
    {
        var confirmed = false;
        var wait = WindowChange.WaitAsync(() => confirmed, TestContext.Current.CancellationToken);
        wait.IsCompleted.Should().BeFalse();
        var later = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(50, TestContext.Current.CancellationToken);
            confirmed = true;
        }, DispatcherPriority.Background);
        try
        {
            (await wait).Should().BeTrue();
            confirmed.Should().BeTrue();
        }
        finally { await later; }
    }

    [AvaloniaFact]
    public async Task Wait_reports_an_unapplied_request()
    {
        (await WindowChange.WaitAsync(() => false, TestContext.Current.CancellationToken, 25)).Should().BeFalse();
    }
}

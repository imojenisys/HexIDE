using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime.Tests;

[Collection<ErrorUnwindCollection>]
public class NormalReturnUnwindTests : BaseVBTestFixture
{
    // HexIDE's call-depth cap and the Work/Deep probe are recorded in #5152, not a VB6 recursion limit.
    [Theory(Timeout = 300_000)]
    [InlineData(1000, true, false)]
    [InlineData(4900, false, true)]
    [InlineData(4900, true, false)]
    public async Task AWalkThatReturns_LeavesStackForTheNextWalk(int depth, bool stops, bool everyStatement)
        => await RunWalk(depth, stops, everyStatement);

    // #5152's Windows guard: Linux's native stack probe already refuses Work 1000 on this 8 MiB stack.
    [WindowsOnlyFact]
    public async Task AWalkWithoutSuspension_LeavesStackForTheNextWalk()
        => await RunWalk(1000, stops: false, everyStatement: false);

    private async Task RunWalk(int depth, bool stops, bool everyStatement)
    {
        await OnStack(() =>
        {
            var (vb, dbg) = NewDebuggable(
                "Dim maxDepth As Long\n" +
                $"Work {depth}\n" +
                "Deep 0\nDebug.Print maxDepth\n" +
                "Sub Work(ByVal n As Long)\n" +
                "  If n > 0 Then Work n - 1\n" +
                "End Sub\n" +
                "Sub Deep(ByVal d As Long)\n" +
                "  On Error GoTo Done\n" +
                "  maxDepth = d\n" +
                "  Deep d + 1\n" +
                "Done:\nEnd Sub\n", "M");
            dbg.TimeSliceMs = everyStatement ? -1 : stops ? 15 : null;
            var pump = (Pump)SynchronizationContext.Current!;
            var hits = 0;
            if (stops)
            {
                dbg.SetBreakpoints("M", [6]);
                dbg.Stopped += _ =>
                {
                    hits++;
                    pump.Post(_ => dbg.Continue(), null);
                };
            }
            var run = vb.Execute();
            pump.Drain();
            run.IsCompleted.Should().BeTrue("the walk must finish on the pumping thread");
            run.GetAwaiter().GetResult();
            hits.Should().Be(stops ? depth + 1 : 0);
            TestContext.Current.TestOutputHelper?.WriteLine($"Work {depth}, stops={stops}, everyStatement={everyStatement}: {debug[0].Value}");
            AssertDebugLog([new Vb6Value(4999L)]);
        });
    }

    [Fact]
    public async Task AReturnAcrossSuspendedCallers_PostsAtTheUnwindBoundary()
    {
        await OnStack(() =>
        {
            var (vb, dbg) = NewDebuggable(
                "Work 32\nDebug.Print 1\n" +
                "Sub Work(ByVal n As Long)\n  If n > 0 Then Work n - 1\nEnd Sub\n", "M");
            dbg.TimeSliceMs = null;
            dbg.SetBreakpoints("M", [4]);
            dbg.SetBreakpointConditions("M", new Dictionary<int, string> { [4] = "n = 0" });
            var pump = (Pump)SynchronizationContext.Current!;
            dbg.Stopped += _ => pump.Post(_ => dbg.Continue(), null);
            var run = vb.Execute();
            pump.Drain();
            run.IsCompleted.Should().BeTrue("the walk must finish on the pumping thread");
            run.GetAwaiter().GetResult();
            AssertDebugLog([1]);
            // Continue itself and the break-gate continuation account for two posts; the return needs a third.
            pump.Posts.Should().Be(3);
        });
    }

    [Fact]
    public async Task CallsAtAnUnwindBoundary_WithoutSuspension_PostNothing()
    {
        await OnStack(() =>
        {
            // Loop is frame 32; Leaf returns with 32 frames below it, the normal-unwind yield boundary.
            var (vb, dbg) = NewDebuggable(
                "Work 30\n" +
                "Sub Work(ByVal n As Long)\n" +
                "  If n > 0 Then\n    Work n - 1\n  Else\n    LoopCalls\n  End If\nEnd Sub\n" +
                "Sub LoopCalls()\n  Dim i As Long\n  For i = 1 To 1000\n    Leaf\n  Next\nEnd Sub\n" +
                "Function Leaf() As Long\n  Leaf = 1\nEnd Function\n", "M");
            dbg.TimeSliceMs = null;
            var pump = (Pump)SynchronizationContext.Current!;
            var run = vb.Execute();
            run.IsCompletedSuccessfully.Should().BeTrue();
            run.GetAwaiter().GetResult();
            pump.Posts.Should().Be(0);
        });
    }

    private static async Task OnStack(Action body)
    {
        Exception? failure = null;
        await Task.Run(() =>
        {
            var thread = new Thread(() =>
            {
                var previous = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new Pump());
                try { body(); }
                catch (Exception error) { failure = error; }
                finally { SynchronizationContext.SetSynchronizationContext(previous); }
            }, 8 * 1024 * 1024);
            thread.Start();
            thread.Join();
        }, TestContext.Current.CancellationToken);
        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(failure);
    }

    private sealed class Pump : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object? State)> pending = new();
        public int Posts { get; private set; }
        public override void Post(SendOrPostCallback callback, object? state)
        {
            Posts++;
            pending.Enqueue((callback, state));
        }
        public void Drain()
        {
            while (pending.TryDequeue(out var work))
                work.Callback(work.State);
        }
    }
}

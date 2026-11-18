# #4484 validation (WSL, 2026-11-17–18)

Baseline: `a1c0be6f3` (`upstream/main` when this run began), StreamJsonRpc 2.24.92,
.NET 11.0.0, Ubuntu under WSL. The baseline test assembly includes the new churn
harness, `NullLogger` clients, stop-time output, and an inert default-false
`TryEndOutput` interface member and timeout constant so the new tests compile.
Its handlers and teardown retain main's behaviour. Immutable copies of the built
assemblies keep baseline and branch processes separate. No package pins changed.
The churn implementation was adapted from this repository's `wip/3520-churn`
commit `f15fc9270960f1d4175c3b513cd877419ca99852`, without its experimental adapters
or poison pools. The cause and earlier measurements are recorded on
[#3520](https://github.com/hexide-io/HexIDE/issues/3520); the final scope is
[#4484](https://github.com/hexide-io/HexIDE/issues/4484)'s round 4 comment.

## Regression teeth

On the baseline, the stdio-shaped client-first arm (`CHURN=12`, eight checked
pairs) detected its first bad frame at **9.261 seconds** from the ledger's start.
It finished after 16,212 checked frames and 1,490 teardowns, with one bad frame.
A new `Content-Length` header appeared inside another connection's JSON string.
The framing assertion failed; the run ended after 10.357 seconds overall.

Five half-close cases fail before implementation: the two `StopEndsOutput...`
rows, the unsupported-transport call assertion, and the stdio/TCP end-of-input
wire tests. The leave-running row already passes on main and is a regression
guard. Two additional unit tests fail under isolated mutations: replacing the interlocked
swap with a plain read fails the 999-suppressed-disposals assertion; replacing the
private array pool with the shared pool fails the cross-connection isolation
assertion. Those are mutation checks of the new helper, not additional failures
claimed against main. Both protections were restored and rebuilt afterwards.
No existing test's expectation was changed. The two wire tests accept a
`JsonRpcRequest` subclass: the formatter returns its own derived request type.

## Stop timings

Milliseconds measured around `StopAsync`, excluding startup. These are single
samples, including scheduler noise. The existing time bounds were not changed.

| Case | Main | Branch (58-test focused run) |
|---|---:|---:|
| Server stops reading (#1221) | 4,010 | 4,005 |
| Server never answers shutdown (#392) | 2,010 | 2,002 |
| Reply stalls halfway | 2,018 | 2,001 |
| Slow shutdown, stdio | 1,514 | 2,526 |
| Slow shutdown, named pipe | 1,514 | 1,533 |
| Unanswered shutdown helper, stdio | 3,518 | 4,528 |
| Unanswered shutdown helper, named pipe | 3,544 | 3,533 |

Stdio helpers that keep output open spend the new one-second output-end wait.
The blocked-write path skips it. Named pipes cannot half-close. Scheduling adds
small differences beyond the configured wait; these measurements do not claim
an exact wall-clock bound. All 23 baseline fault/slow-shutdown cases passed.

## Harness setup and incomplete runs

The first stdio client-first run did not return after its 20-minute load and was
interrupted; it is not counted as a passing run. Adding watchdogs exposed that
waiting for cancelled sends before ending the pipes did not model product
teardown: product disposes RPC, then its transport. The harness now ends its
pipes before awaiting those sends. A second diagnostic run was stopped while
that correction was being made, also without a passing result.

A further client-first run stopped churning at 14,337 teardowns although checked
traffic continued. An auxiliary diagnostic run also hit thread-start resource
exhaustion and a peer cleanup timeout; it was not counted as validation. An owned
process dump showed these concurrent stacks on .NET 11.0.0 under Linux:

- `StdioRound` → `PipeStream.Dispose` → `SafePipeHandle.Dispose` → `Socket.Dispose`
  → `SafeSocketHandle.CloseAsIs` (spinning).
- `PipeStream.ReadAsyncCore` → `SafePipeHandle.CreatePipeSocket` → `Socket.Dispose`
  → `SafeSocketHandle.CloseAsIs` (spinning).

The fixture was racing initial lazy Unix pipe-socket setup with disposal. The
final stdio fixture first observes a framed notification and actual peer input
before releasing the flood and tearing down. This models an established
connection, with both directions used, and still tears down mid-write. Merely
waiting for the old writer's flush did not establish that the peer had received
bytes, so the input drain signals receipt explicitly. This is a fixture setup
change, not a product teardown redesign. No claim about an upstream fix is made.

Subsequent diagnostic runs use a separate `TMPDIR` as well as `XDG_RUNTIME_DIR`:
parallel isolated processes can otherwise have ambiguous default diagnostic
socket names. The dump collector checked the test command line before collecting
only the requested owned process. No other clone was inspected or changed.

## Reproduction

Before builds/tests, remove `/mnt/` entries from PATH, export
`UseSharedCompilation=false`, and set `XDG_RUNTIME_DIR` to a fresh writable temp
directory, as the task's WSL instructions require. For each branch churn arm:

```sh
cd IDE
HEXIDE_WIRE_STRESS_SECONDS=1200 HEXIDE_WIRE_STRESS_CHURN=12 \
  HEXIDE_WIRE_STRESS_CHURN_ORDER=stdio-client-first \
  dotnet test HexIDE.Tests/ -- --filter-method '*ConcurrentConnectionsKeepEveryFrameValid' \
  --hangdump --hangdump-timeout 25m \
  --report-xunit --report-xunit-filename 4484-churn.xml
```

Repeat with `stdio-half-close` and `tcp-client-first`. The TCP arm uses TCP for
both checked and churn pairs. For the no-churn cost comparison, omit CHURN and
CHURN_ORDER and set SECONDS to 60. The baseline uses the identical checked-pair
workload with `HeaderDelimitedMessageHandler(Stream, Stream, formatter)`.
Allocations include the entire harness and are divided by checked frames;
throughput is checked frames divided by elapsed load time. Full-GC memory is
the whole managed test heap, with the checked connections and ledger still live.

The direct executions of immutable assembly copies used xunit's in-process
runner (`dotnet <test.dll> -method '*ConcurrentConnectionsKeepEveryFrameValid'
-showLiveOutput -xml <report.xml>`); ordinary `dotnet test` uses MTP's options
shown above. Both execute the same test method and assertions.

To reproduce the baseline stress arm from source, bring only the current
`WireStressTests.cs` onto the baseline commit and replace its
`new LspMessageHandler(...)` calls with `new HeaderDelimitedMessageHandler(...)`.
Keep the `NullLogger` change in both arms; otherwise NSubstitute retains cancelled
requests through recorded log arguments and the memory comparison measures the
logger harness instead of the transport.

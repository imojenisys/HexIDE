# MCP restart investigation (#778)

This is a diagnostic probe, not a proposed fix. The original Windows timeout has not
been reproduced here. Do not infer that changing shutdown, disabling pooling or
increasing a timeout fixes it.

## Repeat

From the repository root, apply `git apply scripts/probes/778/restart-trace.patch`,
then run `python3 scripts/probes/778/run.py`. The patch repeats only
`A_client_attached_before_a_restart_is_answered_after_it` 2,000 times inside one test.
It records System.Net.Http, System.Net.Sockets and System.Net.NameResolution events
through an EventListener. The pooled run keeps the fixture's original SDK transport;
only the explicit control supplies an HttpClient with zero pooled connection lifetime.
There is no ConnectCallback. Both modes retain the existing 20-second call bound.
The runner builds first, starts 48 CPU workers, runs both modes sequentially with a
10-minute bound each, and terminates its own workers in a finally block.

Logs and event traces go into `artifacts/778/` and are temporary. Copy any failing
trace somewhere durable before that directory is emptied. An event line includes a
Stopwatch timestamp, source, event name and payload; `iteration N` separates cycles.
The fixture also retains its existing phase timing and exception-chain diagnostics.
The listener is process-wide, so unrelated HTTP events can appear if other tests run
in the same host. Use only the method filter in the runner.

After probing, reverse the patch with
`git apply -R scripts/probes/778/restart-trace.patch` and rebuild before ordinary test
runs. The diagnostic test requires HEXIDE_778_TRACE, which the runner sets. The patch
is intentionally kept outside the compiled test suite: default test behavior and
transport are unchanged.

## Measurement

Measured 2026-11-13 on WSL/Linux, .NET 10, MCP SDK 2.2.0, base c0451457e.
The host reported 36 logical CPUs. The load was 48 Python CPU workers, alive throughout
both controlled runs. This does not recreate the Windows socket stack or the exact
contention in the issue's reports. Event collection can affect scheduling; these
passing instrumented runs do not rule out the uninstrumented race.

- Preliminary traced run: 200/200 cycles passed; load ended around the build, so this
  is not counted as a controlled load measurement.
- Pooled client under load: 2,000/2,000 cycles passed (one test, 3m 15.978s), zero skipped.
  Every cycle had four RequestHeadersStart events: initialize, before, after, again.
  In every cycle the connection id for after differed from before. No RequestFailed
  event was recorded.

- Pooling-disabled control under the same load: 2,000/2,000 cycles passed
  (one test, 3m 36.583s), zero skipped. Each cycle used four distinct HTTP
  connection ids; no RequestFailed event was recorded.

The pooled run did not reproduce the hypothesized reuse of an orphaned connection.
Passing both modes cannot distinguish the hypotheses in the issue. A failing Windows
trace is still needed before choosing a server shutdown change. No product code,
transport configuration, timeout, language inventory or MCP gap status changed.
There is no #778 entry in docs/mcp-gaps/open/ to update.

## Validation

The temporary patch was reversed before validation. The compiled restart class
passed 45/45 tests without skips. The infrastructure namespace passed 811 tests,
with one platform skip. Debug Desktop built with zero warnings and errors. Tree
hygiene passed with nine nonblocking warnings. No new regression assertion was
introduced, so the teeth count is zero; these passes do not establish a flake fix.

The live supervisor check remains: attach, call a tool, use shutdown_ide, relaunch,
then call a tool through the same attachment. No live MCP tools were used in this run.

Full Integration: 3,553 passed, four skipped, zero failed (3,557 total),
10m 50.586s. The xUnit report check found no hidden errors. HangDump initially
failed before discovery because its mutex used read-only /run/user/1000; retrying
with XDG_RUNTIME_DIR set to a directory inside this clone resolved that startup
problem. The skipped tests supply no coverage for this investigation.

Full HexIDE.Tests: 7,909 passed, 66 skipped, zero failed (7,975 total),
25m 03.953s, within the requested 45-minute bound. The xUnit report check found
no hidden errors. Exact skipped test names and reasons for both full suites are
in [skipped-tests.md](skipped-tests.md). No skipped test is claimed as coverage.

# Issue #3858: End during pending external-debug startup

Measured under WSL on 2026-11-12/13, from upstream/main `58c1f289`.

## Reproduction and sequence

The original theory passed 200/200 repetitions (400 cases, both stderr rows), with four independent
xunit hosts while the full HexIDE.Tests suite ran alongside. The full baseline passed 7,858 tests,
skipped 69, failed zero; xunit recorded 7,927 cases and zero errors (23m 08s test duration).

The deterministic fixture suffix `-startup` withholds startup's `threads` response, reports
`startup-pending`, and continues answering stopped-frame inspection. The test waits for both that
marker and `CanEvaluate` before End. Both new rows failed against unchanged product code:

```text
startup-pending
disconnect-pending
DAP connection closed before its response arrived.
```

The stderr row instead contained `startup-pending` and the connection-closed diagnostic: cleanup
could close the output pipe before the disconnect marker was consumed. Neither row had the notice.

`ExternalDebugService.StartAsync` awaits `ThreadsAsync(cancellation.Token)` after launch, while its
independent `ReadUpdatesAsync`/`InspectAsync` path can already select a stopped frame. `StopCoreAsync`
cancels that token before awaiting `DisconnectAsync`. Startup catches cancellation and enters its
`finally`, disposing the session while disconnect is held. `DapConnection.Close` faults the pending
disconnect with the quoted IOException; `StopCoreAsync` wrote its message to Output/Immediate. The
process-result notice block was inside startup's try, so cancellation skipped it entirely.
The relevant code is [startup and final cleanup](../../IDE/HexIDE/Debugging/ExternalDebugService.cs#L175),
[End's disconnect](../../IDE/HexIDE/Debugging/ExternalDebugService.cs#L434), and
[connection closure](../../IDE/HexIDE.Dap/DapConnection.cs#L245).

The fix makes startup's finally await the existing End task before disposal, and reports the process
result there for completed sessions or intentional End. Disconnect teardown exceptions stay in the
log. This follows the existing single-notice requirement in `openspec/specs/dap-client/spec.md`;
there is no reason to show a request's connection-closed diagnostic when End deliberately closes it.

## Tests and siblings

- The original two rows still assert the complete Immediate contents: marker, one localized notice,
  optional separate stderr, and final newline; neither retries nor longer waits were added.
- The two deterministic startup rows share those assertions. Both failed before the fix; all four
  passed after it, with zero xunit errors.
- A temporary non-constant mutation changed the process-result guard from
  `(reportExit || stopping is not null)` to `(reportExit || stopping is not null && !IsActive)`.
  That dropped the notice only on cancelled startup while the session was still active. Both new
  rows failed with only the startup/disconnect markers in Immediate; the two original rows passed.
  The mutation was restored before the class and full-suite runs. Teeth count: two new cases.
- The restored `MainViewViewModelTests` class passed 136/136 cases, none skipped, zero xunit errors
  (4m 14s). Desktop Debug build passed with zero warnings and errors. Tree hygiene passed with nine
  existing nonblocking warnings; `git diff --check` passed.
- After the fix, 200/200 repetitions of all four notice cases passed under the concurrent full-suite
  load: 800 passes, zero failures, skips or xunit errors. Each of the 200 XML reports was checked for
  exactly four passes. This includes 400 deterministic startup cases as well as the 400 original cases.
- The final full HexIDE.Tests run passed 7,860 tests, skipped the same 69 cases, and failed zero
  (23m 21s). Its xunit XML recorded 7,929 cases and zero errors, checked with
  `scripts/check-xunit-report.sh`. The full run and stress repetitions overlapped throughout the
  stress loop. The two extra full-suite passes are the new startup regression rows.

The final class/full commands (after removing Windows PATH entries and setting the environment above):

```sh
pwsh scripts/test.ps1 IDE/HexIDE.Tests/ --no-restore -- --filter-class '*MainViewViewModelTests' --report-xunit --report-xunit-filename issue3858-class.xml
pwsh scripts/test.ps1 IDE/HexIDE.Tests/ --no-build --no-restore -- --report-xunit --report-xunit-filename issue3858-final.xml
```
- `Ending_external_session_restores_design_before_disconnect_completes` waits for the adapter's
  `disconnect-pending` output through `WaitFor`, then checks design state while cleanup is pending.
  Both original notice rows await StopAsync and the session task; none waits a fixed sleep for a notice.
  The fix applies to both stderr rows and the design-state sibling through the shared product path.

## Repeating the experiment

Drop `/mnt/` entries from PATH and set `HEXIDE_FOREIGN_LSP_DOWNLOAD=0` before tests. Build first with
`pwsh scripts/test.ps1 IDE/HexIDE.Tests/ -- --filter-class '*MainViewViewModelTests'`.
Start the full suite with `--report-xunit --report-xunit-filename issue3858-full.xml` after `--`, then:

```sh
python3 scripts/probes/issue-3858-repeat.py artifacts/issue-3858-repeat 200 --include-startup
```

Each repetition launches xunit directly (`-method`/`-xml`, rather than MTP's `--filter-method` flags),
checks exactly four passes with `--include-startup` (two without), no skips/failures/errors, and retains
stdout and XML per host. Startup repetitions use eight hosts; the baseline used four. An initial
attempt with MTP flags on the direct xunit runner executed no tests and is excluded from the counts.
Run `bash scripts/check-xunit-report.sh` against the full suite's XML, not just MTP's exit code.
Local NuGet writable caches were redirected into this checkout, using the preinstalled package cache
as a read-only restore fallback; no dependency versions changed.

## Skips

The baseline's 69 skipped cases are not coverage: 53 Windows-only (24 Windows repacker, 22 filesystem,
three job-object, two type-library, one made-program launch, one share-link), 10 requiring the absent
VB6 Template tree, three macOS-only, one missing pinned DAP schema, and two uncached foreign adapters
(netcoredbg and debugpy, with downloads disabled). None of the End-notice cases skipped.

No VB6 runtime behaviour or language inventory status changes; MISSING_LANGUAGE counts are unchanged.

## Supervisor live check

Live IDE verification was not performed in this WSL run. With an adapter that never acknowledges
disconnect, press End while startup still awaits its thread reply but stopped inspection is available.
Check that design state returns before cleanup finishes and that Immediate ends with one localized
adapter/wait notice, followed only by any stderr. No connection-closed exception or raw exit line
should appear. The document-content assertions run headlessly on Linux; they do not verify rendering.

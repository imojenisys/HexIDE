# #2008 runner refresh (2026-11-18)

The latest [issue comment](https://github.com/hexide-io/HexIDE/issues/2008) scopes this refresh:
merge main, move all six test projects to MTP/HangDump 2.5.3, retain the documented script commands,
and leave CLAUDE.md byte-for-byte equal to main. Main for this run is `1a9e9098f`;
merge commit `48c37b8ce` resolves the already-started merge.

## CLAUDE.md corrections for maintainer approval

These are proposed replacements, **not edits to CLAUDE.md**. Line numbers refer to main
`1a9e9098f`. The script commands at lines 19–32 continue to work.

- Lines 47–49: replace with “the suite's length. HangDump 1.x's mutex handoff made macOS count the
  whole run as idle (#3874). HangDump 2.5.3 signals activity over the pipe; retain the existing timeouts
  until macOS is measured again. The two headless suites and `HexIDE.Tests` (#2128)”.
- Line 67: replace “this tree's MTP 1.9.1” with “the then-current MTP 1.9.1”.
  That measurement remains historical evidence, not a measurement of 2.5.3.
- Line 252: replace the command with
  `dotnet test --project HexIDE.Tests/HexIDE.Tests.csproj --artifacts-path /repo/artifacts/container`.
- Lines 328–330: clarify “Through `scripts/test.ps1`, a relative `--results-directory TestResults`
  puts reports in the project's own `TestResults/`. Bare native `dotnet test` resolves the path from
  the caller's directory; CI supplies `<project>/TestResults` explicitly.”
- Lines 345–346: replace the parenthesis with
  “(`Microsoft.Testing.Extensions.HangDump`, pinned with `Microsoft.Testing.Platform` at 2.5.3)”.
- Line 402: replace the command with
  `cd IDE && dotnet test --project HexIDE.Tests/HexIDE.Tests.csproj -- --filter-trait "Category=ToolDescription"`.
- Lines 1081–1082: replace with “**Test framework migration is complete** (#297, #2008).
  All six test projects use **xunit v3 3.2.2** through `xunit.v3.mtp-v2`, with
  **Microsoft.Testing.Platform 2.5.3**.”
- Lines 1084–1085: replace “and `<TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>`
  and nothing else” with “and the native runner selected by `global.json`.
  `dotnet test` requires `--project path/to/Tests.csproj`; the local scripts resolve directory paths.
  The old `TestingPlatformDotnetTestSupport` and `TestingPlatformShowTestsFailure` properties are removed”.
  Preserve the historical parenthesis at lines 1085–1087.
- Line 1094: replace `xunit.v3` with `xunit.v3.mtp-v2`.
- Lines 1099–1102: replace with “**Use xunit filters with the native MTP runner.** VSTest's
  `--filter "FullyQualifiedName~Foo"` is rejected. The former forwarding mode ignored it with MTP0001
  and ran the whole suite (measured: 153 tests instead of 27).”
- Line 1104: replace with “Keep the `--` in script commands to separate build options from runner
  arguments; native MTP also accepts runner options without it.”

Possible drift: the latest supervisor instruction deliberately leaves these stale instructions in the
contributor guide pending separate maintainer approval. That temporarily conflicts with CLAUDE.md's
Currency & Best Practice goal; the concrete breakage is the positional commands at lines 252 and 402.
The documented script forms are preserved, so the usual contributor path remains usable.

## Package checks

NuGet registration API, checked 2026-11-18 at each pinned version:

| Package | Version | Licence |
| --- | --- | --- |
| xunit.v3.mtp-v2 | 3.2.2 | Apache-2.0 |
| xunit.v3.core.mtp-v2 | 3.2.2 | Apache-2.0 |
| Microsoft.Testing.Extensions.HangDump | 2.5.3 | MIT |
| Microsoft.Testing.Extensions.Telemetry | 2.5.3 | MIT |
| Microsoft.Testing.Extensions.TrxReport.Abstractions | 2.5.3 | MIT |
| Microsoft.Testing.Platform | 2.5.3 | MIT |
| Microsoft.Testing.Platform.MSBuild | 2.5.3 | MIT |

API form: `https://api.nuget.org/v3/registration5-gz-semver2/{lowercase-id}/{version}.json`,
following `catalogEntry` and reading `licenseExpression`. Telemetry and TrxReport.Abstractions
also require pins: xunit's 3.2.2 dependency minimum otherwise resolves them to 2.0.2.

## Validation setup

Linux in WSL, .NET SDK 11.0.100. Windows PATH entries removed, `UseSharedCompilation=false`,
`MSBUILDDISABLENODEREUSE=1`, `DOTNET_CLI_USE_MSBUILD_SERVER=0`, `MSBUILDUSESERVER=0`,
a writable `XDG_RUNTIME_DIR`, and `HEXIDE_FOREIGN_LSP_DOWNLOAD=0`.
The last two server variables avoid reusing MSBuild nodes outside this sandbox.
The branch uses clone-local `NUGET_PACKAGES` and `NUGET_HTTP_CACHE_PATH`: the shared caches
are read-only here. An initial restore failed before tests started; that is not a test result.

Main is tested in a detached worktree inside this clone. After the first build-only failure, an
early retry inadvertently duplicated the baseline invocation while the first had continued to the IDE
suite. Both were allowed to finish; no full test run was stopped. Their shared console logs can contain
NUL gaps from overlapping redirections, so counts are extracted from complete summary records and
headless xunit reports, with the two complete IDE totals compared. Full suites run through
`pwsh scripts/test.ps1 <project>`. Headless suites additionally write xunit reports checked with
`scripts/check-xunit-report.sh`. No Windows/macOS result is claimed; the reviewer owns those checks.

## Focused checks and teeth

- The unchanged new test sources were linked into an isolated xunit/MTP 1.9.1 probe, using main's
  workflow. Three assertions fail: platform 2.5.3, native project selection, and project-local CI
  report paths. HangDump/platform assembly equality already passes on main and is a regression guard.
- The updated script selftests fail against main's scripts: PowerShell stops at its first project
  invocation mismatch; the shell test records 12 invocation mismatches. The report-path assertion
  separately fails against the first native wrapper, before report-path normalization.
  After the changes: **20 PowerShell checks and 17 shell checks pass**.
- Through the real PowerShell wrapper: class filter 2 passed; method and query filters 1 passed each;
  empty filter and excessive floor both exit 9; explicit `--minimum-expected-tests=2` passes.
  VSTest's `--filter` is refused with exit 5.
- Dispatcher-affinity guards: **5 passed**, including cross-class and continuation checks.
  The final workflow guards: **13 passed**, including the added report-path guard. A real wrapper
  invocation writes their xunit report into the project's `TestResults/`; its report guard passes.
- `RequiredServersTests`: **7 passed** with cached fixtures. Explicit missing paths for all five
  foreign servers under `HEXIDE_REQUIRE_FOREIGN_LSP=1` give **5 failed, 2 passed, 0 skipped**, exit 2.
  This is the expected refusal, not a suite regression. The clone's cached foreign servers and
  protocol model are temporarily moved aside for the full branch run to match main's empty caches.
- Runtime headless HangDump check: **3291 passed, 0 failed, 2 skipped**, total 3293, **3m27s** with
  `--hangdump --hangdump-timeout 20s --diagnostic`. The controller diagnostic records **197 activity
  signals received** and the pipe connection. HangDump starts in this sandbox; no mutex failure or
  timeout occurs. The two skipped tests need Windows GDI/the oracle's Windows Segoe UI build.
  xunit's report records all 3293 tests and **0 errors**.
- This first native run exposed the report-path change: the report landed in root `TestResults/`,
  rather than the project. Its report guard passes at that actual location. The wrapper now resolves
  relative result paths against the project and CI names project-local paths explicitly. The scheduled
  final headless runs verify the corrected locations. The focused workflow run has already verified
  the corrected location with a real native runner and a clean xunit report.

No VB6 behaviour changes; no unmeasured VB6 values pinned. MISSING_LANGUAGE's counts do not move:
its only edit updates the recount command for the native runner.

## Changed files

- `Directory.Packages.props`, `global.json`, and the six test projects' `.csproj` files.
- `.github/workflows/build.yml`; `scripts/test.ps1`, `scripts/test-linux.sh`, their two selftests,
  and `scripts/package-licences.tsv`.
- `IDE/HexIDE.Tests/Infrastructure/TestingPlatformVersionTests.cs`, `WorkflowActionTests.cs`,
  and the command example in `ToolDescriptionAttribute.cs`.
- `CONTRIBUTING.md`, `README.md`, `LspServer/README.md`,
  `IDE/HexIDE.Runtime/Interpreter/README.md`, `demo/battleship/README.md`,
  `docs/TEST_PROJECTS.md`, `docs/dap-client.md`, `docs/mcp-server-gaps.md`,
  `docs/serialization-outcomes.md`, and the recount command in `docs/MISSING_LANGUAGE.md`.
- `changelog.d/2008.md` and this validation record. The #2008 fragment never existed on main;
  it is unreleased, so the existing PR fragment is updated rather than recreated after release.

## IDE count comparison

Both complete main IDE runs agree: **10104 passed, 0 failed, 121 skipped**, total **10225**
(38m44s and 39m01s). The branch is **10111 passed, 0 failed, 118 skipped**, total **10229**
(36m18s). The four extra tests are the two version guards and two workflow guards.

The three skip-to-pass changes are availability, not discovery: the branch clone had cached DAP data
and adapters, whereas the main worktree did not. A targeted main run with downloads disabled confirms
exactly **0 passed, 0 failed, 3 skipped**, and its xunit report names:

- `DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode`
- `ForeignAdapterTests.Debugpy_runs_a_complete_session`
- `ForeignAdapterTests.Netcoredbg_runs_a_complete_session`

All three ran successfully in the branch's full suite. The targeted main invocation exits 1 because
its floor requires one executed test; all three cases were discovered but skipped.

## Baseline harness correction

The artifact-nested baseline initially gave TwinBasic **348 passed, 1 failed, 2 skipped**.
`CompilerRecordingTests.Every_recording_in_the_tree_passes_redaction` rejects paths containing
`/artifacts/`, which excluded every recording because the worktree itself was below that directory.
A second detached main worktree inside this clone, outside `artifacts/`, gives a clean full run:
**349 passed, 0 failed, 2 skipped**, total **351**. No test or product assertion was changed.
The two live-compiler tests skip on Linux and provide no coverage here. No compiler binary was inspected.

The first baseline harness also reported a trailing shell parse error after all six project invocations;
it had been edited during the early retry. Per-project exit codes, complete summaries, and xunit reports
are used rather than that harness exit status. Both complete main IDE runs agree.

## Final full-suite results

Counts are **passed / failed / skipped**. All full branch suites and all six corrected main baselines pass.

| Project | Main | Branch |
| --- | --- | --- |
| HexIDE.Runtime.Tests | 9883 / 0 / 20 | 9883 / 0 / 20 |
| HexIDE.Tests | 10104 / 0 / 121 | 10111 / 0 / 118 |
| HexIDE.Integration.Tests | 4211 / 0 / 4 | 4211 / 0 / 4 |
| HexIDE.Runtime.Integration.Tests | 3291 / 0 / 2 | 3291 / 0 / 2 |
| HexIDE.TwinBasic.Tests | 349 / 0 / 2 | 349 / 0 / 2 |
| HexIDE.VbLspServer.Tests | 786 / 0 / 0 | 786 / 0 / 0 |

The IDE total increases by exactly four new tests; its three availability changes are explained above.
Every other project's passed, failed and skipped counts match exactly. Both final headless xunit reports
pass `scripts/check-xunit-report.sh`: 4215 and 3293 total, **0 errors** each, on main and branch.
The final runtime-headless run again uses a **20-second** hang timeout and completes normally with its
report in the project-local directory.

`dotnet list <project>.csproj package --include-transitive` for **each of the six projects** shows:
**Microsoft.Testing.* = 2.5.3 only** (Platform, MSBuild, Telemetry, TrxReport.Abstractions, and HangDump
where referenced). No 1.x or 2.0.2 component remains in a resolved graph.

The final skip names and case counts are in [skipped-tests.txt](skipped-tests.txt). Skips provide no
coverage: they include platform-only cases, absent VB6 templates/oracle, unavailable foreign LSP/model
fixtures, the opt-in wire stress test, and four existing disabled headless IDE tests. The runtime-headless
font/GDI tests and the two live-compiler tests also skip here.


## Final static checks and handoff

- Licence guard: passes.
- `actionlint` over the workflows with `shellcheck` installed: no findings.
- Runner selftests: 20 PowerShell checks and 17 shell checks pass.
- Changelog fragment check: all 178 fragments well formed.
- Tree hygiene: passes, 9 existing non-blocking warnings, no FAILED finding.
- `git diff --check`: clean. `CLAUDE.md` is identical to main.
- Temporary main worktrees removed; original foreign-server/model caches restored.
  Generated project `bin/` and `obj/` directories are removed after validation.

The implementation remains uncommitted for the supervising session. Only the main merge is committed.
Reviewer work remains: Windows/macOS/CI validation and approval of the listed CLAUDE.md corrections;
the macOS timer measurement is explicitly outside this lane's deliverable.

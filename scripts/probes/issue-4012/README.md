# #4012 — .NET 11 spike

Measured on Linux x64 under WSL, 2026-11-15, from `upstream/main` / `612b62c4`.
The scope is the latest comment on [issue #4012](https://github.com/hexide-io/HexIDE/issues/4012).
This is a try-and-report branch, not a merge-ready migration. No package version was bumped,
no runtime expectation was changed, and no new VB6 measurement was made.

## Build

SDK 11.0.100 and runtime 11.0.0 came from the supplied user-scope SDK. The comparison used the
system SDK 10.0.115 and `upstream/main` in a detached worktree inside this clone. The root
`HexIDE.slnx` includes both halves and all six test projects; AddinPacker is built through project
references. It covers Desktop, Standalone, Repack, AIChat and TestAddin too. Browser was tried separately.
AddinPacker, Dap.Smoke and Dap.Parity were also built explicitly in Release, because project-reference
configuration overrides can otherwise build their Debug output in a Release solution build; all three
explicit Release builds succeed with zero warnings and errors.

| Configuration | .NET 10 main | .NET 11 final |
|---|---|---|
| Debug, complete root solution | 0 warnings, 0 errors | 0 warnings, 0 errors |
| Release, complete root solution | 0 warnings, 92 errors | 0 warnings, 92 errors |
| LSP solution, Debug and Release | Debug succeeds; Release compiles in the root build | Both succeed, 0 warnings, 0 errors |
| Shipping projects, Release | Compile in the root build | Compile and publish successfully |

The Release errors match main by location, ID and message:

| Project | ID | Count | Cause |
|---|---|---:|---|
| HexIDE.Tests | CS0234 | 41 | Tests reference namespaces excluded from Release |
| HexIDE.Tests | CS0246 | 27 | Tests reference types excluded from Release |
| HexIDE.Integration.Tests | CS0234 | 12 | Tests reference namespaces excluded from Release |
| HexIDE.Integration.Tests | CS0246 | 7 | Tests reference types excluded from Release |
| HexIDE.Integration.Tests | CS0103 | 1 | Debug-only name unavailable |
| HexIDE.Runtime.Integration.Tests | CS0117 | 4 | `LockKeys.SetOverride` is Debug-only |

[Every error, including its source location and message](release-diagnostics.md) is retained separately.
These are pre-existing build failures, not suppressed or repaired by the spike.

**New warning before the mechanical fix:** NU1510, `HexIDE.Lsp`, emitted twice in a collected
solution build (restore and the inner framework build). The first strict restore stops with one
warning-as-error. .NET 11 includes `Microsoft.Extensions.Logging.Abstractions` in
`Microsoft.NETCore.App.Ref/11.0.0`; its `PackageOverrides.txt` lists version 11.0.0. Removing the
redundant direct PackageReference is the only dependency fix. The central version is untouched.
Afterward strict Debug succeeds and strict Release reaches the 92 existing errors, with zero warnings.
`-p:TreatWarningsAsErrors=false` was used only on diagnostic collection commands, never saved.

Sandbox setup also mattered: the CLI initially could not create its first-run sentinel in the
read-only user profile, and ANTLR's named mutex failed under read-only `XDG_RUNTIME_DIR`.
Redirecting the CLI home and runtime directory into the clone resolved both. These are environment
failures, not patched product code. First-party add-in packing is skipped because this clone has
no signing key; the build reports that explicitly.

## Packages

[The complete central-pin/selected-asset table](packages.md) comes from restored `project.assets.json`
files, using [package-assets.py](package-assets.py). A separate package-only `net11.0` restore
included every central pin except logging abstractions (supplied by the framework), covering the
otherwise unused Browser and Svg.Skia pins too. It succeeded without warnings. This restore does
not establish that the Browser application builds.

No selected package supplies a native `net11.0` managed asset. Avalonia/Headless 12.1.4, Dock
12.0.0.2, Classic.Avalonia.Theme 12.0.1-beta1 and LanguageExt use `net10.0`. StreamJsonRpc,
EmmyLua, Serilog, MTP and HangDump use `net9.0`; AwesomeAssertions uses `net8.0`, NSubstitute
`net6.0`, and ANTLR's task/runtime use `netstandard2.0`. Pure.DI and PropertyChanged.SourceGenerator
supply analyzer/build assets and execute successfully during compilation. The table shows
xunit.analyzers 1.27.0 where it is only transitive, alongside the explicit 2.0.0-pre.40 pin;
this existing difference was not changed. xunit.v3 resolves
3.2.2, inside its deliberate `[3.2.2,4.0.0)` ceiling; MTP and HangDump remain 1.9.1.

There were no NU1701, NU1603, NU1608 or other restore warnings beyond the removed NU1510 reference.
No package bump is currently required by a restore/compiler failure. Roslyn remains 5.0.0;
using new C# 15 syntax in sources inspected by syntax-based tests would require a separate
compatibility check and possibly a reviewed, licence-recorded bump. The xunit ceiling must remain
until its documented Avalonia discovery incompatibility is resolved.

## Tests

Full runs use `HEXIDE_FOREIGN_LSP_DOWNLOAD=0`, the workflow's minimum-test floors, and xunit reports.
The main comparison below is a real Git checkout, preserving `.gitattributes`, rather than an
archive under `artifacts/`: a preliminary archive run was unsuitable because the recording scan
explicitly excludes every path containing `artifacts`. Its
`CompilerRecordingTests.Every_recording_in_the_tree_passes_redaction` failed with
"Expected files not to be empty" on unmodified main. The proper main worktree passes that class
and the full TwinBasic suite. The preliminary archive is not used as the baseline.

P/F/S means passed / failed / skipped. The CI column is the measured total in main’s workflow comments, not the current checkout’s total.

| Project | Main CI comment | Discovered, both runs | Main .NET 10 P/F/S | .NET 11 P/F/S |
|---|---:|---:|---|---|
| Runtime | 8623 | 8801 | 8782/0/19 | 8773/9/19 |
| TwinBasic | 350 | 350 | 348/0/2 | 348/0/2 |
| HexIDE.Tests | 8089 | 8445 | 8331/0/114 | 8331/0/114 |
| Integration | 3588 | 3642 | 3638/0/4 | 3638/0/4 |
| Runtime.Integration | 2750 | 2876 | 2874/0/2 | 2874/0/2 |
| LspServer | 742 | 742 | 742/0/0 | 742/0/0 |

All 24,856 tests are discovered on both targets. Main: 24,715 passed, zero failed, 141 skipped. .NET 11: 24,706 passed, nine failed, 141 skipped. Every xunit report records zero out-of-test errors.

Both headless suites’ reports pass `scripts/check-xunit-report.sh` on both targets. The final IDE run completed in 23m13s; main’s comparison completed in 22m21s. [All skipped tests and reasons](skipped-tests.md) are retained, including Windows-only, live-compiler, foreign-server and unavailable-model cases.

[Runtime failure names and complete assertion messages](runtime-failures.md) cover all nine
.NET 11 runtime failures. The five affected classes were also run individually on main with
.NET 10: DoubleToCurrencyTests 22/22, NumericFormatFunctionsTests 138/138, FractionalForTests 79/79,
FormatNumericTests 5/5 and StringToDateTests 154/154, with no failures or skips in those comparisons.
The new target changes decimal conversion/rounding and rejects the measured date input `24.0`.
The decimal loop regression executes ten iterations instead of the measured eleven. These
behaviour changes were reported, not changed to guessed expectations or fixed outside the scope.

An interim full IDE run, before retargeting the last two documentation assertions, discovered
8,445 tests: 8,373 passed, two failed, 70 skipped, in 26m17s. The failures were
`ReleaseDownloadDocumentationTests.ReleaseTemplateNamesPlainRuntimeAndAcknowledgesNativeDependencies`
and `ReadmeExplainsLinuxAssetSelectionExtractionAndLaunchBeforeBuilding`, still expecting
".NET 10 Runtime" from the new .NET 11 docs. Both assertions were mechanically updated.
Their class passes all four tests separately on main and on the final branch. The original clone
has cached foreign servers and the LSP model; clean main/final worktrees do not. That accounts
for 44 extra skips in clean runs (37 foreign-server cases and seven model cases), not a
discovery reduction. No skipping result is cited as coverage.

**Teeth:** five updated tests fail against main's original workflow/README inputs: three in
ReleaseBuildTests and two in ReleaseDownloadDocumentationTests. Against the retargeted inputs,
all five ReleaseBuildTests and all four ReleaseDownloadDocumentationTests pass. WorkflowActionTests
passes all 11 cases. The isolated before-input check ran the compiled tests with xunit's direct
`-class` runner; complete suites use `dotnet test` / MTP.

## Browser

`dotnet build IDE/HexIDE.Browser` fails with NETSDK1147: `wasm-tools` is missing.
`dotnet workload install wasm-tools` fails immediately because the supplied SDK's `metadata`
directory is read-only in the sandbox. No SDK, workload, system installation or user `.dotnet`
installation was modified. Browser compilation beyond workload resolution remains untested.
The Cloudflare script's SDK channel was mechanically updated too.

## Publish

All four commands succeeded without warnings: Desktop and Standalone, each with
`-f net11.0 --self-contained false`, and each with `-f net11.0 -r linux-x64 --self-contained true`.
They used Release defaults and a scratch directory outside the clone. Neither AOT nor trimming
was enabled. Unsigned first-party add-ins were not included.

The self-contained Standalone ran [the smoke module](smoke/Main.bas), using
[the project template](smoke/Smoke.vbp.in), under `xvfb-run` and exited **0**. **stdout and stderr
were empty.** `StandaloneStandardLib.DebugPrint` ignores the typed channel and `DebugWrite`
sends text to `VBDebugConsole`; Standalone installs no console subscriber. Thus this proves a
clean application launch/exit, not capture of the two printed strings. No console behavior was added.
An initial launch failed with exit 160 because the inherited bundle-extraction directory was
read-only; redirecting `DOTNET_BUNDLE_EXTRACT_BASE_DIR` into the clone allowed the successful run.

## CLAUDE.md

Left unchanged as requested. Suggested replacements (line numbers in this checkout):

| Line | New wording |
|---:|---|
| 11 | `Requirements: .NET 11 SDK. No Java needed — Antlr4BuildTasks bundles the ANTLR tool.` |
| 34 | `cd IDE && dotnet publish HexIDE.Desktop -f net11.0 -o bin/` |
| 35 | `cd IDE && dotnet publish HexIDE.Standalone -f net11.0 -o bin/standalone/` |
| 38 | `cd IDE && dotnet publish HexIDE.Desktop -f net11.0 -o bin/` |
| 181 | `# Ubuntu + dotnet-sdk-11.0 from Ubuntu's own repos (no Microsoft feed needed on 26.04+).` |
| 189 | `wsl -e bash -lc 'sudo apt-get update && sudo apt-get install -y dotnet-sdk-11.0 rsync'` |
| 251 | `mcr.microsoft.com/dotnet/sdk:11.0-resolute` (retain the command continuation) |

The apt package and container tag substitutions above are mechanical suggestions, not installation
or image-pull measurements from this run. Historical measurements at lines 186–188 stay unchanged.
The test-runner filtering and floor rules did not change.

## Needs a decision

- The nine new runtime regressions block adopting the target. Repairing conversion semantics is
  behavioural work and was deliberately left for review, rather than changing oracle-backed assertions.
- Decide how Release builds of the three affected test projects should reference Debug-only APIs.
  Shipping Release projects work; the complete Release solution does not, on either runtime.
- Install `wasm-tools` outside this sandbox into the user-scope SDK before assessing Browser further.
- The supervisor must verify the running IDE with the RDCore add-in, including Avalonia rendering,
  and separately test .NET 10 add-in compatibility in the .NET 11 host. Neither is established here.
  For the live check, launch .NET 11 Desktop with an isolated profile, open code and a classic form
  designer (including an OptionButton), then load RDCore's .NET 11 add-in, open its UI and invoke
  a contributed command. Repeat loading a known .NET 10 add-in; check for load-context,
  MissingMethodException and TypeLoadException failures. Windows, macOS and other publishing RIDs
  were not exercised.
- No commit or push was made: the run-level instruction leaves changes uncommitted for the supervisor,
  overriding the issue comment's request to commit the spike.

`IDE/.github/workflows/` was left unchanged. A tracked-content search found no reference that invokes
those nested workflows; GitHub's active workflows are in the root `.github/workflows/`.
The LspServer nested copy's SDK selector was retargeted because it was not in the explicit exclusion.

## Checks and remaining coverage

`actionlint` with ShellCheck installed reports no findings. WorkflowActionTests passes 11/11.
`check-licences.sh` passes. Tree hygiene reports OK, nine existing nonblocking warnings; the identity
scan is skipped because `HEXIDE_IDENTITY_PATTERN` is unset. `git diff --check` passes.

MISSING_LANGUAGE and MISSING_FEATURES counts did not move: no language feature was added or claimed
complete. No oracle page or oracle index was changed. This experimental branch's regressions remain
explicit adoption blockers above. Nothing was measured against VB6 in this run.

## Possible drift

The latest supervisor comment's instruction to retarget **every tracked build target** also
retargets historical measurement probes, including `scripts/probes/2953/StackProbe.csproj` and
`scripts/probes/issue-3210/StackProbe.csproj`. Their reports remain explicitly measured on .NET 10,
so their default rerun now measures a different host. This may weaken the reproducibility goal
behind CLAUDE.md's *Claims Say What Was Checked* rule. The nine runtime regressions demonstrate
that the runtime version can matter; no .NET 11 stack-depth result is being inferred here.
The instruction was followed because this is cheap to undo. Review whether historical probes
should retain a separately selectable .NET 10 target. The bounded try-and-report migration itself
fits the shared-host/add-in goal.

## Reproduction

From this clone's root, with the supplied SDK already installed:

```sh
export DOTNET_ROOT=$HOME/sdk/dotnet11 PATH=$HOME/sdk/dotnet11:$PATH
export PATH="$(printf '%s' "$PATH" | tr ':' '\n' | grep -v '^/mnt/' | paste -sd:)"
export DOTNET_CLI_HOME=$PWD/artifacts/dotnet11-home
export NUGET_PACKAGES=$PWD/artifacts/nuget NUGET_FALLBACK_PACKAGES=$HOME/.nuget/packages
export XDG_RUNTIME_DIR=$PWD/artifacts/4012-runtime
export DOTNET_BUNDLE_EXTRACT_BASE_DIR=$PWD/artifacts/4012-bundle
mkdir -p "$XDG_RUNTIME_DIR"
chmod 700 "$XDG_RUNTIME_DIR"
export HEXIDE_FOREIGN_LSP_DOWNLOAD=0
dotnet --version # 11.0.100
dotnet build HexIDE.slnx -c Debug
dotnet build HexIDE.slnx -c Release
```

For each project in the results table, the full test command is
`dotnet test <project-directory> --no-build --no-restore -- --minimum-expected-tests <floor> --report-xunit --report-xunit-filename result.xml --results-directory TestResults`.
The workflow floors used were 8191 / 332 / 7684 / 3408 / 2612 / 704 in table order.
Run `bash scripts/check-xunit-report.sh <project-directory>/TestResults/result.xml` for each headless suite.
No full IDE run was stopped early.

The package-only probe can be recreated without copying or changing a version pin:

```sh
mkdir -p artifacts/4012-package-probe
python3 - <<'PYTHON'
from pathlib import Path
import xml.etree.ElementTree as ET
project = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
properties = ET.SubElement(project, "PropertyGroup")
ET.SubElement(properties, "TargetFramework").text = "net11.0"
ET.SubElement(properties, "OutputType").text = "Exe"
ET.SubElement(properties, "TreatWarningsAsErrors").text = "true"
items = ET.SubElement(project, "ItemGroup")
for pin in ET.parse("Directory.Packages.props").iter("PackageVersion"):
    if pin.attrib["Include"] != "Microsoft.Extensions.Logging.Abstractions":
        ET.SubElement(items, "PackageReference", Include=pin.attrib["Include"])
ET.ElementTree(project).write("artifacts/4012-package-probe/Probe.csproj")
PYTHON
dotnet restore artifacts/4012-package-probe/Probe.csproj
python3 scripts/probes/issue-4012/package-assets.py artifacts/4012-package-probe/obj/project.assets.json
```

For the Standalone smoke, copy `smoke/Smoke.vbp.in` as `Smoke.vbp` beside `smoke/Main.bas`
in the scratch project directory. Launch the self-contained runner with that `.vbp` path under
`xvfb-run -a`; the observed successful run produced no stdout/stderr and exited zero.

## Files changed

The 47 existing files receive mechanical target/path/documentation edits, plus the redundant logging reference removal and clarification of a historical measurement comment. The nine new files retain the changelog, probe and report.

- `.github/workflows/build.yml`
- `.github/workflows/compiler-beta-check.yml`
- `.github/workflows/publish.yml`
- `CONTRIBUTING.md`
- `IDE/Directory.Build.props`
- `IDE/HexIDE.AIChat/HexIDE.AIChat.csproj`
- `IDE/HexIDE.AddinPacker/HexIDE.AddinPacker.csproj`
- `IDE/HexIDE.Desktop/HexIDE.Desktop.csproj`
- `IDE/HexIDE.Integration.Tests/Automation/AutomationStatusSectionTests.cs`
- `IDE/HexIDE.Lsp/HexIDE.Lsp.csproj`
- `IDE/HexIDE.Repack/HexIDE.Repack.csproj`
- `IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs`
- `IDE/HexIDE.TestAddin/HexIDE.TestAddin.csproj`
- `IDE/HexIDE.Tests/DebugAdapter/ForeignAdapterTests.cs`
- `IDE/HexIDE.Tests/Infrastructure/BundledServerBuildTests.cs`
- `IDE/HexIDE.Tests/Infrastructure/ReleaseBuildTests.cs`
- `IDE/HexIDE.Tests/Infrastructure/ReleaseDownloadDocumentationTests.cs`
- `IDE/HexIDE.Tests/LspClient/BundledServerTraceRoundTripTests.cs`
- `IDE/HexIDE.Tests/Projects/MadeProgramTests.cs`
- `IDE/build_cloudflare.sh`
- `LspServer/.github/workflows/build.yml`
- `LspServer/Directory.Build.props`
- `README.md`
- `docs/TEST_PROJECTS.md`
- `docs/dap-adapter-smoke.md`
- `global.json`
- `openspec/config.yaml`
- `scripts/dev-ide.ps1`
- `scripts/probes/2622-stack/StackProbe.csproj`
- `scripts/probes/2874-bidi/BidiProbe.csproj`
- `scripts/probes/2953/StackProbe.csproj`
- `scripts/probes/2953/measure.py`
- `scripts/probes/for-each-corpus/ForEachCorpus.csproj`
- `scripts/probes/issue-2908/check-round3-teeth.py`
- `scripts/probes/issue-3074-teeth.py`
- `scripts/probes/issue-3197/repeat.py`
- `scripts/probes/issue-3210-block-cost/BlockCostProbe.csproj`
- `scripts/probes/issue-3210/README.md`
- `scripts/probes/issue-3210/StackProbe.csproj`
- `scripts/probes/issue-3210/conditional-limits.py`
- `scripts/probes/issue-3210/endpoints.py`
- `scripts/probes/issue-3210/measure.py`
- `scripts/probes/issue-3210/measure_win.py`
- `tools/IcoGen/IcoGen.csproj`
- `tools/ParseDepthProbe/ParseDepthProbe.csproj`
- `tools/ParseDepthProbe/run.py`
- `tools/TranslationCoverage/TranslationCoverage.csproj`
- `changelog.d/4012.md`
- `scripts/probes/issue-4012/README.md`
- `scripts/probes/issue-4012/package-assets.py`
- `scripts/probes/issue-4012/packages.md`
- `scripts/probes/issue-4012/release-diagnostics.md`
- `scripts/probes/issue-4012/runtime-failures.md`
- `scripts/probes/issue-4012/skipped-tests.md`
- `scripts/probes/issue-4012/smoke/Main.bas`
- `scripts/probes/issue-4012/smoke/Smoke.vbp.in`

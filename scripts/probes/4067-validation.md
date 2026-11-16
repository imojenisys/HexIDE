# Running-program contract validation (#4067)

Checked on 2026-11-16, starting from `upstream/main` at `6636f0340`.

The Core `IRunningProgram` contract answers form identity/name, activity since input, and printed output.
`InterpreterRunningProgram` implements it beside the engine and is registered in DI; application startup
installs the observer before the main view or automation server subscribes. An absent provider uses
`EmptyRunningProgram`. Activity is read through `IProgramActivity`, which the existing pump implements;
the polling interval, dispatcher priority, opaque walk identities and reply composition stay unchanged.
Name metadata is retained for window selection without classifying a named ordinary window as a running form.

The latest issue comment explicitly requests no changelog entry for this internal refactor. No language
behaviour was added or changed, no VB6 measurement was assumed, and `MISSING_LANGUAGE.md` counts did not move.
No messages or tool descriptions were added or changed.
There is no issue-named MCP gap file to close. The pooled #4054 guard was absent from the starting
`upstream/main` (`6636f0340`).

## Checks

| Final check | Passed | Skipped | Total |
|---|---:|---:|---:|
| `HexIDE.Tests` in full | 8,520 | 68 | 8,588 |
| `HexIDE.Tests.Infrastructure` namespace | 884 | 2 | 886 |
| `HexIDE.Integration.Tests` in full | 3,666 | 4 | 3,670 |
| `HexIDE.Runtime.Integration.Tests` in full | 2,888 | 2 | 2,890 |
| New headless contract cases | 4 | 0 | 4 |

All final xUnit reports were checked with `scripts/check-xunit-report.sh`: no errors or lost cases.
The infrastructure and contract slices are subsets of the full suites. All six new cases ran on Linux.
Two full unit runs completed, in 35m 41s and 38m 01s; neither was stopped early. Both had the same totals
and skip set. The table reports the final run, after the form-name/running-form distinction was added.

The headless suites ran with `--hangdump --hangdump-timeout 20m` and xUnit reports enabled.
`dotnet build HexIDE.Desktop/HexIDE.Desktop.csproj -c Debug -m:1 -nr:false` succeeded with **zero warnings**.
The required infrastructure filter was `--filter-namespace "HexIDE.Tests.Infrastructure"`.

## Coverage and teeth

Six tests were added: four headless contract cases, an empty-provider case, and the source dependency guard.
The positive contract cases start the real interpreter fixture and exercise the existing automation input and dispatcher
settle route, rather than driving the pump directly. They cover a form, an IDE window, an arbitrary object,
name metadata without a running form, input completion, printed text, and the empty answers without a run.
The empty-provider case runs without Avalonia or an installed engine.

The structural guard passed on the changed tree and failed against the full product source from
`upstream/main`: `git archive` supplied the isolated source tree, and the built test executable was placed
beneath it so `RepoTree.Root()` read that tree. xUnit's console runner ran only
`RunningProgramBoundaryTests`: **1 test, 1 failed, 0 errors**. The guard's code was identical in the passing
and failing runs. This is **one demonstrated pre-change failure**; the five behaviour-preservation cases
are compatibility coverage, not claimed as pre-change behavioural regressions.

All existing automation assertions remain unchanged. Three `ProgramWaitTests` now read or drive the concrete
pump through `run.Window.Events`, instead of reaching it through the automation watch:

- `A_DoEvents_loop_caught_between_two_DoEvents_is_still_the_program_s_loop`
- `An_old_dialog_does_not_hold_a_reply_when_the_watch_finds_a_busy_loop`
- `A_Timer_s_DoEvents_loop_that_starts_after_the_action_does_not_hold_the_reply`

Both test applications install the interpreter observer. No other existing test setup was changed.

## Environment

Builds and tests dropped Windows entries from `PATH` and exported `UseSharedCompilation=false`.
The sandbox's default NuGet HTTP cache was read-only; a workspace cache resolved that restore failure.
The requested runtime-suite retry reproduced the pre-test `TESTINGPLATFORM_HANGDUMP_MUTEXNAME` failure
(`EROFS` while creating the shared-memory directory). Setting `XDG_RUNTIME_DIR` to a private, writable
workspace directory resolved it, with hang-dump collection still enabled. A transient generated-output
read-only-file error during a concurrent build was resolved by rebuilding with `-m:1 -nr:false` and
`MSBUILDDISABLENODEREUSE=1`. No shared processes or other clones were touched.

## Remaining review

The supervisor's live check should select a running form by its stable name after its caption changes,
confirm an IDE dialog is not treated as the program, and invoke an input handler that yields before
updating its form. The reply must wait for the settled value and retain the existing notes for a busy
handler, breakpoint or program dialog. Printed text should arrive once in both the Immediate window
and the automation reply buffer. After stopping the program, IDE input should have nothing to wait for.

Slice 5 remains responsible for `VBMenu`, `VBTimer`, `VBCommonDialog`, the other `VBProps` uses,
`LockKeys`, `LockKey`, `LockKeySource`, `VBCursorTypeExtensions` and `VBFontExtensions`.

## Possible drift

The round-2 comment's guard covers every product C# file, beyond the automation callers and two IDE uses
that define step 3. It also catches the designer preview's concrete local declaration and two documentation
comments. Those declarations/comments were changed to satisfy the requested guard. The preview still calls
`FormComponentClass`, `VBLoader` and `StandaloneStandardLib`: passing this name guard does not mean the
designer has been detached from the interpreter. Treating it as that proof would head away from CLAUDE.md's
provider/peer-engine goal; a later provider extraction still needs to address the preview seam.

## Changed files

- `IDE/HexIDE.Core/IDE/IRunningProgram.cs`: contract, activity observations, form answer and empty provider.
- `IDE/HexIDE/IDE/InterpreterRunningProgram.cs`: interpreter implementation.
- `IDE/HexIDE.Runtime/EventPump.cs`: implements the read-only activity interface.
- `IDE/HexIDE/Automation/UiAutomationDriver.cs` and `OpenDialogs.cs`: form and activity questions.
- `IDE/HexIDE/IDE/ForegroundWindow.cs`: stable form names through the contract.
- `IDE/HexIDE/MainViewViewModel.cs`: Immediate output subscription.
- `IDE/HexIDE/DISetup.cs` and `App.axaml.cs`: registration and startup installation.
- `IDE/HexIDE.Desktop/Server/HexIdeTools.cs` and `ProgramPrinted.cs`: activity and output questions.
- `IDE/HexIDE.Desktop/Server/IdeShutdown.cs`: comment terminology for the guard.
- `IDE/HexIDE/VisualDesigner/ViewModels/FormEditViewModel.cs`: preview declaration for the guard.
- `IDE/HexIDE.Integration.Tests/Automation/ProgramWaitTests.cs`: contract coverage and three pump setups.
- `IDE/HexIDE.Integration.Tests/TestApp.cs` and `IDE/HexIDE.Tests/TestApp.cs`: install the test provider.
- `IDE/HexIDE.Tests/Infrastructure/RunningProgramBoundaryTests.cs` and `EmptyRunningProgramTests.cs`.
- `openspec/specs/hexide-mcp-server/spec.md`: live contract requirement inside `## Requirements`.
- This validation note.

## Skipped cases

Skipped cases are not counted as coverage. Infrastructure skips are included in the full unit list.

### HexIDE.Tests

1 skipped: A link to a share is a Windows path.

- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`

1 skipped: A made program is started on Windows only: elsewhere it needs a desktop session.

- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`

3 skipped: Job objects exist only on Windows.

- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`

25 skipped: Needs a Windows x64 test host, whose launcher stands in for a stock executable.

- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"HexIDE.exe\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"LICENSE\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"acme-ide.exe\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"brand.json\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"branding/logo.png\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"hexide-root.pub\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"nested/HEXIDE-FIRSTPARTY.PUB\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.json\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.sig\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.signed\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.json\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.sig\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`

2 skipped: Type libraries are read only on Windows.

- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`

10 skipped: VB6 template tree unavailable; VB6_TEMPLATES is unset.

- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`

23 skipped: Windows-only: this pins behaviour the product deliberately varies by host filesystem.

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`

3 skipped: macOS-only: asks macOS's own tools about the result.

- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`

### HexIDE.Integration.Tests

4 skipped: Needs Avalonia 12.1.5; pinned back to 12.1.4 for #3869

- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Inspect_TreeNode_ReportsExpandCollapseState`
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Collapse_LeafNode_RefusesClearly`
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Expand_LeafNode_RefusesClearly`
- `HexIDE.Integration.Tests.Views.ContextMenuMeasureTests.CodeMenuFitsItsShortcutsAndSubmenuArrowsOnFirstOpening`

### HexIDE.Runtime.Integration.Tests

1 skipped: Requires Windows GDI

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`

1 skipped: Requires the oracle's Windows Segoe UI font build

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`

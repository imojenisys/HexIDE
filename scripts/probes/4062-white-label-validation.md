# Issue #4062: white-label regression validation

Measured on WSL/Linux on 2026-11-16, based on `upstream/main` at `a3f9d9c48`.

The existing startup refusal and state-root behavior is preserved. The desktop gate now owns the
continuation containing subsequent startup, so refusal tests can observe whether state access starts.
Settings and log-root calculations have identity/input seams. Tests also launch the actual desktop
entry point from a broken installation and the actual bundled language server with `--log-root`,
including a path containing spaces.

## Final checks

| Check | Result |
|---|---|
| Full `HexIDE.Tests` | 8,411 passed, 70 skipped, 0 failed; 8,481 total; 34m 30s |
| xUnit report guard | 8,481 tests, no framework errors; all 8 new IDE cases passed |
| Full `HexIDE.VbLspServer.Tests` | 744 passed, 0 skipped, 0 failed |
| Restored focused IDE cases | 8 passed, 0 skipped |
| `ProviderTestSortTests` slice | 11 passed, 0 skipped |
| Debug desktop build | 0 warnings, 0 errors |
| Tree hygiene | Passed; 9 existing warnings, none blocking |
| Test-runner policy self-tests | PowerShell and Bash runners passed with the new floors |

Windows PATH entries were removed before testing, and `HEXIDE_FOREIGN_LSP_DOWNLOAD=0` was set for
IDE runs. The final full IDE run used the freshly built test output, the script's 7,684-test floor,
and `--report-xunit --report-xunit-filename 4062.xml --results-directory TestResults`.
`bash scripts/check-xunit-report.sh IDE/HexIDE.Tests/TestResults/4062.xml` passed. Source hashes stayed
unchanged throughout that final run. The measured totals then raised the CI floors to 8,056 IDE and
706 language-server tests (95%, rounded down); both full results exceed those new floors.

An earlier full IDE run completed with 8,409 passed, 70 skipped and one failure in 38m 18s. The compiler
guard caught test sources edited during that run being checked against the earlier built gate
signature. Rebuilding resolved it; the guard slice and final full run above both passed.

## Regression sensitivity

These are deliberate temporary mutations, all restored before final validation. They are not failures
on unmodified main: main already implements the behavior this issue asks to cover.

| Temporary mutation | Observed failure |
|---|---|
| Ignore the server's supplied log root | Both real-server log tests failed because the requested log directory was absent (2/2) |
| Invoke subsequent startup before validating the descriptor, keeping the refusal exit codes | Both refusal tests failed on received state-access callbacks (2/2) |
| Reverse gate exit codes, omit successful startup, and use stock folders for branded settings/logs | All 8 IDE cases failed (8/8) |

All 10 new cases were therefore shown to detect regressions, and all ran on Linux. No failing
behavioral baseline on unmodified main was demonstrated. The white-label task list remains untouched:
its literal pre-change-failure criterion is not established, and its supervisor-review box is unchanged.

## Scope and limits

No VB6 semantics, product strings, feature statuses or language-coverage counts changed.
`MISSING_FEATURES.md` and `MISSING_LANGUAGE.md` therefore stay unchanged. No changelog fragment is
needed under the latest issue comment's test-only exemption. No oracle pages were changed.
The Windows mapping branch is exercised with rooted host paths, as neighboring recovery tests do;
native Windows execution and its console-free refusal dialog were not exercised in WSL. A supervisor
live check can launch a broken installation from Explorer, confirm the refusal dialog and unchanged
state directories, then confirm an ordinary valid installation starts.

Possible drift: the latest supervisor comment's requirement that each test fail on main sits beside
the issue body's statement that the behavior is already implemented. For a coverage task, forcing an
actual behavior change to manufacture that baseline would work against correctness and the requested
small scope. This run preserves the behavior and reports mutation sensitivity separately.

## Skipped IDE cases

The 70 skips comprise 54 Windows-only cases, 3 macOS-only cases, 10 unavailable VB6 Template cases,
2 unavailable foreign debug adapters and 1 unavailable DAP schema. They are not claimed as coverage.
The exact skipped methods from the final xUnit report follow; the count includes theory cases.

| Test method | Skipped cases |
|---|---:|
| `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` | 1 |
| `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode` | 1 |
| `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session` | 1 |
| `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session` | 1 |
| `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` | 1 |
| `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named` | 1 |
| `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` | 1 |
| `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` | 1 |
| `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` | 1 |
| `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile` | 1 |
| `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole` | 1 |
| `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process` | 1 |
| `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it` | 1 |
| `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it` | 1 |
| `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused` | 1 |
| `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead` | 1 |
| `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused` | 1 |
| `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header` | 1 |
| `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched` | 1 |
| `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity` | 1 |
| `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case` | 1 |
| `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused` | 1 |
| `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main` | 1 |
| `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output` | 11 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it` | 1 |
| `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn` | 1 |
| `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation` | 1 |
| `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot` | 1 |
| `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests` | 1 |
| `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData` | 1 |

# #4068 verification — 2026-11-16, WSL

The form-host extraction is described in [the lookup audit](../../docs/runtime-form-host.md).
These are headless checks; the supervisor still owns the live IDE check listed there.

## Environment

All builds and tests used a PATH with `/mnt/` entries removed and `UseSharedCompilation=false` exported.
NuGet's HTTP cache was directed into this clone's `artifacts/nuget-http`, because the sandbox cannot write the default cache.

The first full headless attempt aborted before running tests: HangDump could not create its named mutex in the
read-only runtime directory. A standalone `new Mutex(false, "HexIDE4068Probe")` probe succeeded after setting
`XDG_RUNTIME_DIR` to a clone-local directory with mode 700. The headless retries retain `--hangdump --hangdump-timeout 20m`.
This changes the test process environment, not the product.

## Teeth

[`issue-4068-mutations.py`](issue-4068-mutations.py) runs the targeted mutations and restores each source byte-for-byte.
All five behaviour mutations were killed by their corresponding test; no test skipped.

| Removed behaviour | Test | Observed failure |
|---|---|---|
| Hold and content hit-test suppression | `HostedFormTests.A_held_window_stops_real_pointer_and_key_input_and_reports_the_press` | Held press count was 0 instead of 1 through real pointer input. |
| Program close-box refusal | `HostedFormTests.The_close_box_can_be_refused_but_the_program_can_close_without_asking` | The close callback was not called. |
| Empty extent while minimised | `HostedFormTests.A_minimised_extent_is_empty_even_with_a_pending_size_assignment` | A minimised window retained its requested non-empty size. |
| WithEvents window-to-root helper | `SingleViewFormsTests.A_WithEvents_variable_hears_a_single_view_forms_control` | Restoring the old cast produced the #1556 refusal during Load. |
| Unload window-to-root helper | `SingleViewFormsTests.Unload_Me_finds_the_runtime_root_inside_the_single_view_window` | Restoring the old cast produced the control-array-only Unload refusal. |

Both seam guards also failed with the unmodified #4066 engine and its constructor callers restored from
`f7cef45d1f3ea932d23641a8478b2ebf1a2f2c9f`:

- `ProgramHostSeamTests` identified `InterpreterRunEngine.cs` as naming forbidden host types/operations.
- `InterpreterSeamTests.The_IDE_does_not_own_runtime_form_roots` identified `InterpreterRunEngine+VBMDIFormRuntime`.

That is **five behaviour mutations and two base-engine guard failures**. The behaviour tests preserve the existing
behaviour, so their proof is removal of the extracted implementation, rather than a claim that these behaviours were
broken on the original branch.

## Completed suites on the final source

| Suite | Passed | Skipped | Total | xunit errors |
|---|---:|---:|---:|---:|
| HexIDE.Tests | 8408 | 67 | 8475 | 0 |
| HexIDE.Integration.Tests | 3656 | 4 | 3660 | 0 |
| HexIDE.Runtime.Tests | 8848 | 19 | 8867 | 0 |
| HexIDE.Runtime.Integration.Tests | 2886 | 2 | 2888 | 0 |

Both headless reports passed `scripts/check-xunit-report.sh`. `MissingLanguageDocTests` passed 20/20 with no skips;
MISSING_LANGUAGE coverage counts and test-count floors did not move. Tree hygiene passed with nine existing,
non-blocking warnings. No changelog entry was added, as the latest issue comment requests for this internal extraction.

The final headless IDE report includes `HostedFormTests` 3/3, `SingleViewFormsTests` 9/9 and
`SyntaxErrorAtStartTests` 21/21, all passing with no skips. The existing assertions in the latter two remain unchanged;
only the single-view tests' route to `IFormView` changed.

An earlier full IDE run also completed, with 8408 passed, 67 skipped and 8475 total in 37m 31s. It was repeated after
restoring the former MDI root's optional event defaults, so the table above records the final assembly's run.

## Skipped test methods

These cases provide no coverage in this WSL run. The reports include platform-specific cases, unavailable VB6 templates/oracle opt-in, a child-only stack probe,
and four tests needing Avalonia 12.1.5 while the tree is pinned to 12.1.4 for #3869; none of the new tests or seam guards skipped. Counts below preserve the number of skipped theory cases.

### HexIDE.Tests

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` (1 case).
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` (1 case).
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named` (1 case).
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` (1 case).
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` (1 case).
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` (1 case).
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile` (1 case).
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole` (1 case).
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process` (1 case).
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it` (1 case).
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it` (1 case).
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused` (1 case).
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead` (1 case).
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused` (1 case).
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header` (1 case).
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched` (1 case).
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce` (1 case).
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity` (1 case).
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case` (1 case).
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused` (1 case).
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main` (1 case).
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows` (1 case).
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows` (1 case).
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one` (1 case).
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing` (1 case).
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output` (11 cases).
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks` (1 case).
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it` (1 case).
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible` (1 case).
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it` (1 case).
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet` (1 case).
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it` (1 case).
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn` (1 case).
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation` (1 case).
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot` (1 case).
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests` (1 case).
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData` (1 case).

### HexIDE.Integration.Tests

- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Inspect_TreeNode_ReportsExpandCollapseState` (1 case).
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Collapse_LeafNode_RefusesClearly` (1 case).
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Expand_LeafNode_RefusesClearly` (1 case).
- `HexIDE.Integration.Tests.Views.ContextMenuMeasureTests.CodeMenuFitsItsShortcutsAndSubmenuArrowsOnFirstOpening` (1 case).

### HexIDE.Runtime.Tests

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte` (1 case).
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else` (1 case).
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive` (1 case).
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure` (1 case).
- `HexIDE.Runtime.Tests.DateFractionOfASecondTests.Long_date_uses_the_measured_Windows_pattern` (1 case).
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character` (1 case).
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows` (1 case).
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows` (1 case).
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows` (1 case).
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows` (1 case).
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape` (1 case).
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly` (1 case).
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack` (1 case).
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6` (1 case).
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips` (1 case).
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text` (1 case).
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does` (1 case).
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx` (1 case).
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date` (1 case).

### HexIDE.Runtime.Integration.Tests

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements` (1 case).
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent` (1 case).

## Changed files

- [`IDE/HexIDE.Core/IDE/IHostedForm.cs`](../../IDE/HexIDE.Core/IDE/IHostedForm.cs)
- [`IDE/HexIDE.Core/IDE/IProgramHost.cs`](../../IDE/HexIDE.Core/IDE/IProgramHost.cs)
- [`IDE/HexIDE.Integration.Tests/TestRunners.cs`](../../IDE/HexIDE.Integration.Tests/TestRunners.cs)
- [`IDE/HexIDE.Integration.Tests/Views/HostedFormTests.cs`](../../IDE/HexIDE.Integration.Tests/Views/HostedFormTests.cs)
- [`IDE/HexIDE.Integration.Tests/Views/SingleViewFormsTests.cs`](../../IDE/HexIDE.Integration.Tests/Views/SingleViewFormsTests.cs)
- [`IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaInteroperability.cs`](../../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaInteroperability.cs)
- [`IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs`](../../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs)
- [`IDE/HexIDE.Runtime/AvaloniaInterop/ControlFont.cs`](../../IDE/HexIDE.Runtime/AvaloniaInterop/ControlFont.cs)
- [`IDE/HexIDE.Runtime/AvaloniaInterop/IdentityMembers.cs`](../../IDE/HexIDE.Runtime/AvaloniaInterop/IdentityMembers.cs)
- [`IDE/HexIDE.Runtime/HostedFormRoot.cs`](../../IDE/HexIDE.Runtime/HostedFormRoot.cs)
- [`IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs`](../../IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs)
- [`IDE/HexIDE.Runtime/VBProgram.cs`](../../IDE/HexIDE.Runtime/VBProgram.cs)
- [`IDE/HexIDE.Runtime/VBWindowContext.cs`](../../IDE/HexIDE.Runtime/VBWindowContext.cs)
- [`IDE/HexIDE.Tests/DebugAdapter/RunEngineSeamTests.cs`](../../IDE/HexIDE.Tests/DebugAdapter/RunEngineSeamTests.cs)
- [`IDE/HexIDE.Tests/Infrastructure/InterpreterSeamTests.cs`](../../IDE/HexIDE.Tests/Infrastructure/InterpreterSeamTests.cs)
- [`IDE/HexIDE.Tests/Infrastructure/ProgramHostSeamTests.cs`](../../IDE/HexIDE.Tests/Infrastructure/ProgramHostSeamTests.cs)
- [`IDE/HexIDE.Tests/ProviderTestSort.md`](../../IDE/HexIDE.Tests/ProviderTestSort.md)
- [`IDE/HexIDE.Tests/TestRunners.cs`](../../IDE/HexIDE.Tests/TestRunners.cs)
- [`IDE/HexIDE/Automation/OpenDialogs.cs`](../../IDE/HexIDE/Automation/OpenDialogs.cs)
- [`IDE/HexIDE/Automation/UiAutomationDriver.cs`](../../IDE/HexIDE/Automation/UiAutomationDriver.cs)
- [`IDE/HexIDE/IDE/InterpreterRunEngine.cs`](../../IDE/HexIDE/IDE/InterpreterRunEngine.cs)
- [`IDE/HexIDE/IDE/ProgramHost.cs`](../../IDE/HexIDE/IDE/ProgramHost.cs)
- [`docs/MISSING_LANGUAGE.md`](../../docs/MISSING_LANGUAGE.md)
- [`docs/runtime-form-host.md`](../../docs/runtime-form-host.md)
- [`scripts/probes/issue-4068-mutations.py`](../../scripts/probes/issue-4068-mutations.py)
- [`scripts/probes/issue-4068-verification.md`](../../scripts/probes/issue-4068-verification.md)

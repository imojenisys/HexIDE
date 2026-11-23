# #1070 validation — WSL, 2026-11-23

These are HexIDE test results, not new VB6 measurements. The VB6 evidence remains in
[the Error Trapping oracle page](../../../docs/vb6-fidelity-oracle/the-error-trapping-option.md)
and [the unhandled-error page, c1](../../../docs/vb6-fidelity-oracle/an-unhandled-run-time-error.md).

## Full runs

| Suite | Passed | Failed | Skipped | Total | Duration |
|---|---:|---:|---:|---:|---|
| Runtime | 12662 | 1 | 27 | 12690 | 13m 01s 228ms |
| Runtime integration | 5345 | 0 | 50 | 5395 | 8m 21s 365ms |
| IDE | 10681 | 2 | 82 | 10765 | 50m 12s 143ms |
| IDE integration | 4629 | 0 | 0 | 4629 | 21m 35s 460ms |

The full IDE run was allowed to finish; it was not stopped early. All runs used a PATH without
Windows mount entries, `UseSharedCompilation=false` and a fresh writable `XDG_RUNTIME_DIR`.
The full IDE suites used binaries rebuilt after the wiring mutations were restored; those
mutations did not change the runtime assemblies. The later profile-classification followup was
checked by the final focused batch. Both integration suites produced xUnit reports, checked by `scripts/check-xunit-report.sh`: no xUnit errors or lost tests.

### Failures retained in the totals

- Runtime: `DebuggerTests.SynchronousResume_DeepRecursionUnwindsTheStack` exceeded its 15-second
  signal guard. The signal arrived 3,936 ms late; the guard recorded 15.7 seconds of GC pause.
  The unchanged `DebuggerTests` class then passed all 36 tests, with no skips. No timeout was
  widened. That passing recheck does not establish that the intermittent failure is fixed.
- IDE: `ProviderTestSortTests.The_listing_names_every_file_that_binds_a_provider_type_with_its_label`
  found the new provider binding in `StartupCompositionTests.cs` absent from its listing.
  The generated listing was copied as the guard instructed (one added row). All 11
  `ProviderTestSortTests` then passed, with no skips. No product code changed for this correction.
- IDE: `ProfileBundleTests.EverySettableSettingIsAllowedOrExcludedWithAReason` found
  `errorTrapping` unclassified. It was added to the exclusion-reason dictionary and profile spec,
  following the approved closed allowlist and the rule that profiles do not change runtime functionality.
  Profile loading already refused it; a new preservation row pins that unchanged refusal.
  The final combined guard run is recorded below.

## Focused checks and teeth

- `HaltOnErrorTests`: 33 passed, no skips, also covered by the full IDE-integration run.
- Options, settings, shipped-pack parity, localization coverage and missing-language doc checks:
  286 passed, no skips.
- Final combined checks, including profile policy, startup composition and provider sorting:
  **346 passed, no skips** after resolving both IDE guard failures.
- Before the runtime change, with the setting API wired and `StatementExecutor` unchanged,
  G1 and G2 did not halt, and G3 printed `g3 handler 11`. The four new oracle halt rows and
  the form-caller c1 halt test failed on their missing halt: **5 tests with pre-change teeth**.
- Temporarily disabling the new wiring made two persistence rows, Options loading, initial
  main-view wiring, and the setting change during a run fail: **5 further tests with mutation
  teeth**. The final sources were restored and rebuilt before the full IDE suites.
- Already-correct measured regression guard: G3 under Unhandled Errors prints `g3 handler 11`
  without a halt. G1/G2 under the unchanged default and seven unmeasured handler-shape rows
  preserve today's behaviour. The persistence default row also remains a guard.

## Documentation checks

- `MISSING_LANGUAGE.md`: detail notes updated; statuses and counts unchanged.
- `git diff --check`: passed.
- `bash scripts/check-tree-hygiene.sh`: passed, nine existing nonblocking warnings.
- `openspec validate --specs --strict`: fails existing long-requirement warnings. The edited
  debugger spec has the same seven warning objects as upstream; the profiles spec has the same
  six. Both were compared with upstream JSON output.
- Searches across the test projects found no old #1070 refusal assertions to replace.
  No interpreter message text changed; the four new IDE labels are localized in every shipped
  neutral pack, with a Portuguese regional override where wording differs.

## Unmeasured and live verification

No new expectations were pinned for repairing the divisor before Continue, F8 from these breaks,
class `Err.Raise`, or errors raised in a handler. The unmeasured handler shapes named in the brief
keep their existing handling. No oracle file was changed.

The supervisor's live check remains: Tools > Options > Environment > General, inspect the three
radio buttons with `dump_visual_tree` and `inspect_element`, confirm the fresh default is Break
on Unhandled Errors, choose Break on All Errors, OK, reopen and confirm the selection persisted.

## Skipped checks

These skips are not coverage. Names are grouped by method; the number records skipped theory
cases where there is more than one. None of the new Error Trapping tests skipped.

### Runtime

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`.
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`.
- `HexIDE.Runtime.Tests.DeclareCallTests.Windows_KeepsRefusal`.
- `HexIDE.Runtime.Tests.ErrorFidelityTests.KeptHresult_WindowsMessagesMatchMeasurements`.
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`.
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`.
- `HexIDE.Runtime.Tests.RecursionCostProbe.Measure`.
- `HexIDE.Runtime.Tests.RecursionCostProbe.MeasureCollectionsWithTheSameActivationsRetainedAfterReturn`.
- `HexIDE.Runtime.Tests.ShellTests.BatchFileRuns`.
- `HexIDE.Runtime.Tests.ShellTests.ConvertedPathFindsExecutableInCurrentDirectory`.
- `HexIDE.Runtime.Tests.ShellTests.ExecuteDeniedIs5`.
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`.
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`.
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`.
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`.

### Runtime integration

- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_enable_disable_and_alignment_edges` — 2 cases.
- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_measured_caption` — 19 cases.
- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_sequence_and_italic` — 2 cases.
- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_wrapping_and_center_divergence` — 4 cases.
- `HexIDE.Runtime.Integration.Tests.Views.SortedListItemsTests.Windows_keeps_every_measured_code_point` — 13 cases.
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`.
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`.
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font` — 8 cases.

### IDE

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`.
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`.
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`.
- `HexIDE.Tests.IDE.ProjectLocalStateTests.Directory_junction_is_ignored_and_never_written_through`.
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`.
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault`.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault` — 6 cases.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`.
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`.
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`.
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output` — 12 cases.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`.

### IDE integration

None.

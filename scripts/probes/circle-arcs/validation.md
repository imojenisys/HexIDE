# Validation for #4771 (2026-11-19)

Base: `upstream/main` / `f32fd19b5`. No runtime product source changed.
The latest issue comment explicitly accepts a comparison-only result when none of
the candidates reproduces every grid. No commit or push was made in this run.

- `python3 scripts/probes/circle-arcs/compare.py`: 63 candidates, 22 grids; no exact
  candidate. Best total: 52 differing pixels. [Per-grid minima and ties](results.md).
  Regenerating the report and comparing it byte-for-byte succeeded.
- `python3 -m unittest discover -s scripts/probes/circle-arcs -p 'test_*.py'`:
  6 passed (probe mechanics, not interpreter fidelity coverage).
- Full `HexIDE.Runtime.Tests`: 10,762 total, 10,739 passed, 23 skipped, 0 failed.
- Full `HexIDE.Runtime.Integration.Tests`, including the new guards: 4,125 total,
  4,123 passed, 2 skipped, 0 failed. The earlier run before adding the guards also
  passed (4,121 total). `scripts/check-xunit-report.sh` accepted both reports;
  the runtime-unit report also passed that check.
- Full `HexIDE.Tests`: 10,425 total, 10,351 passed, 74 skipped, 0 failed, in
  30m 40s. The run was not interrupted. Its xunit report passed
  `scripts/check-xunit-report.sh`.
- Filtered `HexIDE.Tests`: `MissingLanguageDocTests` 20/20, `DesignRecordTests` 6/6,
  `ChangelogFragmentTests` 3/3. No skips or failures.
- `bash scripts/check-tree-hygiene.sh`: passed, nine existing non-blocking warnings.
- `git diff --check`: passed.

## Teeth and unchanged behaviour

Zero changed interpreter behaviours, so no new failing-before-fix grid tests apply.
The four new single-angle cases in `CircleDrawingTests` pass against unchanged main
runtime code: start-only and end-only, each on a form and PictureBox. They are
regression guards for the issue's explicit refusal requirement, not claimed teeth.
The existing five arc/sector refusal cases on both targets still pass. The 12
full-circle grids and filled circle still pass on both targets in that class.

Arcs/sectors remain refused with exactly the existing #988 message. Unequal scale
units and DrawWidth above 1 remain refused. A one-angle call remains refused because
the other angle's default is unmeasured. No oracle files or MISSING_LANGUAGE counts
changed. No API surface, UI or message text changed; no live UI check is required
for this comparison-only result. Possible drift: none.

## Test environment

All builds/tests dropped `/mnt/` entries from PATH, exported
`UseSharedCompilation=false`, and used a fresh writable `XDG_RUNTIME_DIR`.
Initial attempts failed before tests ran because of read-only build/cache writes.
The successful runs additionally used `MSBUILDDISABLENODEREUSE=1`,
`DOTNET_CLI_USE_MSBUILD_SERVER=0`, a clone-local `NUGET_HTTP_CACHE_PATH` and
`--artifacts-path` inside this clone. A retry using `-nr:false` was rejected by MTP
as an unknown test option; removing it allowed the suites to run. These failed
setup attempts are not counted as tests or skips.

## Skips

Skipped tests prove no coverage. Exact skipped test names follow.

### HexIDE.Runtime.Tests

- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `HexIDE.Runtime.Tests.ShellTests.ExecuteDeniedIs5`
- `HexIDE.Runtime.Tests.ShellTests.ConvertedPathFindsExecutableInCurrentDirectory`
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`
- `HexIDE.Runtime.Tests.ShellTests.BatchFileRuns`

### HexIDE.Runtime.Integration.Tests

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`

### HexIDE.Tests

- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"acme-ide.exe\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.json\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"branding/logo.png\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"hexide-root.pub\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"LICENSE\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"brand.json\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.json\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.sig\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"HexIDE.exe\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.signed\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.sig\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"nested/HEXIDE-FIRSTPARTY.PUB\")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`

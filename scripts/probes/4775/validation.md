# #4775 validation

WSL, 2026-11-19, `fix/4775-codex` based on `33cbe5281`. No product code changed.
Every check in the table passed with exit code 0. Windows checks were not run here; cleanup limits are recorded separately below.

Before builds/tests, Windows PATH entries were removed and `UseSharedCompilation=false` was exported.
`MSBUILDDISABLENODEREUSE=1` disabled build-node reuse; NuGet's HTTP cache was
redirected into this clone after the first restore found the user cache read-only. Test command groups
used a writable `XDG_RUNTIME_DIR` under the temporary directory.

## Checks

| Check | Total | Passed | Skipped | Failed | Duration |
|---|---:|---:|---:|---:|---|
| Runtime, Release (final diagnostic set) | 10847 | 10821 | 26 | 0 | 4m 49s 976ms |
| Runtime, Debug | 10846 | 10821 | 25 | 0 | 5m 49s 341ms |
| Runtime integration | 4170 | 4168 | 2 | 0 | 2m 48s 082ms |
| IDE, full | 10442 | 10364 | 78 | 0 | 30m 54s 626ms |
| ErrorUnwindTests, Release | 11 | 11 | 0 | 0 | 12s 588ms |
| GoSubTests, Release | 42 | 42 | 0 | 0 | 5s 677ms |
| UiThreadTests, Release | 6 | 5 | 1 | 0 | 8s 421ms |
| MissingLanguageDocTests | 20 | 20 | 0 | 0 | 3s 839ms |
| ChangelogFragmentTests | 3 | 3 | 0 | 0 | 2s 651ms |
| Final strengthened guard, Debug | 1 | 1 | 0 | 0 | 7s 977ms |
| Final strengthened guard, Release | 1 | 1 | 0 | 0 | 6s 077ms |

The Debug full run preceded addition of the second, opt-in retained-activation diagnostic. The final
Release full run includes both diagnostic skips. The cap assertion was then strengthened to print
**every** depth at or above the cap (so going past it cannot pass unnoticed), and rerun in both builds.
No product or allocation-path code changed after the full runs.

- The main diagnostic was run explicitly in each of the five modes recorded in `results/`: ordinary,
  raised budget, no-GC, runaway, and runaway without GC. Each passed its depth/error assertions.
- The retained-activation diagnostic was run explicitly in Release and passed at all five depths.
- The runtime integration xunit report recorded all 4,170 cases, with no errors
  (`scripts/check-xunit-report.sh`).
- `MissingLanguageDocTests`: counts unchanged; only `Sub`'s Detail note changed.
- The changelog fragment guard passed after creation of the fragment.
- Hygiene passed with nine existing warnings and no blocking errors. `git diff --check` passed.
- All five IDE test projects were searched for #4775 references. There was no old refusal to replace;
  no interpreter or IDE message text changed.
- No public API declarations, test-count floors, oracle pages or archived specs changed.

## Teeth

**Zero failure-before-fix teeth claimed.** No product behaviour changed. The new structural allocation
regression guard passes on the unchanged main runtime, in Debug and Release. It guards linear allocation
growth and the real Error 28 depth cap; it does not assert a one-second timing or claim to fix GC.
The opt-in diagnostics are measurements, not ordinary-suite coverage. Their skips below are not counted
as proof; their explicitly enabled runs supply the recorded data.

## Remaining

Repeat the Release table and `ErrorUnwindTests`, `GoSubTests` and `UiThreadTests` on Windows. WSL does
not establish the cause or timings of the original Windows reproduction. Native CPU sampling was
refused by the sandbox; GC events, interventions and the retained-activation experiment establish the
GC/deep-stack cost here, without identifying its exact native routine. No process GC configuration or
runtime allocation optimisation is delivered. See the investigation's runtime-cause alternative and
its limits in [README.md](README.md).

Possible drift: none.

## Exact skips

These are exclusions, not claimed coverage. The child-only UiThread test is intentionally skipped in
the ordinary suite; the parent Linux process test passed. Other exclusions require Windows/macOS,
VB6's templates/compiler, or explicit activation of the diagnostic.

### Runtime, final Release

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `HexIDE.Runtime.Tests.ErrorFidelityTests.KeptHresult_WindowsMessagesMatchMeasurements`
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `HexIDE.Runtime.Tests.RecursionCostProbe.Measure`
- `HexIDE.Runtime.Tests.RecursionCostProbe.MeasureCollectionsWithTheSameActivationsRetainedAfterReturn`
- `HexIDE.Runtime.Tests.ShellTests.BatchFileRuns`
- `HexIDE.Runtime.Tests.ShellTests.ConvertedPathFindsExecutableInCurrentDirectory`
- `HexIDE.Runtime.Tests.ShellTests.ExecuteDeniedIs5`
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`

### Runtime integration

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`

### IDE, full

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "CancellationWireShapeTests")`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "RenameWireShapeTests")`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "ShutdownWireShapeTests")`
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
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
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.signed")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`

## Cleanup

The sandbox's automatic command review rejected recursive `rm` cleanup: “rm -f style commands are
not permitted. Use a safer approach”. Standard `dotnet clean` removed local build outputs, but its
Antlr `CustomBeforeClean` target then tried to remove the shared user JRE cache and was denied
(MSB3231). No attempt was made to change that shared cache. Some intermediate metadata remains;
all test results above were obtained before cleanup. The standalone trace-summary clean succeeded.

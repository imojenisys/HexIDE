# #4863 validation (WSL, 2026-11-20)

Source baseline: `d1fb96f26`, matching `upstream/main` at the start of the run.
Scope: [the supervisor comment on #4863](https://github.com/hexide-io/HexIDE/issues/4863).
No new VB6 measurements were made. The Err.Raise assertion cites
[the existing oracle page](../../docs/vb6-fidelity-oracle/when-err-is-cleared-and-what-a-running-handler-traps.md).

## Teeth

With only the test delay and new theory added, both rows failed before the hook and wait were added:

- `Err.Raise 5`, preparation delay 4000 ms: expected crashed, got ran to the time limit.
- `Do` / `Loop`: expected detail containing `Sub Main started.`, got null.

After the change, all 32 rows of `ProjectSmokeRunnerTests` passed, none skipped. This includes all
nine existing isolated-startup rows, seven startup-Timer rows and three preparation-budget facts.
Those existing rows are regression guards, not newly failing tests.

## Corpus comparison

The specified corpus sweep ran in full before and after: 37 projects, passing both times.
The baseline used the xUnit assembly runner (`-method`); the after run used `dotnet test` with
`--filter-method`. Both invoke the same test and isolated workers. The baseline took 298 seconds;
the after run took 433 seconds. No project or expectation was changed.

Every Sub Main status change is below. All twelve before/after details are null.

| Project | Before | After |
|---|---|---|
| `demo/battleship/Battleship.vbp` | ran to the time limit | ran to completion |
| `corpus/member-attributes/round-trip/Acc2/Acc2.vbp` | ran to the time limit | ran to completion |
| `corpus/member-attributes/round-trip/Probe2/Probe2.vbp` | ran to the time limit | ran to completion |
| `corpus/member-attributes/round-trip/Tar2/Tar2.vbp` | ran to the time limit | ran to completion |
| `corpus/member-attributes/evidence/LoadShapes/saved/LoadShapes.vbp` | ran to the time limit | ran to completion |
| `corpus/member-attributes/evidence/Load2/saved/Load2.vbp` | ran to the time limit | ran to completion |

Of the other named candidates, `corpus/member-attributes/round-trip/Extras/Extras.vbp` completed
in both runs (null detail). `corpus/member-attributes/evidence/Const2/saved/Const2.vbp` was refused
in both, with the same detail:

```text
Compile error:

Constants, fixed-length strings, arrays, user-defined types and Declare statements not allowed as Public members of object modules
```

`demo/battleship-forms/BattleshipForms.vbp` remained at the time limit, with detail changing
from null to `Sub Main started.` No new crash was exposed.

One form project also changed: `demo/neon-aurora/NeonAurora.vbp` was refused before with
`Run-time error '53':` followed by a blank line and `File not found: gdi32`; after, it reached the
time limit with `the startup form did not show within 30 seconds.` The form path was not changed.

## Full checks

- `HexIDE.Runtime.Tests`: 11,523 total; 11,496 passed, 27 skipped, no failures.
- `HexIDE.Runtime.Integration.Tests`: 4,547 total; 4,537 passed, 10 skipped, no failures.
  `check-xunit-report.sh` confirmed all 4,547 rows recorded and no xUnit errors.
- Full `HexIDE.Tests`: 10,551 total; 10,473 passed, 78 skipped, no failures (45m 20s).
- Explicit `MissingLanguageDocTests`: 20 passed, none skipped; counts unchanged.
- Tree hygiene passed with nine existing warnings; `git diff --check` passed.
- No interpreter/IDE error text was removed or changed; searches across the test projects found
  no old #4863 gap assertion to update. The added English text is a smoke-test report, not IDE chrome.
- No language support status changed; `MISSING_LANGUAGE.md` rows and counts are unchanged.
- No public API was added, and no live UI check is needed for this test-harness change.
- Left unimplemented: none in the chosen scope. Running to completion within a different bound remains
  outside this run's deliverable. Possible drift: none.

## Skipped tests

These rows prove nothing in this sandbox. Names below come from the completed test logs.

### Runtime

- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `HexIDE.Runtime.Tests.DeclareCallTests.Windows_KeepsRefusal`
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `HexIDE.Runtime.Tests.ErrorFidelityTests.KeptHresult_WindowsMessagesMatchMeasurements`
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`
- `HexIDE.Runtime.Tests.RecursionCostProbe.MeasureCollectionsWithTheSameActivationsRetainedAfterReturn`
- `HexIDE.Runtime.Tests.RecursionCostProbe.Measure`
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`
- `HexIDE.Runtime.Tests.ShellTests.ConvertedPathFindsExecutableInCurrentDirectory`
- `HexIDE.Runtime.Tests.ShellTests.ExecuteDeniedIs5`
- `HexIDE.Runtime.Tests.ShellTests.BatchFileRuns`

### Runtime integration

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1: Set b = Toolbar1.Butto"···, property: "b.Height", measured: 329.95280000000002)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1: Set b = Toolbar1.Butto"···, property: "Toolbar1.Height", measured: 420)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1: Set b = Toolbar1.Butto"···, property: "b.Width", measured: 870.23630000000003)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1", property: "Toolbar1.ButtonHeight", measured: 569.76430000000005)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \"Alpha\")", property: "b.Width", measured: 569.76379999999995)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \"Alpha\")", property: "Toolbar1.Height", measured: 660)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \"Alpha\", 5)", property: "b.Width", measured: 794.8347)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \"Alpha\")", property: "b.Height", measured: 569.76379999999995)`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`

### IDE

- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.signed")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "RenameWireShapeTests")`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "ShutdownWireShapeTests")`
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "CancellationWireShapeTests")`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`

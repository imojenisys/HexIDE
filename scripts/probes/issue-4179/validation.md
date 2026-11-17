# #4179 validation — 2026-11-16/17, Ubuntu / WSL

Behaviour source: [the revised issue decision](https://github.com/hexide-io/HexIDE/issues/4179),
including [oracle PR #4350](https://github.com/hexide-io/HexIDE/pull/4350). No new VB6 measurements were made.

All builds/tests exported `UseSharedCompilation=false` and removed `/mnt/` entries from `PATH`.
HangDump initially failed before testing because the sandbox cannot create its named mutex under
`/run/user`. Exporting `XDG_RUNTIME_DIR` to a private directory under this clone's ignored `artifacts/`
allowed the headless runs to complete. No product workaround was added.

## Completed checks

| Check | Result |
|---|---|
| Full `HexIDE.Tests` | 9,128 passed, 68 skipped, 2 failed, 9,198 total; 49m58s; xUnit report checked, no errors |
| Provider-test inventory guards (IDE and integration) | 1 passed in each; generated missing save-test row added |
| Isolated debugpy disconnect rerun | 1 passed |
| Full `HexIDE.Runtime.Tests` | 8,987 passed, 19 skipped, 9,006 total; 9m18s |
| Full `HexIDE.Runtime.Integration.Tests` | 2,947 passed, 2 skipped, 2,949 total; 3m48s; xUnit report checked, no errors |
| `NegativeSizeSaveTests` (opening, save/project/group, cancellation, automation, recovery, packaging copy) | 12 passed in both scripted baseline and rebuilt restore, none skipped |
| `ShippedPackParityTests` | 32 passed, none skipped |
| `LocalizationCoverageTests` | 13 passed, none skipped |
| `MissingLanguageDocTests` | 20 passed, none skipped; no count changes |
| `NegativeSizeLoadTests` | 4 passed, none skipped |
| `SerializationRoundTripTests` | 87 passed, none skipped |
| `NegativeSizeRunTests` (Standalone resources and loader) | 3 passed, none skipped |
| `StartupFormFailsToLoadTests` (including updated old gap assertion) | 3 passed, none skipped; xUnit report checked, no errors |
| `RunFaultTests.Negative_saved_sizes_run_as_zero` (F5 runner, with and without designer) | 2 passed, none skipped; xUnit report checked, no errors |
| Tree hygiene | Passed; 9 existing warnings; identity scan unavailable without `HEXIDE_IDENTITY_PATTERN` |
| OpenSpec strict validation | Exit 0; existing long-requirement warnings, none on the added requirement |

The full IDE run failed on the new save-test class's missing generated provider-inventory row and on
`ForeignAdapterTests.Debugpy_runs_a_complete_session` (adapter exited 1 with a broken pipe while sending
`debugpySockets` during disconnect). The inventory was regenerated and its guard passed; the integration
inventory guard also passed. The debugpy case passed its isolated rerun. DAP code and its test were not
changed, and no claim is made that main was separately run for this failure.

## Mutation checks

The issue-named `mutation-check.ps1` is available through WSL's mounted tools folder.
The initial manual source mutations below restored the original source with fresh timestamps after each run;
the required scripted checks below use that helper's green baseline, failing mutant and green rebuilt restore.

- Removing only `val = 0` makes both negative-size `NegativeSizeRunTests` cases fail with the original
  Avalonia `ArgumentException` in `VBLoader.PlaceComponentTree`; the Height=0 control still passes.
- Removing the recovery block makes both negative-size `NegativeSizeLoadTests` cases fail; the two
  zero/small-size controls still pass. Restoring the block returns all four to passing.

### Scripted results

| Mutation | Baseline | Mutant | Rebuilt restore |
|---|---|---|---|
| Remove only `val = 0`, `NegativeSizeRunTests` | 3 passed | 2 failed, 1 passed | 3 passed; helper exit 0 |
| Remove only `form.LoadedWithErrors = true`, `NegativeSizeSaveTests` | 12 passed | 10 failed, 2 passed | 12 passed; helper exit 0 |
| Remove the full recovery block, fixed six-slice regression matrix | 40 passed | 18 failed, 22 passed | 40 passed; helper exit 0 |
| Restore the old automation refusal explanation, same matrix | 40 passed | 6 failed, 34 passed | 40 passed; helper exit 0 |

That is **24 distinct cases with teeth**: 18 recovery regressions and six program-facing wording cases.
The other 16 matrix cases are unchanged controls and are not claimed as regression teeth. Every selected
case runs on Linux; none skipped. The IDE diagnostic uses the translated key in all 39 declared packs;
noninteractive refusals and automation replies are English because programs read them.

The additional regression matrix rebuilds the changed implementation assembly and copies its fresh output
into test projects while retaining the fully built host dependencies (`BuildProjectReferences=false`).
The mutations change method bodies, keeping those host APIs stable. The fixed slice in
`check-regressions.ps1` enforces each selected class's observed case count; global CI floors are untouched.

## Skips are not coverage

The full runtime suite skipped these 19 cases (class names below are in `HexIDE.Runtime.Tests`):

- `VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `UiThreadTests.Child_ReportsTheUiThreadsStack`
- `MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`
- `ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `FileSystemTests.Windows_attribute_rows`
- `FileSystemTests.Windows_error_rows`
- `FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `FileSystemTests.Windows_name_rows`
- `FileSystemTests.Windows_numeric_rows`
- `Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `DateFractionOfASecondTests.Long_date_uses_the_measured_Windows_pattern`

The full runtime integration suite skipped these two Windows font/GDI cases:

- `ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`
- `ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`

## Supervisor live check

Open the measured negative-Width and negative-Height forms: check the localized load diagnostic names
both controls and their Begin lines, both controls remain present and editable, and F5 prints zero both
before and after opening the designer. Save the form, project and group: choose a new path and verify
zero in the output, the new project reference, and unchanged original bytes/time. Cancel a picker and
verify no write; save again after successful Save As and verify no picker. A clean form must save in
place, and Height=0 must run without a diagnostic. This live check remains the supervisor's to perform.

Non-numeric size conversions, root form dimensions, `.ctl`/`.pag` size recovery, designer Properties edits,
size floors and repeating the
load diagnostic on a second F5 were not newly measured or changed. MISSING_LANGUAGE status counts do
not move; the existing Width/Height row's detail records the implemented file-load recovery.

## Full IDE suite skips

These 68 cases did not run and are not cited as coverage:

- `Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`
- `Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`
- `Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`
- `LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`
- `Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`
- `Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`
- `Repack.WindowsRepackerTests.Included_missing_folder_is_refused`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"LICENSE\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.json\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"acme-ide.exe\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"nested/HEXIDE-FIRSTPARTY.PUB\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"HexIDE.exe\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.signed\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"branding/logo.png\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.sig\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"hexide-root.pub\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.sig\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.json\")`
- `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"brand.json\")`
- `Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`
- `Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`
- `Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`
- `Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`
- `Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`
- `Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`
- `Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`
- `Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`
- `Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`
- `Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
- `Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`
- `Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`
- `Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`
- `Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`
- `Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`
- `Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`
- `Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`
- `Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`
- `Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `UserDataPathTests.OnWindowsItIsUnderAppData`
- `Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`
- `Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`
- `Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`

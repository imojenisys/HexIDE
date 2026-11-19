# #4821 validation

Checked under WSL Ubuntu on 2026-11-19, from baseline `3ad2675d6`.

The host-table facts come from [issue #4821](https://github.com/hexide-io/HexIDE/issues/4821):
RDCore.SDK `PredefinedConditionalConstants.cs` (`Of`) at SDK 0.1.8-alpha and Excel for Mac 16.115.
This run made no new VB6 measurements.

## Results

| Check | Passed | Failed | Skipped | Total |
|---|---:|---:|---:|---:|
| Runtime | 10538 | 0 | 20 | 10558 |
| IDE | 10338 | 0 | 73 | 10411 |
| Runtime integration | 3853 | 0 | 2 | 3855 |
| Rendered greying class | 39 | 0 | 0 | 39 |
| Target persistence class | 13 | 0 | 0 | 13 |
| Language inventory guard | 20 | 0 | 0 | 20 |
| Language pack parity class | 32 | 0 | 0 | 32 |
| Interpreter seam class | 2 | 0 | 0 | 2 |
| SDK reachable-type guard | 1 | 0 | 0 | 1 |

All xUnit reports have zero assembly errors. The full suites ran to completion; the final full IDE
run took 55 minutes 13 seconds (10,411 total).
The first full IDE run found the product-name allowlist missing RDCore and the new enum missing
its SDK baseline/analyzer section, plus an editor reference crossing the interpreter seam.
The declarations were completed and the shared numeric tables moved to Core before the full rerun.

Every build/test exported `UseSharedCompilation=false` and dropped `/mnt/` entries from `PATH`.
Each test group used a fresh writable `XDG_RUNTIME_DIR`. NuGet used a writable HTTP cache inside this clone.
A runtime-integration build reported a read-only reference assembly in this clone; its retry used
`MSBUILDDISABLENODEREUSE=1` and a separate `dotnet build -m:1 -nodeReuse:false`, then ran tests
with `--no-build`. The native test runner forwards raw MSBuild flags to its test host.
The full IDE run used a 45-minute quiet-time hang dump; runtime integration used 10 minutes,
and the rendered greying class used 20 minutes.

## Teeth

For the new API tests, main’s behavior was temporarily restored while retaining the new API
declarations needed for compilation. Production files were restored after the check.
The additional product-name guard was checked with the two affected packs restored to main.
The unknown target containing an equals sign was checked against the previous reserved-key test.

| Changed behavior | Failing cases before the change |
|---|---:|
| Runtime host tables, defined names and NavalBattle-shaped routing | 9 |
| Target persistence and verbatim unknown values | 7 |
| Editor host tables and default selections | 7 |
| Contributed project type selection | 2 |
| Rendered RDCore greying through the selector and dispatcher timer | 1 |
| Shared product name in the pack guard | 1 |
| **Total** | **27** |

Seventeen added cases already pass with main’s behavior: the RDCore Windows numeric row;
VBA’s undefined Linux names; the no-target Mac Declare route; the existing override precedence;
unchanged VB6, Windows VBA7 and unmeasured-host fallback tables; an unrelated contributed type;
the no-target rendered route on Linux; and UTF-8 records for paths named Target or Target=Future.
These are regression guards, not claimed fixes.

## Scope and remaining host checks

The key selects predefined constants, not a run engine. A contributed `.rdproj` is still refused
by the interpreter run engine. No oracle pages, compiler binaries or test floors were changed.
The inventory detail notes changed; its status counts did not.

32-bit VBA7 on macOS and VBA7 on Linux remain unmeasured and retain the Windows tables, as the
issue directs. There are no newly refused members or remaining implementation points.

Actual Windows execution and a live macOS IDE check remain for the supervisor: open a `.vbp`
with `Target=RDCore` and confirm RDCore is selected and the Timer branch is active; with
`VbaBitness=64` and no target key, confirm VBA 64-bit is selected and the Declare branch is active.
A project opened through the `.rdproj` type should default to RDCore greying.

Tree hygiene passes with the nine existing warnings. Strict spec validation has no errors and
only pre-existing requirement-length warnings (54 interpreter, 23 serialization, 16 editor;
the baseline editor spec had 17). Possible drift: none.

## Skipped tests

Skipped tests are not counted as coverage. No new host-table, persistence or greying test skips.

### Runtime

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`

### IDE

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
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
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`

### Runtime integration

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`

## Files changed

- `IDE/HexIDE.Core/.editorconfig`
- `IDE/HexIDE.Core/Language/PredefinedCompilationConstants.cs`
- `IDE/HexIDE.Core/ProjectElements/ProjectDefinition.cs`
- `IDE/HexIDE.Core/ProjectElements/ProjectTarget.cs`
- `IDE/HexIDE.Core/PublicAPI.Unshipped.txt`
- `IDE/HexIDE.Integration.Tests/Views/ReadOnlyGreyingTests.cs`
- `IDE/HexIDE.Runtime.Tests/HostPredefinedConstantsTests.cs`
- `IDE/HexIDE.Runtime.Tests/VbaBitnessTests.cs`
- `IDE/HexIDE.Runtime/Interpreter/ConditionalCompilation/CompilationConstants.cs`
- `IDE/HexIDE.Runtime/Serialization/ProjectDeserializer.cs`
- `IDE/HexIDE.Runtime/Serialization/ProjectEncodingRecords.cs`
- `IDE/HexIDE.Runtime/Serialization/ProjectLoader.cs`
- `IDE/HexIDE.Runtime/Serialization/ProjectSerializer.cs`
- `IDE/HexIDE.Tests/Projects/ContributedProjectTests.cs`
- `IDE/HexIDE.Tests/Serialization/VbaBitnessPersistenceTests.cs`
- `IDE/HexIDE.Tests/ShippedPackParityTests.cs`
- `IDE/HexIDE.Tests/ViewModels/CodeEditorViewModelTests.cs`
- `IDE/HexIDE/Forms/ViewModels/CodeEditorViewModel.cs`
- `IDE/HexIDE/Forms/Views/CodeEditorView.ConditionalCompilation.cs`
- `IDE/HexIDE/Forms/Views/CodeEditorView.axaml`
- `IDE/HexIDE/Localization/Packs/ar.json`
- `IDE/HexIDE/Localization/Packs/bg.json`
- `IDE/HexIDE/Localization/Packs/ca.json`
- `IDE/HexIDE/Localization/Packs/cs.json`
- `IDE/HexIDE/Localization/Packs/da.json`
- `IDE/HexIDE/Localization/Packs/de.json`
- `IDE/HexIDE/Localization/Packs/el.json`
- `IDE/HexIDE/Localization/Packs/en.json`
- `IDE/HexIDE/Localization/Packs/es.json`
- `IDE/HexIDE/Localization/Packs/et.json`
- `IDE/HexIDE/Localization/Packs/eu.json`
- `IDE/HexIDE/Localization/Packs/fi.json`
- `IDE/HexIDE/Localization/Packs/fr.json`
- `IDE/HexIDE/Localization/Packs/gl.json`
- `IDE/HexIDE/Localization/Packs/he.json`
- `IDE/HexIDE/Localization/Packs/hr.json`
- `IDE/HexIDE/Localization/Packs/hu.json`
- `IDE/HexIDE/Localization/Packs/id.json`
- `IDE/HexIDE/Localization/Packs/it.json`
- `IDE/HexIDE/Localization/Packs/ja.json`
- `IDE/HexIDE/Localization/Packs/ko.json`
- `IDE/HexIDE/Localization/Packs/lt.json`
- `IDE/HexIDE/Localization/Packs/lv.json`
- `IDE/HexIDE/Localization/Packs/nb.json`
- `IDE/HexIDE/Localization/Packs/nl.json`
- `IDE/HexIDE/Localization/Packs/pl.json`
- `IDE/HexIDE/Localization/Packs/pt.json`
- `IDE/HexIDE/Localization/Packs/ro.json`
- `IDE/HexIDE/Localization/Packs/ru.json`
- `IDE/HexIDE/Localization/Packs/sk.json`
- `IDE/HexIDE/Localization/Packs/sl.json`
- `IDE/HexIDE/Localization/Packs/sr-Latn.json`
- `IDE/HexIDE/Localization/Packs/sv.json`
- `IDE/HexIDE/Localization/Packs/th.json`
- `IDE/HexIDE/Localization/Packs/tr.json`
- `IDE/HexIDE/Localization/Packs/uk.json`
- `IDE/HexIDE/Localization/Packs/vi.json`
- `IDE/HexIDE/Localization/Packs/zh-Hans.json`
- `IDE/HexIDE/Localization/Packs/zh-Hant.json`
- `IDE/HexIDE/Projects/ContributedProjects.cs`
- `changelog.d/4821.md`
- `docs/MISSING_FEATURES.md`
- `docs/MISSING_LANGUAGE.md`
- `openspec/specs/code-editor/spec.md`
- `openspec/specs/interpreter-core/spec.md`
- `openspec/specs/serialization-round-trip/spec.md`
- `scripts/probes/4821-validation.md`

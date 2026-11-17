# Issue #4399 validation

Checked in WSL on 2026-11-17 against the reviewed #4302 base `1c0932803`.
Measurements come from [#4399](https://github.com/hexide-io/HexIDE/issues/4399), oracle measuring PR #4398;
this run made no oracle measurements and changed no oracle pages.

Windows PATH entries were removed, `UseSharedCompilation=false` was exported, and each test session
used a writable temporary `XDG_RUNTIME_DIR`. Commands below run from `IDE/`.

## Tests and teeth

- Before the product change, all 39 new `VbaBitness32Tests` cases failed at the startup refusal.
- Runtime `--filter-class '*VbaBitness*'`: 61 passed, no skips.
- Full `HexIDE.Runtime.Tests`: 9,700 passed, 21 skipped, total 9,721.
- Full `HexIDE.Runtime.Integration.Tests`: 3,014 passed, 2 skipped, total 3,016; xUnit report has no errors.
- `HexIDE.Tests --filter-class '*MissingLanguageDocTests'`: 20 passed, no skips; inventory counts unchanged.
- Full `HexIDE.Tests`: 9,277 passed, 68 skipped, total 9,345, in 27m 19s; xUnit report has no errors.
- IDE `--filter-class '*VbaBitness*'`: 6 passed, no skips.

The requested mutation-check PowerShell script ran three baseline/mutant/restored cycles:

| Mutation | Baseline | Mutant | Restored |
|---|---|---|---|
| Map 32-bit LongPtr to LongLong | 13 passed | T1 failed, 12 passed | 13 passed |
| Set 32-bit Win64 to 1 | 6 passed | W1 failed, 5 passed | 6 passed |
| Set 32-bit VBA6 to 0 | 6 passed | W3 failed, 5 passed | 6 passed |

Every cycle rebuilt, restored the source byte for byte with a fresh modification time, and passed afterwards.
Filters were `*VbaBitness32Tests.MeasuredRow` and `*VbaBitness32Tests.MeasuredConstant`, respectively.

`bash scripts/check-tree-hygiene.sh` passed with nine pre-existing warnings. `git diff --check` passed.

## Limits

P1–P3 exercise loading actual Declare syntax on Linux, with and without PtrSafe and with an As LongPtr
result. Native DLL invocation remains the existing Declare deferral, so no GetTickCount runtime value
is claimed as covered. No test added by this change skips under Linux. No new diagnostic strings were added;
the existing program-facing English errors remain in use.

K1/K5 retain the interpreter's existing parser diagnostic wording (#4415); tests assert the compile-error type,
not VBA's literal "Syntax error" text, as the base branch's LongLong tests do.

DefLngLng's x86 diagnostic text was not read: its test preserves the VB6 path as reasoned, as the brief asks.
LongPtr UDT layout, array-bound and For-counter edges remain unmeasured and have no new rules or expectations.
The language-server grammar remains outside this change (#4365). No UI was changed. Possible drift: none.

## Skipped tests

These are exclusions, not coverage for this change.

### Runtime

- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`: Set HEXIDE_ORACLE=1 to compile round-tripped forms with VB6.
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`: Runs only as the child of OnLinux_TheUiThreadsStack_IsTheSameWhateverUlimitSays
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.DateFractionOfASecondTests.Long_date_uses_the_measured_Windows_pattern`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`: Windows-only: this pins behaviour the product deliberately varies by host.

### Runtime integration

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`: Requires the oracle's Windows Segoe UI font build
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`: Requires Windows GDI

### IDE

- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"LICENSE\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"hexide-root.pub\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.sig\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.signed\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"branding/logo.png\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.sig\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"acme-ide.exe\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.json\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"nested/HEXIDE-FIRSTPARTY.PUB\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"HexIDE.exe\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"brand.json\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.json\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`: A made program is started on Windows only: elsewhere it needs a desktop session.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`: A link to a share is a Windows path.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.

## Files changed

- `IDE/HexIDE.Runtime.Tests/VbaBitnessTests.cs`
- `IDE/HexIDE.Runtime/Interpreter/BaseTypeMapper.cs`
- `IDE/HexIDE.Runtime/Interpreter/BasicInterpreter.cs`
- `IDE/HexIDE.Runtime/Interpreter/ConditionalCompilation/CompilationConstants.cs`
- `IDE/HexIDE.Runtime/Interpreter/PrePass.cs`
- `IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs`
- `IDE/HexIDE.Runtime/Interpreter/VB6BuiltIns.Conversion.cs`
- `IDE/HexIDE.Runtime/Interpreter/VB6BuiltIns.cs`
- `IDE/HexIDE.Runtime/Interpreter/VB6Visitor.cs`
- `IDE/HexIDE.Tests/DebugAdapter/VbaBitnessEngineTests.cs`
- `docs/MISSING_LANGUAGE.md`
- `docs/interpreter-gaps.md`
- `openspec/specs/interpreter-core/spec.md`
- `IDE/HexIDE.Runtime.Tests/VbaBitness32Tests.cs`
- `changelog.d/4399-vba7-32-bit.md`
- `scripts/probes/4399-validation.md`

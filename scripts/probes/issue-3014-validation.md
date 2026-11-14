# #3014 validation (2026-11-14, WSL)

Baseline: `16d474e4`, equal to `upstream/main` when this run began.

## Evidence and limits

The latest supplied issue comment contains hexide-int's native/p-code layout table from
[measuring PR #3958](https://github.com/hexide-io/HexIDE/pull/3958). Its claimed new oracle-page
section is absent from this checkout. The existing oracle page measures controls and answers,
not layout, so the implementation cites the issue's supplied measurements and leaves that
author's page untouched. No new VB6 measurement was made here.

The managed content follows the four measured rows. Native caption/close-box placement is
left to Avalonia and the window manager on Windows, Linux and macOS; no native caption
relocation or custom title bar was added. Actual native chrome placement has not been
checked here. The supervisor's Windows live check must inspect all four boxes (styles 35,
524323, 1048611 and 1572899): icon side, prompt alignment, button order, caption and close box,
then press a button and confirm its answer. Also check that an RTL IDE language does not
change the no-flag program box.

RTL Windows UI, multiline prompts, owner-form layout, and flags alongside invalid style
fields remain unmeasured; no new expectations for them were pinned. InputBox is unchanged.

## Teeth

With the new API retained so the tests compile, restoring main's flag-discarding call
makes three interpreter rows fail and leaves the no-flag row passing. Removing the layout
handling from the dialog makes three host-level layout rows fail and leaves the no-flag
row passing. Restoring main's MISSING_LANGUAGE wording makes both updated doc cases fail.
That is **eight demonstrated failures: six behavior cases and two documentation cases**.
The two no-flag cases already pass and are baseline coverage, not new regression teeth.
All temporary reversions were restored.

## Checks

- Full HexIDE.Tests: 8,059 passed, 67 skipped, 8,126 total in 31m 10s; no failures.
  The xunit report guard found no runner errors.
- Full Runtime.Tests: 8,629 passed, 16 skipped, 8,645 total; no failures (including the ar-SA row).
- Full Runtime.Integration.Tests: 2,802 passed, two skipped, 2,804 total. All four layout
  cases passed. The xunit report guard found no runner errors. Skips are
  `ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements` (Windows Segoe UI)
  and `ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`
  (Windows GDI). Neither is coverage for this change.
- Existing MessageBox UI classes: 13 passed, zero skipped; xunit report guard passed.
- DialogFlowDirectionTests: 14 passed, zero skipped; xunit report guard passed.
- MissingLanguageDocTests: 20 passed, zero skipped. MsgBox remains Partial and no counts moved.
- Tree hygiene passed with nine existing warnings and no blockers; `git diff --check` passed.

Windows PATH entries were removed. Builds used `-m:1 -nr:false` after an initial MSBuild
invalid-node-id failure. HangDump initially crashed before test execution because
`/run/user/1000` is read-only in the sandbox; the successful headless runs set
`XDG_RUNTIME_DIR` to a mode-700 directory under this clone's ignored artifacts.
Headless runs kept HangDump enabled and emitted xunit reports.

## Files changed

- `IDE/HexIDE.Core/IDE/MessageBoxEnums.cs`
- `IDE/HexIDE.Runtime.Tests/MsgBoxStyleAndColorTests.cs`
- `IDE/HexIDE.Runtime/Dialogs/MessageBox.axaml`
- `IDE/HexIDE.Runtime/Dialogs/MessageBox.axaml.cs`
- `IDE/HexIDE.Runtime/Interpreter/BasicInterpreter.cs`
- `IDE/HexIDE.Runtime/Interpreter/VB6BuiltIns.cs`
- `IDE/HexIDE.Runtime/StandaloneStandardLib.cs`
- `IDE/HexIDE.Tests/Infrastructure/MissingLanguageDocTests.cs`
- `IDE/HexIDE/IDE/IWindowManager.cs`
- `IDE/HexIDE/IDE/InterpreterRunEngine.cs`
- `IDE/HexIDE/IDE/ProgramDialogs.cs`
- `IDE/HexIDE/IDE/WindowManager.cs`
- `changelog.d/3014.md`
- `docs/MISSING_FEATURES.md`
- `docs/MISSING_LANGUAGE.md`
- `docs/interpreter-gaps.md`
- `docs/vb6-fidelity-oracle.md`
- `openspec/specs/localization/spec.md`
- `IDE/HexIDE.Runtime.Integration.Tests/Views/MessageBoxLayoutTests.cs`
- `scripts/probes/issue-3014-validation.md`

## Runtime-suite skips

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`: Runs only as the child of OnLinux_TheUiThreadsStack_IsTheSameWhateverUlimitSays
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`: Set HEXIDE_ORACLE=1 to compile round-tripped forms with VB6.

## IDE-suite skips

- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`: A link to a share is a Windows path.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`: A made program is started on Windows only: elsewhere it needs a desktop session.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"acme-ide.exe\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.json\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.sig\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"nested/HEXIDE-FIRSTPARTY.PUB\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.json\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"branding/logo.png\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"HexIDE.exe\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.sig\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"brand.json\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"hexide-root.pub\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"LICENSE\")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`: Type libraries are read only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.

Possible drift: none. Changes were left uncommitted for supervisor review.

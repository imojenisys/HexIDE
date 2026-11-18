# #4594 validation

Validated under WSL on 2026-11-18, starting from `upstream/main` at `86dcc6c50`.
Scope and measured VB6 values: [issue #4594](https://github.com/hexide-io/HexIDE/issues/4594), including its supervisor comment.

## Regression evidence

- `EnumInBoxConstantTests` on unchanged main: **21 failed, 6 passed, 0 skipped**, 27 total. The six passing guards cover the three variable/Const-below rows, `K + 1`, a qualified call and a four-part name. They preserve main; they are not failing-before teeth.
- On the final implementation all **27 passed**, with no skips, in the full runtime run.
- `mutation-check.ps1`, guard removed: **2 failed** (both project Const rows); both passed before mutation and after byte-for-byte restoration.
- `mutation-check.ps1`, bare lookup removed: **3 failed** (`vbTextCompare`, `vbObjectError Or 127`, `vbRed`); the selected tests passed before mutation and after restoration. The method filter also selected seven existing Width tests, which passed throughout.
- The existing `EnumTests`, `EnumMembersAcrossModulesTests` and `EnumOwnNameTests` ran in the full runtime suite.

## Completed checks

- `HexIDE.Runtime.Tests`: **9923 passed, 20 skipped, 0 failed**, 9943 total (12m 40s 381ms).
- `HexIDE.Runtime.Integration.Tests`: **3293 passed, 2 skipped, 0 failed**, 3295 total (5m 48s 635ms).
- `HexIDE.Tests`: **10243 passed, 72 skipped, 0 failed**, 10315 total (1h 02m 07s 917ms).
- `MissingLanguageDocTests`: **20 passed, 0 skipped, 0 failed**, 20 total (12s 392ms).
- Runtime integration xUnit XML report: **3,295 tests, no runner errors** (`scripts/check-xunit-report.sh`).
- `scripts/check-tree-hygiene.sh`: passed, with nine pre-existing non-blocking warnings. The optional identity scan was not configured.
- `git diff --check`: passed.
- MISSING_LANGUAGE: Enum remains Partial; **no count changes**. Its Detail notes now cite #4594 and #4614.

Windows PATH entries were removed. `UseSharedCompilation=false` was exported; `XDG_RUNTIME_DIR` and the NuGet caches used writable temporary directories. No package versions or test floors changed. The full suites ran through `scripts/test.ps1`, with their existing floors enforced.

## Skipped tests

These are exclusions, not coverage of the change. None of the 27 new cases skipped.

### HexIDE.Runtime.Tests (20)

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character` — Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows` — Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows` — Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows` — Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows` — Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault` — Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does` — Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text` — Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date` — Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx` — Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape` — VB6 templates unavailable.
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6` — Set HEXIDE_ORACLE=1 to compile round-tripped forms with VB6.
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack` — Runs only as the child of OnLinux_TheUiThreadsStack_IsTheSameWhateverUlimitSays
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows` — Windows-only: this pins behaviour the product deliberately varies by host.

### HexIDE.Runtime.Integration.Tests (2)

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent` — Requires Windows GDI
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements` — Requires the oracle's Windows Segoe UI font build

### HexIDE.Tests (72)

- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` — A link to a share is a Windows path.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main` — A made program is started on Windows only: elsewhere it needs a desktop session.
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute` — macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon` — macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding` — macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one` — macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary` — Registry type libraries require Windows.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical` — VB6 templates unavailable.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce` — VB6 templates unavailable.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity` — VB6 templates unavailable.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid` — Set HEXIDE_WIRE_STRESS_SECONDS to run the wire stress test (#3520)
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it` — Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process` — Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it` — Job objects exist only on Windows.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.signed")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report` — Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused` — Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole` — Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile` — Type libraries are read only on Windows.

## Deliberate gaps

String constants and missing in-box container members in Enum values remain explicit #4594 refusals because they are unmeasured. Project Const values and other project-name/qualifier collisions remain explicit #4614 refusals. Other Const expressions and referenced control-library constants keep main's behavior. The refusal messages are program-facing English. No oracle page was edited.

Possible drift: none.

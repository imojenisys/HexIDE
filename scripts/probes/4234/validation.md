# Validation for #4234

Run under WSL on 2026-11-16. Both suites use the existing CI floors.

| Check | Result |
|---|---|
| Desktop Debug build | Passed; zero warnings, zero errors |
| ShippedThemePackTests | 97 passed, none skipped |
| Main-window render | 2 passed, none skipped |
| Drawn chrome contrast | 14 passed, none skipped |
| Full HexIDE.Integration.Tests | 3,859 passed, 4 skipped, 3,863 total; 28m 49s |
| Integration xUnit report | Zero errors and zero not-run cases |
| Full HexIDE.Tests | 8,851 passed, 71 skipped, 8,922 total; 47m 55s |
| Unit xUnit report | Zero errors and zero not-run cases |
| Tree hygiene | Passed; nine existing advisory warnings; identity scan not configured |
| OpenSpec strict | Exits 1 for seven unchanged pre-existing long-requirement warnings; no new warning |

Ten regression cases failed against the original resources and pass with the change.
The new guard also covers already-complete high-contrast and unaffected light packs;
those passing cases are not counted as regression teeth.

`PATH` excluded Windows entries. `UseSharedCompilation=false` and
`MSBUILDDISABLENODEREUSE=1` reached all build/test subprocesses. The unit suite also
used `HEXIDE_FOREIGN_LSP_DOWNLOAD=0`. Runtime mutex files used a writable directory
inside the clone.

## Integration skips

These four skipped cases provide no coverage in this run. Each is already skipped
for the Avalonia 12.1.4 pin tracked by #3869:

- `HexIDE.Integration.Tests.Views.ContextMenuMeasureTests.CodeMenuFitsItsShortcutsAndSubmenuArrowsOnFirstOpening`
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Expand_LeafNode_RefusesClearly`
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Inspect_TreeNode_ReportsExpandCollapseState`
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Collapse_LeafNode_RefusesClearly`

## Unit skips

These 71 skipped cases provide no coverage in this run.

### VB6 Template tree unavailable; VB6_TEMPLATES is unset. (10)

- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`

### DAP schema unavailable; set HEXIDE_DAP_SCHEMA to the pinned schema for offline use. (1)

- `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode`

### Windows-only: this pins behaviour the product deliberately varies by host filesystem. (23)

- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`

### macOS-only: asks macOS's own tools about the result. (3)

- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`

### A link to a share is a Windows path. (1)

- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`

### debugpy 1.8.22 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition. (1)

- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session`

### netcoredbg 3.2.0-1092 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition. (1)

- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session`

### A made program is started on Windows only: elsewhere it needs a desktop session. (1)

- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`

### Type libraries are read only on Windows. (2)

- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`

### Needs a Windows x64 test host, whose launcher stands in for a stock executable. (25)

- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.signed")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`

### Job objects exist only on Windows. (3)

- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`

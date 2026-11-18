# #4505 validation

The replacement prompt is IDE-facing, so `Str.FilePicker.ReplaceExisting` is translated in all 39 shipped neutral
packs, with a Portuguese (Brazil) override. The question is not a program-facing refusal.

## Behavior and provenance

[Issue #4505](https://github.com/hexide-io/HexIDE/issues/4505) and
[oracle PR #4501](https://github.com/hexide-io/HexIDE/pull/4501) supply the measurements and decisions.
All eight implementation points are covered, including confirmed Save Project As onto the current path and a different
path, for clean and dirty projects. Ordinary outside-change handling remains covered by `ExternalProjectChangeTests`.
The group regression checks name, project order and startup project with two projects loaded.

An empty render never deletes a companion at another destination. Successful adopting saves with no companion content
clear old citation ownership, so a subsequent ordinary Save cannot delete the preserved file. Make's temporary copies
leave that ownership unchanged (source review of the adoption guards).

## Teeth

`SaveReplacementTests`: 35 cases pass on the fixed source, none skipped. All 35 fail when built against main's
`ProjectService.cs`. Unchanged ordinary-Save assertions are passing coverage; they are not claimed as independent teeth.

The supervisor's `mutation-check.ps1` built and ran baseline, mutant and restored source, restoring bytes with a fresh
modification time:

| Mutation | Failed | Passed | Skipped |
|---|---:|---:|---:|
| Bypass the replacement existence check | 34 | 1 | 0 |
| Restore companion deletion at another destination | 4 | 31 | 0 |

Each restored run passed all 35 cases. The second mutation covers `.frm`, `.ctl` and `.pag` writers.

## Existing tests adjusted

Each fixture now agrees to the new replacement question so it can reach the behavior it already tested.

| File / case | Reason |
|---|---|
| `UserFileWriteFailureTests.MakePreservesEachOutputOnPartialWrite` | Agree to replacing the existing executable before the partial-write injection. |
| `UserFileWriteFailureTests.MakeAsksBeforeReplacingDerivedArchive` | Agree to the executable question; keep each archive Yes/No/Cancel answer unchanged. |
| `UserFileWriteFailureTests.DirectorySaveAsksBeforeReplacingAnyOutput` | The cleared-picture case now expects the foreign orphan's bytes to survive. |
| `MakeOutcomesTests.A_made_program_is_reported_with_its_path_and_size_and_whether_it_wrote_over_one` | Its second Make replaces the first executable. |
| `MakeOutcomesTests.A_No_to_replacing_the_archive_is_reported_as_that_with_no_box` | Reach the archive's No after agreeing to executable replacement. |
| `MakeOutcomesTests.A_replace_question_nobody_answers_is_reported_as_where_Make_waits` | Keep the archive question unanswered, after agreeing to the executable question. |
| `MemberAttributesFollowARenameTests.ItsOwnSaveAsToTheFileItWasReadFromRewritesAsItsSaveDoes` | Both form and class cases select their existing current file. |
| `NegativeSizeSaveTests.Choosing_the_original_asks_again_until_a_new_path_is_chosen` | Agree to replacement so the existing loaded-with-errors refusal and repicker remain exercised. |
| `SourceEncodingPersistenceTests.Making_twice_to_the_same_path_replaces_the_archive_with_the_second_build` | The second Make replaces the executable too. |
| `WriteAtSaveTests.UnsavedProjectCarriesOnlyThisSavesPreviousPickerDirectory` | The second project's picker selects the first project's existing name. |
| `DesignerHalfSaveTests.Save_As_from_a_designer_half_asks_for_a_ctl` | Its second picker selects the `.ctl` just saved. |
| `ContributedProjectFileTests.A_module_saved_as_its_project_s_own_file_is_refused_before_anything_is_written` | Agree to replacement so the existing member-over-contributed-project-file refusal is reached. |
| `ContributedProjectFileTests.A_vbp_project_s_module_saved_over_its_vbp_is_refused_too` | The same consent for the VB6 project-file refusal path. |

## Full checks

| Check | Passed | Skipped | Total | Duration |
|---|---:|---:|---:|---|
| Full `HexIDE.Tests` | 10081 | 75 | 10156 | 43m 25s 679ms |
| Integration `*Save*` slice | 48 | 0 | 48 | 46s 111ms |
| `ShippedPackParityTests` | 32 | 0 | 32 | 16s 262ms |
| `LocalizationCoverageTests` | 13 | 0 | 13 | 6s 005ms |

Both saved xUnit reports passed `check-xunit-report.sh`: no host/collection errors.
Desktop Debug build: zero warnings and errors. Strict `project-file-format` OpenSpec validation passed.
Tree hygiene passed with nine existing warnings; the optional identity scan had no configured pattern.
The adjusted fixture classes, external-project-change tests and picker/faithfulness guards also passed separately.

### Skipped full-suite tests

These skipped methods are not claimed as coverage. Counts include their skipped theory cases.

| Method | Skipped cases |
|---|---:|
| `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` | 1 |
| `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode` | 1 |
| `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session` | 1 |
| `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session` | 1 |
| `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute` | 1 |
| `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file` | 1 |
| `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` | 1 |
| `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named` | 1 |
| `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` | 1 |
| `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` | 1 |
| `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` | 1 |
| `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary` | 1 |
| `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid` | 1 |
| `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile` | 1 |
| `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole` | 1 |
| `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process` | 1 |
| `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it` | 1 |
| `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it` | 1 |
| `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused` | 1 |
| `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead` | 1 |
| `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused` | 1 |
| `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header` | 1 |
| `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched` | 1 |
| `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce` | 1 |
| `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity` | 1 |
| `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case` | 1 |
| `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused` | 1 |
| `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main` | 1 |
| `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing` | 1 |
| `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output` | 12 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks` | 1 |
| `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet` | 1 |
| `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it` | 1 |
| `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn` | 1 |
| `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation` | 1 |
| `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot` | 1 |
| `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests` | 1 |
| `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData` | 1 |

Builds and tests removed Windows mount entries from `PATH` and exported `UseSharedCompilation=false`.
Test runs used a fresh writable `XDG_RUNTIME_DIR`; foreign downloads were disabled with
`HEXIDE_FOREIGN_LSP_DOWNLOAD=0`. MSBuild node reuse was also disabled so nested checker processes exit cleanly. The completed full rerun used
[MTP diagnostic logging](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-config) for observable test progress.
NuGet vulnerability data used a clone-local HTTP cache because the global cache was read-only.
The first full attempt was left for over 71 minutes, then stopped only after a live diagnostic snapshot identified
an existing refusal fixture repeating a fixed picker answer without confirming replacement. Both refusal fixtures now agree to
replacement so their original refusal checks are reached. The completed rerun above supplies the full total.

## Unmeasured and live verification

Save As onto the current file asking for replacement is reasoned, not separately probed. VB6's handling of a replaced
`.frm`'s companion when the new render has no companion remains unmeasured; preservation is the supervisor's decision.
No task item is left unimplemented. `MISSING_LANGUAGE.md` and its counts are unchanged. No oracle page or index changed.
Possible drift: none.

The supervisor's running-IDE check should confirm the localized filename, Yes/No buttons with No focused by default,
and a reopened native picker retaining the chosen name and folder after No. Exercise form, module, project, group and
Make; cancel the reopened picker and check no writes. Verify one question for `.frm`/`.frx` replacement, orphan naming
only when a companion is written, preservation through a later ordinary Save, and no second conflict question for
confirmed clean or dirty Save Project As. Make's separate archive question remains unchanged.

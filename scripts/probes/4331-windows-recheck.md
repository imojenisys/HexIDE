# #4331 Windows test recheck

Checked under Ubuntu/WSL on 2026-11-17. This run addresses the latest supervisor
comment's test failures. The corpus fix changes test infrastructure only; the
required upstream merge also reconciles the existing project-filter behaviour.

## Reproduction and teeth

The original `OfficeProjectBrowsingTests` class passes all 44 cases in its normal
output layout. Its `CorpusFile` helper nevertheless stops at any ancestor with a
`corpus` directory, even if that directory contains no Office fixtures.
`ProjectCorpusSmokeTests.Repository_root_requires_both_project_trees_despite_a_nearer_corpus_directory`
already exercises such a directory beside the test binary.

To isolate this layout from the running full suite, a copy of the built test
output was placed under `artifacts/4331-before/`, with an empty `corpus/` beside
its test assembly. Running the original Office class there produced 41 failures
out of 44 cases, including all 30 extension-routing cases with
`DirectoryNotFoundException`.

The new `Corpus_file_lookup_ignores_a_nearer_corpus_directory` regression failed
before changing the helper: one new test with demonstrated teeth. After using
the existing `RepoTree.Root()` helper, all 45 Office cases passed in the same
layout, with no skips or xUnit errors. `scripts/check-xunit-report.sh` accepted
the report. The temporary output copy is disposable; these are the retained
results.

## Other checks

- Debug Desktop build: zero warnings and zero errors.
- Test project builds: zero warnings and zero errors.
- Tree hygiene: passed, with nine existing advisory warnings.
- `git diff --check`: passed.

## Upstream integration

The required merge of `upstream/main` is commit `83f0c75c9`. Its two conflicts
were in `FileBrowserViewModel` and `ProjectService`. The resolution retains
main's contributed project filters, the Office extensions, the localized
browser labels, the archive workspace-preservation rule and main's path for a
registered reader when no archive service is supplied.

`ContributedProjects.OpenablePatterns` uses the complete container pattern list.
The upstream `Vb6ProjectTemplatesTests` catalog expectations now include the
Office extensions and localized captions. No test dedicated to archives was renamed
or changed in this run.

The merged Desktop and test projects build with zero warnings or errors.
All eight `Vb6ProjectTemplatesTests` and all 164 `Category=ToolDescription` tests
pass, with no skips. These are compatibility checks; no fail-before claim is
made for them.

After restoring the uncommitted corpus fix, all 45 `OfficeProjectBrowsingTests`
also pass on the merged tree, with no skips or xUnit errors.

The supervisor's live check should inspect the Existing-project browser and
native Open Project filters with both Office files and an add-in project type,
including the width of the localized filter caption containing the patterns.

## Full-suite verification

Baseline full `HexIDE.Tests`: 9,616 passed, 72 skipped, 9,688 total in
26m 54s. The floor policy passed; the xUnit report records zero errors.

With the corpus fix, before merging upstream: 9,617 passed, 72 skipped,
9,689 total in 29m 23s. The floor policy and xUnit report check both passed.

**Final merged tree:** 9,765 passed, 73 skipped, 9,838 total in 25m 44s.
The floor policy passed; `scripts/check-xunit-report.sh` confirms zero errors.
All 45 Office browsing cases passed in this full run.

Every build/test exported `UseSharedCompilation=false` and removed Windows
PATH entries. Tests used a fresh writable `XDG_RUNTIME_DIR` and
`HEXIDE_FOREIGN_LSP_DOWNLOAD=0`. No full run was stopped early.

The following exact cases skipped in the final merged run and are not
claimed as coverage.

### Optional wire stress run not enabled (1)

- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`

### Unavailable DAP schema (1)

- `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode`

### Unavailable VB6 template corpus (10)

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

### Uncached foreign debugger adapters; acquisition disabled (2)

- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session`
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session`

### Windows-only (56)

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
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

### macOS-only (3)

- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`

## Limits

The brief omits the Windows exception's path and stack trace. The layout above
reproduces the reported exception type and affected routing tests locally; a
supervisor Windows rerun must confirm whether it fixes the reported failures.
No Windows-only test is claimed as coverage from WSL.

No VB6 behaviour was measured or changed by the corpus fix. Feature status and
`MISSING_LANGUAGE` counts are unchanged by this work.

## Possible drift

The earlier #4331 supervisor comment requires no Office language-server sessions
because the bundled grammar would reject VBA7 `PtrSafe` and `LongPtr`.
`docs/office-containers.md` and `docs/lsp-client.md` retain that rationale.
Upstream #4365 now adds both spellings to
`LspServer/HexIDE.VbLspServer.Tests/Vba7SyntaxTests.cs`'s acceptance rows that expect
no diagnostics. This is read from the code, not a new LSP test run in this task.

Keeping language tooling disabled on that outdated rationale could head away
from the project's deep support for the VB family. The explicit no-session policy
is retained: choosing Office language-server routing is outside this Windows
test repair deliverable and still needs its recorded follow-up decision.

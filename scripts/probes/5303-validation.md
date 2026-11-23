# #5303 validation

Checked under WSL Ubuntu on branch `fix/5303-codex`, based on `4288ab927`.

## Teeth

With the final tests and unchanged product defaults, six test cases failed:

- `SettingsServiceTests.ErrorTrappingDefaultsAndPersists`: all three enum rows.
- `SettingsServiceTests.MissingErrorTrappingUsesTheDefaultAndStaysUnwritten`.
- `RunEngineSeamTests.A_new_runner_uses_the_IDE_error_trapping_default`.
- `OptionsViewModelTests.ErrorTrappingLoadsSavesAndRestoresDefaults`.

`StoredUnhandledErrorsSurvivesSavingAnotherProperty` passes on main; it guards existing persistence.
The made-program caller-handler test also passes on main. For its mutation check, temporarily set
`DebugController.ErrorTrapping`'s initializer to `BreakInClassModule`, without the explicit standalone
setting: the test failed because the handler printed nothing. Adding the explicit standalone setting
made all nine `StandaloneErrorTests` pass with the mutated engine default still in place. The engine
initializer was then restored to `BreakOnUnhandledErrors` before final checks. No mutation remains.
The measured program is c1 in [the oracle](../../docs/vb6-fidelity-oracle/an-unhandled-run-time-error.md).

## Environment and limitations

Windows entries were removed from PATH. `UseSharedCompilation=false`, `HEXIDE_FOREIGN_LSP_DOWNLOAD=0`,
and a fresh writable `XDG_RUNTIME_DIR` were exported. Final runs also disabled MSBuild worker reuse
with `MSBUILDDISABLENODEREUSE=1` and `DOTNET_CLI_USE_MSBUILD_SERVER=0`.
An early build hit CS2012 despite the shared-compilation environment setting; its retry passed.
An early test invocation used MSBuild `-m`/`-nr` flags that native MTP forwarded to the runner and rejected;
the final test invocations omit them.

`bash scripts/test-linux.sh IDE/HexIDE.Tests/ -- --filter-class '*SettingsServiceTests'` refuses this
ext4 checkout: the script requires a source path under `/mnt`. Tests instead use `pwsh scripts/test.ps1`
directly in WSL, keeping all work within this clone. No Windows run or live IDE check is claimed.

OpenSpec strict validation before and after: **18 passed, 47 failed, 65 items**. The same specs fail;
existing strict validation failures are outside this change. Tree hygiene passes with nine existing warnings.

## Supervisor live check

With a fresh profile, inspect Tools > Options > Environment > General: Break in Class Module is selected.
A class with no local handler, called directly by a form or standard module with a handler, should halt
in the class; Debug then Continue should retry that statement. Select Break on Unhandled Errors and repeat:
the caller's handler should run. A made program should still run the caller's handler. An existing numeric
`errorTrapping: 2` setting should remain selected after changing and saving another option.

Unmeasured shapes retain their prior handling: forms/standard modules raising, class callers, longer
chains and own handlers. `MISSING_LANGUAGE.md` notes change, but support statuses and counts do not.
No public API additions or localization keys. Possible drift: none.

## Final checks

- Focused IDE classes (`SettingsServiceTests`, `OptionsViewModelTests`, `StartupCompositionTests`,
  `RunEngineSeamTests`): **150 passed, 0 skipped**.
- `HaltOnErrorTests`: **42 passed, 0 skipped**.
- `StandaloneErrorTests`: **9 passed, 0 skipped**.
- Full runtime integration: **5,586 total, 5,536 passed, 50 skipped, 0 failed** (7m 58s).
  `check-xunit-report.sh` reports no errors.

Runtime integration's 50 skips are existing Windows-only cases: `LabelAutoSizeTests` (27),
`SortedListItemsTests` (13), `ToolbarRuntimeTests` (8), and `ToolbarCaptionTests` (2).
They provide no coverage on this host.

- Full IDE integration: **4,700 total, 4,700 passed, 0 skipped, 0 failed** (22m 28s).
  `check-xunit-report.sh` reports no errors.
- Debug Desktop build: **0 warnings, 0 errors**. The expected keyless-clone message skips first-party add-in packing.

## Changed files

- `IDE/HexIDE.Core/IDE/SettingsDefaults.cs`
- `IDE/HexIDE.Integration.Tests/Views/HaltOnErrorTests.cs`
- `IDE/HexIDE.Runtime.Integration.Tests/Views/StandaloneErrorTests.cs`
- `IDE/HexIDE.Runtime/Debugging/DebugController.cs`
- `IDE/HexIDE.Standalone/StandaloneRun.cs`
- `IDE/HexIDE.Tests/DebugAdapter/RunEngineSeamTests.cs`
- `IDE/HexIDE.Tests/SettingsServiceTests.cs`
- `IDE/HexIDE.Tests/StartupCompositionTests.cs`
- `IDE/HexIDE.Tests/ViewModels/OptionsViewModelTests.cs`
- `IDE/HexIDE/IDE/ProjectRunnerService.cs`
- `changelog.d/5303.md`
- `docs/MISSING_FEATURES.md`
- `docs/MISSING_LANGUAGE.md`
- `docs/debugger-vb6-divergences.md`
- `openspec/specs/interpreter-debugger/spec.md`
- `scripts/probes/5303-validation.md`

## Full IDE result and skipped tests

Full `HexIDE.Tests`: **10,984 total, 10,895 passed, 89 skipped, 0 failed** (51m 48s).
The run was allowed to finish without interruption. Its floor policy passed (minimum 7,684).
All three full suites passed; both headless xUnit reports have no errors. No changed test skipped.

The following existing tests skipped on this host. Counts group theory rows by method; none is claimed as coverage.

### HexIDE.Tests

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`: 1 case.
- `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode`: 1 case.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session`: 1 case.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_launch_failure_includes_the_native_call_log`: 1 case.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session`: 1 case.
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`: 1 case.
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`: 1 case.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`: 1 case.
- `HexIDE.Tests.IDE.ProjectLocalStateTests.Directory_junction_is_ignored_and_never_written_through`: 1 case.
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`: 1 case.
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`: 1 case.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`: 1 case.
- `HexIDE.Tests.Infrastructure.ResolvedTempTests.Resolve_returns_the_original_path_on_Windows`: 1 case.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault`: 1 case.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault`: 6 cases.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`: 1 case.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`: 1 case.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`: 1 case.
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`: 1 case.
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`: 1 case.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`: 1 case.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`: 1 case.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`: 1 case.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`: 1 case.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`: 1 case.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`: 1 case.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`: 1 case.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`: 1 case.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`: 1 case.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`: 1 case.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`: 1 case.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`: 1 case.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`: 1 case.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`: 1 case.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`: 1 case.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`: 1 case.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`: 1 case.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`: 1 case.
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`: 1 case.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`: 12 cases.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`: 1 case.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`: 1 case.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`: 1 case.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`: 1 case.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`: 1 case.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`: 1 case.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`: 1 case.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`: 1 case.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`: 1 case.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`: 1 case.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`: 1 case.
- `HexIDE.Tests.ViewModels.FontDialogScriptTests.Installed_fonts_expose_the_measured_script`: 2 cases.

### HexIDE.Runtime.Integration.Tests

- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_enable_disable_and_alignment_edges`: 2 cases.
- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_measured_caption`: 19 cases.
- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_sequence_and_italic`: 2 cases.
- `HexIDE.Runtime.Integration.Tests.Views.LabelAutoSizeTests.Windows_wrapping_and_center_divergence`: 4 cases.
- `HexIDE.Runtime.Integration.Tests.Views.SortedListItemsTests.Windows_keeps_every_measured_code_point`: 13 cases.
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`: 1 case.
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`: 1 case.
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font`: 8 cases.

Reasons include Windows filesystem/COM/font/NLS behaviour, absent VB6 templates and fonts,
uncached netcoredbg with acquisition disabled, and opt-in diagnostic/stress settings not enabled.
The IDE integration suite skipped nothing.

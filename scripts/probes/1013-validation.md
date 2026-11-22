# #1013 validation

Base: `d4c9b47c5`, equal to `upstream/main` at the start of this run. Changes are uncommitted.

## Behaviour and evidence

Both lexical readers count procedure names everywhere, variables/constants only before the first
body-opening logical line, and designer names. Locals no longer change a renamed variable run's
ownership, replacement guard or saved qualifier. Evidence:
[oracle, “A local of the stale name changes nothing”](../../docs/vb6-fidelity-oracle/member-attributes-where-vb6-attaches-them-and-what-its-ide-writes.md#a-local-of-the-stale-name-changes-nothing-2026-11-21-1013-ide-sp6).

No oracle files were changed. The loaded-orphan/requalification divergence remains as directed by
the latest issue comment. Code-window retyping with a local, class-module measurements and a parameter
of the stale name remain unmeasured; no separate VB6 expectation was inferred for them.
No language-support status changed; MISSING_LANGUAGE counts are unchanged. No public API or strings added.
Possible drift: none.

## Before-change proof

The tests were added before either reader changed. IDE slice: 226 total, 217 passed, nine failed.
Eight failures are counted as the before-change proof:

- `MemberRunsTests.ALocalOrAVariableAfterTheFirstBodyDoesNotPreventARenamedRun`: four rows
  (Dim, Static, Const, and a variable after the first body).
- `GuardedContentMemberRunTests.AddingOrDeletingALocalLeavesARenamedVariablesRunAlone`: two rows.
- `MemberAttributeQualifiersTests.ARenameCarriesAVariableDescriptionWithALocalOfTheStaleName`: one.
- `BundledServerRespectsTheIdesRegionsTests.TheInlineFilesCarryRunsThatDescribeAMemberAndOrphansBoth`: one.

The ninth IDE failure was the protocol formatting comparison while the server was being rebuilt;
it is excluded from the teeth count because that run mixed old IDE and newly built server binaries.
The server slice, before its implementation changed: 79 total, 78 passed, one failed, zero skipped.
That failure was the new local row in `ARunVb6GivesToTheLineAboveIsProtected`.
After the full IDE run completed, the server reader alone was temporarily restored to HEAD and
the final server tests rerun: **79 total, 77 passed, two failed, zero skipped**. Both the new local
row and `EachRuleOfTheDeclarationReaderDecidesItsOwnRun` (updated fixture expectation) failed.
The final implementation was restored in a finally block and the full server suite rerun.

Total established before-change failures: **ten**.

Existing module-level variable, constant, procedure and designer-name orphan guards were retained.
They pass before the change and are regression guards, not new failing cases.

## Checks

Environment: Windows PATH entries removed; `UseSharedCompilation=false`,
`MSBUILDDISABLENODEREUSE=1`, `DOTNET_CLI_USE_MSBUILD_SERVER=0`; fresh writable
`XDG_RUNTIME_DIR`; writable NuGet HTTP cache. Foreign-server downloads disabled for IDE tests.
No shared build server was stopped.

- Full IDE suite: **10,678 passed, 86 skipped, zero failed; 10,764 total**, 35m53s.
- Full bundled-server suite: **839 passed, zero failed, zero skipped**.
  An earlier full run failed only `ParseReportTests.A_parse_records_the_time_it_took`
  (2.263 seconds against its two-second limit). The unchanged test passed on the full rerun
  with `DOTNET_PROCESSOR_COUNT=4`.
- Full runtime suite: **12,663 passed, 27 skipped, zero failed; 12,690 total**.
- Desktop Debug build: **zero warnings, zero errors**.
- Tree hygiene: passed, nine pre-existing warnings; identity scan not configured.
- Integration suite is not required: no production files under `IDE/HexIDE/` changed.

## Supervisor live check

With `Public Tally As Long` followed by `Attribute Count.VB_VarDescription`, add and delete
`Dim Count As Long` inside `Reset`. The run should remain greyed, read-only and folded, and a
whole replacement adding/deleting the local should succeed without changing the run. Save should
write `Attribute Tally.VB_VarDescription`. Repeat with Static/Const locals. Confirm the bundled
server leaves the protected run alone when formatting and does not rename/highlight its qualifier.

## Skipped runtime tests (no coverage claimed)

- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `HexIDE.Runtime.Tests.ErrorFidelityTests.KeptHresult_WindowsMessagesMatchMeasurements`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`
- `HexIDE.Runtime.Tests.DeclareCallTests.Windows_KeepsRefusal`
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `HexIDE.Runtime.Tests.RecursionCostProbe.Measure`
- `HexIDE.Runtime.Tests.RecursionCostProbe.MeasureCollectionsWithTheSameActivationsRetainedAfterReturn`
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`
- `HexIDE.Runtime.Tests.ShellTests.BatchFileRuns`
- `HexIDE.Runtime.Tests.ShellTests.ExecuteDeniedIs5`
- `HexIDE.Runtime.Tests.ShellTests.ConvertedPathFindsExecutableInCurrentDirectory`

## Skipped IDE tests (no coverage claimed)

- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "StdioProcessLspTransportTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "WebSocketLspTransportTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "ShutdownWireShapeTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "CancellationWireShapeTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "RenameWireShapeTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "TraceOnTheWireTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault`: Set HEXIDE_UNOBSERVED_PROBE and provide netcoredbg to run adapter teardown diagnostics.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`: Registry type libraries require Windows.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.signed")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`: Set HEXIDE_WIRE_STRESS_SECONDS to run the wire stress test (#3520)
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\Program Files (x86)\Microsoft Visual Studio\VB98\Template'.
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`: Avalonia's Win32 platform runs only on Windows
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`: Job objects exist only on Windows.
- `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode`: DAP schema unavailable; set HEXIDE_DAP_SCHEMA to the pinned schema for offline use.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`: A made program is started on Windows only: elsewhere it needs a desktop session.
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.IDE.ProjectLocalStateTests.Directory_junction_is_ignored_and_never_written_through`: Directory junctions require Windows; symbolic links are covered on Linux.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`: A link to a share is a Windows path.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session`: netcoredbg 3.2.0-1092 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session`: debugpy 1.8.22 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_launch_failure_includes_the_native_call_log`: netcoredbg 3.2.0-1092 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.

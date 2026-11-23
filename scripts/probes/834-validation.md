# #834 validation (2026-11-23)

Linux/WSL, .NET 11.0.100, base `cf602a678` (`upstream/main`). This records this run's headless checks, not new VB6 measurements. The behaviour source is [issue #834](https://github.com/hexide-io/HexIDE/issues/834), latest round-2 comment, and the [owned oracle page](../../docs/vb6-fidelity-oracle/rgb-qbcolor-and-msgboxs-style.md#a-message-box-is-modal-to-the-whole-program-2026-11-21-834-sp6).

## Files

- `IDE/HexIDE.Runtime.Integration.Tests/Views/MessageBoxProgramModalityTests.cs`
- `IDE/HexIDE.Runtime/Dialogs/MessageBox.axaml.cs`
- `IDE/HexIDE.Runtime/Dialogs/MessageBoxFormHold.cs`
- `IDE/HexIDE.Runtime/HostedFormRoot.cs`
- `IDE/HexIDE.Runtime/Interpreter/VB6BuiltIns.cs`
- `IDE/HexIDE.Runtime/VBFormRuntime.cs`
- `IDE/HexIDE.Runtime/VBProgram.cs`
- `changelog.d/834.md`
- `docs/MISSING_FEATURES.md`
- `docs/MISSING_LANGUAGE.md`
- `docs/interpreter-gaps.md`
- `scripts/probes/834-validation.md`

## Behaviour and teeth

A counted hold surrounds the shared built-in's awaited host call, separately from the program's own Enabled setting. Managed input is held on desktop forms and hosted form roots. The native hold begins in ShowAndWait after the host has chosen its owner; owner hide/show reapplies it. Native enablement is restored both immediately and on the real dispatcher after Avalonia removes a closed dialog from its owner. It snapshots the loaded forms at opening. Neither the event pump nor Timer scheduling was changed.

For the baseline, the five tracked runtime integration edits were reverted to `cf602a678`; the new internal hold helper and unused callback property remained as compilation scaffolding for the tests. There was no effective-enablement consumer, built-in wiring or ShowAndWait native entry. The final new test class then recorded **18 cases: 15 failed, 3 passed, 0 skipped**:

- Six real pointer cases (visible, hidden before, hidden during, each at 100% and 150%) failed because a click queued behind the box ran after its answer.
- Both overlapping-box completion orders failed to keep the other form effectively disabled.
- Enabling a previously disabled form while the box waited failed to keep it held.
- The hosted-root input case delivered a pointer press while held.
- Five native-bridge cases failed because no hold reached the backend: visible, hidden before, hidden during, and a pre-disabled owner or other form. Native overlap assertions also cover both completion orders.

The three cases already passing on main behaviour were the no-box click control, preserving a form disabled before the box, and assigning Enabled = False while it waited. These are regression guards, not claimed fixes. All **18 new cases pass** in the final full runtime integration run, with no skips.

Existing guards: `MessageBoxHostTests` **7 passed**, `MessageBoxAcrossRestartTests` **3 passed**, and `HaltOnErrorTests.A_tick_waits_behind_a_MsgBox_in_a_nested_handler` **passed**.

## Full checks

The initial managed-hold full IDE test command failed during its build, before discovery, with MSB3061 (a sandbox read-only filesystem error deleting Runtime and AIChat reference DLLs). Both focused guard runs had just built the same sources successfully. That full suite completed 10,900 cases (10,817 passed, 83 skipped) from the verified build with `dotnet test IDE/HexIDE.Tests/ --no-build -- ...`, exporting `DOTNET_CLI_USE_MSBUILD_SERVER=0` and `MSBUILDDISABLENODEREUSE=1` as well, so child build probes do not reuse a node from another sandbox. After completing the native hold, all four projects are rebuilt with those variables and the final full suites run with --no-build.

Every test command exported a PATH without `/mnt/` entries, `UseSharedCompilation=false`, and a writable temporary `XDG_RUNTIME_DIR`. Runs used `dotnet test IDE/<project>/ -- --report-xunit ...`; headless suites also used `--hangdump` (10 minutes for runtime integration, 20 for IDE integration, 45 for the full IDE suite). The focused guards used `--filter-class`.

| Run | Passed | Failed | Skipped | Total |
| --- | ---: | ---: | ---: | ---: |
| HexIDE.Runtime.Integration.Tests | 5553 | 0 | 50 | 5603 |
| HexIDE.Runtime.Tests | 13545 | 0 | 27 | 13572 |
| HexIDE.Integration.Tests | 4659 | 0 | 0 | 4659 |
| MissingLanguageDocTests | 20 | 0 | 0 | 20 |
| ProviderTestSortTests | 11 | 0 | 0 | 11 |
| HexIDE.Tests (full) | 10817 | 0 | 83 | 10900 |

The completed xUnit reports are also checked with `scripts/check-xunit-report.sh` for runner errors that MTP could omit. MISSING_LANGUAGE status counts did not move: MsgBox remains Partial. No message text, public add-in API, dependency or test-count floor changed. The five test projects were searched for #834 and old refusal pins; none needed replacing.

`scripts/check-tree-hygiene.sh` passes with nine pre-existing warnings (five machine-path warnings and four upstream-attribution warnings); its optional identity scan is skipped because `HEXIDE_IDENTITY_PATTERN` is unset. `git diff --check` passes.

## Skipped tests

Skipped tests below provide no coverage for this run. No new #834 test skipped. Headless SetEnabled is a no-op: the native tests observe the real bridge's calls and ownership-cleanup timing, and do not claim an operating-system IsWindowEnabled measurement. Each row names the exact method; the count includes its skipped data rows.

| Suite | Test | Cases | Reason |
| --- | --- | ---: | --- |
| HexIDE.Runtime.Integration.Tests | `LabelAutoSizeTests.Windows_enable_disable_and_alignment_edges` | 2 | Requires Windows GDI and the measured fonts |
| HexIDE.Runtime.Integration.Tests | `LabelAutoSizeTests.Windows_measured_caption` | 19 | Requires Windows GDI and the measured fonts |
| HexIDE.Runtime.Integration.Tests | `LabelAutoSizeTests.Windows_sequence_and_italic` | 2 | Requires Windows GDI and the measured fonts |
| HexIDE.Runtime.Integration.Tests | `LabelAutoSizeTests.Windows_wrapping_and_center_divergence` | 4 | Requires Windows GDI and the measured fonts |
| HexIDE.Runtime.Integration.Tests | `SortedListItemsTests.Windows_keeps_every_measured_code_point` | 13 | #5059: exact en-GB order requires Windows NLS. |
| HexIDE.Runtime.Integration.Tests | `ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements` | 1 | Requires the oracle's Windows Segoe UI font build |
| HexIDE.Runtime.Integration.Tests | `ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent` | 1 | Requires Windows GDI |
| HexIDE.Runtime.Integration.Tests | `ToolbarRuntimeTests.Running_bar_uses_console_message_font` | 8 | Requires Windows with Segoe UI 9 message font |
| HexIDE.Runtime.Tests | `ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `DeclareCallTests.Windows_KeepsRefusal` | 1 | Windows-only refusal |
| HexIDE.Runtime.Tests | `ErrorFidelityTests.KeptHresult_WindowsMessagesMatchMeasurements` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `FileSystemTests.CurDir_uses_only_the_first_drive_character` | 1 | Windows filesystem attributes required. |
| HexIDE.Runtime.Tests | `FileSystemTests.Windows_attribute_rows` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `FileSystemTests.Windows_error_rows` | 1 | Windows filesystem attributes required. |
| HexIDE.Runtime.Tests | `FileSystemTests.Windows_name_rows` | 1 | Windows filesystem attributes required. |
| HexIDE.Runtime.Tests | `FileSystemTests.Windows_numeric_rows` | 1 | Windows filesystem attributes required. |
| HexIDE.Runtime.Tests | `LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `RecursionCostProbe.Measure` | 1 | Opt-in diagnostic: set HEXIDE_RECURSION_PROBE=1 |
| HexIDE.Runtime.Tests | `RecursionCostProbe.MeasureCollectionsWithTheSameActivationsRetainedAfterReturn` | 1 | Opt-in diagnostic: set HEXIDE_RECURSION_STACK_GC_PROBE=1 |
| HexIDE.Runtime.Tests | `ShellTests.BatchFileRuns` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `ShellTests.ConvertedPathFindsExecutableInCurrentDirectory` | 1 | Windows filesystem attributes required. |
| HexIDE.Runtime.Tests | `ShellTests.ExecuteDeniedIs5` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `ShellTests.RecordsMeasuredStylesAndDefault` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `UiThreadTests.Child_ReportsTheUiThreadsStack` | 1 | Runs only as the child of OnLinux_TheUiThreadsStack_IsTheSameWhateverUlimitSays |
| HexIDE.Runtime.Tests | `Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6` | 1 | Set HEXIDE_ORACLE=1 to compile round-tripped forms with VB6. |
| HexIDE.Runtime.Tests | `Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Runtime.Tests | `VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Runtime.Tests | `WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date` | 1 | Windows-only: this pins behaviour the product deliberately varies by host. |
| HexIDE.Tests (full) | `ChildProcessJobTests.A_closed_job_refuses_a_process` | 1 | Job objects exist only on Windows. |
| HexIDE.Tests (full) | `ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it` | 1 | Job objects exist only on Windows. |
| HexIDE.Tests (full) | `ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it` | 1 | Job objects exist only on Windows. |
| HexIDE.Tests (full) | `ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CodeTrustTests.MacOS_reads_the_quarantine_attribute` | 1 | macOS-only: asks macOS's own tools about the result. |
| HexIDE.Tests (full) | `CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity` | 1 | No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'. |
| HexIDE.Tests (full) | `ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `LinuxRepackerTests.A_linux_build_is_refused_on_windows` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `MacRepackerTests.A_mac_build_is_refused_on_windows` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `MacRepackerTests.A_mac_build_is_repacked_as_one` | 1 | macOS-only: asks macOS's own tools about the result. |
| HexIDE.Tests (full) | `MacRepackerTests.A_sealed_bundle_is_resealed_after_branding` | 1 | macOS-only: asks macOS's own tools about the result. |
| HexIDE.Tests (full) | `MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon` | 1 | macOS-only: asks macOS's own tools about the result. |
| HexIDE.Tests (full) | `MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main` | 1 | A made program is started on Windows only: elsewhere it needs a desktop session. |
| HexIDE.Tests (full) | `PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on` | 1 | Avalonia's Win32 platform runs only on Windows |
| HexIDE.Tests (full) | `ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProfileContainmentTests.JunctionEscapeIsRefused` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProfileReaderSafetyTests.WindowsPipeHandleIsRefused` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProjectLocalStateTests.Directory_junction_is_ignored_and_never_written_through` | 1 | Directory junctions require Windows; symbolic links are covered on Linux. |
| HexIDE.Tests (full) | `ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `ResolvedTempTests.Resolve_returns_the_original_path_on_Windows` | 1 | Windows preserves the supplied path spelling. |
| HexIDE.Tests (full) | `TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` | 1 | A link to a share is a Windows path. |
| HexIDE.Tests (full) | `TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary` | 1 | Registry type libraries require Windows. |
| HexIDE.Tests (full) | `UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault` | 1 | Set HEXIDE_UNOBSERVED_PROBE and provide netcoredbg to run adapter teardown diagnostics. |
| HexIDE.Tests (full) | `UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault` | 6 | Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics. |
| HexIDE.Tests (full) | `UserDataPathTests.OnWindowsItIsUnderAppData` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind` | 1 | Windows-only: this pins behaviour the product deliberately varies by host filesystem. |
| HexIDE.Tests (full) | `WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Included_folder_below_a_link_is_allowed` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Included_missing_folder_is_refused` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.Included_protected_files_are_refused_without_output` | 12 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it` | 1 | Needs a Windows x64 test host, whose launcher stands in for a stock executable. |
| HexIDE.Tests (full) | `WireStressTests.ConcurrentConnectionsKeepEveryFrameValid` | 1 | Set HEXIDE_WIRE_STRESS_SECONDS to run the wire stress test (#3520) |
| HexIDE.Tests (full) | `WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile` | 1 | Type libraries are read only on Windows. |
| HexIDE.Tests (full) | `WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole` | 1 | Type libraries are read only on Windows. |

## Left unchanged and live check

- Form.Enabled reads during the wait, keyboard behaviour and forms first loaded/reloaded or first shown while the box waits have no new VB6 expectations pinned here. A form loaded when the box opens is held even if hidden.
- Timers and the pump, including the made-program divergence #5491, remain unchanged. InputBox, CommonDialog, Show vbModal and IDE-owned boxes are outside this change.
- The supervisor must verify the running IDE: two program forms reject real clicks while a MsgBox waits; hiding and re-showing its owner does not admit a MouseDown or Click; answering restores clicks on both forms and preserves disabled/enabled settings. Check native frame interaction and control appearance too. This run used headless tests and did not drive the live IDE.

## Possible drift

The latest issue comment suggests a hold combined into effective enablement, which this change uses. That can change a control's disabled rendering: `VBCommandButton.UpdateCaptionEffect` observes IsEffectivelyEnabled, whereas the existing `VBFormRuntime.HeldBy` modal-form hold deliberately preserves drawing. MsgBox appearance was not measured by this oracle probe. Following the suggested mechanism could therefore depart from the project's VB6 visual-fidelity goal; it is cheap to revise and is flagged for the supervisor's live check rather than assigned an invented VB6 appearance.

# #4926 baseline test results

Run on `upstream/main` / branch base `da7307e4f`, 2026-11-19, with the new tests and unchanged runtime code:

```sh
dotnet test IDE/HexIDE.Runtime.Tests/ --no-restore --no-build -- --filter-class '*FontObjectTests' --report-xunit --report-xunit-filename before4926.xml
```

The 229 measured assignment rows produced 214 not-implemented failures, two failures because this interpreter-only suite has no Avalonia font manager, and 13 passes. Count **214** as behavioural refusal teeth; the two setup failures do not prove a missing assignment. The whole class then had 234 tests: 217 failed and 17 passed. The third additional failure was the non-fresh refusal guard (`Name = "Arial"`, then `Italic = 1`), which also encountered the missing font manager on main. The other four existing/refusal tests passed.

`Courier New` and `Tahoma` already have implementation paths via `FontMembers.Named`; their failures here are setup failures, not claimed new VB6 behaviour. The fresh-name implementation now directly records their measured descriptors without a platform font lookup. Twenty-four guards for unmeasured input types and names were added afterwards; they are not counted as teeth.

Source: [oracle PR #4943](https://github.com/hexide-io/HexIDE/pull/4943), [measured rows](../../docs/vb6-fidelity-oracle/a-free-standing-stdfonts-values-of-every-type.md). As directed by [#4926](https://github.com/hexide-io/HexIDE/issues/4926), tests use `As Object` although the oracle measured `As StdFont`.

## Not-implemented failures (214 rows)

- `Bold = -0.5`
- `Bold = -2`
- `Bold = 0.4`
- `Bold = 0.5`
- `Bold = 1.5`
- `Bold = 2`
- `Bold = CByte(1)`
- `Bold = CCur(0)`
- `Bold = CLng(-1)`
- `Bold = CSng(1)`
- `Bold = Empty`
- `Bold = "0"`
- `Bold = "1"`
- `Bold = "False"`
- `Bold = ""`
- `Bold = "true"`
- `Bold = "x"`
- `Charset = -2`
- `Charset = 128.5`
- `Charset = 129`
- `Charset = 130`
- `Charset = 134`
- `Charset = 136`
- `Charset = 161`
- `Charset = 162`
- `Charset = 163`
- `Charset = 177`
- `Charset = 178`
- `Charset = 186`
- `Charset = 2.5`
- `Charset = 204`
- `Charset = 222`
- `Charset = 238`
- `Charset = 254`
- `Charset = 3`
- `Charset = 32767`
- `Charset = 4`
- `Charset = 77`
- `Charset = 86`
- `Charset = CByte(2)`
- `Charset = CCur(128)`
- `Charset = CLng(65536)`
- `Charset = Empty`
- `Charset = False`
- `Charset = Null`
- `Charset = True`
- `Charset = "2"`
- `Charset = "x"`
- `Italic = -1`
- `Italic = 0`
- `Italic = 0.5`
- `Italic = 1`
- `Italic = 2`
- `Italic = CByte(1)`
- `Italic = CLng(1)`
- `Italic = Empty`
- `Italic = Null`
- `Italic = "0"`
- `Italic = "1"`
- `Italic = "False"`
- `Italic = "True"`
- `Italic = ""`
- `Italic = "true"`
- `Italic = "x"`
- `Name = 5.5`
- `Name = Empty`
- `Name = String(31, "A")`
- `Name = String(32, "A")`
- `Name = String(33, "A")`
- `Name = String(40, "A")`
- `Name = True`
- `Name = " Arial"`
- `Name = "ARIAL"`
- `Name = "Arial "`
- `Name = "Fixedsys"`
- `Name = "Lucida Console"`
- `Name = "MS Sans Serif"`
- `Name = "MS Serif"`
- `Name = "Marlett"`
- `Name = "Microsoft Sans Serif"`
- `Name = "Segoe UI"`
- `Name = "Small Fonts"`
- `Name = "System"`
- `Name = "Terminal"`
- `Name = "Webdings"`
- `Name = "arial"`
- `Name = "ms sans serif"`
- `Size = #1/2/1900#`
- `Size = -0.5`
- `Size = 0.1`
- `Size = 0.5`
- `Size = 0.75`
- `Size = 10.25`
- `Size = 1000`
- `Size = 10000`
- `Size = 11`
- `Size = 12.3`
- `Size = 12.5`
- `Size = 12.6`
- `Size = 12.75`
- `Size = 13`
- `Size = 144`
- `Size = 15`
- `Size = 16`
- `Size = 17`
- `Size = 18`
- `Size = 19`
- `Size = 2`
- `Size = 2000`
- `Size = 20000`
- `Size = 21`
- `Size = 2159`
- `Size = 2161`
- `Size = 22`
- `Size = 23`
- `Size = 24`
- `Size = 24575`
- `Size = 24576`
- `Size = 24577`
- `Size = 25`
- `Size = 26`
- `Size = 27`
- `Size = 28`
- `Size = 29`
- `Size = 3`
- `Size = 30`
- `Size = 3000`
- `Size = 30000`
- `Size = 3001`
- `Size = 31`
- `Size = 32`
- `Size = 32767`
- `Size = 33`
- `Size = 34`
- `Size = 35`
- `Size = 36`
- `Size = 37`
- `Size = 38`
- `Size = 39`
- `Size = 4`
- `Size = 40`
- `Size = 48`
- `Size = 5`
- `Size = 500`
- `Size = 5000`
- `Size = 6`
- `Size = 7`
- `Size = 72`
- `Size = 8`
- `Size = 8.25`
- `Size = 9.5`
- `Size = 96`
- `Size = CByte(12)`
- `Size = CCur(10.5)`
- `Size = CLng(12)`
- `Size = CLng(32768)`
- `Size = CSng(10.5)`
- `Size = Empty`
- `Size = False`
- `Size = True`
- `Size = " 12"`
- `Size = "&H10"`
- `Size = "12.5"`
- `Size = "1e1"`
- `Size = ""`
- `Strikethrough = 1`
- `Strikethrough = 2`
- `Strikethrough = Empty`
- `Strikethrough = Null`
- `Strikethrough = "True"`
- `Strikethrough = "x"`
- `Underline = 1`
- `Underline = 2`
- `Underline = Empty`
- `Underline = Null`
- `Underline = "True"`
- `Underline = "x"`
- `Weight = -2`
- `Weight = 1`
- `Weight = 1001`
- `Weight = 101`
- `Weight = 200`
- `Weight = 2000`
- `Weight = 300`
- `Weight = 32766`
- `Weight = 399`
- `Weight = 401`
- `Weight = 450`
- `Weight = 500`
- `Weight = 549`
- `Weight = 550.4`
- `Weight = 550.5`
- `Weight = 550.6`
- `Weight = 551.5`
- `Weight = 552`
- `Weight = 650`
- `Weight = 699`
- `Weight = 701`
- `Weight = 800`
- `Weight = 899`
- `Weight = 99`
- `Weight = 999`
- `Weight = CByte(255)`
- `Weight = CCur(600)`
- `Weight = CLng(-32768)`
- `Weight = CLng(-32769)`
- `Weight = CLng(32768)`
- `Weight = Empty`
- `Weight = False`
- `Weight = Null`
- `Weight = True`
- `Weight = "700"`
- `Weight = ""`
- `Weight = "x"`

## Font-manager setup failures (2 rows)

- `Name = "Courier New"`
- `Name = "Tahoma"`

## Already passed (13 rows)

- `Bold = Null`
- `Name = Null`
- `Size = -1`
- `Size = 0`
- `Size = 1`
- `Size = 10`
- `Size = 12`
- `Size = 14`
- `Size = 20`
- `Size = 9`
- `Size = Null`
- `Strikethrough = True`
- `Underline = True`

## Final validation

- `FontObjectTests`: 258 passed, none skipped (229 measured rows, 27 refusal guards and two initial-name rows).
- Full `HexIDE.Runtime.Tests`: 11,339 passed, 27 skipped, no failures; xunit report checked.
- Full `HexIDE.Runtime.Integration.Tests`: 4,414 passed, 10 skipped, no failures; xunit report checked.
- Full `HexIDE.Integration.Tests`: 4,464 passed, none skipped, no failures; xunit report checked.
- Full `HexIDE.Tests`: 10,413 passed, 78 skipped, no failures; 31m 55s, xunit report checked.
- `MissingLanguageDocTests`: 20 passed. `DesignRecordTests`: six passed. MISSING_LANGUAGE counts unchanged.
- Desktop Debug build: zero warnings and errors. Tree hygiene passed with nine existing warnings.

All runs removed Windows PATH entries and exported `UseSharedCompilation=false` and a writable temporary `XDG_RUNTIME_DIR`. MSBuild server reuse was also disabled after an initial connection timeout. The first full Runtime/Runtime Integration runs preceded a final restriction to the measured Name spellings; both were repeated in full afterwards. The focused font class also passed afterwards. Integration's unchanged control-font paths and the full IDE suite ran alongside these checks.

The latest #4926 comment's “That leaves” input-type summary does not mention Name's Byte, Long, Single and Currency inputs, which have no measured Name rows. These retain the explicit #4926 refusal under the same comment's “Values of a type no row shows” rule, rather than extrapolating a result.

### Skipped tests

These skips are not claimed as coverage. The new StdFont cases all ran on Linux.

#### HexIDE.Runtime.Tests

- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`: Runs only as the child of OnLinux_TheUiThreadsStack_IsTheSameWhateverUlimitSays
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.DeclareCallTests.Windows_KeepsRefusal`: Windows-only refusal
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`: Set HEXIDE_ORACLE=1 to compile round-tripped forms with VB6.
- `HexIDE.Runtime.Tests.ErrorFidelityTests.KeptHresult_WindowsMessagesMatchMeasurements`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.RecursionCostProbe.MeasureCollectionsWithTheSameActivationsRetainedAfterReturn`: Opt-in diagnostic: set HEXIDE_RECURSION_STACK_GC_PROBE=1
- `HexIDE.Runtime.Tests.RecursionCostProbe.Measure`: Opt-in diagnostic: set HEXIDE_RECURSION_PROBE=1
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.ShellTests.ConvertedPathFindsExecutableInCurrentDirectory`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.ShellTests.ExecuteDeniedIs5`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.ShellTests.BatchFileRuns`: Windows-only: this pins behaviour the product deliberately varies by host.

#### HexIDE.Runtime.Integration.Tests

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`: Requires Windows GDI
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`: Requires the oracle's Windows Segoe UI font build
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1: Set b = Toolbar1.Butto"···, property: "b.Height", measured: 329.95280000000002)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1: Set b = Toolbar1.Butto"···, property: "Toolbar1.Height", measured: 420)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1: Set b = Toolbar1.Butto"···, property: "b.Width", measured: 870.23630000000003)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Toolbar1.TextAlignment = 1", property: "Toolbar1.ButtonHeight", measured: 569.76430000000005)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \\"Alpha\\")", property: "b.Width", measured: 569.76379999999995)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \\"Alpha\\")", property: "Toolbar1.Height", measured: 660)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \\"Alpha\\", 5)", property: "b.Width", measured: 794.8347)`: Requires Windows with Segoe UI 9 message font
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarRuntimeTests.Running_bar_uses_console_message_font(setup: "Set b = Toolbar1.Buttons.Add(, , \\"Alpha\\")", property: "b.Height", measured: 569.76379999999995)`: Requires Windows with Segoe UI 9 message font

#### HexIDE.Tests

- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`: A made program is started on Windows only: elsewhere it needs a desktop session.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`: Job objects exist only on Windows.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`: Registry type libraries require Windows.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`: A link to a share is a Windows path.
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on`: Avalonia's Win32 platform runs only on Windows
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`: Set HEXIDE_WIRE_STRESS_SECONDS to run the wire stress test (#3520)
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault`: Set HEXIDE_UNOBSERVED_PROBE and provide netcoredbg to run adapter teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "RenameWireShapeTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "ShutdownWireShapeTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: "CancellationWireShapeTests")`: Set HEXIDE_UNOBSERVED_PROBE to run teardown diagnostics.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`: Type libraries are read only on Windows.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.signed")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.

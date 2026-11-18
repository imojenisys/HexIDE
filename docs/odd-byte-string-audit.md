# Odd-byte Strings (#4576)

The permitted operations and refusals are scoped by [#4576](https://github.com/hexide-io/HexIDE/issues/4576).
Results come from [the measured byte-string functions](vb6-fidelity-oracle/the-byte-string-functions.md),
VB6 SP6, native and p-code agreeing. This is an implementation audit, not another oracle page.

`OddByteString` carries an odd-length `byte[]`. Even byte results become ordinary UTF-16 Strings by
pairing low and high bytes directly, without an encoding fallback. String and Variant stores retain
it. The byte producers, Len/LenB, InStrB, concatenation and binary `=`/`<` are the only permitted uses,
plus the measured TypeName and VarType observations. Concatenation retains the ordinary branch's
fixed/Variant expression metadata policy, including its existing Null path. Every other built-in argument is refused at the
registry boundary, including calls made directly by conditional compilation. Async built-ins check
before calling the host. Refusals are program-facing English, naming the operation and #1510.

## Text-reader audit

The audit searched `(string)…Value`, `.Value as string`, and `Value is string` throughout
`IDE/HexIDE.Runtime`. These are the sites that cannot receive an odd String at the indicated read:

- `Serialization/MaskedEdBoxPersistence.WriteProperty`: dictionary entries hold persisted CLR property
  values, not interpreter `Vb6Value` payloads.
- `BuiltinControls/VBCaption`'s `e.NewValue is string`: an Avalonia property change carries CLR text.
  Interpreter property writes are checked before entering the control.
- `BuiltinControls/VBUpDown`'s persisted BuddyControl lookup: serialized form-property values, not
  executed program values. Runtime buddy-property writes have an explicit odd-byte refusal.
- `Interpreter/VB6BuiltIns.Interaction`: environment-variable dictionary entries are CLR strings from
  the operating system, not program values.
- `Interpreter/ConditionalCompilation/ConstantExpression`: its closed function set cannot produce
  ChrB or byte-slice results, and constants are independent of runtime variable storage. The direct
  built-in registry boundary also refuses unsupported odd arguments.
- `Interpreter/BasicInterpreter.CallByName`'s member-name cast: the name must be an ordinary
  String, and every argument is checked for odd Strings before dispatch.
- `Interpreter/Vb6Value.CopyString`: the cast is gated on the ordinary String kind. Odd values have
  no mutable text buffer and are retained as values.
- `Interpreter/Vb6Value.TryCompareTo`: ordinary String casts are gated on that kind; odd operands
  explicitly refuse here. The measured operators use the separate byte comparator.
- `Interpreter/VbNumeric.CoerceOnStore`'s String-to-Date branch and `ToLongLong`'s String parser:
  ordinary String type checks guard their casts; odd input refuses before these conversions.
- `Interpreter/ExpressionExecutor.AddOrSubtract`: its three text casts require ordinary String kinds;
  odd input refuses before the Null and String branches.
- `Interpreter/StatementExecutor`'s Select Case text casts: selectors and Case values explicitly
  refuse odd Strings. `TextOf` falls through to the explicit refusal in VbDisplay.
- `Interpreter/VbUdtLayout`: text layout is for fixed-length String fields. Their stores refuse odd
  input in FixedLengthString, so these fields cannot contain it.

The remaining readers explicitly refuse odd values before a text cast, string type test or `as`
fallback (including text read after CoerceOnStore). They are in:

- `AvaloniaInterop`: SSTabMembers, SliderMembers, TextBoxMembers, RichTextBoxMembers,
  FlexGridMembers, ListMembers and FontMembers.
- `Components/InBox`: TabStripModel, TabStripTabs and TreeNodeAutomation.
- `BuiltinControls`: VBDTPicker, VBAnimation, VBCommonDialog, VBUserControl, VBMaskEdBox,
  VBImageList, ToolbarItems, VBToolbar and VBUpDown.
- `Interpreter`: VbApp, VbCollection, BasicInterpreter's CreateObject path, FixedLengthString,
  ImageComboItems, ListViewItems (including TagText), VbErr, FormControls and StatusBarPanels.
- Built-in helpers: Array's Join text reader, DateTime's string reader, Format's string readers,
  Inspection's numeric/date tests and Resources' text reader. The registry refuses odd arguments
  before these helpers; the readers also refuse explicitly.

User-procedure argument binding, property and host-method routes refuse odd arguments before handing them to controls or proxies.
File paths, Print #/Debug.Print, VbDisplay, Put, fixed-length stores, numeric conversion, arithmetic,
bitwise operations, conditions, Select Case and Mid/MidB statements also refuse explicitly. Collection
indexes refuse before the empty-Collection and numeric-index paths, so neither can replace the #1510
refusal with a trappable error. Byte-array-to-String stores also refuse explicitly, rather than
falling into the generic array-as-text error 13 (which still applies to `Array(1)`, as #879 measured). Bare form Print is already unimplemented before its operands are used (#989).

## Left unimplemented

The plain string functions (UCase, Trim, Replace, StrReverse, CStr, Left, Mid, Right and their `$`
forms), byte-slice functions with odd input, MidB statement operands holding odd Strings,
Byte-array conversions, user-procedure arguments and InputB remain outside this slice, under #1510. So do display and file
output of odd Strings, concatenation with numeric or object operands, other relational operators,
Option Compare Text, StrComp, Like and conversions to other declared types. No behaviour is inferred
for these uses. Unlisted `$` result-conversion calls with odd results (`ChrB$`, `MidB$`, `LeftB$`,
`RightB$`) also remain on the existing refusal path under the brief's blanket refusal rule. ChrB refuses Boolean, Empty, Date, object, array, Decimal and LongLong arguments;
Null is the measured error 94 rather than a refusal.

## Validation (2026-11-18, WSL)

The final `ByteStringTests` run passed all 158 cases with no skips. Running the same tests
against the original product sources failed 106 cases and passed 52 existing regression guards.
The guards cover ordinary byte-string function results, measured errors, declared-scalar LenB
sizes and numeric-expression LenB compile errors; they already pass on main.

The supplied mutation checker detected both compiling mutations: restoring the ChrB refusal
failed 90 cases; dropping each odd concatenation operand’s last byte failed nine. Each baseline
and restored-source run passed all 158 cases.

| Final check | Passed | Skipped | Failed | Total |
| --- | ---: | ---: | ---: | ---: |
| Runtime | 10000 | 20 | 0 | 10020 |
| Runtime Integration | 3293 | 2 | 0 | 3295 |
| IDE | 10164 | 72 | 0 | 10236 |

MissingLanguageDocTests also passed all 20 cases. The full IDE run completed in 1h 02m 53s
without interruption. All three xUnit reports record zero out-of-test errors.

MISSING_LANGUAGE totals remain 1,192: Dies moves from 59 to 58 and Partial from 506 to 507.
The intrinsic-function subtotal moves from three to two Dies and 78 to 79 Partial.

### Skipped tests

These skips are not coverage for #4576. Every byte-string test runs on Linux.

#### Runtime (20)

- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`
- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`
- `HexIDE.Runtime.Tests.LocaleCompareTests.StrComp_matches_every_measured_cell_on_Windows`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`
- `HexIDE.Runtime.Tests.ShellTests.RecordsMeasuredStylesAndDefault`
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`

#### Runtime Integration (2)

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`

#### IDE (72)

- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`
- `HexIDE.Tests.IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute`
- `HexIDE.Tests.IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file`
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `HexIDE.Tests.LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`
- `HexIDE.Tests.LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`
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
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`
- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`
- `HexIDE.Tests.Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding`
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`
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
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output`
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

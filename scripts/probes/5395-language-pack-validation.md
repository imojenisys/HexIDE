# Provider language-pack move (#5395)

Checked against `7c7f1d4f9` on WSL, 2026-11-24.

The migration moves 112 existing English keys and adds only
`Str.Standalone.WaylandUnavailableEnvironment`. The shell has 1,434 English keys;
the provider has 113. All 39 neutral packs remain complete in both areas. Four
regional files contribute provider overrides, including `pt-BR` (discovered from
the folder rather than a region list). The test-only `zz` and `zz-ZZ` fixtures
are unchanged and are not shipped pack areas.

Before migration, a temporary test dumped `LocalizationService.TryResolveMap`
for all 164 distinct manifest language/region IDs, with `DistributionIdentity.Stock`
and `new DistributionIdentity("acme", "Acme")`. After migration the same dump
registered the provider area. All 328 map pairs preserved all 1,546 old keys and
values exactly. Each new map adds only the Standalone key, with the same value
as that locale's existing startup Wayland key. Raw JSON union comparison also
preserved every key/value in all 43 pack files. An isolated replay of the
migration script reproduced all 86 shell/provider files byte for byte.

`ProviderPackTests` retains the map-dump hook: set `HEXIDE_MAP_DUMP` to a scratch
directory when running it. The before-layout capture omits area registration
and the assertion for the new key.

Four behavior tests were copied into an untouched main snapshot and failed:

- Provider registration/disposal: no pack registration occurred.
- German property description lifecycle: the description was already available
  before provider initialization, because main still owns it in the shell.
- Standalone embedded English: 1,546 shell entries, rather than 113 provider entries.
- Language/region provider resolution: no provider `en.json` existed to register.

Seven source guard tests failed on eight deliberate faults, exercised with
`5395-language-pack-guards.py` in an isolated source snapshot using the built
test assembly. The same seven guards first passed on the clean snapshot. The probe restores each mutated file:

- Remove `Str.PropDesc.Caption` from provider German: parity fails.
- Remove `Str.Vb6Toolchain.Vb6NotFound` from provider English: GetString coverage fails.
- Duplicate `Str.PropDesc.Caption` in shell English: overlap fails.
- Add `Str.Unowned.Probe` to provider English: the grant check fails.
- Add a provider source literal for that undefined key: provider ownership fails.
- Add a new shell file reading a provider key: the shrinking reader list fails.
- Add a third production pack area: the pinned area set fails.
- Add a foreign key literal in Standalone: its two-key guard fails.

Existing translation assertions remain regression guards; the move changes
ownership and packaging, not their translated values or any VB6 runtime behavior.

Supervisor live check: select German, open a VB6 form, select a property such as
Caption in Properties, and confirm its German description is displayed from the
registered provider area. No headless result substitutes for this check.

The Standalone diagnostic is read by a person, so its split remains a localized
key with each pack's existing startup text. There are no new add-in API members.
`MISSING_FEATURES.md` statuses and `MISSING_LANGUAGE.md` counts do not change.
No VB6 measurements were needed or invented. Possible drift: none.

The shrinking shell-reader baseline includes the existing
`PropertiesToolViewModel` picture-import refusal (`NotImplementedYet`), alongside
`LocalizationService`'s computed property-description lookup. Generic `Str.`
resource-policy checks are not provider text reads.

## Files changed

- `IDE/HexIDE/Localization/Packs/*.json` and
  `IDE/HexIDE.Runtime/Localization/Packs/*.json`: 43 files in each area.
- `IDE/HexIDE.Runtime/BasicProvider.cs`, its project file,
  `IDE/HexIDE/Addins/RegistrationGrants.cs`, and Standalone's `Program.cs` and project file.
- Unit parity, coverage, localization-service, branding and dialog-chrome tests;
  new provider ownership, registration and map-resolution tests and their shared helper;
  the new Integration provider-language-pack tests; both generated `ProviderTestSort.md` inventories.
- `tools/TranslationCoverage/Program.cs`, the migration and guard-probe scripts,
  `CLAUDE.md`, `docs/localization-regions.md`, the live localization spec and `changelog.d/5395.md`.

Pack-test services register into private area registries. Their fixtures stay
independent of the process-lifetime provider registered by application-startup
tests, while each still reads and validates the real shipped provider packs.
The final map comparison was repeated with these isolated services: all 328 pairs
still preserve 1,546 existing keys, with only the split key added (1,547 total).


## Final verification

- Full `HexIDE.Tests`: 11,437 passed, 85 skipped, 11,522 total, zero failures
  (33m 30s). `HEXIDE_FOREIGN_LSP_DOWNLOAD=0`; the available cached servers ran.
- Full `HexIDE.Integration.Tests`: 4,800 passed, zero skipped or failed (13m 28s).
- Both final full-run xUnit reports passed `check-xunit-report.sh` with no setup/discovery errors.
- All 272 affected unit tests passed with zero skips; the focused Integration run passed all nine tests.
- Four new behavior tests failed on untouched main. Seven guards passed clean, then failed on eight deliberate faults.
- Desktop Debug build: zero warnings and errors. TranslationCoverage: both areas complete.
- Tree hygiene: passed with nine existing non-blocking warnings. `git diff --check`: passed.
- OpenSpec strict validation retains eight pre-existing requirement-length warnings; the new requirements add none.

The full runs exposed and resolved the old shell-only template-key reader and
fixture registration into the process-wide registry. The final tests use the
canonical union and private registries. Product registration remains through the
provider's host. The test-sort inventories were regenerated for the two new files
that bind provider types; `ProviderBoundaryTests` and its reference list are unchanged.

## Skipped cases (WSL)

The full runs skipped these 85 cases: 61 require Windows, four require macOS,
two need unavailable fonts, ten need VB6 template fixtures, and eight are opt-in
diagnostic probes. They provide no coverage here. All new behavior and ownership
tests ran on Linux.

| Case | Reason |
|---|---|
| `Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` | Windows required |
| `IDE.CodeTrustTests.MacOS_reads_the_quarantine_attribute` | macOS required |
| `IDE.CodeTrustTests.Windows_reads_the_Zone_Identifier_stream_beside_a_file` | Windows required |
| `IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` | Windows required |
| `IDE.ProjectLocalStateTests.Directory_junction_is_ignored_and_never_written_through` | Windows required |
| `IDE.Vb6ToolchainReplacementTests.Absolute_Path32_Make_and_Run_use_that_folder` | Windows required |
| `Infrastructure.PlatformWindowProbeTests.Avalonia_still_creates_the_window_the_session_end_hook_answers_on` | Windows required |
| `Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named` | Windows required |
| `Infrastructure.ResolvedTempTests.Resolve_returns_the_original_path_on_Windows` | Windows required |
| `Infrastructure.UnobservedTaskProbeTests.Foreign_adapter_teardown_leaves_no_unobserved_fault` | Optional diagnostic probe |
| `Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: \"CancellationWireShapeTests\")` | Optional diagnostic probe |
| `Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: \"RenameWireShapeTests\")` | Optional diagnostic probe |
| `Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: \"ShutdownWireShapeTests\")` | Optional diagnostic probe |
| `Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: \"StdioProcessLspTransportTests\")` | Optional diagnostic probe |
| `Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: \"TraceOnTheWireTests\")` | Optional diagnostic probe |
| `Infrastructure.UnobservedTaskProbeTests.Teardown_leaves_no_unobserved_fault(testClass: \"WebSocketLspTransportTests\")` | Optional diagnostic probe |
| `Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` | Windows required |
| `LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` | Windows required |
| `LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` | Windows required |
| `LspClient.TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary` | Windows required |
| `LspClient.WireStressTests.ConcurrentConnectionsKeepEveryFrameValid` | Optional diagnostic probe |
| `LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile` | Windows required |
| `LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole` | Windows required |
| `Processes.ChildProcessJobTests.A_closed_job_refuses_a_process` | Windows required |
| `Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it` | Windows required |
| `Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it` | Windows required |
| `Profiles.ProfileContainmentTests.JunctionEscapeIsRefused` | Windows required |
| `Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead` | Windows required |
| `Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused` | Windows required |
| `Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header` | VB6 template fixtures unavailable |
| `Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched` | VB6 template fixtures unavailable |
| `Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce` | VB6 template fixtures unavailable |
| `Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity` | VB6 template fixtures unavailable |
| `Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case` | Windows required |
| `Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused` | Windows required |
| `Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main` | Windows required |
| `Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows` | Windows required |
| `Repack.MacRepackerTests.A_mac_build_is_refused_on_windows` | Windows required |
| `Repack.MacRepackerTests.A_mac_build_is_repacked_as_one` | macOS required |
| `Repack.MacRepackerTests.A_sealed_bundle_is_resealed_after_branding` | macOS required |
| `Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon` | macOS required |
| `Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none` | Windows required |
| `Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows` | Windows required |
| `Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand` | Windows required |
| `Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it` | Windows required |
| `Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind` | Windows required |
| `Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so` | Windows required |
| `Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published` | Windows required |
| `Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report` | Windows required |
| `Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed` | Windows required |
| `Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside` | Windows required |
| `Repack.WindowsRepackerTests.Included_missing_folder_is_refused` | Windows required |
| `Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"HexIDE.exe\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"LICENSE\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"acme-ide.exe\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"brand.json\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"branding/logo.png\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"hexide-root.pub\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"nested/HEXIDE-FIRSTPARTY.PUB\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.json\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.sig\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations-cache.signed\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.json\")` | Windows required |
| `Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: \"revocations.sig\")` | Windows required |
| `Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them` | Windows required |
| `Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks` | Windows required |
| `Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it` | Windows required |
| `Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible` | Windows required |
| `Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it` | Windows required |
| `Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet` | Windows required |
| `Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it` | Windows required |
| `Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn` | Windows required |
| `Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation` | Windows required |
| `Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot` | Windows required |
| `Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests` | Windows required |
| `UserDataPathTests.OnWindowsItIsUnderAppData` | Windows required |
| `ViewModels.FontDialogScriptTests.Installed_fonts_expose_the_measured_script(family: \"Arial\", charset: 161, script: \"Greek\")` | Font unavailable |
| `ViewModels.FontDialogScriptTests.Installed_fonts_expose_the_measured_script(family: \"Wingdings\", charset: 2, script: \"Symbol\")` | Font unavailable |

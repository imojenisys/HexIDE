# #3894 disconnect investigation

Run `bash scripts/probes/issue-3894/repeat.sh 200` from the repository root after building
`IDE/HexIDE.Tests/`. It executes the existing complete debugpy session with two `yes` CPU workers,
checks that exactly one test ran without errors, failures or skips, and retains each run's output
under `artifacts/issue-3894/`. A failure's assertions include the adapter exit code and stderr tail;
the test also prints the captured DAP conversation. Logs are disposable; record findings here.

## Teardown inspection (2026-11-13, aa93a7fd)

- `ExternalDebugService.StopCoreAsync` cancels startup, then awaits `DapSession.DisconnectAsync`
  with a five-second cancellation deadline. Its `finally` disposes the session. The start method's
  `finally` waits for `stopping` before disposing. Its update reader calls Stop on `terminated`.
- `DapSession.DisconnectAsync` marks inspection expired, sends `disconnect` with
  `terminateDebuggee:true`, awaits its response, then disposes in `finally`. The connection's default
  request timeout is 30 seconds. `terminated` only changes state and publishes an update; it does
  not dispose streams or complete the event reader.
- Session disposal asks `DapProcess` to stop. Its monitor disposes `DapConnection`, closing both
  streams (stdin included), then waits up to `ShutdownTimeout` for exit and kills the process tree
  if needed. The foreign fixture uses ten seconds. Process exit and transport completion also
  trigger cleanup independently of explicit disposal.
- Exceptions exist: a disconnect deadline/refusal, transport EOF/protocol failure, queue overflow,
  explicit concurrent disposal, launch failure/cancellation or a failed resume can cause cleanup
  before a pending disconnect response. In particular, cancellation during launch can enter
  `LaunchAsync`'s own disposal while Stop is sending disconnect. These are fault/startup paths;
  the foreign session has completed launch and resume before disconnect and does not use the UI
  service. None explains premature local closure on its successful response path by inspection.

`DapSessionTests.Terminated_during_disconnect_keeps_stdin_open_until_the_response` holds the reply,
consumes `terminated`, checks the connection and adapter's stdin are still open, sends the response,
and observes stdin EOF. It passes with the existing product code; it is an order check, not a
regression claimed to fail before a fix.

The pinned debugpy 1.8.22 wheel's
[`adapter/clients.py:disconnect_request`](https://github.com/microsoft/debugpy/blob/v1.8.22/src/debugpy/adapter/clients.py)
calls `request.respond({})` before closing its client channel.
[`common/messaging.py:JsonMessageChannel._run_handlers`](https://github.com/microsoft/debugpy/blob/v1.8.22/src/debugpy/common/messaging.py) exits 1
when a handler raises; stream closure closes the writer and reader. Reading that source establishes
a possible adapter-side failure route, not the cause of the reported CI exception. Exit 1 remains a
failure in the foreign test.

## Loaded reproduction (2026-11-13)

Ubuntu/WSL, Python **3.14.4** already on PATH, .NET **10.0.15**, pinned debugpy **1.8.22**
acquired and verified by the fixture (no pip/global installation). CI's reported Python was **3.14.10**;
this run does not establish behaviour on that patch version.

`bash scripts/probes/issue-3894/repeat.sh 200`: **200 passed, 0 failures, 0 skips**, with two concurrent
`yes` CPU workers. Each iteration ran `ForeignAdapterTests.Debugpy_runs_a_complete_session` in a fresh
xunit process and required exactly one executed test. Each checked adapter exit **0**, no forced
termination and no cleanup failure, as well as the complete session's wire assertions. There were no
failing exit codes or stderr traces to record. The script leaves per-iteration logs in artifacts; this
paragraph is the durable result. Earlier interrupted harness setup runs are excluded from these 200.

No premature closure was reproduced and no product fix is claimed. The existing order and debugpy's
own post-response channel closure are shown above; the CI exception's cause remains unresolved.
The test still rejects exit 1. A full CI traceback and/or reproduction on Python 3.14.10 would help
separate debugpy's local channel-close path from another failure.

## Checks

Windows `/mnt/` PATH entries were removed before running tests. The .NET home and package cache were
kept under this clone's `artifacts/` directory.

- Initial `dotnet test HexIDE.Tests/ --no-restore -- --filter-method '*Debugpy_runs_a_complete_session'`:
  1 passed, no failures or skips (download enabled for fixture acquisition).
- `HEXIDE_FOREIGN_LSP_DOWNLOAD=0 dotnet test HexIDE.Tests/ --no-build --no-restore -- --filter-namespace 'HexIDE.Tests.DebugAdapter'`:
  240 passed, no failures or skips, including both foreign adapters.
- `dotnet build HexIDE.Desktop/HexIDE.Desktop.csproj -c Debug --no-restore`: succeeded, zero warnings/errors.
- `bash scripts/check-tree-hygiene.sh`: passed, nine existing non-blocking warnings.
- `bash -n scripts/probes/issue-3894/repeat.sh` and `git diff --check`: passed.

No product behaviour changed, so the added order test passes on the pre-change product code:
**zero tests are claimed to fail before a fix**. No changelog fragment is added, as the issue's latest
comment requires one only for a product change. No language support status or MISSING_LANGUAGE count
changed. No changes under `IDE/HexIDE/` or the bundled LSP server require the integration/server suites.

Full suite: `cd IDE && timeout 45m env HEXIDE_FOREIGN_LSP_DOWNLOAD=0 dotnet test HexIDE.Tests/ --no-build --no-restore`: **7,910 passed, 0 failed, 66 skipped**, total 7,976, in 25m 57s.
The new order test also passed alone: 1 passed, no failures/skips.

The following skip inventory was checked separately after the full run (discovery plus targeted
execution of the skipped cases, not additional passing coverage). All 66 are accounted for below;
none is a DAP test. VB6 Template skips require the absent proprietary template tree; the others
require another host OS.

- `Addins.PackageVerificationTests` (1): `Locked_signature_file_yields_Untrusted_not_an_exception`.
- `IDE.EditorServiceNavigationTests` (1): `AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`.
- `Infrastructure.WindowStateUnconfirmedTests` (1): `On_Windows_the_probe_does_not_guess`.
- `LspClient.LspDocumentSessionTests` (1): `AWindowsDriveLetterInTheOtherCaseStillMatches`.
- `LspClient.TypeLibraryNamesTests` (1): `ALocalLinkToAShareIsNotALocalPath`.
- `LspClient.WorkspaceArtifactProjectTests` (2): `ALocalTypeLibraryIsNamedFromItsFile`, `TheTypeLibraryNamesOleAutomationStdole`.
- `Processes.ChildProcessJobTests` (3): `A_closed_job_refuses_a_process`, `A_process_left_out_of_the_job_outlives_it`, `Closing_the_job_ends_the_process_put_in_it`.
- `Profiles.ProfileContainmentTests` (2): `JunctionEscapeIsRefused`, `JunctionSwappedInAfterPreflightIsRefusedDuringRead`.
- `Profiles.ProfileReaderSafetyTests` (1): `WindowsPipeHandleIsRefused`.
- `Projects.ClassModuleSaveFidelityTests` (3): `A_reload_from_disk_also_preserves_the_header`, `Editing_the_body_leaves_the_header_untouched`, `Saving_a_project_leaves_an_untouched_class_byte_identical`.
- `Projects.CompanionBinaryPreservationTests` (7): `AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`, `A_form_that_reproduces_its_companion_is_never_denied_the_write`, `A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`, `A_second_save_still_leaves_the_companion_intact`, `Dropping_a_blob_bearing_control_keeps_both_halves_in_step`, `Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`, `The_form_records_that_it_lost_binary_fidelity`.
- `Projects.ContributedProjectTests` (1): `A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`.
- `Projects.DuplicateProjectOpenTests` (1): `Opening_the_same_project_with_different_drive_and_folder_case_is_refused`.
- `Projects.MadeProgramTests` (1): `A_made_program_starts_and_runs_its_Sub_Main`.
- `Repack.LinuxRepackerTests` (1): `A_linux_build_is_refused_on_windows`.
- `Repack.MacRepackerTests` (4): `A_mac_build_is_refused_on_windows`, `A_mac_build_is_repacked_as_one`, `A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`, `macOS_accepts_the_property_list_signature_and_icon`.
- `Repack.WindowsRepackerTests` (26): `A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`, `A_branded_process_declares_that_identity_to_Windows`, `A_default_profile_folder_travels_with_the_brand`, `A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`, `A_run_that_fails_partway_leaves_nothing_behind`, `A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`, `Every_file_but_the_executable_is_exactly_as_published`, `Included_files_reach_the_installable_payload_and_report`, `Included_folder_below_a_link_is_allowed`, `Included_links_are_refused_at_the_root_or_inside`, `Included_missing_folder_is_refused`, `Included_output_ancestor_is_refused_before_staging`, `Included_protected_files_are_refused_without_output` (11 rows), `The_brands_files_land_where_the_IDE_will_look_for_them`, `The_icon_is_the_brands_with_nothing_left_of_stocks`, `The_installer_inputs_are_written_beside_the_build_and_not_inside_it`.
- `Server.ProcessWindowsToolTests` (4): `A_minimized_program_is_read_as_minimized_and_visible`, `A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`, `A_running_process_with_no_window_is_said_to_have_none_yet`, `A_window_of_a_process_the_started_one_started_is_read_through_it`.
- `Sidecar.UserSidecarSaveTests` (1): `TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`.
- `Tools.ProjectTreeBuilderTests` (3): `AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`, `CrossDriveMember_RendersAtRoot`, `ForwardSlashPath_NormalizesAndNests`.
- `UserDataPathTests` (1): `OnWindowsItIsUnderAppData`.

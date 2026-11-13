# Initialize handshake early-return audit (#1937)

Read on 2026-11-13 at upstream/main c0451457, before this fix:

- `ConnectAsync` may return null (no handshake), or throw/cancel before JSON-RPC is bound.
  Null records the transport's failure; exceptions propagate. Neither logs the unanswered-handshake warning.
- JSON-RPC setup or `StartListening` can throw outside the initialize catch; `StartAsync` then faults
  rather than returning to the warning assertion.
- A concurrent stop can remove `_rpc` before `InitializeAsync` starts; its null guard returns.
  After initialize starts, `StopAsync` disposes RPC without cancelling the caller token: the pending
  request can then fail through the generic exception catch and be incorrectly reported as a fault.
- Caller cancellation during initialize returns through the cancellation catch, logging Debug deliberately.
  The mute-server warning test passes the test-run cancellation token, with no independent cancellation source
  and no `StopAsync` before its assertion. Ordinary fixture teardown happens only after the test returns.
- `InvokeRequestAsync` can throw synchronously while preparing/sending the request, or complete early
  through an error reply, failed write, closed stream, or cancellation not requested by the caller.
  These reach the generic initialize catch: Error only, without tearing down the connection.
- A timeout reaches `AbandonUnansweredHandshake`: Warning and teardown, with reconnect where supported.
  Stdio alone can first restart once when its tap proves a flushed request and complete silence.
- If timeout and EOF race, whichever completes the request wait first determines the catch: timeout
  warns about unanswered initialize; EOF reaches the generic catch (and the disconnect callback can
  separately warn about connection loss). A timeout's best-effort cancellation can also throw and
  replace the timeout; that too reaches the generic catch.
- `initialized` notification failure shares the generic catch. Successful initialization and document
  replay cannot occur against this mute server, which never completes its handler.

The deterministic regression sends initialize to a server which either sends an error reply or closes
its stream after receiving that request. It checks receipt, a giving-up Warning with the reason, and
that the client remains uninitialized and the server sees a disconnect. This identifies a reachable gap, not the historical Windows
6 ms failure's proven cause.

## Repetition procedure

For each run, a temporary `[Fact]` in `HexIDE.Tests` invoked
`GivingUpIsReportedAtWarning_BecauseSilenceIsTheSymptom` 500 times, each inside a fresh
`await using var fixture = new SilentServerTests()`. Exceptions were collected with their iteration
numbers and full messages; the final assertion required that collection to be empty. The original
assertion and 300 ms timeout were unchanged. The temporary fact was removed afterwards.

Both runs used the pre-fix product assembly. The run without added CPU load used
`dotnet test HexIDE.Tests/ -p:BuildProjectReferences=false -- --filter-class '*Issue1937LoopProbe'`
from `IDE/`. The loaded run used `--no-build --no-restore` and the same filter. Python's
`os.sched_setaffinity` restricted that run and two child `while True: pass` CPU workers to the same
two CPUs; the workers were terminated and waited for in `finally`. Windows PATH entries were removed
before all test runs. The loop exercises only the in-memory mute server.

- Without load: **0 failures / 500 repetitions**, 2m 33s (2026-11-13, WSL/Linux).
- With CPU load: **0 failures / 500 repetitions**, 2m 39s (same baseline assembly).
- Deterministic error-reply and stream-closure cases: **2 failed / 2 before the fix** because neither
  logged the giving-up Warning. The stream-closure case returned in 13 ms. These exercise the generic
  exception catch; they do not reproduce the original mute server failing spontaneously.
- Direct `StopAsync` during a received initialize was also reported as a failure (Error on pristine
  HEAD, Warning before the shutdown catch filter was applied), rather than ordinary shutdown. The catch now recognizes both
  `_stopping` and caller cancellation, records Cancelled, and logs Debug. Together with the two early
  failure cases, **3 regression cases demonstrated teeth**. A combined run of the final nine-test
  class against pristine HEAD produced exactly **3 failures, 6 passes, 0 skips** (6.870 seconds).

The warning test now gives `StartAsync` a fresh, uncancelled token rather than the runner's token;
its outer `WaitAsync` still honors runner cancellation. Thus the strict timeout-warning assertion
cannot be reached through ordinary caller cancellation. This isolates the intended path without
changing the assertion or the 300 ms deadline; it does not establish that runner cancellation caused
the historical failure.

## Final verification (2026-11-13, WSL/Linux)

- Final `SilentServerTests`: **9 passed, 0 skipped, 0 failed**.
- `cd IDE && HEXIDE_FOREIGN_LSP_DOWNLOAD=0 dotnet test HexIDE.Tests/` in full, with an XML report and
  a 45-minute process limit: **7,912 passed, 66 skipped, 0 failed**, 28m 42s. The XML guard confirmed
  **7,978 tests and no errors**. Windows PATH entries were removed first.
- Desktop Debug build: **0 warnings, 0 errors**.
- Tree hygiene and `git diff --check`: passed. Hygiene has nine existing advisory warnings.
- Strict OpenSpec validation reports 67 long-requirement warnings, all on requirements unchanged
  from HEAD; none concerns this change.

The 66 skipped cases are **53 Windows-specific, 3 macOS-specific, and 10 requiring an unavailable
VB6 Template tree**. None is coverage for this fix: all nine focused cases ran on Linux.
The skipped methods below include case counts where a theory contributes several skips.

| Skipped method | Cases |
|---|---:|
| `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception` | 1 |
| `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves` | 1 |
| `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess` | 1 |
| `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches` | 1 |
| `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath` | 1 |
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
| `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output` | 11 |
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

# Foreign-server SIGPIPE investigation (#449)

Investigated 2026-11-13 on `aa93a7fd` (`upstream/main`), Ubuntu 26.04.1
under WSL, .NET SDK 10.0.115. No product change is proposed. The historical
cause is not established; a passing stress run cannot prove it has gone.

## Stream ownership review

Read `StdioProcessLspTransport`, `ChannelTapStream`, `VBLspClient`,
`LaunchedServerGuard`, `HexIDE.LspGuard/Program.cs` and `ChildProcessJob`.

| Point | Can it stop reading or close a pipe before initialize answers? |
| --- | --- |
| Process startup | stdin/stdout/stderr are redirected at `Process.Start`. `BeginErrorReadLine` starts after job assignment, before returning a handler. On Linux job assignment is a no-op. Until the reader starts, the stderr pipe remains open: a large write can block, but lack of draining alone does not imply SIGPIPE. |
| stderr capture | The asynchronous line reader logs and raises notices. There is no `CancelErrorRead` or second stderr consumer in this transport. It remains attached through process teardown. A blocking/throwing observer could interrupt progress; no such failure was observed. |
| stdout | `HeaderDelimitedMessageHandler` owns the tapped stdout/stdin streams; `rpc.StartListening()` starts protocol reads before sending initialize. Framing/read/JSON-RPC failures can stop the reader and dispose the handler. The capture is needed to distinguish that from silence. |
| initialize timeout | Both `AbandonUnansweredHandshake` and the existing silent-stdio restart path call `DisposeRpc`. Disposal closes the tapped streams, including stdout, even while the child is alive. A child writing afterwards can receive SIGPIPE. This explains a possible *secondary* exit after a timeout, not the preceding silence. |
| cancellation | Initialize's caller-cancellation catch records cancellation and returns. It does not itself dispose the streams. `StopAsync` subsequently can. |
| ordinary stop | `StopAsync` attempts bounded shutdown/exit only after initialize was answered. It then disposes JSON-RPC and the transport. A stop during initialize skips that protocol exchange and closes stdout. |
| process-exit callback | Reports the exit and raises `Closed`. `VBLspClient.OnTransportClosed` clears state; it does not dispose the streams. An exit already happened at this point. |
| transport disposal | Waits for exit, kills after its grace period if necessary, waits again, then disposes the `Process` (releasing stderr too). Launch-failure cleanup also kills/waits/disposes, but no handler was returned and initialize cannot yet have been sent. |
| reconnect | The general reconnect loop disposes the old RPC, but stdio has `CanReconnect == false`; only the explicit silent-stdio restart takes this path for a stdio child. |
| Unix process guard | The child inherits the redirected descriptors; the guard does not relay or read them. It ends the process group after parent loss, SIGTERM or child exit. Windows jobs do nothing on Linux. |
| neighbouring tests | Each fixture constructs its own registry, client, transport and process. No shared stdout/stderr owner was found that lets a preceding fixture dispose this one's descriptors. This is a code review finding, not a reproduction. |

## History since the last sighting

`git log -p --since=2026-10-12 -- IDE/HexIDE.Lsp/Transports/StdioProcessLspTransport.cs`
was reviewed, together with the relevant client and tap changes.

- `BeginErrorReadLine` is present in the initial transport commit `94be7628`
  (2026-07-19). Its ordering relative to initialize has not changed.
- Stderr notices were added in `a5b87664` (#369/#402, 2026-09-13), before the
  last sighting. The brief attributes stderr capture to #1241; this tree's
  history attributes it to that earlier change. No later stderr-drain fix was
  found that establishes the cause is gone.
- `3fe85c6f` (#804, 2026-10-12) added the byte-channel observations. A serialized
  `Sent initialize` entry alone does not prove delivery to the child.
- `fde6fe27` (#817, 2026-10-12) updated rumdl from 0.2.64 to 0.2.79, which is still
  pinned today. The brief does not identify whether that day's failure preceded
  the bump. No claim about a rumdl fix follows from the version change.
- `8325b247` (#1914/#1919, 2026-10-25) already added `ForeignServerWait` and its
  five tests: a captured non-zero exit names the connection, code and outstanding
  request rather than waiting for diagnostics.
- Subsequent changes add structured server identities, bounded process reaping,
  Windows jobs, the Unix process-group guard, exit/assignment ordering, and
  disposal/exit-lock fixes. None removes the initialize-time stdout close above.
- `40e63e45` (#808/#3473, 2026-11-09) already introduced one restart after a silent
  initialize. This investigation does not add or rely on a retry as a fix.
- `a2f83881` (#3553, 2026-11-10) starts initialize's server-response deadline after
  the first write flush and first read are issued, with a separate bounded wait
  for delivery. `SilentStdioRestartTests.A_request_still_on_its_way_to_the_server_does_not_use_up_the_servers_time`
  covers that change. It addresses a plausible pre-delivery timeout, but the old
  record is insufficient to identify it as #449's cause.

## Probes

`python3 scripts/probes/449-pipes.py` measures the difference between blocked and
closed stderr, using the platform shell and a 32 MiB burst from `head`:

```text
Undrained stderr: still alive after 1 s
After draining: 33554432 bytes, exit 0
Closed stderr read end: shell exit 141
```

This is a pipe-mechanics probe, not a reproduction in rumdl or HexIDE. A first
1 MiB attempt fitted in this host's pipe; 32 MiB was used to exceed its capacity.

Build the test project, then run `python3 scripts/probes/449-stress.py`. It runs
the two exact tests named in the brief in each iteration. Each runner is fresh;
the tests still perform their own normal fixture teardown. Two Python busy loops
and the runners share two CPUs through affinity, leaving other CPUs available to
other WSL work. Windows PATH entries are removed. Foreign-server acquisition is
disabled and `HEXIDE_REQUIRE_FOREIGN_LSP=1` is set.

A shell wrapper starts the existing pinned rumdl with inherited stdio, records
its exit code in a separate file, then exits with that code. It does not forward
bytes, retry or change the server. This adds a process and scheduling overhead;
the unwrapped focused run is a separate check. Each XML report must contain two
tests with no skips. A successful run also requires exactly two zero server exits
per iteration: an extra launch or non-zero exit cannot hide behind a passing
test. Logs/XML/exit counts are written under disposable `artifacts/449-stress`;
the findings are recorded here so they survive its deletion.

Measured stress result: **200 iterations, 400 tests passed, 0 failed, 0 skipped,
0 xunit errors**, in **1,950.6 s (32m 30.6s)**, on CPUs 0 and 1. All 200 runner
exit codes were zero. The wrapper recorded **394 server exits, all zero**;
**six of the expected 400 exits were not captured**. No initialize failure or
exit 141 was observed. This is a non-reproduction with incomplete exit accounting,
not evidence that every server exited cleanly or that #449 is fixed.

After iteration 45, launch/exit tracing was added by atomically replacing the
wrapper (existing shells kept their old script). Of the subsequent 305 traced
launches, 302 had matching zero exits and three had no exit record. The earlier
untraced portion accounts for the other three missing exits. A forcibly ended
wrapper cannot record its child's status; the observations do not distinguish
that from another loss of the wrapper's exit record. No cause is assigned to
these missing records.

The final script rejects a non-zero server exit or an exit-count mismatch even
when all test runners pass. That final accounting check was added while this
first run was already active: its original zero script exit is not a passing
accounting check. The 394/400 result fails that check on manual inspection.

## Validation

- Full required run: `cd IDE && HEXIDE_FOREIGN_LSP_DOWNLOAD=0 dotnet test HexIDE.Tests/`
  after removing Windows PATH entries: **7,909 passed, 0 failed, 66 skipped**,
  7,975 total, 26m 09.776s. No foreign-server tests skipped. The skips were
  53 Windows-specific cases, three macOS-specific cases and ten needing the
  absent VB6 Template tree; exact names are below. Skips are not coverage.

- Unwrapped focused run: **59 passed, 0 failed, 0 skipped**, 194.736 s. The built
  xunit executable was run with `HEXIDE_REQUIRE_FOREIGN_LSP=1` and
  `HEXIDE_FOREIGN_LSP_DOWNLOAD=0`, selecting `ForeignServerIntegrationTests`,
  `UntitledDocumentNamesTests`, `StdioProcess*`, `ChannelTapStreamTests`,
  `SilentStdioRestartTests` and `ForeignServerWaitTests` via separate `-class`
  options (the stdio filter is `HexIDE.Tests.LspClient.StdioProcess*`).
- Desktop Debug build: **0 warnings, 0 errors**.
- `bash scripts/check-tree-hygiene.sh`: passed, nine existing non-blocking
  warnings; its optional identity scan was skipped because no pattern was set.
- No test or product behaviour changed. **New before/after teeth: 0.** The
  existing five `ForeignServerWaitTests` pass; this is not a new regression claim.

## Remaining

Do not close #449 on a non-reproduction alone. The historical record lacks byte
delivery/read timing and the disconnect reason before the pipes closed. No
deterministic regression against a newly identified cause was obtained, so there
is no new before/after teeth claim. No VB6 semantics, language inventory counts,
UI behaviour or generated LSP coverage table changes are involved. No changelog
fragment is added: the latest issue instruction requires one only for product
code changes.

## Full-suite skipped cases

### Addins.PackageVerificationTests

- `Locked_signature_file_yields_Untrusted_not_an_exception`

### IDE.EditorServiceNavigationTests

- `AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`

### Infrastructure.WindowStateUnconfirmedTests

- `On_Windows_the_probe_does_not_guess`

### LspClient.LspDocumentSessionTests

- `AWindowsDriveLetterInTheOtherCaseStillMatches`

### LspClient.TypeLibraryNamesTests

- `ALocalLinkToAShareIsNotALocalPath`

### LspClient.WorkspaceArtifactProjectTests

- `ALocalTypeLibraryIsNamedFromItsFile`
- `TheTypeLibraryNamesOleAutomationStdole`

### Processes.ChildProcessJobTests

- `Closing_the_job_ends_the_process_put_in_it`
- `A_closed_job_refuses_a_process`
- `A_process_left_out_of_the_job_outlives_it`

### Profiles.ProfileContainmentTests

- `JunctionSwappedInAfterPreflightIsRefusedDuringRead`
- `JunctionEscapeIsRefused`

### Profiles.ProfileReaderSafetyTests

- `WindowsPipeHandleIsRefused`

### Projects.ClassModuleSaveFidelityTests

- `Saving_a_project_leaves_an_untouched_class_byte_identical`
- `A_reload_from_disk_also_preserves_the_header`
- `Editing_the_body_leaves_the_header_untouched`

### Projects.CompanionBinaryPreservationTests

- `AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`
- `A_second_save_still_leaves_the_companion_intact`
- `A_form_that_reproduces_its_companion_is_never_denied_the_write`
- `Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`
- `The_form_records_that_it_lost_binary_fidelity`
- `A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`
- `Dropping_a_blob_bearing_control_keeps_both_halves_in_step`

### Projects.ContributedProjectTests

- `A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`

### Projects.DuplicateProjectOpenTests

- `Opening_the_same_project_with_different_drive_and_folder_case_is_refused`

### Projects.MadeProgramTests

- `A_made_program_starts_and_runs_its_Sub_Main`

### Repack.LinuxRepackerTests

- `A_linux_build_is_refused_on_windows`

### Repack.MacRepackerTests

- `macOS_accepts_the_property_list_signature_and_icon`
- `A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`
- `A_mac_build_is_refused_on_windows`
- `A_mac_build_is_repacked_as_one`

### Repack.WindowsRepackerTests

- `Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`
- `Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`
- `Included_protected_files_are_refused_without_output(relative: "LICENSE")`
- `Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`
- `Included_protected_files_are_refused_without_output(relative: "revocations.json")`
- `Included_protected_files_are_refused_without_output(relative: "brand.json")`
- `Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`
- `Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`
- `Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`
- `Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`
- `Included_protected_files_are_refused_without_output(relative: "revocations.sig")`
- `The_installer_inputs_are_written_beside_the_build_and_not_inside_it`
- `A_default_profile_folder_travels_with_the_brand`
- `A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`
- `The_brands_files_land_where_the_IDE_will_look_for_them`
- `Included_links_are_refused_at_the_root_or_inside`
- `A_branded_process_declares_that_identity_to_Windows`
- `Every_file_but_the_executable_is_exactly_as_published`
- `Included_files_reach_the_installable_payload_and_report`
- `A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`
- `A_run_that_fails_partway_leaves_nothing_behind`
- `The_icon_is_the_brands_with_nothing_left_of_stocks`
- `Included_folder_below_a_link_is_allowed`
- `Included_missing_folder_is_refused`
- `Included_output_ancestor_is_refused_before_staging`
- `A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`

### Server.ProcessWindowsToolTests

- `A_window_of_a_process_the_started_one_started_is_read_through_it`
- `A_running_process_with_no_window_is_said_to_have_none_yet`
- `A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`
- `A_minimized_program_is_read_as_minimized_and_visible`

### Sidecar.UserSidecarSaveTests

- `TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`

### Tools.ProjectTreeBuilderTests

- `ForwardSlashPath_NormalizesAndNests`
- `CrossDriveMember_RendersAtRoot`
- `AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`

### UserDataPathTests

- `OnWindowsItIsUnderAppData`


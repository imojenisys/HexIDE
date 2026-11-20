# Remaining LspClient teardown faults (#4833)

Checked under WSL on 2026-11-20, starting at `589aaf4db`. The latest comment on
[issue #4833](https://github.com/hexide-io/HexIDE/issues/4833) defines this run's scope.
No Windows run was available.

## Full checks

The full HexIDE.Tests run without the probe completed: **10,475 passed,
85 skipped, zero failed**, 10,560 total, in 41m 03s. The xUnit report check
also confirmed zero runner errors. Skips were 59 Windows-only cases, 4 macOS-only
cases, 10 VB6-template cases, 1 unavailable DAP-schema case, 7 opt-in probe
cases, 3 unavailable foreign-adapter cases (netcoredbg/debugpy), and 1 opt-in
wire-stress case. None is claimed as coverage. The six teardown theory rows
were run separately with the probe enabled, as recorded below.

Debug desktop build: zero warnings and zero errors. Tree hygiene passed with
9 existing warnings. No bundled-server or IDE-shell source changed; language
coverage counts and public add-in API are unchanged.

## Before the fixes

Each class below ran alone with `HEXIDE_UNOBSERVED_PROBE` set to a separate file,
and with `LspSessionLeakCheck` included to collect the last fixture's faults.
The passed counts include that sentinel.

| Class | Passed | Fault records |
|---|---:|---:|
| RequestFailureLoggingTests | 16 | 0 |
| SilentServerTests | 10 | 0 |
| WebSocketLspTransportTests | 10 | 0 |
| TraceOnTheWireTests | 24 | 3 |
| CancellationWireShapeTests | 9 | 0 |
| StdioProcessLspTransportTests | 14 | 1 |

The opt-in teardown theory, with the new class rows temporarily including both
row-7 candidates, gave three failures: WebSocketLspTransportTests,
TraceOnTheWireTests and StdioProcessLspTransportTests. The WebSocket child
recorded `ObjectDisposedException` from `HttpListener.EndGetContext`, confirming
the listener attribution despite the earlier isolated run recording zero.
The cancellation guard already passed before the change; it is a regression
guard, not a demonstrated failing test.

## After the fixes

All six retained teardown theory rows passed with empty child probe files:
ShutdownWireShapeTests, RenameWireShapeTests, CancellationWireShapeTests,
TraceOnTheWireTests, StdioProcessLspTransportTests and WebSocketLspTransportTests.
The separate foreign-adapter guard skipped because netcoredbg was unavailable.

The complete LspClient namespace probe finished in 12m 18s: 1,663 passed,
6 skipped, zero failed, and **2 fault records** (the two below). The brief's
measured baseline for this namespace was 9 records at the same main commit.
The skipped cases were the opt-in wire stress test and five Windows-only
type-library/path cases; they provide no coverage in this run.

- `TypeLibraryServicePathTests.GuidOnlyReferenceLoadsWindowsOleAutomationLibrary`
- `LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`
- `WireStressTests.ConcurrentConnectionsKeepEveryFrameValid`
- `TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`
- `WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`
- `WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`

## Row 7 remains unattributed

A second isolated run of RequestFailureLoggingTests (16 passed) and
SilentServerTests (10 passed) also recorded zero faults. Neither class was
changed or added to the guard, following the brief's explicit instruction for
this outcome.

The namespace probe nevertheless recorded the same stand-in read-loop shape
after `SaveOnTheWireTests.NothingIsSentBeforeTheHandshakeCompletes`:

```text
System.InvalidOperationException: Reading is not allowed after reader was completed.
  at System.IO.Pipelines.PipeReaderStream.ReadAsyncInternal
  at HexIDE.Tests.LspClient.WireDiagnosis.ReadTap.ReadAsync (WireDiagnosis.cs:85)
  at System.IO.Pipelines.StreamPipeReader.ReadInternalAsync
  at StreamJsonRpc.HeaderDelimitedMessageHandler.ReadHeadersAsync
  at StreamJsonRpc.HeaderDelimitedMessageHandler.ReadCoreAsync
  at StreamJsonRpc.MessageHandlerBase.ReadAsync
  at StreamJsonRpc.JsonRpc.ReadAndHandleRequestsAsync
```

Attribution can land late; SaveOnTheWireTests does not construct a ReadTap.
This is evidence of a remaining fault, not evidence that this named test owns it.

## A stackless connection-loss record

The namespace probe also recorded a `StreamJsonRpc.ConnectionLostException`
without a stack after
`SignatureHelpShapeTests.OffsetsOutsideTheSignatureDoNotCrashThePopup`.
SignatureHelpShapeTests then passed alone (7 tests including the sentinel),
with zero records. CancellationWireShapeTests also passed its isolated guard.
There is not enough evidence to assign this record to the cancellation tests'
diagnostic pull, another product task, or another test's task. No product
exception handling was changed for it.

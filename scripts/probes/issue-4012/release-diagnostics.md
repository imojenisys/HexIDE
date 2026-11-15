# Release diagnostics for #4012

Measured 2026-11-15 with SDK 11.0.100; the same 92 diagnostics also occur on
`upstream/main` (`612b62c4`) with SDK 10.0.115. Duplicate summary emissions are excluded.
No suppression or test-source fix was made.

| Source | Line:column | ID | Message |
|---|---|---|---|
| IDE/HexIDE.Integration.Tests/Automation/ActiveDocumentDumpTests.cs | 9:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/ActiveDocumentDumpTests.cs | 21:32 | CS0103 | The name 'HexIdeTools' does not exist in the current context |
| IDE/HexIDE.Integration.Tests/Automation/AutomationStatusSectionTests.cs | 35:14 | CS0246 | The type or namespace name 'AutomationServerStatus' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/AutomationStatusSectionTests.cs | 35:45 | CS0246 | The type or namespace name 'AutomationStatusSection' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/AutomationStatusSectionTests.cs | 48:33 | CS0246 | The type or namespace name 'AutomationStatusSection' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/EmptyDumpNoteTests.cs | 4:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/HoverStayTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/NativePickerFeedbackTests.cs | 11:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/ProgramPrintedStartTests.cs | 7:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/RunningCodeEditTests.cs | 10:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/RunningCodeEditTests.cs | 35:49 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/TipPlacementTests.cs | 7:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/WindowChangeTests.cs | 16:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Automation/WindowStateOtherWindowTests.cs | 5:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Debugging/CallStackReplyTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Debugging/CallStackReplyTests.cs | 8:7 | CS0246 | The type or namespace name 'ModelContextProtocol' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Debugging/CallStackReplyTests.cs | 14:49 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Debugging/CallStackReplyTests.cs | 24:20 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Views/CodeEditorSplitTests.cs | 589:39 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Integration.Tests/Views/WindowStateUnitsTests.cs | 5:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Runtime.Integration.Tests/Views/StatusBarDrawingAndEventsTests.cs | 109:22 | CS0117 | 'LockKeys' does not contain a definition for 'SetOverride' |
| IDE/HexIDE.Runtime.Integration.Tests/Views/StatusBarDrawingAndEventsTests.cs | 111:22 | CS0117 | 'LockKeys' does not contain a definition for 'SetOverride' |
| IDE/HexIDE.Runtime.Integration.Tests/Views/StatusBarDrawingAndEventsTests.cs | 114:22 | CS0117 | 'LockKeys' does not contain a definition for 'SetOverride' |
| IDE/HexIDE.Runtime.Integration.Tests/Views/StatusBarDrawingAndEventsTests.cs | 119:22 | CS0117 | 'LockKeys' does not contain a definition for 'SetOverride' |
| IDE/HexIDE.Tests/Infrastructure/AnswerNextFileDialogNoteTests.cs | 1:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/AnswerNextFileDialogNoteTests.cs | 15:22 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/AnswerNextFileDialogPathsTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/AnswerNextFileDialogPathsTests.cs | 15:22 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ArchiveFileToolsTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ArchiveFileToolsTests.cs | 21:22 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ArchiveFileToolsTests.cs | 22:22 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/BreakpointConditionToolTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/BreakpointConditionToolTests.cs | 18:25 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/BreakpointConditionToolTests.cs | 19:25 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/BreakpointConditionToolTests.cs | 37:31 | CS0246 | The type or namespace name 'MutateResult' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ConversationExportSafetyTests.cs | 4:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/DocumentTabsModifiedTests.cs | 5:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/DocumentTabsModifiedTests.cs | 28:22 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/DocumentTabsModifiedTests.cs | 29:22 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/DumpFitsReplyTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/DumpFlagsNoteTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ExternalRunControlTests.cs | 5:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/FormControlsSummaryTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/FormatSelectionTests.cs | 4:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/GroupDiagnosticProjectTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HeldButtonNoteTests.cs | 5:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HoverQuickInfoNoteTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HoverQuickInfoNoteTests.cs | 5:29 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HoverQuickInfoNoteTests.cs | 22:20 | CS0246 | The type or namespace name 'Asked' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HoverQuickInfoNoteTests.cs | 25:29 | CS0246 | The type or namespace name 'Registered' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HoverQuickInfoNoteTests.cs | 27:78 | CS0246 | The type or namespace name 'Asked' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/HoverQuickInfoNoteTests.cs | 28:23 | CS0246 | The type or namespace name 'Registered' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ImplicitDocumentNamingTests.cs | 6:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ImplicitDocumentNamingTests.cs | 41:25 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ImplicitDocumentNamingTests.cs | 42:25 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ImplicitDocumentNamingTests.cs | 67:31 | CS0246 | The type or namespace name 'MutateResult' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ImplicitDocumentNamingTests.cs | 67:53 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/InlineSnapshotTests.cs | 6:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/LspRowsNoteTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/LspRowsNoteTests.cs | 18:46 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/LspRowsNoteTests.cs | 18:66 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/MatchSearchScopeTests.cs | 6:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/McpRestartTests.cs | 12:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/MenuCommandRefusalTests.cs | 4:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/NativePickerDescriptionTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/NoRunEngineReplyTests.cs | 4:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/NoRunEngineReplyTests.cs | 21:13 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/NoRunEngineReplyTests.cs | 26:13 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/NoRunEngineReplyTests.cs | 66:24 | CS0246 | The type or namespace name 'MutateResult' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/NoRunEngineReplyTests.cs | 66:44 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/OpenDialogNoteTests.cs | 10:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ProjectWriteSafetyTests.cs | 1:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SessionGuardTests.cs | 6:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SetIdeLanguageSavesTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SetIdeLanguageSavesTests.cs | 21:26 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SetIdeLanguageSavesTests.cs | 22:16 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SetLockKeyTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SetLockKeyTests.cs | 15:22 | CS0246 | The type or namespace name 'IdeContext' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SetLockKeyTests.cs | 20:22 | CS0246 | The type or namespace name 'HexIdeTools' could not be found (are you missing a using directive or an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/ShutdownIdeTests.cs | 4:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SnapshotLeftOutNoteTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SnapshotWindowMetadataTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/SplitDocumentTabsTests.cs | 5:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/WindowStateUnconfirmedTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Infrastructure/WorkingCopyReplyTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/LspClient/ClosedDiagnosticsCacheTests.cs | 1:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Projects/MakeOutcomesTests.cs | 7:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Projects/ProjectFileFormatSeamTests.cs | 6:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Serialization/ReadableJsonEncoderTests.cs | 3:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Server/HoverTipTimingTests.cs | 1:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Server/ProcessWindowsToolTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |
| IDE/HexIDE.Tests/Server/ProgramPrintedTests.cs | 2:22 | CS0234 | The type or namespace name 'Server' does not exist in the namespace 'HexIDE.Desktop' (are you missing an assembly reference?) |

# The single-view program form host

[#4068](https://github.com/hexide-io/HexIDE/issues/4068) separates the single-view window from the runtime event root.
`IProgramHost.CreateForm` returns an `IHostedForm`. `ProgramHost` owns its `MDIWindow`, host commands, routed input,
activation and layout notifications. Only the form's own window crosses the seam, as `object` in Core and as an
Avalonia `Control` in the runtime. The handle's extent is in whole device pixels; requested sizes remain layout units.

`HostedFormRoot` is a runtime-owned `Decorator` inside that window. Its child is the existing form content, including
its menu and canvas. It adds no drawing or background. Optional event paths retain the interface defaults used by the
former MDI root. `VBWindowContext.Window` and `VbObject.FormWindow` still refer
to the window, so chrome writes keep their existing destination. The desktop `VBFormRuntime` path is unchanged.

`FormRoot.View` and `FormRoot.Execution` resolve either a desktop window that implements the interfaces itself or the
runtime root held directly in a hosted window's content. They do not search other windows or expose IDE services.
`InterpreterRunEngine.cs` still references `Static.SingleView`; it names no other IDE-owned type (its contracts are
in Core and its interpreter, controls and standard-library adapters are in Runtime).

## Lookup audit

The following tables record every helper call and retained interface ancestor walk in this change (2026-11-16).
The window-to-root calls cover WithEvents and Unload, lifecycle and resize, activation and modal holds, Enabled,
form methods and SetFocus, font and identity, and the IDE's program/window recognisers:

| Source | Sites |
|---|---|
| `HexIDE.Runtime/AvaloniaInterop/AvaloniaInteroperability.cs` | [556](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaInteroperability.cs#L556) |
| `HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs` | [66](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L66), [116](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L116), [148](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L148), [149](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L149), [152](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L152), [278](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L278) |
| `HexIDE.Runtime/AvaloniaInterop/ControlFont.cs` | [24](../IDE/HexIDE.Runtime/AvaloniaInterop/ControlFont.cs#L24) |
| `HexIDE.Runtime/AvaloniaInterop/IdentityMembers.cs` | [41](../IDE/HexIDE.Runtime/AvaloniaInterop/IdentityMembers.cs#L41) |
| `HexIDE.Runtime/Interpreter/StatementExecutor.cs` | [3753](../IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs#L3753), [4035](../IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs#L4035) |
| `HexIDE.Runtime/VBProgram.cs` | [247](../IDE/HexIDE.Runtime/VBProgram.cs#L247), [341](../IDE/HexIDE.Runtime/VBProgram.cs#L341), [348](../IDE/HexIDE.Runtime/VBProgram.cs#L348), [375](../IDE/HexIDE.Runtime/VBProgram.cs#L375) |
| `HexIDE.Runtime/VBWindowContext.cs` | [333](../IDE/HexIDE.Runtime/VBWindowContext.cs#L333), [350](../IDE/HexIDE.Runtime/VBWindowContext.cs#L350), [500](../IDE/HexIDE.Runtime/VBWindowContext.cs#L500) |
| `HexIDE/Automation/OpenDialogs.cs` | [137](../IDE/HexIDE/Automation/OpenDialogs.cs#L137) |
| `HexIDE/Automation/UiAutomationDriver.cs` | [2598](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2598), [2603](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2603), [2605](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2605), [2606](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2606), [2647](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2647), [2655](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2655), [2668](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L2668) |

These sites remain walks from a control; they find `HostedFormRoot` before reaching the host window:

| Source | Sites |
|---|---|
| `HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs` | [148](../IDE/HexIDE.Runtime/AvaloniaInterop/AvaloniaMethodsInteroperability.cs#L148) |
| `HexIDE.Runtime/AvaloniaInterop/TabStripMembers.cs` | [193](../IDE/HexIDE.Runtime/AvaloniaInterop/TabStripMembers.cs#L193) |
| `HexIDE.Runtime/AvaloniaInterop/TimerMembers.cs` | [45](../IDE/HexIDE.Runtime/AvaloniaInterop/TimerMembers.cs#L45) |
| `HexIDE.Runtime/BuiltinControls/AttachedEvents.cs` | [188](../IDE/HexIDE.Runtime/BuiltinControls/AttachedEvents.cs#L188), [200](../IDE/HexIDE.Runtime/BuiltinControls/AttachedEvents.cs#L200), [211](../IDE/HexIDE.Runtime/BuiltinControls/AttachedEvents.cs#L211), [232](../IDE/HexIDE.Runtime/BuiltinControls/AttachedEvents.cs#L232), [254](../IDE/HexIDE.Runtime/BuiltinControls/AttachedEvents.cs#L254) |
| `HexIDE.Runtime/BuiltinControls/RuntimeExtensions.cs` | [17](../IDE/HexIDE.Runtime/BuiltinControls/RuntimeExtensions.cs#L17), [35](../IDE/HexIDE.Runtime/BuiltinControls/RuntimeExtensions.cs#L35) |
| `HexIDE.Runtime/BuiltinControls/StatusBarPresentation.cs` | [212](../IDE/HexIDE.Runtime/BuiltinControls/StatusBarPresentation.cs#L212) |
| `HexIDE.Runtime/BuiltinControls/TextBoxKeyEvents.cs` | [65](../IDE/HexIDE.Runtime/BuiltinControls/TextBoxKeyEvents.cs#L65) |
| `HexIDE.Runtime/BuiltinControls/VBListView.Editing.cs` | [38](../IDE/HexIDE.Runtime/BuiltinControls/VBListView.Editing.cs#L38), [53](../IDE/HexIDE.Runtime/BuiltinControls/VBListView.Editing.cs#L53) |
| `HexIDE.Runtime/BuiltinControls/VBListView.Mouse.cs` | [91](../IDE/HexIDE.Runtime/BuiltinControls/VBListView.Mouse.cs#L91), [109](../IDE/HexIDE.Runtime/BuiltinControls/VBListView.Mouse.cs#L109) |
| `HexIDE.Runtime/BuiltinControls/VBListView.cs` | [89](../IDE/HexIDE.Runtime/BuiltinControls/VBListView.cs#L89) |
| `HexIDE.Runtime/BuiltinControls/VBMaskEdBox.cs` | [24](../IDE/HexIDE.Runtime/BuiltinControls/VBMaskEdBox.cs#L24) |
| `HexIDE.Runtime/BuiltinControls/VBMonthView.cs` | [230](../IDE/HexIDE.Runtime/BuiltinControls/VBMonthView.cs#L230), [254](../IDE/HexIDE.Runtime/BuiltinControls/VBMonthView.cs#L254), [261](../IDE/HexIDE.Runtime/BuiltinControls/VBMonthView.cs#L261) |
| `HexIDE.Runtime/BuiltinControls/VBRichTextBox.InputDrawing.cs` | [62](../IDE/HexIDE.Runtime/BuiltinControls/VBRichTextBox.InputDrawing.cs#L62), [162](../IDE/HexIDE.Runtime/BuiltinControls/VBRichTextBox.InputDrawing.cs#L162) |
| `HexIDE.Runtime/BuiltinControls/VBSSTab.cs` | [54](../IDE/HexIDE.Runtime/BuiltinControls/VBSSTab.cs#L54), [136](../IDE/HexIDE.Runtime/BuiltinControls/VBSSTab.cs#L136) |
| `HexIDE.Runtime/BuiltinControls/VBTabStrip.cs` | [46](../IDE/HexIDE.Runtime/BuiltinControls/VBTabStrip.cs#L46), [139](../IDE/HexIDE.Runtime/BuiltinControls/VBTabStrip.cs#L139), [168](../IDE/HexIDE.Runtime/BuiltinControls/VBTabStrip.cs#L168), [244](../IDE/HexIDE.Runtime/BuiltinControls/VBTabStrip.cs#L244) |
| `HexIDE.Runtime/BuiltinControls/VBToolbar.cs` | [57](../IDE/HexIDE.Runtime/BuiltinControls/VBToolbar.cs#L57) |
| `HexIDE.Runtime/BuiltinControls/VBTreeView.Events.cs` | [26](../IDE/HexIDE.Runtime/BuiltinControls/VBTreeView.Events.cs#L26) |
| `HexIDE.Runtime/BuiltinControls/VBTreeView.LabelEdit.cs` | [85](../IDE/HexIDE.Runtime/BuiltinControls/VBTreeView.LabelEdit.cs#L85) |
| `HexIDE.Runtime/VBLoader.cs` | [564](../IDE/HexIDE.Runtime/VBLoader.cs#L564), [579](../IDE/HexIDE.Runtime/VBLoader.cs#L579) |
| `HexIDE/Automation/UiAutomationDriver.cs` | [5690](../IDE/HexIDE/Automation/UiAutomationDriver.cs#L5690) |

`FormMembers` and `DebugInspector` contain no interface casts to replace: they keep using the window's property surface,
which reaches the interop sites above. `FormMembers` retains its desktop-specific `VBFormRuntime` checks and today's behaviour.
`VBFormRuntime`, `VBWindowContext` and `VBUserControl` retain their existing runtime root declarations. `ControlEvents`
continues to store and call roots supplied by control binding; it does not resolve a form window.

No language coverage status changes. The `Unload` and `WithEvents` detail notes in `MISSING_LANGUAGE.md` record the
split without moving counts. The new tests preserve existing behaviour; their VB6 references remain the existing
[control binding measurements](vb6-fidelity-oracle/controls-add-and-controls-remove.md),
[form lifecycle measurements](vb6-fidelity-oracle/several-forms-load-show-hide-modal-and-when-the-run-ends.md) and
[Resize measurements](vb6-fidelity-oracle/a-forms-resize-activate-and-deactivate.md).

## Live verification

In the single-view host, show a form with a non-default BackColor, a menu and controls. Check the colour remains visible
through the runtime root, the menu and control positions match, and resizing, minimising and restoring preserve the
form's Resize events. Show a second form modally; pointer and key input on the held form must be blocked, a press must
bring the holder forward, and cancelling QueryUnload must keep the form open. Hiding/showing and Stop must still work.

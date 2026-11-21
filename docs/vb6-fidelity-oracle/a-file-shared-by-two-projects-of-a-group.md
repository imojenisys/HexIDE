## A file shared by two projects of a group (2026-11-21, #2756, IDE, SP6)

**The question.** Two projects of a group can list the same `.bas`, `.cls` or `.frm` file. #2756 asked:
- what VB6's IDE shows;
- whether it holds one copy of the file or two;
- which copy a save writes.

HexIDE needs those answers before it can decide how to handle the case without losing an edit.

**The probe.** A group, `gen2756b.vbg`, of two Standard EXE projects, PX and PY. Each `.vbp` lists its own module and
the shared file:

```
Type=Exe
Module=ModPX; ModPX.bas
Module=Shared; ..\Shared.bas
Startup="Sub Main"
ExeName32="PX.exe"
Name="PX"
```

`Shared.bas`, beside the `.vbg`:

```vb
Attribute VB_Name = "Shared"
Public Function Hello() As String
    Hello = "orig"
End Function
```

The group was opened in VB6's IDE (VB6.EXE 6.0.9782, SP6) through the console agent. Project Explorer nodes were
picked with the keys, and every dialog was recorded and screenshot before it was answered. The steps:
1. View Code on PX's `Shared`, and add a comment line, `'from PX`, at its end.
2. View Code on PY's `Shared`, with PX's window still open.
3. Close the active code window with Ctrl+F4.
4. View Code on PY's `Shared` again, and add `'from PY`.
5. File > Save Project Group.

There were four runs. The first answered every question Yes (Enter), and the second answered No to each "update"
question. Then the No run was repeated with the shared file as a class (`Class=Shared; ..\Shared.cls`) and as a form
(`Form=..\Shared.frm`). The form's nodes were picked by their place in the tree, since a form's node leaves the
Properties window empty until its designer is open.

| Step | What VB6 did, in all four runs |
|---|---|
| Opening the group | `Shared` listed under both projects, and no message |
| 2. PY's copy, with PX's edited window open | "The copy of this file which might have changes is already opened." (OK, Help). After OK, PX's window stays in front showing `'from PX`, and PY's copy isn't opened |
| 3. Closing PX's window | "Shared (PX) is shared by other project(s). Would you like to save it?" (Yes, No, Help). After Yes, "Would you like to update Shared in project(s) which share it with PX?" (Yes, No, Help) |
| 4. PY's copy, after Yes to that update | shows `'from PX` |
| 4. PY's copy, after No to that update | shows the original text without `'from PX`, though the file on disk now has it |
| 5. Save Project Group | one question: "Would you like to update Shared in project(s) which share it with PY?" (Yes, No, Help) |
| Viewing PX's copy after the save | "The copy of this file which might have changes is already opened." again |

| Run | The shared file afterwards |
|---|---|
| `.bas`, Yes throughout | the original, then `'from PX`, then `'from PY` |
| `.bas`, No to each update | the original, then `'from PY`. `'from PX`, written by the save at step 3, is gone |
| `.cls`, No to each update | the same as the `.bas`: `'from PY` only |
| `.frm`, No to each update | the same: `'from PY` only |

On each Save Project Group, the `.vbg` was rewritten. Neither `.vbp` changed.

VB6 also rewrote the other two files' headers:
- **The class** gained `Persistable`, `DataBindingBehavior`, `DataSourceBehavior` and `MTSTransactionMode` lines in its
  `BEGIN` block.
- **The form's** `ClientHeight` went from 1000 to 1005, and its `ClientWidth` from 2000 to 1995. That's consistent with
  rounding to whole pixels at 15 twips a pixel. The rounding is inferred, not measured.

**What it shows:**
- **Each project holds its own copy of a shared file.** After No to the update question, PY's copy kept the original
  text, although PX's had been saved to disk.
- **VB6 won't show the other copy while one has unsaved changes.** It warns, and leaves the open window in front.
- **Saving a shared file asks whether to update the other projects' copies.** Yes copies the saved text into them, and
  No leaves them as they were.
- **Nothing checks the file on disk before a save.** After No, PY's later save overwrote the file PX had saved, without
  a word. So VB6 can lose a saved edit here. That is the case #2756 asked to know about before matching it.
- **A module, a class and a form behave alike** in every message and in the result.

**Incidental, not probed further.** A first run named its projects VA and VB. The IDE showed the project named VB as
"vb4projectVB", in the Project Explorer and the title bar, while `VB.vbp` still said `Name="VB"`. The run was set aside
because of it.

**Not measured:**
- Running one project while the other's copy has an unsaved edit, the fourth question in #2756.
- Saving one project on its own rather than the group.
- A change to a shared form's design, as opposed to its code.
- Closing the group, or VB6, with copies unsaved.

### Both copies changed, and a run of the other project (2026-11-21, #2756, IDE, SP6)

**The questions.** The section above didn't measure two things. First, a run of one project while the other project's
copy differs. Second, Save Project Group when **both** copies have unsaved changes, rather than one.

**The probe.** VB6.EXE 6.00.9782 (SP6) in a clone of the oracle VM, driven on its console. A group, `g.vbg`, listed
`P2.vbp` as its startup project, then `P1.vbp`. Both lived in one folder with:
- `Own1.bas` (P1) and `Own2.bas` (P2), each a `Sub Main` showing `MsgBox "P1 " & SharedValue()` or `"P2 "`;
- `Shared.bas`, listed by both, holding `Public Function SharedValue() As Long` / `SharedValue = 1`.

Edits were Replace Alls in the module's code window, each changing `SharedValue = n` to another number.

**The first steps** behaved as the section above records:
- P1's copy changed to 2;
- P2's copy refused while P1's window was open;
- P1's window closed, the save question answered Yes, and the update question answered **No**.

**A run uses its own project's copy.** P2's copy then opened showing `SharedValue = 1`, while the file held 2. F5 ran
P2, the startup project, and its message box read `P2 1`.

**No to the save question writes nothing.** P2's copy changed to 3 and its window closed. With No to `Shared (P2) is
shared by other project(s). Would you like to save it?`, the file kept 2, and no update question followed.

**Both copies changed.** P1's copy changed from 2 to 4 and its window closed, again with No. File > Save Project Group
then showed, before anything else:

```
Multiple copies of the shared file '<path>\Shared.bas' have been modified.  In the following dialog, select the
copy that should be saved.
```

That message has OK and Help. After OK, the usual `Save changes to the following files?` list came up, with `g.vbg`,
`P1.vbp`, `Shared.bas`, `P2.vbp` and `Shared.bas`, every row selected.
- **Yes with both copies selected** brought the same warning back. Nothing was written.
- **With P2's `Shared.bas` row deselected,** Yes wrote `Shared.bas` holding P1's 4.
- **Then came the update question,** `Would you like to update Shared in project(s) which share it with P1?`. With Yes,
  P2's copy was replaced by the file's. Its code window showed `SharedValue = 4`, and F5 showed `P2 4`. P2's unsaved 3
  was gone.
- **What else was written:** `g.vbg` was rewritten after the update answer. `P1.vbp` and `P2.vbp` were never written.
  Closing VB6 afterwards raised no save question.

So when both copies have changed, VB6 makes the developer pick one, and won't save both. The other copy's unsaved edit
is lost only through the update question's Yes, whose wording doesn't say that copy has unsaved changes.

**What resisted explanation.** Straight after that update, P2's Project Explorer branch read `Module1 (Shared.bas)`,
listed above `Own2`. Opened, the module was `P2 - Shared`, and the branch read `Shared (Shared.bas)` below `Own2`
again. It was seen once and not repeated.

### Not measured

- **No to the update question** after Save Project Group, with the other copy still changed.
- **A run of P1,** the non-startup project.

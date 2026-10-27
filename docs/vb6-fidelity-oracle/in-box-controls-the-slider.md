## In-box controls: the Slider (2026-10-27, #1091)

**The probes.**
- **The software:** VB6 6.00.8176 SP6, with MSCOMCTL.OCX 6.01.9782, on Windows 10 at 96 dpi.
  - The form line is `Object = "{831FDD16-0C5C-11D2-A9FC-0000F8754DA1}#2.0#0"; "MSCOMCTL.OCX"`.
  - The block is `Begin MSComctlLib.Slider`, `_Version` 393216.
- **The form:** VB6's own double-clicked Slider block from `corpus/inbox-controls/pending/defaults/`, moved to 120, 120 and
  widened to 4500.
- **The programs:** each ran in a Sub Main and logged each value with its `TypeName`.
  - Every error was read straight after its statement.
  - Questions that earlier statements could disturb each ran on a fresh instance of the form.
- **The builds:** `/make` as native code and as p-code, both run on the guest's console. They logged the same lines.

### The object model

- **Its members,** from the type library:
  - **The range:** Min, Max, Value, SmallChange, LargeChange.
  - **The ticks:** TickStyle, TickFrequency, GetNumTicks.
  - **The selection:** SelectRange, SelStart, SelLength, ClearSel.
  - **The tip shown while dragging:** Text, TextPosition.
  - **The rest:** Orientation, BorderStyle, Enabled, hWnd, MousePointer, MouseIcon, OLEDropMode, Refresh, OLEDrag.
  - **Hidden:** `_Value`, the default member, and DoClick.
- **Its events:** Change, Scroll, Click, KeyDown, KeyPress, KeyUp, MouseDown, MouseMove, MouseUp, and the OLE drag events.
- **The constants:**
  - Orientation: ccOrientationHorizontal 0, ccOrientationVertical 1.
  - TickStyle: sldBottomRight 0, sldTopLeft 1, sldBoth 2, sldNoTicks 3.
  - TextPosition: sldAboveLeft 0, sldBelowRight 1.
- **`TypeName`** is `Slider`.
- **The default member is Value.** `v = Slider1` read 0, a Long, and `Slider1 = 4` set Value.

### Defaults

| Read | Logged |
|---|---|
| Min, Max, Value | 0, 10, 0, Longs |
| SmallChange, LargeChange | 1, 5 |
| TickStyle, TickFrequency | 0, 1 |
| GetNumTicks | 11 |
| Orientation, TextPosition, BorderStyle, MousePointer, OLEDropMode | 0 |
| SelectRange | False |
| SelStart, SelLength | 0, 0 |
| Text | empty |
| Enabled, TabStop | True |

### Value, Min and Max

| Statement | Logged |
|---|---|
| `Value = 5` | Change |
| `Value = 5` again | **no Change** |
| `Value = 11`, past Max | **10**, no error, **Change** |
| `Value = -1`, below Min | **0**, no error, Change |
| `Value = 2.5`, `Value = 3.5` | 2, 4: **half rounds to even** |
| `Value = "7"` | 7. `Value = "x"` is **13** |
| `Max = 0` with Min 0; `Min = 10` with Max 10 | **380**: **Max must be more than Min**, not equal |
| `Min` above Max, `Max` below Min | 380 |
| `Min = 7` with Value 5 and Max 10 | Min 7, and **Value pulled up to 7, with no Change** |
| `Max = 4` with Value 7 | Max 4, and **Value pulled down to 4, with no Change** |
| `Min = -100`, `Max = 100000` | taken |

### The ticks and the steps

| Assignment | Logged |
|---|---|
| SmallChange, LargeChange, TickFrequency 0 | taken |
| SmallChange, LargeChange, TickFrequency -1 | **380** |
| SmallChange 20, LargeChange 20, TickFrequency 20 | taken, more than the range |
| `GetNumTicks = 3` | **450**: read-only |

**GetNumTicks counts Min, Max, and every multiple of TickFrequency from Min in between.**
- With Min 0 and Max 10, TickFrequency 1, 2, 3, 4, 7, 10 and 11 gave 11, 6, 5, 4, 3, 2 and 2.
- With Min -10 and Max 10, TickFrequency 3 gave 8: -10, -7, -4, -1, 2, 5, 8 and 10.
- Min 5 with Max 10 gave 6. Min 1 with Max 2 gave 2.
- **TickFrequency 0 gave 2:** the ends only.
- Every row fits `Int((Max - Min - 1) / TickFrequency) + 2`, with TickFrequency 0 counting as no ticks between.

### The enumerations

| Property | Taken | 380 |
|---|---|---|
| TickStyle | 0 to 3 | -1, 4, 99 |
| Orientation, TextPosition, BorderStyle, OLEDropMode | 0, 1 | -1, 2 and up |
| MousePointer | 1, 2, 3, 4, 99 | -1 |

**Orientation can be set at run time, and it swaps Height and Width.**
- 4500 × 675 became 675 wide and 4500 high, at the same Left and Top. Setting it back swapped them again.
- 3000 × 400 became 405 wide and 3000 high: the new width rounded to whole pixels.

### The selection

**With SelectRange False, SelStart is Value.**
- Setting Value moved SelStart with it.
- **`SelStart = 3` moved Value to 3, with no Change.**
- `SelStart = 20` is 380.
- `SelLength = 4` is **380**. SelLength stays 0, and ClearSel changes nothing.

**With SelectRange True, the selection is separate from Value.**
- SelStart 2 with SelLength 3, then `Value = 8`: the selection stayed at 2 and 3.
- **A SelStart that would carry the selection past Max cuts SelLength.** `SelStart = 8` with SelLength 3 gave SelLength 2,
  ending at Max 10, with no error.
- **But a SelLength that runs past Max is 380.** `SelLength = 3` from 8 was refused.
- SelLength or SelStart past the range, or negative, is 380.
- **Lowering Max cuts the selection too.**
  - Max 4 over a selection from 2 to 5 gave SelLength 2.
  - **Max 1, below SelStart 2, left SelStart 2 and SelLength -1.**
- **ClearSel sets SelStart to -1** and SelLength to 0. SelectRange stays True.
- Setting SelectRange False after a selection set SelStart back to Value and SelLength to 0.

### Events from code

- **Change** is raised by a Value that changes, whether assigned or clamped by its own assignment.
  - Not by assigning the same value.
  - Not by a Min or Max that pulls Value in.
  - Not by SelStart moving Value.
- **Scroll is never raised from code.**
- **The hidden DoClick raises Click.**
- Refresh raised nothing.

### In HexIDE

HexIDE doesn't model the Slider (#1091). The designer draws it as an empty box (#157). Running a form that holds one is
refused, as for any control HexIDE can't create (#857), and the refusal names #1091 as the issue that tracks it.

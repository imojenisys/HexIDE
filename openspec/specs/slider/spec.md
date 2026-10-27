# Slider

## Purpose

The MSComctlLib Slider object model, measured against VB6 SP6 in the fidelity oracle (#1091 A).
Input and drawing are deferred to part B, and persisted own properties and faithful file writing to C.

## Requirements

### Requirement: Measured object model

HexIDE SHALL register MSComctlLib.Slider on the in-box control foundation and implement its measured property defaults,
validation, selection, tick count and methods. Value SHALL be its default member for Let reads and writes.

#### Scenario: Value assignment

- **WHEN** code assigns Value outside Min to Max
- **THEN** Value clamps to the nearest endpoint without an error
- **AND** Change fires only if Value changed
- **AND** Scroll does not fire

#### Scenario: Range assignment

- **WHEN** code assigns Min greater than or equal to Max, or Max less than or equal to Min
- **THEN** error 380 leaves the range unchanged
- **WHEN** a valid range assignment passes Value
- **THEN** Value follows silently

#### Scenario: Selection

- **WHEN** SelectRange is False
- **THEN** SelStart reads Value and assigning SelStart moves Value without Change
- **WHEN** SelectRange is True
- **THEN** the selection is independent of Value, SelStart truncates its length at Max, and an excessive SelLength raises 380
- **AND** ClearSel sets SelStart to -1 and SelLength to 0

### Requirement: Incomplete surface

Unmeasured members and conversions SHALL refuse as not implemented. Forms containing Sliders SHALL stay read-only
until their writer is proven faithful. Runtime loading of persisted Slider own properties SHALL refuse until part C.

#### Scenario: Native handle

- **WHEN** code reads Slider.hWnd
- **THEN** HexIDE reports that the member is not implemented

#### Scenario: Saving before part C

- **WHEN** a form contains a Slider
- **THEN** the form cannot be saved and the designer is read-only

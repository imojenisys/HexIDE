### Added

- **UserControls read their saved properties at run time** (part of #4741). Placed controls and loaded
  array elements receive a read-only PropertyBag from their saved `.frm` lines, preserving the measured
  numeric types, quoted strings, first duplicate and defaults. Extender properties stay out of the bag.
  Writes raise 425 and Contents raises 70; unmeasured conversions and persistence shapes explicitly
  refuse, including Font fields the existing realization table cannot read. Both hosts follow VB6's
  F5 behaviour; the made-EXE build-time pass is #5508.

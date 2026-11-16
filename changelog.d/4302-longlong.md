### Added

- **64-bit VBA7 LongLong and LongPtr** (part of #1548, #4302): declarations, conversions, measured arithmetic and Variant overflow, `^` literals, formatting and DefLngLng/DefLngPtr use 64-bit integers. VB6 preserves its measured errors and user-defined names. Unmeasured array bounds, For counters, UDT layouts and further intrinsic/operator uses refuse explicitly.

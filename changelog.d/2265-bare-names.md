### Fixed

- **Common-control class declarations keep their library and class** (Refs #2265). The measured MSComctlLib and ComctlLib bare names resolve through the project's libraries in read order, and typed Set and TypeOf work for their controls, collections and items. A wrong-class Set raises 13 without replacing the earlier object; TypeName uses the measured interface names. Tab remains qualified only.

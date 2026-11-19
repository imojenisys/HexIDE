### Fixed

- **Controls carried as text take their measured TabIndex at form load** (#4884). Repeated and out-of-range indices are renumbered with the other controls, including nested carried controls. Only the number in each carried TabIndex line changes; the rest of the block is retained.

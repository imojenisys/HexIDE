### Fixed

- **Class default members compare with Strings using their typed or Variant origin** (#174, #211).
  Typed numeric defaults compare numerically; a numeric default read through a Variant compares as text.

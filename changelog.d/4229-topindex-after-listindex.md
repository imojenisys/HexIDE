### Fixed

- **ListBox keeps a TopIndex assigned after code selection** (#4229). Setting ListIndex or Selected and
  then TopIndex in the same run no longer scrolls back to the selection on the next dispatcher turn.
  A selection made in Form_Load still comes into view when the form opens.

### Fixed

- **ListBox Page Up follows VB6 selection and scrolling** (#3986). On a single-select list it first selects the top visible row, then pages up when already there, raising Click before Scroll when the view moves. Selected and Text assignments bring the item into view before Click, without Scroll.

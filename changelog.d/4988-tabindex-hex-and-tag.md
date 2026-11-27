### Fixed

- **The .frm reader accepts short hex and octal TabIndex values and rejects unquoted Tag values**
  (part of #4988). A readable TabIndex participates in tab-order normalization and saves as decimal.
  The reader logs an invalid value for a standalone unquoted Tag and preserves the original line on save.

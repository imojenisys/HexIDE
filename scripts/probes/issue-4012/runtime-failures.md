# Runtime regressions observed in #4012

SDK 11.0.100, runtime 11.0.0, Linux x64, 2026-11-15. Expectations are existing tests,
not new VB6 measurements. Each named class also ran separately on `upstream/main`
(`612b62c4`) with SDK 10.0.115: all passed (22, 138, 79, 5 and 154 tests respectively
for DoubleToCurrencyTests, NumericFormatFunctionsTests, FractionalForTests,
FormatNumericTests and StringToDateTests). No expectation or runtime implementation was changed.

## HexIDE.Runtime.Tests.DoubleToCurrencyTests.Times_a_Decimal_is_a_Decimal

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug[1].Value to be 1.00005M, but found 1.0000500000000001055155962604M.
```

## HexIDE.Runtime.Tests.StringToDateTests.A_String_reads_as_a_Date_as_VB6_reads_it(text: "24.0", vb6: "[24.0] CDate:Date 23/01/1900 (24) | store:Date 23/"···)

Also fails on main: **no**, in the separate class comparison.

```text
System.OverflowException : Not a legal OleAut date.
```

## HexIDE.Runtime.Tests.NumericFormatFunctionsTests.MeasuredRow(expression: "FormatCurrency(0.015)", expected: "£0.02", error: 0)

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug to be equal to {<Long>(0), <String>(£0.02), <String>(String)}, but {<Long>(0), <String>(£0.01), <String>(String)} differs at index 1.
```

## HexIDE.Runtime.Tests.NumericFormatFunctionsTests.MeasuredRow(expression: "FormatPercent(1234.5678, 99)", expected: "123,456.780000000000000000000000000000000000000000"···, error: 0)

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug to be equal to {<Long>(0), <String>(123,456.780000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000%), <String>(String)}, but {<Long>(0), <String>(123,456.780000000003383320000000000000000000000000000000000000000000000000000000000000000000000000000000000%), <String>(String)} differs at index 1.
```

## HexIDE.Runtime.Tests.NumericFormatFunctionsTests.MeasuredRow(expression: "FormatNumber(0.015)", expected: "0.02", error: 0)

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug to be equal to {<Long>(0), <String>(0.02), <String>(String)}, but {<Long>(0), <String>(0.01), <String>(String)} differs at index 1.
```

## HexIDE.Runtime.Tests.NumericFormatFunctionsTests.MeasuredRow(expression: "FormatNumber(1234.5678, 99)", expected: "1,234.56780000000000000000000000000000000000000000"···, error: 0)

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug to be equal to {<Long>(0), <String>(1,234.567800000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000), <String>(String)}, but {<Long>(0), <String>(1,234.567800000000033833000000000000000000000000000000000000000000000000000000000000000000000000000000000), <String>(String)} differs at index 1.
```

## HexIDE.Runtime.Tests.NumericFormatFunctionsTests.MeasuredRow(expression: "FormatCurrency(1234.5678, 99)", expected: "£1,234.5678000000000000000000000000000000000000000"···, error: 0)

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug to be equal to {<Long>(0), <String>(£1,234.567800000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000), <String>(String)}, but {<Long>(0), <String>(£1,234.567800000000033833000000000000000000000000000000000000000000000000000000000000000000000000000000000), <String>(String)} differs at index 1.
```

## HexIDE.Runtime.Tests.FractionalForTests.A_Variant_counter__For_x_0_To_1_Step_CDec_0_1

Also fails on main: **no**, in the separate class comparison.

```text
Expected Printed() to be equal to {"=== var-decstep: For x = 0 To 1 Step CDec(0.1)", "  pass 1: Decimal 0", "  pass 2: Decimal 0.1", "  pass 3: Decimal 0.2", "  pass 4: Decimal 0.3", "  pass 5: Decimal 0.4", "  pass 6: Decimal 0.5", "  pass 7: Decimal 0.6", "  pass 8: Decimal 0.7", "  pass 9: Decimal 0.8", "  pass 10: Decimal 0.9", "  pass 11: Decimal 1", "  after: Decimal 1.1 passes 11 err 0"}, but {"=== var-decstep: For x = 0 To 1 Step CDec(0.1)", "  pass 1: Decimal 0", "  pass 2: Decimal 0.1000000000000000055511151231", "  pass 3: Decimal 0.2000000000000000111022302462", "  pass 4: Decimal 0.3000000000000000166533453693", "  pass 5: Decimal 0.4000000000000000222044604924", "  pass 6: Decimal 0.5000000000000000277555756155", "  pass 7: Decimal 0.6000000000000000333066907386", "  pass 8: Decimal 0.7000000000000000388578058617", "  pass 9: Decimal 0.8000000000000000444089209848", "  pass 10: Decimal 0.9000000000000000499600361079", "  after: Decimal 1.000000000000000055511151231 passes 10 err 0"} contains 1 item(s) less.
```

## HexIDE.Runtime.Tests.FormatNumericTests.Rounding_IsHalfAwayFromZero_NotBankers

Also fails on main: **no**, in the separate class comparison.

```text
Expected debug to be equal to {<String>(1), <String>(2), <String>(3), <String>(2.35), <String>(2.36)}, but {<String>(1), <String>(2), <String>(3), <String>(2.35), <String>(2.35)} differs at index 4.
```


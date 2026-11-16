using System.Globalization;
using System.Threading.Tasks;
using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime.Tests;

// vb6.exe SP6, native and p-code: PR #4017,
// docs/vb6-fidelity-oracle/a-double-or-a-single-converted-to-decimal.md.
public class DecimalConversionTests : BaseVBTestFixture
{
    [Theory]
    [InlineData("CDec(0.1)", "0.1")]
    [InlineData("CDec(1.00005)", "1.00005")]
    [InlineData("CDec(1234567890123445#)", "1234567890123440")]
    [InlineData("CDec(-1234567890123445#)", "-1234567890123440")]
    [InlineData("CDec(1234567890123465#)", "1234567890123460")]
    [InlineData("CDec(CSng(0.1))", "0.1")]
    [InlineData("CDec(CSng(12345665))", "12345660")]
    [InlineData("CDec(CSng(-12345665))", "-12345660")]
    [InlineData("CDec(1) * 0.1", "0.1")]
    public async Task Floating_point_to_Decimal(string expression, string expected)
    {
        await Run($"Debug.Print {expression}\n");
        AssertDebugLog([Vb6Value.NewDecimal(decimal.Parse(expected, CultureInfo.InvariantCulture))]);
    }

    [Theory]
    [InlineData("FormatNumber(1.005, 2)", "1.01")]
    [InlineData("Format$(0.1, \"0.00000000000000000000\")", "0.10000000000000000000")]
    public async Task Formatting_sees_the_rounded_Decimal(string expression, string expected)
    {
        using var locale = VbLocale.Use(CultureInfo.GetCultureInfo("en-GB"));
        await Run($"Debug.Print {expression}\n");
        AssertDebugLog([new Vb6Value(expected)]);
    }

    [Fact]
    public async Task Numeric_date_string_is_a_serial()
    {
        using var locale = VbLocale.Use(CultureInfo.GetCultureInfo("en-GB"));
        await Run("Debug.Print CDbl(CDate(\"24.0\"))\nDebug.Print IsDate(\"24.0\")\n");
        AssertDebugLog([new Vb6Value(24.0), new Vb6Value(false)]);
    }
}

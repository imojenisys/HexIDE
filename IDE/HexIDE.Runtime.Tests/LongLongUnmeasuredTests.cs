using HexIDE.Runtime.Interpreter;
using HexIDE.Runtime.Interpreter.ConditionalCompilation;
using HexIDE.Runtime.ProjectElements;

namespace HexIDE.Runtime.Tests;

// #4302 supplies only the measured intrinsic/operator rows; the remaining VBA7 measurements belong to #1548.
public class LongLongUnmeasuredTests : BaseVBTestFixture
{
    public LongLongUnmeasuredTests() => Compilation = CompilationConstants.ForProject(
        new ProjectDefinition(VBProjectType.EXE, "Test") { VbaBitness = VbaBitness.Bit64 });

    [Theory]
    [InlineData("Debug.Print -CLngLng(\"-9223372036854775808\")")]
    [InlineData("Debug.Print CLngLng(\"-9223372036854775808\") \\ CLngLng(-1)")]
    [InlineData("Debug.Print CLngLng(\"-9223372036854775808\") Mod CLngLng(-1)")]
    [InlineData("Debug.Print 9223372036854775808^")]
    [InlineData("Debug.Print &H10000000000000000^")]
    [InlineData("Debug.Print CLngLng(5) = 5")]
    [InlineData("Debug.Print CLngLng(5) = CLng(5)")]
    [InlineData("Debug.Print CLngLng(5) = CSng(5)")]
    [InlineData("Debug.Print CLngLng(5) = CCur(5)")]
    [InlineData("Debug.Print CLngLng(5) = CDec(5)")]
    [InlineData("Debug.Print CLngLng(5) = True")]
    [InlineData("Debug.Print CLngLng(5) = \"5\"")]
    [InlineData("Debug.Print CLngLng(5) = CLngLng(5)")]
    [InlineData("Debug.Print CLngLng(5) = #1/1/2000#")]
    [InlineData("Select Case CLngLng(5)\nCase 5#\nDebug.Print \"hit\"\nEnd Select")]
    [InlineData("Debug.Print CLngLng(5) And CDbl(3)")]
    [InlineData("Debug.Print CLngLng(5) And \"3\"")]
    [InlineData("Debug.Print CLngLng(5) \\ CDbl(2)")]
    [InlineData("Debug.Print CLngLng(5) \\ CCur(2)")]
    [InlineData("Debug.Print CLngLng(5) Mod CDec(3)")]
    [InlineData("Debug.Print CLngLng(5) * #1/1/2000#")]
    [InlineData("Debug.Print CSng(CLngLng(5))")]
    [InlineData("Debug.Print CCur(CLngLng(5))")]
    [InlineData("Debug.Print CDec(CLngLng(5))")]
    [InlineData("Debug.Print CBool(CLngLng(5))")]
    [InlineData("Debug.Print CDate(CLngLng(5))")]
    [InlineData("Debug.Print CVar(CLngLng(5))")]
    [InlineData("Debug.Print Abs(CLngLng(5))")]
    [InlineData("Debug.Print Int(CLngLng(5))")]
    [InlineData("Debug.Print Fix(CLngLng(5))")]
    [InlineData("Debug.Print Round(CLngLng(5))")]
    [InlineData("Debug.Print IsNumeric(CLngLng(5))")]
    [InlineData("Dim n As LongLong\nn = 5\nDebug.Print Len(n)")]
    [InlineData("v = CLngLng(5)\nDebug.Print LenB(v)")]
    [InlineData("Debug.Print Mid(\"abc\", CLngLng(1))")]
    [InlineData("Debug.Print Format(5, CLngLng(1))")]
    [InlineData("Debug.Print CLngLng(5) Or CLng(3)")]
    [InlineData("Debug.Print CLngLng(5) Xor CLng(3)")]
    [InlineData("Debug.Print CLngLng(5) Eqv CLng(3)")]
    [InlineData("Debug.Print CLngLng(5) Imp CLng(3)")]
    [InlineData("Debug.Print Not CLngLng(5)")]
    public async Task UnmeasuredUseRefuses(string code)
    {
        var act = () => Run(code + "\n");
        await act.Should().ThrowAsync<NotImplementedException>().WithMessage("*#1548*");
    }
}

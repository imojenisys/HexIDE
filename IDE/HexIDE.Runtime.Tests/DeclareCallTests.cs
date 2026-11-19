using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime.Tests;

// Measurements: https://github.com/hexide-io/HexIDE/issues/4820 (including argument evaluation).
public class DeclareCallTests : BaseVBTestFixture
{
    public static bool Windows => OperatingSystem.IsWindows();

    [Theory(Skip = "Off-Windows behavior", SkipWhen = nameof(Windows))]
    [InlineData("kernel32", "Sleep 1")]
    [InlineData("hexide-nosuch", "Sleep 1")]
    [InlineData("kernel32", "Call Sleep(1)")]
    [InlineData("kernel32", "Call Module1.Sleep(1)")]
    public async Task Sub_MissingLibrary_IsTrappable(string library, string call)
    {
        await Run($"""
            Private Declare Sub Sleep Lib "{library}" (ByVal milliseconds As Long)
            On Error Resume Next
            {call}
            Debug.Print Err.Number
            Debug.Print Err.Description
            """);
        AssertDebugLog([new Vb6Value(53L), new Vb6Value("File not found: " + library)]);
    }

    [Theory(Skip = "Off-Windows behavior", SkipWhen = nameof(Windows))]
    [InlineData("NoSuchLib")]
    [InlineData("Module1.NoSuchLib")]
    public async Task Function_EvaluatesArgumentOncePerCall_BeforeTrappableError(string call)
    {
        await Run($"""
            Private Declare Function NoSuchLib Lib "hexide-nosuch.dll" (ByRef n As Long) As Long
            Dim calls As Long
            Function Side() As Long
                calls = calls + 1
                Side = calls
            End Function
            On Error Resume Next
            Dim result As Long
            result = {call}(Side())
            Debug.Print Err.Number
            Debug.Print Err.Description
            Debug.Print calls
            Err.Clear
            result = {call}(Side())
            Debug.Print Err.Number
            Debug.Print Err.Description
            Debug.Print calls
            """);
        AssertDebugLog([new Vb6Value(53L), new Vb6Value("File not found: hexide-nosuch.dll"), new Vb6Value(1L),
            new Vb6Value(53L), new Vb6Value("File not found: hexide-nosuch.dll"), new Vb6Value(2L)]);
    }

    // These refusals are HexIDE policy for unmeasured edges, not VB6 error measurements.
    [Theory(Skip = "Off-Windows behavior", SkipWhen = nameof(Windows))]
    [InlineData("Alias \"NoSuchEntry\" ", "()", "NativeCall", "Declare Alias calls are not implemented (#4820)")]
    [InlineData("", "(ByVal n As Long)", "NativeCall n:=1", "Named arguments on Declare calls are not implemented (#4820)")]
    public async Task UnmeasuredCallForms_Refuse(string alias, string parameters, string call, string message)
    {
        var run = () => Run($"""
            Private Declare Sub NativeCall Lib "hexide-nosuch.dll" {alias}{parameters}
            On Error Resume Next
            {call}
            """);
        await run.Should().ThrowAsync<NotImplementedException>().WithMessage(message);
    }

    [Theory(Skip = "Off-Windows behavior", SkipWhen = nameof(Windows))]
    [InlineData("native.dylib")]
    [InlineData("native.DYLIB")]
    [InlineData("native.so")]
    [InlineData("native.SO.6")]
    public async Task HostLibrary_KeepsRefusal(string library)
    {
        var run = () => Run($"""
            Private Declare Sub NativeCall Lib "{library}" ()
            On Error Resume Next
            NativeCall
            """);
        await run.Should().ThrowAsync<VBCompileErrorException>().WithMessage("*Sub or Function not defined (NativeCall)*");
    }

    [Fact(Skip = "Windows-only refusal", SkipUnless = nameof(Windows))]
    public async Task Windows_KeepsRefusal()
    {
        var run = () => Run("""
            Private Declare Sub Sleep Lib "kernel32" (ByVal milliseconds As Long)
            On Error Resume Next
            Sleep 1
            """);
        await run.Should().ThrowAsync<VBCompileErrorException>().WithMessage("*Sub or Function not defined (Sleep)*");
    }
}

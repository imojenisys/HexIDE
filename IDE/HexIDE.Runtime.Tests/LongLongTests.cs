using HexIDE.Runtime.Interpreter;
using HexIDE.Runtime.Interpreter.ConditionalCompilation;
using HexIDE.Runtime.ProjectElements;

namespace HexIDE.Runtime.Tests;

// Excel x64 measurements copied into #4302 (measuring PR #3965); VB6 refusals: measuring PR #4247.
public class LongLongTests : BaseVBTestFixture
{
    private void Use64BitProject() => Compilation = CompilationConstants.ForProject(
        new ProjectDefinition(VBProjectType.EXE, "Test") { VbaBitness = VbaBitness.Bit64 });

    [Theory]
    [InlineData("V1", "VarType(CLngPtr(5))", "20", "Long")]
    [InlineData("C1", "CLngPtr(2.5)", "2", "LongLong")]
    [InlineData("C2", "CLngLng(2.5)", "2", "LongLong")]
    [InlineData("C3", "CLngLng(3.5)", "4", "LongLong")]
    [InlineData("C4", "CLngLng(-2.5)", "-2", "LongLong")]
    [InlineData("C5", "CLngLng(-3.5)", "-4", "LongLong")]
    [InlineData("C6", "CLngLng(\"9223372036854775807\")", "9223372036854775807", "LongLong")]
    [InlineData("C8", "CLngLng(\"-9223372036854775808\")", "-9223372036854775808", "LongLong")]
    [InlineData("C11", "CLngLng(-9.2233720368547758E+18)", "-9223372036854775808", "LongLong")]
    [InlineData("C12", "CLngLng(\"&H7FFFFFFFFFFFFFFF\")", "9223372036854775807", "LongLong")]
    [InlineData("C13", "CLngLng(\"&HFFFFFFFFFFFFFFFF\")", "-1", "LongLong")]
    [InlineData("C14", "CLngLng(\"&H123456789\")", "4886718345", "LongLong")]
    [InlineData("C16", "CInt(CLngLng(5))", "5", "Integer")]
    [InlineData("C17", "CDbl(CLngLng(\"9007199254740993\"))", "9.00719925474099E+15", "Double")]
    [InlineData("C18", "CStr(CLngLng(\"9007199254740993\"))", "9007199254740993", "String")]
    [InlineData("A1", "CLngLng(5) + CInt(1)", "6", "LongLong")]
    [InlineData("A2", "CLngLng(5) + CLng(1)", "6", "LongLong")]
    [InlineData("A3", "CLngLng(5) + CSng(1)", "6", "Double")]
    [InlineData("A4", "CLngLng(5) + CDbl(1)", "6", "Double")]
    [InlineData("A5", "CLngLng(5) + CCur(1)", "6", "Currency")]
    [InlineData("A6", "CLngLng(5) + CDec(1)", "6", "Decimal")]
    [InlineData("A7", "CLngLng(5) + CByte(1)", "6", "LongLong")]
    [InlineData("A8", "CLngLng(5) + True", "4", "LongLong")]
    [InlineData("A9", "CLngLng(5) + \"1\"", "6", "Double")]
    [InlineData("A11", "CLngLng(5) * CLngLng(5)", "25", "LongLong")]
    [InlineData("A12", "CLngLng(5) / CLngLng(2)", "2.5", "Double")]
    [InlineData("A13", "CLngLng(5) \\ CLngLng(2)", "2", "LongLong")]
    [InlineData("A14", "CLngLng(5) Mod CLngLng(3)", "2", "LongLong")]
    [InlineData("A15", "CLngLng(5) ^ 2", "25", "Double")]
    [InlineData("A16", "-CLngLng(5)", "-5", "LongLong")]
    [InlineData("A17", "CLngLng(5) And CLng(3)", "1", "LongLong")]
    [InlineData("A18", "CLngLng(5) & \"x\"", "5x", "String")]
    [InlineData("A19", "CLngLng(5) = 5#", "True", "Boolean")]
    [InlineData("A20", "CLngLng(\"9007199254740993\") = 9007199254740992#", "True", "Boolean")]
    [InlineData("L1", "3000000000", "3000000000", "Double")]
    [InlineData("L2", "2147483648", "2147483648", "Double")]
    [InlineData("K1", "5^", "5", "LongLong")]
    [InlineData("K1 whitespace", "5^   ", "5", "LongLong")]
    [InlineData("K1 comment", "5^ ' K1", "5", "LongLong")]
    [InlineData("K2", "&HFF^", "255", "LongLong")]
    [InlineData("K3", "&H7FFFFFFFFFFFFFFF^", "9223372036854775807", "LongLong")]
    [InlineData("K4", "&HFFFFFFFFFFFFFFFF^", "-1", "LongLong")]
    [InlineData("K5", "&H8000000000000000^", "-9223372036854775808", "LongLong")]
    [InlineData("K6", "&HFFFFFFFF^", "4294967295", "LongLong")]
    [InlineData("K7", "&HFFFFFFFF", "-1", "Long")]
    [InlineData("H1", "Hex$(CLngLng(-1))", "FFFFFFFFFFFFFFFF", "String")]
    [InlineData("H2", "Oct$(CLngLng(-1))", "1777777777777777777777", "String")]
    [InlineData("H3", "Hex$(CLngLng(\"1099511627776\"))", "10000000000", "String")]
    [InlineData("H4", "Hex$(CLng(-1))", "FFFFFFFF", "String")]
    [InlineData("S1", "Str(CLngLng(5))", " 5", "String")]
    [InlineData("S2", "Format$(CLngLng(\"123456789012\"), \"#,##0\")", "123,456,789,012", "String")]
    [InlineData("S3", "CStr(CLngLng(\"-9223372036854775808\"))", "-9223372036854775808", "String")]
    public async Task MeasuredExpression(string row, string expression, string value, string type)
    {
        Use64BitProject();
        await Run($"' {row}\n v = {expression}\nDebug.Print CStr(v)\nDebug.Print TypeName(v)\n");
        AssertDebugLog([new Vb6Value(value), new Vb6Value(type)]);
    }

    [Theory]
    [InlineData("C7", "CLngLng(\"9223372036854775808\")")]
    [InlineData("C9", "CLngLng(\"-9223372036854775809\")")]
    [InlineData("C10", "CLngLng(9.2233720368547758E+18)")]
    [InlineData("C15", "CLng(CLngLng(3000000000#))")]
    [InlineData("O1", "CLngLng(\"9223372036854775807\") + 1")]
    [InlineData("O2", "CLngLng(\"4611686018427387904\") * 2")]
    public async Task MeasuredOverflow(string row, string expression)
    {
        Use64BitProject();
        await Run($"' {row}\nOn Error Resume Next\nv = {expression}\nDebug.Print Err.Number\n");
        AssertDebugLog([new Vb6Value(6L)]);
    }

    [Theory]
    [InlineData("T1", "Dim x As LongPtr", "", "Debug.Print TypeName(x)\nDebug.Print VarType(x)\nDebug.Print LenB(x)", "LongLong", "20", "8")]
    [InlineData("T2", "Dim x As LongLong", "", "Debug.Print TypeName(x)\nDebug.Print VarType(x)\nDebug.Print LenB(x)", "LongLong", "20", "8")]
    [InlineData("K8", "Dim x^", "x = 5", "Debug.Print TypeName(x)", "LongLong", null, null)]
    [InlineData("A22", "Dim x As LongLong", "x = 2147483647\nx = x + 1", "Debug.Print CStr(x)\nDebug.Print TypeName(x)", "2147483648", "LongLong", null)]
    [InlineData("D1", "DefLngLng A-Z", "x = 5", "Debug.Print TypeName(x)", "LongLong", null, null)]
    [InlineData("D2", "DefLngPtr A-Z", "x = 5", "Debug.Print TypeName(x)", "LongLong", null, null)]
    [InlineData("O3", "Dim x", "x = CLng(2147483647)\nx = x + 1", "Debug.Print CStr(x)\nDebug.Print TypeName(x)", "2147483648", "Double", null)]
    [InlineData("O4", "Dim x", "x = CInt(32767)\nx = x + 1", "Debug.Print CStr(x)\nDebug.Print TypeName(x)", "32768", "Long", null)]
    [InlineData("O5", "Dim x", "x = CLngLng(\"9223372036854775807\")\nx = x + 1", "Debug.Print CStr(x)\nDebug.Print TypeName(x)", "9.22337203685478E+18", "Double", null)]
    public async Task MeasuredVariable(string row, string declaration, string body, string print, string first, string? second, string? third)
    {
        Use64BitProject();
        await Run($"' {row}\n{declaration}\n{body}\n{print}\n");
        var expected = new List<Vb6Value> { new(first) };
        if (second is not null) expected.Add(new Vb6Value(second));
        if (third is not null) expected.Add(new Vb6Value(third));
        // CStr makes the observations independent of the log's boxed numeric representation.
        debug.Select(VbDisplay.ToText).Should().Equal(expected.Select(VbDisplay.ToText));
    }

    [Fact]
    public async Task A21TypedLongStillOverflows()
    {
        Use64BitProject();
        await Run("Dim a As Long\na = 2147483647\nOn Error Resume Next\na = a + 1\nDebug.Print Err.Number\n");
        AssertDebugLog([new Vb6Value(6L)]);
    }

    [Fact]
    public async Task A10DateAddition()
    {
        Use64BitProject();
        await Run("v = CLngLng(5) + #1/1/2000#\nDebug.Print Year(v)\nDebug.Print Month(v)\nDebug.Print Day(v)\nDebug.Print TypeName(v)\n");
        debug.Select(VbDisplay.ToText).Should().Equal("2000", "1", "6", "Date");
    }

    [Theory]
    [InlineData("Dim x As LongLong", "User-defined type not defined*")]
    [InlineData("Dim x As LongPtr", "User-defined type not defined*")]
    [InlineData("Debug.Print CLngLng(5)", "Sub or Function not defined*")]
    [InlineData("Debug.Print CLngPtr(5)", "Sub or Function not defined*")]
    [InlineData("Debug.Print 5^", "Syntax error*")]
    [InlineData("DefLngLng A-Z", "Invalid outside procedure*")]
    public async Task Vb6Refusal(string code, string message)
    {
        var act = () => Run(code + "\n");
        await act.Should().ThrowAsync<VBCompileErrorException>().WithMessage("*" + message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UserConversionNameWins(bool vba64)
    {
        if (vba64) Use64BitProject();
        await Run("Function CLngLng(n)\nCLngLng = \"mine\"\nEnd Function\nDebug.Print CLngLng(5)\n");
        AssertDebugLog([new Vb6Value("mine")]);
    }

    [Fact]
    public async Task Vb6UserTypeNameWins()
    {
        await Run("Type LongLong\nn As Long\nEnd Type\nDim x As LongLong\nx.n = 5\nDebug.Print x.n\n");
        AssertDebugLog([new Vb6Value(5L)]);
    }

    [Theory]
    [InlineData("Debug.Print CLngLng(Expression:=5)")]
    [InlineData("Debug.Print CLngPtr(Expression:=5)")]
    [InlineData("Debug.Print CLngLng()")]
    [InlineData("Debug.Print CLngPtr(5, 6)")]
    [InlineData("Debug.Print CLngLng(True)")]
    [InlineData("Debug.Print CLngLng(CCur(5))")]
    [InlineData("Debug.Print CLngLng(CDec(5))")]
    [InlineData("Debug.Print CLngLng(#1/1/2000#)")]
    [InlineData("Debug.Print CLngLng(Empty)")]
    [InlineData("Debug.Print CLngLng(Null)")]
    [InlineData("Dim n As LongLong\nn = 5\nDim a(n)")]
    [InlineData("Dim n As LongPtr\nn = 5\nDim a()\nReDim a(n)")]
    [InlineData("Dim n As LongLong\nFor n = 1 To 5\nNext n")]
    [InlineData("Type T\nn As LongLong\nEnd Type\nDim x As T\nDebug.Print Len(x)")]
    [InlineData("Type T\nn As LongPtr\nEnd Type\nDim x As T\nDebug.Print LenB(x)")]
    public async Task UnmeasuredUseRefuses(string code)
    {
        Use64BitProject();
        var act = () => Run(code.StartsWith("Type", StringComparison.Ordinal) ? code + "\n"
            : "Sub Main()\n" + code + "\nEnd Sub\nMain\n");
        await act.Should().ThrowAsync<NotImplementedException>().WithMessage("*#1548*");
    }

    [Theory]
    [InlineData("LongLong")]
    [InlineData("LongPtr")]
    public async Task EveryDeclarationSiteUsesTheWideType(string type)
    {
        Use64BitProject();
        await Run($"""
            Type T
                n As {type}
            End Type
            Const N As {type} = 5
            Function Echo(ByVal arg As {type}) As {type}
                Debug.Print TypeName(arg)
                Echo = arg
            End Function
            Dim x As T
            x.n = N
            Debug.Print TypeName(N)
            Debug.Print TypeName(x.n)
            Debug.Print TypeName(Echo(x.n))
            Debug.Print CStr(x.n)
            """);
        AssertDebugLog([new Vb6Value("LongLong"), new Vb6Value("LongLong"),
            new Vb6Value("LongLong"), new Vb6Value("LongLong"), new Vb6Value("5")]);
    }

    [Theory]
    [InlineData("Get")]
    [InlineData("Put")]
    public async Task UnmeasuredRecordLayoutRefuses(string operation)
    {
        Use64BitProject();
        var folder = System.IO.Directory.CreateTempSubdirectory("hexide-longlong-").FullName;
        try
        {
            var file = System.IO.Path.Combine(folder, "record.bin");
            await System.IO.File.WriteAllBytesAsync(file, new byte[16], TestContext.Current.CancellationToken);
            var vb = new BasicInterpreter(new MockStdLib(debug), context, rootEnv,
                $"Type T\nn As LongLong\nEnd Type\nDim x As T\nOpen \"{file}\" For Binary As #1\n{operation} #1, 1, x\n",
                compilationConstants: Compilation);
            try
            {
                var act = () => vb.Execute();
                await act.Should().ThrowAsync<NotImplementedException>().WithMessage("*#1548*");
            }
            finally
            {
                // The refusal interrupts the program before it can Close its file (Windows forbids deleting it).
                vb.BuiltIns.Files.CloseAll();
            }
            using var released = System.IO.File.Open(file, System.IO.FileMode.Open,
                System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
        }
        finally
        {
            System.IO.Directory.Delete(folder, true);
        }
    }

    // Preserve HexIDE's existing VB6 parse refusal; this does not pin a new vb6.exe diagnostic.
    [Fact]
    public async Task Vb6DoesNotAcquireTheVbaDeclarationCharacter()
    {
        var act = () => Run("Dim x^\n");
        await act.Should().ThrowAsync<VBCompileErrorException>();
    }
}

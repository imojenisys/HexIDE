using System.Threading.Tasks;

namespace HexIDE.Runtime.Tests;

/// <summary>
/// An undeclared variable, which in VB6 is the ORDINARY case rather than a legacy one: "Require Variable
/// Declaration" is off by default, so a module without <c>Option Explicit</c> is what most VB6 code is.
///
/// Every expectation is measured — see "DefType" and the implicit-declaration rows in
/// docs/vb6-fidelity-oracle.md. The rule: on a bare name nothing else claims, VB6 creates a
/// procedure-local Variant, Empty, fresh per call, on read as well as write.
///
/// Before #171 all of this raised <c>Err 424 "Object required"</c>.
/// </summary>
public class ImplicitDeclarationTests : BaseVBTestFixture
{
    [Fact]
    public async Task AnUndeclaredVariableCanBeAssigned()
    {
        await Run("x = 5\r\nDebug.Print CStr(x)");
        AssertDebugLog(["5"]);
    }

    [Fact]
    public async Task ReadingAnUndeclaredVariableGivesEmpty()
    {
        // Legal, and not an error: an undeclared read is Empty.
        await Run("Debug.Print TypeName(neverTouched)");
        AssertDebugLog(["Empty"]);
    }

    [Fact]
    public async Task AnUndeclaredVariableReadsAsEmptyThenAccumulates()
    {
        // The idiom this unblocks: `counter = counter + 1` with no Dim, where the read is Empty and
        // Empty + 1 is 1.
        await Run("counter = counter + 1\r\ncounter = counter + 1\r\nDebug.Print CStr(counter)");
        AssertDebugLog(["2"]);
    }

    [Fact]
    public async Task AnImplicitVariableIsProcedureLocal()
    {
        // Measured: another procedure sees Empty, not the value. It is a local, not a module variable.
        await Run(
            "Sub Main()\r\n  SetIt\r\n  Debug.Print TypeName(hidden)\r\nEnd Sub\r\n" +
            "Sub SetIt()\r\n  hidden = 42\r\nEnd Sub\r\nMain");
        AssertDebugLog(["Empty"]);
    }

    [Fact]
    public async Task AnImplicitVariableIsFreshOnEachCall()
    {
        // Measured: Bump() returns 1 twice — no Static-like persistence.
        await Run(
            "Sub Bump()\r\n  n = n + 1\r\n  Debug.Print CStr(n)\r\nEnd Sub\r\nBump\r\nBump");
        AssertDebugLog(["1", "1"]);
    }

    [Fact]
    public async Task AModuleLevelDeclarationWinsOverImplicitCreation()
    {
        // Measured: a declared module variable is shared, not shadowed by a fresh local.
        await Run(
            "Dim shared_\r\n" +
            "Sub WriteIt()\r\n  shared_ = 99\r\nEnd Sub\r\n" +
            "Sub Main()\r\n  WriteIt\r\n  Debug.Print CStr(shared_)\r\nEnd Sub\r\nMain");
        AssertDebugLog(["99"]);
    }

    [Fact]
    public async Task OptionExplicitStillRejectsAnUndeclaredVariable()
    {
        // The other half of the defect: Option Explicit was collected by PrePass and read by nothing, so
        // the directive that SHOULD produce this error did nothing while code omitting it got the error.
        var act = async () => await Run("Option Explicit\r\nx = 5");
        await act.Should().ThrowAsync<HexIDE.Runtime.Interpreter.VBVariableNotDefinedException>();
    }

    [Fact]
    public async Task ADeclaredVariableIsUnaffected()
    {
        await Run("Dim x As Long\r\nx = 7\r\nDebug.Print TypeName(x)");
        AssertDebugLog(["Long"]);
    }
}

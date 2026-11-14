using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime.Tests;

public class UdtObjectFieldTests : BaseVBTestFixture
{
    [Theory]
    [InlineData("TextBox")]
    [InlineData("StdFont")]
    [InlineData("ADODB.Connection")]
    public async Task Other_recognised_object_fields_refuse_without_guessing_their_behaviour(string type)
    {
        // HexIDE's measured-only refusal policy, not a VB6 error expectation.
        var run = () => Run($"Private Type T\nObj As {type}\nEnd Type\nDim u As T\n");
        (await run.Should().ThrowAsync<NotImplementedException>()).WithMessage("*not implemented (#3924)*");
    }

    [Fact]
    public async Task A_project_class_field_takes_Set_through_a_nested_Type()
    {
        // vb6.exe SP6, native and p-code: chained-writes-qualified-set-and-udt-returning-members.md, B3 (#3924).
        await RunClasses("""
            Private Type InnerO
                Obj As Leaf
            End Type
            Private Type OuterO
                In1 As InnerO
            End Type
            Dim u As OuterO
            Private Function NewLeaf(ByVal n As Long) As Leaf
                Dim o As Leaf
                Set o = New Leaf
                o.Value = n
                Set NewLeaf = o
            End Function
            Set u.In1.Obj = NewLeaf(7)
            Debug.Print u.In1.Obj.Value

            """, ("Leaf", "Public Value As Long\n"));
        AssertDebugLog([new Vb6Value(7L)]);
    }
}

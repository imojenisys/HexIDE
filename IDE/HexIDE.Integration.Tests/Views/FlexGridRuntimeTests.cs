using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;

namespace HexIDE.Integration.Tests.Views;

public class FlexGridRuntimeTests
{
    private const string Form = """
        VERSION 5.00
        Begin VB.Form Form1
           ClientHeight = 3015
           ClientWidth = 4560
           Begin MSFlexGridLib.MSFlexGrid Grid1
              Height = 750
              Width = 1500
           End
        End
        Attribute VB_Name = "Form1"
        """;

    private static async Task<string[]> Run(string code)
    {
        var (_, _, ctx, env) = ContainerRuntimeHarness.Spawn(Form);
        return (await ContainerRuntimeHarness.Run(ctx, env, code))
            .Select(v => Convert.ToString(v.Value, System.Globalization.CultureInfo.InvariantCulture) ?? "").ToArray();
    }

    [AvaloniaFact]
    public async Task A_form_can_fill_and_read_its_grid()
    {
        (await Run("""
            Debug.Print Grid1.Rows, Grid1.Cols, Grid1.FixedRows, Grid1.FixedCols
            Grid1.Rows = 4
            Grid1.Cols = 3
            Grid1.Row = 3
            Grid1.Col = 2
            Grid1.Text = "current"
            Grid1.TextMatrix(0, 0) = "heading"
            Debug.Print Grid1.Row, Grid1.Col, Grid1.Text, Grid1.TextMatrix(0, 0)
            Grid1.FixedRows = 2
            Debug.Print Grid1.Row
            Grid1.Rows = 0
            Grid1.Cols = 0
            Debug.Print Grid1.Row, Grid1.Col, Grid1.FixedRows, Grid1.FixedCols
            """)).Should().Equal("2", "2", "1", "1", "3", "2", "current", "heading", "2", "-1", "-1", "0", "0");
    }

    [AvaloniaFact]
    public async Task FormatString_sets_headings_literally_and_keeps_its_assigned_value()
    {
        (await Run("""
            Grid1.FormatString = "<a|<b;>x|>y"
            Debug.Print Grid1.TextMatrix(0, 0), Grid1.TextMatrix(0, 1), Grid1.TextMatrix(1, 0)
            Grid1.TextMatrix(0, 0) = "changed"
            Debug.Print Grid1.FormatString
            """)).Should().Equal(">x", "b", ">y", "<a|<b;>x|>y");
    }

    [AvaloniaFact]
    public async Task Invalid_indices_keep_the_controls_own_error_numbers()
    {
        (await Run("""
            On Error Resume Next
            Grid1.Row = 2
            Debug.Print Err.Number
            Err.Clear
            Grid1.Col = -1
            Debug.Print Err.Number
            Err.Clear
            Debug.Print Grid1.TextMatrix(2, 0)
            Debug.Print Err.Number
            """)).Should().Equal("30009", "30010", "381");
    }

    [AvaloniaFact]
    public async Task FormatString_grows_columns_but_never_shrinks_them()
    {
        (await Run("""
            Grid1.FormatString = "A||C"
            Debug.Print Grid1.Cols, Grid1.TextMatrix(0, 1), Grid1.TextMatrix(0, 2)
            Grid1.FormatString = "X|Y"
            Debug.Print Grid1.Cols, Grid1.TextMatrix(0, 2)
            """)).Should().Equal("3", "", "C", "3", "C");
    }

    [AvaloniaFact]
    public async Task FormatString_trims_measured_trailing_spaces_from_column_headings()
    {
        (await Run("""
            Grid1.FormatString = "Name     |Wide"
            Debug.Print Grid1.TextMatrix(0, 0), Grid1.TextMatrix(0, 1), Grid1.FormatString
            """)).Should().Equal("Name", "Wide", "Name     |Wide");
    }

    [AvaloniaTheory]
    [InlineData("Debug.Print Grid1.Clip")]
    [InlineData("Grid1.RowSel = 0")]
    [InlineData("Grid1.Sort = 1")]
    [InlineData("Grid1.Clear")]
    [InlineData("Debug.Print Grid1.hWnd")]
    [InlineData("Grid1.Text = 42")]
    [InlineData("Grid1.Rows = 1.5")]
    public async Task Later_members_and_unmeasured_conversions_are_refused(string code)
    {
        var act = () => Run(code);
        await act.Should().ThrowAsync<NotImplementedException>().WithMessage("*MSFlexGrid*not implemented*#1099*");
    }
}

using System.Collections.Generic;
using System.Threading;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using HexIDE.Runtime;
using HexIDE.Runtime.Interpreter;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;

namespace HexIDE.Runtime.Integration.Tests.Views;

// #990: measured SP6 native/p-code rows in docs/vb6-fidelity-oracle/
// the-coordinate-system-scalemode-scale-scalex-and-scaley.md and issue #990's final cm/mm decision.
public class CoordinateSystemTests
{
    private sealed class NullSink : IDeserializeErrorSink
    {
        public void LogError(string _) { }
    }

    private const string Form = """
        VERSION 5.00
        Begin VB.Form Form1
           Caption = "Form1"
           ClientHeight = 3195
           ClientWidth = 4680
           Begin VB.PictureBox Picture1
              Left = 2160
              Top = 1200
              Width = 2415
              Height = 1815
           End
        End
        Attribute VB_Name = "Form1"

        """;

    private static string Snapshot(string target) =>
        $"CStr({target}ScaleMode) & \"|\" & CStr({target}ScaleLeft) & \"|\" & CStr({target}ScaleTop) & \"|\" & CStr({target}ScaleWidth) & \"|\" & CStr({target}ScaleHeight)";

    private static void Check(string code, params string[] expected)
    {
        var run = RunLoad(Form, code);
        run.Errors.Should().BeEmpty();
        run.Printed.Should().Equal(expected);
    }

    private sealed record Run(List<string> Printed, List<Exception> Errors);

    /// <summary>Run Form1 as F5 does, with <paramref name="load"/> as its Load, then close it.</summary>
    private static Run RunLoad(string frm, string load)
    {
        var project = new ProjectDefinition(VBProjectType.EXE, "P");
        var code = "Private Sub Form_Load()\r\n" + load.ReplaceLineEndings("\r\n") + "\r\nEnd Sub\r\n"
            + "Sub S(ByVal what As String, ByVal v As Variant)\r\n    Debug.Print what & \" = \" & v & \" \" & TypeName(v)\r\nEnd Sub\r\n";
        var form = new FormDeserializer().Deserialize(project, frm + code, new NullSink())!;
        project.AddForm(form);

        var run = new Run([], []);
        void OnFaulted(VBWindowContext _, Exception e) => run.Errors.Add(e);
        void OnRunTimeError(VBWindowContext _, VBRunTimeException e) => run.Errors.Add(e);
        void OnCompileError(VBWindowContext _, VBCompileErrorException e) => run.Errors.Add(e);
        VBDebugConsole.Output += run.Printed.Add;
        VBWindowContext.Faulted += OnFaulted;
        VBWindowContext.RunTimeError += OnRunTimeError;
        VBWindowContext.CompileError += OnCompileError;
        try
        {
            VBLoader.RunForm(form, CancellationToken.None, out var window);
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
        finally
        {
            VBDebugConsole.Output -= run.Printed.Add;
            VBWindowContext.Faulted -= OnFaulted;
            VBWindowContext.RunTimeError -= OnRunTimeError;
            VBWindowContext.CompileError -= OnCompileError;
        }
        return run;
    }

    [AvaloniaTheory]
    [InlineData("Me.", "ScaleMode", "1 Integer")]
    [InlineData("Me.", "ScaleLeft", "0 Single")]
    [InlineData("Me.", "ScaleTop", "0 Single")]
    [InlineData("Me.", "ScaleWidth", "4680 Single")]
    [InlineData("Me.", "ScaleHeight", "3195 Single")]
    [InlineData("Picture1.", "ScaleMode", "1 Integer")]
    [InlineData("Picture1.", "ScaleLeft", "0 Single")]
    [InlineData("Picture1.", "ScaleTop", "0 Single")]
    [InlineData("Picture1.", "ScaleWidth", "2355 Single")]
    [InlineData("Picture1.", "ScaleHeight", "1755 Single")]
    public void Initial_properties_have_the_measured_type_and_value(string target, string member, string expected) =>
        Check($"Debug.Print CStr({target}{member}) & \" \" & TypeName({target}{member})", expected);

    [AvaloniaTheory]
    [InlineData(0, "0|0|0|4680|3195")]
    [InlineData(1, "1|0|0|4680|3195")]
    [InlineData(2, "2|0|0|234|159.75")]
    [InlineData(3, "3|0|0|312|213")]
    [InlineData(4, "4|0|0|39|13.3125")]
    [InlineData(5, "5|0|0|3.25|2.21875")]
    [InlineData(6, "6|0|0|82.55008|56.3563")]
    [InlineData(7, "7|0|0|8.255008|5.635631")]
    public void Setting_mode_maps_the_client_area(int mode, string expected) =>
        Check($"ScaleMode = {mode}\nDebug.Print " + Snapshot("Me."), expected);

    [AvaloniaTheory]
    [InlineData("ScaleWidth = 1000", "0|0|0|1000|3195")]
    [InlineData("ScaleLeft = 50", "0|50|0|4680|3195")]
    [InlineData("ScaleWidth = -100", "0|0|0|-100|3195")]
    [InlineData("ScaleWidth = 1000\nScaleMode = vbTwips", "1|0|0|4680|3195")]
    [InlineData("ScaleWidth = 1000\nScale", "1|0|0|4680|3195")]
    [InlineData("Scale (0, 100)-(10, 0)", "0|0|100|10|-100")]
    [InlineData("Scale (10, 10)-(110, 60)", "0|10|10|100|50")]
    [InlineData("Me.Scale (0, 100)-(10, 0)", "0|0|100|10|-100")]
    [InlineData("Form1.Scale (0, 100)-(10, 0)", "0|0|100|10|-100")]
    public void Changing_a_scale_keeps_the_other_dimensions(string statement, string expected) =>
        Check(statement + "\nDebug.Print " + Snapshot("Me."), expected);

    [AvaloniaFact]
    public void PictureBox_owns_its_scale_and_reset() =>
        Check("Picture1.Scale (0, 100)-(10, 0)\nDebug.Print " + Snapshot("Picture1.")
            + "\nPicture1.Scale\nDebug.Print " + Snapshot("Picture1."),
            "0|0|100|10|-100", "1|0|0|2355|1755");

    [AvaloniaFact]
    public void Child_positions_use_the_user_scale_and_return_to_pixels() =>
        Check("Scale (10, 10)-(110, 60)\nDebug.Print CStr(Picture1.Left) & \"|\" & CStr(Picture1.Top) & \"|\" & CStr(Picture1.Width)"
            + "\nScaleMode = vbPixels\nDebug.Print CStr(Picture1.Left) & \"|\" & CStr(Picture1.Top) & \"|\" & CStr(Picture1.Width)",
            "56.15385|28.77934|51.60257", "144|80|161");

    [AvaloniaTheory]
    [InlineData("Me.")]
    [InlineData("Picture1.")]
    public void Every_scale_property_can_be_set(string target) =>
        Check($"{target}ScaleLeft = 10\n{target}ScaleTop = 10\n{target}ScaleWidth = 100\n{target}ScaleHeight = 50\nDebug.Print " + Snapshot(target),
            "0|10|10|100|50");

    [AvaloniaFact]
    public void A_user_position_assignment_rounds_back_to_the_same_whole_twips() =>
        Check("Scale (10, 10)-(110, 60)\nPicture1.Left = Picture1.Left\nPicture1.Top = Picture1.Top\nPicture1.Width = Picture1.Width\nPicture1.Height = Picture1.Height\n"
            + "ScaleMode = vbTwips\nDebug.Print CStr(Picture1.Left) & \"|\" & CStr(Picture1.Top) & \"|\" & CStr(Picture1.Width) & \"|\" & CStr(Picture1.Height)",
            "2160|1200|2415|1815");

    [AvaloniaFact]
    public void A_PictureBoxs_children_use_its_user_mapping()
    {
        // Give the enclosing PictureBox the oracle form's client area: the same mapping must produce the same values.
        var frm = Form.ReplaceLineEndings("\n").Replace("   Begin VB.PictureBox Picture1", "   Begin VB.PictureBox Container1\nWidth = 4740\nHeight = 3255\n   Begin VB.PictureBox Picture1")
            .Replace("   End\nEnd", "   End\n   End\nEnd");
        var run = RunLoad(frm, "Container1.Scale (10, 10)-(110, 60)\nDebug.Print CStr(Picture1.Left) & \"|\" & CStr(Picture1.Top) & \"|\" & CStr(Picture1.Width)");
        run.Errors.Should().BeEmpty();
        run.Printed.Should().Equal("56.15385|28.77934|51.60257");
    }

    [AvaloniaTheory]
    [InlineData("ScaleMode = 8", 380)]
    [InlineData("ScaleMode = -1", 380)]
    [InlineData("ScaleWidth = 0", 380)]
    [InlineData("ScaleHeight = 0", 380)]
    [InlineData("Scale (0, 0)-(0, 10)", 11)]
    public void Invalid_changes_raise_the_measured_error_and_keep_the_scale(string statement, int error) =>
        Check("On Error Resume Next\n" + statement + "\nDebug.Print CStr(Err.Number)\nDebug.Print " + Snapshot("Me."),
            error.ToString(), "1|0|0|4680|3195");

    [AvaloniaFact]
    public void Resizing_preserves_the_unit_in_user_scales() =>
        Check("Width = Width + 1500\nHeight = Height + 750\nDebug.Print " + Snapshot("Me.")
            + "\nScale (0, 0)-(100, 100)\nWidth = Width - 1500\nHeight = Height - 750\nDebug.Print " + Snapshot("Me.")
            + "\nScaleMode = vbPixels\nWidth = Width + 1500\nDebug.Print CStr(ScaleWidth)",
            "1|0|0|6180|3945", "0|0|0|75.72816|80.98859", "412");

    [AvaloniaTheory]
    [InlineData("ScaleX(1, vbInches, vbTwips)", "1440")]
    [InlineData("ScaleX(100, vbPixels, vbTwips)", "1500")]
    [InlineData("ScaleY(100, vbPixels, vbTwips)", "1500")]
    [InlineData("ScaleX(1000)", "566.9291")]
    [InlineData("ScaleX(1000, vbHimetric)", "566.9291")]
    [InlineData("ScaleX(1, vbCharacters, vbTwips)", "120")]
    [InlineData("ScaleY(1, vbCharacters, vbTwips)", "240")]
    [InlineData("ScaleX(1, vbPoints, vbTwips)", "20")]
    [InlineData("ScaleX(1, vbCentimeters, vbTwips)", "566.9286")]
    [InlineData("ScaleY(1, vbCentimeters, vbTwips)", "566.9286")]
    [InlineData("ScaleX(1, vbMillimeters, vbTwips)", "56.69286")]
    [InlineData("ScaleX(1, vbTwips, vbCentimeters)", "1.763891E-03")]
    [InlineData("ScaleX(4680, vbTwips, vbCentimeters)", "8.255008")]
    [InlineData("ScaleX(1, vbCentimeters, vbInches)", "0.3937004")]
    [InlineData("ScaleX(1000, vbHimetric, vbCentimeters)", "1.000001")]
    [InlineData("ScaleX(1, vbCentimeters, vbHimetric)", "999.999")]
    [InlineData("ScaleX(1, vbPixels, vbCentimeters)", "2.645836E-02")]
    [InlineData("ScaleX(1, vbPoints, vbCentimeters)", "3.527781E-02")]
    [InlineData("ScaleX(1, vbCentimeters, vbMillimeters)", "10")]
    public void ScaleX_and_ScaleY_return_the_printed_Single(string call, string expected) =>
        Check($"Debug.Print CStr(Me.{call}) & \" \" & TypeName(Me.{call})", expected + " Single");

    [AvaloniaTheory]
    [InlineData("Picture1.ScaleX(1, vbInches, vbTwips)", "1440")]
    [InlineData("Picture1.ScaleY(100, vbPixels, vbTwips)", "1500")]
    [InlineData("ScaleX(1000)", "566.9291")]
    [InlineData("ScaleY(1, vbCharacters, vbTwips)", "240")]
    public void PictureBox_and_bare_conversions_use_the_same_units(string call, string expected) =>
        Check($"Debug.Print CStr({call}) & \" \" & TypeName({call})", expected + " Single");

    [AvaloniaTheory]
    [InlineData("Me.", "", "ScaleX(1, vbUser, vbTwips)", "error")]
    [InlineData("Me.", "Scale (0, 0)-(100, 100)", "ScaleX(1, vbUser, vbTwips)", "46.8")]
    [InlineData("Me.", "Scale (0, 0)-(100, 100)", "ScaleX(10)", "0.1211387")]
    [InlineData("Picture1.", "", "ScaleX(1, 11, vbTwips)", "error")]
    [InlineData("Me.", "", "ScaleX(1, 11, vbTwips)", "error")]
    public void Conversion_defaults_and_invalid_units(string target, string setup, string call, string expected) =>
        Check(setup + $"\nOn Error Resume Next\nDim v\nv = {target}{call}\nIf Err.Number <> 0 Then\nDebug.Print \"error \" & Err.Number\nElse\nDebug.Print CStr(v)\nEnd If",
            expected == "error" ? "error 5" : expected);

    public static IEnumerable<object[]> ContainerConversions()
    {
        foreach (var target in new[] { "Me.", "Picture1." })
        foreach (var setup in new[] { "", "ScaleMode = vbPixels", "Scale (0, 0)-(100, 100)" })
        foreach (var unit in new[] { "vbContainerPosition", "vbContainerSize" })
        foreach (var arguments in new[] { $"1, {unit}, vbTwips", $"1, vbTwips, {unit}" })
            yield return [target, setup, arguments];
    }

    [AvaloniaTheory]
    [MemberData(nameof(ContainerConversions))]
    public void Container_units_are_error_5(string target, string setup, string arguments) =>
        Check(setup + $"\nOn Error Resume Next\nDim v\nv = {target}ScaleX({arguments})\nDebug.Print CStr(Err.Number)", "5");

    [AvaloniaTheory]
    [InlineData("Me.ScaleX()")]
    [InlineData("Me.ScaleX(1, 1, 1, 1)")]
    [InlineData("Picture1.ScaleY()")]
    [InlineData("Picture1.ScaleY(1, 1, 1, 1)")]
    public void Unmeasured_conversion_argument_counts_refuse_naming_the_issue(string call)
    {
        var run = RunLoad(Form, $"Dim v\nv = {call}");
        run.Errors.Should().ContainSingle().Which.Should().BeOfType<NotImplementedException>()
            .Which.Message.Should().Contain("argument count").And.Contain("#990");
    }

    [AvaloniaTheory]
    [InlineData("As Object", "o.Scale (0, 0)-(100, 100)")]
    [InlineData("", "o.Scale (0, 0)-(100, 100)")]
    [InlineData("As Object", "o.Scale")]
    [InlineData("", "o.Scale")]
    public void Unmeasured_late_bound_Scale_refuses_naming_the_issue(string declaration, string statement)
    {
        var run = RunLoad(Form, $"Dim o {declaration}\nSet o = Me\n{statement}");
        run.Errors.Should().ContainSingle().Which.Should().BeOfType<NotImplementedException>()
            .Which.Message.Should().Contain("Late-bound Scale").And.Contain("#990");
    }

    [AvaloniaTheory]
    [InlineData(7, "1", "567", "1.000126")]
    [InlineData(7, "2.5", "1417", "2.499433")]
    [InlineData(6, "10", "567", "10.00126")]
    public void Metric_positions_keep_whole_twips(int mode, string value, string twips, string printed) =>
        Check($"ScaleMode = {mode}\nPicture1.Left = {value}\nDebug.Print CStr(Picture1.Left)\nScaleMode = vbTwips\nDebug.Print CStr(Picture1.Left)",
            printed, twips);

    [AvaloniaTheory]
    [InlineData(7, 15, "8.281466")]
    [InlineData(6, 15, "82.81467")]
    [InlineData(7, 1500, "10.90084")]
    public void Metric_scale_sizes_follow_resize(int mode, int extra, string expected) =>
        Check($"ScaleMode = {mode}\nWidth = Width + {extra}\nDebug.Print CStr(ScaleWidth)", expected);
}

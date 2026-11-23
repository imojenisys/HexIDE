using System;
using System.Linq;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;

namespace HexIDE.Runtime.Tests;

// Saved spellings and default for #1596: https://github.com/hexide-io/HexIDE/pull/5216.
public class FormBorderStyleSerializationTests
{
    private sealed class Sink : IDeserializeErrorSink
    {
        public void LogError(string error) => throw new InvalidOperationException(error);
    }

    [Theory]
    [InlineData(0, "None")]
    [InlineData(1, "Fixed Single")]
    [InlineData(2, "Sizable")]
    [InlineData(3, "Fixed Dialog")]
    [InlineData(4, "Fixed ToolWindow")]
    [InlineData(5, "Sizable ToolWindow")]
    public void Modelled_styles_write_the_measured_line_in_alphabetical_order(int style, string name)
    {
        var form = new FormDefinition(new ProjectDefinition(VBProjectType.EXE, "P"), FormComponentClass.Instance, "Form1");
        var instance = form.Components.Single();
        var property = instance.BaseClass.Properties.SingleOrDefault(p => p.Name == "BorderStyle");
        property.Should().NotBeNull();
        instance.SetProperty(VBProperties.BackColorProperty, HexIDE.Runtime.BuiltinTypes.VBColor.FromOle(0));
        instance.SetProperty(VBProperties.CaptionProperty, "Form1");
        instance.SetUntypedProperty(property!, Enum.ToObject(property!.PropertyType, style));
        var saved = new FormSerializer().Serialize(form, "Form1.frm").frmText;
        if (style == 2) saved.Should().NotContain("BorderStyle");
        else saved.Should().Contain("   BackColor       =   &H00000000&\r\n   BorderStyle     =   " + style
            + "  '" + name + "\r\n   Caption");
    }

    [Theory]
    [InlineData("   BorderStyle     =   3  'Fixed Dialog\r\n")]
    [InlineData("")]
    public void Fixed_dialog_and_default_sizable_round_trip_byte_for_byte(string border)
    {
        var source = "VERSION 5.00\r\nBegin VB.Form Form1 \r\n" + border
            + "   Caption         =   \"Form1\"\r\n   ClientHeight    =   1500\r\n"
            + "   ClientLeft      =   60\r\n   ClientTop       =   345\r\n   ClientWidth     =   2520\r\n"
            + "   ScaleHeight     =   1500\r\n   ScaleWidth      =   2520\r\n"
            + "End\r\nAttribute VB_Name = \"Form1\"\r\n";
        var form = new FormDeserializer().Deserialize(new ProjectDefinition(VBProjectType.EXE, "P"), source, new Sink())!;
        var saved = new FormSerializer().Serialize(form, "Form1.frm").frmText;
        saved.Should().Be(source);
        if (border.Length == 0) saved.Should().NotContain("BorderStyle");
    }

    [Theory]
    [InlineData("99")]
    [InlineData("-1")]
    [InlineData("\"unreadable\"")]
    public void Unmeasured_values_keep_the_raw_line(string value)
    {
        var source = "VERSION 5.00\r\nBegin VB.Form Form1 \r\n   BorderStyle     =   " + value
            + "\r\nEnd\r\nAttribute VB_Name = \"Form1\"\r\n";
        var form = new FormDeserializer().Deserialize(new ProjectDefinition(VBProjectType.EXE, "P"), source, new Sink())!;
        form.Components.Single().UnknownRawPropertyLines.Should().Contain("   BorderStyle     =   " + value);
        new FormSerializer().Serialize(form, "Form1.frm").frmText.Should().Contain("   BorderStyle     =   " + value + "\r\n");
    }
}

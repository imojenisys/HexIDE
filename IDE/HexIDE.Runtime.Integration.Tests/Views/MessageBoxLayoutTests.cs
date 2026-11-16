using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HexIDE.IDE;
using MessageBox = HexIDE.Runtime.Dialogs.MessageBox;

namespace HexIDE.Runtime.Integration.Tests.Views;

public class MessageBoxLayoutTests
{
    // #3973; docs/vb6-fidelity-oracle/rgb-qbcolor-and-msgboxs-style.md, measuring PR #3958.
    [AvaloniaTheory]
    [InlineData(MessageBoxLayout.None, false)]
    [InlineData(MessageBoxLayout.Right, false)]
    [InlineData(MessageBoxLayout.RtlReading, true)]
    [InlineData(MessageBoxLayout.Right | MessageBoxLayout.RtlReading, true)]
    public void Windows_program_window_styles_follow_RtlReading(MessageBoxLayout layout, bool mirrored)
    {
        var box = new MessageBox { IsProgramDialog = true, Layout = layout };
        var callback = box.WindowStylesCallback(true);
        if (!mirrored)
        {
            callback.Should().BeNull();
            return;
        }
        callback.Should().NotBeNull();
        callback!(0x12345678, 0x00010101).Should().Be((0x12345678u, 0x00410101u));
        callback(0, 0x00400000).Should().Be((0u, 0x00400000u));
    }

    [AvaloniaFact]
    public void Other_platforms_and_IDE_boxes_have_no_window_style_callback()
    {
        var program = new MessageBox { IsProgramDialog = true, Layout = MessageBoxLayout.RtlReading };
        program.WindowStylesCallback(false).Should().BeNull();
        if (!System.OperatingSystem.IsWindows())
            program.WindowStylesCallback(System.OperatingSystem.IsWindows()).Should().BeNull();
        var ide = new MessageBox { Layout = MessageBoxLayout.RtlReading };
        ide.WindowStylesCallback(true).Should().BeNull();
    }

    // VB6 SP6 native and p-code, supplied in #3014 by measuring PR #3958, style 35 and the two layout flags.
    [AvaloniaTheory]
    [InlineData(MessageBoxLayout.None, false, false)]
    [InlineData(MessageBoxLayout.Right, false, true)]
    [InlineData(MessageBoxLayout.RtlReading, true, false)]
    [InlineData(MessageBoxLayout.Right | MessageBoxLayout.RtlReading, true, true)]
    public async Task The_programs_layout_flags_position_the_icon_text_and_buttons(
        MessageBoxLayout layout, bool mirrored, bool rightAligned)
    {
        var owner = new Window();
        owner.Show();
        var answer = new StandaloneStandardLib(owner).MsgBox("probe", "Layout probe",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, layout);
        var window = owner.OwnedWindows.Single();
        var box = (MessageBox)window.Content!;
        window.Width = 420;
        window.Height = 180;
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var prompt = box.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "probe");
            var icon = box.GetVisualDescendants().OfType<Border>().Single(b => b.Child is TextBlock { Text: "?" });
            var promptLeft = prompt.PointToScreen(default).X;
            var iconLeft = icon.PointToScreen(default).X;
            (iconLeft > promptLeft).Should().Be(mirrored);
            var line = prompt.TextLayout.TextLines.Single();
            (line.Start > 1).Should().Be(rightAligned, "the laid-out glyphs align within the prompt's available width");
            var buttons = box.GetVisualDescendants().OfType<Button>()
                .OrderBy(b => b.PointToScreen(default).X).ToArray();
            buttons.Select(b => b.Content).Should().Equal(mirrored
                ? new[] { "Cancel", "No", "Yes" } : new[] { "Yes", "No", "Cancel" });
            buttons.Single(b => Equals(b.Content, "No")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            (await answer).Should().Be(MessageBoxResult.No);
        }
        finally
        {
            window.Close();
            owner.Close();
        }
    }
}

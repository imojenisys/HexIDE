using System.Text.RegularExpressions;

namespace HexIDE.Tests.Infrastructure;

public class ProgramHostSeamTests
{
    [Fact]
    public void Run_engine_shows_program_dialogs_and_gets_failure_texts_through_Core()
    {
        // #4066: the outside engine inherits IProgramHost, not the IDE's window manager or localization service.
        var forbidden = new Regex(@"\b(ILocalizationService|HexIDE\.Localization|OpenFilePickerAsync|SaveFilePickerAsync)\b|windowManager\.(MessageBox|InputBox|ShowPrintDialog|ShowColorDialog|ShowFontDialog)");
        ProductSource.FilesMatching(forbidden)
            .Where(file => Path.GetFileName(file) == "InterpreterRunEngine.cs")
            .Should().BeEmpty();
    }
}

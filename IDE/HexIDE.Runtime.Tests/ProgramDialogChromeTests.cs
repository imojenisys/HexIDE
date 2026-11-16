using HexIDE.Runtime.Dialogs;

namespace HexIDE.Runtime.Tests;

public class ProgramDialogChromeTests
{
    // GetWindowLong measurements in hexide-io/HexIDE#4099: no min/max boxes, WS_EX_DLGMODALFRAME.
    [Theory]
    [InlineData(0xffffffffu, 0xfffffffeu, 0xfffcffffu, 0xffffffffu)]
    [InlineData(0x94c801c5u, 0x00010100u, 0x94c801c5u, 0x00010101u)]
    public void Only_the_measured_caption_bits_change(uint style, uint exStyle, uint expectedStyle, uint expectedExStyle)
    {
        ProgramDialogChrome.WindowsStyles(style, exStyle).Should().Be((expectedStyle, expectedExStyle));
    }
}

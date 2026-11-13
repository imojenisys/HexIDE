using System.Xml.Linq;

namespace HexIDE.Tests.Infrastructure;

public class AvaloniaPinTests
{
    [Theory]
    [InlineData("Avalonia")]
    [InlineData("Avalonia.Browser")]
    [InlineData("Avalonia.Controls.ColorPicker")]
    [InlineData("Avalonia.Desktop")]
    [InlineData("Avalonia.Headless")]
    [InlineData("Avalonia.Headless.XUnit")]
    [InlineData("Avalonia.Skia")]
    [InlineData("Avalonia.Themes.Simple")]
    [InlineData("Avalonia.Wayland")]
    public void Repository_packages_stay_on_the_macos_mitigation_version(string package)
    {
        var packages = XDocument.Load(Path.Combine(RepoTree.Root(), "Directory.Packages.props"));
        packages.Descendants("PackageVersion").Single(p => (string?)p.Attribute("Include") == package)
            .Attribute("Version")!.Value.Should().Be("12.1.4", "#3869 pins back while macOS instability is investigated");
    }
}

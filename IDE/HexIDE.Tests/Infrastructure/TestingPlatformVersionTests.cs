namespace HexIDE.Tests.Infrastructure;

public class TestingPlatformVersionTests
{
    [Fact]
    public void RunnerUsesMicrosoftTestingPlatformV2()
    {
        typeof(Microsoft.Testing.Platform.Builder.TestApplication).Assembly.GetName().Version!
            .Should().Be(new Version(2, 5, 3, 0));
    }

    [Fact]
    public void HangDumpMatchesMicrosoftTestingPlatform()
    {
        typeof(Microsoft.Testing.Extensions.HangDumpExtensions).Assembly.GetName().Version
            .Should().Be(typeof(Microsoft.Testing.Platform.Builder.TestApplication).Assembly.GetName().Version);
    }
}

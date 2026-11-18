using Avalonia.Controls.ApplicationLifetimes;
using HexIDE.Addins;
using HexIDE.Desktop;

namespace HexIDE.Tests.Infrastructure;

public class AddinShutdownTests
{
    [AvaloniaFact]
    public void Cancelled_request_keeps_addins_loaded()
    {
        using var desktop = new ClassicDesktopStyleApplicationLifetime();
        var loader = Substitute.For<IAddinLoader>();
        desktop.ShutdownRequested += (_, e) => e.Cancel = true;
        DesktopStartup.DisposeOnShutdown(desktop, loader);

        desktop.TryShutdown().Should().BeFalse();

        loader.DidNotReceive().Dispose();
    }

    [AvaloniaFact]
    public void Accepted_request_disposes_addins_once()
    {
        using var desktop = new ClassicDesktopStyleApplicationLifetime();
        var loader = Substitute.For<IAddinLoader>();
        DesktopStartup.DisposeOnShutdown(desktop, loader);

        desktop.TryShutdown().Should().BeTrue();

        loader.Received(1).Dispose();
    }
}

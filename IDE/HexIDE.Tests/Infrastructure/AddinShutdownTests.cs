using Avalonia.Controls.ApplicationLifetimes;
using HexIDE.Addins;
using HexIDE.Desktop;
using HexIDE.Localization;

namespace HexIDE.Tests.Infrastructure;

public class AddinShutdownTests : CompositionTestBase
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
        // Subscribed after the guard, so the guard sees the request still going through. Cancelling it here keeps the
        // lifetime from calling Dispatcher.UIThread.InvokeShutdown(), which would end the dispatcher the headless test
        // session shares with every later test (AppExitTests does the same).
        desktop.ShutdownRequested += (_, e) => e.Cancel = true;

        desktop.TryShutdown();

        loader.Received(1).Dispose();
    }

    [AvaloniaFact]
    public void Cancelled_request_keeps_provider_registered()
    {
        using var desktop = new ClassicDesktopStyleApplicationLifetime();
        var basicAreas = LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic");
        using var provider = BasicProviderComposition.Register(Composition.HexIdeHost);
        LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic").Should().Be(basicAreas + 1);
        var loader = Substitute.For<IAddinLoader>();
        desktop.ShutdownRequested += (_, e) => e.Cancel = true;
        DesktopStartup.DisposeOnShutdown(desktop, loader, provider);

        desktop.TryShutdown().Should().BeFalse();

        LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic").Should().Be(basicAreas + 1);
        loader.DidNotReceive().Dispose();
    }

    [AvaloniaFact]
    public void Accepted_request_withdraws_provider_after_addins()
    {
        using var desktop = new ClassicDesktopStyleApplicationLifetime();
        var basicAreas = LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic");
        using var provider = BasicProviderComposition.Register(Composition.HexIdeHost);
        LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic").Should().Be(basicAreas + 1);
        var loader = Substitute.For<IAddinLoader>();
        loader.When(l => l.Dispose()).Do(_ =>
            LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic").Should().Be(basicAreas + 1));
        DesktopStartup.DisposeOnShutdown(desktop, loader, provider);
        // Cancel after the cleanup, as above, to keep the shared dispatcher alive.
        desktop.ShutdownRequested += (_, e) => e.Cancel = true;

        desktop.TryShutdown();

        loader.Received(1).Dispose();
        LanguagePackAreas.Current.Areas.Count(a => a.OwnerId == "hexide.basic").Should().Be(basicAreas);
    }
}

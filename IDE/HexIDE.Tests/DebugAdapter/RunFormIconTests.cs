using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Platform;
using HexIDE.IDE;
using HexIDE.Runtime;
using HexIDE.Runtime.Debugging;
using HexIDE.Runtime.ProjectElements;

namespace HexIDE.Tests.DebugAdapter;

// Host policy (#4187), not a new VB6 measurement. The default stays big only through FormIcon:
// docs/vb6-fidelity-oracle/a-forms-window-icon-no-icon-line.md.
public class RunFormIconTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_branded_run_prepares_the_neutral_runner_icon_with_or_without_a_logo(bool logo)
    {
        var identity = new DistributionIdentity("acme", "Acme", logo: logo ? [1] : null);
        var previous = VBLoader.DefaultFormIcon;
        try
        {
            VBLoader.DefaultFormIcon = null;
            Prepare(identity);

            VBLoader.DefaultFormIcon.Should().NotBeNull();
        }
        finally
        {
            VBLoader.DefaultFormIcon = previous;
        }
    }

    [AvaloniaFact]
    public void A_stock_run_clears_any_hosts_default_icon()
    {
        var previous = VBLoader.DefaultFormIcon;
        try
        {
            VBLoader.DefaultFormIcon = new WindowIcon(AssetLoader.Open(new Uri("avares://HexIDE.Runtime/form.ico")));
            Prepare(DistributionIdentity.Stock);

            VBLoader.DefaultFormIcon.Should().BeNull();
        }
        finally
        {
            VBLoader.DefaultFormIcon = previous;
        }
    }

    [AvaloniaFact]
    public void The_runner_loads_and_embeds_the_same_shared_icon()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "LspServer")))
            root = root.Parent;
        root.Should().NotBeNull();
        var runner = Path.Combine(root!.FullName, "IDE", "HexIDE.Standalone");
        var source = File.ReadAllText(Path.Combine(runner, "App.axaml.cs"));
        source.Should().Contain("avares://HexIDE.Runtime/form.ico");
        source.Should().NotContain("avares://HexIDE.Standalone/form.ico");
        File.ReadAllText(Path.Combine(root.FullName, "IDE", "HexIDE", "IDE", "InterpreterRunEngine.cs"))
            .Should().Contain("avares://HexIDE.Runtime/form.ico");
        using var stream = AssetLoader.Open(new Uri("avares://HexIDE.Runtime/form.ico"));
        new WindowIcon(stream).Should().NotBeNull();
        using var bytes = AssetLoader.Open(new Uri("avares://HexIDE.Runtime/form.ico"));
        using var actual = new MemoryStream();
        bytes.CopyTo(actual);
        var project = System.Xml.Linq.XDocument.Load(Path.Combine(runner, "HexIDE.Standalone.csproj"));
        var path = project.Descendants("ApplicationIcon").Single().Value;
        var embedded = Path.GetFullPath(Path.Combine(runner, path.Replace('\\', Path.DirectorySeparatorChar)));
        embedded.Should().Be(Path.Combine(root.FullName, "IDE", "HexIDE.Runtime", "form.ico"));
        actual.ToArray().Should().Equal(File.ReadAllBytes(embedded));
    }

    private static void Prepare(DistributionIdentity distribution)
    {
        var engine = new InterpreterRunEngine(Substitute.For<IWindowManager>(), Substitute.For<IProgramHost>(), new DebugController());
        // Even a refused Sub Main start must prepare the host before any form can be loaded.
        var project = new ProjectDefinition(VBProjectType.EXE, "P") { StartsAtSubMain = true };
        engine.Prepare(project, distribution).Failure.Should().NotBeNull();
    }
}

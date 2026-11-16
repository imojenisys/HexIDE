using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Runtime.Debugging;
using HexIDE.Runtime.ProjectElements;

namespace HexIDE.Tests.DebugAdapter;

// #4245: go through the engine's real VBProgram construction, before the first statement runs.
public class VbaBitnessEngineTests
{
    [AvaloniaFact]
    public void Missing_PtrSafe_uses_the_localized_host_diagnostic_before_starting()
    {
        var localization = Substitute.For<ILocalizationService>();
        localization.GetString("Str.ProjectRunner.VbaPtrSafeRequired").Returns("localized: {0}");
        var host = new ProgramHost(Substitute.For<IWindowManager>(), localization);
        var result = Engine(host).Prepare(Project(VbaBitness.Bit64,
            "Private Declare Function Tick Lib \"kernel32\" () As Long\n"));
        result.Run.Should().BeNull();
        result.Failure!.Text.Should().Be("localized: Tick");
    }

    [AvaloniaFact]
    public void Unmeasured_32_bit_project_refuses_during_real_program_construction()
    {
        var host = Substitute.For<IProgramHost>();
        host.NotImplementedYet.Returns("Not implemented: {0}");
        var result = Engine(host).Prepare(Project(VbaBitness.Bit32, ""));
        result.Run.Should().BeNull();
        result.Failure!.Text.Should().Contain("32-bit VBA7 is not measured yet (#1548)");
    }

    private static InterpreterRunEngine Engine(IProgramHost host) =>
        new(Substitute.For<IWindowManager>(), host, new DebugController());

    private static ProjectDefinition Project(VbaBitness bitness, string declarations)
    {
        var project = new ProjectDefinition(VBProjectType.EXE, "Test") { VbaBitness = bitness, StartsAtSubMain = true };
        var module = new ModuleDefinition(project, "Module1", ModuleKind.StandardModule);
        module.UpdateCode(declarations + "Sub Main()\nEnd Sub\n");
        project.AddModule(module);
        return project;
    }
}

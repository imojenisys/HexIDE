using System.Text.RegularExpressions;

namespace HexIDE.Tests.Infrastructure;

public class RunningProgramBoundaryTests
{
    [Fact]
    public void Product_callers_ask_the_contract_instead_of_naming_interpreter_types()
    {
        var root = RepoTree.Root();
        string[] allowed = ["IDE/InterpreterRunningProgram.cs", "IDE/InterpreterRunEngine.cs", "DISetup.cs"];
        var names = new Regex(@"\b(EventPump|IModuleExecutionRoot|VBFormRuntime|VBDebugConsole)\b");
        var offenders = new List<string>();
        foreach (var project in new[] { "HexIDE", "HexIDE.Desktop" })
        {
            var directory = Path.Combine(root, "IDE", project);
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(directory, file).Replace('\\', '/');
                if (relative.StartsWith("obj/", StringComparison.Ordinal) || relative.StartsWith("bin/", StringComparison.Ordinal)
                    || project == "HexIDE" && allowed.Contains(relative)) continue;
                if (names.IsMatch(File.ReadAllText(file))) offenders.Add(project + "/" + relative);
            }
        }
        offenders.Should().BeEmpty("running-program questions belong to the contract in Core (#4067)");
    }
}

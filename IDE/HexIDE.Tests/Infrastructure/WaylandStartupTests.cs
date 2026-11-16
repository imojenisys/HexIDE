using System.Diagnostics;

namespace HexIDE.Tests.Infrastructure;

public class WaylandStartupTests
{
    public static bool IsLinux => OperatingSystem.IsLinux();

    [Theory(Skip = "Wayland startup is Linux-only", SkipUnless = nameof(IsLinux))]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Unavailable_display_exits_two_and_writes_the_stderr_line_to_disk(bool standalone, bool environment)
    {
        var directory = Path.Combine(RepoTree.Root(), "artifacts", "wayland-startup-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var launcher = standalone ? typeof(HexIDE.Standalone.Program).Assembly : typeof(HexIDE.Desktop.Program).Assembly;
            var tests = typeof(WaylandStartupTests).Assembly.Location;
            var start = new ProcessStartInfo("dotnet")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = directory,
                ArgumentList =
                {
                    "exec", "--runtimeconfig", Path.ChangeExtension(tests, "runtimeconfig.json"),
                    "--depsfile", Path.ChangeExtension(tests, "deps.json"), launcher.Location
                }
            };
            start.Environment.Remove("WAYLAND_DISPLAY");
            start.Environment.Remove("DISPLAY");
            // Prevent an ambient compositor at wayland-0 from satisfying the connection.
            start.Environment["XDG_RUNTIME_DIR"] = directory;
            start.Environment["XDG_DATA_HOME"] = directory;
            start.Environment["LANG"] = "en_US.UTF-8";
            start.Environment["LC_ALL"] = "en_US.UTF-8";
            start.Environment["HEXIDE_PLATFORM"] = environment ? "wayland" : "x11";
            if (!environment)
            {
                start.ArgumentList.Add("--platform");
                start.ArgumentList.Add("wayland");
            }
            if (!standalone)
            {
                start.ArgumentList.Add("--newproject-unsaved");
                start.ArgumentList.Add("--user-data-dir");
                start.ArgumentList.Add(Path.Combine(directory, "profile"));
            }
            using var process = Process.Start(start)!;
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var finished = process.WaitForExit(60_000);
            if (!finished)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
            finished.Should().BeTrue("startup should refuse without entering the event loop");
            var error = await stderr;
            process.ExitCode.Should().Be(2, error + await stdout);
            var line = error.TrimEnd('\r', '\n');
            line.Should().StartWith("wayland was required, but no Wayland display could be reached: ")
                .And.Contain("Unable to connect to Wayland display")
                .And.EndWith("Use --platform x11 or --platform auto on this session.");
            error.Should().Be(line + Environment.NewLine);
            var log = Directory.GetFiles(Path.Combine(directory, "HexIDE", "logs", "ide"), "*.log")
                .Should().ContainSingle().Subject;
            File.ReadAllText(log).Should().Contain(line + Environment.NewLine);
            Directory.GetFiles(directory, "core*").Should().BeEmpty();
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

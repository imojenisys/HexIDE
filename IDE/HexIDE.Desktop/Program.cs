using System;
using System.IO;
using Avalonia;
using HexIDE.Startup;
using Avalonia.Media.Fonts;
using Classic.CommonControls;

namespace HexIDE.Desktop;

sealed partial class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "MessageBoxW",
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    private static partial int ShowNativeMessageBox(nint owner, string text, string caption, uint type);

    private static string? EstablishDistribution(string installationDirectory)
    {
        try
        {
            HexIDE.IDE.Distribution.Establish(HexIDE.IDE.DistributionDescriptor.Resolve(installationDirectory));
            WindowsTaskbarIdentity.Apply(HexIDE.IDE.Distribution.Current);
            return null;
        }
        catch (HexIDE.IDE.DistributionDescriptorException ex)
        {
            var localization = new HexIDE.Localization.LocalizationService();
            var reason = string.Format(localization.GetStringFrom("system", ex.LocalizationKey), ex.Arguments);
            return string.Format(localization.GetStringFrom("system", "Str.Startup.BrandingError"), reason);
        }
    }

    // A shortcut or Explorer gives this program no console, and on Windows a start that says nothing looks like a hang.
    private static void ReportDistributionError(string message)
    {
        const uint iconError = 0x10;
        if (!ConsoleOutput.Write(message + Environment.NewLine) && OperatingSystem.IsWindows())
        {
            var localization = new HexIDE.Localization.LocalizationService();
            ShowNativeMessageBox(0, message, localization.GetStringFrom("system", "Str.Startup.CannotStart"), iconError);
        }
    }

    [STAThread]
    public static void Main(string[] args)
    {
        // Read before FixCurrentWorkingDictionary moves it. A relative path on the command line means
        // relative to the shell it was typed in; resolved after the move, ../demo/x.vbp pointed inside
        // bin/ and the IDE opened nothing, without a word.
        var invocationDirectory = Environment.CurrentDirectory;
        FixCurrentWorkingDictionary();

        if (RunDistributionGate(args, invocationDirectory, AppContext.BaseDirectory))
            return;

        // Before anything else starts: UserDataPath refuses a redirect once any per-user file has been looked up.
        if (ServerOptions.UserDataDirectory is { } userData)
            HexIDE.IDE.UserDataPath.RedirectTo(userData);

        try
        {
            HexIDE.Static.Profile = ServerOptions.ResolveProfile(invocationDirectory, HexIDE.IDE.UserDataPath.Directory);
        }
        catch (HexIDE.Profiles.ProfileLoadException ex)
        {
            var localization = new HexIDE.Localization.LocalizationService();
            var message = string.Format(localization.GetStringFrom("system", "Str.Profiles.LoadError"), ex.SourceId, ex.File);
            ConsoleOutput.Write(message + Environment.NewLine);
            Environment.ExitCode = 2;
            return;
        }

        DesktopStartup.Register();
        // The lifetime returns the code the app shut down with. Discarding it made every exit 0, including
        // the one a server that could not bind asks for (hexide-io/HexIDE#53).
        var builder = LinuxWindowIdentity.Apply(BuildAvaloniaApp(), HexIDE.IDE.Distribution.Current);
#if DEBUG
        // Avalonia's only scale override on Windows: unaware, Windows tells the process the display is at 96 DPI, so
        // RenderScaling is 1 and Windows stretches the window on screen to the display's scaling (#3083).
        if (ServerOptions.DpiUnaware)
            builder = builder.With(new Win32PlatformOptions { DpiAwareness = Win32DpiAwareness.Unaware });
#endif
        // On a thread of its own, because VB code runs on the UI thread and this first thread's stack is the
        // launcher's choice, 1.5 MiB on Windows (#3228).
        Environment.ExitCode = HexIDE.Runtime.UiThread.Run(() => builder.StartWithClassicDesktopLifetime(args));
    }

    internal static bool RunDistributionGate(string[] args, string invocationDirectory, string installationDirectory,
        Action<string>? write = null, Action<string>? reportError = null)
    {
        var useConsole = write == null;
        write ??= text => ConsoleOutput.Write(text);
        reportError ??= ReportDistributionError;
        // First of all: every per-user path, log and caption is derived from this (#2042). A failure is held until
        // the arguments are read, so that --help still answers on a broken installation.
        var distributionError = EstablishDistribution(installationDirectory);

        ServerOptions.ParseArgs(args, invocationDirectory);

        // Before anything Avalonia touches: --help answers and leaves, so asking what the flags are never
        // starts an IDE, loads a project or opens a port.
        if (ServerOptions.HelpRequested)
        {
            if (distributionError != null)
            {
                write(distributionError + Environment.NewLine);
                Environment.ExitCode = 2;
            }

            if (useConsole)
                ConsoleOutput.Write(ServerOptions.HelpText);
            else
                write(ServerOptions.HelpText(new ConsoleTarget(false, int.MaxValue)));
            return true;
        }

        // Refused rather than started as stock: stock would open HexIDE's own settings and recovery files.
        if (distributionError != null)
        {
            reportError(distributionError);
            Environment.ExitCode = 2;
            return true;
        }

        if (ServerOptions.ProfileArgumentError is { } profileError)
        {
            var localization = new HexIDE.Localization.LocalizationService();
            write(localization.GetStringFrom("system", profileError) + Environment.NewLine);
            Environment.ExitCode = 2;
            return true;
        }

        return RefuseInvalidArguments(write);
    }

    internal static bool RefuseInvalidArguments(Action<string> write)
    {
        if (ServerOptions.UserDataDirectoryMissing)
        {
            write(StartupMessage("Str.Startup.UserDataDirectoryMissing",
                "--user-data-dir", HexIDE.IDE.Distribution.DisplayName) + Environment.NewLine);
            Environment.ExitCode = 2;
            return true;
        }

#if DEBUG
        if (ServerOptions.ServerPortInvalid is { } badPort)
        {
            write(StartupMessage("Str.Startup.ServerPortInvalid",
                "--server-port", 1, 65535, badPort, HexIDE.IDE.Distribution.DisplayName) + Environment.NewLine);
            Environment.ExitCode = 2;
            return true;
        }

        if (ServerOptions.SessionMissing)
        {
            write(StartupMessage("Str.Startup.SessionMissing",
                "--session", HexIDE.IDE.Distribution.DisplayName) + Environment.NewLine);
            Environment.ExitCode = 2;
            return true;
        }

        if (ServerOptions.AnsiCodePageInvalid is { } badCodePage)
        {
            write(StartupMessage("Str.Startup.AnsiCodePageInvalid",
                "--ansi-code-page", 932, badCodePage, HexIDE.IDE.Distribution.DisplayName) + Environment.NewLine);
            Environment.ExitCode = 2;
            return true;
        }

        if (ServerOptions.DpiUnaware && !OperatingSystem.IsWindows())
        {
            write(StartupMessage("Str.Startup.DpiUnaware",
                "--dpi-unaware", HexIDE.IDE.Distribution.DisplayName, "AVALONIA_GLOBAL_SCALE_FACTOR") + Environment.NewLine);
            Environment.ExitCode = 2;
            return true;
        }
#endif

        return false;
    }

    private static string StartupMessage(string key, params object[] arguments)
    {
        var localization = new HexIDE.Localization.LocalizationService();
        return string.Format(localization.GetStringFrom("system", key), arguments);
    }

    private static void FixCurrentWorkingDictionary()
    {
        if (Path.GetDirectoryName(Environment.ProcessPath) is { } dir)
        {
            Environment.CurrentDirectory = dir;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseDisplayBackend(ServerOptions.Platform)
            .UseMessageBoxSounds()
            .LogToTrace()
            .ConfigureFonts(manager =>
            {
                manager.AddFontCollection(new EmbeddedFontCollection(new Uri("fonts:App", UriKind.Absolute),
                    new Uri("avares://HexIDE/Resources", UriKind.Absolute)));
            });
}

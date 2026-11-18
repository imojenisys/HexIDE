using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Serilog;

namespace HexIDE.Infrastructure;

internal sealed class ExitStop(
    IClassicDesktopStyleApplicationLifetime desktop,
    Action disposeFileWatcher,
    Func<Task> stop,
    Action shutdownLogging,
    TimeSpan timeout)
{
    // One server: shutdown 2 s + exit 2 s + output drain 1 s (#4484), with 1 s scheduling room.
    internal static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(6);
    private bool _stopping;
    private bool _stopped;

    internal void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (_stopped || e.Cancel) return;
        e.Cancel = true;
        if (_stopping) return;
        _stopping = true;
        _ = StopThenShutdownAsync();
    }

    private async Task StopThenShutdownAsync()
    {
        // Even a synchronous stop must retry after the first ShutdownRequested has returned.
        await Task.Yield();
        try
        {
            disposeFileWatcher();
            await stop().WaitAsync(timeout);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Language server stop failed during IDE exit (limit {Timeout}) — exiting", timeout);
        }
        finally
        {
            shutdownLogging();
            _stopped = true;
            desktop.TryShutdown();
        }
    }
}

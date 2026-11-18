using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using HexIDE.Lsp;
using HexIDE.Lsp.Messages;
using Microsoft.Extensions.Logging.Abstractions;

namespace HexIDE.Tests.LspClient;

public sealed class WatchedFileBatchTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly ConcurrentQueue<DidChangeWatchedFilesParams> _sent = new();
    private readonly WatchedFileRegistrations _watcher;
    private readonly string _path;

    public WatchedFileBatchTests()
    {
        Directory.CreateDirectory(_root);
        _path = Path.Combine(_root, "file.txt");
        File.WriteAllText(_path, "old");
        _watcher = CreateWatcher(true);
    }

    private WatchedFileRegistrations CreateWatcher(bool correction)
    {
        // Reflection lets these same tests exercise the pre-correction implementation for teeth.
        var constructor = typeof(WatchedFileRegistrations).GetConstructors().Single();
        object[] args = [new Func<string[]>(() => [_root]),
            new Func<DidChangeWatchedFilesParams, Task>(p => { _sent.Enqueue(p); return Task.CompletedTask; }),
            NullLogger.Instance];
        if (constructor.GetParameters().Length > 3) args = [.. args, correction];
        return (WatchedFileRegistrations)constructor.Invoke(args);
    }

    private void Register(int kind = 7) => _watcher.Register(JsonSerializer.SerializeToElement(new
    {
        registrations = new[] { new { id = "test", method = "workspace/didChangeWatchedFiles",
            registerOptions = new { watchers = new[] { new { globPattern = "*.txt", kind } } } } }
    }));

    private void Disk(int type, WatchedFileRegistrations? watcher = null)
    {
        watcher ??= _watcher;
        var entry = typeof(WatchedFileRegistrations).GetMethod("NotifyFromDisk", BindingFlags.Instance | BindingFlags.NonPublic);
        if (entry == null) watcher.Notify(_path, type);
        else entry.Invoke(watcher, [_path, type, _root]);
    }

    private async Task<int[]> Batch()
    {
        await Task.Delay(400, TestContext.Current.CancellationToken);
        var events = _sent.ToArray();
        _sent.Clear();
        return events.SelectMany(p => p.Changes).Select(e => e.Type).ToArray();
    }

    [Fact]
    public async Task ExistingFileEditIsChanged()
    {
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Register();
        Disk(2); Disk(1);
        (await Batch()).Should().Equal(2);
    }

    [Fact]
    public async Task NewFileThenEditThenRecreation()
    {
        File.Delete(_path);
        Register();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        File.WriteAllText(_path, "new");
        // Windows can tunnel the deleted file's birth time when its name is reused (#3650).
        // This scenario needs a genuinely new birth time, as measured on APFS.
        File.SetCreationTimeUtc(_path, DateTime.UtcNow);
        Disk(2); Disk(1);
        (await Batch()).Should().Equal(1);
        Disk(2); Disk(1);
        (await Batch()).Should().Equal(2);
        File.Delete(_path);
        Disk(3);
        (await Batch()).Should().Equal(3);
        File.WriteAllText(_path, "recreated");
        File.SetCreationTimeUtc(_path, DateTime.UtcNow);
        Disk(2); Disk(1);
        (await Batch()).Should().Equal(1);
    }

    [Fact]
    public async Task CreatedOnlyDoesNotReceiveAnEdit()
    {
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Register(1);
        Disk(2); Disk(1);
        (await Batch()).Should().BeEmpty();
    }

    [Fact]
    public async Task IdeCreatedRemainsCreated()
    {
        Register();
        _watcher.Notify(_path, 1);
        (await Batch()).Should().Equal(1);
    }

    [Fact]
    public async Task CorrectionOffRetainsLastCreated()
    {
        using var watcher = CreateWatcher(false);
        watcher.Register(JsonSerializer.SerializeToElement(new { registrations = new[] { new { id = "off",
            method = "workspace/didChangeWatchedFiles", registerOptions = new { watchers = new[] { new { globPattern = "*.txt" } } } } } }));
        Disk(2, watcher); Disk(1, watcher);
        (await Batch()).Should().Equal(1);
    }

    [Fact]
    public async Task MissingFileRetainsReportedType()
    {
        File.Delete(_path);
        Register();
        Disk(2); Disk(1);
        (await Batch()).Should().Equal(1);
    }

    public void Dispose()
    {
        _watcher.Dispose();
        Directory.Delete(_root, true);
    }
}

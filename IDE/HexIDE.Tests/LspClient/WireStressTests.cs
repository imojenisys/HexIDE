using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using HexIDE.Lsp;
using HexIDE.Lsp.Messages;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using StreamJsonRpc;

namespace HexIDE.Tests.LspClient;

public class WireStressTests(ITestOutputHelper output)
{
    private sealed class Ledger
    {
        public readonly ConcurrentQueue<string> Failures = new();
        public readonly Dictionary<string, Queue<byte[]>> Starts = [];
        public long Frames;
        public readonly ConcurrentDictionary<string, int> Methods = new();

        public void Remember(string side, byte[] bytes)
        {
            lock (Starts)
            {
                if (!Starts.TryGetValue(side, out var entries)) Starts[side] = entries = new();
                entries.Enqueue(bytes);
                while (entries.Count > 64) entries.Dequeue();
            }
        }

        public string Matches(ReadOnlySpan<byte> bytes)
        {
            var matches = new List<string>();
            var copy = bytes.ToArray();
            lock (Starts)
                foreach (var (side, entries) in Starts)
                    if (entries.Any(start => copy.AsSpan().StartsWith(start)))
                        matches.Add(side);
            return matches.Count == 0 ? "none" : string.Join(", ", matches);
        }
    }

    // Independent incremental framing: never alter the chunks handed to StreamJsonRpc.
    private sealed class Frames(Ledger ledger, string side, bool writing)
    {
        private readonly List<byte> header = [];
        private readonly Queue<byte> recent = new();
        private byte[]? body;
        private int used;
        private bool failed;
        private static string Escape(byte[] bytes) => string.Concat(bytes.Select(b => b switch
        {
            13 => "\\r", 10 => "\\n", 9 => "\\t", 92 => "\\\\",
            >= 32 and <= 126 => ((char)b).ToString(),
            _ => "\\x" + b.ToString("X2", CultureInfo.InvariantCulture)
        }));

        public void Feed(ReadOnlySpan<byte> bytes)
        {
            lock (this)
                foreach (var b in bytes)
                {
                    recent.Enqueue(b);
                    if (recent.Count > 4096) recent.Dequeue();
                    if (failed) continue;
                    if (body is null)
                    {
                        header.Add(b);
                        if (header.Count > 8192) { Bad(0, "header too long", [.. header]); continue; }
                        if (header.Count < 4 || !header.TakeLast(4).SequenceEqual(new byte[] { 13, 10, 13, 10 })) continue;
                        var lines = Encoding.ASCII.GetString([.. header]).Split("\r\n")
                            .Where(l => l.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)).ToArray();
                        if (lines.Length != 1 || !int.TryParse(lines[0][15..].Trim(), CultureInfo.InvariantCulture, out var length)
                            || length <= 0 || length > 4 * 1024 * 1024)
                        { Bad(0, "invalid Content-Length", [.. header]); continue; }
                        body = new byte[length];
                        used = 0;
                        if (writing) ledger.Remember(side, [.. header]);
                    }
                    else
                    {
                        body[used++] = b;
                        if (used != body.Length) continue;
                        try
                        {
                            using var json = JsonDocument.Parse(body);
                            if (!writing)
                            {
                                Interlocked.Increment(ref ledger.Frames);
                                if (json.RootElement.ValueKind == JsonValueKind.Object
                                    && json.RootElement.TryGetProperty("method", out var method))
                                    ledger.Methods.AddOrUpdate(method.GetString()!, 1, (_, count) => count + 1);
                            }
                        }
                        catch (JsonException e)
                        {
                            var offset = 0;
                            for (long line = 0; line < e.LineNumber; offset++)
                                if (body[offset] == 10) line++;
                            offset += (int)(e.BytePositionInLine ?? 0);
                            Bad(offset, e.Message, body);
                        }
                        body = null;
                        header.Clear();
                    }
                }
        }

        private void Bad(int offset, string reason, byte[] bytes)
        {
            failed = true;
            var at = Math.Clamp(offset, 0, bytes.Length);
            ledger.Failures.Enqueue($"#3520 pair/side {side} {(writing ? "write" : "read")}: header={Escape([.. header])}; "
                + $"offset={offset}; around={Escape(bytes[Math.Max(0, at - 64)..Math.Min(bytes.Length, at + 128)])}; "
                + $"frame-start matches={ledger.Matches(bytes.AsSpan(at))}; reason={reason}; last4096={Escape([.. recent])}");
        }

        public void End()
        {
            lock (this)
                if (!failed && (header.Count != 0 || body is not null))
                    Bad(used, "truncated frame at EOF", body is null ? [.. header] : body[..used]);
        }
    }

    private sealed class CheckingStream(Stream inner, Ledger ledger, string side) : Stream
    {
        private readonly Frames reads = new(ledger, side, false);
        private readonly Frames writes = new(ledger, side, true);
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var n = await inner.ReadAsync(buffer, cancellationToken);
            reads.Feed(buffer.Span[..n]);
            if (n == 0) reads.End();
            return n;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override int Read(byte[] buffer, int offset, int count)
        {
            var n = inner.Read(buffer, offset, count);
            reads.Feed(buffer.AsSpan(offset, n));
            if (n == 0) reads.End();
            return n;
        }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            writes.Feed(buffer.Span);
            return inner.WriteAsync(buffer, cancellationToken);
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override void Write(byte[] buffer, int offset, int count)
        { writes.Feed(buffer.AsSpan(offset, count)); inner.Write(buffer, offset, count); }
        public override void Flush() => inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    }

    private sealed class Server
    {
        [JsonRpcMethod("initialize", UseSingleObjectParameterDeserialization = true)]
        public JsonElement Initialize(JsonElement _) => JsonSerializer.SerializeToElement(new Dictionary<string, object>
        {
            ["capabilities"] = new Dictionary<string, object> { ["textDocumentSync"] = 1, ["hoverProvider"] = true, ["documentSymbolProvider"] = true }
        });
        [JsonRpcMethod("initialized")] public void Initialized(JsonElement _) { }
        [JsonRpcMethod("textDocument/didOpen")] public void Open(JsonElement _) { }
        [JsonRpcMethod("textDocument/didChange")] public void Change(JsonElement _) { }
        [JsonRpcMethod("textDocument/didClose")] public void Close(JsonElement _) { }
        [JsonRpcMethod("textDocument/hover", UseSingleObjectParameterDeserialization = true)]
        public async Task<JsonElement> Hover(JsonElement _, CancellationToken cancellationToken)
        {
            await Task.Delay(5, cancellationToken);
            return JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["contents"] = "stress" });
        }
        [JsonRpcMethod("textDocument/documentSymbol", UseSingleObjectParameterDeserialization = true)]
        public JsonElement Symbols(JsonElement _) => JsonSerializer.SerializeToElement(Array.Empty<int>());
        [JsonRpcMethod("shutdown")] public object? Shutdown() => null;
        [JsonRpcMethod("exit")] public void Exit() { }
    }

    // #3520 experiment: a pool that never reuses a block. On return it fills the block with 0xDD and quarantines it;
    // a block found changed later was written after its return, and the report carries the stack that returned it.
    private sealed class PoisonPool(string name) : System.Buffers.MemoryPool<byte>
    {
        public static readonly ConcurrentQueue<string> Violations = new();
        public string Name => name;
        public long Rented, Returned, Checked;
        private readonly ConcurrentQueue<Owner> quarantine = new();
        public override int MaxBufferSize => int.MaxValue;
        public override System.Buffers.IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            Interlocked.Increment(ref Rented);
            return new Owner(this, new byte[Math.Max(minBufferSize, 4096)]);
        }
        protected override void Dispose(bool disposing) { }
        public void VerifyAll() { while (quarantine.TryDequeue(out var o)) o.Verify(); }
        public override string ToString() => $"{name}: rented={Rented}, returned={Returned}, checked={Checked}";
        private void OnReturn(Owner owner)
        {
            Interlocked.Increment(ref Returned);
            quarantine.Enqueue(owner);
            while (quarantine.Count > 4096 && quarantine.TryDequeue(out var old)) old.Verify();
        }
        private sealed class Owner(PoisonPool pool, byte[] array) : System.Buffers.IMemoryOwner<byte>
        {
            private int returned;
            private StackTrace? returnedAt;
            private int firstThread;
            public Memory<byte> Memory => array;
            public void Dispose()
            {
                if (Interlocked.Exchange(ref returned, 1) != 0)
                {
                    SpinWait.SpinUntil(() => Volatile.Read(ref returnedAt) is not null, 500);
                    if (Violations.Count < 20) Violations.Enqueue($"{pool.Name}: returned twice. FIRST return (thread {firstThread}):{Environment.NewLine}{returnedAt}{Environment.NewLine}SECOND return (thread {Environment.CurrentManagedThreadId}):{Environment.NewLine}{new StackTrace(1, false)}");
                    return;
                }
                returnedAt = new StackTrace(1, false);
                firstThread = Environment.CurrentManagedThreadId;
                array.AsSpan().Fill(0xDD);
                pool.OnReturn(this);
            }
            public void Verify()
            {
                Interlocked.Increment(ref pool.Checked);
                var at = array.AsSpan().IndexOfAnyExcept((byte)0xDD);
                if (at >= 0 && Violations.Count < 20)
                    Violations.Enqueue($"{pool.Name}: written after return at {at} of {array.Length}: '{Encoding.ASCII.GetString(array, at, Math.Min(40, array.Length - at))}'; returned at:{Environment.NewLine}{returnedAt}");
            }
        }
    }

    private static readonly PoisonPool ReaderPool = new("reader pool"), WriterPool = new("writer pool");

    // #3520, the product-shaped fix under test: a pool whose owners hand their array back at most once. The poison
    // check caught StreamPipeReader disposing one owner twice as a connection ends; on a plain pool that returns the
    // array twice and two later renters share it. Here the second dispose is a counted no-op.
    private sealed class GuardedPool(System.Buffers.ArrayPool<byte> arrays) : System.Buffers.MemoryPool<byte>
    {
        public static long Rents, Suppressed, RentsAfterSuppressed;
        private int suppressedHere;
        private System.Buffers.ArrayPool<byte> Arrays => arrays;
        public override int MaxBufferSize => int.MaxValue;
        public override System.Buffers.IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            Interlocked.Increment(ref Rents);
            if (Volatile.Read(ref suppressedHere) != 0) Interlocked.Increment(ref RentsAfterSuppressed);
            return new Owner(this, arrays.Rent(Math.Max(minBufferSize, 4096)));
        }
        protected override void Dispose(bool disposing) { }
        public static string Report() => $"guarded: rents={Rents}, second disposes suppressed={Suppressed}, rents from that pool after one={RentsAfterSuppressed}";
        private sealed class Owner(GuardedPool pool, byte[] rented) : System.Buffers.IMemoryOwner<byte>
        {
            private readonly byte[] array = rented;
            private byte[]? held = rented;
            public Memory<byte> Memory => array;
            public void Dispose()
            {
                if (Interlocked.Exchange(ref held, null) is { } a) { pool.Arrays.Return(a); return; }
                Interlocked.Increment(ref Suppressed);
                Volatile.Write(ref pool.suppressedHere, 1);
            }
        }
    }

    private static readonly GuardedPool SharedGuarded = new(System.Buffers.ArrayPool<byte>.Shared);

    private static async Task<(Stream Client, Stream Server)> TcpPair(System.Net.Sockets.TcpListener listener, CancellationToken token)
    {
        var client = new System.Net.Sockets.TcpClient { NoDelay = true };
        var accepting = listener.AcceptTcpClientAsync(token).AsTask();
        await client.ConnectAsync(System.Net.IPAddress.Loopback, ((System.Net.IPEndPoint)listener.LocalEndpoint).Port, token);
        var server = await accepting;
        server.NoDelay = true;
        return (client.GetStream(), server.GetStream());
    }

    private static int Setting(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var n) && n > 0 ? n : fallback;

    // Opt-in: the load deliberately occupies thread-pool workers, and test classes run in parallel, so in the
    // default suite it could starve the in-memory pairs of other classes, which is the kind of flake it hunts.
    public static bool StressRequested => Setting("HEXIDE_WIRE_STRESS_SECONDS", 0) > 0;

    [Fact(Skip = "Set HEXIDE_WIRE_STRESS_SECONDS to run the wire stress test (#3520)", SkipUnless = nameof(StressRequested))]
    public async Task ConcurrentConnectionsKeepEveryFrameValid()
    {
        var seconds = Setting("HEXIDE_WIRE_STRESS_SECONDS", 3);
        var pairs = Setting("HEXIDE_WIRE_STRESS_PAIRS", 8);
        var ledger = new Ledger();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(seconds + 30));
        var token = deadline.Token;
        var clients = new List<VBLspClient>();
        var peers = new List<JsonRpc>();
        using var release = new ManualResetEventSlim();
        var blockers = new List<Task>();
        Thread? releaser = null;
        try
        {
            var checkedTcp = Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHECKED_TCP") == "1";
            using var checkedListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            if (checkedTcp) checkedListener.Start();
            IJsonRpcMessageHandler CheckedHandler(Stream s, IJsonRpcMessageFormatter f) => checkedTcp
                ? new HeaderDelimitedMessageHandler(
                    System.IO.Pipelines.PipeWriter.Create(s, new System.IO.Pipelines.StreamPipeWriterOptions(leaveOpen: true)),
                    System.IO.Pipelines.PipeReader.Create(s, new System.IO.Pipelines.StreamPipeReaderOptions(leaveOpen: true)), f)
                : new HeaderDelimitedMessageHandler(s, s, f);
            for (var i = 0; i < pairs; i++)
            {
                var (local, remote) = checkedTcp ? await TcpPair(checkedListener, token) : FullDuplexStream.CreatePair();
                var a = new CheckingStream(local, ledger, $"{i}/client");
                var b = new CheckingStream(remote, ledger, $"{i}/server");
                var rpc = new JsonRpc(CheckedHandler(b, new SystemTextJsonFormatter()), new Server());
                peers.Add(rpc);
                rpc.StartListening();
                var transport = Substitute.For<ILspTransport>();
                transport.IsAlive.Returns(true);
                transport.LeaveServerRunning.Returns(true);
                transport.ConnectAsync(Arg.Any<IJsonRpcMessageFormatter>(), Arg.Any<CancellationToken>())
                    .Returns(c => Task.FromResult<IJsonRpcMessageHandler?>(CheckedHandler(a, c.Arg<IJsonRpcMessageFormatter>())));
                clients.Add(new VBLspClient(transport, Substitute.For<ILogger<VBLspClient>>(), "vb6"));
            }
            await Task.WhenAll(clients.Select(c => c.StartAsync(token)));
            clients.Should().OnlyContain(c => c.IsRunning);
            var clock = Stopwatch.StartNew();
            // Churn (#3520 experiment): unchecked pairs torn down while both sides are mid-write, beside the checked pairs.
            var churnRounds = 0L;
            var churners = Enumerable.Range(0, Setting("HEXIDE_WIRE_STRESS_CHURN", 0)).Select(n => Task.Run(async () =>
            {
                var tcp = Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_TCP") == "1";
                // Stdio's shape: our side is main's handler over two one-way OS pipes (the server's stdin and stdout). The
                // peer stands in for a separate process: its own buffers, no pooling. It pushes framed notifications while
                // draining its input, and on end of input it stops pushing and closes its output, as a server does on exit.
                // ORDER 1 disposes our connection while the peer still writes (main's stop); ORDER 4 finishes our writes,
                // closes our write side, waits (bounded) for the read side to end, then disposes.
                async Task StdioRound(int round)
                {
                    var order = int.TryParse(Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_ORDER"), out var o) ? o : 1;
                    var text = new string('z', 8192 + round % 8192);
                    using var toPeer = new System.IO.Pipes.AnonymousPipeServerStream(System.IO.Pipes.PipeDirection.Out);
                    using var peerIn = new System.IO.Pipes.AnonymousPipeClientStream(System.IO.Pipes.PipeDirection.In, toPeer.ClientSafePipeHandle);
                    using var fromPeer = new System.IO.Pipes.AnonymousPipeServerStream(System.IO.Pipes.PipeDirection.In);
                    using var peerOut = new System.IO.Pipes.AnonymousPipeClientStream(System.IO.Pipes.PipeDirection.Out, fromPeer.ClientSafePipeHandle);
                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { jsonrpc = "2.0", method = "window/logMessage", @params = new { type = 4, message = text } }));
                    var frame = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n").Concat(body).ToArray();
                    using var endOfInput = new ManualResetEventSlim();
                    var drain = Task.Factory.StartNew(() =>
                    {
                        var buffer = new byte[65536];
                        try { while (peerIn.Read(buffer, 0, buffer.Length) > 0) { } } catch (IOException) { } catch (ObjectDisposedException) { }
                        endOfInput.Set();
                    }, TaskCreationOptions.LongRunning);
                    var pump = Task.Factory.StartNew(() =>
                    {
                        try { for (var k = 0; k < 64 && !endOfInput.IsSet; k++) peerOut.Write(frame); } catch (IOException) { } catch (ObjectDisposedException) { }
                        endOfInput.Wait(TimeSpan.FromSeconds(10));
                        peerOut.Dispose();
                    }, TaskCreationOptions.LongRunning);
                    var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(toPeer, fromPeer, new SystemTextJsonFormatter()));
                    rpc.StartListening();
                    var sends = Task.Run(async () => { for (var k = 0; k < 64; k++) await rpc.NotifyAsync("textDocument/didChange", text); });
                    await Task.Delay(round % 3);
                    if (order == 4)
                    {
                        try { await sends; } catch { }
                        toPeer.Dispose();
                        await Task.WhenAny(rpc.Completion, Task.Delay(TimeSpan.FromSeconds(5)));
                        rpc.Dispose();
                    }
                    else
                    {
                        rpc.Dispose();
                        try { await sends; } catch { }
                    }
                    toPeer.Dispose();
                    fromPeer.Dispose();
                    await Task.WhenAny(Task.WhenAll(drain, pump), Task.Delay(TimeSpan.FromSeconds(15)));
                }
                using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
                if (tcp) listener.Start();
                for (var round = 0; clock.Elapsed.TotalSeconds < seconds && !token.IsCancellationRequested; round++)
                {
                    if (Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_STDIO") == "1")
                    {
                        await StdioRound(round);
                        Interlocked.Increment(ref churnRounds);
                        continue;
                    }
                    var pipes = Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_PIPES") == "1";
                    var (local, remote) = tcp ? await TcpPair(listener, token) : FullDuplexStream.CreatePair();
                    var toServer = new System.IO.Pipelines.Pipe();
                    var toClient = new System.IO.Pipelines.Pipe();
                    var adapter = Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_ADAPTER");
                    var privatePool = Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_PRIVATE");
                    IJsonRpcMessageHandler Over(Stream stream) => adapter switch
                    {
                        "nbwriter" => new HeaderDelimitedMessageHandler(stream.UsePipeWriter(),
                            System.IO.Pipelines.PipeReader.Create(stream, new System.IO.Pipelines.StreamPipeReaderOptions(leaveOpen: true)), new SystemTextJsonFormatter()),
                        "netwriter" => new HeaderDelimitedMessageHandler(
                            System.IO.Pipelines.PipeWriter.Create(stream, new System.IO.Pipelines.StreamPipeWriterOptions(leaveOpen: true)),
                            System.IO.Pipelines.PipeReader.Create(stream, new System.IO.Pipelines.StreamPipeReaderOptions(leaveOpen: true)), new SystemTextJsonFormatter()),
                        // As lane I's class does: the writer owns the stream, so completing it closes the stream at once,
                        // possibly while this handler's own read is still pending on it.
                        "netowned" => new HeaderDelimitedMessageHandler(
                            System.IO.Pipelines.PipeWriter.Create(stream, new System.IO.Pipelines.StreamPipeWriterOptions(pool: privatePool == "writer" ? WriterPool : null, leaveOpen: false)),
                            System.IO.Pipelines.PipeReader.Create(stream, new System.IO.Pipelines.StreamPipeReaderOptions(pool: privatePool switch
                            {
                                "reader" => ReaderPool,
                                "guarded" => SharedGuarded,
                                "guarded-conn" => new GuardedPool(System.Buffers.ArrayPool<byte>.Create()),
                                _ => null,
                            }, leaveOpen: true)), new SystemTextJsonFormatter()),
                        "nbreader" => new HeaderDelimitedMessageHandler(
                            System.IO.Pipelines.PipeWriter.Create(stream, new System.IO.Pipelines.StreamPipeWriterOptions(leaveOpen: true)),
                            stream.UsePipeReader(), new SystemTextJsonFormatter()),
                        _ => new HeaderDelimitedMessageHandler(stream, stream, new SystemTextJsonFormatter()),
                    };
                    var peer = pipes
                        ? new JsonRpc(new HeaderDelimitedMessageHandler(toClient.Writer, toServer.Reader, new SystemTextJsonFormatter()), new Server())
                        : new JsonRpc(Over(remote), new Server());
                    peer.StartListening();
                    var transport = Substitute.For<ILspTransport>();
                    transport.IsAlive.Returns(true);
                    transport.LeaveServerRunning.Returns(true);
                    transport.ConnectAsync(Arg.Any<IJsonRpcMessageFormatter>(), Arg.Any<CancellationToken>())
                        .Returns(c => Task.FromResult<IJsonRpcMessageHandler?>(new HeaderDelimitedMessageHandler(local, local, c.Arg<IJsonRpcMessageFormatter>())));
                    var raw = pipes || tcp || Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_RAW") == "1";
                    var order = int.TryParse(Environment.GetEnvironmentVariable("HEXIDE_WIRE_STRESS_CHURN_ORDER"), out var o) ? o : -1;
                    var client = new VBLspClient(transport, Substitute.For<ILogger<VBLspClient>>(), "vb6");
                    var rawClient = !raw ? null : pipes
                        ? new JsonRpc(new HeaderDelimitedMessageHandler(toServer.Writer, toClient.Reader, new SystemTextJsonFormatter()))
                        : new JsonRpc(Over(local));
                    try
                    {
                        var uri = $"file:///churn-{n}.bas";
                        var text = new string('z', 8192 + round % 8192);
                        if (rawClient is not null)
                        {
                            rawClient.StartListening();
                            await rawClient.InvokeWithParameterObjectAsync<JsonElement>("initialize", JsonSerializer.SerializeToElement(new { capabilities = new { } }), token);
                        }
                        else
                        {
                            await client.StartAsync(token);
                            await client.OpenDocumentAsync(uri, text, token);
                        }
                        var pushes = Task.Run(async () =>
                        {
                            for (var k = 0; k < 64; k++)
                                await peer.NotifyWithParameterObjectAsync("window/logMessage", JsonSerializer.SerializeToElement(new { type = 3, message = text }));
                        });
                        var sends = Task.Run(async () =>
                        {
                            for (var k = 1; k <= 64; k++)
                                if (rawClient is not null)
                                    await rawClient.NotifyWithParameterObjectAsync("textDocument/didChange", JsonSerializer.SerializeToElement(new { textDocument = new { uri, version = k }, contentChanges = new[] { new { text } } }));
                                else
                                    await client.ChangeDocumentAsync(uri, k, text, token);
                        });
                        await Task.Delay(round % 3, token);
                        async Task CloseClient() { rawClient?.Dispose(); await client.DisposeAsync(); }
                        switch (order >= 0 ? order : pipes ? round % 2 : round % 3)
                        {
                            case 0: peer.Dispose(); await CloseClient(); break;
                            case 1: await CloseClient(); peer.Dispose(); break;
                            case 2: local.Dispose(); remote.Dispose(); peer.Dispose(); await CloseClient(); break;
                            default: // control: let both sides finish writing, then close
                                try { await Task.WhenAll(pushes, sends); } catch { }
                                await CloseClient(); peer.Dispose(); break;
                        }
                        if (adapter is not null and not "netowned")
                        {
                            // The (PipeWriter, PipeReader) handlers don't own the stream: close it once both ends are done, as
                            // the Stream constructor's handler does after its writer has flushed.
                            try { await Task.WhenAll(peer.Completion, rawClient?.Completion ?? Task.CompletedTask).WaitAsync(TimeSpan.FromSeconds(5), token); } catch { }
                            local.Dispose();
                            remote.Dispose();
                        }
                        try { await Task.WhenAll(pushes, sends); } catch { /* torn down mid-write on purpose */ }
                    }
                    catch (Exception) when (!token.IsCancellationRequested) { /* the churn pair itself may fail; only checked pairs count */ }
                    finally
                    {
                        peer.Dispose();
                        rawClient?.Dispose();
                        try { await client.DisposeAsync(); } catch { /* already disposed */ }
                    }
                    Interlocked.Increment(ref churnRounds);
                }
            }, token)).ToArray();
            var load = Task.WhenAll(clients.Select(async (client, i) =>
            {
                var version = 0;
                while (clock.Elapsed.TotalSeconds < seconds)
                {
                    var uri = $"file:///stress-{i}-{version / 4}.bas";
                    var text = new string((char)('a' + i % 26), 16384 + (int)((version * 7919L + i * 997) % 49153));
                    await client.OpenDocumentAsync(uri, text, token);
                    await client.ChangeDocumentAsync(uri, ++version, text + version, token);
                    using var cancel = CancellationTokenSource.CreateLinkedTokenSource(token);
                    var hover = client.RequestHoverAsync(uri, new Position(0, 0), cancel.Token);
                    if (version % 2 == 0) cancel.Cancel();
                    var rpc = peers[i];
                    await Task.WhenAll(hover, client.RequestDocumentSymbolsAsync(uri, token),
                        rpc.InvokeWithParameterObjectAsync<JsonElement>("workspace/configuration", JsonSerializer.SerializeToElement(new { items = new[] { new { section = "stress" } } }), token),
                        rpc.InvokeWithParameterObjectAsync<JsonElement>("workspace/applyEdit", JsonSerializer.SerializeToElement(new { edit = new { changes = new Dictionary<string, object>() } }), token),
                        rpc.NotifyWithParameterObjectAsync("window/logMessage", JsonSerializer.SerializeToElement(new { type = 3, message = text[..256] })));
                    if (version % 4 == 0) await client.CloseDocumentAsync(uri, token);
                }
            }));
            // Occupy a bounded set of workers during the middle of the load, releasing from a dedicated thread.
            for (var i = 0; i < Math.Min(16, Math.Max(4, Environment.ProcessorCount)); i++)
                blockers.Add(Task.Run(() => release.Wait(token), token));
            releaser = new Thread(() => { release.Wait(TimeSpan.FromSeconds(seconds / 2.0)); release.Set(); });
            releaser.Start();
            await load;
            await Task.WhenAll(churners);
            output.WriteLine($"#3520: churn rounds={Interlocked.Read(ref churnRounds)}");
            await Task.WhenAll(blockers);
        }
        finally
        {
            release.Set();
            releaser?.Join();
            foreach (var client in clients) await client.DisposeAsync();
            foreach (var peer in peers) peer.Dispose();
            output.WriteLine($"#3520: pairs={pairs}; seconds={seconds}; checked frames={ledger.Frames}; bad={ledger.Failures.Count}");
            using (var self = Process.GetCurrentProcess())
                output.WriteLine($"#3520: memory after full GC={GC.GetTotalMemory(true) / 1048576} MB; peak working set={self.PeakWorkingSet64 / 1048576} MB; pool threads={ThreadPool.ThreadCount}; allocated={GC.GetTotalAllocatedBytes() / 1048576} MB");
            if (Setting("HEXIDE_WIRE_STRESS_HOLD_SECONDS", 0) is var hold and > 0)
            {
                // For a heap dump of what the run left behind: the marker file names this process.
                File.WriteAllText(Path.Combine(Path.GetTempPath(), $"hexide-3520-hold-{Environment.ProcessId}.txt"), "holding");
                Thread.Sleep(TimeSpan.FromSeconds(hold));
            }
            ReaderPool.VerifyAll();
            WriterPool.VerifyAll();
            output.WriteLine($"#3520: {ReaderPool}; {WriterPool}; violations={PoisonPool.Violations.Count}; {GuardedPool.Report()}");
            foreach (var violation in PoisonPool.Violations.Take(6)) output.WriteLine($"#3520 poison: {violation}");
            ledger.Failures.Should().BeEmpty(string.Join(Environment.NewLine, ledger.Failures));
        }
        ledger.Frames.Should().BeGreaterThan(pairs * 10);
        foreach (var method in new[] { "textDocument/didOpen", "textDocument/didChange", "textDocument/hover",
                     "textDocument/documentSymbol", "$/cancelRequest", "workspace/configuration", "workspace/applyEdit", "window/logMessage" })
            ledger.Methods.Should().ContainKey(method);
    }

    [Fact]
    public void CheckerAcceptsFragmentedAndCoalescedFramesAndMatchesForeignHeaders()
    {
        var ledger = new Ledger();
        var frame = Encoding.UTF8.GetBytes("Content-Length: 10\r\n\r\n{\"x\":\"é\"}");
        var writer = new Frames(ledger, "7/client", true);
        writer.Feed(frame);
        var reader = new Frames(ledger, "7/server", false);
        foreach (var b in frame) reader.Feed(new byte[] { b });
        reader.Feed(frame.Concat(frame).ToArray());
        reader.End();
        ledger.Failures.Should().BeEmpty();
        ledger.Frames.Should().Be(3);
        ledger.Matches(frame).Should().Be("7/client");
        ledger.Matches(Encoding.ASCII.GetBytes("unrelated")).Should().Be("none");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckerDetectsCorruptAndTruncatedFrames(bool truncated)
    {
        var ledger = new Ledger();
        var frames = new Frames(ledger, "3/server", false);
        frames.Feed(Encoding.ASCII.GetBytes("Content-Length: 20\r\n\r\n" + (truncated ? "{" : "{\"x\":Content-Length: ")));
        frames.End();
        ledger.Failures.Should().ContainSingle().Which.Should().Contain("3/server").And.Contain("offset=").And.Contain("last4096=");
    }
}

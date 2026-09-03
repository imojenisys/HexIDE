using HexIDE.Lsp;
using HexIDE.Lsp.Messages;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;

// NB: namespace deliberately avoids a `Lsp` segment — see VBLspClientTests.
namespace HexIDE.Tests.LspClient;

/// <summary>
/// Guards the LSP client's serialization boundary.
///
/// <para>
/// HexIDE speaks LSP to servers it did not write, and the shapes on the wire are defined by the
/// <em>protocol</em> — not by whatever library the far end happens to use. That distinction is the
/// whole of the replaceable-backend claim: the moment a foreign server's DTOs reach our types, the
/// backend is only replaceable with that server.
/// </para>
///
/// <para>
/// The concrete hazard is measured, not hypothetical. A real third-party VB-family server exposes a
/// custom method whose payload is its own in-process model serialised by OmniSharp's Newtonsoft-based
/// <c>LspSerializer</c>. Consuming it would mean adopting both its object model and its serializer.
/// And it would not even work: that payload does not round-trip <b>with OmniSharp's serializer on
/// both ends</b> — no discriminator is emitted for its abstract node type — so the dependency buys
/// nothing. Whatever we want from such a server has to arrive as standard LSP.
/// </para>
///
/// <para>
/// Two different instruments, deliberately. The OmniSharp guard reads the compiled assembly manifest
/// rather than the package graph, because the compiler emits a reference only for an assembly actually
/// <em>used</em> — so it catches binding, not mere presence. The formatter guard instead asserts the
/// object the client really builds.
/// </para>
///
/// <para>
/// Neither is phrased as "Newtonsoft is absent from the closure", and that is the point: it is
/// <em>present</em> today because StreamJsonRpc brings it, and that is a fact about StreamJsonRpc
/// which is visibly in motion — its closure currently carries both the old <c>MessagePack</c> and the
/// newer <c>Nerdbank.MessagePack</c>, and Microsoft is consolidating on System.Text.Json. A guard
/// written against that fact would quietly become vacuous the day the dependency is dropped, while
/// still passing. What must stay true regardless is that the JSON path never leaves the
/// source-generated <c>LspJsonContext</c> — every <c>JsonRpc</c> and message-handler construction has
/// to use the 3-arg overload, because the shorter ones silently default to a Newtonsoft formatter and
/// that fails quietly under AOT rather than loudly.
/// </para>
/// </summary>
public class LspSerializationBoundaryTests
{
    private static IEnumerable<string> ReferencedAssembliesOfLspClient() =>
        typeof(ILspTransport).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty);

    [Fact]
    public void TheManifestReadCanActuallySeeAThirdPartyBinding()
    {
        // The control for the OmniSharp guard, which asserts an ABSENCE — exactly the shape that keeps
        // passing after the instrument stops working. A trimming change, a compiler change or a rename
        // would leave a permanently-green test guarding nothing. StreamJsonRpc is a third-party
        // assembly this project genuinely binds, so seeing it proves the manifest read discriminates
        // rather than returning an empty or BCL-only list.
        ReferencedAssembliesOfLspClient()
            .Should().Contain("StreamJsonRpc",
                "if this disappears, the OmniSharp assertion has stopped meaning anything");
    }

    [Fact]
    public void TheLspClientDoesNotBindToOmniSharp()
    {
        ReferencedAssembliesOfLspClient()
            .Should().NotContain(name => name.StartsWith("OmniSharp", StringComparison.OrdinalIgnoreCase),
                "consuming a foreign server's custom methods would drag its object model and its "
              + "serializer into HexIDE — and the payload that tempts us does not round-trip even "
              + "with OmniSharp on both ends. Standard LSP only.");
    }

    [Fact]
    public async Task TheClientHandsEveryTransportASystemTextJsonFormatter()
    {
        // The JSON path must stay on the source-generated LspJsonContext: the shorter JsonRpc and
        // message-handler overloads silently default to a Newtonsoft formatter, and under AOT that
        // fails quietly rather than loudly.
        //
        // Asserted against the object the client actually builds, NOT against what is in the package
        // closure. Newtonsoft's presence there is a fact about StreamJsonRpc's own dependencies, which
        // is visibly in motion — its closure currently carries both the old MessagePack and the newer
        // Nerdbank.MessagePack — and Microsoft is consolidating on STJ. A guard phrased as "Newtonsoft
        // is absent" would silently become vacuous the day that dependency is dropped, while still
        // passing. This one keeps meaning the same thing either way.
        var transport = Substitute.For<ILspTransport>();
        transport.ConnectAsync(Arg.Any<IJsonRpcMessageFormatter>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IJsonRpcMessageHandler?>(null));

        await new VBLspClient(transport, Substitute.For<ILogger<VBLspClient>>()).StartAsync();

        var formatter = (IJsonRpcMessageFormatter?)transport.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(ILspTransport.ConnectAsync))
            .GetArguments()[0];

        formatter.Should().BeOfType<SystemTextJsonFormatter>();
        ((SystemTextJsonFormatter)formatter!).JsonSerializerOptions.TypeInfoResolver
            .Should().BeSameAs(LspJsonContext.Default,
                "the AOT-safe source-generated resolver is the point — a plain SystemTextJsonFormatter "
              + "without it reflects at runtime and fails under trimming");
    }
}

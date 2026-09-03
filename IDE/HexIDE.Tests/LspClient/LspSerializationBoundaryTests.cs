using HexIDE.Lsp;

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
/// These assertions read the compiled assembly manifest rather than the package graph, which is the
/// stricter and more useful test: the compiler emits a reference only for an assembly actually
/// <em>used</em>. <c>Newtonsoft.Json</c> is already in this project's transitive package closure —
/// StreamJsonRpc brings it — so "is it in the closure" would fail today and tell us nothing. What
/// matters is that we never bind to it, which is also an AOT requirement: the JSON path has to stay
/// on the source-generated <c>LspJsonContext</c>, and every <c>JsonRpc</c> / message-handler
/// construction has to use the 3-arg overload, because the shorter ones silently default to the
/// Newtonsoft formatter.
/// </para>
/// </summary>
public class LspSerializationBoundaryTests
{
    private static IEnumerable<string> ReferencedAssembliesOfLspClient() =>
        typeof(ILspTransport).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty);

    [Fact]
    public void TheGuardsInThisClassCanActuallyFail()
    {
        // The control. Both assertions below are "X is absent", which is exactly the shape that keeps
        // passing after the instrument stops working — a trimming change, a compiler change, or a
        // rename would leave two permanently-green tests guarding nothing. StreamJsonRpc is a
        // third-party assembly this project genuinely binds, so seeing it proves the manifest read
        // discriminates rather than returning an empty or BCL-only list.
        ReferencedAssembliesOfLspClient()
            .Should().Contain("StreamJsonRpc",
                "if this disappears, the absence assertions below have stopped meaning anything");
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
    public void TheLspClientDoesNotBindToNewtonsoftEvenThoughItIsInTheClosure()
    {
        // StreamJsonRpc puts Newtonsoft.Json in the package graph whether we want it or not. Present
        // is fine; bound is not. Binding it would mean something took a JsonRpc or message-handler
        // overload that defaults to the Newtonsoft formatter, bypassing the AOT-safe
        // SystemTextJsonFormatter + LspJsonContext — which fails silently under AOT rather than loudly.
        ReferencedAssembliesOfLspClient()
            .Should().NotContain(name => name.StartsWith("Newtonsoft", StringComparison.OrdinalIgnoreCase),
                "the JSON path must stay on the source-generated LspJsonContext; a Newtonsoft binding "
              + "means a 1- or 2-arg JsonRpc/handler overload slipped in");
    }
}

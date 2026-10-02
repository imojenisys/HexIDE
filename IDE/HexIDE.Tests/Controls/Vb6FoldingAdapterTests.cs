using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using HexIDE.Controls;
using HexIDE.Lsp.Messages;
using HexIDE.Runtime.Serialization;

namespace HexIDE.Tests.Controls;

/// <summary>
/// The code window's folds: a server's, the header's, and the two merged into one list the folding manager
/// accepts (hexide-io/HexIDE#273 task 3.14).
/// </summary>
public class Vb6FoldingAdapterTests
{
    // Header: lines 1-9, 0-based 0-8. Code from line 10.
    private const string Frm =
        "VERSION 5.00\r\n" +
        "Begin VB.Form frmOrders \r\n" +
        "   Caption         =   \"Orders\"\r\n" +
        "   Begin VB.CommandButton cmdOK \r\n" +
        "      Caption         =   \"OK\"\r\n" +
        "   End\r\n" +
        "End\r\n" +
        "Attribute VB_Name = \"frmOrders\"\r\n" +
        "Attribute VB_PredeclaredId = True\r\n" +
        "Option Explicit\r\n" +
        "\r\n" +
        "Private Sub cmdOK_Click()\r\n" +
        "    Unload Me\r\n" +
        "End Sub\r\n";

    /// <summary>Where the designer half ends and the code section starts, as the model splits them.</summary>
    private static readonly int FrmPrefix = Frm.IndexOf("Attribute VB_Name", StringComparison.Ordinal);

    private static NewFolding? HeaderOf(string text, int prefixLength) =>
        Vb6FoldingAdapter.Header(new TextDocument(text), ReadOnlyRegions.Of(text, prefixLength));

    private static NewFolding Fold(int start, int end) => new(start, end);

    // ── The header's fold ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AFormsHeaderFoldEndsWithTheHeadersLastLineNotItsLineBreak()
    {
        var fold = HeaderOf(Frm, FrmPrefix);

        fold.Should().NotBeNull();
        fold!.StartOffset.Should().Be(0);
        var lastLine = "Attribute VB_PredeclaredId = True";
        fold.EndOffset.Should().Be(Frm.IndexOf(lastLine, StringComparison.Ordinal) + lastLine.Length,
            "folded, the header is one line and the first line of code stays a line of its own");
    }

    [Fact]
    public void TheHeaderFoldIsLabelledWithItsFirstLine()
    {
        HeaderOf(Frm, FrmPrefix)!.Name.Should().Be("VERSION 5.00");
    }

    [Fact]
    public void ATwoLineHeaderIsFolded()
    {
        // The shortest header a fold can hide anything of.
        var text = "Attribute VB_Name = \"Module1\"\r\nAttribute VB_Description = \"Helpers\"\r\nOption Explicit\r\n";

        var fold = HeaderOf(text, text.IndexOf("Option", StringComparison.Ordinal));

        fold.Should().NotBeNull();
        fold!.EndOffset.Should().Be(text.IndexOf("\r\nOption", StringComparison.Ordinal));
    }

    [Fact]
    public void AOneLineHeaderIsNotFolded()
    {
        var text = "Attribute VB_Name = \"Module1\"\r\nOption Explicit\r\n";

        HeaderOf(text, text.IndexOf("Option", StringComparison.Ordinal)).Should().BeNull();
    }

    [Fact]
    public void AFileWithNoHeaderHasNoHeaderFold()
    {
        HeaderOf("Option Explicit\r\n\r\nSub Main()\r\nEnd Sub\r\n", 0).Should().BeNull();
    }

    [Fact]
    public void AMembersAttributeRunIsNotTheHeader()
    {
        // The first region is a member's when the file has no header, and it is phase 4's to fold, not this one's.
        var text = "Option Explicit\r\nPublic Sub Go()\r\nAttribute Go.VB_Description = \"Goes\"\r\nAttribute Go.VB_UserMemId = 0\r\nEnd Sub\r\n";
        var regions = ReadOnlyRegions.Of(text, 0);
        regions.Should().NotBeEmpty();
        regions[0].Anchor.Should().NotBeNull("the test needs a member's run first");

        Vb6FoldingAdapter.Header(new TextDocument(text), regions).Should().BeNull();
    }

    [Fact]
    public void AHeaderWithNoFinalLineBreakEndsAtTheEndOfTheFile()
    {
        // A file that is nothing but its header, and ends without a line break.
        var text = "Attribute VB_Name = \"Module1\"\r\nAttribute VB_Description = \"Helpers\"";

        var fold = HeaderOf(text, text.Length);

        fold.Should().NotBeNull();
        fold!.EndOffset.Should().Be(text.Length);
    }

    // ── A server's folds ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AServerFoldRunsFromItsFirstLineToTheEndOfItsLast()
    {
        var document = new TextDocument(Frm);

        var folds = Vb6FoldingAdapter.FromServer(document, [new FoldingRange(11, 13)]);

        folds.Should().ContainSingle();
        folds[0].StartOffset.Should().Be(document.GetLineByNumber(12).Offset);
        folds[0].EndOffset.Should().Be(document.GetLineByNumber(14).EndOffset);
        folds[0].Name.Should().Be("Private Sub cmdOK_Click()");
    }

    [Fact]
    public void AServerFoldOnOneLineOrPastTheEndIsSkipped()
    {
        var document = new TextDocument(Frm);

        Vb6FoldingAdapter.FromServer(document, [new FoldingRange(3, 3), new FoldingRange(11, 99), new FoldingRange(5, 4)])
            .Should().BeEmpty();
    }

    // ── Merging ───────────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TheHeaderFoldIsKeptWhenTheServerSendsNothing()
    {
        // The defect this merge exists for: applying a server's answer replaces every fold, and no answer at all
        // is the commonest one.
        var header = Fold(0, 50);

        Vb6FoldingAdapter.Merge([header], []).Should().Equal(header);
    }

    [Fact]
    public void TheMergeIsSortedByStartWithTheLongerFirstOnATie()
    {
        // The manager throws on a list out of start order. On a tie it pairs folds with sections in list order.
        var header = Fold(0, 50);
        var late = Fold(200, 260);
        var outer = Fold(100, 180);
        var inner = Fold(100, 140);

        Vb6FoldingAdapter.Merge([header], [late, inner, outer]).Should().Equal(header, outer, inner, late);
    }

    [Fact]
    public void AServerFoldCrossingTheHeaderIsDropped()
    {
        var header = Fold(0, 50);
        var crossing = Fold(40, 90);
        var after = Fold(60, 90);

        Vb6FoldingAdapter.Merge([header], [crossing, after]).Should().Equal(header, after);
    }

    [Fact]
    public void AServerFoldInsideTheHeaderIsKept()
    {
        var header = Fold(0, 50);
        var inside = Fold(10, 40);
        var endingWithIt = Fold(20, 50);

        Vb6FoldingAdapter.Merge([header], [inside, endingWithIt]).Should().Equal(header, inside, endingWithIt);
    }

    [Fact]
    public void AServerFoldThatStartsWhereTheHeaderStartsIsDropped()
    {
        // The folding manager matches folds to sections by start offset alone, so two starting together can swap
        // sections and the header's folded state with them. Measured on AvaloniaEdit 12.0.0, 2026-10-02.
        var header = Fold(0, 50);

        Vb6FoldingAdapter.Merge([header], [Fold(0, 120), Fold(0, 20), Fold(0, 50)]).Should().Equal(header);
    }

    [Fact]
    public void AServerFoldContainingTheHeaderFromFurtherDownIsKept()
    {
        // Not reachable from the top of the file, where the header always starts, but the rule is containment,
        // and phase 4's attribute folds start further down.
        var own = Fold(30, 50);
        var containing = Fold(20, 90);

        Vb6FoldingAdapter.Merge([own], [containing]).Should().Equal(containing, own);
    }

    [Fact]
    public void AZeroLengthFoldIsDropped()
    {
        var kept = Fold(70, 80);

        Vb6FoldingAdapter.Merge([Fold(10, 10)], [Fold(60, 60), kept]).Should().Equal(kept);
    }

    [Fact]
    public void WithNoOwnFoldsAServersAreKeptAndSorted()
    {
        // The related-document window, which has no header.
        Vb6FoldingAdapter.Merge([], [Fold(50, 60), Fold(0, 20)])
            .Select(f => f.StartOffset).Should().Equal(0, 50);
    }
}

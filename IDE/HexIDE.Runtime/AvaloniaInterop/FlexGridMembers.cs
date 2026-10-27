using HexIDE.Runtime.BuiltinControls;
using HexIDE.Runtime.Interpreter;

namespace HexIDE.Runtime.AvaloniaInterop;

internal static class FlexGridMembers
{
    internal static Vb6Value Read(VBMSFlexGrid grid, string name, IReadOnlyList<Vb6Value> args)
    {
        if (args.Count != 0 && !IsIndexed(name)) throw Refused(name);
        var model = grid.Model;
        return name.ToLowerInvariant() switch
        {
            "rows" => new Vb6Value(model.Rows),
            "cols" => new Vb6Value(model.Cols),
            "row" => new Vb6Value(model.Row),
            "col" => new Vb6Value(model.Col),
            "fixedrows" => new Vb6Value(model.FixedRows),
            "fixedcols" => new Vb6Value(model.FixedCols),
            "rowsel" => new Vb6Value(model.RowSel),
            "colsel" => new Vb6Value(model.ColSel),
            "fillstyle" => new Vb6Value(model.FillStyle),
            "clip" => new Vb6Value(model.Clip),
            "textarray" when args.Count == 1 => new Vb6Value(model.GetTextArray(Index(args[0]))),
            "colwidth" when args.Count == 1 => new Vb6Value(model.GetColWidth(Index(args[0]))),
            "rowheight" when args.Count == 1 => new Vb6Value(model.GetRowHeight(Index(args[0]))),
            "colalignment" when args.Count == 1 => new Vb6Value(model.GetAlignment(Index(args[0]))),
            "fixedalignment" when args.Count == 1 => new Vb6Value(model.GetAlignment(Index(args[0]), true)),
            "text" => new Vb6Value(model.Text),
            "textmatrix" when args.Count == 2 => new Vb6Value(model.GetText(Index(args[0]), Index(args[1]))),
            "formatstring" => new Vb6Value(model.FormatString),
            _ => throw Refused(name),
        };
    }

    internal static void Write(VBMSFlexGrid grid, string name, IReadOnlyList<Vb6Value> args, Vb6Value value)
    {
        if (args.Count != 0 && !IsIndexed(name)) throw Refused(name);
        var model = grid.Model;
        switch (name.ToLowerInvariant())
        {
            case "rows": model.Rows = Index(value); break;
            case "cols": model.Cols = Index(value); break;
            case "row": model.Row = Index(value); break;
            case "col": model.Col = Index(value); break;
            case "fixedrows": model.FixedRows = Index(value); break;
            case "fixedcols": model.FixedCols = Index(value); break;
            case "rowsel": model.RowSel = Index(value); break;
            case "colsel": model.ColSel = Index(value); break;
            case "fillstyle": model.FillStyle = Index(value); break;
            case "clip": model.Clip = Text(value); break;
            case "textarray" when args.Count == 1: model.SetTextArray(Index(args[0]), Text(value)); break;
            case "colwidth" when args.Count == 1: model.SetColWidth(Index(args[0]), Index(value)); break;
            case "rowheight" when args.Count == 1: model.SetRowHeight(Index(args[0]), Index(value)); break;
            case "rowheightmin": model.SetRowHeightMin(Index(value)); break;
            case "colalignment" when args.Count == 1: model.SetColAlignment(Index(args[0]), Index(value)); break;
            case "text": model.Text = Text(value); break;
            case "textmatrix" when args.Count == 2: model.SetText(Index(args[0]), Index(args[1]), Text(value)); break;
            case "formatstring": model.FormatString = Text(value); break;
            default: throw Refused(name);
        }
    }

    private static bool IsIndexed(string name) => name.ToLowerInvariant() is
        "textmatrix" or "textarray" or "colwidth" or "rowheight" or "colalignment" or "fixedalignment";

    // This slice accepts Integer/Long inputs and strings. Refuse unmeasured conversions.
    private static int Index(Vb6Value value) => value.Value switch
    {
        short n => n,
        int n => n,
        long n when n >= int.MinValue && n <= int.MaxValue => (int)n,
        _ => throw Refused("numeric conversion"),
    };
    private static string Text(Vb6Value value) => value.Value is string s ? s : throw Refused("text conversion");
    internal static NotImplementedException Refused(string name) => new($"MSFlexGrid.{name} is not implemented (#1099)");
}

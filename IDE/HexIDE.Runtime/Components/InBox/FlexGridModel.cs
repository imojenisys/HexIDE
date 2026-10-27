using System;
using System.Collections.Generic;

namespace HexIDE.Runtime.Components.InBox;

/// <summary>The measured MSFlexGrid shape, cursor and text state (#1099), independent of drawing and input.</summary>
internal sealed class FlexGridModel
{
    private int rows = 2, cols = 2, fixedRows = 1, fixedCols = 1;
    private int row = 1, col = 1, rowSel = 1, colSel = 1, fillStyle;
    private readonly Dictionary<(int Row, int Col), string> text = new();

    public Action<string>? CellEvent { get; set; }
    public int TopRow { get; private set; } = 1;

    public int Rows
    {
        get => rows;
        set
        {
            if (value < 0) throw InvalidRow();
            RefuseRemovedFormatting(value, cols);
            rows = value;
            fixedRows = Math.Min(fixedRows, Math.Max(0, rows - 1));
            row = Math.Min(row, rows - 1);
            rowSel = Math.Min(rowSel, rows - 1);
            DiscardRemovedText();
        }
    }

    public int Cols
    {
        get => cols;
        set
        {
            if (value < 0) throw InvalidColumn();
            RefuseRemovedFormatting(rows, value);
            cols = value;
            fixedCols = Math.Min(fixedCols, Math.Max(0, cols - 1));
            col = Math.Min(col, cols - 1);
            colSel = Math.Min(colSel, cols - 1);
            DiscardRemovedText();
        }
    }

    public int FixedRows
    {
        get => fixedRows;
        set
        {
            if (value < 0) throw InvalidRow();
            if (value >= rows) throw Error(30016, "FixedRows must be at least one less than Rows value");
            fixedRows = value;
            row = rowSel = TopRow = value;
        }
    }

    public int FixedCols
    {
        get => fixedCols;
        set
        {
            if (value >= cols) throw Error(30017, "FixedCols must be at least one less than Cols value");
            throw new NotImplementedException("Assigning MSFlexGrid.FixedCols requires measured cursor effects");
        }
    }

    public int Row
    {
        get => row;
        set
        {
            CheckRow(value);
            if (row == value) return;
            CellEvent?.Invoke("LeaveCell");
            row = rowSel = value;
            EnterCell();
        }
    }

    public int Col
    {
        get => col;
        set
        {
            CheckColumn(value);
            if (col == value) return;
            CellEvent?.Invoke("LeaveCell");
            col = colSel = value;
            EnterCell();
        }
    }

    public int RowSel { get => rowSel; set { CheckRow(value); rowSel = value; } }
    public int ColSel { get => colSel; set { CheckColumn(value); colSel = value; } }
    public int FillStyle { get => fillStyle; set { if (value is 0 or 1) fillStyle = value; } }

    private readonly Dictionary<int, int> columnWidths = new();
    private readonly Dictionary<int, int> rowHeights = new();
    private readonly Dictionary<int, int> columnAlignments = new();

    public int GetColWidth(int index)
    {
        if (index == -1) throw new NotImplementedException("MSFlexGrid.ColWidth(-1) read is not implemented (#1099)");
        CheckColumn(index);
        return columnWidths.GetValueOrDefault(index, 960);
    }

    public void SetColWidth(int index, int value)
    {
        if (index != -1) CheckColumn(index);
        if (value < -1) throw Error(30014, "Invalid Column Width Value");
        var width = value == -1 ? 960 : PixelTwips(value);
        if (index == -1)
            for (var c = 0; c < cols; c++) columnWidths[c] = width;
        else columnWidths[index] = width;
    }

    public int GetRowHeight(int index)
    {
        if (index == -1) throw new NotImplementedException("MSFlexGrid.RowHeight(-1) is not implemented (#1099)");
        CheckRow(index);
        return rowHeights.GetValueOrDefault(index, 240);
    }

    public void SetRowHeight(int index, int value)
    {
        if (index == -1) throw new NotImplementedException("MSFlexGrid.RowHeight(-1) is not implemented (#1099)");
        CheckRow(index);
        if (value < -1) throw Error(30013, "Invalid Row Height Value");
        rowHeights[index] = value == -1 ? 240 : PixelTwips(value);
    }

    public void SetRowHeightMin(int value)
    {
        if (value < 0) throw Error(30013, "Invalid Row Height Value");
        var height = PixelTwips(value);
        for (var r = 0; r < rows; r++)
            if (GetRowHeight(r) < height) rowHeights[r] = height;
    }

    public int GetAlignment(int index, bool fixedCell = false)
    {
        if (index < 0 || index >= cols) throw Error(30004, "Application-defined or object-defined error");
        return fixedCell ? 9 : columnAlignments.GetValueOrDefault(index, 9);
    }

    public void SetColAlignment(int index, int value)
    {
        if (index < 0 || index >= cols) throw new NotImplementedException("MSFlexGrid.ColAlignment index error is not implemented (#1099)");
        if (value < 0 || value > 9) throw Error(30005, "Invalid Alignment value");
        columnAlignments[index] = value;
    }

    private static int PixelTwips(int value) => checked((int)(Math.Round(value / 15.0) * 15));

    public string Text
    {
        get => GetText(row, col);
        set
        {
            if (fillStyle == 0) SetText(row, col, value);
            else
                foreach (var cell in Selection()) SetText(cell.Row, cell.Col, value);
        }
    }

    public string GetText(int atRow, int atCol)
    {
        CheckCell(atRow, atCol);
        return text.GetValueOrDefault((atRow, atCol), "");
    }

    public void SetText(int atRow, int atCol, string value)
    {
        CheckCell(atRow, atCol);
        if (value.Length == 0) text.Remove((atRow, atCol));
        else text[(atRow, atCol)] = value;
    }

    public string GetTextArray(int index)
    {
        CheckIndex(index);
        return GetText(index / cols, index % cols);
    }

    public void SetTextArray(int index, string value)
    {
        CheckIndex(index);
        SetText(index / cols, index % cols, value);
    }

    public string Clip
    {
        get
        {
            var lines = new List<string>();
            for (var r = Math.Min(row, rowSel); r <= Math.Max(row, rowSel); r++)
            {
                var fields = new List<string>();
                for (var c = Math.Min(col, colSel); c <= Math.Max(col, colSel); c++)
                    fields.Add(GetText(r, c));
                lines.Add(string.Join('\t', fields));
            }
            return string.Join('\r', lines);
        }
        set
        {
            var lines = value.Split('\r');
            var firstRow = Math.Min(row, rowSel);
            var firstCol = Math.Min(col, colSel);
            for (var r = 0; r < lines.Length && firstRow + r <= Math.Max(row, rowSel); r++)
            {
                var fields = lines[r].Split('\t');
                for (var c = 0; c < fields.Length && firstCol + c <= Math.Max(col, colSel); c++)
                    SetText(firstRow + r, firstCol + c, fields[c]);
            }
        }
    }

    private readonly Dictionary<(int Row, int Col, string Member), object> cellFormatting = new();

    public object GetCellFormat(string member)
    {
        CheckFormattingCell();
        if (cellFormatting.TryGetValue((row, col, member), out var value)) return value;
        return member switch
        {
            "cellfontname" => "MS Sans Serif",
            "cellfontsize" => 8.25,
            "cellfontbold" => false,
            _ => throw new NotImplementedException($"MSFlexGrid.{member} default is not implemented (#1099)"),
        };
    }

    public void SetCellFormat(string member, object value)
    {
        CheckFormattingCell();
        if (member == "cellalignment")
        {
            var alignment = (int)value;
            if (alignment == 10) throw Error(30005, "Invalid Alignment value");
            if (alignment < 0 || alignment > 10)
                throw new NotImplementedException("MSFlexGrid.CellAlignment validation is not implemented (#1099)");
        }
        if (fillStyle == 0) cellFormatting[(row, col, member)] = value;
        else
            foreach (var cell in Selection()) cellFormatting[(cell.Row, cell.Col, member)] = value;
    }

    public void AddItem(string item, int? index = null)
    {
        var at = index ?? rows;
        if (at >= 0 && at < fixedRows) throw Error(30001, "Cannot use AddItem on a fixed row");
        if (at < 0) throw new NotImplementedException("MSFlexGrid.AddItem negative index is not implemented (#1099)");
        if (at > rows) throw Error(30002, "Grid does not contain that row");
        RefuseRowEditEdges();
        var fields = item.Split('\t');
        var moved = new Dictionary<(int Row, int Col), string>();
        foreach (var cell in text)
            moved[(cell.Key.Row >= at ? cell.Key.Row + 1 : cell.Key.Row, cell.Key.Col)] = cell.Value;
        Rows++;
        text.Clear();
        foreach (var cell in moved) text[cell.Key] = cell.Value;
        for (var c = 0; c < fields.Length && c < cols; c++) SetText(at, c, fields[c]);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < fixedRows) throw Error(30000, "Cannot do a RemoveItem on a fixed row");
        if (index < 0) throw new NotImplementedException("MSFlexGrid.RemoveItem negative index is not implemented (#1099)");
        if (index >= rows) throw Error(30002, "Grid does not contain that row");
        if (rows - fixedRows == 1) throw Error(30015, "Can not remove last non-fixed row");
        RefuseRowEditEdges();
        var moved = new Dictionary<(int Row, int Col), string>();
        foreach (var cell in text)
            if (cell.Key.Row != index)
                moved[(cell.Key.Row > index ? cell.Key.Row - 1 : cell.Key.Row, cell.Key.Col)] = cell.Value;
        Rows--;
        text.Clear();
        foreach (var cell in moved) text[cell.Key] = cell.Value;
    }

    private void RefuseRowEditEdges()
    {
        if (rows == 0 || cols == 0 || cellFormatting.Count != 0 || rowHeights.Count != 0)
            throw new NotImplementedException("MSFlexGrid row operations with empty dimensions, formatting or row heights are not implemented (#1099)");
    }

    private string formatString = "";
    public string FormatString
    {
        get => formatString;
        set
        {
            formatString = value;
            if (fixedRows == 0 && fixedCols == 0) return;
            var sections = value.Split(';', 2);
            var columns = sections[0].Split('|');
            if (columns.Length > cols) Cols = columns.Length;
            if (fixedRows > 0)
                for (var c = 0; c < columns.Length; c++)
                {
                    var heading = columns[c];
                    if (heading.Length > 0 && heading[0] is '<' or '^' or '>') heading = heading[1..];
                    SetText(0, c, heading.TrimEnd(' '));
                }
            if (fixedCols > 0 && sections.Length == 2)
            {
                var headings = sections[1].Split('|');
                for (var r = 0; r < headings.Length && r < rows; r++)
                    SetText(r, 0, headings[r]);
            }
        }
    }

    private IEnumerable<(int Row, int Col)> Selection()
    {
        for (var r = Math.Min(row, rowSel); r <= Math.Max(row, rowSel); r++)
            for (var c = Math.Min(col, colSel); c <= Math.Max(col, colSel); c++)
                yield return (r, c);
    }

    private void EnterCell()
    {
        CellEvent?.Invoke("EnterCell");
        CellEvent?.Invoke("RowColChange");
    }

    private void CheckFormattingCell()
    {
        if (row < 0 || col < 0)
            throw new NotImplementedException("MSFlexGrid formatting without a current cell is not implemented (#1099)");
    }

    private void RefuseRemovedFormatting(int newRows, int newCols)
    {
        foreach (var cell in cellFormatting.Keys)
            if (cell.Row >= newRows || cell.Col >= newCols)
                throw new NotImplementedException("MSFlexGrid resizing formatted cells is not implemented (#1099)");
    }

    private void DiscardRemovedText()
    {
        var removed = new List<(int Row, int Col)>();
        foreach (var cell in text.Keys)
            if (cell.Row >= rows || cell.Col >= cols) removed.Add(cell);
        foreach (var cell in removed) text.Remove(cell);
    }

    private void CheckRow(int value) { if (value < 0 || value >= rows) throw InvalidRow(); }
    private void CheckColumn(int value) { if (value < 0 || value >= cols) throw InvalidColumn(); }
    private void CheckCell(int r, int c)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols) throw Error(381, "Subscript out of range");
    }
    private void CheckIndex(int index)
    {
        if (index < 0 || index >= (long)rows * cols) throw Error(381, "Subscript out of range");
    }
    private static VBRunTimeException InvalidRow() => Error(30009, "Invalid Row Value");
    private static VBRunTimeException InvalidColumn() => Error(30010, "Invalid Column Value");
    private static VBRunTimeException Error(int number, string description) => new(new VBStandardError(number, description));
}

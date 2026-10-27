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

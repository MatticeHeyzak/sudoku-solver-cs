using System.Numerics;
using Raylib_cs;
using SudokuSolver.Model;

namespace SudokuSolver.Renderer;

public class SudokuRenderer : IRenderer
{
    private const int RegionSize = 3;

    private readonly Color _lineColor = Color.Black;
    private readonly Color _givenColor = Color.Black;
    private readonly Color _userColor = Color.DarkBlue;
    private readonly Color _solvedColor = Color.Green;
    private readonly Color _selectionColor = new(173, 216, 230, 255);

    private readonly float _thickLine;
    private readonly float _thinLine;
    private readonly int _fontSize;
    private readonly BoardLayout _layout;

    public SudokuRenderer(BoardLayout layout, float thickLine = 4.0f, float thinLine = 1.0f)
    {
        _layout = layout;
        _thickLine = thickLine;
        _thinLine = thinLine;
        _fontSize = (int)(_layout.CellLength * 0.6f);
    }

    public void Draw(SudokuBoard board, (int Row, int Col)? selectedCell)
    {
        DrawSelection(selectedCell);
        DrawDigits(board);
        DrawGridLines();
    }

    private void DrawSelection((int Row, int Col)? selected)
    {
        if (selected is not { } sel) return;
        Raylib.DrawRectangleRec(_layout.GetCellRect(sel.Row, sel.Col), _selectionColor);
    }

    private void DrawDigits(SudokuBoard board)
    {
        for (int row = 0; row < BoardLayout.GridSize; row++)
        for (int col = 0; col < BoardLayout.GridSize; col++)
        {
            int value = board.GetValue(row, col);
            if (value == 0) continue;

            var color = board.GetOrigin(row, col) switch
            {
                CellOrigin.Given => _givenColor,
                CellOrigin.Solved => _solvedColor,
                _ => _userColor
            };

            var cellRect = _layout.GetCellRect(row, col);
            var text = value.ToString();
            var font = Raylib.GetFontDefault();
            var textSize = Raylib.MeasureTextEx(font, text, _fontSize, 1);
            var pos = new Vector2(
                cellRect.X + (cellRect.Width - textSize.X) / 2f,
                cellRect.Y + (cellRect.Height - textSize.Y) / 2f);

            Raylib.DrawTextEx(font, text, pos, _fontSize, 1, color);
        }
    }

    private void DrawGridLines()
    {
        Raylib.DrawRectangleLinesEx(_layout.Bounds, _thickLine, _lineColor);

        for (int i = 1; i < BoardLayout.GridSize; i++)
        {
            var thickness = i % RegionSize == 0 ? _thickLine : _thinLine;
            var offset = i * _layout.CellLength;

            var vertStart = new Vector2(_layout.Bounds.X + offset, _layout.Bounds.Y);
            var vertEnd = new Vector2(vertStart.X, _layout.Bounds.Y + _layout.Bounds.Height);
            Raylib.DrawLineEx(vertStart, vertEnd, thickness, _lineColor);

            var horizStart = new Vector2(_layout.Bounds.X, _layout.Bounds.Y + offset);
            var horizEnd = new Vector2(_layout.Bounds.X + _layout.Bounds.Width, horizStart.Y);
            Raylib.DrawLineEx(horizStart, horizEnd, thickness, _lineColor);
        }
    }
}

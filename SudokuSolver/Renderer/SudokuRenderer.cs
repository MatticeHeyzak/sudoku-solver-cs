using System.Numerics;
using Raylib_cs;
using SudokuSolver.Model;

namespace SudokuSolver.Renderer;

public class SudokuRenderer : IRenderer
{
    private const int RegionSize = 3;
    private const float FontScale = 0.6f;

    private readonly Color _lineColor = Color.Black;
    private readonly Color _givenColor = Color.Black;
    private readonly Color _userColor = Color.DarkBlue;
    private readonly Color _solvedColor = Color.Green;
    private readonly Color _selectionColor = new(173, 216, 230, 255);

    private readonly float _thickLine;
    private readonly float _thinLine;

    public SudokuRenderer(float thickLine = 4.0f, float thinLine = 1.0f)
    {
        _thickLine = thickLine;
        _thinLine = thinLine;
    }

    public void Draw(SudokuBoard board, (int Row, int Col)? selectedCell, BoardLayout layout)
    {
        DrawSelection(selectedCell, layout);
        DrawDigits(board, layout);
        DrawGridLines(layout);
    }

    private void DrawSelection((int Row, int Col)? selected, BoardLayout layout)
    {
        if (selected is not { } sel) return;
        Raylib.DrawRectangleRec(layout.GetCellRect(sel.Row, sel.Col), _selectionColor);
    }

    private void DrawDigits(SudokuBoard board, BoardLayout layout)
    {
        var font = Raylib.GetFontDefault();
        var fontSize = layout.CellLength * FontScale;

        for (int row = 0; row < BoardLayout.GridSize; row++)
        for (int col = 0; col < BoardLayout.GridSize; col++)
        {
            int value = board.GetValue(row, col);
            if (value == 0) continue;

            var color = board.GetOrigin(row, col) switch
            {
                Model.CellOrigin.Given => _givenColor,
                Model.CellOrigin.Solved => _solvedColor,
                _ => _userColor
            };

            var cellRect = layout.GetCellRect(row, col);
            var text = value.ToString();
            var textSize = Raylib.MeasureTextEx(font, text, fontSize, 1);
            var pos = new Vector2(
                cellRect.X + (cellRect.Width - textSize.X) / 2f,
                cellRect.Y + (cellRect.Height - textSize.Y) / 2f);

            Raylib.DrawTextEx(font, text, pos, fontSize, 1, color);
        }
    }

    private void DrawGridLines(BoardLayout layout)
    {
        Raylib.DrawRectangleLinesEx(layout.Bounds, _thickLine, _lineColor);

        for (int i = 1; i < BoardLayout.GridSize; i++)
        {
            var thickness = i % RegionSize == 0 ? _thickLine : _thinLine;
            var offset = i * layout.CellLength;

            var vertStart = new Vector2(layout.Bounds.X + offset, layout.Bounds.Y);
            var vertEnd = new Vector2(vertStart.X, layout.Bounds.Y + layout.Bounds.Height);
            Raylib.DrawLineEx(vertStart, vertEnd, thickness, _lineColor);

            var horizStart = new Vector2(layout.Bounds.X, layout.Bounds.Y + offset);
            var horizEnd = new Vector2(layout.Bounds.X + layout.Bounds.Width, horizStart.Y);
            Raylib.DrawLineEx(horizStart, horizEnd, thickness, _lineColor);
        }
    }
}
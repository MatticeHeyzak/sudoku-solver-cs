using System.Numerics;
using Raylib_cs;

namespace SudokuSolver.Renderer;

public readonly struct BoardLayout
{
    public const int GridSize = 9;
    
    public Rectangle Bounds { get; }
    public float CellLength { get; }
    
    public BoardLayout(Rectangle bounds, float cellLength)
    {
        Bounds = bounds;
        CellLength = cellLength;
    }

    public static BoardLayout FromWindow(int windowWidth, float scale = 0.8f)
    {
        var gridLength = windowWidth * scale;
        var start = (windowWidth - gridLength) / 2f;
        return new BoardLayout(new Rectangle(start, start, gridLength, gridLength), gridLength / GridSize);
    }

    public bool TryGetCell(Vector2 point, out int row, out int col)
    {
        row = col = -1;
        if (!Raylib.CheckCollisionPointRec(point, Bounds))
            return false;

        col = (int)((point.X - Bounds.X) / CellLength);
        row = (int)((point.Y - Bounds.Y) / CellLength);
        return row is >= 0 and < GridSize && col is >= 0 and < GridSize;
    }

    public Rectangle GetCellRect(int row, int col)
    {
        return new Rectangle(
            Bounds.X + col * CellLength,
            Bounds.Y + row * CellLength,
            CellLength,
            CellLength);
    }
}
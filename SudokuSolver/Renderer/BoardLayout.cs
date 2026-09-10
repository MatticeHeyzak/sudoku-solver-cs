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

    /// buttonBarHeight is dynamic because the algorithm selector may wrap
    /// into multiple rows on narrow windows, so the board must shrink to
    /// leave enough room for it.
    public static BoardLayout ComputeForScreen(int screenWidth, int screenHeight, float buttonBarHeight)
    {
        int availableWidth = screenWidth - Settings.BoardMargin * 2;
        float availableHeight = screenHeight - buttonBarHeight - Settings.BoardMargin * 3;

        float gridLength = Math.Max(GridSize, Math.Min(availableWidth, availableHeight));

        float startX = (screenWidth - gridLength) / 2f;
        float startY = Settings.BoardMargin;

        return new BoardLayout(new Rectangle(startX, startY, gridLength, gridLength), gridLength / GridSize);
    }

    public Rectangle GetButtonBarArea(int screenWidth, float buttonBarHeight)
    {
        return new Rectangle(
            Settings.BoardMargin,
            Bounds.Y + Bounds.Height + Settings.BoardMargin,
            screenWidth - Settings.BoardMargin * 2,
            buttonBarHeight);
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
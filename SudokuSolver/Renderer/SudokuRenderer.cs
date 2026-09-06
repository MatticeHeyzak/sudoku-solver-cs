using System.Numerics;
using Raylib_cs;

namespace SudokuSolver.Renderer;

public class SudokuRenderer : IRenderer
{
    private const int GridSize = 9;
    private const int RegionSize = 3;

    private readonly Color _lineColor = Color.Black;
    private readonly float _thickLine;
    private readonly float _thinLine;

    private Rectangle _boardBounds;
    private float _cellLength;

    public SudokuRenderer(float thickLine = 4.0f, float thinLine = 1.0f)
    {
        _thickLine = thickLine;
        _thinLine = thinLine;

        UpdateLayout();
    }

    private void UpdateLayout()
    {
        var gridLength = Settings.WindowWidth * 0.8f;
        var startX = (Settings.WindowWidth - gridLength) / 2f;
        var startY = startX;
        
        _cellLength = gridLength / GridSize;
        _boardBounds = new Rectangle(startX, startY, gridLength, gridLength);
    }

    public void Draw()
    {
        Raylib.DrawRectangleLinesEx(_boardBounds, _thickLine, _lineColor);

        for (int i = 1; i < GridSize; i++)
        {
            var thickness = i % RegionSize == 0
                ? _thickLine
                : _thinLine;
            var offset = i * _cellLength;
            
            // Vertical Line
            Vector2 vertStart = new Vector2(_boardBounds.X + offset, _boardBounds.Y);
            Vector2 vertEnd = new Vector2(vertStart.X, _boardBounds.Y + _boardBounds.Height);
            Raylib.DrawLineEx(vertStart, vertEnd, thickness, _lineColor);

            // Horizontal Line
            Vector2 horizStart = new Vector2(_boardBounds.X, _boardBounds.Y + offset);
            Vector2 horizEnd = new Vector2(_boardBounds.X + _boardBounds.Width, horizStart.Y);
            Raylib.DrawLineEx(horizStart, horizEnd, thickness, _lineColor);
        }
}
}
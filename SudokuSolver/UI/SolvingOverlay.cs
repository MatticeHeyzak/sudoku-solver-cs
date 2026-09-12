using System.Numerics;
using Raylib_cs;

namespace SudokuSolver.UI;

public class SolvingOverlay
{
    private const float Radius = 30f;
    private const float Thickness = 6f;
    private const float DegreesPerSecond = 360f;
    private const float ArcSweepDegrees = 270f;
    private const float FontSize = 22f;

    public void Draw(Rectangle boardBounds, string label)
    {
        Raylib.DrawRectangleRec(boardBounds, new Color(255, 255, 255, 200));

        var center = new Vector2(
            boardBounds.X + boardBounds.Width / 2f,
            boardBounds.Y + boardBounds.Height / 2f);
        
        float angle = (float)(Raylib.GetTime() * DegreesPerSecond) % 360f;
        Raylib.DrawRingLines(center, Radius - Thickness, Radius, angle, angle + ArcSweepDegrees, 32, Color.DarkBlue);

        var font = Raylib.GetFontDefault();
        var textSize = Raylib.MeasureTextEx(font, label, FontSize, 1);
        var textPos = new Vector2(center.X - textSize.X / 2f, center.Y + Radius + 12f);
        Raylib.DrawTextEx(font, label, textPos, FontSize, 1, Color.DarkBlue);
    }
}
using System.Numerics;
using Raylib_cs;

namespace SudokuSolver.UI;

public sealed class Button
{
    public Rectangle Bounds { get; }
    public string Label { get; }
    public bool IsActive { get; set; }

    public Button(Rectangle bounds, string label)
    {
        Bounds = bounds;
        Label = label;
    }
    
    public bool IsClicked() =>
        Raylib.IsMouseButtonPressed(MouseButton.Left)
        && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), Bounds);

    public void Draw()
    {
        var backColor = IsActive ? Color.SkyBlue : Color.LightGray;
        Raylib.DrawRectangleRec(Bounds, backColor);
        Raylib.DrawRectangleLinesEx(Bounds, 2f, Color.Black);

        var fontSize = (int)(Bounds.Height * 0.35f);
        var textWidth = Raylib.MeasureText(Label, fontSize);
        var pos = new Vector2(
            Bounds.X + (Bounds.Width - textWidth) / 2f,
            Bounds.Y + (Bounds.Height - fontSize) / 2f);

        Raylib.DrawText(Label, (int)pos.X, (int)pos.Y, fontSize, Color.Black);
    }
}
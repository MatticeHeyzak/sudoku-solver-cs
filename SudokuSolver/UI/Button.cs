using System.Numerics;
using Raylib_cs;

namespace SudokuSolver.UI;

public sealed class Button
{
    private const float MaxFontScale = 0.4f;
    private const float MinFontSize = 10f;
    private const float HorizontalPadding = 8f;

    public Rectangle Bounds { get; private set; }
    public string Label { get; }
    public bool IsActive { get; set; }

    public Button(Rectangle bounds, string label)
    {
        Bounds = bounds;
        Label = label;
    }

    /// Called whenever the window / bar layout changes size.
    public void UpdateBounds(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public bool IsClicked() =>
        Raylib.IsMouseButtonPressed(MouseButton.Left)
        && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), Bounds);

    public void Draw()
    {
        var backColor = IsActive ? Color.SkyBlue : Color.LightGray;
        Raylib.DrawRectangleRec(Bounds, backColor);
        Raylib.DrawRectangleLinesEx(Bounds, 2f, Color.Black);

        var fontSize = GetFittingFontSize();
        var textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), Label, fontSize, 1);
        var pos = new Vector2(
            Bounds.X + (Bounds.Width - textSize.X) / 2f,
            Bounds.Y + (Bounds.Height - textSize.Y) / 2f);

        Raylib.DrawTextEx(Raylib.GetFontDefault(), Label, pos, fontSize, 1, Color.Black);
    }

    /// Shrinks the font until the label fits within the button width,
    /// preventing the text overlap seen with long algorithm names.
    private float GetFittingFontSize()
    {
        var font = Raylib.GetFontDefault();
        var fontSize = Bounds.Height * MaxFontScale;
        var availableWidth = Bounds.Width - HorizontalPadding * 2;

        var textWidth = Raylib.MeasureTextEx(font, Label, fontSize, 1).X;
        if (textWidth > availableWidth && textWidth > 0)
        {
            fontSize *= availableWidth / textWidth;
        }

        return MathF.Max(fontSize, MinFontSize);
    }
}
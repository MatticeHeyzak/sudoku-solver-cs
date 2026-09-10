using Raylib_cs;

namespace SudokuSolver.UI;

public class ToggleButton
{
    private readonly Button _button;
    
    public bool IsOn { get; private set; }

    public ToggleButton(Rectangle bounds, string label, bool initialValue = false)
    {
        _button = new Button(bounds, label) { IsActive = initialValue };
        IsOn = initialValue;
    }
    
    public void UpdateBounds(Rectangle bounds) => _button.UpdateBounds(bounds);

    public void Update()
    {
        if (!_button.IsClicked())
            return;

        IsOn = !IsOn;
        _button.IsActive = IsOn;
    }

    public void Draw() => _button.Draw();
}
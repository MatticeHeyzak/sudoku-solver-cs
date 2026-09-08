using Raylib_cs;
using SudokuSolver.Solvers;

namespace SudokuSolver.UI;

public class AlgorithmSelector
{
    private const float ButtonGap = 6f;

    private readonly Dictionary<SolverType, Button> _buttons = new();
    private bool _inputMode = true;

    public SolverType Selected { get; private set; } = SolverType.Backtracking;

    public AlgorithmSelector(Rectangle area)
    {
        foreach (var type in Enum.GetValues<SolverType>())
            _buttons[type] = new Button(new Rectangle(), type.ToString());

        UpdateLayout(area);
        _buttons[Selected].IsActive = true;
    }

    public void DisableInputMode() => _inputMode = false;

    /// Recomputes each button's rectangle from the given bar area.
    /// Call every frame (or on resize) so buttons scale with the window.
    public void UpdateLayout(Rectangle area)
    {
        var types = _buttons.Keys.ToArray();
        var buttonWidth = (area.Width - ButtonGap * (types.Length - 1)) / types.Length;

        for (int i = 0; i < types.Length; i++)
        {
            var bounds = new Rectangle(
                area.X + i * (buttonWidth + ButtonGap),
                area.Y,
                buttonWidth,
                area.Height);

            _buttons[types[i]].UpdateBounds(bounds);
        }
    }

    public void Update()
    {
        if (!_inputMode) return;

        foreach (var (type, button) in _buttons)
        {
            if (!button.IsClicked()) continue;

            Selected = type;
            foreach (var b in _buttons.Values) b.IsActive = false;
            button.IsActive = true;
        }
    }

    public void Draw()
    {
        foreach (var button in _buttons.Values) button.Draw();
    }
}
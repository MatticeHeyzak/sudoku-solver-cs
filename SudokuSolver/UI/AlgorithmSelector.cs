using Raylib_cs;
using SudokuSolver.Solvers;

namespace SudokuSolver.UI;

public class AlgorithmSelector
{
    private readonly Dictionary<SolverType, Button> _buttons = new();

    private bool InputMode = true;

    public SolverType Selected { get; private set; } = SolverType.Backtracking;

    public AlgorithmSelector(Rectangle area)
    {
        var types = Enum.GetValues<SolverType>();
        var buttonWidth = area.Width / types.Length;

        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var bounds = new Rectangle(area.X + i * buttonWidth, area.Y, buttonWidth - 4, area.Height);
            _buttons[type] = new Button(bounds, type.ToString());
        }

        _buttons[Selected].IsActive = true;
    }

    public void DisableInputMode() => InputMode = false;
    
    public void Update()
    {
        if (!InputMode)
            return;
        
        foreach (var (type, button) in _buttons)
        {
            if (!button.IsClicked()) continue;

            Selected = type;
            foreach (var b in _buttons.Values)
                b.IsActive = false;
            button.IsActive = true;
        }
    }

    public void Draw()
    {
        foreach (var button in _buttons.Values)
            button.Draw();
    }
}
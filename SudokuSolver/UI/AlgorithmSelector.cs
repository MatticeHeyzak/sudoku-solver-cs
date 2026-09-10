using Raylib_cs;
using SudokuSolver.Solvers;

namespace SudokuSolver.UI;

public class AlgorithmSelector
{
    private const float ButtonGap = 6f;
    private const int WrappedColumns = 2;

    private readonly Dictionary<SolverType, Button> _buttons = new();
    private bool _inputMode = true;

    public static int ButtonCount { get; } = Enum.GetValues<SolverType>().Length;

    public SolverType Selected { get; private set; } = SolverType.Backtracking;

    public AlgorithmSelector(Rectangle area)
    {
        foreach (var type in Enum.GetValues<SolverType>())
            _buttons[type] = new Button(new Rectangle(), type.ToString());

        UpdateLayout(area);
        _buttons[Selected].IsActive = true;
    }

    public void DisableInputMode() => _inputMode = false;

    public static int GetRequiredRows(float availableWidth, int buttonCount)
    {
        float widthPerButton = availableWidth / buttonCount;
        if (widthPerButton >= Settings.AlgorithmSelectorMinButtonWidth)
            return 1;

        return (int)Math.Ceiling(buttonCount / (double)WrappedColumns);
    }

    public static float GetRequiredHeight(float availableWidth, int buttonCount)
    {
        int rows = GetRequiredRows(availableWidth, buttonCount);
        return rows * Settings.ButtonBarHeight + (rows - 1) * ButtonGap;
    }

    public void UpdateLayout(Rectangle area)
    {
        var types = _buttons.Keys.ToArray();
        int rows = GetRequiredRows(area.Width, types.Length);
        int columns = rows == 1 ? types.Length : WrappedColumns;

        var buttonWidth = (area.Width - ButtonGap * (columns - 1)) / columns;
        var buttonHeight = (area.Height - ButtonGap * (rows - 1)) / rows;

        for (int i = 0; i < types.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;

            var bounds = new Rectangle(
                area.X + col * (buttonWidth + ButtonGap),
                area.Y + row * (buttonHeight + ButtonGap),
                buttonWidth,
                buttonHeight);

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
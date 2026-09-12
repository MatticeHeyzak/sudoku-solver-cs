using System.Numerics;
using Raylib_cs;
using SudokuSolver.Input;
using SudokuSolver.Model;
using SudokuSolver.Renderer;
using SudokuSolver.Solvers;
using SudokuSolver.UI;

namespace SudokuSolver;

public sealed class SudokuProgram
{
    private enum SolveState
    {
        Idle,
        Loading,
        Revealing
    }

    private readonly IRenderer _renderer;
    private readonly SudokuBoard _board = new();
    private readonly InputHandler _input = new();
    private readonly AlgorithmSelector _algorithmSelector;
    private readonly ToggleButton _visualizeToggle;
    private readonly SolvingOverlay _solvingOverlay = new();
    private readonly List<SolverStep> _recordedSteps = new();

    private BoardLayout _layout;
    private Rectangle _barArea;
    private SolveState _state = SolveState.Idle;
    private bool _solveStarted;

    private SudokuBoard? _workingBoard;
    private IEnumerator<SolverStep>? _solveEnumerator;
    private int _replayIndex;

    public SudokuProgram(IRenderer renderer)
    {
        _renderer = renderer;

        _layout = ComputeBoardLayout(Settings.WindowWidth, Settings.WindowHeight);

        var (selectorArea, toggleArea) = SplitButtonBar(_layout, Settings.WindowWidth);
        _algorithmSelector = new AlgorithmSelector(selectorArea);
        _visualizeToggle = new ToggleButton(toggleArea, "Visualize");
    }

    public void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(Settings.WindowWidth, Settings.WindowHeight, Settings.WindowName);
        Raylib.SetWindowMinSize(Settings.MinWindowWidth, Settings.MinWindowHeight);
        Raylib.SetTargetFPS(Settings.Fps);

        while (!Raylib.WindowShouldClose())
        {
            Update();
            Draw();
        }

        Raylib.CloseWindow();
    }

    private void Update()
    {
        RecalculateLayout();

        switch (_state)
        {
            case SolveState.Loading:
                AdvanceLoading();
                return;
            case SolveState.Revealing:
                AdvanceReveal();
                return;
        }

        _input.Update(_board, _layout);
        _algorithmSelector.Update();
        _visualizeToggle.Update();

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            StartSolving();
    }

    private void StartSolving()
    {
        _input.DisableInputMode();
        _algorithmSelector.DisableInputMode();
        _solveStarted = true;

        var solver = SolverFactory.Create(_algorithmSelector.Selected);
        _workingBoard = _board.Clone();
        _recordedSteps.Clear();
        _solveEnumerator = solver.Solve(_workingBoard).GetEnumerator();
        _state = SolveState.Loading;
    }

    /// Computes solver steps against the private working board (never the
    /// real one) for up to InstantSolveFrameBudgetMs per frame, so the UI
    /// stays responsive - regardless of the Visualize toggle - while the
    /// spinner plays over the still-unchanged on-screen board.
    private void AdvanceLoading()
    {
        double deadline = Raylib.GetTime() + Settings.InstantSolveFrameBudgetMs / 1000.0;

        while (Raylib.GetTime() < deadline)
        {
            if (_solveEnumerator!.MoveNext())
            {
                _recordedSteps.Add(_solveEnumerator.Current);
                continue;
            }

            _solveEnumerator.Dispose();
            _solveEnumerator = null;
            BeginReveal();
            return;
        }
    }

    private void BeginReveal()
    {
        if (_visualizeToggle.IsOn)
        {
            _replayIndex = 0;
            _state = SolveState.Revealing;
            return;
        }

        // No animation requested - just stamp the final solved values onto
        // the real board in one shot. O(81), independent of how many steps
        // the solver actually took, so this is always instant.
        CopySolvedCells(_workingBoard!, _board);
        FinishSolve();
    }

    private void AdvanceReveal()
    {
        int steps = Settings.VisualizationStepsPerFrame;
        for (int i = 0; i < steps && _replayIndex < _recordedSteps.Count; i++)
        {
            ApplyStep(_recordedSteps[_replayIndex]);
            _replayIndex++;
        }

        if (_replayIndex >= _recordedSteps.Count)
            FinishSolve();
    }

    private void ApplyStep(SolverStep step)
    {
        switch (step.Kind)
        {
            case StepKind.Place:
                _board.TrySet(step.Row, step.Col, step.Value, CellOrigin.Solved);
                break;
            case StepKind.Undo:
                _board.Clear(step.Row, step.Col);
                break;
            // Solved / Failed carry no board mutation.
        }
    }

    private static void CopySolvedCells(SudokuBoard source, SudokuBoard destination)
    {
        for (int r = 0; r < SudokuBoard.Size; r++)
        for (int c = 0; c < SudokuBoard.Size; c++)
        {
            if (destination.GetValue(r, c) != 0) continue; // given/user cell - leave untouched

            int value = source.GetValue(r, c);
            if (value != 0)
                destination.TrySet(r, c, value, CellOrigin.Solved);
        }
    }

    private void FinishSolve()
    {
        _state = SolveState.Idle;
        _workingBoard = null;
        _recordedSteps.Clear();
    }

    private void RecalculateLayout()
    {
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();

        _layout = ComputeBoardLayout(width, height);

        var (selectorArea, toggleArea) = SplitButtonBar(_layout, width);
        _algorithmSelector.UpdateLayout(selectorArea);
        _visualizeToggle.UpdateBounds(toggleArea);
    }

    private static BoardLayout ComputeBoardLayout(int screenWidth, int screenHeight)
    {
        float barHeight = ComputeButtonBarHeight(screenWidth);
        return BoardLayout.ComputeForScreen(screenWidth, screenHeight, barHeight);
    }

    private static float ComputeButtonBarHeight(int screenWidth)
    {
        float selectorWidth = GetSelectorWidth(screenWidth);
        return AlgorithmSelector.GetRequiredHeight(selectorWidth, AlgorithmSelector.ButtonCount);
    }

    private static float GetSelectorWidth(int screenWidth) =>
        screenWidth - Settings.BoardMargin * 3 - Settings.VisualizeToggleWidth;

    private (Rectangle Selector, Rectangle Toggle) SplitButtonBar(BoardLayout layout, int screenWidth)
    {
        float barHeight = ComputeButtonBarHeight(screenWidth);
        _barArea = layout.GetButtonBarArea(screenWidth, barHeight);

        var toggleArea = new Rectangle(
            _barArea.X + _barArea.Width - Settings.VisualizeToggleWidth,
            _barArea.Y + (_barArea.Height - Settings.ButtonBarHeight) / 2f,
            Settings.VisualizeToggleWidth,
            Settings.ButtonBarHeight);

        var selectorArea = new Rectangle(
            _barArea.X,
            _barArea.Y,
            _barArea.Width - Settings.VisualizeToggleWidth - Settings.BoardMargin,
            _barArea.Height);

        return (selectorArea, toggleArea);
    }

    private void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);

        _renderer.Draw(_board, _input.SelectedCell, _layout);

        // Loading always shows the spinner - the real board hasn't been
        // touched yet in either mode. Revealing only animates when
        // Visualize is on, so no overlay is needed there.
        if (_state == SolveState.Loading)
            _solvingOverlay.Draw(_layout.Bounds, "Solving...");

        _algorithmSelector.Draw();
        _visualizeToggle.Draw();

        if (!_solveStarted)
            DrawStartHint();

        Raylib.EndDrawing();
    }

    private void DrawStartHint()
    {
        const string text = "Press ENTER to start solving";
        var font = Raylib.GetFontDefault();
        const float fontSize = 18f;
        var textSize = Raylib.MeasureTextEx(font, text, fontSize, 1);

        var pos = new Vector2(
            (Raylib.GetScreenWidth() - textSize.X) / 2f,
            _barArea.Y + _barArea.Height + 8f);

        Raylib.DrawTextEx(font, text, pos, fontSize, 1, Color.Gray);
    }
}
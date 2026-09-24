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

    private SudokuBoard? _preSolveSnapshot;
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

        // Reset/cancel takes priority over everything else, and works
        // whether a solve is currently running (Loading/Revealing) or has
        // already finished (Idle with _solveStarted still true).
        if (_solveStarted && Raylib.IsKeyPressed(KeyboardKey.R))
        {
            ResetToPreSolveState();
            return;
        }

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
        _preSolveSnapshot = _board.Clone();

        var solver = SolverFactory.Create(_algorithmSelector.Selected);
        _workingBoard = _board.Clone();
        _recordedSteps.Clear();
        _solveEnumerator = solver.Solve(_workingBoard).GetEnumerator();
        _state = SolveState.Loading;
    }

    private void ResetToPreSolveState()
    {
        _solveEnumerator?.Dispose();
        _solveEnumerator = null;
        _workingBoard = null;
        _recordedSteps.Clear();
        _replayIndex = 0;
        _state = SolveState.Idle;

        if (_preSolveSnapshot is not null)
            _board.RestoreFrom(_preSolveSnapshot);
        _preSolveSnapshot = null;

        _solveStarted = false;
        _input.EnableInputMode();
        _algorithmSelector.EnableInputMode();
    }

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
        }
    }

    private static void CopySolvedCells(SudokuBoard source, SudokuBoard destination)
    {
        for (int r = 0; r < SudokuBoard.Size; r++)
        for (int c = 0; c < SudokuBoard.Size; c++)
        {
            if (destination.GetValue(r, c) != 0) continue;

            int value = source.GetValue(r, c);
            if (value != 0)
                destination.TrySet(r, c, value, CellOrigin.Solved);
        }
    }

    /// Marks the solve as finished. Deliberately leaves _solveStarted and
    /// _preSolveSnapshot untouched - the board stays showing the solved
    /// result, editing stays disabled, and "Press R to reset" keeps
    /// showing until the user explicitly resets.
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

        if (_state == SolveState.Loading)
            _solvingOverlay.Draw(_layout.Bounds, "Solving...");

        _algorithmSelector.Draw();
        _visualizeToggle.Draw();

        DrawStatusHint();

        Raylib.EndDrawing();
    }

    /// Shows "Press ENTER to start solving" above the board before a solve
    /// has begun, or "Press R to reset" while solving/after it's done.
    private void DrawStatusHint()
    {
        string text = _solveStarted ? "Press R to reset" : "Press ENTER to start solving";

        var font = Raylib.GetFontDefault();
        const float fontSize = 18f;
        var textSize = Raylib.MeasureTextEx(font, text, fontSize, 1);

        var pos = new Vector2(
            (Raylib.GetScreenWidth() - textSize.X) / 2f,
            MathF.Max(2f, _layout.Bounds.Y - textSize.Y - 8f));

        Raylib.DrawTextEx(font, text, pos, fontSize, 1, Color.Gray);
    }
}
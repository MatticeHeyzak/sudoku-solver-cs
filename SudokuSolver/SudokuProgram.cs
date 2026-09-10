using Raylib_cs;
using SudokuSolver.Input;
using SudokuSolver.Model;
using SudokuSolver.Renderer;
using SudokuSolver.Solvers;
using SudokuSolver.UI;

namespace SudokuSolver;

public sealed class SudokuProgram
{
    private readonly IRenderer _renderer;
    private readonly SudokuBoard _board = new();
    private readonly InputHandler _input = new();
    private readonly AlgorithmSelector _algorithmSelector;
    private readonly ToggleButton _visualizeToggle;

    private BoardLayout _layout;
    private IEnumerator<SolverStep>? _activeSolve;

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

        if (_activeSolve is not null)
        {
            AdvanceVisualization();
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
        var solver = SolverFactory.Create(_algorithmSelector.Selected);
        _input.DisableInputMode();
        _algorithmSelector.DisableInputMode();

        var steps = solver.Solve(_board).GetEnumerator();

        if (_visualizeToggle.IsOn)
        {
            _activeSolve = steps;
        }
        else
        {
            while (steps.MoveNext()) { }
            steps.Dispose();
        }
    }

    private void AdvanceVisualization()
    {
        for (int i = 0; i < Settings.VisualizationStepsPerFrame; i++)
        {
            if (_activeSolve!.MoveNext())
                continue;

            _activeSolve.Dispose();
            _activeSolve = null;
            break;
        }
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

    /// Reserves however much vertical space the (possibly wrapped) algorithm
    /// selector needs before computing the board's square bounds.
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

    /// Splits the button bar into an algorithm-selector region (left, may be
    /// multi-row) and a fixed-size visualize toggle (right, vertically centered).
    private static (Rectangle Selector, Rectangle Toggle) SplitButtonBar(BoardLayout layout, int screenWidth)
    {
        float barHeight = ComputeButtonBarHeight(screenWidth);
        var barArea = layout.GetButtonBarArea(screenWidth, barHeight);

        var toggleArea = new Rectangle(
            barArea.X + barArea.Width - Settings.VisualizeToggleWidth,
            barArea.Y + (barArea.Height - Settings.ButtonBarHeight) / 2f,
            Settings.VisualizeToggleWidth,
            Settings.ButtonBarHeight);

        var selectorArea = new Rectangle(
            barArea.X,
            barArea.Y,
            barArea.Width - Settings.VisualizeToggleWidth - Settings.BoardMargin,
            barArea.Height);

        return (selectorArea, toggleArea);
    }

    private void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
        _renderer.Draw(_board, _input.SelectedCell, _layout);
        _algorithmSelector.Draw();
        _visualizeToggle.Draw();
        Raylib.EndDrawing();
    }
}
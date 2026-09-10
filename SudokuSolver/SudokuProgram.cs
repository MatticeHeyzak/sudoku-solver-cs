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

    private BoardLayout _layout;

    public SudokuProgram(IRenderer renderer)
    {
        _renderer = renderer;

        _layout = BoardLayout.ComputeForScreen(Settings.WindowWidth, Settings.WindowHeight);
        _algorithmSelector = new AlgorithmSelector(_layout.GetButtonBarArea(Settings.WindowWidth));
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

        _input.Update(_board, _layout);
        _algorithmSelector.Update();

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            var solver = SolverFactory.Create(_algorithmSelector.Selected);
            var solved = solver.TrySolve(_board);
            _input.DisableInputMode();
            _algorithmSelector.DisableInputMode();

            if (!solved)
            {
                Console.WriteLine("Failed to find a solution");
            }
                
        }
    }

    private void RecalculateLayout()
    {
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();

        _layout = BoardLayout.ComputeForScreen(width, height);
        _algorithmSelector.UpdateLayout(_layout.GetButtonBarArea(width));
    }

    private void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
        _renderer.Draw(_board, _input.SelectedCell, _layout);
        _algorithmSelector.Draw();
        Raylib.EndDrawing();
    }
}
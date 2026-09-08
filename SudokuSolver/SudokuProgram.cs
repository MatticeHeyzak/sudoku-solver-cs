using Raylib_cs;
using SudokuSolver.Input;
using SudokuSolver.Model;
using SudokuSolver.Renderer;
using SudokuSolver.Solvers;
using SudokuSolver.UI;

namespace SudokuSolver;

public sealed class SudokuProgram
{

    private IRenderer _renderer;
    private readonly SudokuBoard _board = new();
    private readonly InputHandler _input;
    private readonly AlgorithmSelector _algorithmSelector;

    public SudokuProgram(
        IRenderer renderer,
        BoardLayout layout)
    {
        _renderer = renderer;
        _input = new InputHandler(layout);
        
        var barArea = new Rectangle(
            0,
            layout.Bounds.Y + layout.Bounds.Height + 20,
            Settings.WindowWidth,
            Settings.ButtonBarHeight);

        _algorithmSelector = new AlgorithmSelector(barArea);
    }
    
    public void Run()
    {
        Raylib.InitWindow(Settings.WindowWidth, Settings.WindowHeight, Settings.WindowName);
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
        _input.Update(_board);
        _algorithmSelector.Update();

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            var solver = SolverFactory.Create(_algorithmSelector.Selected);
            solver.TrySolve(_board);
            _input.DisableInputMode();
            _algorithmSelector.DisableInputMode();
        }
    }

    private void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
        _renderer.Draw(_board, _input.SelectedCell);
        _algorithmSelector.Draw();
        Raylib.EndDrawing();
    }
}
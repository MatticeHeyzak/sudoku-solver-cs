using Raylib_cs;
using SudokuSolver.Renderer;

namespace SudokuSolver;

public sealed class SudokuProgram
{

    private IRenderer _renderer;

    public SudokuProgram(
        IRenderer renderer)
    {
        _renderer = renderer;
    }
    
    public void Run()
    {
        Raylib.InitWindow(
            Settings.WindowWidth,
            Settings.WindowHeight,
            Settings.WindowName);
        
        Raylib.SetTargetFPS(Settings.Fps);

        while (!Raylib.WindowShouldClose())
        {
            Update();
            Draw();
        }
    }
    
    private void Update()
    {}

    private void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
        _renderer.Draw();
        Raylib.EndDrawing();
    }
}
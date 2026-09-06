using Raylib_cs;

namespace SudokuSolver;

public sealed class SudokuProgram
{
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
        Raylib.EndDrawing();
    }
}
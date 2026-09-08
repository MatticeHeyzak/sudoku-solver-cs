using SudokuSolver;
using SudokuSolver.Renderer;

var layout = BoardLayout.FromWindow(Settings.WindowWidth);
var renderer = new SudokuRenderer(layout);

var program = new SudokuProgram(renderer, layout);
program.Run();

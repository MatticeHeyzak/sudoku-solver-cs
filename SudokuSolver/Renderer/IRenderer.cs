using SudokuSolver.Model;

namespace SudokuSolver.Renderer;

public interface IRenderer
{
    void Draw(SudokuBoard board, (int Row, int Col)? selectedCell, BoardLayout layout);
}
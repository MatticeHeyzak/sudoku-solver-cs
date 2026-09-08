using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class BacktrackingSolver : ISudokuSolver
{
    public string DisplayName => "Backtracking";

    public bool TrySolve(SudokuBoard board)
    {
        return false;
    }
}
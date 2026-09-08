using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class BacktrackingMrvSolver : ISudokuSolver
{
    public string DisplayName => "Backtracking with MRV";

    public bool TrySolve(SudokuBoard board)
    {
        return false;
    }
}
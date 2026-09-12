using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class BacktrackingMrvSolver : ISudokuSolver
{
    public string DisplayName => "Backtracking with MRV";

    public IEnumerable<SolverStep> Solve(SudokuBoard board) =>
        BacktrackingCore.Solve(board, b => b.FindMostConstrainedCell());
}
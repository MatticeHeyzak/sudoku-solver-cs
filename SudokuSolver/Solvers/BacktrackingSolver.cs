using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class BacktrackingSolver : ISudokuSolver
{
    public string DisplayName => "Backtracking";

    public IEnumerable<SolverStep> Solve(SudokuBoard board) =>
        BacktrackingCore.Solve(board, b => b.FindFirstEmpty());
}
using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class DancingLinksSolver : ISudokuSolver
{
    public string DisplayName => "Dancing links";

    public IEnumerable<SolverStep> Solve(SudokuBoard board)
    {
        yield return SolverStep.Failed();
    }
}
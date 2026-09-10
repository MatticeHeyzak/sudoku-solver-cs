using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class ConstraintPropagationSolver : ISudokuSolver
{
    public string DisplayName => "Constraint propagation";

    public IEnumerable<SolverStep> Solve(SudokuBoard board)
    {
        yield return SolverStep.Failed();
    }
}
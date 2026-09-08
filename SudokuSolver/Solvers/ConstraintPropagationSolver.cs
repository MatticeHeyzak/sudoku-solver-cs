using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class ConstraintPropagationSolver : ISudokuSolver
{
    public string DisplayName => "Constraint propagation";

    public bool TrySolve(SudokuBoard board)
    {
        return false;
    }
}
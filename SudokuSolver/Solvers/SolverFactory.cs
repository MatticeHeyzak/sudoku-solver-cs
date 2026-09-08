namespace SudokuSolver.Solvers;

public static class SolverFactory
{
    public static ISudokuSolver Create(SolverType type) => type switch
    {
        SolverType.Backtracking => new BacktrackingSolver(),
        SolverType.BacktrackingWithMrv => new BacktrackingMrvSolver(),
        SolverType.ConstraintPropagation => new ConstraintPropagationSolver(),
        SolverType.DancingLinks => new DancingLinksSolver(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
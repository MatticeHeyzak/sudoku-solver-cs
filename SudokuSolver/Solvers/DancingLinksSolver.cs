using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class DancingLinksSolver : ISudokuSolver
{
    public string DisplayName => "Dancing links";

    public bool TrySolve(SudokuBoard board)
    {
        return false;
    }
}
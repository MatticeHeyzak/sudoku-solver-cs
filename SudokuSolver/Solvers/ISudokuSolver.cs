using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public interface ISudokuSolver
{
    string DisplayName { get; }
    
    IEnumerable<SolverStep> Solve(SudokuBoard board);
}
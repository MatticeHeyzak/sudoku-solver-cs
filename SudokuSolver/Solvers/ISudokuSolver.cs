using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public interface ISudokuSolver
{
    string DisplayName { get; }
    
    bool TrySolve(SudokuBoard board);
}
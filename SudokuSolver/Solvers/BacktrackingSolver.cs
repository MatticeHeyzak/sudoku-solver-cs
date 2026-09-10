using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class BacktrackingSolver : ISudokuSolver
{
    public string DisplayName => "Backtracking";

    public IEnumerable<SolverStep> Solve(SudokuBoard board)
    {
        return SolveRecursive(board);
    }
    
    private static IEnumerable<SolverStep> SolveRecursive(SudokuBoard board)
    {
        var next = board.FindFirstEmpty();
        if (next is null)
        {
            yield return SolverStep.Solved();
            yield break;
        }

        var (row, col) = next.Value;
        int candidates = board.GetCandidateMask(row, col);

        foreach (int digit in SudokuBoard.EnumerateDigits(candidates))
        {
            if (!board.TrySet(row, col, digit, CellOrigin.Solved))
                continue;
            
            yield return SolverStep.Place(row, col, digit);

            foreach (var inner in SolveRecursive(board))
            {
                yield return inner;

                if (inner.Kind == StepKind.Solved)
                    yield break; // solution found further down - unwind without undoing
            }
            
            board.Clear(row, col);
            yield return SolverStep.Undo(row, col);
        }
        
        yield return SolverStep.Failed();
    }
}
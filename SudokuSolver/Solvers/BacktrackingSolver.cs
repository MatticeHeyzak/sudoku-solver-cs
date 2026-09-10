using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class BacktrackingSolver : ISudokuSolver
{
    public string DisplayName => "Backtracking";

    public bool TrySolve(SudokuBoard board)
    {
        return Solve(board);
    }
    
    private static bool Solve(SudokuBoard board)
    {
        var next = board.FindFirstEmpty();
        if (next is null)
            return true;

        var (row, col) = next.Value;
        int candidates = board.GetCandidateMask(row, col);

        foreach (int digit in SudokuBoard.EnumerateDigits(candidates))
        {
            if (!board.TrySet(row, col, digit, CellOrigin.Solved))
                continue; // shouldn't happen since digit came from candidate but stay safe

            if (Solve(board))
                return true;
            
            board.Clear(row, col); // undo and try next candidate
        }

        return false; // no candidate worked -> backtrack
    }
}
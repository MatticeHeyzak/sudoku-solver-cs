using System.Numerics;
using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class ConstraintPropagationSolver : ISudokuSolver
{
    private const int GridSize = SudokuBoard.Size;
    public string DisplayName => "Constraint propagation";
    public IEnumerable<SolverStep> Solve(SudokuBoard board) => SolveRecursive(board);

    private static IEnumerable<SolverStep> SolveRecursive(SudokuBoard board)
    {
        var propagated = new List<(int Row, int Col)>();

        // Repeatedly apply forced movess (naked singles / hidden singles).
        // These are logically guaranteed by the current board state, not
        // guesses, so they're applied unconditionally before any branching.
        while (FindForcedMove(board, out int fRow, out int fCol, out int fDigit))
        {
            board.TrySet(fRow, fCol, fDigit, CellOrigin.Solved);
            propagated.Add((fRow, fCol));
            yield return SolverStep.Place(fRow, fCol, fDigit);
        }
        
        // FindMostConstrainedCell returns null once the board is full, or a
        // cell with 0 candidates if propagation reached a dead end.
        var next = board.FindMostConstrainedCell();
        if (next is null)
        {
            yield return SolverStep.Solved();
            yield break;
        }

        var (row, col) = next.Value;
        int candidates = board.GetCandidateMask(row, col);

        while (candidates != 0)
        {
            int bit = candidates & -candidates;
            int digit = BitOperations.TrailingZeroCount(bit) + 1;
            candidates &= candidates - 1;

            if (!board.TrySet(row, col, digit, CellOrigin.Solved))
                continue;
            
            yield return SolverStep.Place(row, col, digit);

            foreach (var inner in SolveRecursive(board))
            {
                yield return inner;
                if (inner.Kind == StepKind.Solved)
                    yield break;
            }
            
            board.Clear(row, col);
            yield return SolverStep.Undo(row, col);
        }
        
        // This guess (or dead end) didn't pan out - undo everything forced
        // moves placed in this call frame before letting the caller retreact
        // its own guess.
        for (int i = propagated.Count - 1; i >= 0; i--)
        {
            var (pRow, pCol) = propagated[i];
            board.Clear(pRow, pCol);
            yield return SolverStep.Undo(pRow, pCol);
        }

        yield return SolverStep.Failed();
    }

    /// <summary>
    /// Finds one deterministically-forced empty cell: either a naked single
    /// (only one candidate digit remains) or a hidden single (a digit that
    /// only fits one cell within some row/column/box). Returns false if no
    /// forced move currently exists
    /// </summary>
    private static bool FindForcedMove(SudokuBoard board, out int row, out int col, out int digit)
    {
        if (FindNakedSingle(board, out row, out col, out digit))
            return true;

        return FindHiddenSingle(board, out row, out col, out digit);
    }
    
    private static bool FindNakedSingle(SudokuBoard board, out int row, out int col, out int digit)
    {
        for (int r = 0; r < GridSize; r++)
        for (int c = 0; c < GridSize; c++)
        {
            int mask = board.GetCandidateMask(r, c);
            if (mask != 0 && BitOperations.PopCount((uint)mask) == 1)
            {
                row = r;
                col = c;
                digit = BitOperations.TrailingZeroCount(mask) + 1;
                return true;
            }
        }

        row = col = digit = -1;
        return false;
    }

private static bool FindHiddenSingle(SudokuBoard board, out int row, out int col, out int digit)
    {
        for (int r = 0; r < GridSize; r++)
            if (TryFindHiddenSingleInUnit(board, RowCell(r), out row, out col, out digit))
                return true;
        
        for (int c = 0; c < GridSize; c++)
            if (TryFindHiddenSingleInUnit(board, ColumnCell(c), out row, out col, out digit))
                return true;
        
        for (int b = 0; b < GridSize; b++)
            if (TryFindHiddenSingleInUnit(board, BoxCell(b), out row, out col, out digit))
                return true;

        row = col = digit = -1;
        return false;
    }
    
    /// <summary>
    /// Checks one 9-cell unit (row, column, or box). For each digit 1-9,
    /// if exactly one empty cell in the unit can legally hold it, that's a
    /// hidden single.
    /// </summary>
    private static bool TryFindHiddenSingleInUnit(
        SudokuBoard board,
        Func<int, (int Row, int Col)> cellAt,
        out int row, out int col, out int digit)
    {
        for (int d = 1; d <= GridSize; d++)
        {
            int bit = 1 << (d - 1);
            int matchCount = 0;
            int matchIndex = -1;

            for (int i = 0; i < GridSize; i++)
            {
                var (r, c) = cellAt(i);
                if ((board.GetCandidateMask(r, c) & bit) == 0)
                    continue;

                matchCount++;
                matchIndex = i;
                if (matchCount > 1)
                    break;
            }

            if (matchCount == 1)
            {
                (row, col) = cellAt(matchIndex);
                digit = d;
                return true;
            }
        }

        row = col = digit = -1;
        return false;
    }

    private static Func<int, (int Row, int Col)> RowCell(int row) => i => (row, i);
    private static Func<int, (int Col, int Row)> ColumnCell(int col) => i => (col, i);
    
    private static Func<int, (int Row, int Col)> BoxCell(int box)
    {
        int startRow = (box / SudokuBoard.Box) * SudokuBoard.Box;
        int startCol = (box % SudokuBoard.Box) * SudokuBoard.Box;
        return i => (startRow + i / SudokuBoard.Box, startCol + i % SudokuBoard.Box);
    }
}
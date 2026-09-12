using System.Numerics;
using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

internal static class BacktrackingCore
{
    public static IEnumerable<SolverStep> Solve(
        SudokuBoard board,
        Func<SudokuBoard, (int Row, int Col)?> selectCell)
    {
        var next = selectCell(board);
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
            candidates &= candidates - 1; // clear lowest set bit - avoids an enumerator allocation per recursion

            if (!board.TrySet(row, col, digit, CellOrigin.Solved))
                continue;

            yield return SolverStep.Place(row, col, digit);

            foreach (var inner in Solve(board, selectCell))
            {
                yield return inner;
                if (inner.Kind == StepKind.Solved)
                    yield break;
            }

            board.Clear(row, col);
            yield return SolverStep.Undo(row, col);
        }

        yield return SolverStep.Failed();
    }
}
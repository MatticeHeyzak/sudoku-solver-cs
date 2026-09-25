using SudokuSolver.Model;

namespace SudokuSolver.Solvers;

public sealed class DancingLinksSolver : ISudokuSolver
{
    private const int Digits = 9;
    private const int ConstraintsPerType = 81;
    private const int TotalColumns = ConstraintsPerType * 4;
    
    public string DisplayName => "Dancing links";

    public IEnumerable<SolverStep> Solve(SudokuBoard board)
    {
        var header = BuildMatrix(board);
        return Search(board, header);
    }

    // One node per (constraint-type, cell/row/col/box + digit) intersection
    // that a cnadidate placement satisfies. Regular nodes carry the
    // (row, col, digit) they represent; ColumnNode is the header of a
    // constraint's vertical list
    private class Node
    {
        public Node Left = null!, Right = null!, Up = null!, Down = null!;
        public ColumnNode Column = null!;
        public int Row, Col, Digit;
    }

    private sealed class ColumnNode : Node
    {
        public int Size;

        public ColumnNode()
        {
            Column = this;
            Left = Right = Up = Down = this;
        }
    }

    private static int CellConstraint(int row, int col) => row * Digits + col;
    private static int RowDigitConstraint(int row, int digit) => ConstraintsPerType + row * Digits + (digit - 1);
    private static int ColDigitConstraint(int col, int digit) => ConstraintsPerType * 2 + col * Digits + (digit - 1);
    private static int BoxDigitConstraint(int box, int digit) => ConstraintsPerType * 3 + box * Digits + (digit - 1);
    
    /// Builds the full 729-row/324-column exact cover matrix. Cells that are
    /// already filled (givens) only get a single candidate row for their
    /// existing digit, which forces Algorithm X to select that row - so
    /// givens are respected without any special-case logic in the search.
    private static ColumnNode BuildMatrix(SudokuBoard board)
    {
        var header = new ColumnNode();
        var columns = new ColumnNode[TotalColumns];

        for (int i = 0; i < TotalColumns; i++)
        {
            var column = new ColumnNode();
            columns[i] = column;

            column.Right = header;
            column.Left = header.Left;
            header.Left.Right = column;
            header.Left = column;
        }

        for (int row = 0; row < Digits; row++)
        for (int col = 0; col < Digits; col++)
        {
            int existing = board.GetValue(row, col);

            for (int digit = 1; digit <= Digits; digit++)
            {
                if (existing != 0 && existing != digit) continue;
                if (existing == 0 && !board.CanPlace(row, col, digit)) continue;

                AddCandidateRow(columns, row, col, digit);
            }
        }

        return header;
    }
    
    private static void AddCandidateRow(ColumnNode[] columns, int row, int col, int digit)
    {
        int box = SudokuBoard.BoxIndex(row, col);
        Span<int> columnIndices =
        [
            CellConstraint(row, col),
            RowDigitConstraint(row, digit),
            ColDigitConstraint(col, digit),
            BoxDigitConstraint(box, digit)
        ];

        Node? first = null;
        Node? previous = null;

        foreach (int columnIndex in columnIndices)
        {
            var column = columns[columnIndex];
            var node = new Node { Column = column, Row = row, Col = col, Digit = digit };

            // Insert at the bottom of the column's vertical list.
            node.Up = column.Up;
            node.Down = column;
            column.Up.Down = node;
            column.Up = node;
            column.Size++;

            // Link into this candidate's horizontal (row) list.
            if (first is null)
            {
                first = node;
                node.Left = node.Right = node;
            }
            else
            {
                node.Left = previous!;
                node.Right = first;
                previous!.Right = node;
                first.Left = node;
            }

            previous = node;
        }
    }
    
        private static IEnumerable<SolverStep> Search(SudokuBoard board, ColumnNode header)
    {
        if (header.Right == header)
        {
            // Every constraint is covered - a full, valid placement was chosen.
            yield return SolverStep.Solved();
            yield break;
        }

        var column = ChooseColumn(header);
        if (column.Size == 0)
        {
            // A constraint has no remaining candidate - dead end, backtrack.
            yield return SolverStep.Failed();
            yield break;
        }

        Cover(column);

        for (Node candidate = column.Down; candidate != column; candidate = candidate.Down)
        {
            bool mutatedBoard = board.GetValue(candidate.Row, candidate.Col) == 0;
            if (mutatedBoard)
            {
                board.TrySet(candidate.Row, candidate.Col, candidate.Digit, CellOrigin.Solved);
                yield return SolverStep.Place(candidate.Row, candidate.Col, candidate.Digit);
            }

            for (Node node = candidate.Right; node != candidate; node = node.Right)
                Cover(node.Column);

            foreach (var inner in Search(board, header))
            {
                yield return inner;
                if (inner.Kind == StepKind.Solved)
                    yield break;
            }

            for (Node node = candidate.Left; node != candidate; node = node.Left)
                Uncover(node.Column);

            if (mutatedBoard)
            {
                board.Clear(candidate.Row, candidate.Col);
                yield return SolverStep.Undo(candidate.Row, candidate.Col);
            }
        }

        Uncover(column);

        yield return SolverStep.Failed();
    }

    /// Picks the constraint with the fewest remaining candidates - the DLX
    /// analogue of the MRV heuristic, but applied across all 4 constraint
    /// types (cell/row/col/box) instead of only "digits left for a cell".
    private static ColumnNode ChooseColumn(ColumnNode header)
    {
        var best = (ColumnNode)header.Right;
        for (Node node = header.Right.Right; node != header; node = node.Right)
        {
            var column = (ColumnNode)node;
            if (column.Size < best.Size)
                best = column;
        }
        return best;
    }

    private static void Cover(ColumnNode column)
    {
        column.Right.Left = column.Left;
        column.Left.Right = column.Right;

        for (Node row = column.Down; row != column; row = row.Down)
        for (Node node = row.Right; node != row; node = node.Right)
        {
            node.Down.Up = node.Up;
            node.Up.Down = node.Down;
            node.Column.Size--;
        }
    }

    private static void Uncover(ColumnNode column)
    {
        for (Node row = column.Up; row != column; row = row.Up)
        for (Node node = row.Left; node != row; node = node.Left)
        {
            node.Column.Size++;
            node.Down.Up = node;
            node.Up.Down = node;
        }

        column.Right.Left = column;
        column.Left.Right = column;
    }
}
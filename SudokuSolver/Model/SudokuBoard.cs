namespace SudokuSolver.Model;

public enum CellOrigin
{
    Empty,
    Given,
    User,
    Solved
}

public sealed class SudokuBoard
{
    public const int Size = 9;
    public const int Box = 3;
    public const int FullMask = 0x1FF; // 9 bits, one per digit 1-9

    private readonly int[,] _values = new int[Size, Size];
    private readonly CellOrigin[,] _origin = new CellOrigin[Size, Size];

    private readonly int[] _rowMask = new int[Size];
    private readonly int[] _colMask = new int[Size];
    private readonly int[] _boxMask = new int[Size];

    public int GetValue(int row, int col) => _values[row, col];
    public CellOrigin GetOrigin(int row, int col) => _origin[row, col];
    
    public static int BoxIndex(int row, int col) => (row / Box) * Box + (col / Box);
    
    public bool IsGiven(int row, int col) => _origin[row, col] == CellOrigin.Given;

    public bool CanPlace(int row, int col, int value)
    {
        if (value is < 1 or > 9)
            return false;
        
        int bit = 1 << (value - 1);
        int box = BoxIndex(row, col);
        return ((_rowMask[row] | _colMask[col] | _boxMask[box]) & bit) == 0;
    }

    public bool TrySet(int row, int col, int value, CellOrigin origin)
    {
        if (IsGiven(row, col))
            return false;
        
        Clear(row, col);

        if (value == 0)
            return true;

        if (!CanPlace(row, col, value))
            return false;

        int bit = 1 << (value - 1);
        int box = BoxIndex(row, col);

        _values[row, col] = value;
        _origin[row, col] = origin;
        _rowMask[row] |= bit;
        _colMask[col] |= bit;
        _boxMask[box] |= bit;
        return true;
    }
    
    public void Clear(int row, int col)
    {
        int old = _values[row, col];
        if (old != 0)
        {
            int bit = 1 << (old - 1);
            int box = BoxIndex(row, col);
            _rowMask[row] &= ~bit;
            _colMask[col] &= ~bit;
            _boxMask[box] &= ~bit;
        }

        _values[row, col] = 0;
        _origin[row, col] = CellOrigin.Empty;
    }

    public int GetCandidateMask(int row, int col)
    {
        if (_values[row, col] != 0)
            return 0;

        int box = BoxIndex(row, col);
        int used = _rowMask[row] | _colMask[col] | _boxMask[box];
        return ~used & FullMask;
    }

    public void LoadPuzzle(int[,] givens)
    {
        for (int r = 0; r < Size; r++)
        for (int c = 0; c < Size; c++)
            Clear(r, c);
        
        for (int r = 0; r < Size; r++)
        for (int c = 0; c < Size; c++)
        {
            int v = givens[r, c];
            if (v != 0)
                TrySet(r, c, v, CellOrigin.Given);
        }
    }

    public bool IsComplete()
    {
        for (int r = 0; r < Size; r++)
            if (_rowMask[r] != FullMask) return false;

        return true;
    }
}
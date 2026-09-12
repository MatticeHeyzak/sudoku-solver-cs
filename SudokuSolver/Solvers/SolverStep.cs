namespace SudokuSolver.Solvers;

public enum StepKind
{
    Place,
    Undo,
    Solved,
    Failed
}

/// <summary>
/// A single visualized unit of solver work.
/// </summary>
public readonly record struct SolverStep(StepKind Kind, int Row, int Col, int Value)
{
    public static SolverStep Place(int row, int col, int value) => new(StepKind.Place, row, col, value);
    public static SolverStep Undo(int row, int col) => new(StepKind.Undo, row, col, 0);
    public static SolverStep Solved() => new(StepKind.Solved, -1, -1, 0);
    public static SolverStep Failed() => new(StepKind.Failed, -1, -1, 0);
};
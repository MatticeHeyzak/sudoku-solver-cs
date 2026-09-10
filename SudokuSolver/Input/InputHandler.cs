using Raylib_cs;
using SudokuSolver.Model;
using SudokuSolver.Renderer;

namespace SudokuSolver.Input;

public sealed class InputHandler
{
    private bool _inputMode = true;

    public (int Row, int Col)? SelectedCell { get; private set; }

    public void Update(SudokuBoard board, BoardLayout layout)
    {
        if (!_inputMode) return;

        HandleMouseSelection(layout);
        HandleKeyboardNavigation();
        HandleDigitInput(board);
    }

    public void DisableInputMode()
    {
        _inputMode = false;
        SelectedCell = null;
    }

    private void HandleMouseSelection(BoardLayout layout)
    {
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;

        var mousePos = Raylib.GetMousePosition();
        if (layout.TryGetCell(mousePos, out int row, out int col))
            SelectedCell = (row, col);
    }

    private void HandleKeyboardNavigation()
    {
        if (SelectedCell is not { } sel)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.Down) ||
                Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.Right))
            {
                SelectedCell = (0, 0);
            }
            return;
        }

        int row = sel.Row;
        int col = sel.Col;

        if (Raylib.IsKeyPressed(KeyboardKey.Up)) row = Math.Max(0, row - 1);
        else if (Raylib.IsKeyPressed(KeyboardKey.Down)) row = Math.Min(BoardLayout.GridSize - 1, row + 1);
        else if (Raylib.IsKeyPressed(KeyboardKey.Left)) col = Math.Max(0, col - 1);
        else if (Raylib.IsKeyPressed(KeyboardKey.Right)) col = Math.Min(BoardLayout.GridSize - 1, col + 1);
        else return;

        SelectedCell = (row, col);
    }

    private void HandleDigitInput(SudokuBoard board)
    {
        if (SelectedCell is not { } sel) return;

        for (int i = 0; i < 9; i++)
        {
            KeyboardKey mainKey = KeyboardKey.One + i;
            KeyboardKey numpadKey = KeyboardKey.Kp1 + i;

            if (Raylib.IsKeyPressed(mainKey) || Raylib.IsKeyPressed(numpadKey))
            {
                int value = i + 1;
                board.TrySet(sel.Row, sel.Col, value, CellOrigin.User);
                return;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) || Raylib.IsKeyPressed(KeyboardKey.Delete))
        {
            board.TrySet(sel.Row, sel.Col, 0, CellOrigin.Empty);
        }
    }
}
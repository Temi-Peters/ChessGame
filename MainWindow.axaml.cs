using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;

namespace ChessGame
{
    public partial class MainWindow : Window
    {
        private ChessPiece? selectedPiece = null;
        private ChessPiece?[,] board = new ChessPiece?[8,8];
        private bool whiteTurn = true;

        public MainWindow()
        {
            InitializeComponent();
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            var grid = this.FindControl<Grid>("BoardGrid")!;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int currentRow = row;
                    int currentCol = col;

                    var square = new Border
                    {
                        Background = (row + col) % 2 == 0
                            ? Brushes.Beige
                            : Brushes.SaddleBrown
                    };

                    square.PointerPressed += (s, e) =>
                    {
                        OnSquareClicked(currentRow, currentCol);
                    };

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);
                    grid.Children.Add(square);
                }
            }

            SpawnPieces();
        }

        private void SpawnPieces()
        {
            var grid = this.FindControl<Grid>("BoardGrid")!;

            for (int col = 0; col < 8; col++)
            {
                CreatePiece(grid, 1, col, PieceType.Pawn, PieceColor.White);
                CreatePiece(grid, 6, col, PieceType.Pawn, PieceColor.Black);
            }
        }

        private void CreatePiece(Grid grid, int row, int col, PieceType type, PieceColor color)
        {
            var ellipse = new Ellipse
            {
                Fill = color == PieceColor.White ? Brushes.White : Brushes.Black,
                Width = 50,
                Height = 50
            };

            var piece = new ChessPiece(type, color, ellipse);

            ellipse.PointerPressed += (s, e) =>
            {
                if (selectedPiece != null && selectedPiece != piece)
                {
                    int targetRow = Grid.GetRow(piece.Visual);
                    int targetCol = Grid.GetColumn(piece.Visual);
                    OnSquareClicked(targetRow, targetCol);
                }
                else
                {
                    SelectPiece(piece);
                }

                e.Handled = true;
            };

            Grid.SetRow(ellipse, row);
            Grid.SetColumn(ellipse, col);

            board[row, col] = piece;
            grid.Children.Add(ellipse);
        }

        private void SelectPiece(ChessPiece piece)
        {
            // If clicking the already selected piece → deselect it
            if (selectedPiece == piece)
            {
                selectedPiece.Visual.Stroke = null;
                selectedPiece = null;
                return;
            }

            // Enforce turn rules
            if (whiteTurn && piece.Color != PieceColor.White)
                return;

            if (!whiteTurn && piece.Color != PieceColor.Black)
                return;

            if (selectedPiece != null)
                selectedPiece.Visual.Stroke = null;

            selectedPiece = piece;
            piece.Visual.Stroke = Brushes.Yellow;
            piece.Visual.StrokeThickness = 3;
        }
        private void OnSquareClicked(int row, int col)
        {
            if (selectedPiece == null)
                return;

            int oldRow = Grid.GetRow(selectedPiece.Visual);
            int oldCol = Grid.GetColumn(selectedPiece.Visual);

            if (!IsValidMove(selectedPiece, oldRow, oldCol, row, col))
            {
                selectedPiece.Visual.Stroke = null;
                selectedPiece = null;
                return;
            }

            var grid = this.FindControl<Grid>("BoardGrid")!;

            // capture
            if (board[row, col] != null)
            {
                if (board[row, col]!.Color == selectedPiece.Color)
                {
                    selectedPiece.Visual.Stroke = null;
                    selectedPiece = null;
                    return;
                }
                grid.Children.Remove(board[row, col]!.Visual);
            }

            Grid.SetRow(selectedPiece.Visual, row);
            Grid.SetColumn(selectedPiece.Visual, col);

            board[oldRow, oldCol] = null;
            board[row, col] = selectedPiece;

            selectedPiece.Visual.Stroke = null;
            selectedPiece = null;

            whiteTurn = !whiteTurn;
        }

        private bool IsValidMove(ChessPiece piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            switch (piece.Type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(piece, fromRow, fromCol, toRow, toCol);
                default:
                    return true;
            }
        }

        private bool IsValidPawnMove(ChessPiece piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            int direction = piece.Color == PieceColor.White ? 1 : -1;

            // Stay inside board
            if (toRow < 0 || toRow > 7 || toCol < 0 || toCol > 7)
                return false;

            // Move forward 1
            if (toCol == fromCol &&
                toRow == fromRow + direction &&
                board[toRow, toCol] == null)
                return true;

            // Move forward 2 (starting position)
            if (toCol == fromCol &&
                board[toRow, toCol] == null &&
                ((piece.Color == PieceColor.White && fromRow == 1) ||
                (piece.Color == PieceColor.Black && fromRow == 6)) &&
                toRow == fromRow + 2 * direction &&
                board[fromRow + direction, fromCol] == null)
                return true;

            // Diagonal capture
            if (System.Math.Abs(toCol - fromCol) == 1 &&
                toRow == fromRow + direction &&
                board[toRow, toCol] != null &&
                board[toRow, toCol]!.Color != piece.Color)
                return true;

            return false;
        }
    }
}
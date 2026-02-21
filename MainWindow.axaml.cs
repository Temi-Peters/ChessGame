using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;

namespace ChessGame
{
    public partial class MainWindow : Window
    {
        private Rectangle[,] overlays = new Rectangle[8, 8];
        private ChessPiece? selectedPiece = null;
        private ChessPiece?[,] board = new ChessPiece?[8, 8];
        private bool whiteTurn = true;
        private Border[,] squares = new Border[8, 8];

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
                        Background = (row + col) % 2 == 0 ? Brushes.Beige : Brushes.SaddleBrown
                    };

                    square.PointerPressed += (s, e) =>
                    {
                        OnSquareClicked(currentRow, currentCol);
                    };

                    squares[row, col] = square;

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);
                    grid.Children.Add(square);

                    var overlay = new Rectangle
                    {
                        Fill = Brushes.Transparent,
                        IsHitTestVisible = false
                    };

                    overlays[row, col] = overlay;

                    Grid.SetRow(overlay, row);
                    Grid.SetColumn(overlay, col);
                    grid.Children.Add(overlay);
                }
            }

            SpawnPieces();
        }

        private void SpawnPieces()
        {
            var grid = this.FindControl<Grid>("BoardGrid")!;

            // Pawns
            for (int col = 0; col < 8; col++)
            {
                CreatePiece(grid, 6, col, PieceType.Pawn, PieceColor.White);
                CreatePiece(grid, 1, col, PieceType.Pawn, PieceColor.Black);
            }

            // Rooks
            CreatePiece(grid, 7, 0, PieceType.Rook, PieceColor.White);
            CreatePiece(grid, 7, 7, PieceType.Rook, PieceColor.White);
            CreatePiece(grid, 0, 0, PieceType.Rook, PieceColor.Black);
            CreatePiece(grid, 0, 7, PieceType.Rook, PieceColor.Black);

            // Knights
            CreatePiece(grid, 7, 1, PieceType.Knight, PieceColor.White);
            CreatePiece(grid, 7, 6, PieceType.Knight, PieceColor.White);
            CreatePiece(grid, 0, 1, PieceType.Knight, PieceColor.Black);
            CreatePiece(grid, 0, 6, PieceType.Knight, PieceColor.Black);

            // Bishops
            CreatePiece(grid, 7, 2, PieceType.Bishop, PieceColor.White);
            CreatePiece(grid, 7, 5, PieceType.Bishop, PieceColor.White);
            CreatePiece(grid, 0, 2, PieceType.Bishop, PieceColor.Black);
            CreatePiece(grid, 0, 5, PieceType.Bishop, PieceColor.Black);

            // Queens
            CreatePiece(grid, 7, 3, PieceType.Queen, PieceColor.White);
            CreatePiece(grid, 0, 3, PieceType.Queen, PieceColor.Black);

            // Kings
            CreatePiece(grid, 7, 4, PieceType.King, PieceColor.White);
            CreatePiece(grid, 0, 4, PieceType.King, PieceColor.Black);
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
                SelectPiece(piece);
                e.Handled = true;
            };

            Grid.SetRow(ellipse, row);
            Grid.SetColumn(ellipse, col);

            board[row, col] = piece;
            grid.Children.Add(ellipse);
        }

        private void SelectPiece(ChessPiece piece)
        {
            if (whiteTurn && piece.Color != PieceColor.White)
                return;

            if (!whiteTurn && piece.Color != PieceColor.Black)
                return;

            if (selectedPiece == piece)
            {
                selectedPiece.Visual.Stroke = null;
                selectedPiece = null;
                ResetBoardColors();
                return;
            }

            if (selectedPiece != null)
                selectedPiece.Visual.Stroke = null;

            ResetBoardColors();

            selectedPiece = piece;
            piece.Visual.Stroke = Brushes.Yellow;
            piece.Visual.StrokeThickness = 3;

            var pos = FindPiecePosition(piece);
            if (pos != null && piece.Type == PieceType.Pawn)
            {
                HighlightPawnMoves(piece, pos.Value.row, pos.Value.col);
            }
        }

        private (int row, int col)? FindPiecePosition(ChessPiece piece)
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (board[r, c] == piece)
                        return (r, c);

            return null;
        }

        private void HighlightPawnMoves(ChessPiece piece, int fromRow, int fromCol)
        {
            ResetBoardColors();

            var green = new SolidColorBrush(Color.FromArgb(120, 0, 255, 0));

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (IsValidPawnMove(piece, fromRow, fromCol, r, c))
                        overlays[r, c].Fill = green;
        }

        private void ResetBoardColors()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    overlays[r, c].Fill = Brushes.Transparent;
        }

        private void OnSquareClicked(int row, int col)
        {
            if (selectedPiece == null)
                return;

            int oldRow = Grid.GetRow(selectedPiece.Visual);
            int oldCol = Grid.GetColumn(selectedPiece.Visual);

            if (!IsValidMove(selectedPiece, oldRow, oldCol, row, col))
                return;

            var grid = this.FindControl<Grid>("BoardGrid")!;

            if (board[row, col] != null)
            {
                if (board[row, col]!.Color == selectedPiece.Color)
                    return;

                grid.Children.Remove(board[row, col]!.Visual);
            }

            Grid.SetRow(selectedPiece.Visual, row);
            Grid.SetColumn(selectedPiece.Visual, col);

            board[oldRow, oldCol] = null;
            board[row, col] = selectedPiece;

            selectedPiece.Visual.Stroke = null;
            selectedPiece = null;

            ResetBoardColors();

            whiteTurn = !whiteTurn;
        }

        private bool IsValidMove(ChessPiece piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            return piece.Type switch
            {
                PieceType.Pawn => IsValidPawnMove(piece, fromRow, fromCol, toRow, toCol),
                _ => false
            };
        }

        private bool IsValidPawnMove(ChessPiece piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            int direction = piece.Color == PieceColor.White ? -1 : 1;

            if (toRow < 0 || toRow > 7 || toCol < 0 || toCol > 7)
                return false;

            if (toCol == fromCol &&
                toRow == fromRow + direction &&
                board[toRow, toCol] == null)
                return true;

            if (toCol == fromCol &&
                board[toRow, toCol] == null &&
                ((piece.Color == PieceColor.White && fromRow == 6) ||
                 (piece.Color == PieceColor.Black && fromRow == 1)) &&
                toRow == fromRow + 2 * direction &&
                board[fromRow + direction, fromCol] == null)
                return true;

            if (System.Math.Abs(toCol - fromCol) == 1 &&
                toRow == fromRow + direction &&
                board[toRow, toCol] != null &&
                board[toRow, toCol]!.Color != piece.Color)
                return true;

            return false;
        }
    }
}
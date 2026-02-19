using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;
using System.Diagnostics;

namespace ChessGame
{    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            var grid = this.FindControl<Grid>("BoardGrid")!; // null-forgiving operator

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new Border
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.Beige : Brushes.SaddleBrown
                    };

                    square.PointerPressed += (s, e) =>
                    {
                        Debug.WriteLine($"Clicked square: {row},{col}");
                    };

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);
                    grid.Children.Add(square);

                    SpawnPieces();
                }
            }
        }

        // Generates the chess pieces on the board
        private void SpawnPieces()
        {
            var grid = this.FindControl<Grid>("BoardGrid")!;

            // Pawns
            for (int col = 0; col < 8; col++)
            {
                SpawnPiece(grid, 1, col, PieceType.Pawn, PieceColor.White);
                SpawnPiece(grid, 6, col, PieceType.Pawn, PieceColor.Black);
            }

            // Rooks
            SpawnPiece(grid, 0, 0, PieceType.Rook, PieceColor.White);
            SpawnPiece(grid, 0, 7, PieceType.Rook, PieceColor.White);
            SpawnPiece(grid, 7, 0, PieceType.Rook, PieceColor.Black);
            SpawnPiece(grid, 7, 7, PieceType.Rook, PieceColor.Black);

            // Knights
            SpawnPiece(grid, 0, 1, PieceType.Knight, PieceColor.White);
            SpawnPiece(grid, 0, 6, PieceType.Knight, PieceColor.White);
            SpawnPiece(grid, 7, 1, PieceType.Knight, PieceColor.Black);
            SpawnPiece(grid, 7, 6, PieceType.Knight, PieceColor.Black);

            // Bishops
            SpawnPiece(grid, 0, 2, PieceType.Bishop, PieceColor.White);
            SpawnPiece(grid, 0, 5, PieceType.Bishop, PieceColor.White);
            SpawnPiece(grid, 7, 2, PieceType.Bishop, PieceColor.Black);
            SpawnPiece(grid, 7, 5, PieceType.Bishop, PieceColor.Black);

            // Queens
            SpawnPiece(grid, 0, 3, PieceType.Queen, PieceColor.White);
            SpawnPiece(grid, 7, 3, PieceType.Queen, PieceColor.Black);

            // Kings
            SpawnPiece(grid, 0, 4, PieceType.King, PieceColor.White);
            SpawnPiece(grid, 7, 4, PieceType.King, PieceColor.Black);
        }

        // Helper method to create a visual piece
        private void SpawnPiece(Grid grid, int row, int col, PieceType type, PieceColor color)
        {
            var pieceCircle = new Ellipse
            {
                Fill = color == PieceColor.White ? Brushes.White : Brushes.Black,
                Width = 50,
                Height = 50
            };

            Grid.SetRow(pieceCircle, row);
            Grid.SetColumn(pieceCircle, col);
            grid.Children.Add(pieceCircle);
        }

    }
}

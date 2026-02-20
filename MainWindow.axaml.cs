using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;
using System.Diagnostics;

namespace ChessGame
{
    public partial class MainWindow : Window
    {
        private Ellipse? selectedPiece = null;
        private Ellipse?[,] board = new Ellipse?[8,8];

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
                CreatePiece(grid, 1, col, Brushes.White);
                CreatePiece(grid, 6, col, Brushes.Black);
            }

            // rooks
            CreatePiece(grid, 0, 0, Brushes.White);
            CreatePiece(grid, 0, 7, Brushes.White);
            CreatePiece(grid, 7, 0, Brushes.Black);
            CreatePiece(grid, 7, 7, Brushes.Black);

            // knights
            CreatePiece(grid, 0, 1, Brushes.White);
            CreatePiece(grid, 0, 6, Brushes.White);
            CreatePiece(grid, 7, 1, Brushes.Black);
            CreatePiece(grid, 7, 6, Brushes.Black);

            // bishops
            CreatePiece(grid, 0, 2, Brushes.White);
            CreatePiece(grid, 0, 5, Brushes.White);
            CreatePiece(grid, 7, 2, Brushes.Black);
            CreatePiece(grid, 7, 5, Brushes.Black);

            // queens
            CreatePiece(grid, 0, 3, Brushes.White);
            CreatePiece(grid, 7, 3, Brushes.Black);

            // kings
            CreatePiece(grid, 0, 4, Brushes.White);
            CreatePiece(grid, 7, 4, Brushes.Black);
        }

        private void CreatePiece(Grid grid, int row, int col, IBrush color)
        {
            var piece = new Ellipse
            {
                Fill = color,
                Width = 50,
                Height = 50
            };

            piece.PointerPressed += (s, e) =>
            {
                SelectPiece(piece);
                e.Handled = true;
            };

            Grid.SetRow(piece, row);
            Grid.SetColumn(piece, col);

            board[row, col] = piece;
            grid.Children.Add(piece);
        }

        private void SelectPiece(Ellipse piece)
        {
            if (selectedPiece != null)
                selectedPiece.Stroke = null;

            selectedPiece = piece;
            selectedPiece.Stroke = Brushes.Yellow;
            selectedPiece.StrokeThickness = 3;

            int row = Grid.GetRow(piece);
            int col = Grid.GetColumn(piece);
            Debug.WriteLine($"Selected piece at {row},{col}");
        }

        private void OnSquareClicked(int row, int col)
        {
            if (selectedPiece == null)
                return;

            var grid = this.FindControl<Grid>("BoardGrid")!;
            int oldRow = Grid.GetRow(selectedPiece);
            int oldCol = Grid.GetColumn(selectedPiece);

            // capture if present
            if (board[row, col] != null)
            {
                grid.Children.Remove(board[row, col]!);
            }

            // move
            Grid.SetRow(selectedPiece, row);
            Grid.SetColumn(selectedPiece, col);

            // update board array
            board[oldRow, oldCol] = null;
            board[row, col] = selectedPiece;

            selectedPiece.Stroke = null;
            selectedPiece = null;

            Debug.WriteLine($"Moved piece to {row},{col}");
        }
    }
}
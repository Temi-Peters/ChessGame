using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;
using System;

namespace ChessGame
{
    public partial class MainWindow : Window
    {
        private Rectangle[,] overlays = new Rectangle[8, 8];
        private ChessPiece? selectedPiece = null;
        private ChessPiece?[,] board = new ChessPiece?[8, 8];
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
                    int r = row;
                    int c = col;

                    var square = new Border
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.Beige : Brushes.SaddleBrown
                    };

                    square.PointerPressed += (s, e) =>
                    {
                        OnSquareClicked(r, c);
                    };

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

            for (int col = 0; col < 8; col++)
            {
                CreatePiece(grid, 6, col, PieceType.Pawn, PieceColor.White);
                CreatePiece(grid, 1, col, PieceType.Pawn, PieceColor.Black);
            }

            CreatePiece(grid, 7, 0, PieceType.Rook, PieceColor.White);
            CreatePiece(grid, 7, 7, PieceType.Rook, PieceColor.White);
            CreatePiece(grid, 0, 0, PieceType.Rook, PieceColor.Black);
            CreatePiece(grid, 0, 7, PieceType.Rook, PieceColor.Black);

            CreatePiece(grid, 7, 1, PieceType.Knight, PieceColor.White);
            CreatePiece(grid, 7, 6, PieceType.Knight, PieceColor.White);
            CreatePiece(grid, 0, 1, PieceType.Knight, PieceColor.Black);
            CreatePiece(grid, 0, 6, PieceType.Knight, PieceColor.Black);

            CreatePiece(grid, 7, 2, PieceType.Bishop, PieceColor.White);
            CreatePiece(grid, 7, 5, PieceType.Bishop, PieceColor.White);
            CreatePiece(grid, 0, 2, PieceType.Bishop, PieceColor.Black);
            CreatePiece(grid, 0, 5, PieceType.Bishop, PieceColor.Black);

            CreatePiece(grid, 7, 3, PieceType.Queen, PieceColor.White);
            CreatePiece(grid, 0, 3, PieceType.Queen, PieceColor.Black);

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
            if (whiteTurn && piece.Color != PieceColor.White) return;
            if (!whiteTurn && piece.Color != PieceColor.Black) return;

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
            if (pos != null)
                HighlightAllValidMoves(piece, pos.Value.row, pos.Value.col);
        }

        private (int row, int col)? FindPiecePosition(ChessPiece piece)
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (board[r, c] == piece)
                        return (r, c);
            return null;
        }

        private void HighlightAllValidMoves(ChessPiece piece, int fr, int fc)
        {
            ResetBoardColors();
            var green = new SolidColorBrush(Color.FromArgb(120, 0, 255, 0));

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (IsValidMove(piece, fr, fc, r, c))
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
            if (selectedPiece == null) return;

            int oldRow = Grid.GetRow(selectedPiece.Visual);
            int oldCol = Grid.GetColumn(selectedPiece.Visual);

            if (!IsValidMove(selectedPiece, oldRow, oldCol, row, col))
                return;

            var grid = this.FindControl<Grid>("BoardGrid")!;

            if (board[row, col] != null)
            {
                if (board[row, col]!.Color == selectedPiece.Color) return;
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

        private bool IsValidMove(ChessPiece piece, int fr, int fc, int tr, int tc)
        {
            if (tr < 0 || tr > 7 || tc < 0 || tc > 7) return false;
            if (fr == tr && fc == tc) return false;

            return piece.Type switch
            {
                PieceType.Pawn => IsValidPawnMove(piece, fr, fc, tr, tc),
                PieceType.Rook => IsValidRookMove(piece, fr, fc, tr, tc),
                PieceType.Knight => IsValidKnightMove(piece, fr, fc, tr, tc),
                PieceType.Bishop => IsValidBishopMove(piece, fr, fc, tr, tc),
                PieceType.Queen => IsValidQueenMove(piece, fr, fc, tr, tc),
                PieceType.King => IsValidKingMove(piece, fr, fc, tr, tc),
                _ => false
            };
        }

        private bool IsValidPawnMove(ChessPiece piece, int fr, int fc, int tr, int tc)
        {
            int dir = piece.Color == PieceColor.White ? -1 : 1;

            if (tc == fc && tr == fr + dir && board[tr, tc] == null)
                return true;

            if (tc == fc &&
                ((piece.Color == PieceColor.White && fr == 6) ||
                 (piece.Color == PieceColor.Black && fr == 1)) &&
                tr == fr + 2 * dir &&
                board[fr + dir, fc] == null &&
                board[tr, tc] == null)
                return true;

            if (Math.Abs(tc - fc) == 1 &&
                tr == fr + dir &&
                board[tr, tc] != null &&
                board[tr, tc]!.Color != piece.Color)
                return true;

            return false;
        }

        private bool IsValidRookMove(ChessPiece piece, int fr, int fc, int tr, int tc)
        {
            if (fr != tr && fc != tc) return false;

            int rowStep = tr == fr ? 0 : (tr > fr ? 1 : -1);
            int colStep = tc == fc ? 0 : (tc > fc ? 1 : -1);

            int r = fr + rowStep;
            int c = fc + colStep;

            while (r != tr || c != tc)
            {
                if (board[r, c] != null) return false;
                r += rowStep;
                c += colStep;
            }

            return board[tr, tc] == null || board[tr, tc]!.Color != piece.Color;
        }

        private bool IsValidBishopMove(ChessPiece piece, int fr, int fc, int tr, int tc)
        {
            if (Math.Abs(tr - fr) != Math.Abs(tc - fc)) return false;

            int rowStep = tr > fr ? 1 : -1;
            int colStep = tc > fc ? 1 : -1;

            int r = fr + rowStep;
            int c = fc + colStep;

            while (r != tr && c != tc)
            {
                if (board[r, c] != null) return false;
                r += rowStep;
                c += colStep;
            }

            return board[tr, tc] == null || board[tr, tc]!.Color != piece.Color;
        }

        private bool IsValidQueenMove(ChessPiece piece, int fr, int fc, int tr, int tc)
            => IsValidRookMove(piece, fr, fc, tr, tc) ||
               IsValidBishopMove(piece, fr, fc, tr, tc);

        private bool IsValidKnightMove(ChessPiece piece, int fr, int fc, int tr, int tc)
        {
            int dr = Math.Abs(tr - fr);
            int dc = Math.Abs(tc - fc);

            if (!((dr == 2 && dc == 1) || (dr == 1 && dc == 2)))
                return false;

            return board[tr, tc] == null || board[tr, tc]!.Color != piece.Color;
        }

        private bool IsValidKingMove(ChessPiece piece, int fr, int fc, int tr, int tc)
        {
            int dr = Math.Abs(tr - fr);
            int dc = Math.Abs(tc - fc);

            if (dr <= 1 && dc <= 1)
                return board[tr, tc] == null || board[tr, tc]!.Color != piece.Color;

            return false;
        }
    }
}
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ChessGame.Game;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using System;
using Avalonia.Platform;
using System.Linq;

namespace ChessGame;

public partial class MainWindow : Window
{
    private GameState _state = new();
    private (int r, int c)? _selected;
    private List<Move> _legalMoves = new();

    public MainWindow()
    {
        InitializeComponent();
        _state.SetupInitialPosition();
        DrawBoard();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DrawBoard()
    {
        var boardGrid = this.FindControl<UniformGrid>("BoardGrid")
        ?? throw new Exception("BoardGrid not found in XAML");

        var statusText = this.FindControl<TextBlock>("StatusText")
        ?? throw new Exception("StatusText not found in XAML");

        boardGrid.Children.Clear();
        statusText.Text = $"{_state.CurrentTurn} to move";

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var border = new Border
                {
                    Background = (r + c) % 2 == 0
                        ? Brushes.Beige
                        : Brushes.SaddleBrown,
                    Tag = (r, c)
                };

                border.PointerPressed += OnSquareClicked;

                if (_selected.HasValue &&
                    _selected.Value.r == r &&
                    _selected.Value.c == c)
                    border.Background = Brushes.Gold;

                foreach (var move in _legalMoves)
                    if (move.ToRow == r && move.ToCol == c)
                        border.Background = Brushes.LightGreen;

                var piece = _state.Board[r, c];
                if (piece != null)
                {
                    var img = new Image
                    {
                        Source = LoadPieceImage(piece),
                        Stretch = Stretch.Uniform
                    };
                    border.Child = img;
                }

                boardGrid.Children.Add(border);
            }
        }
    }

    private Bitmap LoadPieceImage(Piece piece)
    {
        string prefix = piece.Color == PieceColor.White ? "w" : "b";

        string name = piece.Type switch
        {
            PieceType.Pawn => "p",
            PieceType.Rook => "r",
            PieceType.Knight => "n",
            PieceType.Bishop => "b",
            PieceType.Queen => "q",
            PieceType.King => "k",
            _ => ""
        };

        var uri = new Uri($"avares://ChessGame/Assets/{prefix}{name}.png");

        return new Bitmap(AssetLoader.Open(uri));
    }

    private async void OnSquareClicked(object? sender, PointerPressedEventArgs e)
    {
        var border = (Border)sender!;
        var (r, c) = ((int, int))border.Tag!;

        if (_selected == null)
        {
            var piece = _state.Board[r, c];
            if (piece != null && piece.Color == _state.CurrentTurn)
            {
                _selected = (r, c);
                _legalMoves = MoveGenerator.GenerateLegalMoves(_state, r, c);
            }
        }
        else
        {
            var possibleMoves = _legalMoves
                .Where(m => m.ToRow == r && m.ToCol == c)
                .ToList();

            if (possibleMoves.Count > 0)
            {
                Move selectedMove;

                // Promotion case (4 moves exist)
                if (possibleMoves.Count > 1)
                {
                    var promoWindow = new PromotionWindow();
                    var selectedPiece = await promoWindow.ShowDialog<PieceType>(this);

                    selectedMove = possibleMoves
                        .First(m => m.PromotionType == selectedPiece);
                }
                else
                {
                    selectedMove = possibleMoves[0];
                }

                _state.ApplyMove(selectedMove);
                _selected = null;
                _legalMoves.Clear();
                DrawBoard();
                return;
            }

            _selected = null;
            _legalMoves.Clear();
        }

        DrawBoard();
    }
}
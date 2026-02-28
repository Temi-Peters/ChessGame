using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChessGame.Game;

namespace ChessGame;

public partial class PromotionWindow : Window
{
    public PromotionWindow()
    {
        InitializeComponent();

        this.FindControl<Button>("QueenButton")!.Click += (_, __) =>
            Close(PieceType.Queen);

        this.FindControl<Button>("RookButton")!.Click += (_, __) =>
            Close(PieceType.Rook);

        this.FindControl<Button>("BishopButton")!.Click += (_, __) =>
            Close(PieceType.Bishop);

        this.FindControl<Button>("KnightButton")!.Click += (_, __) =>
            Close(PieceType.Knight);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
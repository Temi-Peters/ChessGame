using Avalonia.Controls.Shapes;

namespace ChessGame
{
    public enum PieceType
    {
        Pawn, Rook, Knight, Bishop, Queen, King
    }

    public enum PieceColor
    {
        White, Black
    }

    public class ChessPiece
    {
        public PieceType Type { get; }
        public PieceColor Color { get; }
        public Ellipse Visual { get; }

        public ChessPiece(PieceType type, PieceColor color, Ellipse visual)
        {
            Type = type;
            Color = color;
            Visual = visual;
        }
    }
}
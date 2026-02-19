namespace ChessGame
{
    // Types of chess pieces
    public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }

    // Piece color
    public enum PieceColor { White, Black }

    // Represents a chess piece
    public class Piece
    {
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }

        public Piece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
        }
    }
}

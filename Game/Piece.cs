namespace ChessGame.Game;

public enum PieceType
{
    Pawn, Rook, Knight, Bishop, Queen, King
}

public enum PieceColor
{
    White, Black
}

public class Piece
{
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }
    public bool HasMoved { get; set; }

    public Piece(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
    }

    public Piece Clone()
    {
        return new Piece(Type, Color)
        {
            HasMoved = this.HasMoved
        };
    }
}
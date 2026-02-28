namespace ChessGame.Game;

public class Move
{
    public int FromRow { get; }
    public int FromCol { get; }
    public int ToRow { get; }
    public int ToCol { get; }

    public PieceType? PromotionType { get; set; }

    public Move(int fromRow, int fromCol, int toRow, int toCol)
    {
        FromRow = fromRow;
        FromCol = fromCol;
        ToRow = toRow;
        ToCol = toCol;
    }
}
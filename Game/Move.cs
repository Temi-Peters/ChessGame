namespace ChessGame.Game;

public class Move
{
    public int FromRow { get; }
    public int FromCol { get; }
    public int ToRow { get; }
    public int ToCol { get; }

    public Move(int fr, int fc, int tr, int tc)
    {
        FromRow = fr;
        FromCol = fc;
        ToRow = tr;
        ToCol = tc;
    }
}
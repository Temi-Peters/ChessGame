using System;

namespace ChessGame.Game;

public class GameState
{
    public Piece?[,] Board { get; set; } = new Piece?[8,8];
    public PieceColor CurrentTurn { get; set; } = PieceColor.White;
    public Move? LastMove { get; set; }

    public void SetupInitialPosition()
    {
        Board = new Piece?[8,8];

        for(int i=0;i<8;i++)
        {
            Board[1,i] = new Piece(PieceType.Pawn, PieceColor.Black);
            Board[6,i] = new Piece(PieceType.Pawn, PieceColor.White);
        }

        Board[0,0] = new Piece(PieceType.Rook, PieceColor.Black);
        Board[0,7] = new Piece(PieceType.Rook, PieceColor.Black);
        Board[7,0] = new Piece(PieceType.Rook, PieceColor.White);
        Board[7,7] = new Piece(PieceType.Rook, PieceColor.White);

        Board[0,1] = new Piece(PieceType.Knight, PieceColor.Black);
        Board[0,6] = new Piece(PieceType.Knight, PieceColor.Black);
        Board[7,1] = new Piece(PieceType.Knight, PieceColor.White);
        Board[7,6] = new Piece(PieceType.Knight, PieceColor.White);

        Board[0,2] = new Piece(PieceType.Bishop, PieceColor.Black);
        Board[0,5] = new Piece(PieceType.Bishop, PieceColor.Black);
        Board[7,2] = new Piece(PieceType.Bishop, PieceColor.White);
        Board[7,5] = new Piece(PieceType.Bishop, PieceColor.White);

        Board[0,3] = new Piece(PieceType.Queen, PieceColor.Black);
        Board[7,3] = new Piece(PieceType.Queen, PieceColor.White);

        Board[0,4] = new Piece(PieceType.King, PieceColor.Black);
        Board[7,4] = new Piece(PieceType.King, PieceColor.White);

        CurrentTurn = PieceColor.White;
        LastMove = null;
    }

    public void ApplyMove(Move move)
    {
        var piece = Board[move.FromRow, move.FromCol];

        // En passant capture
        if (piece?.Type == PieceType.Pawn &&
            move.ToCol != move.FromCol &&
            Board[move.ToRow, move.ToCol] == null)
        {
            int dir = piece.Color == PieceColor.White ? 1 : -1;
            Board[move.ToRow + dir, move.ToCol] = null;
        }

        Board[move.FromRow, move.FromCol] = null;
        Board[move.ToRow, move.ToCol] = piece;

        if (piece != null)
            piece.HasMoved = true;

        // Castling
        if (piece?.Type == PieceType.King &&
            Math.Abs(move.ToCol - move.FromCol) == 2)
        {
            int rookFrom = move.ToCol > move.FromCol ? 7 : 0;
            int rookTo = move.ToCol > move.FromCol ? 5 : 3;

            Board[move.ToRow, rookTo] = Board[move.ToRow, rookFrom];
            Board[move.ToRow, rookFrom] = null;

            Board[move.ToRow, rookTo]!.HasMoved = true;
        }

        // Promotion (auto queen for now)
        if (piece?.Type == PieceType.Pawn &&
            (move.ToRow == 0 || move.ToRow == 7))
        {
            Board[move.ToRow, move.ToCol] =
                new Piece(PieceType.Queen, piece.Color);
        }

        LastMove = move;

        CurrentTurn = CurrentTurn == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;
    }

    public GameState Clone()
    {
        var clone = new GameState();
        clone.CurrentTurn = CurrentTurn;

        for(int r=0;r<8;r++)
        for(int c=0;c<8;c++)
        {
            if(Board[r,c]!=null)
                clone.Board[r,c] = Board[r,c]!.Clone();
        }

        clone.LastMove = LastMove;
        return clone;
    }

    public bool IsInCheck(PieceColor color)
    {
        (int kr, int kc) = FindKing(color);
        return IsSquareAttacked(kr, kc, color);
    }

    (int,int) FindKing(PieceColor color)
    {
        for(int r=0;r<8;r++)
        for(int c=0;c<8;c++)
        {
            if(Board[r,c]?.Type==PieceType.King &&
               Board[r,c]?.Color==color)
                return (r,c);
        }
        throw new Exception("King not found");
    }

    public bool IsSquareAttacked(int row,int col,PieceColor defender)
    {
        var opponent = defender==PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        for(int r=0;r<8;r++)
        for(int c=0;c<8;c++)
        {
            var piece = Board[r,c];
            if(piece==null || piece.Color!=opponent)
                continue;

            var moves = MoveGenerator.GeneratePseudoMoves(this, r, c, false);
            foreach(var m in moves)
                if(m.ToRow==row && m.ToCol==col)
                    return true;
        }
        return false;
    }
}
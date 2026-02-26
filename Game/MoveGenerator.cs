using System;
using System.Collections.Generic;

namespace ChessGame.Game;

public static class MoveGenerator
{
    public static List<Move> GenerateLegalMoves(GameState state, int r, int c)
    {
        var pseudo = GeneratePseudoMoves(state, r, c);
        var legal = new List<Move>();

        foreach (var move in pseudo)
        {
            var copy = state.Clone();
            copy.ApplyMove(move);

            if (!copy.IsInCheck(state.CurrentTurn))
                legal.Add(move);
        }

        return legal;
    }

    public static List<Move> GeneratePseudoMoves(
        GameState state,
        int r,
        int c,
        bool includeCastling = true)
    {
        var piece = state.Board[r, c];
        var moves = new List<Move>();

        if (piece == null)
            return moves;

        switch (piece.Type)
        {
            case PieceType.Pawn:
                PawnMoves(state, r, c, moves);
                break;

            case PieceType.Rook:
                SlideMoves(state, r, c, moves, straight: true, diagonal: false);
                break;

            case PieceType.Bishop:
                SlideMoves(state, r, c, moves, straight: false, diagonal: true);
                break;

            case PieceType.Queen:
                SlideMoves(state, r, c, moves, straight: true, diagonal: true);
                break;

            case PieceType.Knight:
                KnightMoves(state, r, c, moves);
                break;

            case PieceType.King:
                KingMoves(state, r, c, moves);

                if (includeCastling)
                    AddCastling(state, r, c, moves);

                break;
        }

        return moves;
    }

    static void PawnMoves(GameState s, int r, int c, List<Move> moves)
    {
        var p = s.Board[r, c]!;
        int dir = p.Color == PieceColor.White ? -1 : 1;
        int startRow = p.Color == PieceColor.White ? 6 : 1;

        int oneStep = r + dir;

        if (Inside(oneStep, c) && s.Board[oneStep, c] == null)
        {
            moves.Add(new Move(r, c, oneStep, c));

            int twoStep = r + 2 * dir;
            if (r == startRow && s.Board[twoStep, c] == null)
                moves.Add(new Move(r, c, twoStep, c));
        }

        int[] captureCols = { -1, 1 };

        foreach (var dc in captureCols)
        {
            int nr = r + dir;
            int nc = c + dc;

            if (!Inside(nr, nc))
                continue;

            var target = s.Board[nr, nc];

            if (target != null && target.Color != p.Color)
                moves.Add(new Move(r, c, nr, nc));
        }

        // En passant
        if (s.LastMove != null &&
            Math.Abs(s.LastMove.FromRow - s.LastMove.ToRow) == 2)
        {
            if (s.LastMove.ToRow == r &&
                Math.Abs(s.LastMove.ToCol - c) == 1)
            {
                moves.Add(new Move(r, c, r + dir, s.LastMove.ToCol));
            }
        }
    }

    static void KnightMoves(GameState s, int r, int c, List<Move> moves)
    {
        int[] dr = { 2, 2, 1, 1, -1, -1, -2, -2 };
        int[] dc = { 1, -1, 2, -2, 2, -2, 1, -1 };

        for (int i = 0; i < 8; i++)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            if (!Inside(nr, nc))
                continue;

            if (s.Board[nr, nc] == null ||
                s.Board[nr, nc]!.Color != s.Board[r, c]!.Color)
                moves.Add(new Move(r, c, nr, nc));
        }
    }

    static void KingMoves(GameState s, int r, int c, List<Move> moves)
    {
        for (int dr = -1; dr <= 1; dr++)
        for (int dc = -1; dc <= 1; dc++)
        {
            if (dr == 0 && dc == 0)
                continue;

            int nr = r + dr;
            int nc = c + dc;

            if (!Inside(nr, nc))
                continue;

            if (s.Board[nr, nc] == null ||
                s.Board[nr, nc]!.Color != s.Board[r, c]!.Color)
                moves.Add(new Move(r, c, nr, nc));
        }
    }

    static void SlideMoves(
        GameState s,
        int r,
        int c,
        List<Move> moves,
        bool straight,
        bool diagonal)
    {
        var dirs = new List<(int, int)>();

        if (straight)
            dirs.AddRange(new[] { (1, 0), (-1, 0), (0, 1), (0, -1) });

        if (diagonal)
            dirs.AddRange(new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) });

        foreach (var (dr, dc) in dirs)
        {
            int nr = r + dr;
            int nc = c + dc;

            while (Inside(nr, nc))
            {
                if (s.Board[nr, nc] == null)
                {
                    moves.Add(new Move(r, c, nr, nc));
                }
                else
                {
                    if (s.Board[nr, nc]!.Color != s.Board[r, c]!.Color)
                        moves.Add(new Move(r, c, nr, nc));

                    break;
                }

                nr += dr;
                nc += dc;
            }
        }
    }

    static void AddCastling(GameState s, int r, int c, List<Move> moves)
    {
        var king = s.Board[r, c];

        if (king == null || king.HasMoved)
            return;

        if (s.IsInCheck(king.Color))
            return;

        // King side
        if (CanCastle(s, r, c, true))
            moves.Add(new Move(r, c, r, c + 2));

        // Queen side
        if (CanCastle(s, r, c, false))
            moves.Add(new Move(r, c, r, c - 2));
    }

    static bool CanCastle(GameState s, int r, int c, bool kingSide)
    {
        int rookCol = kingSide ? 7 : 0;
        var rook = s.Board[r, rookCol];

        if (rook == null ||
            rook.Type != PieceType.Rook ||
            rook.HasMoved)
            return false;

        int step = kingSide ? 1 : -1;

        // Squares between king and rook must be empty
        for (int col = c + step; col != rookCol; col += step)
            if (s.Board[r, col] != null)
                return false;

        // Squares king passes through must not be attacked
        for (int i = 0; i <= 2; i++)
        {
            int testCol = c + i * step;

            if (s.IsSquareAttacked(r, testCol, s.CurrentTurn))
                return false;
        }

        return true;
    }

    static bool Inside(int r, int c)
        => r >= 0 && r < 8 && c >= 0 && c < 8;
}
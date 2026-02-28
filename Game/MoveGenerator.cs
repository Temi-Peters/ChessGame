using System;
using System.Collections.Generic;

namespace ChessGame.Game;

public static class MoveGenerator
{
    // =========================
    // LEGAL MOVES
    // =========================
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

    // =========================
    // PSEUDO MOVES (RAW RULES)
    // =========================
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
                GeneratePawnMoves(state, r, c, moves);
                break;

            case PieceType.Rook:
                SlideMoves(state, r, c, moves, true, false);
                break;

            case PieceType.Bishop:
                SlideMoves(state, r, c, moves, false, true);
                break;

            case PieceType.Queen:
                SlideMoves(state, r, c, moves, true, true);
                break;

            case PieceType.Knight:
                GenerateKnightMoves(state, r, c, moves);
                break;

            case PieceType.King:
                GenerateKingMoves(state, r, c, moves);

                if (includeCastling)
                    AddCastling(state, r, c, moves);

                break;
        }

        return moves;
    }

    // =========================
    // PAWN
    // =========================
    static void GeneratePawnMoves(GameState s, int r, int c, List<Move> moves)
    {
        var piece = s.Board[r, c]!;
        int direction = piece.Color == PieceColor.White ? -1 : 1;
        int startRow = piece.Color == PieceColor.White ? 6 : 1;

        int oneStep = r + direction;

        // Forward move
        if (Inside(oneStep, c) && s.Board[oneStep, c] == null)
        {
            AddPawnMoveWithPromotion(r, c, oneStep, c, piece.Color, moves);

            // Double move
            int twoStep = r + 2 * direction;
            if (r == startRow && s.Board[twoStep, c] == null)
            {
                moves.Add(new Move(r, c, twoStep, c));
            }
        }

        // Captures
        int[] captureDirs = { -1, 1 };

        foreach (int dc in captureDirs)
        {
            int newR = r + direction;
            int newC = c + dc;

            if (!Inside(newR, newC))
                continue;

            var target = s.Board[newR, newC];

            if (target != null && target.Color != piece.Color)
            {
                AddPawnMoveWithPromotion(r, c, newR, newC, piece.Color, moves);
            }
        }

        // En Passant
        if (s.LastMove != null &&
            Math.Abs(s.LastMove.FromRow - s.LastMove.ToRow) == 2)
        {
            if (s.LastMove.ToRow == r &&
                Math.Abs(s.LastMove.ToCol - c) == 1)
            {
                moves.Add(new Move(r, c, r + direction, s.LastMove.ToCol));
            }
        }
    }

    static void AddPawnMoveWithPromotion(
        int r, int c,
        int newR, int newC,
        PieceColor color,
        List<Move> moves)
    {
        if (newR == 0 || newR == 7)
        {
            moves.Add(new Move(r, c, newR, newC) { PromotionType = PieceType.Knight });
            moves.Add(new Move(r, c, newR, newC) { PromotionType = PieceType.Rook });
            moves.Add(new Move(r, c, newR, newC) { PromotionType = PieceType.Bishop });
            moves.Add(new Move(r, c, newR, newC) { PromotionType = PieceType.Queen });
        }
        else
        {
            moves.Add(new Move(r, c, newR, newC));
        }
    }

    // =========================
    // KNIGHT
    // =========================
    static void GenerateKnightMoves(GameState s, int r, int c, List<Move> moves)
    {
        int[] dr = { 2, 2, 1, 1, -1, -1, -2, -2 };
        int[] dc = { 1, -1, 2, -2, 2, -2, 1, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newR = r + dr[i];
            int newC = c + dc[i];

            if (!Inside(newR, newC))
                continue;

            var target = s.Board[newR, newC];

            if (target == null || target.Color != s.Board[r, c]!.Color)
                moves.Add(new Move(r, c, newR, newC));
        }
    }

    // =========================
    // KING
    // =========================
    static void GenerateKingMoves(GameState s, int r, int c, List<Move> moves)
    {
        for (int dr = -1; dr <= 1; dr++)
        for (int dc = -1; dc <= 1; dc++)
        {
            if (dr == 0 && dc == 0)
                continue;

            int newR = r + dr;
            int newC = c + dc;

            if (!Inside(newR, newC))
                continue;

            var target = s.Board[newR, newC];

            if (target == null || target.Color != s.Board[r, c]!.Color)
                moves.Add(new Move(r, c, newR, newC));
        }
    }

    // =========================
    // CASTLING
    // =========================
    static void AddCastling(GameState s, int r, int c, List<Move> moves)
    {
        var king = s.Board[r, c];

        if (king == null || king.HasMoved)
            return;

        if (s.IsInCheck(king.Color))
            return;

        if (CanCastle(s, r, c, true))
            moves.Add(new Move(r, c, r, c + 2));

        if (CanCastle(s, r, c, false))
            moves.Add(new Move(r, c, r, c - 2));
    }

    static bool CanCastle(GameState s, int r, int c, bool kingSide)
    {
        int rookCol = kingSide ? 7 : 0;
        var rook = s.Board[r, rookCol];

        if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
            return false;

        int step = kingSide ? 1 : -1;

        for (int col = c + step; col != rookCol; col += step)
            if (s.Board[r, col] != null)
                return false;

        for (int i = 0; i <= 2; i++)
        {
            int testCol = c + i * step;

            if (s.IsSquareAttacked(r, testCol, s.CurrentTurn))
                return false;
        }

        return true;
    }

    // =========================
    // UTILITY
    // =========================
    static bool Inside(int r, int c)
        => r >= 0 && r < 8 && c >= 0 && c < 8;

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
            int newR = r + dr;
            int newC = c + dc;

            while (Inside(newR, newC))
            {
                if (s.Board[newR, newC] == null)
                {
                    moves.Add(new Move(r, c, newR, newC));
                }
                else
                {
                    if (s.Board[newR, newC]!.Color != s.Board[r, c]!.Color)
                        moves.Add(new Move(r, c, newR, newC));

                    break;
                }

                newR += dr;
                newC += dc;
            }
        }
    }
}
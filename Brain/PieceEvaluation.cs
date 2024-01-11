using Chess.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Brain
{
    public static class PieceEvaluation
    {
        public const int PAWN = 100, ROOK = 500, QUEEN = 900, BISHOP = 330, KNIGHT = 320;
        public const int weightOfAllPieces = ROOK * 4 + QUEEN * 2 + BISHOP * 4 + KNIGHT * 4;

        //the evaluation boards are set for black (they have to be inverted for white pieces)
        public static readonly int[] pawnPositions =
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] queenPositions =
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        public static readonly int[] knightPositions =
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };

        public static readonly int[] bishopPositions =
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        public static readonly int[] rookPositions =
        {
              0,  0,  0,  0,  0,  0,  0,  0,
              5, 10, 10, 10, 10, 10, 10,  5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              0,  0,  0,  5,  5,  0,  0,  0
        };

        public static readonly int[] middleGameKingPositions =
        {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
        };

        public static int[] endGameKingPositions =
        {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };


        public static int[] kingMatePositions =
        {
            50, 50, 50, 50, 50, 50, 50, 50,
            50, 40, 40, 40, 40, 40, 40, 50,
            50, 40, 30, 30, 30, 30, 40, 50,
            50, 40, 30, 20, 20, 30, 40, 50,
            50, 40, 30, 20, 20, 30, 40, 50,
            50, 40, 30, 30, 30, 30, 40, 50,
            50, 40, 40, 40, 40, 40, 40, 50,
            50, 50, 50, 50, 50, 50, 50, 50
        };
        public static double GetPieceValue(uint piece)
        {
            uint pieceType = Piece.GetPiece(piece);
            switch (pieceType)
            {
                case Piece.PAWN:
                    return PieceEvaluation.PAWN;
                case Piece.KNIGHT:
                    return PieceEvaluation.KNIGHT;
                case Piece.ROOK:
                    return PieceEvaluation.ROOK;
                case Piece.QUEEN:
                    return PieceEvaluation.QUEEN;
                case Piece.BISHOP:
                    return PieceEvaluation.BISHOP;
            }
            return 0;
        }
    }
}

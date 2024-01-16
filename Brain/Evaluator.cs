using Chess.BitMagic;
using Chess.Logic;
using System;

namespace Chess.Brain
{
    //even tough the this is a standalone class it is closely connected to how the search works
    public static class Evaluator
    {
        public const int PAWN = 100, ROOK = 500, QUEEN = 950, BISHOP = 330, KNIGHT = 320, MATE = 30_000;
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

        public static readonly int[] endGamePawnPositions =
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            40, 40, 40, 40, 40, 40, 40, 40,
            30, 30, 30, 30, 30, 30, 30, 30,
            20, 20, 20, 20, 20, 20, 20, 20,
            10, 10, 10, 10, 10, 10, 10, 10,
            5,  5,  5,  5,  5,  5,  5,  5,
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
            -30,-20,-20,-20,-20,-20,-20,-30,
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
            100, 70, 70, 70, 70, 70, 70, 100,
            70, 40, 40, 40, 40, 40, 40, 70,
            70, 40, 30, 30, 30, 30, 40, 70,
            70, 40, 30, 0, 0, 30, 40, 70,
            70, 40, 30, 0, 0, 30, 40, 70,
            70, 40, 30, 30, 30, 30, 40, 70,
            70, 40, 40, 40, 40, 40, 40, 70,
            100, 70, 70, 70, 70, 70, 70, 100
        };
        public static double GetPieceValue(uint piece)
        {
            uint pieceType = Piece.GetPiece(piece);
            switch (pieceType)
            {
                case Piece.PAWN:
                    return PAWN;
                case Piece.KNIGHT:
                    return KNIGHT;
                case Piece.ROOK:
                    return ROOK;
                case Piece.QUEEN:
                    return QUEEN;
                case Piece.BISHOP:
                    return BISHOP;
            }
            return 0;
        }

        //checks for checkmates and stalemates in current position, so it returns the same score no matter who is the one mated this is has to be taken into account during the search
        public static int EvaluateGameState()
        {
            ulong enemyAttackMap = Board.attackMap;
            ulong allyKingPosition = Board.toMove == Piece.WHITE ? Board.whitePieces.kingPosition : Board.blackPieces.kingPosition;
            if ((enemyAttackMap & allyKingPosition) != 0)
            {
                return -MATE;
            }
            else
            {
                return 0;
            }
        }

        //evaluation values are heavily based on https://www.chessprogramming.org/Simplified_Evaluation_Function
        public static int Evaluate(PieceList allyPieces, PieceList enemyPieces, uint allyColor)
        {
            //checks repetitions and half move clock
            //might be inaccurate since repetition table uses zobrist hashing but it should not affect the search
            if (DrawByRules())
            {
                return 0;
            }

            //in case only enemy king's left we just want to make sure it is close to the edge of the board so we can find mate
            if (BitMagician.CountBits(enemyPieces.allPieces) == 1)
            {
                return kingMatePositions[BitMagician.GetBitIndex(enemyPieces.kingPosition)];
            }



            bool isEndGame = MaterialWeightIgnorePawns(enemyPieces) <= QUEEN;

            int evaluation = MaterialWeight(allyPieces) - MaterialWeight(enemyPieces);
            if (!isEndGame)
            {
                evaluation += KingTropism(allyPieces.kingPosition, enemyPieces.allPieces) - KingTropism(enemyPieces.kingPosition, allyPieces.allPieces);
            }
            else
            {
                evaluation += KingDistanceToPawns(allyPieces.kingPosition, allyPieces.pawns | enemyPieces.pawns) - KingDistanceToPawns(enemyPieces.kingPosition, allyPieces.pawns | enemyPieces.pawns);
            }
            for (int i = 0; i < 64; i++)
            {
                uint piece = Board.board[i];
                if (piece == Piece.NONE)
                {
                    continue;
                }
                int index = i;


                //we use this despite the board not being symmetrical along y axis, because the evaluation boards are
                if (Piece.GetColor(piece) == Piece.WHITE)
                {
                    index = 63 - i;
                }
               

                uint pieceType = Piece.GetPiece(piece);
                int score = 0;
                switch (pieceType)
                {
                    case Piece.PAWN:
                        score = pawnPositions[index];
                        break;
                    case Piece.KNIGHT:
                        if (!isEndGame)
                            score = knightPositions[index];
                        break;
                    case Piece.BISHOP:
                        if (!isEndGame)
                            score = bishopPositions[index];
                        break;
                    case Piece.QUEEN:
                        if (!isEndGame)
                            score = queenPositions[index];
                        break;
                    case Piece.ROOK:
                        if (!isEndGame)
                            score = rookPositions[index];
                        break;
                    case Piece.KING:
                        score = isEndGame ? endGameKingPositions[index] : middleGameKingPositions[index];
                        break;
                }

                if (Piece.GetColor(piece) != allyColor)
                {
                    score = -score;
                }

                evaluation += score;
            }
            return evaluation;
        }

        //checks for repetitions and half moves
        private static bool DrawByRules()
        {
            uint halfMoves = Board.currentGameState >> 14;
            return Board.repetitionTable[Board.hash] >= 3 || halfMoves >= 50;
        }

        private static int MaterialWeight(PieceList pieces)
        {
            int bishops = BitMagician.CountBits(pieces.diagonalSliders & ~pieces.orthogonalSliders);
            int rooks = BitMagician.CountBits(pieces.orthogonalSliders & ~pieces.diagonalSliders);
            int pawns = BitMagician.CountBits(pieces.pawns);
            int queens = BitMagician.CountBits(pieces.orthogonalSliders & pieces.diagonalSliders);
            int knights = BitMagician.CountBits(pieces.knights);
            return pawns * PAWN + bishops * BISHOP + rooks * ROOK + queens * QUEEN + knights * KNIGHT;
        }

        private static int MaterialWeightIgnorePawns(PieceList pieces)
        {
            int bishops = BitMagician.CountBits(pieces.diagonalSliders & ~pieces.orthogonalSliders);
            int rooks = BitMagician.CountBits(pieces.orthogonalSliders & ~pieces.diagonalSliders);
            int queens = BitMagician.CountBits(pieces.orthogonalSliders & pieces.diagonalSliders);
            int knights = BitMagician.CountBits(pieces.knights);
            return bishops * BISHOP + rooks * ROOK + queens * QUEEN + knights * KNIGHT;
        }

        //this is a simplified version of kinng safety, we coutn chebyshev distances from our kign and sum them
        private static int KingTropism(ulong kingPosition, ulong enemyPieces)
        {
            int kingField = BitMagician.GetBitIndex(kingPosition);
            int kingRank = kingField / 8;
            int kingFile = kingField % 8;

            int result = 0;

            for (int i = 0; i < 64; i++)
            {
                if ((enemyPieces & (1UL << i)) != 0)
                {

                    int pieceRank = i / 8;
                    int pieceFile = i % 8;

                    int rankDiff = Math.Abs(kingRank - pieceRank);
                    int fileDiff = Math.Abs(kingFile - pieceFile);

                    result += Math.Max(rankDiff, fileDiff);
                }
            }

            return result;
        }

        private static int KingDistanceToPawns(ulong kingPosition, ulong pawns)
        {
            int kingField = BitMagician.GetBitIndex(kingPosition);
            int kingRank = kingField / 8;
            int kingFile = kingField % 8;

            int result = 0;

            for (int i = 0; i < 64; i++)
            {
                if ((pawns & (1UL << i)) != 0)
                {

                    int pieceRank = i / 8;
                    int pieceFile = i % 8;

                    int rankDiff = Math.Abs(kingRank - pieceRank);
                    int fileDiff = Math.Abs(kingFile - pieceFile);

                    result += Math.Max(rankDiff, fileDiff);
                }
            }

            return -result;
        }
    }
}

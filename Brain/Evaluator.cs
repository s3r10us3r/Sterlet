using Chess.BitMagic;
using Chess.Logic;


namespace Chess.Brain
{
    //even tough the this is a standalone class it is closely connected to how the search works
    public static class Evaluator
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
                return -30000;
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

            double endGameWeight = (MaterialWeight(allyPieces) + MaterialWeight(enemyPieces) - 1600) / weightOfAllPieces;

            int evaluation = MaterialWeight(allyPieces) - MaterialWeight(enemyPieces);

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
                        score = knightPositions[index];
                        break;
                    case Piece.BISHOP:
                        score = bishopPositions[index];
                        break;
                    case Piece.QUEEN:
                        score = queenPositions[index];
                        break;
                    case Piece.ROOK:
                        score = rookPositions[index];
                        break;
                    case Piece.KING:
                        score = (int)(middleGameKingPositions[index] * (1 - endGameWeight) + endGameKingPositions[index] * endGameWeight);
                        break;
                }

                if (Piece.GetColor(piece) != allyColor)
                {
                    score = -score;
                }

                evaluation += score;
            }

            //in case only enemy king's left
            if (BitMagician.CountBits(enemyPieces.allPieces) == 1)
            {
                evaluation += Evaluator.kingMatePositions[BitMagician.GetBitIndex(enemyPieces.kingPosition)];
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
    }
}

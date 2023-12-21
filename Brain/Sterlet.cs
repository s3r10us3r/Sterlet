using Chess.BitMagic;
using Chess.Logic;
using System;
using System.Collections.Generic;


namespace Chess.Brain
{
    public partial class Sterlet
    {
        private int initialDepth;

        private const int PAWN = 100, ROOK = 500, QUEEN = 900, BISHOP = 330, KNIGHT = 320;
        private const int weightOfAllPieces = ROOK * 4 + QUEEN * 2 + BISHOP * 4 + KNIGHT * 4;

        //the evaluation boards are set for black (they have to be inverted for white pieces)
        private readonly int[] pawnPositions =
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

        private readonly int[] queenPositions =
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

        private readonly int[] knightPositions =
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

        private readonly int[] bishopPositions =
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

        private readonly int[] rookPositions =
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

        private readonly int[] middleGameKingPositions =
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

        private readonly int[] endGameKingPositions =
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

        private PieceList allyPieces;
        private PieceList enemyPieces;
        private uint allyColor;
        private uint enemyColor;
        private Dictionary<ulong, (int score, int depth)> transpositionTable;

        private readonly Predicate<Move> isCapture = move => MoveClassifier.IsCapture(move);

        private static int nodesVisited = 0;
        private static int transpositions = 0;
        public Sterlet(int depth, uint color)
        {
            this.initialDepth = depth;
            if (color == Piece.WHITE)
            {
                allyPieces = Board.whitePieces;
                enemyPieces = Board.blackPieces;
                allyColor = Piece.WHITE;
                enemyColor = Piece.BLACK;
            }
            else
            {
                allyPieces = Board.blackPieces;
                enemyPieces = Board.whitePieces;
                allyColor = Piece.BLACK;
                enemyColor = Piece.WHITE;
            }
            transpositionTable = new Dictionary<ulong, (int score, int depth)>();
        }

        public Move ChooseMove(List<Move> moves)
        {
            Move chosenMove = null;

            nodesVisited = 0;
            transpositions = 0;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            OrderMoves(moves);
            foreach(Move move in moves)
            {
                Board.MakeMove(move);
                int score = AlphaBetaMin(alpha, beta, initialDepth - 1);
                if ( score >= beta )
                {
                    Board.UnMakeMove();
                    break;
                }
                if (score > alpha)
                {
                    alpha = score;
                    chosenMove = move;
                }
                Board.UnMakeMove();
            }
            

            Console.WriteLine($"VISITED NODES {nodesVisited} MOVE {chosenMove} EVALUATED AT {alpha} TRANSPOSITIONS {transpositions}");
            int capturesNaive = 0;
            foreach(Move move in moves)
            {
                if (MoveClassifier.IsCapture(move))
                    capturesNaive++;
            }
            Console.WriteLine($"0-depth EVAL {Evaluate()} NUMBER OF CAPTURES {capturesNaive}");
            return chosenMove;
        }


        //evaluation values are heavily based on https://www.chessprogramming.org/Simplified_Evaluation_Function
        private int Evaluate()
        {
            nodesVisited++;
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
                //we use this despite the board not being symmetrical along y axis, because the evaluation boards are symmetrical along y axis
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
                        score = (int)(middleGameKingPositions[index] * (1-endGameWeight) + endGameKingPositions[index] * endGameWeight);
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



        private int AlphaBetaMin(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return AlphaBetaMinQuiscence(alpha, beta);
            }  
            List<Move> moves = MoveGenerator.GenerateMoves();
            if(moves.Count == 0)
            {
                return -EvaluateGameState() * depth;
            }
            OrderMoves(moves);
            foreach(Move move in moves)
            {
                Board.MakeMove(move);
                int score;
                if (transpositionTable.ContainsKey(Board.hash) && transpositionTable[Board.hash].depth >= depth)
                {
                    transpositions++;
                    ulong hash = Board.hash;
                    score = transpositionTable[hash].score;
                }
                else
                {
                    score = AlphaBetaMax(alpha, beta, depth - 1);
                    transpositionTable[Board.hash] = (score, depth);
                }
                if (score <= alpha)
                {
                    Board.UnMakeMove();
                    return alpha;
                }
                if (score < beta)
                {
                    beta = score;
                }
                Board.UnMakeMove();
            }
            return beta;
        }

        private int AlphaBetaMax(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return AlphaBetaMaxQuiscence(alpha, beta);
            }
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return EvaluateGameState() * depth;
            }
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score;
                if (transpositionTable.ContainsKey(Board.hash) && transpositionTable[Board.hash].depth >= depth)
                {
                    transpositions++;
                    ulong hash = Board.hash;
                    score = transpositionTable[hash].score;
                }
                else
                {
                    score = AlphaBetaMin(alpha, beta, depth - 1);
                    transpositionTable[Board.hash] = (score, depth);
                }
                if (score >= beta)
                {
                    Board.UnMakeMove();
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
                Board.UnMakeMove();
            }
            return alpha;
        }

        private int AlphaBetaMinQuiscence(int alpha, int beta)
        {
            int eval = Evaluate();
            if (eval <= alpha)
            {
                return alpha;
            }
            if(eval < beta)
            {
                beta = eval;
            }
            List<Move> moves = MoveGenerator.GenerateMoves();
            if(moves.Count == 0)
            {
                return EvaluateGameState();
            }
            moves = moves.FindAll(isCapture);
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score = AlphaBetaMaxQuiscence(alpha, beta);
                if (score <= alpha)
                {
                    Board.UnMakeMove();
                    return alpha;
                }
                if (score < beta)
                {
                    beta = score;
                }
                Board.UnMakeMove();
            }
            return beta;
        }

        private int AlphaBetaMaxQuiscence(int alpha, int beta)
        {
            int eval = Evaluate();
            if (eval >= beta)
            {
                return beta;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return -EvaluateGameState();
            }
            moves = moves.FindAll(isCapture);
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score = AlphaBetaMinQuiscence(alpha, beta);
                if (score >= beta)
                {
                    Board.UnMakeMove();
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
                Board.UnMakeMove();
            }
            return alpha;
        }

        private void OrderMoves(List<Move> moves)
        {
            List<double> scores = new List<double>();
            ulong enemyPawnsAttacks = AttackMapper.getPawnAttacks(enemyPieces.pawns, enemyColor);
            foreach (Move move in moves)
            {
                double score = 0;
                uint movingPiece = Board.board[move.StartSquare];
                uint capturedPiece = Board.board[move.TargetSquare];

                if(capturedPiece != Piece.NONE)
                {
                    score = 10 * (GetPieceValue(capturedPiece) - GetPieceValue(movingPiece));
                }

                if (move.MoveFlag > Move.Flag.PawnTwoForward)
                {
                    score += QUEEN;
                }

                ulong targetBitboard = 1UL << move.TargetSquare;
                if ((targetBitboard & enemyPawnsAttacks) != 0)
                {
                    score -= GetPieceValue(movingPiece);
                }
                scores.Add(score);
            }

            for (int i = 0; i < moves.Count; i++)
            {
                int maxIndex = i;
                for (int j = i + 1; j < moves.Count; j++)
                {
                    if(scores[j] > scores[maxIndex])
                    {
                        maxIndex = j;
                    }
                }
                (moves[i], scores[i], moves[maxIndex], scores[maxIndex]) = (moves[maxIndex], scores[maxIndex], moves[i], scores[i]);
            }
        }

        //checks for checkmates and stalemates
        private int EvaluateGameState()
        {
            ulong enemyAttackMap = MoveGenerator.enemyAttackMap;
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

        private int MaterialWeight(PieceList pieces)
        {
            int bishops = BitMagician.CountBits(pieces.diagonalSliders & ~pieces.orthogonalSliders);
            int rooks = BitMagician.CountBits(pieces.orthogonalSliders & ~pieces.diagonalSliders);
            int pawns = BitMagician.CountBits(pieces.pawns);
            int queens = BitMagician.CountBits(pieces.orthogonalSliders & pieces.diagonalSliders);
            int knights = BitMagician.CountBits(pieces.knights);
            return pawns * PAWN + bishops * BISHOP + rooks * ROOK + queens * QUEEN + knights * KNIGHT;
        }

        private double GetPieceValue(uint piece)
        {
            uint pieceType = Piece.GetPiece(piece);
            switch(pieceType)
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
    }
}

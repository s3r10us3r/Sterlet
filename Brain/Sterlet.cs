using Chess.BitMagic;
using Chess.Logic;
using System;
using System.Collections.Generic;


namespace Chess.Brain
{
    public class Sterlet
    {
        private int initialDepth;
        private const double PAWN = 1, ROOK = 5, QUEEN = 9, BISHOP = 3.3, KNIGHT = 3.2;

        private PieceList allyPieces;
        private PieceList enemyPieces;
        private uint allyColor;
        private uint enemyColor;

        private readonly Predicate<Move> isCapture = move => MoveClassifier.IsCapture(move);

        private static int nodesVisited = 0;
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
        }

        public Move ChooseMove(List<Move> moves)
        {
            Move chosenMove = null;

            nodesVisited = 0;
            double alpha = double.MinValue;
            double beta = double.MaxValue;
            OrderMoves(moves);
            foreach(Move move in moves)
            {
                Board.MakeMove(move);
                double score = AlphaBetaMin(alpha, beta, initialDepth - 1);
                if( score >= beta )
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
            

            Console.WriteLine($"VISITED NODES {nodesVisited} MOVE {chosenMove} EVALUATED AT {alpha}");
            return chosenMove;
        }

        public double Evaluate()
        {
            nodesVisited++;
            return MaterialWeight(allyPieces) - MaterialWeight(enemyPieces);
        }

        private double AlphaBetaMin(double alpha, double beta, int depth)
        {
            if (depth == 0)
            {
                return AlphaBetaMinQuiscence(alpha, beta);
            }  
            List<Move> moves = MoveGenerator.GenerateMoves();
            if(moves.Count == 0)
            {
                return EvaluateGameState();
            }
            OrderMoves(moves);
            foreach(Move move in moves)
            {
                Board.MakeMove(move);
                double score = AlphaBetaMax(alpha, beta, depth - 1);
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

        private double AlphaBetaMax(double alpha, double beta, int depth)
        {
            if (depth == 0)
            {
                return AlphaBetaMaxQuiscence(alpha, beta);
            }
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return -EvaluateGameState();
            }
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                double score = AlphaBetaMin(alpha, beta, depth - 1);
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

        private double AlphaBetaMinQuiscence(double alpha, double beta)
        {
            List<Move> moves = MoveGenerator.GenerateMoves();
            if(moves.Count == 0)
            {
                return EvaluateGameState();
            }
            moves = moves.FindAll(isCapture);
            if(moves.Count == 0)
            {
                return Evaluate();
            }
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                double score = AlphaBetaMaxQuiscence(alpha, beta);
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

        private double AlphaBetaMaxQuiscence(double alpha, double beta)
        {
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return -EvaluateGameState();
            }
            moves = moves.FindAll(isCapture);
            if (moves.Count == 0)
            {
                return Evaluate();
            }
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                double score = AlphaBetaMinQuiscence(alpha, beta);
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

        //checks if player is mated
        private double EvaluateGameState()
        {
            ulong enemyAttackMap = MoveGenerator.enemyAttackMap;
            ulong allyKingPosition = Board.toMove == Piece.WHITE ? Board.whitePieces.kingPosition : Board.blackPieces.kingPosition;
            if((enemyAttackMap & allyKingPosition) != 0)
            {
                return -300;
            }
            else
            {
                return 0;
            }
        }

        private double MaterialWeight(PieceList pieces)
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

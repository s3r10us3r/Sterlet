using Chess.BitMagic;
using Chess.Logic;
using System;
using System.Collections.Generic;


namespace Chess.Brain
{
    public class Sterlet
    {
        private int depth;
        private const double PAWN = 1, ROOK = 5, QUEEN = 9, BISHOP = 3.3, KNIGHT = 3.2;
        private double allyMultiplier = 1;
        private double enemyMultiplier = -1;

        private PieceList allyPieces;
        private PieceList enemyPieces;
        private uint allyColor;
        private uint enemyColor;

        private static int nodesVisited = 0;
        public Sterlet(int depth, uint color)
        {
            this.depth = depth;
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
            double maxScore = double.MinValue;
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                double score = -NegaMax(depth - 1, double.MinValue, double.MaxValue);
                if (score > maxScore)
                {
                    maxScore = score;
                    chosenMove = move;
                }
                Board.UnMakeMove();
            }
            Console.WriteLine($"VISITED NODES {nodesVisited}");
            return chosenMove;
        }

        public double Evaluate(List<Move> moves)
        {
            GameState gameState = Referee.DetermineGameState(moves);
            if (gameState != GameState.ONGOING)
            {
                if (gameState == GameState.WIN)
                {
                    return QUEEN * 30 * allyMultiplier;
                }
                else
                {
                    return 0;
                }
            }

            return (MaterialWeight(allyPieces) - MaterialWeight(enemyPieces)) * allyMultiplier;
        }

        private double NegaMax(int depth, double a, double b)
        {
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (depth == 0 || moves.Count == 0)
            {
                return QuiesNegaMax(-b, -a);
            }

            OrderMoves(moves);
            SwapValues();
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                double score = -NegaMax(depth - 1, -b, -a);
                if (score >= b)
                {
                    Board.UnMakeMove();
                    return b;
                }
                if (score > a)
                {
                    a = score;
                }
                Board.UnMakeMove();
            }
            SwapValues();
            return a;
        }

        private double QuiesNegaMax(double a, double b)
        {
            List<Move> moves = MoveGenerator.GenerateMoves();
            Predicate<Move> isCapture = move => MoveClassifier.IsCapture(move);
            List<Move> quiesMoves = moves.FindAll(isCapture);
            if (quiesMoves.Count == 0)
            {
                nodesVisited++;
                return Evaluate(moves);
            }

            OrderMoves(quiesMoves);
            SwapValues();
            foreach (Move move in quiesMoves)
            {
                Board.MakeMove(move);
                double score = QuiesNegaMax(-b, -a);
                if (score >= b)
                {
                    Board.UnMakeMove();
                    return b;
                }
                if (score > a)
                {
                    a = score;
                }
                Board.UnMakeMove();
            }
            SwapValues();
            return a;
        }

        private void OrderMoves (List<Move> moves)
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
                    score = 10 * GetPieceValue(capturedPiece) - GetPieceValue(movingPiece);
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

        private double MaterialWeight(PieceList pieces)
        {
            int bishops = BitMagician.CountBits(pieces.diagonalSliders & ~pieces.orthogonalSliders);
            int rooks = BitMagician.CountBits(pieces.orthogonalSliders & ~pieces.diagonalSliders);
            int pawns = BitMagician.CountBits(pieces.pawns);
            int queens = BitMagician.CountBits(pieces.orthogonalSliders & pieces.diagonalSliders);
            int knights = BitMagician.CountBits(pieces.knights);
            return pawns * PAWN + bishops * BISHOP + rooks * ROOK + queens * QUEEN + knights * KNIGHT;
        }

        private void SwapValues()
        {
            (allyPieces, enemyPieces) = (enemyPieces, allyPieces);
            (allyMultiplier, enemyMultiplier) = (enemyMultiplier, allyMultiplier);
            (allyColor, enemyColor) = (enemyColor, allyColor);
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

using Chess.Abstracts;
using Chess.BitMagic;
using Chess.gui;
using Chess.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Chess.Brain
{
    public partial class Sterlet : IPlayer
    {
        private int timeInMillis = 2000;
        private int initialDepth;
        private OpeningBook book;
        //private readonly string bookPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "book.txt");

        private PieceList allyPieces;
        private PieceList enemyPieces;
        private readonly uint allyColor;
        private readonly uint enemyColor;

        //table for storing positions that have been evaluated
        //this table IS cleared between search iterations
        private Dictionary<ulong, (int score, int depth)> transpositionTable;

        //this dictionary stores a so called history heuristic, if a move has been cut off we give it a score 2^depth
        //we use this in move ordering
        //key is simply a move value
        //this is not cleared between search iterations
        private int[] historyHeuristic;

        //these are thread fields for iterative deepening
        //when iterative deepening completes a search to a given depth, it sets the chosenMove
        private Move chosenMove = null;
        //when hasFinished is set to true iterative deepening will stop after it completes the search, this is here in case the depth 1 search exceeds given time
        private bool hasFinished = false;
        //this is for debugging purposes
        private int depthReached = 0;

        private readonly Predicate<Move> isCapture = move => MoveClassifier.IsCapture(move);

        private static int nodesVisited = 0;
        private static int transpositions = 0;

        public SearchResults SearchResults { get; private set; }

        bool inOpening = true;
        public Sterlet(int depth, uint color)
        {
            SearchResults = null;
            this.initialDepth = depth;
            if (color == Piece.WHITE)
            {
                allyColor = Piece.WHITE;
                enemyColor = Piece.BLACK;
                allyPieces = Board.whitePieces;
                enemyPieces = Board.blackPieces;
            }
            else
            {
                allyColor = Piece.BLACK;
                enemyColor = Piece.WHITE;
                allyPieces = Board.blackPieces;
                enemyPieces = Board.whitePieces;
            }
            transpositionTable = new Dictionary<ulong, (int score, int depth)>();
            book = new OpeningBook(@"C:\Users\jedyn\source\repos\Chess\Resources\book.txt");
        }

        public Move ChooseMove()
        {
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (inOpening)
            {
                Move chosenMove = GetMoveFromBook(moves);
                if (chosenMove != null)
                {
                    return chosenMove;
                }
                inOpening = false;
            }

            Move moveChosen = MakeSearchForMillis(timeInMillis);
            return moveChosen;
            //return Search(moves, initialDepth);
        }

        

        private Move GetMoveFromBook(List<Move> moves)
        {
            if (Board.moveHistory.Count != 0)
            {
                book.MakeMove(Board.moveHistory.Peek());
                string moveString = book.GetNextMove();

                if (moveString == null)
                {
                    return null;
                }
                else
                {
                    foreach (Move move in moves)
                    {
                        if (move.ToString() == moveString)
                        {
                            book.MakeMove(move);
                            Console.WriteLine("book move");
                            return move;
                        }
                    }
                }
            }

            return null;
        }

        private Move MakeSearchForMillis(int millis)
        {
            chosenMove = null;
            hasFinished = false;
            depthReached = 0;
            Thread searchThread = new Thread(
                new ThreadStart(IterativeDeepeningSearch)
            );
            searchThread.Start();
            Thread.Sleep(millis);
            hasFinished = true;

            searchThread.Join();

            if (chosenMove != null)
            {
                return chosenMove;
            }
            //if a search has not been completed(even depth 1 search) return any move
            else //this is very unlikely and probably won't happen a single time
            {
                List<Move> moves = MoveGenerator.GenerateMoves();
                return moves[0];
            }
        }

        private void IterativeDeepeningSearch()
        {
            historyHeuristic = new int[ushort.MaxValue];
            List<Move> depth1Moves = MoveGenerator.GenerateMoves();
            int currentDepth = 1;
            while (!hasFinished)
            {
                Move thisIteration = Search(depth1Moves, currentDepth);
                if(hasFinished)
                {
                    break;
                }
                chosenMove = thisIteration;
                depthReached = currentDepth;
                currentDepth++;
            }
        }

        private Move Search(List<Move> moves, int depth)
        {
            transpositionTable.Clear();
            
            Move chosenMove = null;
            nodesVisited = 0;
            transpositions = 0;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score = AlphaBetaMin(alpha, beta, depth - 1);
                if (score >= beta)
                {
                    Board.UnMakeMove();
                    break;
                }
                historyHeuristic[move.value] += depth * depth;
                if (score > alpha)
                {
                    alpha = score;
                    chosenMove = move;
                }
                Board.UnMakeMove();
            }

            if (!hasFinished)
            {
                SearchResults = new SearchResults(depth, alpha, Evaluate());
            }

            return chosenMove;
        }

        //evaluation values are heavily based on https://www.chessprogramming.org/Simplified_Evaluation_Function
        private int Evaluate()
        {
            //checks repetitions and half move clock
            //might be inaccurate since repetition table uses zobrist hashing but it should not affect search
            if (DrawByRules())
            {
                return 0;
            }

            nodesVisited++;
            double endGameWeight = (MaterialWeight(allyPieces) + MaterialWeight(enemyPieces) - 1600) / PieceEvaluation.weightOfAllPieces;

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
                        score = PieceEvaluation.pawnPositions[index];
                        break;
                    case Piece.KNIGHT:
                        score = PieceEvaluation.knightPositions[index];
                        break;
                    case Piece.BISHOP:
                        score = PieceEvaluation.bishopPositions[index];
                        break;
                    case Piece.QUEEN:
                        score = PieceEvaluation.queenPositions[index];
                        break;
                    case Piece.ROOK:
                        score = PieceEvaluation.rookPositions[index];
                        break;
                    case Piece.KING:
                        score = (int)(PieceEvaluation.middleGameKingPositions[index] * (1-endGameWeight) + PieceEvaluation.endGameKingPositions[index] * endGameWeight);
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
            if (hasFinished)
            {
                return 0;
            }

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
                historyHeuristic[move.value] += depth*depth;
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
            if(hasFinished)
            {
                return 0;
            }

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

            Move currentCutoff = null;

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
                historyHeuristic[move.value] += depth * depth;
                if (score > alpha)
                {
                    alpha = score;
                    currentCutoff = move;
                }
                Board.UnMakeMove();
            }
            return alpha;
        }

        private int AlphaBetaMinQuiscence(int alpha, int beta)
        {
            if (hasFinished)
                return 0;
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
            if (hasFinished)
                return 0;
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

            for (int i = 0; i < moves.Count; i++)
            {
                int maxIndex = i;
                for (int j = i + 1; j < moves.Count; j++)
                {
                    if (historyHeuristic[j] > historyHeuristic[maxIndex])
                    {
                        maxIndex = j;
                    }
                }
                (moves[i], moves[maxIndex]) = (moves[maxIndex], moves[i]);
            }

            foreach (Move move in moves)
            {
                double score = 0;
                uint movingPiece = Board.board[move.StartSquare];
                uint capturedPiece = Board.board[move.TargetSquare];

                //here we want captures to ALWAYS go first even tough it might seem that higher piece capturing lower piece is usually a bad move, bad captures will be cut quickly and the approach where we place nonCaptures last leads to best move being searched last quite often
                if (capturedPiece != Piece.NONE)
                {
                    score = 10 * PieceEvaluation.GetPieceValue(capturedPiece) - PieceEvaluation.GetPieceValue(movingPiece);
                }

                //this is because flags with higher value than PawnTwoForward are promotions 
                if (move.MoveFlag > Move.Flag.PawnTwoForward)
                {
                    score += 10 * PieceEvaluation.QUEEN;
                }

                ulong targetBitboard = 1UL << move.TargetSquare;
                if ((targetBitboard & enemyPawnsAttacks) != 0)
                {
                    score -= PieceEvaluation.GetPieceValue(movingPiece);
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
            return pawns * PieceEvaluation.PAWN + bishops * PieceEvaluation.BISHOP + rooks * PieceEvaluation.ROOK + queens * PieceEvaluation.QUEEN + knights * PieceEvaluation.KNIGHT;
        }

        public PieceImage GetPiece(uint piece, int field)
        {
            return new PieceImage(piece, field);
        }

        public void RemovePiece(PieceImage piece)
        {
        }

        //checks for repetitions or half moves
        private bool DrawByRules()
        {
            uint halfMoves = Board.currentGameState >> 14;
            return Board.repetitionTable[Board.hash] >= 3 || halfMoves >= 50;
        }
    }
}

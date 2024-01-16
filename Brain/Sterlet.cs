using Chess.Abstracts;
using Chess.gui;
using Chess.Logic;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Chess.Brain
{
    public partial class Sterlet : IPlayer
    {
        //TODO: ADD TIME MANAGMENT
        //time in millis spend on the next move
        private gui.Timer timer;
        private OpeningBook book;
        //private readonly string bookPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "book.txt");

        private PieceList allyPieces;
        private PieceList enemyPieces;

        private readonly uint allyColor;
        private readonly uint enemyColor;

        //table for storing positions that have been evaluated
        //this table IS cleared between search iterations
        private Dictionary<ulong, (int score, int depth)> transpositionTable;

        //this tanle stores a so called history heuristic, if a move has been cut off we give add up to its score depth * depth
        //we use this in move ordering
        //key is simply a move value
        //this is not cleared between search iterations
        private int[] historyHeuristic;

        //PV nodes are nodes that were not cut off
        //as I am preatty dumb i will hash them by just adding move's value to 

        //these are thread fields for iterative deepening
        //when iterative deepening completes a search to a given depth, it sets the chosenMove
        private Move chosenMove = null;
        //when hasFinished is set to true iterative deepening will stop after it completes the search, this is here in case the depth 1 search exceeds given time
        private bool hasFinished = false;
        //when half passed is set to true it means we can't start new deeper search and cut the search short
        private bool halfPassed = false;

        private readonly Predicate<Move> isCapture = move => MoveClassifier.IsCapture(move);

        public SearchResults SearchResults { get; private set; }

        //this can be set to false in order to stop using the opening book
        bool inOpening = true;
        public Sterlet(gui.Timer timer, uint color)
        {
            this.timer = timer;
            SearchResults = null;
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
            historyHeuristic = new int[ushort.MaxValue];
        }

        public Move ChooseMove()
        {
            Console.WriteLine("CALLED");
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (inOpening)
            {
                Console.WriteLine($"FROM BOOK {moves.Count}");
                Move chosenMove = GetMoveFromBook(moves);
                Console.WriteLine(chosenMove);
                if (chosenMove != null)
                {
                    return chosenMove;
                }
                inOpening = false;
            }
            Console.WriteLine("NOT FROM BOOK");
            Move moveChosen = MakeSearchForMillis();
            return moveChosen;
        }

        //we look for any moves which string's match any available for current move history in  our opening book
        private Move GetMoveFromBook(List<Move> moves)
        {
            if (Board.moveHistory.Count != 0)
            {
                book.MakeMove(Board.moveHistory.Peek());
            }
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


            return null;
        }


        //searches for a maximum of estimated time
        private Move MakeSearchForMillis()
        {
            chosenMove = null;
            hasFinished = false;
            halfPassed = false;

            Thread timeThread = new Thread(
                new ThreadStart(() =>
                {
                    int timeForSearch = EstimateTimeInMillis();
                    Console.WriteLine($"Time for search {timeForSearch}");
                    Thread.Sleep(timeForSearch / 2);
                    halfPassed = true;
                    Thread.Sleep(timeForSearch / 2);
                    hasFinished = true;
                })
            );

            timeThread.Start();
            IterativeDeepeningSearch();


            timeThread.Abort();

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
            List<Move> depth1Moves = MoveGenerator.GenerateMoves();
            int currentDepth = 1;
            while (!halfPassed)
            {
                (Move thisIteration, int currentScore) = Search(depth1Moves, currentDepth);
                if (hasFinished)
                {
                    break;
                }
                chosenMove = thisIteration;
                if (currentScore != int.MinValue && Math.Abs(currentScore) >= Evaluator.MATE)
                {
                    break;
                }
                currentDepth++;
            }
        }

        private (Move move, int score) Search(List<Move> moves, int depth)
        {
            transpositionTable.Clear();

            Move chosenMove = null;
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
                SearchResults = new SearchResults(depth, alpha, Evaluator.Evaluate(allyPieces, enemyPieces, allyColor));
            }

            return (chosenMove, alpha);
        }

        private int AlphaBetaMin(int alpha, int beta, int depth)
        {
            if (hasFinished || Board.repetitionTable[Board.hash] >= 3)
            {
                return 0;
            }
            if (depth == 0)
            {
                return AlphaBetaMinQuiscence(alpha, beta, depth - 1);
            }
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return -Evaluator.EvaluateGameState() * depth;
            }
            OrderMoves(moves);

            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score;
                if (transpositionTable.ContainsKey(Board.hash) && transpositionTable[Board.hash].depth >= depth)
                {
                    ulong hash = Board.hash;
                    score = transpositionTable[hash].score;
                }
                else
                {
                    int nextDepth = depth;
                    score = AlphaBetaMax(alpha, beta, nextDepth - 1);
                    transpositionTable[Board.hash] = (score, depth);
                }
                if (score <= alpha)
                {
                    Board.UnMakeMove();
                    return alpha;
                }
                historyHeuristic[move.value] += depth * depth;
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
            if (hasFinished || Board.repetitionTable[Board.hash] >= 3)
            {
                return 0;
            }

            if (depth == 0)
            {
                return AlphaBetaMaxQuiscence(alpha, beta, depth - 1);
            }
            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return Evaluator.EvaluateGameState() * depth;
            }
            OrderMoves(moves);

            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score;
                if (transpositionTable.ContainsKey(Board.hash) && transpositionTable[Board.hash].depth >= depth)
                {
                    ulong hash = Board.hash;
                    score = transpositionTable[hash].score;
                }
                else
                {
                    int nextDepth = depth;
                    score = AlphaBetaMin(alpha, beta, nextDepth - 1);
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
                }
                Board.UnMakeMove();
            }
            return alpha;
        }

        private int AlphaBetaMinQuiscence(int alpha, int beta, int depth)
        {
            if (hasFinished)
                return 0;

            List<Move> moves = MoveGenerator.GenerateMoves();
            if (moves.Count == 0)
            {
                return Evaluator.EvaluateGameState();
            }

            int eval = Evaluator.Evaluate(allyPieces, enemyPieces, allyColor);
            if (eval <= alpha)
            {
                return alpha;
            }
            if (eval < beta)
            {
                beta = eval;
            }

            moves = moves.FindAll(isCapture);
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score;
                score = AlphaBetaMaxQuiscence(alpha, beta, depth - 1);
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

        private int AlphaBetaMaxQuiscence(int alpha, int beta, int depth)
        {
            if (hasFinished)
                return 0;
            int eval = Evaluator.Evaluate(allyPieces, enemyPieces, allyColor);
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
                return -Evaluator.EvaluateGameState();
            }
            moves = moves.FindAll(isCapture);
            OrderMoves(moves);
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int score = AlphaBetaMinQuiscence(alpha, beta, depth - 1);
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
            ulong enemyPawnsAttacks = AttackMapper.GetPawnAttacks(enemyPieces.pawns, enemyColor);

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
                    score = 10 * Evaluator.GetPieceValue(capturedPiece) - Evaluator.GetPieceValue(movingPiece);
                }

                //this is because flags with higher value than PawnTwoForward are promotions 
                if (move.MoveFlag > Move.Flag.PawnTwoForward)
                {
                    score += 10 * Evaluator.QUEEN;
                }

                ulong targetBitboard = 1UL << move.TargetSquare;
                if ((targetBitboard & enemyPawnsAttacks) != 0)
                {
                    score -= Evaluator.GetPieceValue(movingPiece);
                }
                scores.Add(score);
            }

            for (int i = 0; i < moves.Count; i++)
            {
                int maxIndex = i;
                for (int j = i + 1; j < moves.Count; j++)
                {
                    if (scores[j] > scores[maxIndex])
                    {
                        maxIndex = j;
                    }
                }
                (moves[i], scores[i], moves[maxIndex], scores[maxIndex]) = (moves[maxIndex], scores[maxIndex], moves[i], scores[i]);
            }
        }

        public PieceImage GetPiece(uint piece, int field)
        {
            return new PieceImage(piece, field);
        }

        public void RemovePiece(PieceImage piece)
        {
        }

        //this is very simple estimation we just assume we always have 50 moves till the game is finished or we just return 10 seconds if there are no time constraints  
        private int EstimateTimeInMillis()
        {
            return timer.IsBlocked ? 10_000 : (timer.GetTimeLeft() / 50) + timer.Increment;
        }
    }
}

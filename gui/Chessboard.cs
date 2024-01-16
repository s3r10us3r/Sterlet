using Chess.Abstracts;
using Chess.Brain;
using Chess.Logic;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chess.gui
{
    public class ChessBoard : Grid
    {
        public const int FIELDSIZE = 75;

        private static bool isInverted = false;
        private readonly SolidColorBrush WHITE_FIELDS_COLOR = Brushes.Wheat;
        private readonly SolidColorBrush BLACK_FIELDS_COLOR = Brushes.DarkGreen;
        private readonly SolidColorBrush WHITE_HIGHLITED_COLOR = new SolidColorBrush(Color.FromRgb(255, 200, 150));
        private readonly SolidColorBrush BLACK_HIGHLITED_COLOR = new SolidColorBrush(Color.FromRgb(100, 100, 0));
        private readonly SolidColorBrush WHITE_LAST_MOVE_COLOR = new SolidColorBrush(Color.FromRgb(150, 150, 10));
        private readonly SolidColorBrush BLACK_LAST_MOVE_COLOR = new SolidColorBrush(Color.FromRgb(75, 75, 5));

        private PieceImage[] boardArray = new PieceImage[64];
        private Border[,] fieldArray = new Border[8, 8];
        private IPlayer whitePlayer;
        private IPlayer blackPlayer;

        private Timer whiteTimer;
        private Timer blackTimer;

        private readonly TextBlock whoWonText;
        private readonly TextBlock reasonText;

        private uint[] boardRepresentation = (uint[])Board.board.Clone();

        private (int start, int end) lastMoveFields;

        private bool blocked = false;
        public ChessBoard(IPlayer whitePlayer, IPlayer blackPlayer, Timer whiteTimer, Timer blackTimer, TextBlock whoWonText, TextBlock reasonText) : base()
        {
            this.whoWonText = whoWonText;
            this.reasonText = reasonText;

            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;

            this.whiteTimer = whiteTimer;
            this.blackTimer = blackTimer;

            for (int i = 0; i < 8; i++)
            {
                AddRow();
                AddColumn();
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    AddField(i, j);
                }
            }

            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            GameSingleton.chessBoard = this;
            SetUpFromBoard();

            if (whitePlayer is Sterlet && blackPlayer is HumanPlayer)
            {
                Invert();
            }

            GetMoveFromPlayer(Board.toMove == Piece.WHITE ? whitePlayer : blackPlayer);
        }

        public void Invert()
        {
            StopHighlightingField(lastMoveFields.start);
            StopHighlightingField(lastMoveFields.end);

            isInverted = !isInverted;
            foreach (PieceImage pieceImage in boardArray)
            {
                if (pieceImage != null)
                {
                    int newRow, newCol;
                    (newRow, newCol) = FieldToRowCol(pieceImage.field);
                    SetRow(pieceImage, newRow);
                    SetColumn(pieceImage, newCol);
                }
            }

            HighlightLastMoveField(lastMoveFields.start);
            HighlightLastMoveField(lastMoveFields.end);
        }

        private void AddRow()
        {
            RowDefinition row = new RowDefinition
            {
                Height = new GridLength(FIELDSIZE)
            };
            RowDefinitions.Add(row);
        }

        private void AddColumn()
        {
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(FIELDSIZE);
            ColumnDefinitions.Add(column);
        }

        private void AddField(int row, int col)
        {
            Border field = new Border
            {
                Background = (row + col) % 2 == 0 ? WHITE_FIELDS_COLOR : BLACK_FIELDS_COLOR,
                Width = FIELDSIZE,
                Height = FIELDSIZE
            };
            SetRow(field, row);
            SetColumn(field, col);
            Children.Add(field);
            fieldArray[row, col] = field;
        }
        private void SetUpFromBoard()
        {
            for (int i = 0; i < 64; i++)
            {
                if (Board.board[i] != Piece.NONE && Board.board[i] != Piece.WALL)
                {
                    AddPieceToBoard(i, Board.board[i]);
                }
            }

        }

        public void MovePieceOnBoard(int oldField, int newField)
        {
            if (blocked)
            {
                return;
            }

            PieceImage piece = boardArray[oldField];
            boardArray[oldField] = null;
            boardArray[newField] = piece;

            (int newRow, int newCol) = FieldToRowCol(newField);
            piece.field = newField;

            SetRow(piece, newRow);
            SetColumn(piece, newCol);
        }

        public void RemovePieceFromBoard(int field)
        {
            if (blocked)
            {
                return;
            }

            PieceImage piece = boardArray[field];
            boardArray[field] = null;

            if (Piece.GetColor(piece.piece) == Piece.WHITE)
            {
                whitePlayer.RemovePiece(piece);
            }
            else
            {
                blackPlayer.RemovePiece(piece);
            }
            Children.Remove(piece);
        }

        public void AddPieceToBoard(int field, uint newPiece)
        {
            if (blocked)
            {
                return;
            }

            PieceImage piece;
            if (Piece.GetColor(newPiece) == Piece.BLACK)
            {
                piece = blackPlayer.GetPiece(newPiece, field);
            }
            else
            {
                piece = whitePlayer.GetPiece(newPiece, field);
            }
            
            boardArray[field] = piece;
            int row, col;
            (row, col) = FieldToRowCol(piece.field);
            SetRow(piece, row);
            SetColumn(piece, col);

            Children.Add(piece);
        }

        public static int RowColToFieldNumber(int row, int col)
        {
            if (isInverted)
            {
                return 63 - ((7 - row) * 8 + col);
            }
            return (7 - row) * 8 + col;
        }

        public static (int, int) FieldToRowCol(int field)
        {
            int row, col;
            if (isInverted)
            {
                row = field / 8;
                col = 7 - (field % 8);
            }
            else
            {
                row = 7 - field / 8;
                col = field % 8;
            }
            return (row, col);
        }

        public void HighlightFields(List<int> fields)
        {
            if (blocked)
            {
                return;
            }

            foreach (int field in fields)
            {
                HighlightField(field);
            }
        }

        public void StopHighlightingFields(List<int> fields)
        {
            foreach (int field in fields)
            {
                if (field != lastMoveFields.start && field != lastMoveFields.end)
                {
                    StopHighlightingField(field);
                }
            }

            HighlightLastMoveField(lastMoveFields.start);
            HighlightLastMoveField(lastMoveFields.end);
        }

        private void HighlightLastMoveField(int field)
        {
            (int row, int col) = FieldToRowCol(field);
            SolidColorBrush highlightedColor = (row + col) % 2 == 0 ? WHITE_LAST_MOVE_COLOR : BLACK_LAST_MOVE_COLOR;

            Border square = fieldArray[row, col];
            square.Background = highlightedColor;
        }

        private void StopHighlightingField(int field)
        {
            (int row, int col) = FieldToRowCol(field);
            SolidColorBrush color = (row + col) % 2 == 0 ? WHITE_FIELDS_COLOR : BLACK_FIELDS_COLOR;

            Border square = fieldArray[row, col];
            square.Background = color;
        }


        private void HighlightField(int field)
        {
            (int row, int col) = FieldToRowCol(field);
            SolidColorBrush highlightedColor = (row + col) % 2 == 0 ? WHITE_HIGHLITED_COLOR : BLACK_HIGHLITED_COLOR;

            Border square = fieldArray[row, col];
            square.Background = highlightedColor;
        }

        private void HighlightLastMove(Move move)
        {
            StopHighlightingField(lastMoveFields.start);
            StopHighlightingField(lastMoveFields.end);
            lastMoveFields = (move.StartSquare, move.TargetSquare);
            HighlightLastMoveField(lastMoveFields.start);
            HighlightLastMoveField(lastMoveFields.end);
        }

        public void UpdateBoard()
        { 
            if (blocked)
                return;

            for (int i = 0; i < 64; i++)
            {
                if (boardRepresentation[i] != Board.board[i])
                {
                    if (boardRepresentation[i] != Piece.NONE)
                        RemovePieceFromBoard(i);
                    if (Board.board[i] != Piece.NONE)
                        AddPieceToBoard(i, Board.board[i]);
                    boardRepresentation[i] = Board.board[i];
                }
            }

            if (whitePlayer is HumanPlayer && blackPlayer is HumanPlayer)
            {
                Thread.Sleep(10);
                Invert();
            }

            if (Board.toMove == Piece.WHITE)
            {
                blackTimer.Stop();
                whiteTimer.Start();
            }
            else
            {
                whiteTimer.Stop();
                blackTimer.Start();
            }

            List<Move> moves = MoveGenerator.GenerateMoves();
            GameState gameState = Referee.DetermineGameState(moves);
            if (gameState != GameState.ONGOING)
            {
                string whoWon = "";
                string reason = "";

                switch (gameState)
                {
                    case GameState.WIN:
                        whoWon = Board.toMove == Piece.WHITE ? "Black wins!" : "White wins!";
                        reason = "Checkmate!";
                        break;
                    case GameState.DRAW_BY_MATERIAL:
                        whoWon = "Draw";
                        reason = "Insufficient material";
                        break;
                    case GameState.DRAW_BY_REPETITION:
                        whoWon = "Draw";
                        reason = "By repetition";
                        break;
                    case GameState.DRAW_BY_STALEMATE:
                        whoWon = "Draw";
                        reason = "By stalemate";
                        break;
                    case GameState.DRAW_BY_HALFMOVES:
                        whoWon = "Draw";
                        reason = "By fifty-move rule";
                        break;
                }
                Finish(whoWon, reason);
            }
            GetMoveFromPlayer(Board.toMove == Piece.WHITE ? whitePlayer : blackPlayer);
        }

        public async void GetMoveFromPlayer(IPlayer player)
        {
            Move move = null;
            await Task.Run(() =>
            {
                move = player.ChooseMove();
                if (move != null)
                {
                    Board.MakeMove(move);
                }
            });
            HighlightLastMove(move);
            if (player is Sterlet sterlet)
            {
                Game game = Game.game;
                game.UpdateDiagnostics(sterlet.SearchResults);
            }
            UpdateBoard();
        }

        public void Finish(string whoWon, string reason)
        {
            whoWonText.Text = whoWon;
            reasonText.Text = reason;

            blocked = true;
            whiteTimer.Stop();
            blackTimer.Stop();
        }

    }
}
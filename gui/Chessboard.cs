using Chess.Abstracts;
using Chess.Brain;
using Chess.Logic;
using System;
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
        
        private PieceImage[] boardArray = new PieceImage[64];
        private Border[,] fieldArray = new Border[8, 8];
        private Player whitePlayer;
        private Player blackPlayer;

        private Timer whiteTimer;
        private Timer blackTimer;

        private readonly TextBlock whoWonText;
        private readonly TextBlock reasonText;

        private uint[] boardRepresentation = (uint[])Board.board.Clone();

        private bool blocked = false;

        public ChessBoard(Player whitePlayer, Player blackPlayer, Timer whiteTimer, Timer blackTimer, TextBlock whoWonText, TextBlock reasonText) : base()
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

            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;


            SetUpFromBoard();

            //this is akward and has to be changed!
            if (whitePlayer is Sterlet)
            {
                ((Sterlet)whitePlayer).setUp();
            }
            if (blackPlayer is Sterlet)
            {
                ((Sterlet)blackPlayer).setUp();
            }
            GetMoveFromPlayer(Board.toMove == Piece.WHITE ? whitePlayer : blackPlayer);
        }

        public void Invert()
        {
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
        }

        private void AddRow()
        {
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(FIELDSIZE);
            this.RowDefinitions.Add(row);
        }

        private void AddColumn()
        {
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(FIELDSIZE);
            this.ColumnDefinitions.Add(column);
        }

        private void AddField(int row, int col)
        {
            Border field = new Border();
            field.Background = (row + col) % 2 == 0 ? WHITE_FIELDS_COLOR : BLACK_FIELDS_COLOR;
            field.Width = FIELDSIZE;
            field.Height = FIELDSIZE;
            SetRow(field, row);
            SetColumn(field, col);
            this.Children.Add(field);
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
                return;
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
                return;

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
            this.Children.Remove(piece);
        }

        public void AddPieceToBoard(int field, uint newPiece)
        {
            if (blocked)
                return;

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

            this.Children.Add(piece);
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
                return;

            int row, col;
            foreach (int field in fields)
            {
                (row, col) = FieldToRowCol(field);
                SolidColorBrush highlightedColor = (row + col) % 2 == 0 ? WHITE_HIGHLITED_COLOR : BLACK_HIGHLITED_COLOR;

                Border square = fieldArray[row, col];
                square.Background = highlightedColor;
            }
        }

        public void StopHighlightingFields(List<int> fields)
        {
            if (blocked)
                return;

            int row, col;
            foreach (int field in fields)
            {
                (row, col) = FieldToRowCol(field);
                SolidColorBrush color = (row + col) % 2 == 0 ? WHITE_FIELDS_COLOR : BLACK_FIELDS_COLOR;

                Border square = fieldArray[row, col];
                square.Background = color;
            }
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
            if(gameState != GameState.ONGOING)
            {
                if (gameState == GameState.WIN)
                {
                    string whoWon = Board.toMove == Piece.WHITE ? "Black wins!" : "White wins!";
                    string reason = "Checkmate!";
                    Finish(whoWon, reason);
                }
                else if(gameState == GameState.DRAW_BY_MATERIAL)
                {
                    Finish("Draw", "Insufficient material");
                }
                else if(gameState == GameState.DRAW_BY_STALEMATE)
                {
                    Finish("Draw", "By stalemate");
                }
                return;
            }

            GetMoveFromPlayer(Board.toMove == Piece.WHITE ? whitePlayer : blackPlayer);
        }

        public async void GetMoveFromPlayer(Player player)
        {
            Console.WriteLine("This was called");
            await Task.Run(() =>
            {
                Move move = player.ChooseMove();
                if (move != null)
                {
                    Board.MakeMove(move);
                }
            });
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
using System;
using System.Collections.Generic;
using System.Threading;
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
        private readonly SolidColorBrush BLACK_HIGHLITED_COLOR = new SolidColorBrush(Color.FromRgb(100, 100, 00));
        
        private PieceImage[] boardArray = new PieceImage[64];
        private Border[,] fieldArray = new Border[8, 8];
        public List<Logic.Move> moves;
        private PlayerType whitePlayerType;
        private PlayerType blackPlayerType;

        private Timer whiteTimer;
        private Timer blackTimer;

        private readonly TextBlock whoWonText;
        private readonly TextBlock reasonText;

        private uint[] boardRepresentation = (uint[])Logic.Board.board.Clone();

        private bool blocked = false;
        public ChessBoard(PlayerType whitePlayerType, PlayerType blackPlayerType, Timer whiteTimer, Timer blackTimer, TextBlock whoWonText, TextBlock reasonText) : base()
        {
            this.whoWonText = whoWonText;
            this.reasonText = reasonText;

            this.whitePlayerType = whitePlayerType;
            this.blackPlayerType = blackPlayerType;

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
                if (Logic.Board.board[i] != Logic.Piece.NONE && Logic.Board.board[i] != Logic.Piece.WALL)
                {
                    AddPieceToBoard(i, Logic.Board.board[i]);
                }
            }

            Console.WriteLine("HERE 5");

            moves = Logic.MoveGenerator.GenerateMoves();
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

            this.Children.Remove(piece);
        }

        public void AddPieceToBoard(int field, uint newPiece)
        {
            if (blocked)
                return;

            PieceImage piece = new PlayerControlledPiece(newPiece, field, this);
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
                if (boardRepresentation[i] != Logic.Board.board[i])
                {
                    if (boardRepresentation[i] != Logic.Piece.NONE)
                        RemovePieceFromBoard(i);
                    if (Logic.Board.board[i] != Logic.Piece.NONE)
                        AddPieceToBoard(i, Logic.Board.board[i]);
                    boardRepresentation[i] = Logic.Board.board[i];
                }
            }

            if (whitePlayerType == blackPlayerType && whitePlayerType == PlayerType.HUMAN_PLAYER)
            {
                Thread.Sleep(100);
                Invert();
            }

            if (Logic.Board.toMove == Logic.Piece.WHITE)
            {
                blackTimer.Stop();
                whiteTimer.Start();
            }
            else
            {
                whiteTimer.Stop();
                blackTimer.Start();
            }

            moves = Logic.MoveGenerator.GenerateMoves();
        }


        public void Finish(string whoWon, string reason)
        {
            whoWonText.Text = whoWon;
            reasonText.Text = reason;

            moves = new List<Logic.Move>();
            blocked = true;
            whiteTimer.Stop();
            blackTimer.Stop();
        }
    }
}
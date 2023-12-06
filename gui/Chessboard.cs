using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Chess.gui{
    public class ChessBoard : Grid
    {

        public const int FIELDSIZE = 75;
        private readonly SolidColorBrush WHITE_FIELDS_COLOR = Brushes.Wheat;
        private readonly SolidColorBrush BLACK_FIELDS_COLOR = Brushes.DarkGreen;
        private readonly SolidColorBrush WHITE_HIGHLITED_COLOR = new SolidColorBrush(Color.FromRgb(255, 200, 150));
        private readonly SolidColorBrush BLACK_HIGHLITED_COLOR = new SolidColorBrush(Color.FromRgb(100, 100, 00));
        private PieceImage[] boardArray = new PieceImage[120];
        private Border[,] fieldArray = new Border[8, 8];
        public List<Logic.Move> moves;

        private uint[] boardRepresentation =(uint[])Logic.Board.board.Clone();
        public ChessBoard() : base()
        {

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
            for(int i = 0; i < 64; i++)
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
            PieceImage piece = boardArray[field];
            boardArray[field] = null;

            this.Children.Remove(piece);
        }

        public void AddPieceToBoard(int field, uint newPiece)
        {
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
            return (7 - row) * 8 + col;
        }

        public static (int, int) FieldToRowCol(int field)
        { 
            int row = 7 - field / 8;
            int col = field % 8;

            return (row, col);
        }

        public void HighlightFields(List<int> fields)
        {
            int row, col;
            foreach(int field in fields)
            {
                (row, col) = FieldToRowCol(field);
                SolidColorBrush highlightedColor = (row + col) % 2 == 0 ? WHITE_HIGHLITED_COLOR : BLACK_HIGHLITED_COLOR;

                Border square = fieldArray[row, col];
                square.Background = highlightedColor;
            }
        }

        public void StopHighlightingFields(List<int> fields)
        {
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
            moves = Logic.MoveGenerator.GenerateMoves();
        }

        public void BlackWin()
        {
            EndPopup("BLACK WON!", Color.FromRgb(0, 0, 0), Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0));
        }

        public void WhiteWin()
        {
            EndPopup("WHITE WON!", Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0), Color.FromRgb(0, 0, 0)); 
        }

        public void Draw()
        {
            EndPopup("DRAW", Color.FromRgb(0, 0, 0), Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0));
        }

        private void EndPopup(string text, Color textColor, Color backgroundColor, Color shadowColor)
        {
            Popup endPopup = new Popup();

            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(backgroundColor),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(backgroundColor)
            };

            Label label = new Label
            {
                Content = text,
                Padding = new Thickness(10),
                FontSize = 100,
                Foreground = new SolidColorBrush(textColor)
            };

            label.Effect = new DropShadowEffect
            {
                Color = shadowColor,
                ShadowDepth = 0,
                BlurRadius = 2
            };

            border.Child = label;

            endPopup.Child = border;
            endPopup.PlacementTarget = this;
            endPopup.Placement = PlacementMode.Center;
            Children.Add(endPopup);

            endPopup.IsOpen = true;
        }
    }
}
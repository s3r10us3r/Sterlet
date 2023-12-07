using Chess.Logic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess.gui
{
    public class PieceImage : Image
    {

        private static readonly string[] TYPE_STRINGS = { null, "pawn", "bishop", "knight", "rook", "queen", "king" };

        private readonly string CHESS_PIECES_PATH = @"\Resources\chess_pieces\";

        protected ChessBoard parent;
        protected uint piece;
        public int field;

        public PieceImage(uint piece, int field, ChessBoard parent) : base()
        {
            this.piece = piece;
            LoadImage(piece);

            this.Width = 60;
            this.Height = 60;

            this.field = field;
            this.parent = parent;
        }

        private void LoadImage(uint piece)
        {
            uint color = Piece.GetColor(piece);
            uint type = Piece.GetPiece(piece);

            string colorString = (color == Piece.WHITE) ? "white" : "black";
            string pieceString = TYPE_STRINGS[type];

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            string image_path = CHESS_PIECES_PATH + colorString + "_" + pieceString + ".png";

            bitmap.UriSource = new Uri(image_path, UriKind.Relative);
            bitmap.EndInit();
            this.Source = bitmap;
        }

        protected void MoveOnBoard(int oldField, int newField)
        {
            parent.MovePieceOnBoard(oldField, newField);
        }
    }

    public class PlayerControlledPiece : PieceImage
    {
        private bool isDragging;
        private Point initialMousePosition;
        private TranslateTransform transform = new TranslateTransform();
        protected List<Move> availableMoves;

        public PlayerControlledPiece(uint piece, int field, ChessBoard parent) : base(piece, field, parent)
        {
            RenderTransform = transform;
            PreviewMouseDown += PlayerControlledPiece_PreviewMouseDown;
            PreviewMouseMove += PlayerControlledPiece_PreviewMouseMove;
            PreviewMouseUp += PlayerControlledPiece_PreviewMouseUp;
        }

        private void PlayerControlledPiece_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !PromotionMenu.isOpened)
            {
                uint color = Piece.GetColor(piece);

                availableMoves = GetAvailableMoves();
                isDragging = true;

                parent.HighlightFields(GetFieldsToHighlight());
                initialMousePosition = e.GetPosition(Application.Current.MainWindow);
                CaptureMouse();
            }
        }

        private void PlayerControlledPiece_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentMousePosition = e.GetPosition(Application.Current.MainWindow);
                Vector movement = currentMousePosition - initialMousePosition;
                transform.X = movement.X;
                transform.Y = movement.Y;
            }
        }

        private async void PlayerControlledPiece_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && isDragging)
            {
                isDragging = false;
                ReleaseMouseCapture();
                parent.StopHighlightingFields(GetFieldsToHighlight());

                double deltaX = transform.X;
                double deltaY = transform.Y;

                int rowChange = (int)Math.Round(deltaY / ChessBoard.FIELDSIZE);
                int colChange = (int)Math.Round(deltaX / ChessBoard.FIELDSIZE);

                int oldRow, oldCol;
                (oldRow, oldCol) = ChessBoard.FieldToRowCol(field);
                int newRow = oldRow + rowChange;
                int newCol = oldCol + colChange;

                transform.X = 0;
                transform.Y = 0;

                int newField = ChessBoard.RowColToFieldNumber(newRow, newCol);
                Console.WriteLine($"CURRENT ROW {oldRow} NEW ROW {newRow}");
                Console.WriteLine($"CURRENT COL {oldCol} NEW COL {newCol}");
                Console.WriteLine($"CURRENT FIELD {field} NEW FIELD {newField}");

                List<Move> movesThatCouldBeMade = new List<Move>();
                foreach (Move move in availableMoves)
                {
                    if (move.TargetSquare == newField)
                    {
                        movesThatCouldBeMade.Add(move);
                    }
                }
                if (movesThatCouldBeMade.Count == 1)
                {
                    Move move = movesThatCouldBeMade[0];
                    Board.MakeMove(move);
                    Console.WriteLine(move);
                    parent.UpdateBoard();
                }
                else if (movesThatCouldBeMade.Count > 1)
                {
                    uint chosenPiece = await ShowPromotionMenuAndWait();
                    uint chosenPieceType = Piece.GetPiece(chosenPiece);
                    int promotionFlag = 0;
                    switch (chosenPieceType)
                    {
                        case Piece.QUEEN:
                            promotionFlag = Move.Flag.PromoteToQueen;
                            break;
                        case Piece.ROOK:
                            promotionFlag = Move.Flag.PromoteToRook;
                            break;
                        case Piece.BISHOP:
                            promotionFlag = Move.Flag.PromoteToBishop;
                            break;
                        case Piece.KNIGHT:
                            promotionFlag = Move.Flag.PromoteToKnight;
                            break;
                    }
                    foreach (Move move in movesThatCouldBeMade)
                    {
                        if (move.MoveFlag == promotionFlag)
                        {
                            Board.MakeMove(move);
                            parent.UpdateBoard();
                            break;
                        }
                    }
                }

            }
        }

        private List<Move> GetAvailableMoves()
        {
            List<Move> availableMoves = new List<Move>();

            foreach (Move move in parent.moves)
            {
                if (move.StartSquare == field)
                {
                    availableMoves.Add(move);
                }
            };

            return availableMoves;
        }

        private List<int> GetFieldsToHighlight()
        {
            List<int> fieldsToHighlight = new List<int>();
            foreach (Move move in availableMoves)
            {
                fieldsToHighlight.Add(move.TargetSquare);
            }
            return fieldsToHighlight;
        }

        private async Task<uint> ShowPromotionMenuAndWait()
        {
            TaskCompletionSource<uint> tcs = new TaskCompletionSource<uint>();

            PromotionMenu promotionMenu = new PromotionMenu(Piece.GetColor(piece))
            {
                PlacementTarget = this,
                Placement = PlacementMode.Center
            };

            promotionMenu.PromotionChosen += (sender, chosenPiece) =>
            {
                tcs.SetResult(chosenPiece);
            };


            promotionMenu.IsOpen = true;

            uint result = await tcs.Task;
            return result;
        }
    }
}

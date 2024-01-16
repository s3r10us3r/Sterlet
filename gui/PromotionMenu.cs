using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess.gui
{
    class PromotionMenu : Popup
    {
        public static bool isOpened = false;

        public int ChosenPiece { get; set; }

        public delegate void PromotionChosenEventHandler(object sender, uint chosenPiece);

        public event PromotionChosenEventHandler PromotionChosen;

        public PromotionMenu(uint color) : base()
        {
            isOpened = true;

            uint[] possiblePieces = { Logic.Piece.KNIGHT, Logic.Piece.BISHOP, Logic.Piece.ROOK, Logic.Piece.QUEEN };
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            foreach (uint piece in possiblePieces)
            {
                PieceButton pieceButton = new PieceButton(piece, color, this);
                stackPanel.Children.Add(pieceButton);
            }

            Child = stackPanel;
            IsOpen = true;
        }

        public void OnPromotionChosen(uint chosenPiece)
        {
            PromotionChosen?.Invoke(this, chosenPiece);
        }

        private class PieceButton : Button
        {
            private readonly string CHESS_PIECES_PATH = @"pack://application:,,,/Chess;component/Resources/chess_pieces/";
            private readonly SolidColorBrush backgroundColor = Brushes.White;
            private uint returnPiece;
            private PromotionMenu promotionMenu;
            public PieceButton(uint piece, uint color, PromotionMenu parent) : base()
            {
                promotionMenu = parent;
                returnPiece = piece + color;
                Background = backgroundColor;
                Image image = new Image();
                string uriString = color == Logic.Piece.WHITE ? CHESS_PIECES_PATH + "white_" : CHESS_PIECES_PATH + "black_";
                switch (piece)
                {
                    case Logic.Piece.QUEEN:
                        uriString += "queen.png";
                        break;
                    case Logic.Piece.BISHOP:
                        uriString += "bishop.png";
                        break;
                    case Logic.Piece.KNIGHT:
                        uriString += "knight.png";
                        break;
                    case Logic.Piece.ROOK:
                        uriString += "rook.png";
                        break;
                }
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(uriString, UriKind.RelativeOrAbsolute);
                bitmapImage.EndInit();

                image.Source = bitmapImage;

                image.Width = 60;
                image.Height = 60;
                Width = 70;
                Height = 70;
                Content = image;
                Click += Button_Click;
            }

            private void Button_Click(object sender, RoutedEventArgs e)
            {
                promotionMenu.OnPromotionChosen(returnPiece);
                promotionMenu.IsOpen = false;
                isOpened = false;
            }
        }
    }



}


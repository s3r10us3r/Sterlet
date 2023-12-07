﻿using Chess.gui;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        private readonly ChessBoard chessBoard;

        public Game(string FEN, PlayerType whitePlayerType, PlayerType blackPlayerType)
        {
            InitializeComponent();

            Logic.Board.ReadFEN(FEN);
            chessBoard = new ChessBoard(whitePlayerType, blackPlayerType);
            chessBoardBorder.Child = chessBoard;

            if (whitePlayerType == blackPlayerType && whitePlayerType == PlayerType.HUMAN_PLAYER)
                rotateButton.Visibility = Visibility.Collapsed;
        }

        private void RotateClick(object sender, RoutedEventArgs e)
        {
            chessBoard.Invert();
        }
    }
}

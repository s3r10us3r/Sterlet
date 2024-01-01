using Chess.Abstracts;
using Chess.Brain;
using Chess.gui;
using Chess.Logic;
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

        public Game(string FEN, PlayerType whitePlayerType, PlayerType blackPlayerType, TimerOptions timerOptions)
        {
            InitializeComponent();

            Timer timerWhite = new Timer(timerOptions, whiteTimer, Piece.WHITE);
            Timer timerBlack = new Timer(timerOptions, blackTimer, Piece.BLACK);

            if (timerOptions.Option == TimerOptions.Options.NoTime)
            {
                whiteTimerBorder.Visibility = Visibility.Hidden;
                blackTimerBorder.Visibility = Visibility.Hidden;
            }

            Board.ReadFEN(FEN);
            Player whitePlayer;
            if (whitePlayerType == PlayerType.HUMAN_PLAYER)
            {
                whitePlayer = new HumanPlayer();
            }
            else
            {
                whitePlayer = new Sterlet(5, Piece.WHITE);
            }

            Player blackPlayer;
            if (blackPlayerType == PlayerType.HUMAN_PLAYER)
            {
                blackPlayer = new HumanPlayer();
            }
            else
            {
                blackPlayer = new Sterlet(5, Piece.BLACK);
            }

            chessBoard = new ChessBoard(whitePlayer, blackPlayer, timerWhite, timerBlack, whoWonText, reasonText);
            chessBoardBorder.Child = chessBoard;
            timerWhite.ChessBoard = chessBoard;
            timerBlack.ChessBoard = chessBoard;
        }

        private void RotateClick(object sender, RoutedEventArgs e)
        {
            chessBoard.Invert();
        }
    }
}

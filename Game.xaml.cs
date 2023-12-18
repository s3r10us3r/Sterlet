using Chess.gui;
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

            Timer timerWhite = new Timer(timerOptions, whiteTimer, Logic.Piece.WHITE);
            Timer timerBlack = new Timer(timerOptions, blackTimer, Logic.Piece.BLACK);

            if (timerOptions.Option == TimerOptions.Options.NoTime)
            {
                whiteTimerBorder.Visibility = Visibility.Hidden;
                blackTimerBorder.Visibility = Visibility.Hidden;
            }

            Logic.Board.ReadFEN(FEN);
            chessBoard = new ChessBoard(whitePlayerType, blackPlayerType, timerWhite, timerBlack, whoWonText, reasonText);
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

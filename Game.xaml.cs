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
        private const bool DEBUGGING_ON = true;
        public static Game game;

        public Game(string FEN, PlayerType whitePlayerType, PlayerType blackPlayerType, TimerOptions timerOptions)
        {
            InitializeComponent();

            if ( !DEBUGGING_ON )
            {
                enigneAnalysis.Visibility = Visibility.Hidden;
            }

            Timer timerWhite = new Timer(timerOptions, whiteTimer, Piece.WHITE);
            Timer timerBlack = new Timer(timerOptions, blackTimer, Piece.BLACK);

            if (timerOptions.Option == TimerOptions.Options.NoTime)
            {
                whiteTimerBorder.Visibility = Visibility.Hidden;
                blackTimerBorder.Visibility = Visibility.Hidden;
            }

            Board.ReadFEN(FEN);
            IPlayer whitePlayer;
            if (whitePlayerType == PlayerType.HUMAN_PLAYER)
            {
                whitePlayer = new HumanPlayer();
            }
            else
            {
                whitePlayer = new Sterlet(timerWhite, Piece.WHITE);
            }

            IPlayer blackPlayer;
            if (blackPlayerType == PlayerType.HUMAN_PLAYER)
            {
                blackPlayer = new HumanPlayer();
            }
            else
            {
                blackPlayer = new Sterlet(timerBlack, Piece.BLACK);
            }

            chessBoard = new ChessBoard(whitePlayer, blackPlayer, timerWhite, timerBlack, whoWonText, reasonText);
            chessBoardBorder.Child = chessBoard;
            timerWhite.ChessBoard = chessBoard;
            timerBlack.ChessBoard = chessBoard;

            game = this;
        }

        private void RotateClick(object sender, RoutedEventArgs e)
        {
            chessBoard.Invert();
        }

        public void UpdateDiagnostics(SearchResults searchResults)
        {
            if (searchResults != null)
            {
                enigneAnalysis.Text = $"Depth = {searchResults.depthSearched} Eval = {searchResults.eval}  NaiveEval = {searchResults.naiveEval}";
            }
        }
    }
}

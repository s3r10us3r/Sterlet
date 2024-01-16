using Chess.gui;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    public partial class PlayerVsMachinePopupContents : UserControl
    {
        private const int WHITE = 0;
        private const int BLACK = 1;

        private const string whitePawnSource = "Resources/chess_pieces/white_pawn.png";
        private const string blackPawnSource = "Resources/chess_pieces/black_pawn.png";

        private int chosenColor = WHITE;

        private readonly List<TimerOptions> optionsList = new List<TimerOptions>
        {
            new TimerOptions(TimerOptions.Options.OneMinute), new TimerOptions(TimerOptions.Options.OnePlusOne), new TimerOptions(TimerOptions.Options.TwoPlusOne), //bullet
            new TimerOptions(TimerOptions.Options.ThreeMinutes), new TimerOptions(TimerOptions.Options.ThreePlusTwo), new TimerOptions(TimerOptions.Options.FiveMinutes), //blitz
            new TimerOptions(TimerOptions.Options.TenMinutes), new TimerOptions(TimerOptions.Options.FifteenPlusTen), new TimerOptions(TimerOptions.Options.ThirtyMinutes), //rapid
            new TimerOptions(TimerOptions.Options.NoTime)
        };
        private int timeIndex = 6;

        private void TimeUpClicked(object sender, RoutedEventArgs e)
        {
            if (timeIndex + 1 < optionsList.Count)
            {
                timeIndex++;
                timeTextBlock.Text = optionsList[timeIndex].GetOptionString();
            }
        }

        private void TimeDownClicked(object sender, RoutedEventArgs e)
        {
            if (timeIndex > 0)
            {
                timeIndex--;
                timeTextBlock.Text = optionsList[timeIndex].GetOptionString();
            }
        }

        private void SwitchColor(object sender, RoutedEventArgs e)
        {
            if (chosenColor == WHITE)
            {
                chosenColor = BLACK;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();

                bitmap.UriSource = new Uri(blackPawnSource, UriKind.Relative);
                bitmap.EndInit();
                colorButton.Source = bitmap;
            }
            else if (chosenColor == BLACK)
            {
                chosenColor = WHITE;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();

                bitmap.UriSource = new Uri(whitePawnSource, UriKind.Relative);
                bitmap.EndInit();
                colorButton.Source = bitmap;
            }
        }

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            PlayerType whitePlayerType;
            PlayerType blackPlayerType;

            if (chosenColor == WHITE)
            {
                whitePlayerType = PlayerType.HUMAN_PLAYER;
                blackPlayerType = PlayerType.COMPUTER_PLAYER;
            }
            else
            {
                whitePlayerType = PlayerType.COMPUTER_PLAYER;
                blackPlayerType = PlayerType.HUMAN_PLAYER;
            }

            MainWindowSingleton.mainWindow.ChangePage(new Game(Logic.Board.STARTING_POSITION, whitePlayerType, blackPlayerType, optionsList[timeIndex]));
        }

        public PlayerVsMachinePopupContents()
        {
            InitializeComponent();
        }
    }
}

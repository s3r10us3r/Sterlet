using Chess.gui;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for PlayerVsPLayerPopupControl.xaml
    /// </summary>
    public partial class PlayerVsPLayerPopupContents : UserControl
    {
        private readonly List<TimerOptions> optionsList = new List<TimerOptions> 
        {
            new TimerOptions(TimerOptions.Options.OneMinute), new TimerOptions(TimerOptions.Options.OnePlusOne), new TimerOptions(TimerOptions.Options.TwoPlusOne), //bullet
            new TimerOptions(TimerOptions.Options.ThreeMinutes), new TimerOptions(TimerOptions.Options.ThreePlusTwo), new TimerOptions(TimerOptions.Options.FiveMinutes), //blitz
            new TimerOptions(TimerOptions.Options.TenMinutes), new TimerOptions(TimerOptions.Options.FifteenPlusTen), new TimerOptions(TimerOptions.Options.ThirtyMinutes), //rapid
            new TimerOptions(TimerOptions.Options.NoTime)
        };
        private int timeIndex = 6;

        public PlayerVsPLayerPopupContents()
        {
            InitializeComponent();
        }

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

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ChangePage(new Game(Logic.Board.STARTING_POSITION, PlayerType.HUMAN_PLAYER, PlayerType.HUMAN_PLAYER, optionsList[timeIndex]));
        }
    }
}

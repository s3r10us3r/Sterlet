using Chess.Brain;
using Chess.gui;
using Chess.Logic;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void PlayerVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = true;
        }

        private void PlayerVsMachine_Click(object sender, RoutedEventArgs e)
        {
            //popup.IsOpen = true;
            ((MainWindow)Application.Current.MainWindow).ChangePage(new Game(Logic.Board.STARTING_POSITION, new HumanPlayer(), new Sterlet(5, Piece.BLACK), new TimerOptions(TimerOptions.Options.NoTime)));
        }
    }
}

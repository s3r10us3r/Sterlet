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
            playerVsPlayerPopup.IsOpen = true;
        }

        private void PlayerVsMachine_Click(object sender, RoutedEventArgs e)
        {
            playerVsMachinePopup.IsOpen = true;
            //((MainWindow)Application.Current.MainWindow).ChangePage(new Game(Logic.Board.STARTING_POSITION, PlayerType.COMPUTER_PLAYER, PlayerType.HUMAN_PLAYER, new TimerOptions(TimerOptions.Options.NoTime)));
        }
    }
}

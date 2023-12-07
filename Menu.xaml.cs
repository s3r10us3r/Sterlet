using Chess.gui;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Menu : Page
    {
        private MainWindow mainWindow;

        public Menu(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void PlayerVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ChangePage(new Game(Logic.Board.STARTING_POSITION, PlayerType.HUMAN_PLAYER, PlayerType.HUMAN_PLAYER));
        }

        private void PlayerVsMachine_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PLAYER VS MACHINE PLACEHOLDER");
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Menu : Page
    {
        private Popup openedPopup = null;

        public Menu()
        {
            InitializeComponent();
        }

        private void PlayerVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (openedPopup != null && !ReferenceEquals(openedPopup, playerVsPlayerPopup))
            {
                openedPopup.IsOpen = false;
            }

            playerVsPlayerPopup.IsOpen = true;
            openedPopup = playerVsPlayerPopup;
        }

        private void PlayerVsMachine_Click(object sender, RoutedEventArgs e)
        {
            if (openedPopup != null && !ReferenceEquals(openedPopup, playerVsMachinePopup))
            {
                openedPopup.IsOpen = false;
            }

            playerVsMachinePopup.IsOpen = true;
            openedPopup = playerVsMachinePopup;
        }
    }
}

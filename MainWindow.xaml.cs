using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainFrame.Content = new Menu();
            Application.Current.MainWindow = this;
        }

        public void ChangePage(Page page)
        {
            mainFrame.Content = page;
        }
    }
}

using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public Game(string FEN)
        {
            Logic.Board.ReadFEN(FEN);
            InitializeComponent();
        }
    }
}

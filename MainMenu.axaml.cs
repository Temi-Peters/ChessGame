using Avalonia.Controls;

namespace ChessGame
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Open the main game window
            var gameWindow = new MainWindow();
            gameWindow.Show();

            // Close the main menu
            this.Close();
        }
    }
}

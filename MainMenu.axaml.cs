using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChessGame;

public partial class MainMenu : Window
{
    public MainMenu()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
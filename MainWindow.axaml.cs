using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using System;

namespace ChessGame
{    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            var grid = this.FindControl<Grid>("BoardGrid"); // Name your Grid "BoardGrid" in XAML

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new Border
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.Beige : Brushes.SaddleBrown
                    };

                    square.PointerPressed += (s, e) =>
                    {
                        Console.WriteLine($"Clicked square: {row},{col}");
                    };

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);
                    grid.Children.Add(square);
                }
            }
        }
    }
}

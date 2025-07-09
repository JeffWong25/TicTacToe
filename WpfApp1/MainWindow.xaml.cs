using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly Dictionary<Player, ImageSource> imageSource = new()
        {
            {Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
            {Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
        };

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new()
        {
            {Player.X, new ObjectAnimationUsingKeyFrames()},
            {Player.O, new ObjectAnimationUsingKeyFrames()} 
        };

        private readonly DoubleAnimation fadeOutAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(0.5),
            From = 1,
            To = 0
        };

        private readonly DoubleAnimation fadeInAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(0.5),
            From = 0,
            To = 1
        };


        private readonly Image[,] imageControls = new Image[3, 3];
        private readonly GameState gameState = new GameState();
        //You can’t assign gameState to another GameState later. But you can still call methods on it


        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();
            SetupAnimation();
            //subscribe the method to the event that created in the GameState.cs
            gameState.MoveMade += OnMoveMade;
            gameState.GameEnded += OnGameEnded;
            gameState.GameRestarted += OnGameRestarted;
        }

        private void SetupGameGrid()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Image imageControl = new Image(); // Create a new empty Image control. This is like a blank box that can later show X or O
                    GameGrid.Children.Add(imageControl);// Add this image control into your WPF Grid UI so it appears on screen.
                    imageControls[r,c] = imageControl;// Save this image control into your 2D array. So later, you can update the image easily (e.g. show X or O).
                }
            }
        }

        private void SetupAnimation()
        {
            animations[Player.X].Duration = TimeSpan.FromSeconds(0.25);
            animations[Player.O].Duration = TimeSpan.FromSeconds(0.25);

            for (int i = 0; i < 16; i++)
            {
                Uri xUri = new Uri($"pack://application:,,,/Assets/X{i}.png");
                BitmapImage xImg = new BitmapImage(xUri);
                DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
                animations[Player.X].KeyFrames.Add(xKeyFrame);

                Uri oUri = new Uri($"pack://application:,,,/Assets/O{i}.png");
                BitmapImage oImg = new BitmapImage(oUri);
                DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
                animations[Player.O].KeyFrames.Add(oKeyFrame);
            }
        }

        private async Task FadeOut(UIElement uIElement)
        {
            uIElement.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
            uIElement.Visibility = Visibility.Hidden;
        }

        private async Task FadeIn(UIElement uIElement)
        {
            uIElement.Visibility = Visibility.Visible;
            uIElement.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan); 
        }

        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source= winnerImage;
            await FadeIn(EndScreen);
        }

        private async Task TransitionToGameScreen()
        {
            await FadeOut(EndScreen);
            Line.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
        }

        //Point is a struct (a small data type) that represents a position in 2D space.
       /* public struct Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }*/
        private (Point,Point) FindLinePoint(WinInfo winInfo)
        {
            double squareSize = GameGrid.Width / 3;
            double margin = squareSize / 2;

            if (winInfo.Type == WinType.Row)
            {
                double y = winInfo.Number * squareSize + margin;
                return (new Point(0,y),new Point(GameGrid.Width,y));
            }

            if (winInfo.Type == WinType.Column)
            {
                double x = winInfo.Number * squareSize + margin;
                return (new Point(x,0), new Point(x, GameGrid.Height));
            }
            if (winInfo.Type == WinType.LeftToRightDiagonal)
            {
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
            }

            return (new Point(GameGrid.Width, 0),new Point(0,GameGrid.Height));
        }
        
        // async void - only for event handles, dont use unless it's an event. You cannot wait for it, cnnot catch exceptions
        // async task - Use for everything else, Lets you await, combine tasks, handle errors
        private async Task ShowLine(WinInfo winInfo)
        {
            (Point start, Point end) = FindLinePoint(winInfo);
            Line.X1 = start.X;
            Line.Y1 = start.Y;

            DoubleAnimation x2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.25),
                From = start.X,
                To = end.X
            };

            DoubleAnimation y2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.25),
                From = start.Y,
                To = end.Y
            };

            Line.Visibility = Visibility.Visible;
            Line.BeginAnimation(Line.X2Property,  x2Animation);
            Line.BeginAnimation(Line.Y2Property , y2Animation);
            await Task.Delay(x2Animation.Duration.TimeSpan);
        }

        private void OnMoveMade(int r, int c) 
        {
            Player player = gameState.GameGrid[r, c]; //Looks up who owns this square (X, O, or None) in your game logic.
            imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]); //Updates the image at that grid cell to show the right picture (X, O, or empty).
            PlayerImage.Source = imageSource[gameState.CurrentPlayer];//Updates indicator image to show whose turn is next.
        }

        // It means this method can use the await keyword to:
        // run something asynchronously(like a delay or a web request),
        //pause and let other code continue,
        //then come back and continue when it’s ready.
        private async void OnGameEnded(GameResult result) 
        {
            await Task.Delay(1000);
            if (result.Winner == Player.None)
            {
               await TransitionToEndScreen("It's a tie!", null);
            }
            else
            {
                await ShowLine(result.WinInfo);
                await Task.Delay(1000);
                await TransitionToEndScreen("Winner:", imageSource[result.Winner]);
            }
        }

        public async void OnGameRestarted()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                    imageControls[r, c].Source = null;
                }              
            }

            PlayerImage.Source = imageSource[gameState.CurrentPlayer];
            await TransitionToGameScreen();
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareSize = GameGrid.Width / 3;
            Point clickPosition = e.GetPosition(GameGrid);
            int row = (int)(clickPosition.Y / squareSize);
            int col = (int)(clickPosition.X / squareSize);
            gameState.MakeMove(row, col);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (gameState.GameOver)
            {
                gameState.Reset();
            }
            
        }
    }
}

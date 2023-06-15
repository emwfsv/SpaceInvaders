using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpaceShooter
{
    public partial class MainWindow : Window
    {
        private bool isMovingLeft;
        private bool isMovingRight;
        private bool isShooting;
        private bool isBanditMovingRight; 
        private bool isBanditMovingLeft;
        private int StarshipSpeed = 5;
        private int BanditSpeed = 2;


        public MainWindow()
        {
            InitializeComponent();
            isBanditMovingRight = true;
            CompositionTarget.Rendering += GameLoop;
            AddBandits();
        }

        private void GameLoop(object sender, System.EventArgs e)
        {
            if (GameCanvas.ActualWidth == 0)
                return;

            // Handle player movement
            HandleSpaceShip();

            // Handle Bandit movement
            HandleBandits();

            // Handle shooting
            if (isShooting)
                Shoot();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                isMovingLeft = true;

            if (e.Key == Key.Right)
                isMovingRight = true;

            if (e.Key == Key.Space)
                isShooting = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                isMovingLeft = false;

            if (e.Key == Key.Right)
                isMovingRight = false;

            if (e.Key == Key.Space)
                isShooting = false;
        }

        private void HandleSpaceShip()
        {
            if (isMovingLeft && Canvas.GetLeft(img_SpaceShip) > 0)
                Canvas.SetLeft(img_SpaceShip, Canvas.GetLeft(img_SpaceShip) - StarshipSpeed);

            if (isMovingRight && Canvas.GetLeft(img_SpaceShip) + img_SpaceShip.Width < GameCanvas.ActualWidth)
                Canvas.SetLeft(img_SpaceShip, Canvas.GetLeft(img_SpaceShip) + StarshipSpeed);
        }


        private void HandleBandits()
        {

            if (isBanditMovingRight && Canvas.GetLeft(GameCanvas.Children[7]) + Canvas.GetRight(GameCanvas.Children[7]) - Canvas.GetLeft(GameCanvas.Children[7]) <= GameCanvas.ActualWidth)
            {
                Canvas.SetLeft(GameCanvas.Children[7], Canvas.GetLeft(GameCanvas.Children[7]) + BanditSpeed);
                isBanditMovingLeft = false;
            }
            else
            {
                isBanditMovingLeft = true;
                isBanditMovingRight = false;
            }

            if (isBanditMovingLeft && Canvas.GetLeft(GameCanvas.Children[1]) > 0)
            {
                Canvas.SetLeft(GameCanvas.Children[1], Canvas.GetLeft(GameCanvas.Children[1]) - BanditSpeed);
                isBanditMovingRight = false;
            }
            else
            {
                isBanditMovingLeft = false;
                isBanditMovingRight = true;
            }
        }

        private void Shoot()
        {
            // Implement shooting logic here
            // Create bullets, handle their movement, and collision detection
        }

        private void AddBandits()
        {
            var startPosition_left = 0;

            for (int i = 0; i < 8; i++)
            {
                var bandit = new UIElement();

                var t = new Image();
                t.Name = $"img_GreenBandit_{i}";
                t.Width = 30;
                t.Height = 21;
                t.Visibility = Visibility.Visible;
                t.Source = new BitmapImage(new Uri(@"/Resources/Bandit_Green.png", UriKind.Relative));

                GameCanvas.Children.Add(t);
                Canvas.SetLeft(GameCanvas.Children[1 + i], Left = startPosition_left);
                Canvas.SetTop(GameCanvas.Children[1 + i], Top = 10);

                //Create next
                startPosition_left += 50;
            }
        }
    }
}
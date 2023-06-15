using SpaceInvaders.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private int LaserSpeed = 7;
        private List<Bandit> CurrentBanditList;
        private List<LaserBeam> CurrentLaserBeamList;


        public MainWindow()
        {
            InitializeComponent();

            //Create the game loop.
            CompositionTarget.Rendering += GameLoop;

            //Set application defaults.
            isBanditMovingRight = true;
            AddBandits();
            CurrentLaserBeamList= new List<LaserBeam>();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (GameCanvas.ActualWidth == 0)
                return;

            // Handle player movement
            HandleSpaceShip();

            // Handle Bandit movement
            HandleBandits();

            //Handle Laser movement
            HandleLaserBeams();

            //Handle hits
            HandleHits();

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

        //================================================
        //               Sprite handling
        //================================================

        private void HandleSpaceShip()
        {
            if (isMovingLeft && Canvas.GetLeft(img_SpaceShip) > 0)
            {
                Canvas.SetLeft(img_SpaceShip, Canvas.GetLeft(img_SpaceShip) - StarshipSpeed);
                return;
            }

            if (isMovingRight && Canvas.GetLeft(img_SpaceShip) + img_SpaceShip.Width < GameCanvas.ActualWidth)
            {
                Canvas.SetLeft(img_SpaceShip, Canvas.GetLeft(img_SpaceShip) + StarshipSpeed);
                return;
            }
        }

        private void HandleBandits()
        {
            //ToDo: To be replaced by Aggregate!!!
            var mostLeftBandit = CurrentBanditList.Where(x => x.IsAlive).OrderByDescending(x => x.CanvasLeftPos).Last();
            var mostRightBandit = CurrentBanditList.Where(x => x.IsAlive).OrderByDescending(x => x.CanvasLeftPos).Take(1).FirstOrDefault();

            //Moving bandits left
            if (isBanditMovingRight && Canvas.GetLeft(GameCanvas.Children[mostRightBandit.CanvasId]) + mostRightBandit.BanditImage.Width <= GameCanvas.ActualWidth)
            {
                foreach (var bandit in CurrentBanditList.Where(x => x.IsAlive))
                {
                    Canvas.SetLeft(GameCanvas.Children[bandit.CanvasId], Canvas.GetLeft(GameCanvas.Children[bandit.CanvasId]) + BanditSpeed);
                    bandit.CanvasLeftPos = Canvas.GetLeft(GameCanvas.Children[bandit.CanvasId]);
                }
                isBanditMovingLeft = false;
                return;
            }
            else
            {
                isBanditMovingLeft = true;
                isBanditMovingRight = false;
            }

            //Moving bandits right
            if (isBanditMovingLeft && Canvas.GetLeft(GameCanvas.Children[mostLeftBandit.CanvasId]) > 0)
            {
                foreach (var bandit in CurrentBanditList.Where(x => x.IsAlive))
                {
                    Canvas.SetLeft(GameCanvas.Children[bandit.CanvasId], Canvas.GetLeft(GameCanvas.Children[bandit.CanvasId]) - BanditSpeed);
                    bandit.CanvasLeftPos = Canvas.GetLeft(GameCanvas.Children[bandit.CanvasId]);
                }
                isBanditMovingRight = false;
            }
            else
            {
                isBanditMovingLeft = false;
                isBanditMovingRight = true;
            }
        }

        private void HandleLaserBeams()
        {
            //Move all laserbeams up
            foreach(var beam in CurrentLaserBeamList)
            {
                Canvas.SetBottom(GameCanvas.Children[beam.CanvasId], Canvas.GetBottom(GameCanvas.Children[beam.CanvasId]) + LaserSpeed);
                beam.CanvasBottomPos = Canvas.GetBottom(GameCanvas.Children[beam.CanvasId]);
            }

            //Remove any dead ones....
            CurrentLaserBeamList = CurrentLaserBeamList.Where(x => x.CanvasBottomPos < 2000).ToList();
        }

        private void HandleHits()
        {
            //var r = new Random();
            //var bandit = r.Next(1, 21);
   


            //(GameCanvas.Children[bandit] as Image).Visibility = Visibility.Collapsed;
            //CurrentBanditList.FirstOrDefault(x => x.CanvasId == bandit).IsAlive = false;
        }

        private void Shoot()
        {
            // Implement shooting logic here
            // Create bullets, handle their movement, and collision detection

            var beam = new LaserBeam
            {
                Name = $"LaserBeam",
                IsAlive = true,
                CanvasId = GameCanvas.Children.Count,
                CanvasLeftPos = Canvas.GetLeft(img_SpaceShip),
                CanvasBottomPos = 10,
                LaserImage = new Image
                {
                    Name = $"Laser",
                    Width = 25,
                    Height = 25,
                    Visibility = Visibility.Visible,
                    Source = new BitmapImage(new Uri(@"/Resources/LaserBeam.png", UriKind.Relative))
                }
            };

            var leftPos = beam.CanvasLeftPos + (beam.LaserImage.Width / 2 - 3);
            var bottomPos = 10;

            //Add laser to list if we do not have one in range....
            if (!CurrentLaserBeamList.Any(x => x.CanvasLeftPos >= leftPos - 5 && x.CanvasLeftPos <= leftPos + 5 && x.CanvasBottomPos <= 100))
            {
                //Update with positions for laserbeams
                beam.CanvasLeftPos = leftPos;
                beam.CanvasBottomPos = bottomPos;

                //Add beams to current list
                CurrentLaserBeamList.Add(beam);
                GameCanvas.Children.Add(beam.LaserImage);

                //Set positions in canvas
                Canvas.SetLeft(GameCanvas.Children[beam.CanvasId], leftPos);
                Canvas.SetBottom(GameCanvas.Children[beam.CanvasId], bottomPos);
            }
        }

        //================================================
        //               New Game Setting
        //================================================

        private void AddBandits()
        {
            var startPosition_left = 0;
            var startPosition_top = 10;
            var numberOfBanditsInRow = 10;
            var numberOfBanditRows = 2;

            CurrentBanditList = new List<Bandit>();

            //Build bandit rows
            for (int r = 0; r < numberOfBanditRows; r++)
            {
                //Place Bandits in each column
                for (int c = 0; c < numberOfBanditsInRow; c++)
                {
                    var bandit = new Bandit
                    {
                        ColumnIndex = c,
                        RowIndex = 0,
                        Name = $"Bandit_{r}_{c}",
                        IsAlive = true,
                        CanvasId = GameCanvas.Children.Count,
                        CanvasLeftPos = startPosition_left,
                        CanvasTopPos = startPosition_top,
                        BanditImage = new Image
                        {
                            Name = $"Bandit_{r}_{c}",
                            Width = 30,
                            Height = 21,
                            Visibility = Visibility.Visible,
                            Source = new BitmapImage(new Uri(@"/Resources/Bandit_Green.png", UriKind.Relative))
                        }
                    };

                    //Add bandit to list and 
                    CurrentBanditList.Add(bandit);
                    GameCanvas.Children.Add(bandit.BanditImage);

                    Canvas.SetLeft(GameCanvas.Children[bandit.CanvasId], Left = bandit.CanvasLeftPos);
                    Canvas.SetTop(GameCanvas.Children[bandit.CanvasId], Top = bandit.CanvasTopPos);

                    //Create next
                    startPosition_left += 50;
                }
                //Create next
                startPosition_left = 25;
                startPosition_top += 30;
            }
        }
    }
}
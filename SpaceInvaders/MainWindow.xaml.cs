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
        private int LaserSpeed = 3;
        private int BanditWidth = 30;
        private int BanditHeight = 21;
        private int LaserWidth = 25;
        private int LaserHeight = 25;
        private int GameScore = 0;
        private List<Bandit> CurrentBanditList;
        private List<LaserBeam> CurrentLaserBeamList;


        public MainWindow()
        {
            InitializeComponent();

            //Create the game loop.
            CompositionTarget.Rendering += GameLoop;

            //Set application defaults.
            isBanditMovingRight = true;

            //Add Bandits
            AddBandits();

            //Create Beam list
            CurrentLaserBeamList= new List<LaserBeam>();

            //Set score at start
            lbl_Score.Content = GameScore;

        }

        private void GameLoop(object sender, EventArgs e)
        {
            //First loop this is 0 until we have a forms open.
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

            //Handle New Bandits?
            StartNewGame();
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
            var mostLeftBandit = CurrentBanditList.OrderByDescending(x => x.CanvasLeftPos).Last();
            var mostRightBandit = CurrentBanditList.OrderByDescending(x => x.CanvasLeftPos).Take(1).First();

            //Moving bandits right
            if (isBanditMovingRight && Canvas.GetLeft(GetCanvasChildren(mostRightBandit.BanditImage)) + mostRightBandit.BanditImage.Width <= GameCanvas.ActualWidth)
            {
                foreach (var bandit in CurrentBanditList)
                {
                    var child = GetCanvasChildren(bandit.BanditImage);
                    Canvas.SetLeft(child, Canvas.GetLeft(child) + BanditSpeed);
                    bandit.CanvasLeftPos = Canvas.GetLeft(child);
                }
                isBanditMovingLeft = false;
                return;
            }
            else
            {
                if (isBanditMovingRight)
                {
                    foreach (var bandit in CurrentBanditList)
                    {
                        var child = GetCanvasChildren(bandit.BanditImage);
                        Canvas.SetTop(child, Canvas.GetTop(child) + 10);
                        bandit.CanvasTopPos = Canvas.GetTop(child);
                    }
                }

                //We reached right border. move them down
                isBanditMovingLeft = true;
                isBanditMovingRight = false;
            }

            //Moving bandits right
            if (isBanditMovingLeft && Canvas.GetLeft(GetCanvasChildren(mostLeftBandit.BanditImage)) > 0)
            {
                foreach (var bandit in CurrentBanditList/*.Where(x => x.IsAlive)*/)
                {
                    var child = GetCanvasChildren(bandit.BanditImage);
                    Canvas.SetLeft(child, Canvas.GetLeft(child) - BanditSpeed);
                    bandit.CanvasLeftPos = Canvas.GetLeft(child);
                }
                isBanditMovingRight = false;
            }
            else
            {
                if (isBanditMovingLeft)
                {
                    foreach (var bandit in CurrentBanditList)
                    {
                        var child = GetCanvasChildren(bandit.BanditImage);
                        Canvas.SetTop(child, Canvas.GetTop(child) + 10);
                        bandit.CanvasTopPos = Canvas.GetTop(child);
                    }
                }

                isBanditMovingLeft = false;
                isBanditMovingRight = true;
            }
        }

        private void HandleLaserBeams()
        {
            //Move all laserbeams up
            foreach(var beam in CurrentLaserBeamList)
            {
                Canvas.SetBottom(GetCanvasChildren(beam.LaserImage), Canvas.GetBottom(GetCanvasChildren(beam.LaserImage)) + LaserSpeed);
                beam.CanvasBottomPos = Canvas.GetBottom(GetCanvasChildren(beam.LaserImage));
            }

            //Remove any dead ones from list....
            var beamsToRemove = CurrentLaserBeamList.Where(x => x.CanvasBottomPos >= 2000).ToList();

            foreach (var beam in beamsToRemove)
            {
                GameCanvas.Children.Remove(GetCanvasChildren(beam.LaserImage));
                CurrentLaserBeamList.Remove(beam);
            }
            //CurrentLaserBeamList = CurrentLaserBeamList.Where(x => x.CanvasBottomPos < 2000).ToList();
        }

        private void HandleHits()
        {

            foreach(var beam in CurrentLaserBeamList)
            {
                //Logic for checking each bandit towards the beam..
                var beamMidPos = beam.CanvasLeftPos + LaserWidth / 2;
                var beamTopPos = beam.CanvasBottomPos + LaserHeight - GameCanvas.ActualHeight;

                //Get any bandit thats been hit
                var bandit = CurrentBanditList.Where(x => x.CanvasLeftPos <= beamMidPos && x.CanvasLeftPos + BanditWidth >= beamMidPos && x.CanvasTopPos - BanditHeight <= beamTopPos && x.CanvasTopPos >= beamTopPos).FirstOrDefault();

                if (bandit != null)
                {
                    //Remove bandit
                    GameCanvas.Children.Remove(GetCanvasChildren(bandit.BanditImage));
                    CurrentBanditList.RemoveAll(x => x.Uid == bandit.Uid);

                    //Update score.
                    GameScore += 1;
                    lbl_Score.Content = GameScore;
                }
            }


        }

        private void Shoot()
        {
            // Implement shooting logic here
            // Create bullets, handle their movement, and collision detection
            var uuid = Guid.NewGuid();
            var beam = new LaserBeam
            {
                Name = $"LaserBeam",
                IsAlive = true,
                Uid = uuid,
                CanvasLeftPos = Canvas.GetLeft(img_SpaceShip) + img_SpaceShip.Width / 2,
                CanvasBottomPos = 10,
                LaserImage = new Image
                {
                    Name = $"Laser",
                    Uid = uuid.ToString(),
                    Width = LaserWidth,
                    Height = LaserHeight,
                    Visibility = Visibility.Visible,
                    Source = new BitmapImage(new Uri(@"/Resources/LaserBeam.png", UriKind.Relative))
                }
            };

            var leftPos = beam.CanvasLeftPos - (beam.LaserImage.Width / 2);
            var bottomPos = 10;

            //Add laser to list if we do not have one in range....
            if (!CurrentLaserBeamList.Any(x => x.CanvasLeftPos >= leftPos - 20 && x.CanvasLeftPos <= leftPos + 20 && x.CanvasBottomPos <= 100))
            {
                //Update with positions for laserbeams
                beam.CanvasLeftPos = leftPos;
                beam.CanvasBottomPos = bottomPos;

                //Add beams to current list
                CurrentLaserBeamList.Add(beam);
                GameCanvas.Children.Add(beam.LaserImage);

                //Set positions in canvas
                Canvas.SetLeft(GetCanvasChildren(beam.LaserImage), leftPos);
                Canvas.SetBottom(GetCanvasChildren(beam.LaserImage), bottomPos);
            }
        }

        private void StartNewGame()
        {
            if (CurrentBanditList.Count() == 0)
            {
                //Add new bandits
                AddBandits();
            }
        }

        //================================================
        //                  Functions
        //================================================

        private FrameworkElement GetCanvasChildren(Image image)
        {
            var child = (from c in GameCanvas.Children.Cast<FrameworkElement>()
                         where image.Uid.Equals(c.Uid)
                         select c).First();

            return child;
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
                    var uuid = Guid.NewGuid();
                    var bandit = new Bandit
                    {
                        ColumnIndex = c,
                        RowIndex = 0,
                        Name = $"Bandit_{r}_{c}",
                        Uid = uuid,
                        CanvasLeftPos = startPosition_left,
                        CanvasTopPos = startPosition_top,
                        BanditImage = new Image
                        {
                            Name = "Bandit",
                            Uid = uuid.ToString(),
                            Width = BanditWidth,
                            Height = BanditHeight,
                            Visibility = Visibility.Visible,
                            Source = new BitmapImage(new Uri(@"/Resources/Bandit_Green.png", UriKind.Relative))
                        }
                    };

                    //Add bandit to list and 
                    CurrentBanditList.Add(bandit);
                    GameCanvas.Children.Add(bandit.BanditImage);

                    Canvas.SetLeft(GetCanvasChildren(bandit.BanditImage), bandit.CanvasLeftPos);
                    Canvas.SetTop(GetCanvasChildren(bandit.BanditImage), bandit.CanvasTopPos);

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
using SpaceInvaders.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private double BanditSpeed = 1;
        private int LaserSpeed = 2;
        private int BanditWidth = 30;
        private int BanditHeight = 21;
        private int LaserWidth = 25;
        private int LaserHeight = 25;
        private int GameScore = 0;
        private List<Bandit> CurrentBanditList;
        private List<LaserBeam> CurrentLaserBeamList;


        public MainWindow()
        {
            var timer = new Stopwatch();
            timer.Start();

            var Welcome = new SpaceInvaders.Forms.WelcomeScreen();
            Welcome.Show();
            Welcome.Closed += StartNewGame;

            InitializeComponent();
            this.Visibility = Visibility.Hidden;
        }

        private void StartNewGame(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;

            //Set application defaults.
            isBanditMovingRight = true;

            //Create the game loop.
            CompositionTarget.Rendering += GameLoop;

            //Create list
            CurrentLaserBeamList = new List<LaserBeam>();
            CurrentBanditList = new List<Bandit>();

            //Set score at start
            lbl_Score.Content = GameScore;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            //First loop this is 0 until we have a forms open.
            if (GameCanvas.ActualWidth == 0 || GameCanvas.ActualHeight == 0)
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
            CreateNewBandits();

            //Handle Game Over
            HandleGameOver();
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
            try
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
                            Canvas.SetBottom(child, Canvas.GetBottom(child) - 30);
                            bandit.CanvasBottomPos = Canvas.GetBottom(child);
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
                            Canvas.SetBottom(child, Canvas.GetBottom(child) - 30);
                            bandit.CanvasBottomPos = Canvas.GetBottom(child);
                        }
                    }

                    isBanditMovingLeft = false;
                    isBanditMovingRight = true;
                }
            }
            catch { }
        }

        private void HandleLaserBeams()
        {
            //Move all laserbeams up
            foreach(var beam in CurrentLaserBeamList)
            {
                Canvas.SetBottom(GetCanvasChildren(beam.LaserImage), Canvas.GetBottom(GetCanvasChildren(beam.LaserImage)) + LaserSpeed);
                beam.CanvasBottomPos = Canvas.GetBottom(GetCanvasChildren(beam.LaserImage));
            }

            if (CurrentLaserBeamList.Count > 0)
            {
                //Remove any dead ones from list....
                var beamsToRemove = CurrentLaserBeamList.Where(x => x.CanvasBottomPos >= 1500).ToList();

                foreach (var beam in beamsToRemove)
                {
                    GameCanvas.Children.Remove(GetCanvasChildren(beam.LaserImage));
                    CurrentLaserBeamList.Remove(beam);
                }
            }
        }

        private void HandleHits()
        {
            var beamsThatHit = new List<LaserBeam>();
            foreach (var beam in CurrentLaserBeamList)
            {
                //Logic for checking each bandit towards the beam..
                var beamMidPos = beam.CanvasLeftPos + LaserWidth / 2;
                var beamTopPos = beam.CanvasBottomPos + LaserHeight;

                //Get any bandit thats been hit
                var bandit = CurrentBanditList.Where(x => x.CanvasLeftPos <= beamMidPos
                                                     && x.CanvasLeftPos + BanditWidth >= beamMidPos
                                                     && x.CanvasBottomPos <= beamTopPos
                                                     && x.CanvasBottomPos + BanditHeight >= beamTopPos).FirstOrDefault();

                if (bandit != null)
                {
                    //Remove bandit
                    GameCanvas.Children.Remove(GetCanvasChildren(bandit.BanditImage));
                    CurrentBanditList.RemoveAll(x => x.Uid == bandit.Uid);
                    GameCanvas.UpdateLayout();

                    //Add beam to be removed!
                    beamsThatHit.Add(beam);

                    //Update score.
                    GameScore += 1;
                    lbl_Score.Content = GameScore;
                }
            }

            //Also remove beam!
            foreach (var b in beamsThatHit)
            { 
                GameCanvas.Children.Remove(GetCanvasChildren(b.LaserImage));
                CurrentLaserBeamList.Remove(b);
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
            if (!CurrentLaserBeamList.Any(x => x.CanvasBottomPos <= 70))
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

            //isShooting = false;
        }
             
        private void CreateNewBandits()
        {
            if (CurrentBanditList.Count() == 0 && GameCanvas.ActualHeight != 0)
            {
                //Add new bandits
                AddBandits();

                //Increase speed on bandits
                BanditSpeed += 0.2;
            }
        }

        private void HandleGameOver()
        {

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
            //var startPosition_top = 10;
            var startPosition_bottom = GameCanvas.ActualHeight - BanditHeight - 10;
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
                        //CanvasTopPos = startPosition_top,
                        CanvasBottomPos = startPosition_bottom,

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
                    //Canvas.SetTop(GetCanvasChildren(bandit.BanditImage), bandit.CanvasTopPos);
                    Canvas.SetBottom(GetCanvasChildren(bandit.BanditImage), bandit.CanvasBottomPos);

                    //Create next
                    startPosition_left += 50;
                }
                //Create next
                startPosition_left = 25;
                //startPosition_top += 30;
                startPosition_bottom -= 30;
            }
        }
    }
}
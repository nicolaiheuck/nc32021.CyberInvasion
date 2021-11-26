using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CyberInvasion.OwnVersion
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private double m_currentFrameTime;
        private ImageBrush backgroundSprite = new ImageBrush();
        private ImageBrush nc3Sprite = new ImageBrush();
        private ImageBrush logoSprite = new ImageBrush();
        private ImageBrush playerSprite = new ImageBrush();
        private ImageBrush bulletSprite = new ImageBrush();
        private ImageBrush[] m_enemySprites = new ImageBrush[4];
        private List<Enemy> m_enemies = new List<Enemy>();
        private bool m_inGame;
        private bool m_bGameEnded;
        private double m_nc3LogoY;
        private double m_gameLogoMovement;
        private double m_scoreHoverSpeedX;
        private double m_scoreHoverSpeedY;
        private double m_moveSpeed;
        private double m_playerHoverSpeedX;
        private double m_playerHoverSpeedY;
        private bool m_playerDirection;
        private double m_enemySpeed = 1.0;
        private string m_serverIP = "";
        private static readonly HttpClient m_client = new HttpClient();
        private int m_currentSequenceNumber;
        private const double m_enemySize = 140.0;
        //internal Canvas GameCanvas;
        //internal Rectangle background1;
        //internal Rectangle gameLogo;
        //internal Rectangle nc3logo;
        //internal Rectangle nc3SpriteObject_player;
        //internal Rectangle nc3SpriteObject_bullet;
        //internal Rectangle nc3SpriteObject_enemy1;
        //internal Rectangle nc3SpriteObject_enemy2;
        //internal Rectangle nc3SpriteObject_enemy3;
        //internal Rectangle nc3SpriteObject_enemy4;
        //internal TextBlock CurrentScoreText;
        //private bool _contentLoaded;

        public MainWindow(string serverIp)
        {
            this.InitializeComponent();
            this.GameCanvas.Focus();
            this.backgroundSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/bg.png"));
            this.background1.Fill = (Brush)this.backgroundSprite;
            this.nc3Sprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/nc3.png"));
            this.nc3logo.Fill = (Brush)this.nc3Sprite;
            this.logoSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/logo.png"));
            this.gameLogo.Fill = (Brush)this.logoSprite;
            this.gameLogo.Width *= 1.5;
            this.gameLogo.Height *= 1.5;
            this.UpdateGameLogoPosition();
            this.playerSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_player.png"));
            this.nc3SpriteObject_player.Fill = (Brush)this.playerSprite;
            this.nc3SpriteObject_player.Width *= 0.5;
            this.nc3SpriteObject_player.Height *= 0.5;
            this.bulletSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_bullet.png"));
            this.nc3SpriteObject_bullet.Fill = (Brush)this.bulletSprite;
            this.nc3SpriteObject_bullet.Width *= 0.5;
            this.nc3SpriteObject_bullet.Height *= 0.5;
            Canvas.SetTop((UIElement)this.nc3SpriteObject_player, Canvas.GetTop((UIElement)this.nc3SpriteObject_player) + 50.0);
            this.InitNetwork(serverIp);
            this.gameTimer.Tick += new EventHandler(this.OnGameFrame);
            this.gameTimer.Interval = TimeSpan.FromMilliseconds(20.0);
            this.gameTimer.Start();
        }

        private void UpdateGameLogoPosition()
        {
            Canvas.SetTop((UIElement)this.gameLogo, this.background1.Height * 0.5 - this.gameLogo.Height * 0.5);
            Canvas.SetLeft((UIElement)this.gameLogo, this.background1.Width * 0.5 - this.gameLogo.Width * 0.5);
        }

        private void OnGameFrame(object sender, EventArgs e)
        {
            this.m_currentFrameTime += 0.100000001490116;
            this.m_nc3LogoY = Math.Sin(this.m_currentFrameTime) * 1.0;
            Canvas.SetTop((UIElement)this.nc3logo, Canvas.GetTop((UIElement)this.nc3logo) + this.m_nc3LogoY);
            this.m_gameLogoMovement = Math.Cos(this.m_currentFrameTime * 0.9);
            this.gameLogo.Width += this.m_gameLogoMovement;
            this.UpdateGameLogoPosition();
            if (!this.m_inGame || this.m_bGameEnded)
                return;
            this.OnGameFrame();
        }

        private void InitNetwork(string serverIp)
        {
            this.m_serverIP = serverIp;
            this.m_serverIP = "http://" + this.m_serverIP + ":3000/";
            if (this.GetServerReply("clear_current_score") != "OK")
            {
                int num = (int)MessageBox.Show("Server fejl: Er du sikker på at den server IP er rigtig?", "CyberInvasion 2o21 til #nc3ctf2021");
                Environment.Exit(1);
            }
            this.m_currentSequenceNumber = 0;
        }

        private string GetServerReply(string param)
        {
            string str = this.m_serverIP + param;
            try
            {
                HttpResponseMessage result = new HttpClient()
                {
                    BaseAddress = new Uri(str),
                    DefaultRequestHeaders =
                    {
                        Accept =
                        {
                            new MediaTypeWithQualityHeaderValue("application/json")
                        }
                    }
                }.GetAsync(str).Result;
                string empty = string.Empty;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
            }
            return "";
        }

        private string IncreaseScoreOnNetwork()
        {
            int num = (int)this.m_enemySpeed;
            if (num > 1000)
                num = 1000;

            num = 1000;

            ++this.m_currentSequenceNumber;
            byte[] bytes = Encoding.UTF8.GetBytes(this.m_currentSequenceNumber.ToString() + ":" + (object)num);

            for (int index = 0; index < bytes.Length; ++index)
                bytes[index] = (byte)(bytes[index] ^ 3U);
            return GetServerReply("increase_score/" + Convert.ToBase64String(bytes));
        }

        private void StartGame()
        {
            this.gameLogo.Visibility = Visibility.Hidden;
            this.m_enemySpeed = 0.0;
            this.m_bGameEnded = false;
            this.CurrentScoreText.Text = "Highscore: " + this.GetServerReply("highscore");
            this.StartLevel();
            this.m_inGame = true;
        }

        private void StartLevel()
        {
            this.CreateEnemies();
            ++this.m_enemySpeed;
        }

        private void CreateEnemies()
        {
            this.m_enemies.Clear();
            this.m_enemySprites[0] = new ImageBrush();
            this.m_enemySprites[1] = new ImageBrush();
            this.m_enemySprites[2] = new ImageBrush();
            this.m_enemySprites[3] = new ImageBrush();
            this.m_enemySprites[0].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy1.png"));
            this.m_enemySprites[1].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy2.png"));
            this.m_enemySprites[2].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy3.png"));
            this.m_enemySprites[3].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy4.png"));
            for (int index1 = 0; index1 < 7; ++index1)
            {
                for (int index2 = 0; index2 < 1; ++index2)
                {
                    int index3 = index1 % this.m_enemySprites.Length;
                    double num = this.m_enemySprites[index3].ImageSource.Width * 0.5;
                    Rectangle rectangle1 = new Rectangle();
                    rectangle1.Tag = (object)"enemy";
                    rectangle1.Fill = (Brush)this.m_enemySprites[index3];
                    rectangle1.Width = num;
                    rectangle1.Height = this.m_enemySprites[index3].ImageSource.Height * 0.5;
                    Rectangle rectangle2 = rectangle1;
                    Canvas.SetTop((UIElement)rectangle2, Canvas.GetTop((UIElement)this.gameLogo) - 20.0 + (double)(index2 * 140));
                    Canvas.SetLeft((UIElement)rectangle2, Canvas.GetLeft((UIElement)this.gameLogo) + (double)(index1 * 140));
                    this.GameCanvas.Children.Add((UIElement)rectangle2);
                    this.m_enemies.Add(new Enemy()
                    {
                        rect = rectangle2
                    });
                }
            }
            GC.Collect();
        }

        private void OnEndGame()
        {
            this.CurrentScoreText.Text = "-- G@ME 0V3R --";
            this.m_bGameEnded = true;
        }

        private void AddScore() => this.CurrentScoreText.Text = this.IncreaseScoreOnNetwork();

        private void OnGameFrame()
        {
            this.m_scoreHoverSpeedX = Math.Sin(this.m_currentFrameTime + 100.0) * 0.1;
            this.m_scoreHoverSpeedY = Math.Sin(this.m_currentFrameTime + 200.0) * 0.1;
            Canvas.SetTop((UIElement)this.CurrentScoreText, Canvas.GetTop((UIElement)this.CurrentScoreText) + this.m_scoreHoverSpeedY);
            Canvas.SetLeft((UIElement)this.CurrentScoreText, Canvas.GetLeft((UIElement)this.CurrentScoreText) + this.m_scoreHoverSpeedX);
            if (this.nc3SpriteObject_bullet.Visibility == Visibility.Visible)
            {
                Canvas.SetTop((UIElement)this.nc3SpriteObject_bullet, Canvas.GetTop((UIElement)this.nc3SpriteObject_bullet) - 20.0);
                if (Canvas.GetTop((UIElement)this.nc3SpriteObject_bullet) < -this.nc3SpriteObject_bullet.Height)
                    this.nc3SpriteObject_bullet.Visibility = Visibility.Hidden;
            }
            Rect rect1 = new Rect(Canvas.GetLeft((UIElement)this.nc3SpriteObject_bullet), Canvas.GetTop((UIElement)this.nc3SpriteObject_bullet), this.nc3SpriteObject_bullet.Width, this.nc3SpriteObject_bullet.Height);
            for (int index = 0; index < this.m_enemies.Count; ++index)
            {
                Enemy enemy = this.m_enemies[index];
                Rect rect2 = new Rect(Canvas.GetLeft((UIElement)enemy.rect), Canvas.GetTop((UIElement)enemy.rect), enemy.rect.Width, enemy.rect.Height);
                rect2.Inflate(-10.0, -10.0);
                if (this.nc3SpriteObject_bullet.Visibility == Visibility.Visible && rect1.IntersectsWith(rect2))
                {
                    enemy.rect.Visibility = Visibility.Hidden;
                    this.m_enemies.Remove(enemy);
                    this.nc3SpriteObject_bullet.Visibility = Visibility.Hidden;
                    this.AddScore();
                }
                double left = Canvas.GetLeft((UIElement)enemy.rect);
                double top = Canvas.GetTop((UIElement)enemy.rect);
                if (left < 10.0 || left > this.GameCanvas.ActualWidth - 140.0 - 10.0)
                {
                    top += 140.0;
                    enemy.moveDir = !enemy.moveDir;
                }
                double num = -this.m_enemySpeed;
                if (enemy.moveDir)
                    num = this.m_enemySpeed;
                if (top > this.GameCanvas.ActualHeight - 280.0)
                    this.OnEndGame();
                Canvas.SetTop((UIElement)enemy.rect, top);
                Canvas.SetLeft((UIElement)enemy.rect, left + num);
            }
            if (this.m_enemies.Count == 0)
                this.StartLevel();
            this.m_playerHoverSpeedX = Math.Sin(this.m_currentFrameTime * 1.5) * 0.2;
            this.m_playerHoverSpeedY = Math.Sin(this.m_currentFrameTime) * 0.2;
            double left1 = Canvas.GetLeft((UIElement)this.nc3SpriteObject_player);
            Canvas.SetTop((UIElement)this.nc3SpriteObject_player, Canvas.GetTop((UIElement)this.nc3SpriteObject_player) + this.m_playerHoverSpeedY);
            Canvas.SetLeft((UIElement)this.nc3SpriteObject_player, left1 + this.m_playerHoverSpeedX);
            if (left1 < 0.0 || left1 > this.background1.Width - this.nc3SpriteObject_player.Width)
                this.m_playerDirection = !this.m_playerDirection;
            this.m_moveSpeed = Math.Min(this.m_moveSpeed, 4.0);
            this.m_moveSpeed = Math.Max(this.m_moveSpeed, 0.0);
            if (this.m_playerDirection)
                Canvas.SetLeft((UIElement)this.nc3SpriteObject_player, Canvas.GetLeft((UIElement)this.nc3SpriteObject_player) - this.m_moveSpeed);
            else
                Canvas.SetLeft((UIElement)this.nc3SpriteObject_player, Canvas.GetLeft((UIElement)this.nc3SpriteObject_player) + this.m_moveSpeed);
        }

        private void OnGameKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                this.m_moveSpeed = 4.0;
                this.m_playerDirection = true;
            }
            else if (e.Key == Key.Right)
            {
                this.m_moveSpeed = 4.0;
                this.m_playerDirection = false;
            }
            if (e.Key != Key.Space || this.nc3SpriteObject_bullet.Visibility != Visibility.Hidden)
                return;
            Canvas.SetLeft((UIElement)this.nc3SpriteObject_bullet, Canvas.GetLeft((UIElement)this.nc3SpriteObject_player) + this.nc3SpriteObject_player.Width * 0.5);
            Canvas.SetTop((UIElement)this.nc3SpriteObject_bullet, Canvas.GetTop((UIElement)this.nc3SpriteObject_player));
            this.nc3SpriteObject_bullet.Visibility = Visibility.Visible;
        }

        private void GameCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.m_bGameEnded)
                return;
            if (this.m_inGame)
            {
                if (e.Key == Key.A)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        AddScore();
                    }
                }
                if (e.Key == Key.B)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        AddScore();
                    }
                }
                if (e.Key == Key.C)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        AddScore();
                    }
                }
                this.OnGameKeyDown(e);
            }
            else
            {
                if (e.Key != Key.Space)
                    return;
                this.StartGame();
            }
        }

        private void GameCanvas_KeyUp(object sender, KeyEventArgs e)
        {
        }

        //public void InitializeComponent()
        //{
        //    if (this._contentLoaded)
        //        return;
        //    this._contentLoaded = true;
        //    Application.LoadComponent((object)this, new Uri("/CyberInvasion;component/mainwindow.xaml", UriKind.Relative));
        //}

        void Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.GameCanvas = (Canvas)target;
                    this.GameCanvas.KeyDown += new KeyEventHandler(this.GameCanvas_KeyDown);
                    this.GameCanvas.KeyUp += new KeyEventHandler(this.GameCanvas_KeyUp);
                    break;
                case 2:
                    this.background1 = (Rectangle)target;
                    break;
                case 3:
                    this.gameLogo = (Rectangle)target;
                    break;
                case 4:
                    this.nc3logo = (Rectangle)target;
                    break;
                case 5:
                    this.nc3SpriteObject_player = (Rectangle)target;
                    break;
                case 6:
                    this.nc3SpriteObject_bullet = (Rectangle)target;
                    break;
                case 7:
                    this.nc3SpriteObject_enemy1 = (Rectangle)target;
                    break;
                case 8:
                    this.nc3SpriteObject_enemy2 = (Rectangle)target;
                    break;
                case 9:
                    this.nc3SpriteObject_enemy3 = (Rectangle)target;
                    break;
                case 10:
                    this.nc3SpriteObject_enemy4 = (Rectangle)target;
                    break;
                case 11:
                    this.CurrentScoreText = (TextBlock)target;
                    break;
                default:
                    this._contentLoaded = true;
                    break;
            }
        }
    }
    #region Old
    //public partial class MainWindowOld : Window, IComponentConnector
    //{
    //    private DispatcherTimer gameTimer = new DispatcherTimer();
    //    private double m_currentFrameTime;
    //    private ImageBrush backgroundSprite = new ImageBrush();
    //    private ImageBrush nc3Sprite = new ImageBrush();
    //    private ImageBrush logoSprite = new ImageBrush();
    //    private ImageBrush playerSprite = new ImageBrush();
    //    private ImageBrush bulletSprite = new ImageBrush();
    //    private ImageBrush[] m_enemySprites = new ImageBrush[4];
    //    private List<Enemy> m_enemies = new List<Enemy>();
    //    private bool m_inGame;
    //    private bool m_bGameEnded;
    //    private double m_nc3LogoY;
    //    private double m_gameLogoMovement;
    //    private double m_scoreHoverSpeedX;
    //    private double m_scoreHoverSpeedY;
    //    private double m_moveSpeed;
    //    private double m_playerHoverSpeedX;
    //    private double m_playerHoverSpeedY;
    //    private bool m_playerDirection;
    //    private double m_enemySpeed = 1.0;
    //    private string m_serverIP = "";
    //    private static readonly HttpClient m_client = new HttpClient();
    //    private int m_currentSequenceNumber;
    //    private const double m_enemySize = 140.0;
    //    internal Canvas GameCanvas;
    //    internal Rectangle background1;
    //    internal Rectangle gameLogo;
    //    internal Rectangle nc3logo;
    //    internal Rectangle nc3SpriteObject_player;
    //    internal Rectangle nc3SpriteObject_bullet;
    //    internal Rectangle nc3SpriteObject_enemy1;
    //    internal Rectangle nc3SpriteObject_enemy2;
    //    internal Rectangle nc3SpriteObject_enemy3;
    //    internal Rectangle nc3SpriteObject_enemy4;
    //    internal TextBlock CurrentScoreText;
    //    private bool _contentLoaded;

    //    public MainWindowOld(string serverIp)
    //    {
    //        this.InitializeComponent();
    //        this.GameCanvas.Focus();
    //        this.backgroundSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/bg.png"));
    //        this.background1.Fill = (Brush)this.backgroundSprite;
    //        this.nc3Sprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/nc3.png"));
    //        this.nc3logo.Fill = (Brush)this.nc3Sprite;
    //        this.logoSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/logo.png"));
    //        this.gameLogo.Fill = (Brush)this.logoSprite;
    //        this.gameLogo.Width *= 1.5;
    //        this.gameLogo.Height *= 1.5;
    //        this.UpdateGameLogoPosition();
    //        this.playerSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_player.png"));
    //        this.nc3SpriteObject_player.Fill = (Brush)this.playerSprite;
    //        this.nc3SpriteObject_player.Width *= 0.5;
    //        this.nc3SpriteObject_player.Height *= 0.5;
    //        this.bulletSprite.ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_bullet.png"));
    //        this.nc3SpriteObject_bullet.Fill = (Brush)this.bulletSprite;
    //        this.nc3SpriteObject_bullet.Width *= 0.5;
    //        this.nc3SpriteObject_bullet.Height *= 0.5;
    //        Canvas.SetTop((UIElement)this.nc3SpriteObject_player, Canvas.GetTop((UIElement)this.nc3SpriteObject_player) + 50.0);
    //        this.InitNetwork(serverIp);
    //        this.gameTimer.Tick += new EventHandler(this.OnGameFrame);
    //        this.gameTimer.Interval = TimeSpan.FromMilliseconds(20.0);
    //        this.gameTimer.Start();
    //    }

    //    private void UpdateGameLogoPosition()
    //    {
    //        Canvas.SetTop((UIElement)this.gameLogo, this.background1.Height * 0.5 - this.gameLogo.Height * 0.5);
    //        Canvas.SetLeft((UIElement)this.gameLogo, this.background1.Width * 0.5 - this.gameLogo.Width * 0.5);
    //    }

    //    private void OnGameFrame(object sender, EventArgs e)
    //    {
    //        this.m_currentFrameTime += 0.100000001490116;
    //        this.m_nc3LogoY = Math.Sin(this.m_currentFrameTime) * 1.0;
    //        Canvas.SetTop((UIElement)this.nc3logo, Canvas.GetTop((UIElement)this.nc3logo) + this.m_nc3LogoY);
    //        this.m_gameLogoMovement = Math.Cos(this.m_currentFrameTime * 0.9);
    //        this.gameLogo.Width += this.m_gameLogoMovement;
    //        this.UpdateGameLogoPosition();
    //        if (!this.m_inGame || this.m_bGameEnded)
    //            return;
    //        this.OnGameFrame();
    //    }

    //    private void InitNetwork(string serverIp)
    //    {
    //        this.m_serverIP = serverIp;
    //        this.m_serverIP = "http://" + this.m_serverIP + ":3000/";
    //        if (this.GetServerReply("clear_current_score") != "OK")
    //        {
    //            int num = (int)MessageBox.Show("Server fejl: Er du sikker på at den server IP er rigtig?", "CyberInvasion 2o21 til #nc3ctf2021");
    //            Environment.Exit(1);
    //        }
    //        this.m_currentSequenceNumber = 0;
    //    }

    //    private string GetServerReply(string param)
    //    {
    //        string str = this.m_serverIP + param;
    //        try
    //        {
    //            HttpResponseMessage result = new HttpClient()
    //            {
    //                BaseAddress = new Uri(str),
    //                DefaultRequestHeaders = {
    //        Accept = {
    //          new MediaTypeWithQualityHeaderValue("application/json")
    //        }
    //      }
    //            }.GetAsync(str).Result;
    //            string empty = string.Empty;
    //            if (result.IsSuccessStatusCode)
    //                return result.Content.ReadAsStringAsync().Result;
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //        return "";
    //    }

    //    private string IncreaseScoreOnNetwork()
    //    {
    //        int num = (int)this.m_enemySpeed;
    //        if (num > 1000)
    //            num = 1000;
    //        ++this.m_currentSequenceNumber;
    //        byte[] bytes = Encoding.UTF8.GetBytes(this.m_currentSequenceNumber.ToString() + ":" + (object)num);
    //        for (int index = 0; index < bytes.Length; ++index)
    //            bytes[index] = (byte)((uint)bytes[index] ^ 3U);
    //        return this.GetServerReply("increase_score/" + Convert.ToBase64String(bytes));
    //    }

    //    private void StartGame()
    //    {
    //        this.gameLogo.Visibility = Visibility.Hidden;
    //        this.m_enemySpeed = 0.0;
    //        this.m_bGameEnded = false;
    //        this.CurrentScoreText.Text = "Highscore: " + this.GetServerReply("highscore");
    //        this.StartLevel();
    //        this.m_inGame = true;
    //    }

    //    private void StartLevel()
    //    {
    //        this.CreateEnemies();
    //        ++this.m_enemySpeed;
    //    }

    //    private void CreateEnemies()
    //    {
    //        this.m_enemies.Clear();
    //        this.m_enemySprites[0] = new ImageBrush();
    //        this.m_enemySprites[1] = new ImageBrush();
    //        this.m_enemySprites[2] = new ImageBrush();
    //        this.m_enemySprites[3] = new ImageBrush();
    //        this.m_enemySprites[0].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy1.png"));
    //        this.m_enemySprites[1].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy2.png"));
    //        this.m_enemySprites[2].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy3.png"));
    //        this.m_enemySprites[3].ImageSource = (ImageSource)new BitmapImage(new Uri("pack://application:,,,/_resources/sprite_enemy4.png"));
    //        for (int index1 = 0; index1 < 7; ++index1)
    //        {
    //            for (int index2 = 0; index2 < 1; ++index2)
    //            {
    //                int index3 = index1 % this.m_enemySprites.Length;
    //                double num = this.m_enemySprites[index3].ImageSource.Width * 0.5;
    //                Rectangle rectangle1 = new Rectangle();
    //                rectangle1.Tag = (object)"enemy";
    //                rectangle1.Fill = (Brush)this.m_enemySprites[index3];
    //                rectangle1.Width = num;
    //                rectangle1.Height = this.m_enemySprites[index3].ImageSource.Height * 0.5;
    //                Rectangle rectangle2 = rectangle1;
    //                Canvas.SetTop((UIElement)rectangle2, Canvas.GetTop((UIElement)this.gameLogo) - 20.0 + (double)(index2 * 140));
    //                Canvas.SetLeft((UIElement)rectangle2, Canvas.GetLeft((UIElement)this.gameLogo) + (double)(index1 * 140));
    //                this.GameCanvas.Children.Add((UIElement)rectangle2);
    //                this.m_enemies.Add(new Enemy()
    //                {
    //                    rect = rectangle2
    //                });
    //            }
    //        }
    //        GC.Collect();
    //    }

    //    private void OnEndGame()
    //    {
    //        this.CurrentScoreText.Text = "-- G@ME 0V3R --";
    //        this.m_bGameEnded = true;
    //    }

    //    private void AddScore() => this.CurrentScoreText.Text = this.IncreaseScoreOnNetwork();

    //    private void OnGameFrame()
    //    {
    //        this.m_scoreHoverSpeedX = Math.Sin(this.m_currentFrameTime + 100.0) * 0.1;
    //        this.m_scoreHoverSpeedY = Math.Sin(this.m_currentFrameTime + 200.0) * 0.1;
    //        Canvas.SetTop((UIElement)this.CurrentScoreText, Canvas.GetTop((UIElement)this.CurrentScoreText) + this.m_scoreHoverSpeedY);
    //        Canvas.SetLeft((UIElement)this.CurrentScoreText, Canvas.GetLeft((UIElement)this.CurrentScoreText) + this.m_scoreHoverSpeedX);
    //        if (this.nc3SpriteObject_bullet.Visibility == Visibility.Visible)
    //        {
    //            Canvas.SetTop((UIElement)this.nc3SpriteObject_bullet, Canvas.GetTop((UIElement)this.nc3SpriteObject_bullet) - 20.0);
    //            if (Canvas.GetTop((UIElement)this.nc3SpriteObject_bullet) < -this.nc3SpriteObject_bullet.Height)
    //                this.nc3SpriteObject_bullet.Visibility = Visibility.Hidden;
    //        }
    //        Rect rect1 = new Rect(Canvas.GetLeft((UIElement)this.nc3SpriteObject_bullet), Canvas.GetTop((UIElement)this.nc3SpriteObject_bullet), this.nc3SpriteObject_bullet.Width, this.nc3SpriteObject_bullet.Height);
    //        for (int index = 0; index < this.m_enemies.Count; ++index)
    //        {
    //            Enemy enemy = this.m_enemies[index];
    //            Rect rect2 = new Rect(Canvas.GetLeft((UIElement)enemy.rect), Canvas.GetTop((UIElement)enemy.rect), enemy.rect.Width, enemy.rect.Height);
    //            rect2.Inflate(-10.0, -10.0);
    //            if (this.nc3SpriteObject_bullet.Visibility == Visibility.Visible && rect1.IntersectsWith(rect2))
    //            {
    //                enemy.rect.Visibility = Visibility.Hidden;
    //                this.m_enemies.Remove(enemy);
    //                this.nc3SpriteObject_bullet.Visibility = Visibility.Hidden;
    //                this.AddScore();
    //            }
    //            double left = Canvas.GetLeft((UIElement)enemy.rect);
    //            double top = Canvas.GetTop((UIElement)enemy.rect);
    //            if (left < 10.0 || left > this.GameCanvas.ActualWidth - 140.0 - 10.0)
    //            {
    //                top += 140.0;
    //                enemy.moveDir = !enemy.moveDir;
    //            }
    //            double num = -this.m_enemySpeed;
    //            if (enemy.moveDir)
    //                num = this.m_enemySpeed;
    //            if (top > this.GameCanvas.ActualHeight - 280.0)
    //                this.OnEndGame();
    //            Canvas.SetTop((UIElement)enemy.rect, top);
    //            Canvas.SetLeft((UIElement)enemy.rect, left + num);
    //        }
    //        if (this.m_enemies.Count == 0)
    //            this.StartLevel();
    //        this.m_playerHoverSpeedX = Math.Sin(this.m_currentFrameTime * 1.5) * 0.2;
    //        this.m_playerHoverSpeedY = Math.Sin(this.m_currentFrameTime) * 0.2;
    //        double left1 = Canvas.GetLeft((UIElement)this.nc3SpriteObject_player);
    //        Canvas.SetTop((UIElement)this.nc3SpriteObject_player, Canvas.GetTop((UIElement)this.nc3SpriteObject_player) + this.m_playerHoverSpeedY);
    //        Canvas.SetLeft((UIElement)this.nc3SpriteObject_player, left1 + this.m_playerHoverSpeedX);
    //        if (left1 < 0.0 || left1 > this.background1.Width - this.nc3SpriteObject_player.Width)
    //            this.m_playerDirection = !this.m_playerDirection;
    //        this.m_moveSpeed = Math.Min(this.m_moveSpeed, 4.0);
    //        this.m_moveSpeed = Math.Max(this.m_moveSpeed, 0.0);
    //        if (this.m_playerDirection)
    //            Canvas.SetLeft((UIElement)this.nc3SpriteObject_player, Canvas.GetLeft((UIElement)this.nc3SpriteObject_player) - this.m_moveSpeed);
    //        else
    //            Canvas.SetLeft((UIElement)this.nc3SpriteObject_player, Canvas.GetLeft((UIElement)this.nc3SpriteObject_player) + this.m_moveSpeed);
    //    }

    //    private void OnGameKeyDown(KeyEventArgs e)
    //    {
    //        if (e.Key == Key.Left)
    //        {
    //            this.m_moveSpeed = 4.0;
    //            this.m_playerDirection = true;
    //        }
    //        else if (e.Key == Key.Right)
    //        {
    //            this.m_moveSpeed = 4.0;
    //            this.m_playerDirection = false;
    //        }
    //        if (e.Key != Key.Space || this.nc3SpriteObject_bullet.Visibility != Visibility.Hidden)
    //            return;
    //        Canvas.SetLeft((UIElement)this.nc3SpriteObject_bullet, Canvas.GetLeft((UIElement)this.nc3SpriteObject_player) + this.nc3SpriteObject_player.Width * 0.5);
    //        Canvas.SetTop((UIElement)this.nc3SpriteObject_bullet, Canvas.GetTop((UIElement)this.nc3SpriteObject_player));
    //        this.nc3SpriteObject_bullet.Visibility = Visibility.Visible;
    //    }

    //    private void GameCanvas_KeyDown(object sender, KeyEventArgs e)
    //    {
    //        if (this.m_bGameEnded)
    //            return;
    //        if (this.m_inGame)
    //        {
    //            this.OnGameKeyDown(e);
    //        }
    //        else
    //        {
    //            if (e.Key != Key.Space)
    //                return;
    //            this.StartGame();
    //        }
    //    }

    //    private void GameCanvas_KeyUp(object sender, KeyEventArgs e)
    //    {
    //    }

    //    public void InitializeComponent()
    //    {
    //        if (this._contentLoaded)
    //            return;
    //        this._contentLoaded = true;
    //        Application.LoadComponent((object)this, new Uri("/CyberInvasion;component/mainwindow.xaml", UriKind.Relative));
    //    }

    //    void IComponentConnector.Connect(int connectionId, object target)
    //    {
    //        switch (connectionId)
    //        {
    //            case 1:
    //                this.GameCanvas = (Canvas)target;
    //                this.GameCanvas.KeyDown += new KeyEventHandler(this.GameCanvas_KeyDown);
    //                this.GameCanvas.KeyUp += new KeyEventHandler(this.GameCanvas_KeyUp);
    //                break;
    //            case 2:
    //                this.background1 = (Rectangle)target;
    //                break;
    //            case 3:
    //                this.gameLogo = (Rectangle)target;
    //                break;
    //            case 4:
    //                this.nc3logo = (Rectangle)target;
    //                break;
    //            case 5:
    //                this.nc3SpriteObject_player = (Rectangle)target;
    //                break;
    //            case 6:
    //                this.nc3SpriteObject_bullet = (Rectangle)target;
    //                break;
    //            case 7:
    //                this.nc3SpriteObject_enemy1 = (Rectangle)target;
    //                break;
    //            case 8:
    //                this.nc3SpriteObject_enemy2 = (Rectangle)target;
    //                break;
    //            case 9:
    //                this.nc3SpriteObject_enemy3 = (Rectangle)target;
    //                break;
    //            case 10:
    //                this.nc3SpriteObject_enemy4 = (Rectangle)target;
    //                break;
    //            case 11:
    //                this.CurrentScoreText = (TextBlock)target;
    //                break;
    //            default:
    //                this._contentLoaded = true;
    //                break;
    //        }
    //    }
    //}
    #endregion

    public class Enemy
    {
        public Rectangle rect;
        public bool moveDir = true;
    }
}
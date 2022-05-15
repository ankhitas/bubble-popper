using Lesson5ProgrammingProjectWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;

namespace Lesson5ProgrammingProjectWPF
{
    //  namespace enums
    public enum GameMode { Stack, Queue };
    public enum GameDifficulty { Easy, Medium, Hard };
    public partial class MainWindow : Window
    {
        // class constants
        private const int QUEUE_START_NUM = 100;

        // properties
        private GameMode GameMode { get; set; }
        private GameDifficulty GameDifficulty { get; set; }
        private int GameScore { get; set; }
        private int HighScore { get; set; }
        private int GameSpeed { get; set; }
        private int GameTime { get; set; }
        private int StartingBubbles { get; set; }
        public string SETTINGS_FULL_PATH { get; }
        private int numBubblesClicked { get; set; }
        private int queueIndex { get; set; }
        private int nextQueue { get; set; }
        private int nextStack { get; set; }
        private int time { get; set; }
        private bool GameOver { get; set; }


        // fields
        private DispatcherTimer _gameTimer;
        private DispatcherTimer _totalGameTime;
        private Queue<Button> _queue;
        private Stack<Button> _stack;
        private Random _rng;

        //registry
        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BubblePopperApp");

        // constructor
        public MainWindow()
        {
            InitializeComponent();

            time = 0;

            //initialize settings file
            // initialize the path to the high scores file
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = System.IO.Path.Combine(appDataPath, "BubblePopperApp");
            System.IO.Directory.CreateDirectory(appDataPath);
            SETTINGS_FULL_PATH = System.IO.Path.Combine(appDataPath, "GameSettings.txt");

            //registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BubblePopperApp");
            if (key != null)
            {
                double highScore = double.Parse(key.GetValue("HighScore").ToString());
                labelHighScore.Content = "High Score: " + highScore.ToString();

            }

            // load game settings
            LoadSettings();

            // initialize timer
            _gameTimer = new DispatcherTimer();
            _gameTimer.Tick += TimerGame_Tick;

            if (this.GameDifficulty == GameDifficulty.Easy)
            {
                _gameTimer.Interval = new TimeSpan(60000000);
            }
            else if (this.GameDifficulty == GameDifficulty.Medium)
            {
                _gameTimer.Interval = new TimeSpan(50000000);
            }
            else {
                _gameTimer.Interval = new TimeSpan(40000000);
            }


            // initialize other fields
            _queue = new Queue<Button>();

            _stack = new Stack<Button>();
            _rng = new Random();

            // reset high score
            this.HighScore = 0;

            // initialize the game 
            ResetGame();

        }

        // instance methods
        private void LoadSettings()
        {
            // initalize settings to defaults
            this.GameMode = GameMode.Stack;
            this.GameSpeed = 1000;  // new bubble every 1 second
            this.GameTime = 30;  // game length is 30 seconds
            this.StartingBubbles = 10;
           
            // load saved user preference and override defaults
            if (File.Exists(SETTINGS_FULL_PATH) == true) { 
            StreamReader reader = new StreamReader(SETTINGS_FULL_PATH);

                using (reader)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        string line = reader.ReadLine();
                            // parse the line as comma separated values
                            string[] parts = line.Split(':');
                            string setting = parts[0];
                            string value = parts[1];

                            if (i == 0)
                            {
                                this.GameMode = (GameMode)int.Parse(value);
                            }
                            else if (i == 1)
                            {
                                this.GameDifficulty = (GameDifficulty)int.Parse(value);
                            }
                            else if (i == 2)
                            {
                                this.StartingBubbles = int.Parse(value);
                            }
                            else
                            {
                                this.GameTime = int.Parse(value);
                            }
                    }
                }
                Background.Background = new SolidColorBrush(Colors.PowderBlue);

            }

            _totalGameTime = new DispatcherTimer();
            _totalGameTime.Tick += totalGameTime_Tick;
            _totalGameTime.Interval = new TimeSpan(10000000);
        }

        private Button CreateBubbleButton(int num)
        {
            // construct the button control for this bubble
            Button button = new Button();
            // set the text for the button
            button.Content = num.ToString();
            button.FontWeight = FontWeights.Bold;
            // set the background image for the button
            string uri = "pack://application:,,,/Resources/SmallBubble.png";
            if (_rng.Next(0, 10) == 0)
            {
                // 10% chance to get large bubble
                uri = "pack://application:,,,/Resources/LargeBubble.png";
            }
            BitmapImage image = new BitmapImage(new Uri(uri));
            button.Background = new ImageBrush(image);
            // button dimensions
            button.Height = image.Height;
            button.Width = image.Width;
            // remove the border
            button.BorderThickness = new Thickness(0.0);
            // place the bubble on the screen
            int left = _rng.Next((int)button.Width, (int)(this.Width - button.Width * 2));
            int top = _rng.Next((int)button.Height, (int)(this.Height - button.Height * 2));
            Canvas.SetLeft(button, left);
            Canvas.SetTop(button, top);
            // set the style of the button
            button.Style = (Style)FindResource("ButtonStyleNoMouseOver");
            // add the event handler
            button.Click += BubbleButton_Click;
            // return the new button
            return button;
        }

        private void ResetGame()
        {
            // reset the scoreboard
            this.GameScore = 0;
            labelScore.Content = $"Score: {this.GameScore}";
            labelHighScore.Content = $"High Score: {this.HighScore}";
            this.time = 0;
            labelTimer.Content = "Timer: 0";
            GameOver = false;
            Background.Background = new SolidColorBrush(Colors.PowderBlue);
            nextQueue = 100 + StartingBubbles;
            nextStack = StartingBubbles;

            // set the queue and the stack
            this._queue.Clear();
            this._stack.Clear();

            // remove all bubble buttons
            List<UIElement> bubbles = new List<UIElement>();
            foreach (UIElement e in gameCanvas.Children)
            {
                if (e is Button)
                {
                    bubbles.Add(e);
                }
            }
            foreach (UIElement e in bubbles)
            {
                gameCanvas.Children.Remove(e);
            }

            // create the starting number of bubbles
            if (this.GameMode == GameMode.Stack)
            {
                for (int num = 1; num <= this.StartingBubbles; num++)
                {
                    Button b = CreateBubbleButton(num);
                    // add the button to the stack
                    _stack.Push(b);
                    // add the button to the canvas
                    gameCanvas.Children.Add(b);
                }
            }
            else
            {
                for (int num = 1; num <= this.StartingBubbles; num++)
                {
                    Button b = CreateBubbleButton(QUEUE_START_NUM + num - 1);
                    // add the button to the end of queue
                    _queue.Enqueue(b);
                    // add the button to the canvas
                    gameCanvas.Children.Insert(0, b);
                }
            }

            if (key != null)
            {
                string s = key.GetValue("HighScore").ToString();
                int prevScore = int.Parse(key.GetValue("HighScore").ToString());
                labelHighScore.Content = "High Score: " + prevScore;
            }
            else 
            {
                labelHighScore.Content = "High Score: 0";
            }
        }

        // event handlers
        
        private void  BubbleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameTimer.IsEnabled == false && GameOver == false) {
                _gameTimer.Start();
                _totalGameTime.Start();
            }
            if (GameOver == false)
            {
                //get number of button that was clicked on
                Button current = (Button)sender;
                int element = int.Parse((string)current.Content);

                //is that the greatest number? - get element at top of stack
                int topElement;
                if (this.GameMode == GameMode.Stack)
                {
                    Button topOfStack = _stack.Peek();
                    topElement = int.Parse((string)topOfStack.Content);
                }
                else
                {
                    Button beginningOfQueue = _queue.Peek();
                    topElement = int.Parse((string)beginningOfQueue.Content);
                }

                if (element == topElement)
                {
                    current.Visibility = Visibility.Hidden;
                    numBubblesClicked++;
                    labelScore.Content = "Score: " + numBubblesClicked;
                    if (this.GameMode == GameMode.Stack)
                    {
                        _stack.Pop();
                        nextStack--;
                    }
                    else //gameMode must be Queue
                    {
                        _queue.Dequeue();
                    }
                }
            }
        }

        private void newHighScore() {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BubblePopperApp");
            if (key.GetValue("HighScore") != null)
            {
                int prevScore = int.Parse(key.GetValue("HighScore").ToString());
                if (numBubblesClicked > prevScore)
                {
                    Background.Background = new SolidColorBrush(Colors.PaleGreen);
                    MessageBox.Show("New High Score!");
                    key.SetValue("HighScore", numBubblesClicked);
                    key.Close();
                    labelHighScore.Content = "High Score: " + numBubblesClicked;
                    numBubblesClicked = 0;
                    return;
                }
                else {
                    Background.Background = new SolidColorBrush(Colors.PaleVioletRed);
                    GameOver = true;
                    MessageBox.Show("Game Over!");
                }
            }
            else
            {
                key.SetValue("HighScore", numBubblesClicked);
                Background.Background = new SolidColorBrush(Colors.PaleGreen);
                MessageBox.Show("New High Score!");
                labelHighScore.Content = "High Score: " + numBubblesClicked;
            }
            
        }

        private void totalGameTime_Tick(object sender, EventArgs e) {
                time++;
                labelTimer.Content = "Timer: " + time;

            if (time >= GameTime)
            {
                _totalGameTime.Stop();
                _gameTimer.Stop();
                newHighScore();
            }
        }

    private void TimerGame_Tick(object sender, EventArgs e)
        {
            Button current = null;


            if (this.GameMode == GameMode.Stack)
            {
                Button endOfStack= _stack.LastOrDefault();
                nextStack++;
                Button b = CreateBubbleButton(nextStack);
                _stack.Push(b);
                gameCanvas.Children.Add(b);
            }
            else //gameMode must be Queue
            {
                    Button endOfQueue = _queue.LastOrDefault();
                    Button newButton = CreateBubbleButton(nextQueue);
                    nextQueue++;
                    _queue.Enqueue(newButton);
                    gameCanvas.Children.Insert(0,newButton);
            }
        }

        private void MenuItem_Click_New(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }


        private void MenuItem_Click_Settings(object sender, RoutedEventArgs e)
        {
            GameSettingsWindow gsw = new GameSettingsWindow(this.GameMode, this.GameDifficulty,
                                                this.StartingBubbles, this.GameTime);
            gsw.ShowDialog();
            if (gsw.DialogResult == true)
            {
                this.GameMode = (GameMode)gsw.gameMode;
                this.GameDifficulty = (GameDifficulty)gsw.gameDifficulty;
                this.StartingBubbles = gsw.startingBubbles;
                this.GameTime = gsw.gameTime;

                StreamWriter writer = new StreamWriter(SETTINGS_FULL_PATH);
                using (writer)
                {
                    writer.WriteLine("GameMode:" + gsw.gameMode);
                    writer.WriteLine("GameDifficulty:" + gsw.gameDifficulty);
                    writer.WriteLine("StartingBubbles:" + this.StartingBubbles);
                    writer.WriteLine("GameTime:" + this.GameTime);
                }

                ResetGame();
            }
        }

        private void MenuItem_Click_Quit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

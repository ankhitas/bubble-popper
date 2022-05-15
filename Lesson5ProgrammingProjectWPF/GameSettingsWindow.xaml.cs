using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Lesson5ProgrammingProjectWPF
{
    /// <summary>
    /// Interaction logic for GameSettingsWindow.xaml
    /// </summary>
    public partial class GameSettingsWindow : Window
    {
        public int gameMode { get; set; }
        public int gameDifficulty { get; set; }
        public int startingBubbles { get; set; }
        public int gameTime { get; set; }

        public GameSettingsWindow(GameMode gameMode, GameDifficulty difficult, int startingBubbles, int gameTime)
        {
            InitializeComponent();

            this.gameMode = (int)gameMode;
            this.gameDifficulty = (int)difficult;
            this.startingBubbles = startingBubbles;
            this.gameTime = gameTime;

            // set up values of controls on the forms to mimic the parameters
            if (gameMode == GameMode.Stack)
            {
                stackButton.IsChecked = true;
                queueButton.IsChecked = false;
            }
            else
            {
                stackButton.IsChecked = false;
                queueButton.IsChecked = true;
            }

            if ((GameDifficulty)this.gameDifficulty == GameDifficulty.Easy)
            {
                easyButton.IsChecked = true;
                mediumButton.IsChecked = false;
                hardButton.IsChecked = false;
            }
            else if ((GameDifficulty)this.gameDifficulty == GameDifficulty.Medium)
            {
                easyButton.IsChecked = false;
                mediumButton.IsChecked = true;
                hardButton.IsChecked = false;
            }
            else {
                easyButton.IsChecked = false;
                mediumButton.IsChecked = false;
                hardButton.IsChecked = true;
            }

            startingBubblesTextBox.Text = this.startingBubbles.ToString();
            gameLengthTextBox.Text = this.gameTime.ToString();
        }

        private void stackButton_Checked(object sender, RoutedEventArgs e)
        {
            gameMode = 0;
        }

        private void queueButton_Checked(object sender, RoutedEventArgs e)
        {
            gameMode = 1;
        }

        private void easyButton_Checked(object sender, RoutedEventArgs e)
        {
            gameDifficulty = 0;
        }

        private void mediumButton_Checked(object sender, RoutedEventArgs e)
        {
            gameDifficulty = 1;
        }

        private void hardButton_Checked(object sender, RoutedEventArgs e)
        {
            gameDifficulty = 2;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            
            //get input from user on the number of starting bubbles;
            bool bubbleNumOk;
            int bubbleInput;
                bubbleNumOk = int.TryParse(startingBubblesTextBox.Text, out bubbleInput);
                if (bubbleNumOk == false) {
                    MessageBox.Show("Please enter a valid integer input as your number of starting bubbles!");
                    startingBubblesTextBox.Text = "";
                    return;
                }
            startingBubbles = bubbleInput;

            //get input from the user on the length of the game;
            bool gameLengthOk;
            int gameLengthInput;
                gameLengthOk = int.TryParse(gameLengthTextBox.Text, out gameLengthInput);
                if (gameLengthOk == false)
                {
                    MessageBox.Show("Please enter a valid integer input as your number of starting bubbles!");
                    gameLengthTextBox.Text = "";
                return;
                }
            gameTime = gameLengthInput;

            //close window
            this.DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            //close window
            this.DialogResult = false;
            this.Close();
        }
    }
}

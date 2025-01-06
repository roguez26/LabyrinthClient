using LabyrinthClient.GameService;
using LabyrinthClient.Properties;
using System;
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
using System.Windows.Threading;



namespace LabyrinthClient
{
    public partial class CharacterSelection : Window
    {
        DispatcherTimer _animationTimer = new DispatcherTimer();
        int currentFrame = 0;
        List<BitmapImage> _walkFrames = new List<BitmapImage>();
        public string _skinSelection { get; set; }
        public string SelectedSkin => _skinSelection;



        public CharacterSelection()
        {
            initializeAnimationTimer();
            InitializeComponent();
        }

        private void BackButtonIsPressed(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void initializeAnimationTimer()
        {
            _animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            _animationTimer.Tick += UpdateFrame;
        }

        private void InitializeWalkFrames(string characterName)
        {
            _walkFrames.Clear();
            for (int i = 0; i <= 15; i++)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri($"pack://application:,,,/Skins/{characterName}/{characterName}_{i:00}.png");
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                _walkFrames.Add(image);
            }
        }

        private void UpdateFrame(object sender, EventArgs e)
        {
            CharacterSprites.Source = _walkFrames[currentFrame];
            currentFrame = (currentFrame + 1) % _walkFrames.Count;

        }

        private void StartAnimation(string characterName)
        {
            _animationTimer.Stop();
            InitializeWalkFrames(characterName);
            _animationTimer.Start();
        }

        private void AdventurerIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.AdventurerNameLabel.ToString();
            CharacterDescription.Content = Characters.AdventurerDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/adventurous/adventurous.png"));
            StartAnimation("adventurous");
            _skinSelection = "Adventurer";
        }

        private void AlienIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.AlienNameLabel.ToString();
            CharacterDescription.Content = Characters.AlienDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/alien/alien.png"));
            StartAnimation("alien");
            _skinSelection = "Alien";
        }

        private void HunterIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.HunterNameLabel.ToString();
            CharacterDescription.Content = Characters.HunterDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/hunter/hunter.png"));
            StartAnimation("hunter");
            _skinSelection = "Hunter";
        }

        private void KnightIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.KnightNameLabel.ToString();
            CharacterDescription.Content = Characters.KnightDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/knight/knight.png"));
            StartAnimation("knight");
            _skinSelection = "Knight";
        }

        private void NinjaIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.NinjaNameLabel.ToString();
            CharacterDescription.Content = Characters.NinjaDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/ninja/ninja.png"));
            StartAnimation("ninja");
            _skinSelection = "Ninja";
        }
        private void PirateIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.PirateNameLabel.ToString();
            CharacterDescription.Content = Characters.PirateDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/pirate/pirate.png"));
            StartAnimation("pirate");
            _skinSelection = "Pirate";
        }
        private void PrincessIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.PrincessNameLabel.ToString();
            CharacterDescription.Content = Characters.PrincessDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/princess/princess.png"));
            StartAnimation("princess");
            _skinSelection = "Princess";
        }

        private void RobotIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.RobotNameLabel.ToString();
            CharacterDescription.Content = Characters.RobotDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/robot/robot.png"));
            StartAnimation("robot");
            _skinSelection = "Robot";
        }

        private void ScientistIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.ScientistNameLabel.ToString();
            CharacterDescription.Content = Characters.ScientistDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/scientist/scientist.png"));
            StartAnimation("scientist");
            _skinSelection = "Scientist";
        }

        private void WizzardIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.WizzardNameLabel.ToString();
            CharacterDescription.Content = Characters.WizzardDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/wizzard/wizzard.png"));
            StartAnimation("wizzard");
            _skinSelection = "Wizzard";
        }

        private void selectCharacterButtonIsPressed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}


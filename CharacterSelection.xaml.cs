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

        public CharacterSelection()
        {
            initializeAnimationTimer();
            InitializeComponent();
        }

        private void BackButtonIsPressed(object sender, RoutedEventArgs e)
        {
            AdminLobby.GetInstance().Show();
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
                image.UriSource = new Uri($"pack://application:,,,/Skins/{characterName}/sprites/{characterName}_{i:00}.png");
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

        private void StopAnimation()
        {
            _animationTimer.Stop();
            currentFrame = 0;
            CharacterSprites.Source = _walkFrames[currentFrame]; 
        }

        private void PrincessIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.PrincessNameLabel.ToString();
            CharacterDescription.Content = Characters.PrincessDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/princess/princess.png"));
            StartAnimation(Characters.PrincessNameLabel.ToString());
        }

        private void AdventurerIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.AdventurerNameLabel.ToString();
            CharacterDescription.Content = Characters.AdventurerDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/adventurer/adventurer.png"));
            StartAnimation(Characters.AdventurerNameLabel.ToString());
        }

        private void WizzardIsSelected(object sender, RoutedEventArgs e)
        {

            CharacterName.Content = Characters.WizzardNameLabel.ToString();
            CharacterDescription.Content = Characters.WizzardDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/wizzard/wizzard.png"));
            StartAnimation(Characters.WizzardNameLabel.ToString());
        }

        private void KnightIsSelected(object sender, RoutedEventArgs e)
        {
            CharacterName.Content = Characters.KnightNameLabel.ToString();
            CharacterDescription.Content = Characters.KnightDescriptionLabel.ToString();
            CharacterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Skins/knight/knight.png"));
            StartAnimation(Characters.KnightNameLabel.ToString());
        }

    }

}


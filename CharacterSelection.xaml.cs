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
    /// <summary>
    /// Lógica de interacción para CharacterSelection.xaml
    /// </summary>

    public partial class CharacterSelection : Window
    {
        // Define el timer
        DispatcherTimer animationTimer = new DispatcherTimer();
        int currentFrame = 0;
        List<BitmapImage> walkFrames = new List<BitmapImage>();

        public CharacterSelection()
        {
            initializeAnimationTimer();
            InitializeComponent();
        }

        private void BackButtonIsPressed(object sender, RoutedEventArgs e)
        {

        }

        private void initializeAnimationTimer()
        {
            animationTimer.Interval = TimeSpan.FromMilliseconds(100); 
            animationTimer.Tick += UpdateFrame;
        }

        private void InitializeWalkFrames(string characterName)
        {
            walkFrames.Clear();
            for (int i = 0; i <= 15; i++)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri($"pack://application:,,,/Skins/{characterName}/sprites/{characterName}_{i:00}.png");
                image.CacheOption = BitmapCacheOption.OnLoad; 
                image.EndInit();
                walkFrames.Add(image);
            }
        }

        private void UpdateFrame(object sender, EventArgs e)
        {
            CharacterSprites.Source = walkFrames[currentFrame];
            currentFrame = (currentFrame + 1) % walkFrames.Count; 

        }

        private void StartAnimation(string characterName)
        {
            animationTimer.Stop();
            InitializeWalkFrames(characterName);
            animationTimer.Start();
        }

        private void StopAnimation()
        {
            animationTimer.Stop();
            currentFrame = 0;
            CharacterSprites.Source = walkFrames[currentFrame]; 
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


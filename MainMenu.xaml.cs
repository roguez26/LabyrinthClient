using System;
using System.Collections.Generic;
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
using LabyrinthClient.UserManagementService;

namespace LabyrinthClient
{
    /// <summary>
    /// Lógica de interacción para MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {

        private static MainMenu _instance;
        private TransferUser _currentSession;
         
        private MainMenu(TransferUser user)
        {
            InitializeComponent();
            _currentSession = user;
            UpdateData(_currentSession);
        }

        private void UpdateData(TransferUser user)
        {
            userButton.Content = user.Username;

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                changeProfilePicture();
            }
        }

        private void changeProfilePicture()
        {
            UserManagementService.UserManagementClient userManagement = new UserManagementService.UserManagementClient();
            byte[] imageData = userManagement.getUserProfilePicture(_currentSession.ProfilePicture);

            if (imageData != null && imageData.Length > 0)
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream memoryStream = new MemoryStream(imageData))
                {
                    memoryStream.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }

                userProfilePictureImage.Source = bitmapImage;
            }
        }

        public static MainMenu GetInstance(TransferUser user)
        {
            if (_instance == null || !_instance.IsVisible)
            {
                _instance = new MainMenu(user);
            } else
            {
                _instance.UpdateData(user);
            }
            _instance.Activate();
            return _instance;
        }        

        private void HostGameButtonIsPressed(object sender, RoutedEventArgs e)
        {
            PlayerLobby playerLobby = new PlayerLobby(_currentSession);
            playerLobby.Show();
            this.Close();
        }

        private void JoinGameButtonIsPressed(object sender, RoutedEventArgs e)
        {

        }

        private void userButtonIsPressed(object sender, RoutedEventArgs e)
        {

            MyUser myUser = MyUser.GetInstance(_currentSession);
            myUser.Show();
        }
    }
}

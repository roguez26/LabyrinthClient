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
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;

namespace LabyrinthClient
{
    public partial class MainMenu : Window
    {

        private static MainMenu _instance;
        private MyUser _myUser;
        private LobbySelection _lobbySelection;
        private MainMenu()
        {
            InitializeComponent();
            UpdateData();
            
        }

        private void UpdateData()
        {
            userButton.Content = CurrentSession.CurrentUser.Username;

            if (!string.IsNullOrEmpty(CurrentSession.CurrentUser.ProfilePicture))
            {
                ChangeProfilePicture();
            }
        }

        private void ChangeProfilePicture()
        {
            UserManagementService.UserManagementClient userManagement = new UserManagementService.UserManagementClient();
            byte[] imageData = userManagement.GetUserProfilePicture(CurrentSession.CurrentUser.ProfilePicture);

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

        public static MainMenu GetInstance()
        {
            if (_instance == null || !_instance.IsVisible)
            {
                _instance = new MainMenu();
                _instance.Activate();
            } else
            {
                _instance.UpdateData();
            }
            
            return _instance;
        }        

        private void HostGameButtonIsPressed(object sender, RoutedEventArgs e)
        {
            AdminLobby.GetInstance().Show();
            this.Hide();
            this.Close();
        }

        private void JoinGameButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (_lobbySelection == null)
            {
                _lobbySelection = new LobbySelection();
                _lobbySelection.Closed += (s, args) => _lobbySelection = null;
                _lobbySelection.Show();
            }
            else
            {
                _lobbySelection.Activate();
            }

        }

        private void UserButtonIsPressed(object sender, RoutedEventArgs e)
        {

            if (_myUser == null)
            {
                _myUser = new MyUser();
                _myUser.Closed += (s, args) => _myUser = null;
                _myUser.Show();
            }
            else
            {
                _myUser.Activate();
            }
        }
    }
}

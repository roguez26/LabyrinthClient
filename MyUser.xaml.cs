using LabyrinthClient.CatalogManagementService;
using LabyrinthClient.Properties;
using LabyrinthClient.UserManagementService;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
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

namespace LabyrinthClient
{
    /// <summary>
    /// Lógica de interacción para User.xaml
    /// </summary>
    public partial class MyUser : Window
    {
        private static MyUser _instance;
        private TransferUser _currentSession;

        private MyUser(TransferUser user)
        {
            _currentSession = user;
            InitializeComponent();
            userNameTextBox.Text = user.Username;

            emailTextBox.Text = user.Email;

            CatalogManagementService.CatalogManagementClient client = new CatalogManagementService.CatalogManagementClient();

            var countries = client.getAllCountries();
            countryComboBox.ItemsSource = countries;

            countryComboBox.DisplayMemberPath = "CountryName";
            countryComboBox.SelectedValuePath = "CountryId";
            countryComboBox.SelectedValue = user.TransferCountry.CountryId;

            TransferStats stats = client.getStatsByUserId(_currentSession.IdUser);
            if (stats.StatId > 0)
            {
                gamesPlayedCuantityLabel.Content = stats.GamesPlayed;
                gamesWonCuantityLabel.Content = stats.GamesWon;
            }


            if (!string.IsNullOrEmpty(_currentSession.ProfilePicture))
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

        public static MyUser GetInstance(TransferUser user)
        {
            if (_instance == null || !_instance.IsVisible)
            {
                _instance = new MyUser(user);
            }
            _instance.Activate();
            return _instance;
        }


        private void EditProfileButtonIsPressed(object sender, RoutedEventArgs e)
        {
            changeToEditMode(true);
        }

        private void AcceptButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int response = 0;
            changeToEditMode(false);

            if (!string.IsNullOrWhiteSpace(newPasswordPasswordBox.Password) && EncryptPassword(newPasswordPasswordBox.Password) != _currentSession.Password)
            {
                response = UpdatePassword();
            }
            else if (userNameTextBox.Text != _currentSession.Username || emailTextBox.Text != _currentSession.Email || (int)countryComboBox.SelectedValue != _currentSession.TransferCountry.CountryId)
            {
                response = UpdateUser();
            }

            switch (response)
            {
                case 0:
                    changeToEditPasswordMode(false);
                    changeToEditMode(false);
                    break;
                case 1:
                    _currentSession = GetTransferUser();
                    MainMenu.GetInstance(_currentSession);
                    ShowMessage("SuccessProfileUpdatedTitle", "SuccessProfileUpdatedMessage");
                    break;
                case -1: ShowMessage("FailProfileNotUpdatedTitle", "FailUserNotFoundMessage"); break;
                case -2: ShowMessage("FailProfileNotUpdatedTitle", "FailDuplicatedEmailMessage"); break;
            }
        }

        private int UpdatePassword()
        {
            int response = 0;
            UserManagementService.UserManagementClient userManagement = new UserManagementService.UserManagementClient();
            UserManagementService.TransferUser transferUser = new UserManagementService.TransferUser();
            transferUser.Password = EncryptPassword(oldPasswordPasswordBox.Password);
            transferUser.Email = _currentSession.Email;
            transferUser = userManagement.userVerification(transferUser);

            if (!string.IsNullOrEmpty(transferUser.ErrorCode))
            {
                ShowMessage("FailProfileNotUpdatedTitle", transferUser.ErrorCode);
            }
            else
            {
                _currentSession.Password = EncryptPassword(newPasswordPasswordBox.Password);
                response = UpdateUser();
            }
            return response;
        }

        private TransferUser GetTransferUser() 
        {
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();
            user.IdUser = _currentSession.IdUser;
            user.Username = userNameTextBox.Text;
            user.Email = emailTextBox.Text;
            user.Country = (int)countryComboBox.SelectedValue;
            user.TransferCountry = _currentSession.TransferCountry;
            user.Password = _currentSession.Password;
            user.TransferCountry.CountryId = ((CatalogManagementService.TransferCountry)countryComboBox.SelectedItem).CountryId;
            user.TransferCountry.CountryName = ((CatalogManagementService.TransferCountry)countryComboBox.SelectedItem).CountryName;
            return user;
        }

        private int UpdateUser()
        {
            UserManagementService.UserManagementClient client = new UserManagementService.UserManagementClient();
            return client.updateUser(GetTransferUser());
        }

        private void CancelButtonIsPressed(object sender, RoutedEventArgs e)
        {
            changeToEditPasswordMode(false);
            changeToEditMode(false);
        }

        private void ChangePasswordButtonIsPressed(object sender, RoutedEventArgs e)
        {
            changeToEditPasswordMode(true);
        }

        private void changeToEditPasswordMode(Boolean passwordIsEditable)
        {
            if (passwordIsEditable)
            {
                changePasswordButton.Visibility = Visibility.Collapsed;
                userNameTextBox.Visibility = Visibility.Collapsed;
                countryComboBox.Visibility = Visibility.Collapsed;
                emailTextBox.Visibility = Visibility.Collapsed;
                oldPasswordPasswordBox.Visibility = Visibility.Visible;
                newPasswordPasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                userNameTextBox.Visibility = Visibility.Visible;
                countryComboBox.Visibility = Visibility.Visible;
                emailTextBox.Visibility = Visibility.Visible;
                oldPasswordPasswordBox.Visibility = Visibility.Collapsed;
                newPasswordPasswordBox.Visibility = Visibility.Collapsed;
                oldPasswordPasswordBox.Clear();
                newPasswordPasswordBox.Clear();
            }
        }

        private void changeToEditMode(Boolean isEditable)
        {
            if (isEditable)
            {
                editProfileButton.Visibility = Visibility.Collapsed;
                acceptButton.Visibility = Visibility.Visible;
                cancelButton.Visibility = Visibility.Visible;
                changePasswordButton.Visibility = Visibility.Visible;
                changeProfilePicureButton.Visibility = Visibility.Visible;

            }
            else
            {
                editProfileButton.Visibility = Visibility.Visible;
                acceptButton.Visibility = Visibility.Collapsed;
                cancelButton.Visibility = Visibility.Collapsed;
                changePasswordButton.Visibility = Visibility.Collapsed;
                changeProfilePicureButton.Visibility = Visibility.Collapsed;

            }
            userNameTextBox.IsEnabled = isEditable;
            countryComboBox.IsEnabled = isEditable;
            emailTextBox.IsEnabled = isEditable;
        }

        private void ChangeProfilePictureIsPressed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                byte[] fileBytes = File.ReadAllBytes(filePath);

                UserManagementService.UserManagementClient client = new UserManagementService.UserManagementClient();

                try
                {

                    _currentSession.ProfilePicture = client.changeUserProfilePicture(_currentSession.IdUser, fileBytes);
                    changeProfilePicture();
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error al subir la imagen: {ex.Message}");
                }
            }
        }

        private void ShowMessage(string errorTitleCode, string errorMessageCode)
        {
            string message = Messages.ResourceManager.GetString(errorMessageCode);
            string title = Messages.ResourceManager.GetString(errorTitleCode);
            MessageBox.Show(message, title);
        }

        public string EncryptPassword(string password)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder stringBuilder = new StringBuilder();

            stream = sha256.ComputeHash(encoding.GetBytes(password));
            for (int i = 0; i < stream.Length; i++)
            {
                stringBuilder.AppendFormat("{0:x2}", stream[i]);
            }
            return stringBuilder.ToString();
        }



    }
}

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
        private static MyUser instance;
        private TransferUser currentSession;

        private MyUser(TransferUser user)
        {
            currentSession = user;
            InitializeComponent();
            userNameTextBox.Text = user.Username;

            emailTextBox.Text = user.Email;

            CatalogManagementService.CatalogManagementClient client = new CatalogManagementService.CatalogManagementClient();

            var countries = client.getAllCountries();
            countryComboBox.ItemsSource = countries;

            countryComboBox.DisplayMemberPath = "CountryName";
            countryComboBox.SelectedValuePath = "CountryId";
            countryComboBox.SelectedValue = user.TransferCountry.CountryId;

            TransferStats stats = client.getStatsByUserId(currentSession.IdUser);
            if (stats.StatId > 0)
            {
                gamesPlayedCuantityLabel.Content = stats.GamesPlayed;
                gamesWonCuantityLabel.Content = stats.GamesWon;
            }


            if (!string.IsNullOrEmpty(currentSession.ProfilePicture))
            {
                changeProfilePicture();

            }
        }

        private void changeProfilePicture()
        {
            UserManagementService.UserManagementClient userManagement = new UserManagementService.UserManagementClient();
            byte[] imageData = userManagement.getUserProfilePicture(currentSession.ProfilePicture);

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
            if (instance == null || !instance.IsVisible)
            {
                instance = new MyUser(user);
            }
            instance.Activate();
            return instance;
        }


        private void EditProfileButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToEditMode(true);
        }

        private void AcceptButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int response = 0;
            ChangeToEditMode(false);

            if (!string.IsNullOrWhiteSpace(newPasswordPasswordBox.Password) && EncryptPassword(newPasswordPasswordBox.Password) != currentSession.Password)
            {
                response = UpdatePassword();
            }
            else if (userNameTextBox.Text != currentSession.Username || emailTextBox.Text != currentSession.Email || (int)countryComboBox.SelectedValue != currentSession.TransferCountry.CountryId)
            {
                response = UpdateUser();
            }

            switch (response)
            {
                case 0:
                    ChangeToEditPasswordMode(false);
                    ChangeToEditMode(false);
                    break;
                case 1:
                    currentSession = GetTransferUser();
                    MainMenu.GetInstance(currentSession);
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
            transferUser.Email = currentSession.Email;
            transferUser = userManagement.userVerification(transferUser);

            if (!string.IsNullOrEmpty(transferUser.ErrorCode))
            {
                ShowMessage("FailProfileNotUpdatedTitle", transferUser.ErrorCode);
            }
            else
            {
                currentSession.Password = EncryptPassword(newPasswordPasswordBox.Password);
                response = UpdateUser();
            }
            return response;
        }

        private TransferUser GetTransferUser() 
        {
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();
            user.IdUser = currentSession.IdUser;
            user.Username = userNameTextBox.Text;
            user.Email = emailTextBox.Text;
            user.Country = (int)countryComboBox.SelectedValue;
            user.TransferCountry = currentSession.TransferCountry;
            user.Password = currentSession.Password;
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
            ChangeToEditPasswordMode(false);
            ChangeToEditMode(false);
        }

        private void ChangePasswordButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToEditPasswordMode(true);
        }

        private void ChangeToEditPasswordMode(Boolean passwordIsEditable)
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

        private void ChangeToEditMode(Boolean isEditable)
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

        private void ChangeProfilePictureButtonIsPressed(object sender, RoutedEventArgs e)
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
                    currentSession.ProfilePicture = client.changeUserProfilePicture(currentSession.IdUser, fileBytes);
                    MainMenu.GetInstance(currentSession);
                    changeProfilePicture();
                    ChangeToEditMode(false);
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

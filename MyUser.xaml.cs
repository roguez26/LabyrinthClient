using HelperClasses;
using LabyrinthClient.CatalogManagementService;
using LabyrinthClient.Properties;
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.ServiceModel;
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

    public partial class MyUser : Window, MenuManagementService.IMenuManagementServiceCallback
    {
        private UserManagementService.UserManagementClient _client;

        public MyUser()
        {
            InitializeComponent();
            DataContext = this;
            InstanceContext context = new InstanceContext(this);
            _client = new UserManagementClient(context);
            userNameTextBox.Text = CurrentSession.CurrentUser.Username;

            emailTextBox.Text = CurrentSession.CurrentUser.Email;

            ChargeCountriesAndStats();

            if (CurrentSession.ProfilePicture != null)
            {
                userProfilePictureImage.Source = CurrentSession.ProfilePicture;
            }
            else
            {
                userProfilePictureImage.Source = null;
            }
        }

        private void ChargeCountriesAndStats()
        {
            CatalogManagementService.CatalogManagementClient catalogClient = new CatalogManagementService.CatalogManagementClient();
            countryComboBox.DisplayMemberPath = "CountryName";
            countryComboBox.SelectedValuePath = "CountryId";
            countryComboBox.SelectedValue = CurrentSession.CurrentUser.TransferCountry.CountryId;
            try
            {
                CatalogManagementService.TransferStats stats = catalogClient.GetStatsByUserId(CurrentSession.CurrentUser.IdUser);
                CatalogManagementService.TransferCountry[] countries = catalogClient.GetAllCountries();
                countryComboBox.ItemsSource = countries;
                if (stats.StatId > 0)
                {
                    gamesPlayedCuantityLabel.Content = stats.GamesPlayed;
                    gamesWonCuantityLabel.Content = stats.GamesWon;
                }
            }
            catch (FaultException<CatalogManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
        }


        private void EditProfileButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToEditMode(true);
        }

        private void AcceptButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int response = 0;

            if (!string.IsNullOrWhiteSpace(newPasswordPasswordBox.Password) && newPasswordPasswordBox.IsVisible)
            {
                response = UpdatePassword();
            }
            else if (userNameTextBox.Text != CurrentSession.CurrentUser.Username || emailTextBox.Text != CurrentSession.CurrentUser.Email || (int)countryComboBox.SelectedValue != CurrentSession.CurrentUser.TransferCountry.CountryId)
            {
                response = UpdateUser();
            }

            if (response > 0)
            {
                Message message = new Message("InfoProfileUpdatedMessage");
                message.ShowDialog();
                MainMenu.GetInstance();
                ChangeToEditMode(false);
                if (changePasswordButton.Visibility == Visibility.Collapsed)
                {
                    ChangeToEditPasswordMode(false);
                }
            }
        }

        private int UpdatePassword()
        {
            int response = 0;
            string oldPassword = EncryptPassword(oldPasswordPasswordBox.Password);
            string newPassword = EncryptPassword(newPasswordPasswordBox.Password);

            try
            {
                Message message = new Message("InfoUpdatePasswordConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
                message.ShowDialog();
                if (message.UserDialogResult == Message.DialogResult.Confirm)
                {
                    response = _client.UpdatePassword(oldPassword, newPassword, CurrentSession.CurrentUser.Email);
                }
            }
            catch (FaultException<UserManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            
            return response;
        }

        private TransferUser GetTransferUser() 
        {
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();

            user.IdUser = CurrentSession.CurrentUser.IdUser;
            user.Username = userNameTextBox.Text;
            user.Email = emailTextBox.Text;
            user.Country = (int)countryComboBox.SelectedValue;
            user.TransferCountry = CurrentSession.CurrentUser.TransferCountry;
            user.TransferCountry.CountryId = ((CatalogManagementService.TransferCountry)countryComboBox.SelectedItem).CountryId;
            user.TransferCountry.CountryName = ((CatalogManagementService.TransferCountry)countryComboBox.SelectedItem).CountryName;
            return user;
        }

        private int UpdateUser()
        {
            int result = 0;
            try
            {
                Message message = new Message("InfoUpdateProfileConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
                message.ShowDialog();
                if (message.UserDialogResult == Message.DialogResult.Confirm)
                {
                    TransferUser newUser = GetTransferUser();
                    result = _client.UpdateUser(newUser);
                    CurrentSession.CurrentUser = newUser;
                }
            }
            catch (FaultException<UserManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            return result;
        }

        private void CancelButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToEditMode(false);
            if (changePasswordButton.Visibility == Visibility.Collapsed)
            {
                ChangeToEditPasswordMode(false);
            }
        }

        private void ChangePasswordButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToEditPasswordMode(true);
        }

        private void ChangeToEditPasswordMode(bool isEditable)
        {
            ChangeDataVisibility(!isEditable);
            if (isEditable)
            {
                changeProfilePicureButton.Visibility = Visibility.Collapsed;
                changePasswordButton.Visibility = Visibility.Collapsed;
                oldPasswordPasswordBox.Visibility = Visibility.Visible;
                newPasswordPasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                oldPasswordPasswordBox.Visibility = Visibility.Collapsed;
                newPasswordPasswordBox.Visibility = Visibility.Collapsed;
                oldPasswordPasswordBox.Clear();
                newPasswordPasswordBox.Clear();
                ChangeDataVisibility(!isEditable);

            }
        }

        private void ChangeDataVisibility(bool visibility)
        {
            if (visibility)
            {
                userNameTextBox.Visibility = Visibility.Visible;
                countryComboBox.Visibility = Visibility.Visible;
                emailTextBox.Visibility = Visibility.Visible;
            } else
            {
                userNameTextBox.Visibility = Visibility.Collapsed;
                countryComboBox.Visibility = Visibility.Collapsed;
                emailTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeToEditMode(bool isEditable)
        {
            ChangeToEditDataMode(isEditable);
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
        }

        private void ChangeToEditDataMode(bool isEditable)
        {
            if (isEditable)
            {
                userNameTextBox.IsEnabled = isEditable;
                countryComboBox.IsEnabled = isEditable;
                emailTextBox.IsEnabled = isEditable;
            } 
            else
            {
                userNameTextBox.IsEnabled = isEditable;
                countryComboBox.IsEnabled = isEditable;
                emailTextBox.IsEnabled = isEditable;
            }
        }

        private void ChangeProfilePictureButtonIsPressed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg)|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                Message message = new Message("InfoUpdateProfilePictureConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
                message.ShowDialog();
                if (message.UserDialogResult == Message.DialogResult.Confirm)
                {
                    string filePath = openFileDialog.FileName;
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    try
                    {
                        string newPath = _client.ChangeUserProfilePicture(CurrentSession.CurrentUser.IdUser, fileBytes);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            CurrentSession.CurrentUser.ProfilePicture = newPath;
                            CurrentSession.ProfilePicture = ProfilePictureManager.ByteArrayToBitmapImage(fileBytes);
                            userProfilePictureImage.Source = CurrentSession.ProfilePicture;
                            MainMenu.GetInstance();
                            ChangeToEditMode(false);
                        }
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Error al subir la imagen: {ex.Message}");
                    }
                }
                
            }
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

        public void AttendInvitation(string lobbyCode)
        {
            throw new NotImplementedException();
        }
    }
}

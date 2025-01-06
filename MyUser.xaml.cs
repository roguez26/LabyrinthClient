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
using System.Drawing;

namespace LabyrinthClient
{

    public partial class MyUser : Window
    {
        private const int  MaxLongProfilePicture = 1048576;
        public MyUser()
        {
            InitializeComponent();
            InitializeUserData();
            LoadCountriesFromResources();
            LoadStats();
        }

        private void InitializeUserData()
        {
            userNameTextBox.Text = CurrentSession.CurrentUser.Username;
            emailTextBox.Text = CurrentSession.CurrentUser.Email;
            if (CurrentSession.ProfilePicture != null)
            {
                userProfilePictureImage.Source = CurrentSession.ProfilePicture;
            }
            else
            {
                userProfilePictureImage.Source = null;
            }
        }

        private void LoadStats()
        {
            CatalogManagementService.CatalogManagementClient catalogManagementClient = new CatalogManagementService.CatalogManagementClient();
            try
            {
                CatalogManagementService.TransferStats stats = catalogManagementClient.GetStatsByUserId(CurrentSession.CurrentUser.IdUser);
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
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
        }

        private void EditProfileButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToEditMode(true);
            LoadCountriesFromResources();
        }
        private void LoadCountriesFromResources()
        {
            var resourceSet = Countries.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            var countries = resourceSet.Cast<System.Collections.DictionaryEntry>()
                                        .Select(entry => new
                                        {
                                            CountryCode = entry.Key.ToString(),
                                            CountryName = entry.Value.ToString()
                                        })
                                        .OrderBy(country => country.CountryName)
                                        .ToList();

            countryComboBox.ItemsSource = countries;
            countryComboBox.DisplayMemberPath = "CountryName";
            countryComboBox.SelectedValuePath = "CountryCode";
            countryComboBox.SelectedValue = CurrentSession.CurrentUser.CountryCode;
        }

        private void AcceptButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int response = 0;

            if (!string.IsNullOrWhiteSpace(newPasswordPasswordBox.Password) && newPasswordPasswordBox.IsVisible)
            {
                response = UpdatePassword();
            }
            else if (userNameTextBox.Text != CurrentSession.CurrentUser.Username || emailTextBox.Text != CurrentSession.CurrentUser.Email || countryComboBox.SelectedValue.ToString() != CurrentSession.CurrentUser.CountryCode)
            {
                response = UpdateUser();
            }

            if (response > 0)
            {
                Message message = new Message(Messages.InfoProfileUpdatedMessage);
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
            Message message = new Message("InfoUpdatePasswordConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
            message.ShowDialog();

            try
            {
                if (message.UserDialogResult == Message.CustomDialogResult.Confirm)
                {
                    UserManagementService.UserManagementClient userManagementClient = new UserManagementService.UserManagementClient();
                    response = userManagementClient.UpdatePassword(oldPassword, newPassword, CurrentSession.CurrentUser.Email);
                }
            }
            catch (FaultException<UserManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }

            return response;
        }

        private TransferUser GetTransferUser() 
        {
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();

            user.IdUser = CurrentSession.CurrentUser.IdUser;
            user.Username = userNameTextBox.Text;
            user.Email = emailTextBox.Text;
            user.CountryCode = countryComboBox.SelectedValue.ToString();
            return user;
        }

        private int UpdateUser()
        {
            int result = 0;
            Message message = new Message("InfoUpdateProfileConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
            message.ShowDialog();

            try
            {
                if (message.UserDialogResult == Message.CustomDialogResult.Confirm)
                {
                    UserManagementService.UserManagementClient userManagementClient = new UserManagementService.UserManagementClient();
                    TransferUser newUser = GetTransferUser();
                    result = userManagementClient.UpdateUser(newUser);
                    CurrentSession.CurrentUser = newUser;
                }
            }
            catch (FaultException<UserManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
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
            userNameTextBox.IsEnabled = isEditable;
            countryComboBox.IsEnabled = isEditable;
            emailTextBox.IsEnabled = isEditable;
        }

        private void ChangeProfilePictureButtonIsPressed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg)|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                Message confirmationMessage = new Message("InfoUpdateProfilePictureConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
                confirmationMessage.ShowDialog();
                if (confirmationMessage.UserDialogResult == Message.CustomDialogResult.Confirm)
                {
                    string filePath = openFileDialog.FileName;
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    if (fileBytes.Length <= MaxLongProfilePicture)
                    {
                        try
                        {
                            UserManagementService.UserManagementClient userManagementClient = new UserManagementService.UserManagementClient();
                            userManagementClient.ChangeUserProfilePicture(CurrentSession.CurrentUser.IdUser, fileBytes);
                            CurrentSession.ProfilePicture = ProfilePictureManager.ByteArrayToBitmapImage(fileBytes);
                            userProfilePictureImage.Source = CurrentSession.ProfilePicture;
                            MainMenu.GetInstance();
                            ChangeToEditMode(false);
                        }
                        catch (IOException)
                        {
                            Message message = new Message(Messages.FailHandleProfilePictureMessage);
                            message.ShowDialog();
                        }
                    }
                    else
                    {
                        Message message = new Message(Messages.FailTooLongProfilePictureMessage);
                        message.ShowDialog();
                    }
                }
            }
        }

        public static string EncryptPassword(string password)
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

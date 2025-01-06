using LabyrinthClient.UserManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.ServiceModel;
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
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using System.Globalization;
using LabyrinthClient.Properties;
using System.ServiceModel.Channels;
using HelperClasses;
using LabyrinthClient.Session;
using System.Collections.Specialized;
using System.Threading;
using LabyrinthClient.UserProfilePictureManagementService;
using System.Windows.Markup;
using System.Net.NetworkInformation;

namespace LabyrinthClient
{
    public partial class MainWindow : Window
    {

        private string _passwordForShow;
        public MainWindow()
        {
           
            CultureInfo ui_culture = new CultureInfo("en-US");
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = ui_culture;
            Thread.CurrentThread.CurrentCulture = culture;
            InitializeComponent();
        }

        public MainWindow(string languajeCode)
        {
            CultureInfo ui_culture = new CultureInfo(languajeCode);
            CultureInfo culture = new CultureInfo(languajeCode);
            Thread.CurrentThread.CurrentUICulture = ui_culture;
            Thread.CurrentThread.CurrentCulture = culture;
            InitializeComponent();
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

            CountryCombobox.ItemsSource = countries;
            CountryCombobox.DisplayMemberPath = "CountryName"; 
            CountryCombobox.SelectedValuePath = "CountryCode"; 
        }

        private void LoginButtonIsPressed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FieldValidator.IsValidEmail(EmailForLoginTextBox.Text) //&& FieldValidator.IsValidPassword(PasswordForLoginTextBox.Password)
                    ) 
                {
                    UserManagementService.UserManagementClient usermanagementClient = new UserManagementService.UserManagementClient();
                    UserManagementService.TransferUser user;

                    user = usermanagementClient.VerificateUser(EmailForLoginTextBox.Text, EncryptPassword(PasswordForLoginTextBox.Password));
                    if (user != null && user.IdUser > 0)
                    {
                        CurrentSession.CurrentUser = user;
                        MainMenu mainMenu= MainMenu.GetInstance();
                        if (mainMenu.JoinGame())
                        {
                            mainMenu.Show();
                            this.Close();
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                ExceptionHandler.HandleValidationException(ex);
            }
            catch (FaultException<UserManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (FaultException<MenuManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException ex)
            {
                ExceptionHandler.HandleFailConnectionToServer(ex.Message);
            }
            catch (CommunicationException ex)
            {
                ExceptionHandler.HandleFailConnectionToServer(ex.Message);
            }
        }

        private void SignupButtonIsPressed(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (VerificationCodeTextBox.IsVisible)
                {
                    if (!string.IsNullOrEmpty(VerificationCodeTextBox.Text))
                    {
                        UserManagementService.UserManagementClient userClient = new UserManagementService.UserManagementClient();
                        if (userClient.VerificateCode(EmailTextbox.Text, VerificationCodeTextBox.Text))
                        {
                            AddUser();
                            ChangeToVerificationMode(false);
                        }
                    }
                }
                else
                {
                    if (FieldValidator.IsValidUsername(UsernameTextbox.Text) && FieldValidator.IsValidPassword(PasswordBox.Password) && FieldValidator.IsValidEmail(EmailTextbox.Text) && CountryCombobox.SelectedItem != null)
                    {
                        UserManagementService.UserManagementClient userClient = new UserManagementService.UserManagementClient();
                        if (userClient.AddVerificationCode(EmailTextbox.Text, UsernameTextbox.Text) > 0)
                        {
                            Message message = new Message(Messages.InfoVerificationCodeSentMessage);
                            message.ShowDialog();
                            ChangeToVerificationMode(true);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                ExceptionHandler.HandleValidationException(ex);
            }
            catch (FaultException<LabyrinthException> ex)
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

        private void ChangeToVerificationMode(bool isActive)
        {
            if (isActive)
            {
                EmailTextbox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Collapsed;
                UsernameTextbox.Visibility = Visibility.Collapsed;
                CountryCombobox.Visibility = Visibility.Collapsed;
                VerificationCodeTextBox.Visibility = Visibility.Visible;
                ShowPasswordForSignup.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmailTextbox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                UsernameTextbox.Visibility = Visibility.Visible;
                CountryCombobox.Visibility = Visibility.Visible;
                VerificationCodeTextBox.Visibility = Visibility.Collapsed;
                ShowPasswordForSignup.Visibility = Visibility.Visible;
            }
        }

        private void AddUser()
        {
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();
            UserManagementService.UserManagementClient userClient = new UserManagementService.UserManagementClient();
            
            string password = "";
            user.CountryCode = CountryCombobox.SelectedValue.ToString();
            user.Username = UsernameTextbox.Text;
            user.Email = EmailTextbox.Text;
            password = EncryptPassword(PasswordBox.Password);

           
            if (userClient.AddUser(user, password) > 0)
            {
                CleanFields();
                Message message = new Message(Messages.InfoUserRegisteredMessage);
                message.ShowDialog();
                ChangeToVerificationMode(false);
            }
        }

        private void CleanFields()
        {
            EmailTextbox.Clear();
            PasswordBox.Clear();
            UsernameTextbox.Clear();
            CountryCombobox.SelectedItem = null;
            VerificationCodeTextBox.Clear();
            VerificationCodeTextBox.Clear();
        }
        private void ExitButtonIsPressed(Object sender, RoutedEventArgs e)
        {
            Message message = new Message("InfoExitConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
            message.Owner = this;
            message.ShowDialog();
            if (message.UserDialogResult == Message.CustomDialogResult.Confirm)
            {
                this.Close();
            }
        }

        private void JoinAsGuestButtonIsPressed(object sender, RoutedEventArgs e)
        {
            Lobby lobbyForGuest = null;

            try
            {
                if (FieldValidator.IsValidLobbyCode(LobbyTextBox.Text) && FieldValidator.IsValidUsername(UserNameForJoinAsGuestTextBox.Text))
                {
                    lobbyForGuest = new Lobby();
                    lobbyForGuest.JoinAsGuest(LobbyTextBox.Text, UserNameForJoinAsGuestTextBox.Text);
                    lobbyForGuest.Show();
                    this.Close();
                }
            }
            catch (ArgumentException ex)
            {
                ExceptionHandler.HandleValidationException(ex);
            }
            catch (FaultException<ChatService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
                lobbyForGuest?.Close();
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

        private void LanguageButtonIsPressed(object sender, RoutedEventArgs e)
        {
            string languageCode;
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;

            if (currentCulture == "es-ES")
            {
                languageCode = "en-US";
            }
            else
            {
                languageCode = "es-ES";
            }
            
            MainWindow newWindow = new MainWindow(languageCode);
            newWindow.Show();
            this.Close();
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

        private void ChangeToJoinAsGuestIsPressed(object sender, RoutedEventArgs e)
        {

            ChangeToSignupButton.FontWeight = FontWeights.Regular;
            ChangeToLoginButton.FontWeight = FontWeights.Regular;
            ChangeToJoinAsGuest.FontWeight = FontWeights.Bold;
            ChangeToJoinAsGuestMode(true);
            ChangeToLoginMode(false);
            ChangeToSignupMode(false);
        }
        private void ChangeToLoginIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeToSignupButton.FontWeight = FontWeights.Regular;
            ChangeToJoinAsGuest.FontWeight = FontWeights.Regular;
            ChangeToLoginButton.FontWeight = FontWeights.Bold;
            ChangeToJoinAsGuestMode(false);
            ChangeToLoginMode(true);
            ChangeToSignupMode(false);

        }
        private void ChangeToSignupIsPressed(object sender, RoutedEventArgs e)
        {
            LoadCountriesFromResources();
            ChangeToJoinAsGuest.FontWeight = FontWeights.Regular;
            ChangeToLoginButton.FontWeight = FontWeights.Regular;
            ChangeToSignupButton.FontWeight = FontWeights.Bold;
            ChangeToJoinAsGuestMode(false);
            ChangeToLoginMode(false);
            ChangeToSignupMode(true);

        }

        private void ChangeToLoginMode(bool isEnabled)
        {
            if (isEnabled)
            {
                EmailForLoginTextBox.Visibility = Visibility.Visible;
                PasswordForLoginTextBox.Visibility = Visibility.Visible;
                LoginButton.Visibility = Visibility.Visible;
                ShowPasswordForLogin.Visibility = Visibility.Visible;
            } else
            {
                EmailForLoginTextBox.Visibility = Visibility.Collapsed;
                PasswordForLoginTextBox.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Collapsed;
                ShowPasswordForLogin.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeToSignupMode(bool isEnabled)
        {
            if (isEnabled)
            {
                UsernameTextbox.Visibility = Visibility.Visible;
                EmailTextbox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                CountryCombobox.Visibility = Visibility.Visible;
                SignupButton.Visibility = Visibility.Visible;
                ShowPasswordForSignup.Visibility = Visibility.Visible;
            } else
            {
                UsernameTextbox.Visibility = Visibility.Collapsed;
                EmailTextbox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Collapsed;
                CountryCombobox.Visibility = Visibility.Collapsed;
                SignupButton.Visibility = Visibility.Collapsed;
                ShowPasswordForSignup.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeToJoinAsGuestMode (bool isEnabled)
        {
            if (isEnabled)
            {
                LobbyTextBox.Visibility = Visibility.Visible;
                UserNameForJoinAsGuestTextBox.Visibility = Visibility.Visible;
                JoinAsGuestButton.Visibility = Visibility.Visible;
            } else
            {
                LobbyTextBox.Visibility = Visibility.Collapsed;
                UserNameForJoinAsGuestTextBox.Visibility= Visibility.Collapsed;
                JoinAsGuestButton.Visibility = Visibility.Collapsed;
            }
        }
     

        private void PasswordIsChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;  
            var password = passwordBox.Password;    

            if (string.IsNullOrEmpty(password))
            {
                passwordBox.Tag = Properties.Resources.GlobalPasswordTextBoxPlaceholder;
            }
            else
            {
                passwordBox.Tag = "";
            }
        }

        private void ShowPasswordIsPressed(object sender, RoutedEventArgs e)
        {
            if (PasswordForLoginTextBox.IsVisible)
            {
                if (!string.IsNullOrEmpty(PasswordForLoginTextBox.Password))
                {
                    _passwordForShow = PasswordForLoginTextBox.Password;
                    PasswordForLoginTextBox.Password = "";

                    PasswordForLoginTextBox.Tag = _passwordForShow;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    _passwordForShow = PasswordBox.Password;
                    PasswordBox.Password = "";

                    PasswordBox.Tag = _passwordForShow;
                }
            }
        }

        private void HidePasswordIsPressed(object sender, RoutedEventArgs e)
        {
            if (PasswordForLoginTextBox.IsVisible)
            {
                if (string.IsNullOrEmpty(PasswordForLoginTextBox.Password))
                {
                    PasswordForLoginTextBox.Password = _passwordForShow;
                }
            } 
            else
            {
                if (string.IsNullOrEmpty(PasswordBox.Password))
                {
                    PasswordBox.Password = _passwordForShow;
                }
            }
            _passwordForShow = "";
        }

    }
    public static class ExceptionHandler
    {
        public static Message.CustomDialogResult HandleLabyrinthException(string messageCode)
        {
            Message message = new Message(Messages.ResourceManager.GetString(messageCode));
            message.ShowDialog();
            return message.UserDialogResult;
        }

        public static Message.CustomDialogResult HandleValidationException(ArgumentException exception)
        {
            Message message = new Message(Messages.ResourceManager.GetString(exception.Message));
            message.ShowDialog();
            return message.UserDialogResult;
        }

        public static Message.CustomDialogResult HandleFailConnectionToServer(string messageText)
        {
            Message message = new Message(messageText);
            message.ShowDialog();
            return message.UserDialogResult;
        }

        public static Message.CustomDialogResult HandleCommunicationException()
        {
            Message message = new Message("FailServerCommunication");
            message.ShowDialog();
            return message.UserDialogResult;
        }
    }
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            return string.IsNullOrWhiteSpace(text) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            return string.IsNullOrWhiteSpace(text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

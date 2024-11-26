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
using System.Windows.Forms;
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

namespace LabyrinthClient
{
    public partial class MainWindow : Window, UserManagementService.IUserManagementCallback
    {
        private UserManagementService.UserManagementClient client;
        public MainWindow()
        {
            DataContext = this;
            InstanceContext context = new InstanceContext(this);
            client = new UserManagementService.UserManagementClient(context);
            InitializeComponent();
            LoadCountries();
            CultureInfo ui_culture = new CultureInfo("en-US");
            CultureInfo culture = new CultureInfo("en-US");

            Thread.CurrentThread.CurrentUICulture = ui_culture;
            Thread.CurrentThread.CurrentCulture = culture;

        }

        private void LoadCountries()
        {
            try
            {
                CatalogManagementService.CatalogManagementClient client = new CatalogManagementService.CatalogManagementClient();
                var countries = client.GetAllCountries();
                CountryCombobox.ItemsSource = countries;
                CountryCombobox.DisplayMemberPath = "CountryName";
                CountryCombobox.SelectedValuePath = "CountryId";
            }
            catch (FaultException<LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
        }
        private void LoginButtonIsPressed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FieldValidator.IsValidEmail(emailForLoginTextBox.Text) //&& FieldValidator.IsValidPassword(passwordForLoginTextBox.Password)
                    ) 
                {
                    UserManagementService.TransferUser user = new UserManagementService.TransferUser();

                    user = client.VerificateUser(emailForLoginTextBox.Text, EncryptPassword(passwordForLoginTextBox.Password));
                    if (user.IdUser > 0)
                    {
                        CurrentSession.CurrentUser = user;
                        MainMenu.GetInstance().Show();
                        this.Close();
                    }
                }
            }
            catch (ArgumentException ex)
            {
                ExceptionHandler.HandleException(ex);
            }
            catch (FaultException<LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            
        }

        private void SignupButtonIsPressed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (verificationCodeTextBox.IsVisible)
                {
                    if (!string.IsNullOrEmpty(verificationCodeTextBox.Text))
                    {
                        if (client.VerificateCode(EmailTextbox.Text, verificationCodeTextBox.Text))
                        {
                            AddUser();
                            ChangeToVerificationMode(false);
                        }
                    }
                }
                else
                {
                    if (FieldValidator.IsValidUsername(UsernameTextbox.Text) && FieldValidator.IsValidPassword(PasswordBox.Password) && FieldValidator.IsValidEmail(EmailTextbox.Text))
                    {
                        if (client.AddVerificationCode(EmailTextbox.Text, UsernameTextbox.Text) > 0)
                        {
                            Message message = new Message("InfoVerificationCodeSentMessage");
                            ChangeToVerificationMode(true);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                ExceptionHandler.HandleException(ex);
            }
            catch (FaultException<LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
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
                verificationCodeTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                EmailTextbox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                UsernameTextbox.Visibility = Visibility.Visible;
                CountryCombobox.Visibility = Visibility.Visible;
                verificationCodeTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void AddUser()
        {
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();
            string password = "";

            user.Username = UsernameTextbox.Text;
            user.Email = EmailTextbox.Text;
            password = EncryptPassword(PasswordBox.Password);

            user.Country = (int)CountryCombobox.SelectedValue;
           
            if (client.AddUser(user, password) > 0)
            {
                Message message = new Message("InfoUserRegisteredMessage");
                verificationCodeTextBox.Clear();
                ChangeToVerificationMode(false);
            }
        }
        private void ExitButtonIsPressed(Object sender, RoutedEventArgs e)
        {
            Message message = new Message("InfoExitConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);
            message.ShowDialog();
            if (message.UserDialogResult == Message.DialogResult.Confirm)
            {
                this.Close();
            }
        }
        
        private void JoinAsGuestButtonIsPressed(Object sender, RoutedEventArgs e)
        {
            Lobby lobbyForGuest = new Lobby();
            lobbyForGuest.IsRegistered = false;
            lobbyForGuest.JoinAsGuest(lobbyTextBox.Text,userNameForJoinAsGuestTextBox.Text);
            lobbyForGuest.Show();
            this.Close();
        }


        private void LanguageButtonIsPressed(object sender, RoutedEventArgs e)
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;

            if (currentCulture == "es-ES")
            {
                CultureInfo ui_culture = new CultureInfo("en-US");
                CultureInfo culture = new CultureInfo("en-US");

                Thread.CurrentThread.CurrentUICulture = ui_culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
            else
            {
                CultureInfo ui_culture = new CultureInfo("es-ES");
                CultureInfo culture = new CultureInfo("es-ES");

                Thread.CurrentThread.CurrentUICulture = ui_culture;
                Thread.CurrentThread.CurrentCulture = culture;
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

        private void ChangeToJoinAsGuestIsPressed(object sender, RoutedEventArgs e)
        {

            changeToSignupButton.FontWeight = FontWeights.Regular;
            changeToLoginButton.FontWeight = FontWeights.Regular;
            changeToJoinAsGuest.FontWeight = FontWeights.Bold;
            ChangeToJoinAsGuestMode(true);
            ChangeToLoginMode(false);
            ChangeToSignupMode(false);
        }
        private void ChangeToLoginIsPressed(object sender, RoutedEventArgs e)
        {
            changeToSignupButton.FontWeight = FontWeights.Regular;
            changeToJoinAsGuest.FontWeight = FontWeights.Regular;
            changeToLoginButton.FontWeight = FontWeights.Bold;
            ChangeToJoinAsGuestMode(false);
            ChangeToLoginMode(true);
            ChangeToSignupMode(false);

        }
        private void ChangeToSignupIsPressed(object sender, RoutedEventArgs e)
        {
            changeToJoinAsGuest.FontWeight = FontWeights.Regular;
            changeToLoginButton.FontWeight = FontWeights.Regular;
            changeToSignupButton.FontWeight = FontWeights.Bold;
            ChangeToJoinAsGuestMode(false);
            ChangeToLoginMode(false);
            ChangeToSignupMode(true);

        }

        private void ChangeToLoginMode(bool isEnabled)
        {
            if (isEnabled)
            {
                emailForLoginTextBox.Visibility = Visibility.Visible;
                passwordForLoginTextBox.Visibility = Visibility.Visible;
                LoginButton.Visibility = Visibility.Visible;
            } else
            {
                emailForLoginTextBox.Visibility = Visibility.Collapsed;
                passwordForLoginTextBox.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Collapsed;
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
                signupButton.Visibility = Visibility.Visible;
            } else
            {
                UsernameTextbox.Visibility = Visibility.Collapsed;
                EmailTextbox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Collapsed;
                CountryCombobox.Visibility = Visibility.Collapsed;
                signupButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeToJoinAsGuestMode (bool isEnabled)
        {
            if (isEnabled)
            {
                lobbyTextBox.Visibility = Visibility.Visible;
                userNameForJoinAsGuestTextBox.Visibility = Visibility.Visible;
                joinAsGuestButton.Visibility = Visibility.Visible;
            } else
            {
                lobbyTextBox.Visibility = Visibility.Collapsed;
                userNameForJoinAsGuestTextBox.Visibility= Visibility.Collapsed;
                joinAsGuestButton.Visibility = Visibility.Collapsed;
            }
        }

        public void ReceiveProfilePicture(int userId, byte[] dataImage)
        {
        }
    }

    public class ExceptionHandler
    {
        public static Message.DialogResult HandleLabyrinthException(string messageText)
        {
            Message message = new Message(messageText);
            message.ShowDialog();
            return message.UserDialogResult;
        }

        public static Message.DialogResult HandleException(Exception ex)
        {
            Message message = new Message(ex.Message);
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
            throw new NotImplementedException();
        }
    }

    


}

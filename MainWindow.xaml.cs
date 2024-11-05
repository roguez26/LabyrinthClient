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

namespace LabyrinthClient
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            LoadCountries();
            AddWatermarkToTextBox(UsernameTextbox, Properties.Resources.GlobalUsernameTextBoxPlaceholder);
            AddWatermarkToTextBox(EmailTextbox, Properties.Resources.GlobalEmailTextBoxPlaceholder);
            AddWatermarkToTextBox(emailForLoginTextBox, Properties.Resources.GlobalEmailTextBoxPlaceholder);
            AddWatermarkToTextBox(userNameForJoinAsGuestTextBox, Properties.Resources.GlobalUsernameTextBoxPlaceholder);
            AddWatermarkToTextBox(lobbyTextBox, Properties.Resources.GlobalLobbyIdTextBoxPlaceholder);
        }

        private void LoadCountries()
        {
            try
            {
                CatalogManagementService.CatalogManagementClient client = new CatalogManagementService.CatalogManagementClient();
                var countries = client.getAllCountries();
                CountryCombobox.ItemsSource = countries;
                Console.WriteLine(countries);
                CountryCombobox.DisplayMemberPath = "CountryName";
                CountryCombobox.SelectedValuePath = "CountryId";
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error al cargar países: {exception.Message}");
            }
        }

        private void SignupButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserManagementService.UserManagementClient client = new UserManagementService.UserManagementClient();
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();
            int response = 0;

            if (string.IsNullOrEmpty(verificationCodeTextBox.Text))
            {
                ChangeToVerificationMode(true);
                response = client.addVerificationCode(EmailTextbox.Text);
                switch (response)
                {
                    case 0:; break;
                    case 1: ShowMessage("", "InfoVerificationCodeMessage"); break;
                    case -1:; break;
                }
            }
            else
            {
                if (client.verificateCode(EmailTextbox.Text, verificationCodeTextBox.Text))
                {
                    AddUser();
                }
                else
                {
                    ShowMessage("", "FailIncorrectVerificationCodeMessage");
                }
            }
        }

        private void ChangeToVerificationMode(Boolean isActive)
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
            UserManagementService.UserManagementClient client = new UserManagementService.UserManagementClient();
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();

            user.Username = UsernameTextbox.Text;
            user.Email = EmailTextbox.Text;
            user.Password = EncryptPassword(PasswordBox.Password);

            user.Country = (int)CountryCombobox.SelectedValue;
            if (client.addUser(user) > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Se ha completado su registro, ya puede iniciar sesion", "Registro completado");
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Ha habido un error en su registro, revise su conexion e intente mas tarde", "No se pudo completar su registro");
            }
        }
        private void ExitButtonIsPressed(Object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("¿Esta seguro de que desea salir", "Confirmacion de salida", MessageBoxButtons.YesNo);
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void LanguageButtonIsPressed(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("ToDo", "Cambiar lenguaje");
        }

        private void AddWatermarkToTextBox(TextBox textBox, string watermarkText)
        {
            textBox.Text = watermarkText;
            textBox.Foreground = Brushes.Gray;

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == watermarkText)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = watermarkText;
                    textBox.Foreground = Brushes.Gray;
                }
            };
        }

        private void LoginButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserManagementService.UserManagementClient client = new UserManagementService.UserManagementClient();
            UserManagementService.TransferUser user = new UserManagementService.TransferUser();

            user.Email = emailForLoginTextBox.Text;
            user.Password = EncryptPassword(passwordForLoginTextBox.Text);

            user = client.userVerification(user);

            if (string.IsNullOrEmpty(user.ErrorCode))
            {
                MainMenu mainMenu = MainMenu.GetInstance(user);
                mainMenu.Show();
                this.Close();
            }
            else
            {
                ShowMessage("FailLoginErrorTitle", user.ErrorCode);
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

        private void ShowMessage(string errorTitleCode, string errorMessageCode)
        {
            string message = Messages.ResourceManager.GetString(errorMessageCode);
            string title = Messages.ResourceManager.GetString(errorTitleCode);
            MessageBox.Show(message, title);
        }


    }
}

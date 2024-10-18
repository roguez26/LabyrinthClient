using System;
using System.Collections.Generic;
using System.Linq;
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
                var countries= client.getAllCountries();
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

            user.Username = UsernameTextbox.Text;
            user.Email = EmailTextbox.Text;
            user.Password = PasswordBox.Password;
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
    }
}

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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            LoadCountries();
        }
        private void LoadCountries()
        {
            try
            {

                CatalogManagementService.CatalogManagementClient client = new CatalogManagementService.CatalogManagementClient();
                var countries = client.getAllCountries();

                CountryCombobox.ItemsSource = countries;
                CountryCombobox.DisplayMemberPath = "name";
                CountryCombobox.SelectedValuePath = "idCountry";
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error al cargar países: {exception.Message}");
            }
        }
        private void SignupButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserManagementService.UserManagementClient client = new UserManagementService.UserManagementClient();
            UserManagementService.User user = new UserManagementService.User();   
            user.Username = UsernameTextbox.Text;
            user.Email = UsernameTextbox.Text;
            user.Password = UsernameTextbox.Text;
            user.Country = 1;
            int check = client.addUser(user);           

        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

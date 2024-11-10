using LabyrinthClient.LobbyManagementService;
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;
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
using System.Windows.Shapes;

namespace LabyrinthClient
{

    public partial class LobbySelection : Window
    {
     
        public LobbySelection()
        {
            InitializeComponent();
            InstanceContext context = new InstanceContext(this);
        }

        private void JoinButtonIsPressed(object sender, RoutedEventArgs e)
        {
            
            PlayerLobby playerLobby = new PlayerLobby();
            playerLobby.Show();
            if (playerLobby.JoinToLobby(lobbyCodeTextBox.Text))
            {
                this.Close();
                MainMenu.GetInstance().Close();
            }
            
           
            
        }

        private void CancelButtonIsPressed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       
    }
}
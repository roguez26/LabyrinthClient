using LabyrinthClient.LobbyManagementService;
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
    /// <summary>
    /// Lógica de interacción para LobbySelection.xaml
    /// </summary>
    public partial class LobbySelection : Window, LobbyManagementService.ILobbyManagementServiceCallback
    {
        private LobbyManagementService.LobbyManagementServiceClient lobbyManagementServiceClient;
        private TransferUser currentSession;
        public LobbySelection(TransferUser user)
        {
            InitializeComponent();
            InstanceContext context = new InstanceContext(this);
            lobbyManagementServiceClient = new LobbyManagementService.LobbyManagementServiceClient(context);
            currentSession = user;
        }

        private void JoinButtonIsPressed(object sender, RoutedEventArgs e)
        {
            lobbyManagementServiceClient.joinToGame(lobbyCodeTextBox.Text, currentSession.Username);

            PlayerLobby playerLobby = new PlayerLobby(currentSession);
            playerLobby.Show();
            this.Close();
            MainMenu.GetInstance(currentSession).Close();

        }

        private void CancelButtonIsPressed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void BroadcastCreated(string message)
        {
        
        }

        public void BroadcastJoined(string userName)
        {
            Dispatcher.Invoke(() =>
            {

                
            });
        }
    }
}

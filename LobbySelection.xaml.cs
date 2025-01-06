using HelperClasses;
using LabyrinthClient.LobbyManagementService;
using LabyrinthClient.Properties;
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
        }

        private void JoinButtonIsPressed(object sender, RoutedEventArgs e)
        {
            Lobby playerLobby = null;
            try
            {
                if (FieldValidator.IsValidLobbyCode(lobbyCodeTextBox.Text))
                {
                    playerLobby = new Lobby();
                    playerLobby.JoinToLobby(lobbyCodeTextBox.Text);
                    playerLobby.Show();
                    this.Close();
                    MainMenu.GetInstance().Close();
                }
            } catch (ArgumentException ex) {
                ExceptionHandler.HandleValidationException(ex);
            }
            catch (FaultException<ChatService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
                playerLobby?.Close();
            }
            catch (CommunicationObjectFaultedException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                playerLobby?.Close();
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                playerLobby?.Close();
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                playerLobby?.Close();
            }
        }

        private void CancelButtonIsPressed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       
    }
}
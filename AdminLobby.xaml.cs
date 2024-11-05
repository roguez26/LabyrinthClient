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
    /// Lógica de interacción para AdminLobby.xaml
    /// </summary>
    public partial class AdminLobby : Window, ChatService.IChatServiceCallback, LobbyManagementService.ILobbyManagementServiceCallback
    {
        private TransferUser _currentSession;

        private ChatService.ChatServiceClient chatServiceClient;
        private LobbyManagementService.LobbyManagementServiceClient lobbyManagementServiceClient;
        public AdminLobby(TransferUser user)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InstanceContext context = new InstanceContext(this);
            lobbyManagementServiceClient = new LobbyManagementService.LobbyManagementServiceClient(context);
            chatServiceClient = new ChatService.ChatServiceClient(context);
            chatServiceClient.Start();
            _currentSession = user;

            try
            {
                
                    lobbyManagementServiceClient.createLobby();
                
                   
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
        }

        public void BroadcastCreated(string message)
        {
            Dispatcher.Invoke(() =>
            {
                lobbyCodeLabel.Content = message;
                MessagesListBox.Items.Add("Lobby created");
            });
        }

        public void BroadcastJoined(string userName)
        {
            Dispatcher.Invoke(() =>
            {
                MessagesListBox.Items.Add(userName + " has joined to the game");
            });
        }

        public void BroadcastMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                MessagesListBox.Items.Add(message);
            });
        }

        private void SendButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MessageTextBox.Text))
            {
                try
                {
                    if (chatServiceClient != null && chatServiceClient.State != CommunicationState.Faulted)
                    {
                        String message = "<" + _currentSession.Username + "> " + MessageTextBox.Text;
                        chatServiceClient.SendMessage(message);
                        MessageTextBox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("El cliente no está conectado al servicio.");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Error al enviar el mensaje: {exception.Message}");
                }
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (chatServiceClient != null)
            {
                if (chatServiceClient.State != CommunicationState.Closed)
                    chatServiceClient.Close();
            }
        }

        private void BackButtonIsPressed(object sender, RoutedEventArgs e)
        {
            MainMenu mainMenu = MainMenu.GetInstance(_currentSession);
            mainMenu.Show();
            this.Close();
        }
    }
}

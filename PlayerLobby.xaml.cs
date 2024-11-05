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
    /// Lógica de interacción para PlayerLobby.xaml
    /// </summary>
    public partial class PlayerLobby : Window, ChatService.IChatServiceCallback
    {
        private TransferUser _currentSession;

        private ChatService.ChatServiceClient chatServiceClient;
        public PlayerLobby(TransferUser user)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InstanceContext context = new InstanceContext(this);
            chatServiceClient = new ChatService.ChatServiceClient(context);
            chatServiceClient.Start();
            _currentSession = user;
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

        private void WindowClosed(object sender, System.EventArgs e)
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

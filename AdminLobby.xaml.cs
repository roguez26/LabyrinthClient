using LabyrinthClient.LobbyManagementService;
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    
    public partial class AdminLobby : Window, ChatService.IChatServiceCallback, LobbyManagementService.ILobbyManagementServiceCallback
    {
        public ObservableCollection<PlayerItem> items { get; set; } = new ObservableCollection<PlayerItem>();

        private static AdminLobby _instance;
        private CharacterSelection _characterSelection;

        private ChatService.ChatServiceClient _chatServiceClient;
        private LobbyManagementService.LobbyManagementServiceClient _lobbyManagementServiceClient;
        private AdminLobby()
        {
            DataContext = this;
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InstanceContext context = new InstanceContext(this);
            _lobbyManagementServiceClient = new LobbyManagementService.LobbyManagementServiceClient(context);
            _chatServiceClient = new ChatService.ChatServiceClient(context);
            _chatServiceClient.Start();

            try
            {
                lobbyCodeLabel.Text = _lobbyManagementServiceClient.CreateLobby(new LabyrinthClient.LobbyManagementService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                    
                    Email = CurrentSession.CurrentUser.Email,
                    Country = CurrentSession.CurrentUser.Country,
                    ErrorCode = CurrentSession.CurrentUser.ErrorCode,
                    ProfilePicture = CurrentSession.CurrentUser.ProfilePicture,
                    TransferCountry = new LabyrinthClient.LobbyManagementService.TransferCountry
                    {
                        CountryName = CurrentSession.CurrentUser.TransferCountry.CountryName,
                        CountryId = CurrentSession.CurrentUser.TransferCountry.CountryId
                    }
                });
                items.Add(new PlayerItem { username = CurrentSession.CurrentUser.Username, idUser = CurrentSession.CurrentUser.IdUser, isFriend = true, isCurrentUser = true});

            }
            catch (Exception exception)
            { 
                Console.WriteLine(exception.Message); 
            }
            
        }

        public static AdminLobby GetInstance()
        {
            if (_instance == null || !_instance.IsVisible)
            {
                _instance = new AdminLobby();
                _instance.Activate();
            }
            else
            {
               // _instance.UpdateData();
            }

            return _instance;
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
                    if (_chatServiceClient != null && _chatServiceClient.State != CommunicationState.Faulted)
                    {
                        string message = "<" + CurrentSession.CurrentUser.Username + "> " + MessageTextBox.Text;
                        _chatServiceClient.SendMessage(message);
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

        private void BackButtonIsPressed(object sender, RoutedEventArgs e)
        {
            MainMenu.GetInstance().Show();
            this.Hide();
            this.Close();
        }

        private void AddFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int response = 0;
            FriendsManagementService.FriendsManagementServiceClient client = new FriendsManagementService.FriendsManagementServiceClient();
            Button button = sender as Button;

            if (button != null)
            {
                var newFriend = button.DataContext as PlayerItem; 

                if (newFriend != null)
                {
                    int friendId = newFriend.idUser; 
                    response = client.SendFriendRequest(CurrentSession.CurrentUser.IdUser, friendId);
                }
            }

            if (response > 0)
            {
                MessageBox.Show("Se envio la solicitud");
            }

        }
        private void InviteFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {

        }

        private void KickOutButtonIsPressed(object sender, RoutedEventArgs e)
        {

        }

        private void SelectCharacterButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (_characterSelection == null)
            {
                _characterSelection = new CharacterSelection();
                _characterSelection.Closed += (s, args) => _characterSelection = null; 
                _characterSelection.Show();
            }
            else
            {
                _characterSelection.Activate();
            }
        }

        public void NotifyUserHasJoined(LobbyManagementService.TransferUser user)
        {
            Dispatcher.Invoke(() =>
            {

                items.Add(new PlayerItem { username = user.Username, idUser = user.IdUser, isFriend = false, isCurrentUser = false });
                MessagesListBox.Items.Add(user.Username + " has joined to the game");
            });
        }

        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        

    }

    public class PlayerItem
    {
        public int idUser {  get; set; }
        public string username {  get; set; }
        public bool isFriend { get; set; }
        public bool isCurrentUser { get; set; }
    }
}

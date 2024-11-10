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
using LabyrinthClient.Session;
using System.Collections.ObjectModel;
using LabyrinthClient.LobbyManagementService;

namespace LabyrinthClient
{

    public partial class PlayerLobby : Window, ChatService.IChatServiceCallback, LobbyManagementService.ILobbyManagementServiceCallback
    {
        private LobbyManagementService.LobbyManagementServiceClient _lobbyManagementServiceClient;
        private CharacterSelection _characterSelection;

        public ObservableCollection<PlayerItem> items { get; set; } = new ObservableCollection<PlayerItem>();

        private ChatService.ChatServiceClient chatServiceClient;
        public PlayerLobby()
        {
            InitializeComponent();
        }

        public bool JoinToLobby(string lobbyCode)
        {
            bool result = false;
            LobbyManagementService.TransferUser[] lobbyMembers;
            DataContext = this;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InstanceContext context = new InstanceContext(this);
            _lobbyManagementServiceClient = new LobbyManagementService.LobbyManagementServiceClient(context);
            lobbyMembers = _lobbyManagementServiceClient.JoinToGame(lobbyCode, new LabyrinthClient.LobbyManagementService.TransferUser
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

            if (FillMembersList(lobbyMembers))
            {
                result = true;
                chatServiceClient = new ChatService.ChatServiceClient(context);
                chatServiceClient.Start();
                lobbyCodeLabel.Content = lobbyCode;
                MessagesListBox.Items.Add("You have joined to the game");
            } 
            else
            {
                this.Close();
                MainMenu.GetInstance().Show();
            }
            return result;
            
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
                        String message = "<" + CurrentSession.CurrentUser.Username + "> " + MessageTextBox.Text;
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
            MainMenu mainMenu = MainMenu.GetInstance();
            mainMenu.Show();
            this.Close();
        }
        private void InviteFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {
        }
        private void AddFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {
        }
        public void NotifyUserHasJoined(LobbyManagementService.TransferUser user)
        {
            
            Dispatcher.Invoke(() =>
            {
                string message = "";

                if (user.IdUser == CurrentSession.CurrentUser.IdUser)
                {
                    message = "You have joined to the game";
                }
                else
                {
                    message = user.Username + " has joined to the game";
                    items.Add(new PlayerItem { username = user.Username, idUser = user.IdUser, isCurrentUser = (user.IdUser == CurrentSession.CurrentUser.IdUser), isFriend = false });

                }
                MessagesListBox.Items.Add(message);

            });
            
        }

        public bool FillMembersList(LobbyManagementService.TransferUser[] members)
        {
            bool result = members.Length > 0;

            
            if (members.Length > 0){ 
                Dispatcher.Invoke(() =>
                {
                    items.Clear();
                    foreach (LobbyManagementService.TransferUser member in members)
                    {
                        items.Add(new PlayerItem { username = member.Username, idUser = member.IdUser, isCurrentUser = (member.IdUser == CurrentSession.CurrentUser.IdUser), isFriend = false });
                    }
                });
            }
            return result;
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


    }


}
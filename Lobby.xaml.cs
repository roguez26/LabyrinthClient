using HelperClasses;
using LabyrinthClient.ChatService;
using LabyrinthClient.LobbyManagementService;
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    
    public partial class Lobby : Window, ChatService.IChatServiceCallback, LobbyManagementService.ILobbyManagementServiceCallback, UserManagementService.IUserManagementCallback, MenuManagementService.IMenuManagementServiceCallback
    {
        public ObservableCollection<PlayerItem> Items { get; set; } = new ObservableCollection<PlayerItem>();
        public bool IsAdmin { get; set; } = false;
        public bool IsRegistered { get; set; } = true;

        private static Lobby _instance;
        private CharacterSelection _characterSelection;

        private ChatService.ChatServiceClient _chatServiceClient;
        private LobbyManagementService.LobbyManagementServiceClient _lobbyManagementServiceClient;
        private UserManagementService.UserManagementClient _userManagementServiceClient;

        public Lobby()
        {
            DataContext = this;
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InstanceContext context = new InstanceContext(this);
            _lobbyManagementServiceClient = new LobbyManagementService.LobbyManagementServiceClient(context);
            _chatServiceClient = new ChatService.ChatServiceClient(context);
            _userManagementServiceClient = new UserManagementClient(context);
        }

        public void HostGame()
        {
            try
            {
                lobbyCodeLabel.Text = _lobbyManagementServiceClient.CreateLobby(new LabyrinthClient.LobbyManagementService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                    ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                });
                _chatServiceClient.Start();
                Items.Add(new PlayerItem
                {
                    Username = CurrentSession.CurrentUser.Username,
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    ProfilePicture = CurrentSession.ProfilePicture,
                    IsCurrentUser = true,
                    IsFriend = true, IsAdmin = true, IsRegistered = true
                });
            }
            catch (FaultException<LobbyManagementService.LabyrinthException> exception)
            {
                ExceptionHandler.HandleLabyrinthException(exception.Detail.ErrorCode);
            }
        }

        public void JoinToLobby(string lobbyCode)
        {
            try
            {
                _lobbyManagementServiceClient.JoinToGame(lobbyCode, new LobbyManagementService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                    ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                });
                lobbyCodeLabel.Text = lobbyCode;
                _chatServiceClient.Start();
            }
            catch (FaultException<LobbyManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
        }

        public void JoinAsGuest(string lobbyCode, string username)
        {
            try
            {
                _lobbyManagementServiceClient.JoinToGame(lobbyCode, new LobbyManagementService.TransferUser
                {
                    Username = username,
                });
                CurrentSession.CurrentUser.Username = username;
                lobbyCodeLabel.Text = lobbyCode;
                _chatServiceClient.Start();
            }
            catch (FaultException<LobbyManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
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
            this.Close();
        }

        private void LeaveLobby()
        {
            _lobbyManagementServiceClient.RemoveUserFromLobby(lobbyCodeLabel.Text, new LabyrinthClient.LobbyManagementService.TransferUser
            {
                IdUser = CurrentSession.CurrentUser.IdUser,
                Username = CurrentSession.CurrentUser.Username,
                ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
            });
            if (IsRegistered)
            {
                MainMenu.GetInstance().Show();
            } else
            {
                MainWindow mainWindow = new MainWindow();   
                mainWindow.Show();
            }
           
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
                    int friendId = newFriend.IdUser;

                    try
                    {
                        response = client.SendFriendRequest(CurrentSession.CurrentUser.IdUser, friendId);
                        button.Visibility = Visibility.Collapsed;
                    } 
                    catch (FaultException <FriendsManagementService.LabyrinthException> ex)
                    {
                        ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
                    }

                }
            }

            if (response > 0)
            {
                
                Message message = new Message("InfoSentFriendRequestMessage");
            }

        }
        private void InviteFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {

        }

        private void KickOutButtonIsPressed(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                var member = button.DataContext as PlayerItem;

                if (member != null)
                {
                    try
                    {
                        _lobbyManagementServiceClient.RemoveUserFromLobby(lobbyCodeLabel.Text, new LabyrinthClient.LobbyManagementService.TransferUser
                        {
                            IdUser = member.IdUser,
                            Username = member.Username,
                        });
                    }
                    catch (FaultException<FriendsManagementService.LabyrinthException> ex)
                    {
                        ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
                    }
                }
            }
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


        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LeaveLobby();
        }

        public void NotifyUserHasJoined(LobbyManagementService.TransferUser user)
        {
            Dispatcher.Invoke(() =>
            {
                if (user.IdUser > 0 && user.IdUser == CurrentSession.CurrentUser.IdUser)
                {
                    MessagesListBox.Items.Add("You have joined the game");
                }
                else
                {
                    MessagesListBox.Items.Add(user.Username + " has joined the game");
                }
            });
        }
        public void FillMembersList(LobbyManagementService.TransferUser[] members)
        {
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();
          
            if (members != null && members.Length != 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Items.Clear();
                    foreach (LobbyManagementService.TransferUser member in members)
                    {
                        Items.Add(new PlayerItem
                        {
                            Username = member.Username ?? string.Empty, // Si Username es null, asigna una cadena vacía
                            IdUser = member.IdUser, // Si IdUser es null, asigna 0 como valor predeterminado
                            IsCurrentUser = member.IdUser == CurrentSession.CurrentUser.IdUser,
                            IsFriend = friendsManagement.IsFriend(CurrentSession.CurrentUser.IdUser, member.IdUser) && IsRegistered == true,
                            IsAdmin = IsAdmin, // Asumiendo que IsAdmin ya es un booleano seguro

                        });
                        _userManagementServiceClient.GetUserProfilePicture(member.IdUser, member.ProfilePicture);
                    }
                });
            }
        }

        public void GestMembersList(LobbyManagementService.TransferUser[] members)
        {
            if (members != null && members.Length != 0)
            {
                FillMembersList(members);
            } 
            else
            {
                Message message = new Message("FailLobbyNotFoundMessage");

                MainMenu mainMenu = MainMenu.GetInstance();
                mainMenu.Show();

                message.Owner = mainMenu;

                this.Close();

                message.ShowDialog();

            }
        }

        public void ReceiveProfilePicture(int userId, byte[] dataImage)
        {
            if (dataImage != null)
            {
                BitmapImage profilePicture = ProfilePictureManager.ByteArrayToBitmapImage(dataImage);

                var playerItem = Items.FirstOrDefault(item => item.IdUser == userId);
                if (playerItem != null)
                {
                    playerItem.ProfilePicture = profilePicture;
                }
            }
        }

        public void NotifyUserHasLeft(LobbyManagementService.TransferUser user)
        {
            Dispatcher.Invoke(() =>
            {
                    MessagesListBox.Items.Add(user.Username + " has left the game");

            });
        }

        public void KickOutPlayer(LobbyManagementService.TransferUser user)
        {
            this.Close();
        }

        public void AttendInvitation(string lobbyCode)
        {
        }

    }

    public class PlayerItem : INotifyPropertyChanged
    {
        private ImageSource _profilePicture;
        public int IdUser { get; set; }
        public string Username { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool IsFriend { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsRegistered { get; set; }

        public  string ProfilePicturePath { get; set; }
        public ImageSource ProfilePicture
        {
            get => _profilePicture;
            set
            {
                _profilePicture = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

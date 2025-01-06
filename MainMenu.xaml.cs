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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelperClasses;
using LabyrinthClient.Properties;
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;

namespace LabyrinthClient
{
    public partial class MainMenu : Window, UserProfilePictureManagementService.IUserProfilePictureManagementServiceCallback, MenuManagementService.IMenuManagementServiceCallback
    {

        private static MainMenu _instance;
        private MyUser _myUser;
        private LobbySelection _lobbySelection;
        public ObservableCollection<PlayerItem> Items { get; set; } = new ObservableCollection<PlayerItem>();
        public ObservableCollection<TopPlayerItem> TopPlayersItems { get; set; } = new ObservableCollection<TopPlayerItem>();
        public ObservableCollection<FriendRequestItem> RequestItems { get; set; } = new ObservableCollection<FriendRequestItem>();
        private UserProfilePictureManagementService.UserProfilePictureManagementServiceClient _userManagementServiceClient;
        private MenuManagementService.MenuManagementServiceClient _menuManagementClient;
        private InstanceContext _instanceContext;

        private MainMenu()
        {
            DataContext = this;
            _instanceContext = new InstanceContext(this);
            InitializeComponent();
            InitializeClient();
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                LoadUserData();
                FillTopPlayersList(GetTopPlayersList());
                FillFriendsList(GetFriendsList());
            }
            catch (FaultException<UserManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (FaultException<FriendsManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
        }
        private void InitializeClient()
        {
            if (_userManagementServiceClient != null)
            {
                try
                {
                    _userManagementServiceClient.Close();
                }
                catch (CommunicationException)
                {
                    _userManagementServiceClient.Abort();
                }
            }
            if (_menuManagementClient != null)
            {
                try
                {
                    _menuManagementClient.Close();
                }
                catch (CommunicationException)
                {
                    _menuManagementClient.Abort();
                }
            }
            _userManagementServiceClient = new UserProfilePictureManagementService.UserProfilePictureManagementServiceClient(_instanceContext);
            _menuManagementClient = new MenuManagementService.MenuManagementServiceClient(_instanceContext);
        }

        public void UpdateCallback()
        {
            try
            {
                if (_menuManagementClient.State == CommunicationState.Closed || _menuManagementClient.State == CommunicationState.Faulted)
                {
                    InitializeClient();
                }
                _menuManagementClient.UpdateCallback(new MenuManagementService.TransferUser { IdUser = CurrentSession.CurrentUser.IdUser });
            }
            catch (FaultException<MenuManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
        }

        public bool JoinGame()
        {
            try
            {
                if (_menuManagementClient.State == CommunicationState.Closed || _menuManagementClient.State == CommunicationState.Faulted)
                {
                    InitializeClient();
                }
                _menuManagementClient.Start(new MenuManagementService.TransferUser { 
                    IdUser = CurrentSession.CurrentUser.IdUser, 
                    Username = CurrentSession.CurrentUser.Username });
            }
            catch 
            {
                CurrentSession.Logout();
                this.Close();
                throw;
            }
            return true;
        }

        private void LoadUserData()
        {
            userButton.Content = CurrentSession.CurrentUser.Username;
            if (CurrentSession.ProfilePicture == null)
            {
                try
                {
                    if (_userManagementServiceClient.State == CommunicationState.Closed || _userManagementServiceClient.State == CommunicationState.Faulted)
                    {
                        InitializeClient();
                    }
                    _userManagementServiceClient.GetUserProfilePicture(CurrentSession.CurrentUser.IdUser, CurrentSession.CurrentUser.ProfilePicture);
                }
                catch 
                {
                    throw;
                }
            }
            else
            {
                userProfilePictureImage.Source = CurrentSession.ProfilePicture;
            }
        }

        private void UpdateUserData()
        {
            if (CurrentSession.ProfilePicture != null)
            {
                userProfilePictureImage.Source = CurrentSession.ProfilePicture;
            }

            if (CurrentSession.CurrentUser != null)
            {
                userButton.Content = CurrentSession.CurrentUser.Username;
            }
        }


        public static MainMenu GetInstance()
        {
            if (_instance == null || !_instance.IsVisible)
            {
                _instance = new MainMenu();
                _instance.Activate();
            }
            else
            {
                _instance.UpdateUserData();
            }
            return _instance;
        }        

        private void HostGameButtonIsPressed(object sender, RoutedEventArgs e)
        {
            Lobby adminLobby = new Lobby();
            try
            {
                adminLobby.HostGame();
                adminLobby.Show();
                this.Close();
            }
            catch (FaultException<LobbyManagementService.LabyrinthException> exception)
            {
                ExceptionHandler.HandleLabyrinthException(exception.Detail.ErrorCode);
            }
            catch (FaultException<MenuManagementService.LabyrinthException> exception)
            {
                ExceptionHandler.HandleLabyrinthException(exception.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
        }

        private void JoinGameButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (_lobbySelection == null)
            {
                _lobbySelection = new LobbySelection();
                _lobbySelection.Closed += (s, args) => _lobbySelection = null;
                _lobbySelection.Show();
            }
            else
            {
                _lobbySelection.Activate();
            }
        }

        private void UserButtonIsPressed(object sender, RoutedEventArgs e)
        {
            _myUser = new MyUser();
            
            _myUser.ShowDialog();
        }

        private void ShowFriendsButtonIsPressed(object sender, RoutedEventArgs e)
        {
            DynamicRequestsListView.Visibility = Visibility.Collapsed;
            DynamicFriendsListView.Visibility = Visibility.Visible; 
            try
            {
                FillFriendsList(GetFriendsList());
            }
            catch (FaultException<FriendsManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
        }

        private void ShowFriendRequestButtonIsPressed(object sender, RoutedEventArgs e)
        {
            DynamicRequestsListView.Visibility = Visibility.Visible;
            DynamicFriendsListView.Visibility = Visibility.Collapsed;
            try
            {
                FillFriendRequestsList(GetFriendsRequests());
            }
            catch (FaultException<FriendsManagementService.LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
        }

        private void AcceptButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int result = 0;
            Message confirmationMessage = new Message("InfoAcceptRequestConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);

            confirmationMessage.ShowDialog();
            if (confirmationMessage.UserDialogResult == Message.CustomDialogResult.Confirm)
            {
                result = AttendRequest(FriendsManagementService.FriendRequestStatus.Accepted, sender);
                if (result > 0)
                {
                    Message message = new Message(Messages.InfoRequestAcceptedMessage);
                    message.ShowDialog();
                }
            }
        }

        private int AttendRequest(FriendsManagementService.FriendRequestStatus status, object sender)
        {
            int response = 0;
            FriendsManagementService.FriendsManagementServiceClient client = new FriendsManagementService.FriendsManagementServiceClient();
            Button button = sender as Button;
            var newFriend = new FriendRequestItem();

            if (button != null)
            {
                newFriend = button.DataContext as FriendRequestItem;

                if (newFriend != null)
                {
                    int friendId = newFriend.FriendRequestId;

                    try
                    {
                        response = client.AttendFriendRequest(friendId, status);
                        RequestItems.Remove(newFriend);
                    }
                    catch (FaultException<FriendsManagementService.LabyrinthException> ex)
                    {
                        ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
                    }
                    catch (EndpointNotFoundException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                    }
                    catch (CommunicationException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                    }
                }
            }
            return response;
        }
        private void RejectButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int result = 0;
            Message confirmationMessage = new Message("InfoRejectRequestConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);

            confirmationMessage.ShowDialog();
            if (confirmationMessage.UserDialogResult == Message.CustomDialogResult.Confirm)
            {
                result = AttendRequest(FriendsManagementService.FriendRequestStatus.Rejected, sender);
                if (result > 0)
                {
                    Message message = new Message(Messages.InfoRequestRejectedMessage);
                    message.ShowDialog();
                }
            }
        }

        private static FriendsManagementService.TransferUser[] GetFriendsList()
        {
            FriendsManagementService.TransferUser[] list = new FriendsManagementService.TransferUser[0];
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();

            try
            {
                list = friendsManagement.GetMyFriendsList(CurrentSession.CurrentUser.IdUser);
            } 
            catch 
            {
                throw;
            }
            return list;
        }

        private static FriendsManagementService.TransferFriendRequest[] GetFriendsRequests()
        {
            FriendsManagementService.TransferFriendRequest[] list = new FriendsManagementService.TransferFriendRequest[0];
            FriendsManagementService.FriendsManagementServiceClient client = new FriendsManagementService.FriendsManagementServiceClient();

            try
            {
                list = client.GetFriendRequestsList(CurrentSession.CurrentUser.IdUser);
            }
            catch 
            {
                throw;
            }
            return list;
        }

        private void FillFriendsList(FriendsManagementService.TransferUser[] members)
        {
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();

            if (members.Length > 0)
            {
                Items.Clear();
                try
                {
                    foreach (FriendsManagementService.TransferUser member in members)
                    {
                        Items.Add(new PlayerItem
                        {
                            Username = member.Username,
                            IdUser = member.IdUser,
                            IsCurrentUser = true,
                            IsFriend = friendsManagement.IsFriend(CurrentSession.CurrentUser.IdUser, member.IdUser),
                            ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png"))
                        });
                        GetUserProfilePicture(member.IdUser, member.ProfilePicture);
                    }
                }
                catch 
                {
                    throw;
                }
            }
        }

        private void GetUserProfilePicture(int userId, string path)
        {
            if (_userManagementServiceClient.State == CommunicationState.Closed || _userManagementServiceClient.State == CommunicationState.Faulted)
            {
                InitializeClient();
            }

            try
            {
                _userManagementServiceClient.GetUserProfilePicture(userId, path);
            }
            catch 
            {
                throw;
            }
        }

        private static UserManagementService.TransferUser[] GetTopPlayersList()
        {
            UserManagementService.TransferUser[] playersList = new UserManagementService.TransferUser[0];
            UserManagementService.UserManagementClient userClient = new UserManagementService.UserManagementClient();

            try
            {
                playersList = userClient.GetRanking();
            }
            catch 
            {
                throw;
            }
            return playersList;
        }

        private void FillTopPlayersList(UserManagementService.TransferUser[] members)
        {
            TopPlayersItems.Clear();

            if (members == null || members.Length == 0) { 
                return; 
            }

            foreach (UserManagementService.TransferUser member in members)
            {
                TopPlayersItems.Add(new TopPlayerItem
                {
                    IdUser = member.IdUser,
                    Username = member.Username,
                    GamesWon = "🏆" + member.TransferStats.GamesWon,
                    ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png"))
                });
                GetUserProfilePicture(member.IdUser, member.ProfilePicture);
            }
        }

        private void FillFriendRequestsList(FriendsManagementService.TransferFriendRequest[] friendsRequest)
        {
            RequestItems.Clear();

            if (friendsRequest == null  || friendsRequest.Length == 0)
            {
                return;
            }
            
            foreach (FriendsManagementService.TransferFriendRequest request in  friendsRequest)
            {
                RequestItems.Add(new FriendRequestItem { FriendRequestId = request.IdFriendRequest, 
                    Username = request.Requester.Username, 
                    UserId = request.Requester.IdUser, 
                    ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png")) });
                GetUserProfilePicture(request.Requester.IdUser, request.Requester.ProfilePicture);
            }
        }

        public void ReceiveProfilePicture(int userId, byte[] dataImage)
        {
            if (dataImage == null)
            {
                return;
            }
            
            BitmapImage profilePicture = ProfilePictureManager.ByteArrayToBitmapImage(dataImage);

            var topPlayerItem = TopPlayersItems.FirstOrDefault(item => item.IdUser == userId);
            if (topPlayerItem != null)
            {
                topPlayerItem.ProfilePicture = profilePicture;
            }

            if (userId == CurrentSession.CurrentUser.IdUser)
            {
                userProfilePictureImage.Source = profilePicture;
                CurrentSession.ProfilePicture = profilePicture;
            }
            else
            {
                var playerItem = Items.FirstOrDefault(item => item.IdUser == userId);
                if (playerItem != null)
                {
                    playerItem.ProfilePicture = profilePicture;
                }
                var friendRequestItem = RequestItems.FirstOrDefault(item => item.UserId == userId);
                if (friendRequestItem != null)
                {
                    friendRequestItem.ProfilePicture = profilePicture;
                }
            }
        }

        public void AttendInvitation(MenuManagementService.TransferUser inviter,  string lobbyCode)
        {
            Message confirmationMessage = new Message("InfoAttendInvitationConfirmationMessage", inviter.Username, Properties.Resources.YesButton, Properties.Resources.NoButton);
            confirmationMessage.Owner = this;
            confirmationMessage.ShowDialog();
            if (confirmationMessage.UserDialogResult == Message.CustomDialogResult.Confirm)
            {
                Lobby playerLobby = new Lobby();
                try
                {
                    if (FieldValidator.IsValidLobbyCode(lobbyCode))
                    {
                        playerLobby = new Lobby();
                        playerLobby.JoinToLobby(lobbyCode);
                        playerLobby.Show();
                        this.Close();
                        MainMenu.GetInstance().Close();
                    } 
                }
                catch (ArgumentException ex)
                {
                    ExceptionHandler.HandleValidationException(ex);
                }
                catch (FaultException<ChatService.LabyrinthException> ex)
                {
                    ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
                    playerLobby.Close();
                }
                catch (EndpointNotFoundException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                }
                catch (CommunicationException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                }
            }
        }
    }

    public class FriendRequestItem : INotifyPropertyChanged
    {
        private ImageSource _profilePicture;
        public int UserId { get; set; }
        public int FriendRequestId { get; set; }
        public string Username { get; set; }

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

    public class TopPlayerItem : INotifyPropertyChanged
    {
        private ImageSource _profilePicture;
        public int IdUser { get; set; }
        public string Username { get; set; }

        public string GamesWon { get; set; }
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

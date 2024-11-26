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
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;

namespace LabyrinthClient
{
    public partial class MainMenu : Window, UserManagementService.IUserManagementCallback
    {

        private static MainMenu _instance;
        private MyUser _myUser;
        private LobbySelection _lobbySelection;
        public ObservableCollection<PlayerItem> Items { get; set; } = new ObservableCollection<PlayerItem>();
        public ObservableCollection<TopPlayerItem> TopPlayersItems { get; set; } = new ObservableCollection<TopPlayerItem>();
        public ObservableCollection<FriendRequestItem> RequestItems { get; set; } = new ObservableCollection<FriendRequestItem>();

        private UserManagementService.UserManagementClient _userManagementClient;

        private MainMenu()
        {
            DataContext = this;
            InstanceContext context = new InstanceContext(this);
            _userManagementClient = new UserManagementService.UserManagementClient(context);
            InitializeComponent();
            userButton.Content = CurrentSession.CurrentUser.Username;
            FillTopPlayersList(GetTopPlayersList());

            if (CurrentSession.ProfilePicture == null)
            {
                _userManagementClient.GetUserProfilePicture(CurrentSession.CurrentUser.IdUser, CurrentSession.CurrentUser.ProfilePicture);
            }
            else
            {
                userProfilePictureImage.Source = CurrentSession.ProfilePicture;
            }
            FillFriendsList(GetFriendsList());
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
            } else
            {
                _instance.UpdateUserData();
            }
            
            return _instance;
        }        

        private void HostGameButtonIsPressed(object sender, RoutedEventArgs e)
        {
            Lobby adminLobby = new Lobby();
            adminLobby.HostGame();
            adminLobby.IsAdmin = true;
            adminLobby.Show();
            this.Close();
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
            if (_myUser == null)
            {
                _myUser = new MyUser();
                _myUser.Closed += (s, args) => _myUser = null;
                _myUser.Show();
            }
            else
            {
                _myUser.Activate();
            }
        }

        private void ShowFriendsButtonIsPressed(object sender, RoutedEventArgs e)
        {
            DynamicRequestsListView.Visibility = Visibility.Collapsed;
            DynamicFriendsListView.Visibility = Visibility.Visible; 
            FillFriendsList(GetFriendsList());
        }

        private void ShowFriendRequestButtonIsPressed(object sender, RoutedEventArgs e)
        {
            DynamicRequestsListView.Visibility = Visibility.Visible;
            DynamicFriendsListView.Visibility = Visibility.Collapsed;
            FillFriendRequestsList(GetFriendsRequests());
        }

        private void AcceptButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int result = 0;
            Message confirmationMessage = new Message("InfoAcceptRequestConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);

            confirmationMessage.ShowDialog();
            if (confirmationMessage.UserDialogResult == Message.DialogResult.Confirm)
            {
                result = AttendRequest(FriendsManagementService.FriendRequestStatus.Accepted, sender);
                if (result > 0)
                {
                    Message message = new Message("InfoRequestAcceptedMessage");
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
                }
            }
            return response;
        }
        private void RejectButtonIsPressed(object sender, RoutedEventArgs e)
        {
            int result = 0;
            Message confirmationMessage = new Message("InfoRejectRequestConfirmationMessage", Properties.Resources.YesButton, Properties.Resources.NoButton);

            confirmationMessage.ShowDialog();
            if (confirmationMessage.UserDialogResult == Message.DialogResult.Confirm)
            {
                result = AttendRequest(FriendsManagementService.FriendRequestStatus.Rejected, sender);
                if (result > 0)
                {
                    Message message = new Message("InfoRequestRejectedMessage");
                    message.ShowDialog();
                }
            }
        }



        private FriendsManagementService.TransferUser[] GetFriendsList()
        {
            FriendsManagementService.TransferUser[] list = new FriendsManagementService.TransferUser[0];
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();

            try
            {
                list = friendsManagement.GetMyFriendsList(CurrentSession.CurrentUser.IdUser);
            } 
            catch(FaultException<LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            return list;
        }

        private FriendsManagementService.TransferFriendRequest[] GetFriendsRequests()
        {
            FriendsManagementService.TransferFriendRequest[] list = new FriendsManagementService.TransferFriendRequest[0];
            FriendsManagementService.FriendsManagementServiceClient client = new FriendsManagementService.FriendsManagementServiceClient();

            try
            {
                list = client.GetFriendRequestsList(CurrentSession.CurrentUser.IdUser);
            }
            catch (FaultException<LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            return list;
        }

        private bool FillFriendsList(FriendsManagementService.TransferUser[] members)
        {
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();
            bool result = members.Length > 0;

            if (members.Length > 0)
            {
                Items.Clear();
                foreach (FriendsManagementService.TransferUser member in members)
                {
                    Items.Add(new PlayerItem { Username = member.Username, IdUser = member.IdUser, IsCurrentUser = true, IsFriend = friendsManagement.IsFriend(CurrentSession.CurrentUser.IdUser, member.IdUser),
                        ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png"))});
                   _userManagementClient.GetUserProfilePicture(member.IdUser, member.ProfilePicture);
                }
            }
            return result;
        }

        private UserManagementService.TransferUser[] GetTopPlayersList()
        {
            UserManagementService.TransferUser[] list = new UserManagementService.TransferUser[0];

            try
            {
                list = _userManagementClient.GetRanking();
            }
            catch (FaultException<LabyrinthException> ex)
            {
                ExceptionHandler.HandleLabyrinthException(ex.Detail.ErrorCode);
            }
            return list;
        }

        private bool FillTopPlayersList(UserManagementService.TransferUser[] members)
        {
            bool result = members.Length > 0;

            if (members.Length > 0)
            {
                TopPlayersItems.Clear();
                foreach (UserManagementService.TransferUser member in members)
                {
                    TopPlayersItems.Add(new TopPlayerItem
                    {
                        IdUser = member.IdUser,
                        Username = member.Username,
                        GamesWon = "Games won: " + member.TransferStats.GamesWon,
                        ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png"))
                    });
                    _userManagementClient.GetUserProfilePicture(member.IdUser, member.ProfilePicture);
                }
            }
            return result;
        }

        private bool FillFriendRequestsList(FriendsManagementService.TransferFriendRequest[] friendsRequest)
        {
            bool result = friendsRequest.Length > 0;

            if (result)
            {
                RequestItems.Clear();
                foreach (FriendsManagementService.TransferFriendRequest request in  friendsRequest)
                {
                    RequestItems.Add(new FriendRequestItem { FriendRequestId = request.IdFriendRequest, Username = request.Requester.Username, 
                        UserId = request.Requester.IdUser, ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png")) });
                    _userManagementClient.GetUserProfilePicture(request.Requester.IdUser, request.Requester.ProfilePicture);
                }
            }
            return true;
        }

        public void ReceiveProfilePicture(int userId, byte[] dataImage)
        {
            if (dataImage != null)
            {
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

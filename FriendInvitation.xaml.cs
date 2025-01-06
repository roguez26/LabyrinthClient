using HelperClasses;
using LabyrinthClient.FriendsManagementService;
using LabyrinthClient.Properties;
using LabyrinthClient.Session;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
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

    public partial class FriendInvitation : Window, UserProfilePictureManagementService.IUserProfilePictureManagementServiceCallback, MenuManagementService.IMenuManagementServiceCallback
    {
        public ObservableCollection<PlayerItem> FriendItems { get; set; } = new ObservableCollection<PlayerItem>();
        private UserProfilePictureManagementService.UserProfilePictureManagementServiceClient _userManagementServiceClient;
        private MenuManagementService.MenuManagementServiceClient _menuManagementClient;
        private string _lobbyCode;
        private InstanceContext _instanceContext;

        public enum FriendsInvitationDialogResult
        {
            OK
        }

        public FriendsInvitationDialogResult UserDialogResult { get; private set; }

        public FriendInvitation(string lobbyCode)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            DataContext = this;
            _instanceContext = new InstanceContext(this);
            _lobbyCode = lobbyCode;
            InitializeClient();
            UpdateCallback();
            FillFriendsList(GetFriendsList());
        }

        private void UpdateCallback()
        {
            try
            {
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

        public void SendInvitationButtonIsFriend(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var friend = button.DataContext as PlayerItem;

            if (button == null || friend == null)
            {
                return;
            }
            var playerItem = Lobby.Items.FirstOrDefault(item => item.IdUser == friend.IdUser);
            if (playerItem == null)
            {
                _menuManagementClient.InviteFriend(new MenuManagementService.TransferUser { 
                    IdUser = CurrentSession.CurrentUser.IdUser, 
                    Username = CurrentSession.CurrentUser.Username }, 
                    new MenuManagementService.TransferUser { 
                        IdUser = friend.IdUser }, 
                    _lobbyCode);
                button.Visibility = Visibility.Collapsed;
            } 
            else
            {
                Message message = new Message(Messages.FailPlayerInGameMessage);
                message.Owner = this;
                message.ShowDialog();
            }
        }

        private void CloseButtonIsPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserDialogResult = FriendsInvitationDialogResult.OK;
        }

        public void ReceiveProfilePicture(int userId, byte[] dataImage)
        {
            if (dataImage != null)
            {
                BitmapImage profilePicture = ProfilePictureManager.ByteArrayToBitmapImage(dataImage);
                var listFriend = FriendItems.FirstOrDefault(friend => friend.IdUser == userId);
                if (listFriend != null)
                {
                    listFriend.ProfilePicture = profilePicture;
                }
            }
        }

        private static FriendsManagementService.TransferUser[] GetFriendsList()
        {
            FriendsManagementService.TransferUser[] friendsList = new FriendsManagementService.TransferUser[0];
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();

            try
            {
                friendsList = friendsManagement.GetMyFriendsList(CurrentSession.CurrentUser.IdUser);
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
            return friendsList;
        }

        private void FillFriendsList(FriendsManagementService.TransferUser[] members)
        {
            FriendsManagementService.FriendsManagementServiceClient friendsManagement = new FriendsManagementService.FriendsManagementServiceClient();

            if (members.Length > 0)
            {
                FriendItems.Clear();
                try
                {
                    foreach (FriendsManagementService.TransferUser member in members)
                    {
                        FriendItems.Add(new PlayerItem
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

        private void GetUserProfilePicture(int userId, string path)
        {
            if (_userManagementServiceClient.State == CommunicationState.Closed || _userManagementServiceClient.State == CommunicationState.Faulted)
            {
                InitializeClient();
            }
            _userManagementServiceClient.GetUserProfilePicture(userId, path);
        }

        public void AttendInvitation(MenuManagementService.TransferUser inviter, string lobbyCode)
        {
            string additionalMessage = inviter.Username + " (" + lobbyCode + ")";
            Message confirmationMessage = new Message("InfoInvitationMessage", additionalMessage);
            confirmationMessage.Owner = this;
            confirmationMessage.ShowDialog();
        }
    }

    public class FriendItem
    {
        private ImageSource _profilePicture;

        public string Username { get; set;  } 
        public int IdUser { get; set;}
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


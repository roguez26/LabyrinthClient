using GameplayClasses;
using HelperClasses;
using LabyrinthClient.CatalogManagementService;
using LabyrinthClient.ChatService;
using LabyrinthClient.GameService;
using LabyrinthClient.LobbyManagementService;
using LabyrinthClient.Properties;
using LabyrinthClient.Session;
using LabyrinthClient.UserManagementService;
using LabyrinthClient.UserProfilePictureManagementService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;
using static LabyrinthClient.FriendInvitation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LabyrinthClient
{
    public partial class Lobby : Window, ChatService.IChatServiceCallback, LobbyManagementService.ILobbyManagementServiceCallback, UserProfilePictureManagementService.IUserProfilePictureManagementServiceCallback, MenuManagementService.IMenuManagementServiceCallback, GameService.IGameServiceCallback
    {
        public static ObservableCollection<PlayerItem> Items { get; set; } = new ObservableCollection<PlayerItem>();
        public bool IsAdmin { get; set; } = false;
        public bool ClassicStyleIsSelected { get; set; } = true;
        private CharacterSelection _characterSelection;
        private FriendInvitation _friendInvitation;
        private ChatService.ChatServiceClient _chatServiceClient;
        private GameService.GameServiceClient _gameServiceClient;
        private LobbyManagementService.LobbyManagementServiceClient _lobbyManagementServiceClient;
        private UserProfilePictureManagementService.UserProfilePictureManagementServiceClient _userManagementServiceClient;
        private MenuManagementService.MenuManagementServiceClient _menuClient;
        private GameplayClasses.GameBoard _gameBoard;
        private (string Direction, int Index) _lastExit;
        private DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        private TimeSpan _initialTime = TimeSpan.FromMinutes(0.75f);
        private bool _isMyTurn = false;

        public Lobby()
        {
            InitializeComponent();
            this.KeyDown += OnKeyDown;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeClient();
            _gameBoard = new GameBoard(ClassicStyleIsSelected);
            InitializeTimer();
        }
         
        private void InitializeClient()
        {
            DataContext = this;
            InstanceContext context = new InstanceContext(this);
            _lobbyManagementServiceClient = new LobbyManagementService.LobbyManagementServiceClient(context);
            _chatServiceClient = new ChatService.ChatServiceClient(context);
            _gameServiceClient = new GameServiceClient(context);
            _userManagementServiceClient = new UserProfilePictureManagementServiceClient(context);
            _menuClient = new MenuManagementService.MenuManagementServiceClient(context);
        }

        public void InitializeTimer()
        {
            _timeRemaining = _initialTime;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += TimerTick;

            UpdateTimerText();
        }

        public void Dispose()
        {
            this.KeyDown -= OnKeyDown;
            _timer.Tick -= TimerTick;
            _timer.Stop();
            _timer = null;
            _gameBoard = null;
            DataContext = null;
            Items.Clear();
            IsAdmin = false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_timeRemaining > TimeSpan.Zero)
            {
                _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerText();
            }
            else
            {
                if (_isMyTurn)
                {
                    ChangeNextTurn();
                }
            }
        }

        private void ChangeNextTurn()
        {
            try
            {
                _gameServiceClient.AsignTurn(lobbyCodeLabel.Text, new GameService.TransferPlayer { Username = CurrentSession.CurrentUser.Username });
                EndTurnButton.Visibility = Visibility.Collapsed;
                _isMyTurn = false;
            }
            catch (CommunicationObjectFaultedException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                LeaveLobby();
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                LeaveLobby();
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                LeaveLobby();
            }
        }

        private void UpdateTimerText()
        {
            TimerTextBlock.Text = _timeRemaining.ToString(@"mm\:ss");
        }

        public void HostGame()
        {
            IsAdmin = true;
            try
            {
                _menuClient.UpdateCallback(new MenuManagementService.TransferUser { IdUser = CurrentSession.CurrentUser.IdUser, Username = CurrentSession.CurrentUser.Username });
                lobbyCodeLabel.Text = _lobbyManagementServiceClient.CreateLobby(new LabyrinthClient.LobbyManagementService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                    ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                });
                _chatServiceClient.Start(lobbyCodeLabel.Text, new ChatService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                });
                _gameServiceClient.Start(lobbyCodeLabel.Text, new GameService.TransferPlayer
                {
                    Username = CurrentSession.CurrentUser.Username,
                });
            }
            catch
            {
                this.Close();
                throw;
            }
            AddAdminItem();
            GenerateGameBoard();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_isMyTurn)
            {
                Player player = _gameBoard.GetPlayer(CurrentSession.CurrentUser.Username);
                string direction = e.Key.ToString();

                if (player != null && _gameBoard.ValidateMovement(player.Name, direction))
                {
                    try
                    {
                        _gameServiceClient.MovePlayer(lobbyCodeLabel.Text, player.Name, direction);
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                        LeaveLobby();
                    }
                    catch (EndpointNotFoundException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                        LeaveLobby();
                    }
                    catch (CommunicationException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                        LeaveLobby();
                    }
                }
            }
        }

        private void AddAdminItem()
        {
            Items.Add(new PlayerItem
            {
                Username = CurrentSession.CurrentUser.Username,
                IdUser = CurrentSession.CurrentUser.IdUser,
                ProfilePicture = CurrentSession.ProfilePicture,
                IsCurrentUser = true,
                IsFriend = true,
                IsAdmin = true,
                IsRegistered = true
            });
            Player player = new Player
            {
                Character = new Character("Default"),
                Name = CurrentSession.CurrentUser.Username,
                InactivityCount = 0,
                PlayerId = CurrentSession.CurrentUser.IdUser,
                TreasuresForSearch = new List<string>()
            };
            _gameBoard.AddPlayer(player);
        }

        public void JoinToLobby(string lobbyCode)
        {
            ConfigurationCanvas.Visibility = Visibility.Collapsed;
            lobbyCodeLabel.Text = lobbyCode;

            try
            {
                _chatServiceClient.JoinToChat(lobbyCode, new ChatService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                    ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                });
                _lobbyManagementServiceClient.JoinToGame(lobbyCode, new LobbyManagementService.TransferUser
                {
                    IdUser = CurrentSession.CurrentUser.IdUser,
                    Username = CurrentSession.CurrentUser.Username,
                    ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                });                
                _gameServiceClient.JoinToGame(lobbyCode, new GameService.TransferPlayer
                {
                    Username = CurrentSession.CurrentUser.Username,
                    InactivityCount = 0,
                    SkinPath = "Default",
                });
                _menuClient.UpdateCallback(new MenuManagementService.TransferUser { IdUser = CurrentSession.CurrentUser.IdUser, Username = CurrentSession.CurrentUser.Username });
            }
            catch 
            {
                throw;
            }
        }

        public void JoinAsGuest(string lobbyCode, string username)
        {
            inviteFriendButton.Visibility = Visibility.Collapsed;
            ConfigurationCanvas.Visibility = Visibility.Collapsed;
            CurrentSession.CurrentUser.Username = username;
            lobbyCodeLabel.Text = lobbyCode;
            try
            {
                _chatServiceClient.JoinToChat(lobbyCode, new ChatService.TransferUser
                {
                    Username = username,
                });
                _lobbyManagementServiceClient.JoinToGame(lobbyCode, new LobbyManagementService.TransferUser
                {
                    Username = username,
                });
                _gameServiceClient.JoinToGame(lobbyCode, new GameService.TransferPlayer
                {
                    Username = username,
                    InactivityCount = 0,
                    SkinPath = "Default",
                });
            }
            catch
            {
                throw;
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
                    string message = "<" + CurrentSession.CurrentUser.Username + "> " + MessageTextBox.Text;
                    _chatServiceClient.SendMessage(message, lobbyCodeLabel.Text);
                    MessageTextBox.Clear();
                }
                catch (CommunicationObjectFaultedException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                    LeaveLobby();
                }
                catch (EndpointNotFoundException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                    LeaveLobby();
                }
                catch (CommunicationException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                    LeaveLobby();
                }
            }
        }
        private void BackButtonIsPressed(object sender, RoutedEventArgs e)
        {
            LeaveLobby();
        }

        private void LeaveLobby()
        {
            try
            {
                LeaveServices();
                if (CurrentSession.CurrentUser.IdUser > 0)
                {
                    GoBackToMenuWindow();
                }
                else
                {
                    GoBackToMainWindow();
                }
            }
            catch (CommunicationObjectFaultedException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
            }
            finally
            {
                this.Close();
            }
        }

        private void GoBackToMenuWindow()
        {
            MainMenu mainMenu = MainMenu.GetInstance();
            if (_menuClient.State != CommunicationState.Closed && _menuClient.State != CommunicationState.Faulted)
            {
                mainMenu.UpdateCallback();
            }
            mainMenu.Show();
        }

        private static void GoBackToMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void LeaveServices()
        {
            try
            {
                if (_lobbyManagementServiceClient.State != CommunicationState.Closed && _lobbyManagementServiceClient.State != CommunicationState.Faulted)
                {
                    _lobbyManagementServiceClient.RemoveUserFromLobby(lobbyCodeLabel.Text, new LabyrinthClient.LobbyManagementService.TransferUser
                    {
                        IdUser = CurrentSession.CurrentUser.IdUser,
                        Username = CurrentSession.CurrentUser.Username,
                        ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                    });
                }
                if (_chatServiceClient.State != CommunicationState.Closed && _chatServiceClient.State != CommunicationState.Faulted)
                {
                    _chatServiceClient.RemoveUserFromChat(lobbyCodeLabel.Text, new ChatService.TransferUser
                    {
                        IdUser = CurrentSession.CurrentUser.IdUser,
                        Username = CurrentSession.CurrentUser.Username,
                        ProfilePicture = CurrentSession.CurrentUser.ProfilePicture
                    });
                }
                if (_gameServiceClient.State != CommunicationState.Closed && _gameServiceClient.State != CommunicationState.Faulted)
                {
                    _gameServiceClient.RemoveUserFromGame(lobbyCodeLabel.Text, new GameService.TransferPlayer
                    {
                        Username = CurrentSession.CurrentUser.Username,
                    });
                }
            }
            catch (CommunicationObjectFaultedException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
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

        private void EndTurnButtonIsPressed(object sender, RoutedEventArgs e)
        {
            ChangeNextTurn();
        }

        private void AddFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {
            FriendsManagementService.FriendsManagementServiceClient friendsManagementClient = new FriendsManagementService.FriendsManagementServiceClient();
            Button button = sender as Button;

            if (button != null)
            {
                PlayerItem newFriend = button.DataContext as PlayerItem; 

                if (newFriend != null)
                {
                    int friendId = newFriend.IdUser;

                    try
                    {
                        if (friendsManagementClient.SendFriendRequest(CurrentSession.CurrentUser.IdUser, friendId) > 0) {
                            Message message = new Message(Messages.InfoSentFriendRequestMessage);
                            button.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                        LeaveLobby();
                    }
                    catch (EndpointNotFoundException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                        LeaveLobby();

                    }
                    catch (CommunicationException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                        LeaveLobby();
                    }
                }
            }
        }
        private void InviteFriendButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (_friendInvitation == null || !_friendInvitation.IsVisible)
            {
                _friendInvitation = new FriendInvitation(lobbyCodeLabel.Text);

                if (_friendInvitation.UserDialogResult == FriendsInvitationDialogResult.OK)
                {
                    try
                    {
                        _menuClient.UpdateCallback(new MenuManagementService.TransferUser { IdUser = CurrentSession.CurrentUser.IdUser });
                        _friendInvitation.ShowDialog();
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                        _friendInvitation.Close();
                        LeaveLobby();
                    }
                    catch (EndpointNotFoundException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                        LeaveLobby();
                        _friendInvitation.Close();
                    }
                    catch (CommunicationException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                        LeaveLobby();
                        _friendInvitation.Close();
                    }
                }
            }
        }
        private void KickOutButtonIsPressed(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                PlayerItem member = button.DataContext as PlayerItem;

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
                    catch (CommunicationObjectFaultedException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                        LeaveLobby();
                    }
                    catch (EndpointNotFoundException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                        LeaveLobby();
                    }
                    catch (CommunicationException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                        LeaveLobby();
                    }
                }
            }
        }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Dispose();
        }

        public void NotifyUserHasJoined(LobbyManagementService.TransferUser user)
        {
            if (user == null)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (user.IdUser > 0 && user.IdUser == CurrentSession.CurrentUser.IdUser)
                {
                    MessagesListBox.Items.Add(Messages.YouHaveJoinedTheGameMessage);
                }
                else
                {
                    MessagesListBox.Items.Add(user.Username + " "+ Messages.GuestHasJoinedGameMessage);
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
                            Username = member.Username,
                            IdUser = member.IdUser,
                            IsCurrentUser = member.IdUser == CurrentSession.CurrentUser.IdUser,
                            IsFriend = member.IdUser > 0 && CurrentSession.CurrentUser.IdUser > 0? friendsManagement.IsFriend(CurrentSession.CurrentUser.IdUser, member.IdUser): true,
                            IsRegistered = member.IdUser > 0,
                            IsAdmin = IsAdmin,
                            ProfilePicture = new BitmapImage(new Uri("pack://application:,,,/GraphicItems/userProfilePicture.png"))
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
                Message message = new Message(Messages.FailLobbyNotFoundMessage);

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
                MessagesListBox.Items.Add(user.Username + Messages.GuestHasLeftGame);
            });
        }

        public void KickOutPlayer(LobbyManagementService.TransferUser user)
        {
            LeaveLobby();
            this.Close();
        }

        public void AttendInvitation(MenuManagementService.TransferUser inviter, string lobbyCode)
        {
            string additionalMessage = inviter.Username + " (" + lobbyCode + ")";
            Message confirmationMessage = new Message("InfoInvitationMessage", additionalMessage);
            confirmationMessage.Owner = this;
            confirmationMessage.ShowDialog();
        }
        public void GenerateGameBoard()
        {
            GameBoard newGameBoard = new GameBoard(ClassicStyleIsSelected);
            foreach (Player player in _gameBoard.Players)
            {
                newGameBoard.AddPlayer(player);
            }
            _gameBoard = newGameBoard;

            int rows = _gameBoard.Board.GetLength(0);
            int cols = _gameBoard.Board.GetLength(1);

            var elements = GameBoardGrid.Children.OfType<Image>().ToList();
            foreach (var element in elements)
            {
                GameBoardGrid.Children.Remove(element);
            }
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var currentTile = _gameBoard.Board[row, col];
                    Image tileImage = new Image
                    {
                        Source = currentTile.GetCurrentImage(),
                        Stretch = Stretch.UniformToFill
                    };

                    Grid.SetRow(tileImage, row + 1);
                    Grid.SetColumn(tileImage, col + 1);
                    GameBoardGrid.Children.Add(tileImage);
                    if (currentTile.TreasureOnTile != null && !currentTile.TreasureOnTile.IsFound)
                    {
                        Image treasureImage = new Image
                        {
                            Source = currentTile.TreasureOnTile.ImageTreasure,
                            Width = 30,
                            Height = 30,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center
                        };

                        Grid.SetRow(treasureImage, row + 1);
                        Grid.SetColumn(treasureImage, col + 1);
                        GameBoardGrid.Children.Add(treasureImage);
                    }
                }
            }
            ReorderButton.Visibility = Visibility.Visible;
            ExtraTileImage.Source = _gameBoard.ExtraTile.Image;
            UpdateGameboardAfterMove();
        }

        private void ConvertTransferToGameBoard(TransferGameBoard transferBoard)
        {
            ClassicStyleIsSelected = transferBoard.IsClassicSelected;
            _gameBoard = new GameBoard (transferBoard.IsClassicSelected, transferBoard.TilesPositions);

            foreach (TransferPlayer transferPlayer in transferBoard.Players)
            {
                _gameBoard.AddPlayer(new Player
                {
                    Character = new Character(transferPlayer.SkinPath),
                    Name = transferPlayer.Username,
                    InactivityCount = transferPlayer.InactivityCount,
                    TreasuresForSearch = transferPlayer.TreasuresForSearching.ToList(),
                });
            }
            UpdateGameboardAfterMove();
        }

        private TransferGameBoard GetTransferGameBoard()
        {
            TransferGameBoard board = new TransferGameBoard();
            List<TransferPlayer> players = new List<TransferPlayer>();

            board.MaxTreasures = _gameBoard.MaxTreasures;

            foreach(Player playerOnBoard in _gameBoard.Players)
            {
                players.Add(new TransferPlayer
                {
                    Username = playerOnBoard.Name,
                    InitialPosition = playerOnBoard.InitialPosition,
                    SkinPath = playerOnBoard.Character.Name,
                    InactivityCount = 0,
                    TreasuresForSearching = playerOnBoard.TreasuresForSearch.ToArray(),
                });
            }

            board.Players = players.ToArray();

            int rows = _gameBoard.Board.GetLength(0);
            int cols = _gameBoard.Board.GetLength(1);

            int count = 0;
            int[] tilesPosition = new int[50];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    tilesPosition[count] = _gameBoard.Board[i, j].Type;
                    count++;
                }
            }
            tilesPosition[tilesPosition.Length - 1] = _gameBoard.ExtraTile.Type;
            board.TilesPositions = tilesPosition;
            board.IsClassicSelected = ClassicStyleIsSelected;
            return board;
        }

        private void MoveRowClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string direction = button.Tag.ToString();
            if (_isMyTurn && direction != _gameBoard.MyLastInsert)
            {
                try
                {
                    switch (direction)
                    {
                        case "MoveRow2Right": _gameServiceClient.MoveRow(lobbyCodeLabel.Text, direction, 1, true); break;
                        case "MoveRow2Left": _gameServiceClient.MoveRow(lobbyCodeLabel.Text, direction, 1, false); break;
                        case "MoveRow4Right": _gameServiceClient.MoveRow(lobbyCodeLabel.Text, direction, 3, true); break;
                        case "MoveRow4Left": _gameServiceClient.MoveRow(lobbyCodeLabel.Text, direction, 3, false); break;
                        case "MoveRow6Right": _gameServiceClient.MoveRow(lobbyCodeLabel.Text, direction, 5, true); break;
                        case "MoveRow6Left": _gameServiceClient.MoveRow(lobbyCodeLabel.Text, direction, 5, false); break;
                    }
                    _gameBoard.MyLastInsert = direction;
                }
                catch (CommunicationObjectFaultedException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                    LeaveLobby();
                }
                catch (EndpointNotFoundException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                    LeaveLobby();
                }
                catch (CommunicationException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                    LeaveLobby();
                }
            }
            else
            {
                Message message = new Message(Messages.FailInvalidMoveMessage);
                message.ShowDialog();
            }
        }

        private void MoveColumnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string direction = button.Tag.ToString();
            if (_isMyTurn && direction != _gameBoard.MyLastInsert)
            {
                try
                {
                    switch (direction)
                    {
                        case "MoveColumn2Down": _gameServiceClient.MoveColumn(lobbyCodeLabel.Text, direction, 1, true); break;
                        case "MoveColumn2Up": _gameServiceClient.MoveColumn(lobbyCodeLabel.Text, direction, 1, false); break;
                        case "MoveColumn4Down": _gameServiceClient.MoveColumn(lobbyCodeLabel.Text, direction, 3, true); break;
                        case "MoveColumn4Up": _gameServiceClient.MoveColumn(lobbyCodeLabel.Text, direction, 3, false); break;
                        case "MoveColumn6Down": _gameServiceClient.MoveColumn(lobbyCodeLabel.Text, direction, 5, true); break;
                        case "MoveColumn6Up": _gameServiceClient.MoveColumn(lobbyCodeLabel.Text, direction, 5, false); break;
                    }
                    _gameBoard.MyLastInsert = direction;
                }
                catch (CommunicationObjectFaultedException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                    LeaveLobby();
                }
                catch (EndpointNotFoundException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                    LeaveLobby();

                }
                catch (CommunicationException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                    LeaveLobby();
                }
            }
            else
            {
                Message message = new Message(Messages.FailInvalidMoveMessage);
                message.ShowDialog();
            }
        }

        private void UpdateExtraTileView()
        {
            ExtraTileImage.Source = _gameBoard.ExtraTile.RotatedImage;
        }

        private void UpdateGameboardAfterMove()
        {
            var elements = GameBoardGrid.Children.OfType<Image>().ToList();

            foreach (var element in elements)
            {
                GameBoardGrid.Children.Remove(element);
            }

            int boardSize = _gameBoard.Board.GetLength(0);

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var currentTile = _gameBoard.Board[i, j];
                    Border tileBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.LightGray,
                        Child = new Image
                        {
                            Source = currentTile.GetCurrentImage(),
                            Stretch = Stretch.Fill
                        }
                    };

                    Grid.SetRow(tileBorder, i + 1);
                    Grid.SetColumn(tileBorder, j + 1);
                    GameBoardGrid.Children.Add(tileBorder);
                    if (currentTile.TreasureOnTile != null && !currentTile.TreasureOnTile.IsFound)
                    {
                        Image treasureImage = new Image
                        {
                            Source = currentTile.TreasureOnTile.ImageTreasure,
                            Width = 30,
                            Height = 30,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center
                        };

                        Grid.SetRow(treasureImage, i + 1);
                        Grid.SetColumn(treasureImage, j + 1);
                        GameBoardGrid.Children.Add(treasureImage);
                    }
                    if (currentTile.HasPlayers())
                    {
                        foreach (var player in currentTile.PlayersOnTile)
                        {
                            Image playerImage = new Image
                            {
                                Source = player.Character.ImageCharacter,
                                Width = 35,
                                Height = 35,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center
                            };

                            Grid.SetRow(playerImage, i + 1);
                            Grid.SetColumn(playerImage, j + 1);
                            GameBoardGrid.Children.Add(playerImage);
                        }
                    }
                }
            }
        }

        private void ReorderGameBoard(object sender, RoutedEventArgs e)
        {
            GenerateGameBoard();
        }

        private void RotateExtraFile(object sender, RoutedEventArgs e)
        {
            _gameBoard.ExtraTile.RotateRight();
            ExtraTileImage.Source = _gameBoard.ExtraTile.GetCurrentImage();
            try
            {
                _gameServiceClient.RotateTile(lobbyCodeLabel.Text, CurrentSession.CurrentUser.Username);
            }
            catch (CommunicationObjectFaultedException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                LeaveLobby();
            }
            catch (EndpointNotFoundException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                LeaveLobby();

            }
            catch (CommunicationException)
            {
                ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                LeaveLobby();
            }

        }

        private void ChangeGameBoardSkin(object sender, SelectionChangedEventArgs e)
        {
            if (GameBoardSkinComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string tag = selectedItem.Tag.ToString();
                switch (tag)
                {
                    case "Classic":
                        ClassicStyleIsSelected = true;
                        break;

                    case "Volcano":
                        ClassicStyleIsSelected = false;
                        break;

                    default:
                        break;
                }
            }
            GenerateGameBoard();
        }

        private void StartButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (QuantityTreasuresComboBox.SelectedItem != null)
            {
                _isMyTurn = true;
                int maxTreasures = int.Parse(((ComboBoxItem)QuantityTreasuresComboBox.SelectedItem).Content.ToString());
                _gameBoard.AssignTreasures(maxTreasures);
                ShowExtraTileAndNextTreasure(maxTreasures);
                try
                {
                    _gameServiceClient.SendGameBoardToLobby(lobbyCodeLabel.Text, GetTransferGameBoard());
                    EndTurnButton.Visibility = Visibility.Visible;
                    StartGame();
                }
                catch (CommunicationObjectFaultedException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                    LeaveLobby();
                }
                catch (EndpointNotFoundException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                    LeaveLobby();
                }
                catch (CommunicationException)
                {
                    ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                    LeaveLobby();
                }
            }
        }

        private void ShowExtraTileAndNextTreasure(int maxTreasures)
        {
            _gameBoard.MaxTreasures = maxTreasures;
            TreasuresCountLabel.Content = _gameBoard.GetPlayer(CurrentSession.CurrentUser.Username).TreasuresForSearch.Count + "/" + maxTreasures;
            ExtraTileAndTreasureCanvas.Visibility = Visibility.Visible;
            foreach (Player p in _gameBoard.Players)
            {
                Console.WriteLine(p.Name);

                for (int i = 0; i < maxTreasures; i++)
                {
                    Console.WriteLine(p.TreasuresForSearch[i]);
                }
            }
        }
        private void StartGame()
        {
            ConfigurationCanvas.Visibility = Visibility.Collapsed;
            ReorderButton.Visibility = Visibility.Collapsed;
            ExtraTileButton.IsEnabled = true;
            OverlayImage.Visibility = Visibility.Visible;
            MoveRow2RightButton.Visibility = Visibility.Visible;
            MoveRow2LeftButton.Visibility = Visibility.Visible;
            MoveRow4RightButton.Visibility = Visibility.Visible;
            MoveRow4LeftButton.Visibility = Visibility.Visible;
            MoveRow6RightButton.Visibility = Visibility.Visible;
            MoveRow6LeftButton.Visibility = Visibility.Visible;
            MoveColumn2DownButton.Visibility = Visibility.Visible;
            MoveColumn2UpButton.Visibility = Visibility.Visible;
            MoveColumn4DownButton.Visibility = Visibility.Visible;
            MoveColumn4UpButton.Visibility = Visibility.Visible;
            MoveColumn6DownButton.Visibility = Visibility.Visible;
            MoveColumn6UpButton.Visibility = Visibility.Visible;
            _timer.Start(); 
            NexTraesureImage.Source = _gameBoard.ShowNextTreasure(CurrentSession.CurrentUser.Username);
        }

        private void EndGame()
        {
            _isMyTurn = false;
            if (IsAdmin)
            {
                ConfigurationCanvas.Visibility = Visibility.Visible;
                ReorderButton.Visibility = Visibility.Visible;
            }
            ExtraTileButton.IsEnabled = false;
            OverlayImage.Visibility = Visibility.Hidden;
            MoveRow2RightButton.Visibility = Visibility.Hidden;
            MoveRow2LeftButton.Visibility = Visibility.Hidden;
            MoveRow4RightButton.Visibility = Visibility.Hidden;
            MoveRow4LeftButton.Visibility = Visibility.Hidden;
            MoveRow6RightButton.Visibility = Visibility.Hidden;
            MoveRow6LeftButton.Visibility = Visibility.Hidden;
            MoveColumn2DownButton.Visibility = Visibility.Hidden;
            MoveColumn2UpButton.Visibility = Visibility.Hidden;
            MoveColumn4DownButton.Visibility = Visibility.Hidden;
            MoveColumn4UpButton.Visibility = Visibility.Hidden;
            MoveColumn6DownButton.Visibility = Visibility.Hidden;
            MoveColumn6UpButton.Visibility = Visibility.Hidden;
            _timer.Stop();
            _timeRemaining = _initialTime;
            UpdateTimerText();
            NexTraesureImage.Source = _gameBoard.ShowNextTreasure(CurrentSession.CurrentUser.Username);
            ExtraTileAndTreasureCanvas.Visibility = Visibility.Collapsed;
            EndTurnButton.Visibility = Visibility.Collapsed;
        }

        public void ReceiveGameBoard(TransferGameBoard gameBoard)
        {
            ConvertTransferToGameBoard(gameBoard);
            ShowExtraTileAndNextTreasure(gameBoard.MaxTreasures);
            UpdateExtraTileView();
            StartGame();
            Dispatcher.Invoke(() =>
            {
                MessagesListBox.Items.Add(Messages.InfoStartGameMessage);
            });
        }

        public void NotifyTurn(GameService.TransferPlayer currentPlayer)
        {
            _timeRemaining = _initialTime;
            UpdateTimerText();
            _timer.Start();
            Dispatcher.Invoke(() =>
            {
                MessagesListBox.Items.Add(Messages.ResourceManager.GetString(Messages.InfoTurnMessage) + " " + currentPlayer.Username);

            });
            if (currentPlayer.Username == CurrentSession.CurrentUser.Username)
            {
                _isMyTurn = true;
                EndTurnButton.Visibility = Visibility.Visible;
            }
        }

        public void NotifyPlayerHasJoined(GameService.TransferPlayer player)
        {
            if (player != null)
            {
                _gameBoard.AddPlayer(new Player
                {
                    Character = new Character(player.SkinPath),
                    InactivityCount = 0,
                    Name = player.Username,
                    TreasuresForSearch = new List<string>()
                });
            }
        }

        public void MovePlayerOnTile(string username, string direction)
        {
            Player player = _gameBoard.GetPlayer(username);
            Dispatcher.Invoke(() =>
            {
                _gameBoard.MovePlayer(username, direction);
                if (username == CurrentSession.CurrentUser.Username)
                {
                    TreasuresCountLabel.Content = player.TreasuresForSearch.Count + "/" + _gameBoard.MaxTreasures;
                    NexTraesureImage.Source = _gameBoard.ShowNextTreasure(username);
                }
            });
            UpdateGameboardAfterMove();
            if (player.CurrentPosition == player.InitialPosition && player.TreasuresForSearch.Count == 0)
            {
                CatalogManagementClient catalogManagementClient = new CatalogManagementClient();
                if (CurrentSession.CurrentUser.IdUser > 0)
                {
                    try
                    {
                        catalogManagementClient.AddStat(CurrentSession.CurrentUser.IdUser, player.Name == CurrentSession.CurrentUser.Username);
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailLostConnectionMessage);
                        LeaveLobby();
                    }
                    catch (EndpointNotFoundException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNotFoundEndPointMessage);
                        LeaveLobby();

                    }
                    catch (CommunicationException)
                    {
                        ExceptionHandler.HandleFailConnectionToServer(Messages.FailNoServerCommunicationMessage);
                        LeaveLobby();
                    }
                }
                Message message = new Message("InfoWinnerMessage", player.Name);
                message.Owner = this;
                message.ShowDialog();
              
                    MessagesListBox.Items.Add(Messages.ResourceManager.GetString("InfoWinnerMessage") + player.Name);
                EndGame();
            }
        }

        public void MoveRowOnBoard(string direction, int indexRow, bool toRight)
        {
            _lastExit = _gameBoard.MoveRow(indexRow, toRight);
            UpdateExtraTileView();
            UpdateGameboardAfterMove();
        }

        public void MoveColumnOnBoard(string direction, int indexRow, bool toRight)
        {
            _lastExit = _gameBoard.MoveColumn(indexRow, toRight);
            UpdateExtraTileView();
            UpdateGameboardAfterMove();
        }
        private void SelectCharacterButtonIsPressed(object sender, RoutedEventArgs e)
        {
            if (_characterSelection == null)
            {
                _characterSelection = new CharacterSelection();

                _characterSelection.Closed += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(_characterSelection.SelectedSkin))
                    {
                        string selectedSkin = _characterSelection.SelectedSkin;
                        if (IsAdmin)
                        {
                            Player player = _gameBoard.GetPlayer(CurrentSession.CurrentUser.Username);
                            UpdateUserCharacter(player, selectedSkin);
                        }
                        else
                        {
                            _gameServiceClient.SelectCharacter(lobbyCodeLabel.Text, CurrentSession.CurrentUser.Username, selectedSkin);
                        }

                    }
                    _characterSelection = null;
                };

                _characterSelection.Show();
            }
            else
            {
                _characterSelection.Activate();
            }
        }

        private void UpdateUserCharacter(Player player, string characterSelected)
        {
            Character character = new Character(characterSelected);
            player.Character = character;
            UpdateGameboardAfterMove();
        }

        public void UpdatePlayerCharacter(string username, string character)
        {
            if (IsAdmin && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(character))
            {
                Player player = _gameBoard.GetPlayer(username);
                UpdateUserCharacter(player, character);
            }
        }

        public void BroadcastExtraTileRotation(string username)
        {
            if (username != CurrentSession.CurrentUser.Username)
            {
                _gameBoard.ExtraTile.RotateRight();
                ExtraTileImage.Source = _gameBoard.ExtraTile.GetCurrentImage();
            }
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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.XPath;

namespace GameplayClasses
{
    public class GameBoard
    {
        public Tile[,] Board { get;  set; }
        public (int Row, int Column)[] PlayerPositions { get;  set; } 
        public Tile ExtraTile { get; set; }
        public List<Player> Players { get; set; }

        public int MaxTreasures { get; set; }

        public string MyLastInsert {  get; set; }

        private readonly List<(int Row, int Column)> _treasuresPositions = new List<(int, int)>
        {
            (2, 2), (2, 3), (2, 4),
            (3, 2), (3, 4), (4, 2),
            (4, 3), (4, 4),
            (1, 3), (3, 1), (3, 5), (5, 3),
            (1, 2), (1, 4), (2, 1), (2, 5),
            (4, 1), (4, 5), (5, 2), (5, 4)
        };

        private readonly List<(int Row, int Column)> _playerPositions = new List<(int, int)>
        {
            (0, 0), (6, 6), (6, 0), (0, 6)
        };

        public GameBoard(bool classicIsSelected)
        {
            Board = new Tile[7, 7];
            Players = new List<Player> ();
            InitializeTiles(classicIsSelected);
        }

        public GameBoard(bool classicIsSelected, int[] tiles)
        {
            Board = new Tile[7, 7];
            Players = new List<Player>();
            InitializeTiles(classicIsSelected, tiles);
        }
        public GameBoard(Tile[,] board, Tile extraTile, List<Player> players, (int Row, int Column)[] playerPositions)
        {
            Board = board;
            ExtraTile = extraTile;
            Players = players;
            PlayerPositions = playerPositions;
        }

        private void InitializeTiles(bool classicIsSelected, int[] tiles)
        {
            Tile[] tilesStyles = classicIsSelected ? ClassicTiles : VolcanoTiles;
            int count = 0;
            for (int row = 0; row < Board.GetLength(0); row++)
            {
                for (int col = 0; col < Board.GetLength(1); col++)
                {
                    Board[row, col] = (Tile)tilesStyles[tiles[count] - 1].Clone();
                    Board[row, col].PlayersOnTile = new List<Player>();
                    Board[row, col].Position = (row, col);
                    count++;
                }
            }

            for (int i = 0; i < _treasuresPositions.Count; i++)
            {
                var (row, col) = _treasuresPositions[i];
                if (i < Treasures.Length)
                {
                    Board[row, col].TreasureOnTile = Treasures[i];
                }
            }
            ExtraTile = (Tile)tilesStyles[tiles[tiles.Length - 1] - 1].Clone();
        }
        private void InitializeTiles(bool classicIsSelected)
        {
            Random random = new Random();
            Tile[] tilesStyles = classicIsSelected ? ClassicTiles : VolcanoTiles;

            for (int row = 0; row < Board.GetLength(0); row++)
            {
                for (int col = 0; col < Board.GetLength(1); col++)
                {
                    int randomIndex = random.Next(tilesStyles.Length);
                    Board[row, col] = (Tile)tilesStyles[randomIndex].Clone();
                    Board[row, col].PlayersOnTile = new List<Player>();
                    Board[row, col].Position = (row, col);
                }
            }
            for (int i = 0; i < _treasuresPositions.Count; i++)
            {
                var (row, col) = _treasuresPositions[i];
                if (i < Treasures.Length)
                {
                    Board[row, col].TreasureOnTile = Treasures[i];
                }
            }

            ExtraTile = (Tile)tilesStyles[random.Next(tilesStyles.Length)].Clone();
        }

        public void AssignPlayerToTile(Player player)
        {
            Console.WriteLine("assign");

            var (row, col) = player.CurrentPosition;
            Board[row, col].AddPlayer(ref player);
        }

        public bool ValidateMovement(string username, string direction)
        {
            bool result = false;

            if (direction == "A" || direction == "W" || direction == "D" || direction == "S")
            {
                var player = Players.FirstOrDefault(p => p.Name == username);
                if (player != null)
                {
                    var (newRow, newCol) = GetNewPosition(player.CurrentPosition, direction);
                    result = ValidateTails(player.CurrentPosition, (newRow, newCol), direction);
                }
            }
            return result;
        }

        public bool ValidateTails((int row, int col) position, (int newRow, int newCol) newPosition, string direction)
        {
            bool result = false;
            int maxRows = Board.GetLength(0);
            int maxCols = Board.GetLength(1);

            if (newPosition.newRow >= 0 && newPosition.newRow < maxRows && newPosition.newCol >= 0 && newPosition.newCol < maxCols)
            {
                if (Board[newPosition.newRow, newPosition.newCol] != null && IsLegalMovement(Board[position.row, position.col], Board[newPosition.newRow, newPosition.newCol], direction))
                {
                   result = true;
                }
            }
            return result;
        }

        private static (int newRow, int newCol) GetNewPosition((int row, int col) position, string direction)
        {
            int newRow = position.row;
            int newCol = position.col;
            switch (direction)
            {
                case "W":
                    newRow = position.row - 1;
                    break;
                case "S":
                    newRow = position.row + 1;
                    break;
                case "A":
                    newCol = position.col - 1;
                    break;
                case "D":
                    newCol = position.col + 1;
                    break;
                default:
                    break;
            }
            return (newRow, newCol);
        }
        public bool MovePlayer(string username, string direction)
        {
            bool result = false;
            var player = Players.FirstOrDefault(p => p.Name == username);

            if (ValidateMovement(username, direction) && player != null)
            {
                var (row, col) = player.CurrentPosition;

                var (newRow, newCol) = GetNewPosition(player.CurrentPosition, direction);

                player.CurrentPosition = (newRow, newCol);
                Board[newRow, newCol].AddPlayer(ref player);
                Board[row, col].RemovePlayer(player);
                CollectTreasure(username, ref Board[newRow, newCol]);
                result = true;
            }
            return result;
        }

        private static bool IsLegalMovement(Tile from, Tile to, string direction)
        {
            bool isLegalMovement = false;
            switch (direction)
            {
                case "W":
                    isLegalMovement = from.IsTopOpen && to.IsBottomOpen;
                    break;
                case "S":
                    isLegalMovement = from.IsBottomOpen && to.IsTopOpen;
                    break;
                case "A":
                    isLegalMovement = from.IsLeftOpen && to.IsRightOpen;
                    break;
                case "D":
                    isLegalMovement = from.IsRightOpen && to.IsLeftOpen;   
                    break;
                default: 
                    break;
            }
            return isLegalMovement;
        }

        public void CollectTreasure(string username, ref Tile currentTile)
        {
            if (currentTile != null && currentTile.TreasureOnTile != null)
            {
                var player = Players.FirstOrDefault(p => p.Name == username);
                
                if (player != null && player.TreasuresForSearch.Count > 0)
                {
                    string nextTreasure = player.TreasuresForSearch.First();
                    if (nextTreasure == currentTile.TreasureOnTile.Name)
                    {
                        player.TreasuresForSearch.Remove(currentTile.TreasureOnTile.Name); 
                        currentTile.TreasureOnTile = null;
                    }
                }
            }
        }

        public Tile[] ClassicTiles =
        {
            new Tile(1, LoadImage("GameplayClasses.tiles.Classic.ClassicTile1.png"), true, false, true, false),
            new Tile(2, LoadImage("GameplayClasses.tiles.Classic.ClassicTile2.png"), false, true, true, false),
            new Tile(3, LoadImage("GameplayClasses.tiles.Classic.ClassicTile3.png"), true, true, true, false)
        };
        
        public Tile[] VolcanoTiles =
        {
            new Tile(1, LoadImage("GameplayClasses.tiles.Volcano.VolcanoTile1.png"), true, false, true, false),
            new Tile(2, LoadImage("GameplayClasses.tiles.Volcano.VolcanoTile2.png"), false, true, true, false),
            new Tile(3, LoadImage("GameplayClasses.tiles.Volcano.VolcanoTile3.png"), true, true, true, false)
        };

        public Treasure[] Treasures =
        {
            new Treasure("Book", LoadImage("GameplayClasses.treasures.Book.png")),
            new Treasure("Chalice", LoadImage("GameplayClasses.treasures.Chalice.png")),
            new Treasure("Chest", LoadImage("GameplayClasses.treasures.Chest.png")),
            new Treasure("Clock", LoadImage("GameplayClasses.treasures.Clock.png")),
            new Treasure("Crown", LoadImage("GameplayClasses.treasures.Crown.png")),
            new Treasure("Dagger", LoadImage("GameplayClasses.treasures.Dagger.png")),
            new Treasure("Flower", LoadImage("GameplayClasses.treasures.Flower.png")),
            new Treasure("Gem", LoadImage("GameplayClasses.treasures.Gem.png")),
            new Treasure("GoldChest", LoadImage("GameplayClasses.treasures.GoldChest.png")),
            new Treasure("Heart", LoadImage("GameplayClasses.treasures.Heart.png")),
            new Treasure("Key", LoadImage("GameplayClasses.treasures.Key.png")),
            new Treasure("Map", LoadImage("GameplayClasses.treasures.Map.png")),
            new Treasure("Necklace", LoadImage("GameplayClasses.treasures.Necklace.png")),
            new Treasure("Potion", LoadImage("GameplayClasses.treasures.Potion.png")),
            new Treasure("Ring", LoadImage("GameplayClasses.treasures.Ring.png")),
            new Treasure("Scepter", LoadImage("GameplayClasses.treasures.Scepter.png")),
            new Treasure("Scroll", LoadImage("GameplayClasses.treasures.Scroll.png")),
            new Treasure("Shield", LoadImage("GameplayClasses.treasures.Shield.png")),
            new Treasure("Statue", LoadImage("GameplayClasses.treasures.Statue.png")),
            new Treasure("Sword", LoadImage("GameplayClasses.treasures.Sword.png"))
        };

        public void AssignTreasures(int quantity)
        {
            List<string> treasuresNames = new List<string>();
            Random random = new Random();

            for (int i = 0; i < Treasures.Length; i++)
            {
                treasuresNames.Add(Treasures[i].Name);
            }

            foreach (Player player in Players)
            {
                player.ClearTreasures();
                for (int i = 0; i < quantity; i++)
                {
                    string treasureName = treasuresNames[random.Next(treasuresNames.Count)];
                    player.AssignTreasure(treasureName);
                    treasuresNames.Remove(treasureName);
                }
            }
        }

        public static BitmapImage LoadImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public (string, int) MoveRow(int rowIndex, bool toRight)
        {            
            string oppositeDirection = toRight ? "Left" : "Right";
           
            Tile displacedTile;
            int rowLength = Board.GetLength(1);

            if (toRight)
            {
                displacedTile = Board[rowIndex, rowLength - 1];
                if (displacedTile.TreasureOnTile != null)
                {
                    ExtraTile.TreasureOnTile = displacedTile.TreasureOnTile;
                    displacedTile.TreasureOnTile = null;
                }

                if (displacedTile.PlayersOnTile != null)
                {
                    ExtraTile.PlayersOnTile = displacedTile.PlayersOnTile;
                    displacedTile.PlayersOnTile = null;
                    MovePlayersOnTile(ExtraTile.PlayersOnTile, rowIndex, 0);
                }

                for (int j = rowLength - 1; j > 0; j--)
                {
                    Board[rowIndex, j] = Board[rowIndex, j - 1];
                    MovePlayersOnTile(Board[rowIndex, j].PlayersOnTile, rowIndex, j);
                }
                Board[rowIndex, 0] = ExtraTile;
            }
            else
            {
                displacedTile = Board[rowIndex, 0];
                if (displacedTile.TreasureOnTile != null)
                {
                    ExtraTile.TreasureOnTile = displacedTile.TreasureOnTile;
                    displacedTile.TreasureOnTile = null;
                }
                if (displacedTile.PlayersOnTile != null)
                {
                    ExtraTile.PlayersOnTile = displacedTile.PlayersOnTile;
                    displacedTile.PlayersOnTile = null;
                    MovePlayersOnTile(ExtraTile.PlayersOnTile, rowIndex, rowLength - 1);
                }
                for (int j = 0; j < rowLength - 1; j++)
                {
                    Board[rowIndex, j] = Board[rowIndex, j + 1];
                    MovePlayersOnTile(Board[rowIndex, j].PlayersOnTile, rowIndex, j);
                }
                Board[rowIndex, rowLength - 1] = ExtraTile;
            }
            ExtraTile = displacedTile;
            return (oppositeDirection, rowIndex);
        }

        private void MovePlayersOnTile(List<Player> playersOnTile, int row, int col)
        {
            foreach (Player player in playersOnTile)
            {
                var playerOnTile = Players.FirstOrDefault(p => p.Name == player.Name);
                if (playerOnTile != null)
                {
                    playerOnTile.CurrentPosition = (row, col);
                }
            }
        }

        public (string, int) MoveColumn(int columnIndex, bool toDown)
        {
            string oppositeDirection = toDown ? "Up" : "Down";

            Tile displacedTile;
            int columnLength = Board.GetLength(0);

            if (toDown)
            {
                displacedTile = Board[columnLength - 1, columnIndex];
                if (displacedTile.TreasureOnTile != null)
                {
                    ExtraTile.TreasureOnTile = displacedTile.TreasureOnTile;
                    displacedTile.TreasureOnTile = null;
                }
                if (displacedTile.PlayersOnTile != null)
                {
                    ExtraTile.PlayersOnTile = displacedTile.PlayersOnTile;
                    displacedTile.PlayersOnTile = null;
                    MovePlayersOnTile(ExtraTile.PlayersOnTile, 0, columnIndex);

                }
                for (int i = columnLength - 1; i > 0; i--)
                {
                    Board[i, columnIndex] = Board[i - 1, columnIndex];
                    MovePlayersOnTile(Board[i, columnIndex].PlayersOnTile, i, columnIndex);
                }
                Board[0, columnIndex] = ExtraTile;
            }
            else
            {
                displacedTile = Board[0, columnIndex];
                if (displacedTile.TreasureOnTile != null)
                {
                    ExtraTile.TreasureOnTile = displacedTile.TreasureOnTile;
                    displacedTile.TreasureOnTile = null;
                }
                if (displacedTile.PlayersOnTile != null)
                {
                    ExtraTile.PlayersOnTile = displacedTile.PlayersOnTile;
                    displacedTile.PlayersOnTile = null;
                    MovePlayersOnTile(ExtraTile.PlayersOnTile, columnLength - 1, columnIndex);
                }
                for (int i = 0; i < columnLength - 1; i++)
                {
                    Board[i, columnIndex] = Board[i + 1, columnIndex];
                    MovePlayersOnTile(Board[i, columnIndex].PlayersOnTile, i, columnIndex);
                }
                Board[columnLength - 1, columnIndex] = ExtraTile;
            }
            ExtraTile = displacedTile;

            return (oppositeDirection, columnIndex);
        }

        public void AddPlayer(Player player)
        {
            if (Players != null && Players.Count < 4)
            {
                player.CurrentPosition = _playerPositions[Players.Count];
                AssignPlayerToTile(player);
                Players.Add(player);
            }
        }

        public BitmapImage ShowNextTreasure(string username)
        {
            BitmapImage result = null;
            Player player = Players.FirstOrDefault(p => p.Name == username);
            if (player != null && player.TreasuresForSearch.Count > 0)
            {
                string nextTreasure = player.TreasuresForSearch.First();

                for (int i = 0; i < Treasures.Length; i++)
                {
                    if (Treasures[i].Name == nextTreasure)
                    {
                        result = Treasures[i].ImageTreasure;
                        i = Treasures.Length - 1;
                    }
                }
            }
           
            return result;
        }

        public Player GetPlayer(string username)
        {
            return Players.FirstOrDefault(player =>  player.Name == username);
        }
    }
}

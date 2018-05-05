using OpenTK;
using OpenTK.Graphics;
using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game
{
    class Board : GameObject
    {
        public GameClient GameClient;
        public Property[] Properties;
        public Player[] Players;
        public int[] PlayerOrder;
        public int CurrentPlayerID = 0;
        public int CurrentPlayerTurn = 0;
        public float MovementSpeed = 2.5f;

        private ShaderProgram PlayerProgram;

        public Board(ShaderProgram boardProgram, ShaderProgram playerProgram, GameClient gameClient)
            : base(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            GameClient = gameClient;
            // Set vertices.
            List<Vertex> vertices = new List<Vertex> {
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), Color4.White, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f, 0.0f), Color4.White, new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f, 0.0f), Color4.White, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f, 0.0f), Color4.White, new Vector2(1.0f, 0.0f))
            };

            int[] indices = new int[] {
                0, 1, 3,
                2, 3, 1
            };

            float scaleFactor = 5.0f;

            Model = new Model(boardProgram, vertices, indices,
                Vector3.Zero, new Vector3(AGameClient.ToRadians(-90.0f), 0.0f, 0.0f), new Vector3(scaleFactor),
                    @"Resources\Textures\polymono.png");
            Properties = new Property[40];
            float xSize;
            float zSize;
            float xOffset;
            float zOffset;
            for (int n = 0; n < 4; n++)
            {
                xSize = (0.5f * scaleFactor) * 0.825f;
                zSize = (0.5f * scaleFactor) * 0.825f;
                switch (n)
                {
                    case 0:
                        for (int i = 0; i < 10; i++)
                        {
                            // Increase X offset by the {i} step of a 10th of xsize.
                            xOffset = -xSize;
                            zOffset = zSize - (i * (zSize / 5));
                            CreateProperties((10 * n) + i, new Vector3(xOffset, 0.0f, zOffset));
                        }
                        break;
                    case 1:
                        for (int i = 0; i < 10; i++)
                        {
                            xOffset = -xSize + (i * (xSize / 5));
                            zOffset = -zSize;
                            CreateProperties((10 * n) + i, new Vector3(xOffset, 0.0f, zOffset));
                        }
                        break;
                    case 2:
                        for (int i = 0; i < 10; i++)
                        {
                            xOffset = xSize;
                            zOffset = -zSize + (i * (zSize / 5));
                            CreateProperties((10 * n) + i, new Vector3(xOffset, 0.0f, zOffset));
                        }
                        break;
                    case 3:
                        for (int i = 0; i < 10; i++)
                        {
                            xOffset = xSize - (i * (xSize / 5));
                            zOffset = zSize;
                            CreateProperties((10 * n) + i, new Vector3(xOffset, 0.0f, zOffset));
                        }
                        break;
                }
            }
            Players = new Player[Polymono.MaxPlayers];
            PlayerOrder = new int[Polymono.MaxPlayers];
            for (int i = 0; i < PlayerOrder.Length;)
            {
                PlayerOrder[i] = i++;
            }
            PlayerProgram = playerProgram;
        }

        public Player GetPlayer(int id)
        {
            return Players[id];
        }

        public Player GetPlayer()
        {
            return Players[CurrentPlayerID];
        }

        public List<Player> GetPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (var player in Players)
            {
                if (player != null)
                {
                    players.Add(player);
                }
            }
            return players;
        }

        public void AddPlayer(int id, string name)
        {
            Polymono.Debug($"Board::AddPlayer(ID: {id}, Name: {name})");
            Player player = new Player(PlayerProgram, this)
            {
                PlayerName = name
            };
            Players[id] = player;
            SetPlayerPosition(id, 0);
        }

        /// <summary>
        /// Force the player to the position of the property immediately.
        /// Typically for finalising network desync problems.
        /// </summary>
        public void SetPlayerPosition(int playerID, int propertyID)
        {
            Vector3 propertyOffset = Properties[propertyID].BoardLocationOffset;
            Vector3 playerOffset = GetOffsetByPlayers(propertyID);
            Players[playerID].NextLocation = propertyID;
            Players[playerID].FinalLocation = propertyID;
            Players[playerID].Position = propertyOffset + playerOffset;
            Players[playerID].OriginPosition = propertyOffset + playerOffset;
            Players[playerID].CurrentLocation = propertyID;
        }

        public bool UpdatePositions(int playerID, float updateFrequency)
        {
            updateFrequency /= MovementSpeed;
            Player player = Players[playerID];
            if (player.CurrentLocation == player.FinalLocation)
            {
                return false;
            }
            // Points of the line.
            Vector3 BeginPoint = player.OriginPosition;
            Vector3 EndPoint = Properties[player.NextLocation].BoardLocationOffset + GetOffsetByPlayers(player.NextLocation);
            // The difference vector.
            Vector3 Difference = EndPoint - BeginPoint;
            Vector3 UpdateDiff = Difference / updateFrequency;
            // Updated player position.
            Vector3 pastPosition = player.Position;
            Vector3 NewPosition = player.Position + UpdateDiff;
            player.Position = NewPosition;
            // Magnitudes
            float completeMagnitude = Difference.Length;
            float firstPartialMagnitude = (NewPosition - BeginPoint).Length;
            float secondPartialMagnitude = UpdateDiff.Length;
            float adjustedMagnitude = firstPartialMagnitude + secondPartialMagnitude;
            // Check magnitudes
            if (completeMagnitude <= adjustedMagnitude)
            {
                Polymono.Debug($"Board::UpdatePosition(ID, UpFreq): End of movement segment.");
                // If magnitude is further than is supposed to be, or equal, force position -> Finish.
                player.Position = EndPoint;
                player.OriginPosition = EndPoint;
                player.CurrentLocation = player.NextLocation;
                if (player.CurrentLocation != player.FinalLocation)
                {
                    player.NextLocation = NextPropertyID(player.CurrentLocation);
                    return false;
                }
                else if (player.CurrentLocation == player.FinalLocation)
                {
                    Polymono.Debug($"Board::UpdatePosition(ID, UpFreq): Movement finished.");
                    GameClient.State = GameState.PlayerOptions;
                    // Moving is done, fully.
                    return true;
                }
            }
            return false;
        }

        public void FinalisePlayerMovement(int playerID)
        {
            Polymono.Debug($"Board::FinalisePlayerMovement(ID: {playerID})");
            SetPlayerPosition(playerID, Players[playerID].FinalLocation);
        }

        public void MoveSpaces(int spaces, int playerID)
        {
            Polymono.Debug($"Board::MoveSpaces(spaces: {spaces}, sender: {playerID})");
            // TODO: Gradual player movement.
            Players[playerID].NextLocation = NextPropertyID(Players[playerID].CurrentLocation);
            int step = Players[playerID].CurrentLocation;
            for (int i = 1; i < spaces + 1; i++)
            {
                step = NextPropertyID(step);
            }
            Players[playerID].FinalLocation = step;
        }

        public int NextPropertyID(int current)
        {
            int next = current + 1;
            if (next >= Properties.Length)
                next = 0;
            return next;
        }

        private Vector3 GetOffsetByPlayers(int propertyID, float scaleFactor = 0.1f)
        {
            int playersAtLocation = 0;
            foreach (var player in GetPlayers())
            {
                if (player.CurrentLocation == propertyID)
                {
                    playersAtLocation++;
                }
            }
            switch (playersAtLocation)
            {
                case 0:
                    return new Vector3(scaleFactor, 0, 0);
                case 1:
                    return new Vector3(0, 0, scaleFactor);
                case 2:
                    return new Vector3(scaleFactor, 0, scaleFactor);
                case 3:
                    return new Vector3(-scaleFactor, 0, 0);
                case 4:
                    return new Vector3(0, 0, -scaleFactor);
                case 5:
                    return new Vector3(-scaleFactor, 0, -scaleFactor);
                case 6:
                    return new Vector3(scaleFactor, 0, -scaleFactor);
                case 7:
                    return new Vector3(-scaleFactor, 0, scaleFactor);
                default:
                    Random rnd = new Random();
                    return new Vector3(
                        ((float)rnd.NextDouble() + 0.5f) * scaleFactor, 0,
                        ((float)rnd.NextDouble() + 0.5f) * scaleFactor);
            }
        }

        public void EndCurrentTurn(int playerID)
        {
            Polymono.Debug($"Board::EndCurrentTurn(ID: {playerID})");
            // If in lobby roll phase.
            if (GameClient.State == GameState.LobbyRoll)
            {
                int nextPlayerTurn = CurrentPlayerTurn + 1;
                // If end of lobby roll phase
                if (nextPlayerTurn >= GetPlayers().Count)
                {
                    // Carry on
                    // Assign an order list and start over.
                    CompleteOrder();
                    CurrentPlayerTurn = PlayerOrder[0];
                    GameClient.State = GameState.PlayerOptions;
                }
                else
                {
                    CurrentPlayerTurn += 1;
                }
            }
            else if (GameClient.State == GameState.PlayerOptions)
            {
                // End turn pressed.
                for (int i = 0; i < PlayerOrder.Length; i++)
                {
                    // Get current turn's position.
                    if (PlayerOrder[i] == CurrentPlayerTurn)
                    {
                        // Current playerOrder, increment order.
                        if (i + 1 >= PlayerOrder.Length)
                        {
                            CurrentPlayerTurn = PlayerOrder[0];
                        }
                        else
                        {
                            CurrentPlayerTurn = PlayerOrder[i + 1];
                        }
                        break;
                    }

                }
            }
        }

        public void CompleteOrder()
        {
            Player[] players = GetPlayers().ToArray();
            Player[] ordPlayers = new Player[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (ordPlayers[i] == null)
                    ordPlayers[i] = players[i];
                else if (ordPlayers[i].HighestRoll >= players[i].HighestRoll)
                    InsertInto(ref players, ref players[i], i);
            }
            PlayerOrder = new int[ordPlayers.Length];
            for (int i = 0; i < ordPlayers.Length; i++)
            {
                PlayerOrder[i] = ordPlayers[i].PlayerID;
            }
        }

        private void InsertInto(ref Player[] players, ref Player insertion, int index)
        {
            for (int i = players.Length - 2; i >= index; i--)
            {
                if (players[i] != null)
                    players[i] = players[i + 1];
            }
            players[index] = insertion;
        }

        private void CreateProperties(int id, Vector3 vector)
        {
            switch (ID)
            {
                case 0:
                    Properties[id] = new Property(id, "Go", PropertyGroup.Go, vector);
                    break;
                case 1:
                    Properties[id] = new Property(id, "Mediterranean Avenue", 60, 30, 2, 10, 30, 90, 160, 250, PropertyGroup.Brown, vector);
                    break;
                case 2:
                    Properties[id] = new Property(id, "Community Chest", PropertyGroup.Community, vector);
                    break;
                case 3:
                    Properties[id] = new Property(id, "Income Tax", 200, PropertyGroup.Tax, vector);
                    break;
                case 4:
                    Properties[id] = new Property(id, "Baltic Avenue", 60, 30, 4, 20, 60, 180, 320, 450, PropertyGroup.Brown, vector);
                    break;
                case 5:
                    Properties[id] = new Property(id, "Reading Railroad", 200, 100, 25, 50, 100, 200, PropertyGroup.Railroad, vector);
                    break;
                case 6:
                    Properties[id] = new Property(id, "Oriental Avenue", 100, 50, 6, 30, 90, 270, 400, 550, PropertyGroup.Cyan, vector);
                    break;
                case 7:
                    Properties[id] = new Property(id, "Vermont Avenue", 100, 50, 6, 30, 90, 270, 400, 550, PropertyGroup.Cyan, vector);
                    break;
                case 8:
                    Properties[id] = new Property(id, "Chance", PropertyGroup.Chance, vector);
                    break;
                case 9:
                    Properties[id] = new Property(id, "Connecticut Avenue", 120, 60, 8, 40, 100, 300, 450, 600, PropertyGroup.Cyan, vector);
                    break;
                case 10:
                    Properties[id] = new Property(id, "Just Visiting", PropertyGroup.Jail, vector);
                    break;
                case 11:
                    Properties[id] = new Property(id, "St. Charles Place", 140, 70, 10, 50, 150, 450, 625, 750, PropertyGroup.Pink, vector);
                    break;
                case 12:
                    Properties[id] = new Property(id, "Electric Company", 150, 75, 4, 10, PropertyGroup.Utility, vector);
                    break;
                case 13:
                    Properties[id] = new Property(id, "States Avenue", 140, 70, 10, 50, 150, 450, 625, 750, PropertyGroup.Pink, vector);
                    break;
                case 14:
                    Properties[id] = new Property(id, "Virginia Avenue", 160, 80, 12, 60, 180, 500, 700, 900, PropertyGroup.Pink, vector);
                    break;
                case 15:
                    Properties[id] = new Property(id, "Pennsylvania Railroad", 200, 100, 25, 50, 100, 200, PropertyGroup.Railroad, vector);
                    break;
                case 16:
                    Properties[id] = new Property(id, "St. James Place", 180, 90, 14, 70, 200, 550, 750, 950, PropertyGroup.Orange, vector);
                    break;
                case 17:
                    Properties[id] = new Property(id, "Community Chest", PropertyGroup.Community, vector);
                    break;
                case 18:
                    Properties[id] = new Property(id, "Tennessee Avenue", 180, 90, 14, 70, 200, 550, 750, 950, PropertyGroup.Orange, vector);
                    break;
                case 19:
                    Properties[id] = new Property(id, "New York Avenue", 200, 100, 16, 80, 220, 600, 800, 1000, PropertyGroup.Orange, vector);
                    break;
                case 20:
                    Properties[id] = new Property(id, "Free Parking", PropertyGroup.Parking, vector);
                    break;
                case 21:
                    Properties[id] = new Property(id, "Kentucky Avenue", 220, 110, 18, 90, 250, 700, 875, 1050, PropertyGroup.Red, vector);
                    break;
                case 22:
                    Properties[id] = new Property(id, "Chance", PropertyGroup.Chance, vector);
                    break;
                case 23:
                    Properties[id] = new Property(id, "Indiana Avenue", 220, 110, 18, 90, 250, 700, 875, 1050, PropertyGroup.Red, vector);
                    break;
                case 24:
                    Properties[id] = new Property(id, "Illinois Avenue", 240, 120, 20, 100, 300, 750, 925, 1100, PropertyGroup.Red, vector);
                    break;
                case 25:
                    Properties[id] = new Property(id, "B & O. Railroad", 200, 100, 25, 50, 100, 200, PropertyGroup.Railroad, vector);
                    break;
                case 26:
                    Properties[id] = new Property(id, "Atlantic Avenue", 260, 130, 22, 110, 330, 800, 985, 1150, PropertyGroup.Yellow, vector);
                    break;
                case 27:
                    Properties[id] = new Property(id, "Ventnor Avenue", 260, 130, 22, 110, 330, 800, 975, 1150, PropertyGroup.Yellow, vector);
                    break;
                case 28:
                    Properties[id] = new Property(id, "Water Works", 150, 75, 4, 10, PropertyGroup.Utility, vector);
                    break;
                case 29:
                    Properties[id] = new Property(id, "Marvin Gardens", 280, 140, 24, 120, 360, 850, 1025, 1200, PropertyGroup.Yellow, vector);
                    break;
                case 30:
                    Properties[id] = new Property(id, "Go To Jail", PropertyGroup.Police, vector);
                    break;
                case 31:
                    Properties[id] = new Property(id, "Pacific Avenue", 300, 150, 26, 130, 390, 900, 1100, 1275, PropertyGroup.Green, vector);
                    break;
                case 32:
                    Properties[id] = new Property(id, "North Carolina Avenue", 300, 150, 26, 130, 390, 900, 1100, 1275, PropertyGroup.Green, vector);
                    break;
                case 33:
                    Properties[id] = new Property(id, "Cumminty Chest", PropertyGroup.Community, vector);
                    break;
                case 34:
                    Properties[id] = new Property(id, "Pennsylvania Avenue", 320, 160, 28, 150, 450, 1000, 1200, 1400, PropertyGroup.Green, vector);
                    break;
                case 35:
                    Properties[id] = new Property(id, "Short Line", 200, 100, 25, 50, 100, 200, PropertyGroup.Railroad, vector);
                    break;
                case 36:
                    Properties[id] = new Property(id, "Chance", PropertyGroup.Chance, vector);
                    break;
                case 37:
                    Properties[id] = new Property(id, "Park Place", 350, 175, 35, 175, 500, 1100, 1300, 1500, PropertyGroup.Blue, vector);
                    break;
                case 38:
                    Properties[id] = new Property(id, "Luxury Tax", 100, PropertyGroup.Tax, vector);
                    break;
                case 39:
                    Properties[id] = new Property(id, "Boardwalk", 400, 200, 50, 200, 600, 1400, 1700, 2000, PropertyGroup.Blue, vector);
                    break;
            }
        }
    }
}

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
        public Dictionary<Player, int> PlayerLocations;
        private ShaderProgram PlayerProgram;
        public int CurrentPlayerID;

        public Board(ShaderProgram boardProgram, ShaderProgram playerProgram, GameClient gameClient)
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
            PlayerLocations = new Dictionary<Player, int>();
            PlayerProgram = playerProgram;
            // Populate all players.
        }

        public Player AddPlayer(string name)
        {
            Player player = new Player(PlayerProgram, this)
            {
                PlayerName = name
            };
            player.Model.CreateBuffer();
            PlayerLocations.Add(player, 0);
            GameClient.Models.Add(player.Model.ID, player.Model);
            return player;
        }

        public int GetPlayerLocation(Player player)
        {
            return PlayerLocations[player];
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

        private void CreateProperties(int id, Vector3 vector)
        {
            switch (ID)
            {
                case 0:
                    Properties[id] = new Property(id, "Go", 0, PropertyGroup.Go, vector);
                    break;
                case 1:
                    Properties[id] = new Property(id, "Mediterranean Avenue", 60, PropertyGroup.Brown, vector);
                    break;
                case 2:
                    Properties[id] = new Property(id, "Community Chest", 0, PropertyGroup.Community, vector);
                    break;
                case 3:
                    Properties[id] = new Property(id, "Income Tax", 0, PropertyGroup.Tax, vector);
                    break;
                case 4:
                    Properties[id] = new Property(id, "Baltic Avenue", 60, PropertyGroup.Brown, vector);
                    break;
                case 5:
                    Properties[id] = new Property(id, "Reading Railroad", 200, PropertyGroup.Railroad, vector);
                    break;
                case 6:
                    Properties[id] = new Property(id, "Oriental Avenue", 100, PropertyGroup.Cyan, vector);
                    break;
                case 7:
                    Properties[id] = new Property(id, "Vermont Avenue", 100, PropertyGroup.Cyan, vector);
                    break;
                case 8:
                    Properties[id] = new Property(id, "Chance", 0, PropertyGroup.Chance, vector);
                    break;
                case 9:
                    Properties[id] = new Property(id, "Connecticut Avenue", 120, PropertyGroup.Cyan, vector);
                    break;
                case 10:
                    Properties[id] = new Property(id, "Just Visiting", 0, PropertyGroup.Jail, vector);
                    break;
                case 11:
                    Properties[id] = new Property(id, "St. Charles Place", 140, PropertyGroup.Pink, vector);
                    break;
                case 12:
                    Properties[id] = new Property(id, "Electric Company", 150, PropertyGroup.Utility, vector);
                    break;
                case 13:
                    Properties[id] = new Property(id, "States Avenue", 140, PropertyGroup.Pink, vector);
                    break;
                case 14:
                    Properties[id] = new Property(id, "Virginia Avenue", 160, PropertyGroup.Pink, vector);
                    break;
                case 15:
                    Properties[id] = new Property(id, "Pennsylvania Railroad", 200, PropertyGroup.Railroad, vector);
                    break;
                case 16:
                    Properties[id] = new Property(id, "St. James Place", 180, PropertyGroup.Orange, vector);
                    break;
                case 17:
                    Properties[id] = new Property(id, "Community Chest", 0, PropertyGroup.Community, vector);
                    break;
                case 18:
                    Properties[id] = new Property(id, "Tennessee Avenue", 180, PropertyGroup.Orange, vector);
                    break;
                case 19:
                    Properties[id] = new Property(id, "New York Avenue", 200, PropertyGroup.Orange, vector);
                    break;
                case 20:
                    Properties[id] = new Property(id, "Free Parking", 0, PropertyGroup.Parking, vector);
                    break;
                case 21:
                    Properties[id] = new Property(id, "Kentucky Avenue", 220, PropertyGroup.Red, vector);
                    break;
                case 22:
                    Properties[id] = new Property(id, "Chance", 0, PropertyGroup.Chance, vector);
                    break;
                case 23:
                    Properties[id] = new Property(id, "Indiana Avenue", 220, PropertyGroup.Red, vector);
                    break;
                case 24:
                    Properties[id] = new Property(id, "Illinois Avenue", 240, PropertyGroup.Red, vector);
                    break;
                case 25:
                    Properties[id] = new Property(id, "B & O. Railroad", 200, PropertyGroup.Railroad, vector);
                    break;
                case 26:
                    Properties[id] = new Property(id, "Atlantic Avenue", 260, PropertyGroup.Yellow, vector);
                    break;
                case 27:
                    Properties[id] = new Property(id, "Ventnor Avenue", 260, PropertyGroup.Yellow, vector);
                    break;
                case 28:
                    Properties[id] = new Property(id, "Water Works", 200, PropertyGroup.Utility, vector);
                    break;
                case 29:
                    Properties[id] = new Property(id, "Marvin Gardens", 280, PropertyGroup.Yellow, vector);
                    break;
                case 30:
                    Properties[id] = new Property(id, "Go To Jail", 0, PropertyGroup.Police, vector);
                    break;
                case 31:
                    Properties[id] = new Property(id, "Pacific Avenue", 300, PropertyGroup.Green, vector);
                    break;
                case 32:
                    Properties[id] = new Property(id, "North Carolina Avenue", 300, PropertyGroup.Green, vector);
                    break;
                case 33:
                    Properties[id] = new Property(id, "Cumminty Chest", 0, PropertyGroup.Community, vector);
                    break;
                case 34:
                    Properties[id] = new Property(id, "Pennsylvania Avenue", 320, PropertyGroup.Green, vector);
                    break;
                case 35:
                    Properties[id] = new Property(id, "Short Line", 200, PropertyGroup.Railroad, vector);
                    break;
                case 36:
                    Properties[id] = new Property(id, "Chance", 0, PropertyGroup.Chance, vector);
                    break;
                case 37:
                    Properties[id] = new Property(id, "Park Place", 350, PropertyGroup.Blue, vector);
                    break;
                case 38:
                    Properties[id] = new Property(id, "Luxury Tax", 0, PropertyGroup.Tax, vector);
                    break;
                case 39:
                    Properties[id] = new Property(id, "Boardwalk", 400, PropertyGroup.Blue, vector);
                    break;
            }
        }
    }
}

using OpenTK;
using OpenTK.Graphics;
using Polymono.Graphics;
using Polymono.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Player : GameObject {
        public static int TOTAL_PLAYER_IDS = 0;
        public int PlayerID;
        public string PlayerName;
        public Board Board;
        //public Property CurrentPosition;
        public ISocket networkHandle;
        public bool RequiresBuffer = true;
        public Vector4 Colour;
        // Location information
        public int CurrentLocation = 0;
        public int NextLocation = 0;
        public int FinalLocation = 0;

        public int HighestRoll = 0;

        public int Money = 1500;
        internal bool IsInJail;

        public Player(ShaderProgram program, Board board)
            : base(Vector3.Zero, Vector3.Zero, new Vector3(0.1f))
        {
            PlayerID = TOTAL_PLAYER_IDS++;
            Board = board;
            Vector3 offset = board.Properties[0].BoardLocationOffset;
            offset.X += (PlayerID / 5f);
            Colour = GetPlayerColour(PlayerID);
            Model = (ModelObject)Board.GameClient.AModels["Player"];
        }

        public void TranslateAndUpdate(Vector3 position)
        {
            Position = position;
            ModelMatrix = Matrix4.CreateTranslation(Position);
        }

        public void SetNetworkHandle(ISocket socket)
        {
            networkHandle = socket;
        }

        public ref ISocket NetworkHandler()
        {
            return ref networkHandle;
        }

        public Vector4 GetRandomColour()
        {
            Random random = new Random();
            float r = (float)random.NextDouble() * 255f;
            float g = (float)random.NextDouble() * 255f;
            float b = (float)random.NextDouble() * 255f;
            float a = (float)random.NextDouble() * 255f;
            return new Vector4(r, g, b, a);
        }

        public Vector4 GetPlayerColour(int playerID)
        {
            Color4 colour;
            switch (playerID)
            {
                case 0:
                    colour = Color4.White;
                    break;
                case 1:
                    colour = Color4.Red;
                    break;
                case 2:
                    colour = Color4.Yellow;
                    break;
                case 3:
                    colour = Color4.Green;
                    break;
                case 4:
                    colour = Color4.Blue;
                    break;
                case 5:
                    colour = Color4.Pink;
                    break;
                case 6:
                    colour = Color4.Purple;
                    break;
                case 7:
                    colour = Color4.Orange;
                    break;
                default:
                    colour = Color4.Black;
                    break;
            }
            return ColourToVector(colour);
        }

        public Vector4 ColourToVector(Color4 colour)
        {
            return new Vector4(colour.R, colour.G, colour.B, colour.A);
        }
    }
}

using OpenTK;
using OpenTK.Graphics;
using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Player : GameObject {
        public static int TOTAL_PLAYER_IDS = 0;
        public int PlayerID;

        public Player(ShaderProgram program, Board board)
        {
            PlayerID = TOTAL_PLAYER_IDS++;
            Vector3 offset = board.Property[0].BoardLocationOffset;
            offset.X += (PlayerID / 5f);
            Model = new ModelObject(
                program,
                @"Resources\Objects\player.obj",
                GetPlayerColour(PlayerID),
                new Vector3(offset),
                Vector3.Zero,
                new Vector3(0.1f, 0.1f, 0.1f),
                @"Resources\Objects\player.mtl",
                @"b0b0b0");
        }

        public Color4 GetRandomColour()
        {
            Random random = new Random();
            float r = (float)random.NextDouble() * 255f;
            float g = (float)random.NextDouble() * 255f;
            float b = (float)random.NextDouble() * 255f;
            float a = (float)random.NextDouble() * 255f;
            return new Color4(r, g, b, a);
        }

        public Color4 GetPlayerColour(int playerID)
        {
            switch (playerID)
            {
                case 0:
                    return Color4.White;
                case 1:
                    return Color4.Red;
                case 2:
                    return Color4.Yellow;
                case 3:
                    return Color4.Green;
                case 4:
                    return Color4.Blue;
                case 5:
                    return Color4.Pink;
                case 6:
                    return Color4.Purple;
                case 7:
                    return Color4.Orange;
                default:
                    return Color4.Black;
            }
        }
    }
}

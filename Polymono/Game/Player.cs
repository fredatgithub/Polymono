using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Player : GameObject {
        public static int TOTAL_PLAYER_IDS = 0;
        public int PlayerID;

        public Player()
        {
            PlayerID = TOTAL_PLAYER_IDS++;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    public enum State {

    }

    class GameState {
        public int PlayerCount;

        // Manages the game state.
        public GameState(int playerCount)
        {
            PlayerCount = playerCount;
        }
    }
}

using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class GameObject {
        public static int TOTAL_IDS = 0;
        public int ID;
        // Modelling
        public AModel Model;

        public GameObject()
        {
            ID = TOTAL_IDS++;
        }
    }
}

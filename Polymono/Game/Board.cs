using OpenTK;
using Polymono.Graphics;
using Polymono.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Board : GameObject {
        public Property[] Property;

        public Board()
        {
            Property = new Property[40];
        }
    }
}

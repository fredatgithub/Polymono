using OpenTK;
using Polymono.Classes.Graphics;
using Polymono.Classes.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Game {
    class Board : GameObject {
        public Property[] Property;

        public Board()
        {
            Property = new Property[40];
        }
    }
}

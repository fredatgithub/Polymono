using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Property : GameObject {
        public Vector3 BoardLocationOffset;

        public Property(Vector3 boardLocationOffset) : base()
        {
            BoardLocationOffset = boardLocationOffset;
        }
    }
}

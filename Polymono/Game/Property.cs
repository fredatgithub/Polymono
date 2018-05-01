using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Property : GameObject {
        // Position on board.
        public int PropertyID;
        public string Name;
        public int Price;
        public PropertyGroup Group;
        public Vector3 BoardLocationOffset;

        public Property(int id, string name, int price, PropertyGroup group, Vector3 boardLocationOffset) 
            : base(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            PropertyID = id;
            Name = name;
            Price = price;
            Group = group;
            BoardLocationOffset = boardLocationOffset;
        }
    }

    enum PropertyGroup
    {
        Brown,
        Cyan,
        Pink,
        Orange,
        Red,
        Yellow,
        Green,
        Blue,
        Railroad,
        Utility,
        Community,
        Chance,
        Go,
        Jail,
        Parking,
        Police,
        Tax
    }
}

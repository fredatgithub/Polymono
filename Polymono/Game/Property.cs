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
        public int Mortgage;
        public PropertyGroup Group;
        public Vector3 BoardLocationOffset;

        public bool IsPurchasable = false;
        public bool IsUpgradable = false;
        public bool IsOwned = false;
        public int Rent;
        public int RentOne;
        public int RentTwo;
        public int RentThree;
        public int RentFour;
        public int RentHotel;

        public int GroupOne;
        public int GroupTwo;
        public int GroupThree;
        public int GroupFour;

        public int Tax;
        
        /// <summary>
        /// Event properties. [Go] [Community Chest] [Chance] [Free parking]
        /// </summary>
        public Property(int id, string name, PropertyGroup group, Vector3 boardLocationOffset)
            : this(id, name, 0, 0, group, boardLocationOffset)
        {
            IsPurchasable = false;
        }

        /// <summary>
        /// Tax properties.
        /// </summary>
        public Property(int id, string name, int tax, PropertyGroup group, Vector3 boardLocationOffset)
            : this(id, name, group, boardLocationOffset)
        {
            Tax = tax;
            IsPurchasable = false;
        }

        /// <summary>
        /// Utility properties.
        /// </summary>
        public Property(int id, string name, int price, int mortgage, int one, int two, PropertyGroup group, Vector3 boardLocationOffset)
            : this(id, name, price, mortgage, group, boardLocationOffset)
        {
            GroupOne = one;
            GroupTwo = two;
            IsPurchasable = true;
        }

        /// <summary>
        /// Railroad properties.
        /// </summary>
        public Property(int id, string name, int price, int mortgage, int one, int two, int three, int four, PropertyGroup group, Vector3 boardLocationOffset)
            : this(id, name, price, mortgage, group, boardLocationOffset)
        {
            GroupOne = one;
            GroupTwo = two;
            GroupThree = three;
            GroupFour = four;
            IsPurchasable = true;
        }

        /// <summary>
        /// Normal properties.
        /// </summary>
        public Property(int id, string name, int price, int mortgage,
            int rent, int rentOne, int rentTwo, int rentThree, int rentFour, int rentHotel,
            PropertyGroup group, Vector3 boardLocationOffset)
            : this(id, name, price, mortgage, group, boardLocationOffset)
        {
            Rent = rent;
            RentOne = rentOne;
            RentTwo = rentTwo;
            RentThree = rentThree;
            RentFour = rentFour;
            RentHotel = rentHotel;
            IsPurchasable = true;
            IsUpgradable = true;
        }

        protected Property(int id, string name, int price, int mortgage, PropertyGroup group, Vector3 boardLocationOffset)
            : base(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            PropertyID = id;
            Name = name;
            Group = group;
            BoardLocationOffset = boardLocationOffset;
        }
    }

    public enum PropertyGroup
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

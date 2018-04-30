using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class PacketData
    {
        public string Data;
        public PacketType Type;

        public PacketData()
        {
            Data = "";
            Type = PacketType.Null;
        }
    }
}

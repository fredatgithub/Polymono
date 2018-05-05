using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    interface INetwork
    {
        ref Queue<Packet> GetPacketQueue();
        Task SendAsync(params Packet[] packets);
    }
}

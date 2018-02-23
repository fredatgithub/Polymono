using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking {
    interface INetwork {
        void Exit();
        void Send(Packet[] packets, AsyncCallback p);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Networking {
    interface INetwork {
        void Send(params Packet[] packets);

        void Exit();
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Networking {
    /// <summary>
    /// State object for reading client data asynchronously.
    /// </summary>
    class SocketState {
        // Local ID
        public int ID = 0;
        // Client socket.
        public Socket Socket = null;
        // Receive buffer.
        public byte[] ByteBuffer = new byte[PacketHandler.BufferSize];
        // Data buffer.
        public StringBuilder DataBuffer = new StringBuilder();
    }
}

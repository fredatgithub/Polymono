using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Client
    {
        public ISocket Socket;

        public Client(bool v6 = true)
        {
            Socket = new PolySocket(v6);
        }

        public async Task ConnectAsync(string host, int port)
        {
            await Socket.ConnectAsync(host, port);
        }

        public async Task<Packet> ReceiveAsync()
        {
            byte[] buffer = new byte[PacketHandler.BufferSize];
            await Socket.ReceiveAsync(buffer, 0, PacketHandler.BufferSize);
            Packet packet = new Packet(buffer);
            packet.Decode();
            return packet;
        }
    }
}

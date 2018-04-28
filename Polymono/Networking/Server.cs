using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Server
    {
        public ISocket Socket;
        public Dictionary<int, ISocket> Clients;
        private int clientIDIndexer = 0;

        public Server(bool v6 = true)
        {
            Socket = new PolySocket(v6);
            Clients = new Dictionary<int, ISocket>();
        }

        public async Task<ISocket> AcceptAsync()
        {
            ISocket socket = await Socket.AcceptAsync();
            Clients.Add(clientIDIndexer++, socket);
            return socket;
        }

        public async Task Send(string text = "Hi.")
        {
            Packet[] packets = PacketHandler.Create(PacketType.Message, text);
            foreach (var client in Clients.Values)
            {
                int packetID = 0;
                foreach (var packet in packets)
                {
                    Polymono.Debug($"Preparing packet[{packetID}] to send to client: {client.GetSocket().RemoteEndPoint}");
                    await client.SendAsync(packet.ByteBuffer, 0, packet.ByteBuffer.Length);
                    Polymono.Debug($"Packet[{packetID++}] sent to client: {client.GetSocket().RemoteEndPoint}");
                }
            }
        }
    }
}

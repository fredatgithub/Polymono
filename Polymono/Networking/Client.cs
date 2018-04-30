using Polymono.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Client : INetwork
    {
        // Game client reference.
        public GameClient GameClient;
        public ISocket NetworkHandler;
        // Packet queue (First in, first out)
        public Queue<PacketData> Packets;
        // Current ID of client.
        public int ID = 0;
        public bool V6;

        public Client(GameClient gameClient, bool v6 = true)
        {
            GameClient = gameClient;
            V6 = v6;
            NetworkHandler = new SocketHandler(v6);
            Packets = new Queue<PacketData>();
            Polymono.Debug("Client initialised.");
        }

        public ref Player[] GetClients()
        {
            return ref GameClient.Board.Players;
        }

        public ref Queue<PacketData> GetPacketQueue()
        {
            return ref Packets;
        }

        public ref ISocket LocalHandler()
        {
            if (GetClients()?[ID]?.NetworkHandler() != null)
            {
                return ref GetClients()[ID].NetworkHandler();
            } else
            {
                return ref NetworkHandler;
            }
        }

        public async Task<bool> ConnectAsync(string host, int port, string name)
        {
            // Await connection to server.
            Polymono.Debug($"Begin connection to server: [{host}]:{port}");
            await LocalHandler().ConnectAsync(host, port);
            Polymono.Debug("Send connection data to server: " + LocalHandler().GetSocket().RemoteEndPoint);
            await SendAsync(PacketHandler.Create(PacketType.Connect, name));
            Polymono.Debug("Receive response to connection request: " + LocalHandler().GetSocket().RemoteEndPoint);
            await ReceiveAsync();
            // Check responses in queue.
            PacketData packet = GetPacketQueue().Dequeue();
            bool sucess = Protocol.DecodeConnectionResponseSuccess(packet.Data);
            if (sucess)
            {
                int id = Protocol.DecodeConnectionResponseID(packet.Data);
                GetClients()[id] = GetClients()[ID];
                ID = id;
                Polymono.Debug("Connection to server successful: " + LocalHandler().GetSocket().RemoteEndPoint);
                return true;
            }
            Polymono.Debug("Connection to server failed: " + LocalHandler().GetSocket().RemoteEndPoint);
            return false;
        }

        public async Task SendAsync(params Packet[] packets)
        {
            foreach (var packet in packets)
            {
                Polymono.Debug($"Preparing packet to send to server: {LocalHandler().GetSocket().RemoteEndPoint}");
                await LocalHandler().SendAsync(packet.ByteBuffer, 0, packet.ByteBuffer.Length);
                Polymono.Debug($"Packet sent to client: {LocalHandler().GetSocket().RemoteEndPoint}");
            }
        }

        /// <summary>
        /// Receive a single chain of packets; then runs the method again.
        /// </summary>
        public async Task ReceiveAsync()
        {
            Polymono.Debug("Receiving started.");
            PacketData packetData = new PacketData();
            while (true)
            {
                byte[] buffer = new byte[PacketHandler.BufferSize];
                await LocalHandler().ReceiveAsync(buffer, 0, PacketHandler.BufferSize);
                Packet packet = new Packet(buffer);
                packet.Decode();
                packetData.Data += packet.DataBuffer;
                packetData.Type = packet.Type;
                if (packet.Terminate)
                {
                    Packets.Enqueue(packetData);
                    Polymono.Debug("Received full packet.");
                    break;
                }
                Polymono.Debug("Received partial packet.");
            }
            await ReceiveAsync();
        }
    }
}

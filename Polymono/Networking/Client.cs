using Polymono.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
        public Queue<Packet> Packets;
        // Current ID of client.
        public int ID = 0;
        public bool V6;

        public Client(GameClient gameClient, bool v6 = true)
        {
            GameClient = gameClient;
            V6 = v6;
            NetworkHandler = new SocketHandler(v6);
            Packets = new Queue<Packet>();
            Polymono.Debug("Client initialised.");
        }

        public ref Player[] GetClients()
        {
            return ref GameClient.Board.Players;
        }

        public ref Queue<Packet> GetPacketQueue()
        {
            return ref Packets;
        }

        public ref ISocket LocalHandler()
        {
            if (GetClients()?[ID]?.NetworkHandler() != null)
            {
                return ref GetClients()[ID].NetworkHandler();
            }
            else
            {
                return ref NetworkHandler;
            }
        }

        public async void ConnectAsync(string host, int port, string name)
        {
            // Await connection to server.
            Polymono.Debug($"Begin connection to server: [{host}]:{port}");
            await LocalHandler().ConnectAsync(host, port);
            Polymono.Debug("Send connection data to server: " + LocalHandler().GetSocket().RemoteEndPoint);
            await SendAsync(PacketHandler.Create(PacketType.Connect, 0,
                Protocol.Connection.EncodeRequest(name)));
            Polymono.Debug("Receive response to connection request: " + LocalHandler().GetSocket().RemoteEndPoint);
            Packet packet = await ReceiveAsync();
            // Check responses in queue.
            bool success = Protocol.Connection.DecodeResponseSuccess(packet.Data);
            if (success)
            {
                int id = Protocol.Connection.DecodeResponseID(packet.Data);
                GameClient.Board.CurrentPlayerID = id;
                // Create player objects upon server sync.
                for (int i = 0; i <= id; i++)
                {
                    Packet clientSync = await ReceiveAsync();
                    int clientID = Protocol.Connection.DecodeClientID(clientSync.Data);
                    string clientName = Protocol.Connection.DecodeClientName(clientSync.Data);
                    GameClient.Board.AddPlayer(clientID, clientName);
                }
                Polymono.Debug("Connection to server successful: " + LocalHandler().GetSocket().RemoteEndPoint);
                while (true)
                {
                    if (LocalHandler().GetSocket().Connected)
                    {
                        try
                        {
                            Packet temp = await ReceiveAsync();
                            Packets.Enqueue(temp);
                        }
                        catch (IOException)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Polymono.Debug("Connection to server failed: " + LocalHandler().GetSocket().RemoteEndPoint);
        }

        public async Task SendAsync(params Packet[] packets)
        {
            foreach (var packet in packets)
            {
                Polymono.Debug($"Preparing packet to send to server: {LocalHandler().GetSocket().RemoteEndPoint}");
                await LocalHandler().SendAsync(packet.Bytes, 0, packet.Bytes.Length);
                Polymono.Debug($"Packet sent to client: {LocalHandler().GetSocket().RemoteEndPoint}");
            }
        }

        public async Task<Packet> ReceiveAsync()
        {
            Polymono.Debug($"Receiving started from: {LocalHandler().GetSocket().RemoteEndPoint}");
            Packet outputPacket = null;
            while (true)
            {
                byte[] buffer = new byte[PacketHandler.BufferSize];
                try
                {
                    if (LocalHandler().GetSocket().Connected)
                    {
                        await LocalHandler().ReceiveAsync(buffer, 0, PacketHandler.BufferSize);
                    }
                    else
                    {
                        throw new IOException();
                    }
                }
                catch (SocketException se)
                {
                    Polymono.Error(se.Message);
                    Polymono.ErrorF(se.StackTrace);
                    throw se;
                }
                catch (IOException ioe)
                {
                    Polymono.Error(ioe.Message);
                    Polymono.ErrorF(ioe.StackTrace);
                    // TODO: Disconnect client, unmanaged disconnect.
                    throw ioe;
                }
                Polymono.Debug($"Packet received: {LocalHandler().GetSocket().RemoteEndPoint}");
                // Build packet from buffer data.
                Packet bufferPacket = new Packet(buffer);
                if (outputPacket == null)
                {
                    // Create output packet.
                    Polymono.Debug($"Creating packet data: {LocalHandler().GetSocket().RemoteEndPoint}");
                    outputPacket = bufferPacket;
                }
                else
                {
                    // Append packet info
                    Polymono.Debug($"Appending packet data: {LocalHandler().GetSocket().RemoteEndPoint}");
                    outputPacket.AppendData(bufferPacket.Data);
                }
                if (bufferPacket.Terminate)
                {
                    Polymono.Debug($"Finalised packet receiving: {LocalHandler().GetSocket().RemoteEndPoint}");
                    return outputPacket;
                }
            }
        }

        ///// <summary>
        ///// Receive a single chain of packets; then runs the method again.
        ///// </summary>
        //public async void ReceiveAsync()
        //{
        //    Polymono.Debug($"Receiving started from: {LocalHandler().GetSocket().RemoteEndPoint}");
        //    Packet outputPacket = null;
        //    while (true)
        //    {
        //        byte[] buffer = new byte[PacketHandler.BufferSize];
        //        try
        //        {
        //            if (LocalHandler().GetSocket().Connected)
        //            {
        //                await LocalHandler().ReceiveAsync(buffer, 0, PacketHandler.BufferSize);
        //            }
        //            else
        //            {
        //                throw new IOException();
        //            }
        //        }
        //        catch (SocketException se)
        //        {
        //            Polymono.Error(se.Message);
        //            Polymono.ErrorF(se.StackTrace);
        //            throw se;
        //        }
        //        catch (IOException ioe)
        //        {
        //            Polymono.Error(ioe.Message);
        //            Polymono.ErrorF(ioe.StackTrace);
        //            // TODO: Disconnect client, unmanaged disconnect.
        //            throw ioe;
        //        }
        //        Polymono.Debug($"Packet received: {LocalHandler().GetSocket().RemoteEndPoint}");
        //        // Build packet from buffer.
        //        Packet bufferPacket = new Packet(buffer);
        //        if (outputPacket == null)
        //        {
        //            // Create output packet.
        //            Polymono.Debug($"Creating packet data: {LocalHandler().GetSocket().RemoteEndPoint}");
        //            outputPacket = bufferPacket;
        //        }
        //        else
        //        {
        //            // Append packet info
        //            Polymono.Debug($"Appending packet data: {LocalHandler().GetSocket().RemoteEndPoint}");
        //            outputPacket.AppendData(bufferPacket.Data);
        //        }
        //        if (bufferPacket.Terminate)
        //        {
        //            Polymono.Debug($"Finalised packet receiving: {LocalHandler().GetSocket().RemoteEndPoint}");
        //            Packets.Enqueue(outputPacket);
        //            break;
        //        }
        //    }
        //    ReceiveAsync();
        //}

        public void DisconnectClient(int i)
        {
            // Remove disconnected client from list of clients.
            GetClients()[i] = null;
        }
    }
}

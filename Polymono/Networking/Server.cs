﻿using Polymono.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Server : INetwork
    {
        // Game client reference.
        public GameClient GameClient;
        // Network sockets for clients.
        //public ISocket[] Clients;
        // Packet queue (First in, first out)
        public Queue<PacketData> Packets;
        public bool WaitForClients = true;
        public int ID = 0;
        public bool V6;

        public Server(GameClient gameClient, string name, bool v6 = true)
        {
            GameClient = gameClient;
            V6 = v6;
            //Clients = new ISocket[Polymono.MaxPlayers];
            //Clients[ID] = new SocketHandler(v6);
            GameClient.Board.AddPlayer(ID, name);
            GetClients()[ID].SetNetworkHandle(new SocketHandler(v6));
            Packets = new Queue<PacketData>();
            Polymono.Debug("Server initialised. [IPv6: " + v6 + "]");
        }

        public ref Player[] GetClients()
        {
            return ref GameClient.Board.Players;
        }

        public ref Queue<PacketData> GetPacketQueue()
        {
            return ref Packets;
        }

        public ref ISocket RemoteHandler(int id)
        {
            if (GetClients()?[id]?.NetworkHandler() != null)
            {
                return ref GetClients()[id].NetworkHandler();
            }
            throw new Exception("Local network handler is null.");
        }

        public ref ISocket LocalHandler()
        {
            if (GetClients()?[ID]?.NetworkHandler() != null)
            {
                return ref GetClients()[ID].NetworkHandler();
            }
            throw new Exception("Local network handler is null.");
        }

        public void Start()
        {
            for (int i = 1; i < Polymono.MaxPlayers; i++)
            {
                // For every possible client...
                // Begin accepting process.
                AcceptFromAsync(i);
            }
            // Finish when accepting processes have all started.
            // It is possible some processes will not finish.
        }

        public async void AcceptFromAsync(int i)
        {
            if (GetClients()[i] == null)
            {
                Polymono.Debug($"Awaiting a connecting client[{i}]...");
                ISocket remoteHandler = await LocalHandler().AcceptAsync();
                // Create new player
                Polymono.Debug($"Client[{i}] connected: {remoteHandler.GetSocket().RemoteEndPoint}");
                // Begin receiving from the client.
                PacketData packet = await ReceiveFromAsync(remoteHandler);
                string name = Protocol.Connection.DecodeConnectionRequest(packet.Data);
                Polymono.Debug($"Client[{i}] requesting name: " + name);
                bool success = true;
                foreach (var client in GetClients())
                {
                    if (client != null)
                    {
                        if (name == client.PlayerName)
                        {
                            success = false;
                        }
                    }
                }
                if (success)
                {
                    Polymono.Debug($"Client[{i}] successfully connected: {remoteHandler.GetSocket().RemoteEndPoint}");
                    await ConnectClient(i, remoteHandler, name);
                    while (true)
                    {
                        if (remoteHandler.GetSocket().Connected)
                        {
                            try
                            {
                                PacketData temp = await ReceiveFromAsync(remoteHandler);
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
                    // Manage a disconnection.
                    DisconnectClient(i);
                }
                else
                {
                    Polymono.Debug($"Client[{i}] failed to connect: {remoteHandler.GetSocket().RemoteEndPoint}");
                    await SendToAsync(remoteHandler, PacketHandler.Create(PacketType.Connect, i,
                        Protocol.Connection.EncodeConnectionResponse(success, i)));
                    AcceptFromAsync(i);
                }
            }
        }

        public async Task ConnectClient(int i, ISocket socket, string name)
        {
            // Setup player in list of clients.
            GameClient.Board.AddPlayer(i, name);
            GetClients()[i].NetworkHandler() = socket;
            // Send connection notice to all clients.
            await SendAsync(PacketHandler.Create(PacketType.Connect, i,
                Protocol.Connection.EncodeConnectionResponse(true, i)));
        }

        public async void DisconnectClient(int i)
        {
            // Remove disconnected client from list of clients.
            GetClients()[i] = null;
            // Send disconnection notice to all clients.
            await SendAsync(PacketHandler.Create(PacketType.Disconnect,
                Protocol.Connection.EncodeDisconnect(i)));
        }

        public async Task SendToAsync(ISocket socket, params Packet[] packets)
        {
            foreach (var packet in packets)
            {
                // Send to socket.
                await SendAsync(socket, packet);
            }
        }

        public async Task SendAsync(params Packet[] packets)
        {
            foreach (var packet in packets)
            {
                // Send to all
                if (packet.TargetID == int.MaxValue)
                {
                    Player[] clients = GetClients();
                    // Send to all but server
                    for (int i = 1; i < clients.Length; i++)
                    {
                        Player player = clients[i];
                        if (player?.NetworkHandler() != null)
                        {
                            await SendAsync(GetClients()[i].NetworkHandler(), packet);
                        }
                    }
                }
                else
                {
                    await SendAsync(RemoteHandler(packet.TargetID), packet);
                }
            }
        }

        public async Task SendAsync(ISocket socket, Packet packet)
        {
            Polymono.Debug($"Preparing packet to send to client: {socket.GetSocket().RemoteEndPoint}");
            await socket.SendAsync(packet.ByteBuffer, 0, packet.ByteBuffer.Length);
            Polymono.Debug($"Packet sent to client: {socket.GetSocket().RemoteEndPoint}");
        }

        /// <summary>
        /// Receive a single chain of packets from a client.
        /// </summary>
        public async Task<PacketData> ReceiveFromAsync(ISocket socket)
        {
            Polymono.Debug($"Receiving started from: {socket.GetSocket().RemoteEndPoint}");
            PacketData packetData = new PacketData();
            bool terminate = false;
            while (!terminate)
            {
                #region Create buffer then receive
                // Create buffer for this packet.
                byte[] buffer = new byte[PacketHandler.BufferSize];
                // Await for information from client.
                try
                {
                    if (socket.GetSocket().Connected)
                    {
                        await socket.ReceiveAsync(buffer, 0, PacketHandler.BufferSize);
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
                Polymono.Debug($"Packet received: {socket.GetSocket().RemoteEndPoint}");
                #endregion
                #region Analyse packet data
                // Build packet from buffer data.
                Packet packet = new Packet(buffer);
                packet.Decode();
                #endregion
                #region Forward packet if needed
                // Figure out where to send the packet, if appropriate.
                if (packet.TargetID == int.MaxValue)
                {
                    for (int i = 1; i < GetClients().Length; i++)
                    {
                        if (GetClients()[i] != null && socket.GetSocket().RemoteEndPoint != RemoteHandler(i).GetSocket().RemoteEndPoint)
                        {
                            Polymono.Debug($"Forwarding packet to: {RemoteHandler(i).GetSocket().RemoteEndPoint}");
                            await SendAsync(RemoteHandler(i), packet);
                        }
                    }
                }
                else if (packet.TargetID != 0)
                {
                    if (GetClients()[packet.TargetID] != null)
                    {
                        Polymono.Debug($"Forwarding packet to: {RemoteHandler(packet.TargetID).GetSocket().RemoteEndPoint}");
                        await SendAsync(RemoteHandler(packet.TargetID), packet);
                    }
                }
                #endregion
                #region Return packet information
                if (packet.TargetID == int.MaxValue || packet.TargetID == 0)
                {
                    Polymono.Debug($"Appending packet data: {socket.GetSocket().RemoteEndPoint}");
                    packetData.Data += packet.DataBuffer;
                    packetData.Type = packet.Type;
                    terminate = packet.Terminate;
                }
                #endregion
            }
            Polymono.Debug($"Finalised packet receiving: {socket.GetSocket().RemoteEndPoint}");
            return packetData;
        }
    }
}

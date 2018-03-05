using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Server : INetwork
    {
        /// <summary>
        /// Server end point for all clients to connect to.
        /// </summary>
        public Socket GlobalEndPoint;
        public Dictionary<int, SocketState> ConnectedUsers;

        public Server()
        {
            ConnectedUsers = new Dictionary<int, SocketState>();
        }

        public void Start(int port)
        {
            try
            {
                // Create IP End point information for the local end point.
                IPEndPoint localEP = new IPEndPoint(IPAddress.IPv6Any, port);
                // Open/Bind a socket allowing connections to our local end point.
                GlobalEndPoint = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                GlobalEndPoint.Bind(localEP);
                GlobalEndPoint.Listen(16);
                Polymono.Print($"Server started on {GlobalEndPoint.LocalEndPoint}");
                // Start receiving.
                GlobalEndPoint.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
            }
        }

        public void Send(Packet[] packets, AsyncCallback callback)
        {
            foreach (Packet packet in packets)
            {
                if (packet.TargetID == int.MaxValue)
                {
                    foreach (SocketState state in ConnectedUsers.Values)
                    {
                        if (state.ID != 0)
                        {
                            // Begin to send packets to all connected users.
                            state.NetworkCallback = callback;
                            state.RemoteEndPoint.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                                SocketFlags.None, ReceiveCallback, state);
                        }
                    }
                }
                else
                {
                    if (ConnectedUsers.ContainsKey(packet.TargetID))
                    {
                        SocketState state = ConnectedUsers[packet.TargetID];
                        state.NetworkCallback = callback;
                        state.RemoteEndPoint.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                            SocketFlags.None, new AsyncCallback(SendCallback), state);
                    }
                }
            }
        }

        public void SendFrom(int fromID, params Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                if (packet.TargetID == int.MaxValue)
                {
                    foreach (SocketState state in ConnectedUsers.Values)
                    {
                        if (state.ID != 0 && state.ID != fromID)
                        {
                            Socket handler = state.RemoteEndPoint;
                            handler.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                                SocketFlags.None, new AsyncCallback(SendCallback), handler);
                        }
                    }
                }
                else
                {
                    if (ConnectedUsers.ContainsKey(packet.TargetID))
                    {
                        Socket handler = ConnectedUsers[packet.TargetID].RemoteEndPoint;
                        handler.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                            SocketFlags.None, new AsyncCallback(SendCallback), handler);
                    }
                }
            }
        }

        public void Exit()
        {

        }

        /// <summary>
        /// Process a client connecting to server end point.
        /// </summary>
        /// <param name="ar">Nullified parameter; no use for a state.</param>
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.
                Socket handler = GlobalEndPoint.EndAccept(ar);
                Polymono.Debug($"Client connected from {handler.RemoteEndPoint}");
                // Create the state object.
                SocketState state = new SocketState()
                {
                    ID = ConnectedUsers.Count,
                    RemoteEndPoint = handler
                };
                // TODO: Run method to propagate client to all clients.
                ConnectedUsers.Add(state.ID, state);
                // Check if more users can join.
                if (ConnectedUsers.Count < Polymono.MaxPlayers)
                {
                    // Begin accepting for more clients.
                    GlobalEndPoint.BeginAccept(new AsyncCallback(AcceptCallback), GlobalEndPoint);
                }
                // Being receiving.
                handler.BeginReceive(state.ByteBuffer, 0, PacketHandler.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
            }
        }

        /// <summary>
        /// Process a client sending a message to server end point.
        /// </summary>
        /// <param name="ar">Asynchronous result to keep track of current state.</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            Polymono.Debug("Server::ReceiveCallback");
            // Retrieve the state object and the server socket   
            // from the asynchronous state object.  
            SocketState state = (SocketState)ar.AsyncState;
            Socket handler = state.RemoteEndPoint;
            try
            {
                // Read data from the remote device.
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // Create packet then decode from byte buffer.
                    Packet packet = new Packet(state.ByteBuffer);
                    packet.Decode();
                    // Forward packets to users if necessary.
                    if (packet.TargetID != 0)
                    {
                        SendFrom(state.ID, packet);
                    }
                    // Append packet data to socket data buffer.
                    state.DataBuffer.Append(packet.DataBuffer);
                    // Check for terminator.
                    if (packet.Terminate)
                    {
                        // Packet is last of chain.
                        state.NetworkCallback(ar);
                        // Reset buffers in state object.
                        state.DataBuffer.Clear();
                    }
                }
                else
                {
                    Polymono.Error("No bytes sent from client.");
                    Polymono.Error($"---------- Stopping all networking on {handler.RemoteEndPoint}. ----------");
                    return;
                }
                // Reset byte buffer.
                state.ByteBuffer = new byte[PacketHandler.BufferSize];
                // Begin receiving the data from the remote device.  
                handler.BeginReceive(state.ByteBuffer, 0, PacketHandler.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
            }
        }

        /// <summary>
        /// Process a packet being sent to a remote end point.
        /// </summary>
        /// <param name="ar">Asynchronous result to keep track of current state.</param>
        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.  
            SocketState state = (SocketState)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = state.RemoteEndPoint.EndSend(ar);
                Polymono.DebugF($"Sent {bytesSent} bytes to client.");
            }
            catch(Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
            }
        }
    }
}

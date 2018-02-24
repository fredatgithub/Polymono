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
        public Socket LocalSocket;
        public Dictionary<int, SocketState> ConnectedUsers;

        public Server()
        {
            ConnectedUsers = new Dictionary<int, SocketState>();
        }

        public void Start(int port)
        {
            Polymono.Debug("Server::Start");
            try
            {
                // Create IP End point information for the local end point.
                IPEndPoint localEP = new IPEndPoint(IPAddress.IPv6Any, port);
                Polymono.Debug($"Host End Point: {localEP} Type: {localEP.AddressFamily}");
                // Open/Bind a socket allowing connections to our local end point.
                LocalSocket = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                LocalSocket.Bind(localEP);
                LocalSocket.Listen(16);
                SocketState state = new SocketState()
                {
                    RemoteEndPoint = LocalSocket
                };
                AddClient(state);
                Polymono.Debug($"Server ID: {state.ID}");
                // Start receiving.
                LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), LocalSocket);
            }
            catch (Exception e)
            {
                Polymono.Debug(e.ToString());
            }
        }

        public void AddClient(SocketState state)
        {
            state.ID = ConnectedUsers.Count;
            ConnectedUsers.Add(ConnectedUsers.Count, state);
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
                            Polymono.Debug($"Send stateobject test ID: {state.TestID}");
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
            Polymono.Debug("Server::Exit");
            if (LocalSocket.Connected)
            {
                LocalSocket.Shutdown(SocketShutdown.Both);
            }
            LocalSocket.Close();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Polymono.Debug("Server::AcceptCallback");
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                // Complete the connection.
                Socket handler = listener.EndAccept(ar);
                Polymono.Debug($"Client connected from {handler.RemoteEndPoint}");
                // Create the state object.
                SocketState state = new SocketState
                {
                    RemoteEndPoint = handler
                };
                // Run method to propagate client to all clients.
                AddClient(state);
                // Check if more users can join.
                if (ConnectedUsers.Count < Polymono.MaxPlayers)
                {
                    // Begin accepting.
                    LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), LocalSocket);
                }
                // Being receiving.
                handler.BeginReceive(state.ByteBuffer, 0, PacketHandler.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Polymono.Debug(e.ToString());
            }
        }

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
                        // TODO: CALLBACK FUNCTION
                        Polymono.Debug($"State Object test ID: {state.TestID}");
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
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Polymono.Debug("Server::SendCallback");
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Polymono.DebugF($"Sent {bytesSent} bytes to client.");
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
            }
        }
    }
}

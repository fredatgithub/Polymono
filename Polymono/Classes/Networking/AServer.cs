using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Networking {
    abstract class AServer : INetwork {
        public Socket LocalSocket;
        public Dictionary<int, SocketState> ConnectedUsers;

        public abstract void Send(params Packet[] packets);

        public abstract void SendFrom(int fromID, params Packet[] packets);

        public abstract void AddClient(SocketState state);

        public abstract void Exit();

        protected void AcceptCallback(IAsyncResult ar)
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
                SocketState state = new SocketState {
                    Socket = handler
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
            } catch (Exception e)
            {
                Polymono.Debug(e.ToString());
            }
        }

        protected void ReceiveCallback(IAsyncResult ar)
        {
            Polymono.Debug("Server::ReceiveCallback");
            // Retrieve the state object and the server socket   
            // from the asynchronous state object.  
            SocketState state = (SocketState)ar.AsyncState;
            Socket handler = state.Socket;
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
                        // Reset buffers in state object.
                        state.DataBuffer.Clear();
                    }
                } else
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
            } catch (Exception e)
            {
                Polymono.Error(e.ToString());
            }
        }

        protected void SendCallback(IAsyncResult ar)
        {
            Polymono.Debug("Server::SendCallback");
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Polymono.DebugF($"Sent {bytesSent} bytes to client.");
            } catch (Exception e)
            {
                Polymono.Error(e.ToString());
            }
        }
    }
}

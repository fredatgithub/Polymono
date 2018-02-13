using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Networking {
    abstract class AClient : INetwork {
        public Socket LocalSocket;
        public Dictionary<int, SocketState> ConnectedUsers;

        public abstract void Send(params Packet[] data);

        public abstract void AddClient(SocketState state);

        public abstract void Exit();

        protected void ConnectCallback(IAsyncResult ar)
        {
            Polymono.Debug("Client::ConnectCallback");
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;
            try
            {
                // Complete the connection.
                client.EndConnect(ar);
                Polymono.Debug($"Connected to {client.RemoteEndPoint}");
                // Create the state object.
                SocketState state = new SocketState {
                    Socket = client
                };
                // Run method to add server to users.
                AddClient(state);
                // Begin receiving the data from the remote device.
                client.BeginReceive(state.ByteBuffer, 0, PacketHandler.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            } catch (Exception e)
            {
                Polymono.Error(e.ToString());
            }
        }

        protected void ReceiveCallback(IAsyncResult ar)
        {
            Polymono.Debug("Client::ReceiveCallback");
            // Retrieve the state object and the client socket   
            // from the asynchronous state object.  
            SocketState state = (SocketState)ar.AsyncState;
            Socket client = state.Socket;
            try
            {
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // Create packet then decode from byte buffer.
                    Packet packet = new Packet(state.ByteBuffer);
                    packet.Decode();
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
                    Polymono.Debug("No bytes sent from server.");
                }
                // Reset byte buffer.
                state.ByteBuffer = new byte[PacketHandler.BufferSize];
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.ByteBuffer, 0, PacketHandler.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            } catch (Exception e)
            {
                Polymono.Error(e.ToString());
            }
        }

        protected void SendCallback(IAsyncResult ar)
        {
            Polymono.Debug("Client::SendCallback");
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Polymono.DebugF($"Sent {bytesSent} bytes to server.");
            } catch (Exception e)
            {
                Polymono.Error(e.ToString());
            }
        }
    }
}

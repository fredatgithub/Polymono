using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking {
    class Client : INetwork {
        public Socket LocalSocket;
        public Dictionary<int, SocketState> ConnectedUsers;

        public Client()
        {
            ConnectedUsers = new Dictionary<int, SocketState>();
        }

        public void Start(string address, int port)
        {
            Polymono.Debug("Client::Start");
            try
            {
                IPAddress ipAddress = IPAddress.Parse(address);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(address), port);
                Polymono.Debug($"Host End Point: {remoteEP} Type: {remoteEP.AddressFamily}");
                // Create a TCP/IP socket.
                LocalSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.  
                LocalSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), LocalSocket);
            }
            catch (Exception e)
            {
                Polymono.Debug(e.ToString());
            }
        }

        public void AddClient(SocketState state)
        {
            ConnectedUsers.Add(ConnectedUsers.Count, state);
        }

        public void Send(Packet[] packets, AsyncCallback p)
        {
            foreach (Packet packet in packets)
            {
                LocalSocket.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                    SocketFlags.None, p, LocalSocket);
            }
        }

        public void Exit()
        {
            Polymono.Debug("Client::Exit");
        }

        private void ConnectCallback(IAsyncResult ar)
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
                    RemoteEndPoint = client
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

        private void ReceiveCallback(IAsyncResult ar)
        {
            Polymono.Debug("Client::ReceiveCallback");
            // Retrieve the state object and the client socket
            // from the asynchronous state object.
            SocketState state = (SocketState)ar.AsyncState;
            Socket client = state.RemoteEndPoint;
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

        private void SendCallback(IAsyncResult ar)
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

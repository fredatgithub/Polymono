using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Client : INetwork
    {
        public const int ServerID = 0;
        public Dictionary<int, SocketState> ConnectedUsers;

        public Client()
        {
            ConnectedUsers = new Dictionary<int, SocketState>();
        }

        public void Start(string address, int port)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(address);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(address), port);
                Polymono.Debug($"Host End Point: {remoteEP} Type: {remoteEP.AddressFamily}");
                // Create a TCP/IP socket.
                SocketState state = new SocketState()
                {
                    RemoteEndPoint = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                };
                // Connect to the remote endpoint.  
                state.RemoteEndPoint.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), state);
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
            }
        }

        public void Send(Packet[] packets, AsyncCallback p)
        {
            //foreach (Packet packet in packets)
            //{
            //    LocalSocket.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
            //        SocketFlags.None, p, LocalSocket);
            //}
        }

        public void Exit()
        {

        }

        private void ConnectCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            SocketState state = (SocketState)ar.AsyncState;
            Socket handler = state.RemoteEndPoint;
            try
            {
                // Complete the connection.
                handler.EndConnect(ar);
                // Add server to connected user dictionary.
                ConnectedUsers.Add(ServerID, state);
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

        private void ReceiveCallback(IAsyncResult ar)
        {
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
                }
                else
                {
                    Polymono.Debug("No bytes sent from server.");
                }
                // Reset byte buffer.
                state.ByteBuffer = new byte[PacketHandler.BufferSize];
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.ByteBuffer, 0, PacketHandler.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
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
            }
            catch (Exception e)
            {
                Polymono.Error(e.ToString());
                Polymono.Debug(e.StackTrace);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polymono.Networking {
    class Client : AClient {
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
            } catch (Exception e)
            {
                Polymono.Debug(e.ToString());
            }
        }

        public override void Send(params Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                LocalSocket.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                    SocketFlags.None, new AsyncCallback(SendCallback), LocalSocket);
            }
        }

        public override void AddClient(SocketState state)
        {
            ConnectedUsers.Add(ConnectedUsers.Count, state);
        }

        public override void Exit()
        {
            Polymono.Debug("Client::Exit");
        }
    }
}

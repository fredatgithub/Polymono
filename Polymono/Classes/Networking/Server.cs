using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polymono.Classes.Networking {
    class Server : AServer {
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
                SocketState state = new SocketState() {
                    Socket = LocalSocket
                };
                AddClient(state);
                Polymono.Debug($"Server ID: {state.ID}");
                // Start receiving.
                LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), LocalSocket);
            } catch (Exception e)
            {
                Polymono.Debug(e.ToString());
            }
        }
        
        public override void Send(params Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                if (packet.TargetID == int.MaxValue)
                {
                    foreach (SocketState state in ConnectedUsers.Values)
                    {
                        if (state.ID != 0)
                        {
                            Socket handler = state.Socket;
                            handler.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                                SocketFlags.None, new AsyncCallback(SendCallback), handler);
                        }
                    }
                } else
                {
                    if (ConnectedUsers.ContainsKey(packet.TargetID))
                    {
                        Socket handler = ConnectedUsers[packet.TargetID].Socket;
                        handler.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                            SocketFlags.None, new AsyncCallback(SendCallback), handler);
                    }
                }
            }
        }

        public override void SendFrom(int fromID, params Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                if (packet.TargetID == int.MaxValue)
                {
                    foreach (SocketState state in ConnectedUsers.Values)
                    {
                        if (state.ID != 0 && state.ID != fromID)
                        {
                            Socket handler = state.Socket;
                            handler.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                                SocketFlags.None, new AsyncCallback(SendCallback), handler);
                        }
                    }
                } else
                {
                    if (ConnectedUsers.ContainsKey(packet.TargetID))
                    {
                        Socket handler = ConnectedUsers[packet.TargetID].Socket;
                        handler.BeginSend(packet.ByteBuffer, 0, packet.ByteBuffer.Length,
                            SocketFlags.None, new AsyncCallback(SendCallback), handler);
                    }
                }
            }
        }

        public override void AddClient(SocketState state)
        {
            state.ID = ConnectedUsers.Count;
            ConnectedUsers.Add(ConnectedUsers.Count, state);
        }

        public override void Exit()
        {
            Polymono.Debug("Server::Exit");
            if (LocalSocket.Connected)
            {
                LocalSocket.Shutdown(SocketShutdown.Both);
            }
            LocalSocket.Close();
        }
    }
}

/*
 * 1. Create server object.                                         new Server()
 * 2. Start server - Create socket for all clients to connect to.   Start()
 * 3. Begin receiving for connections.                              BeginAccept()
 *  ║   3.1. Accept socket information from connecting client.      AcceptCallback()
 *  ║   3.2. Store socket information in Sockets dictionary.        AddClient()
 *  ║   3.3. Send other clients the client handles                  AddClient()
 *  ╚══ 3.4. Begin receiving to client socket.                      BeginReceive()
 *      ------------------ Client created on server ------------------
 * 4. Receive data from client                                      ReceiveCallback()
 *  ╚══ 4.1. Forward to other client(s) if necessary                Send()
 *      ----- OR ------
 * 5. Send data to client                                           Send()
 *      ----- OR ------
 * 6. Send data to all clients                                      Send()
 * 
 */

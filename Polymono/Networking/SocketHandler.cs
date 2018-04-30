using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    public interface ISocket : IDisposable
    {
        void Bind(int port);
        void Listen(int backlog);
        Socket GetSocket();
        Task ConnectAsync(string host, int port);
        Task<ISocket> AcceptAsync();
        Task SendAsync(byte[] buffer, int offset, int count);
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
    }

    class SocketHandler : ISocket
    {
        public Socket _socket;

        public SocketHandler(bool v6 = true)
        {
            _socket = new Socket(v6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private SocketHandler(Socket socket)
        {
            _socket = socket;
        }

        public void Bind(int port)
        {
            var endPoint = new IPEndPoint(_socket.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, port);
            _socket.Bind(endPoint);
        }

        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        public Socket GetSocket()
        {
            return _socket;
        }

        public async Task ConnectAsync(string host, int port)
        {
            await Task.Factory.FromAsync(_socket.BeginConnect, _socket.EndConnect, host, port, null);
        }

        public async Task<ISocket> AcceptAsync()
        {
            return new SocketHandler(await Task.Factory.FromAsync(_socket.BeginAccept, _socket.EndAccept, true));
        }

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            using (var stream = new NetworkStream(_socket))
            {
                await stream.WriteAsync(buffer, offset, count);
            }
        }

        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            using (var stream = new NetworkStream(_socket))
            {
                return await stream.ReadAsync(buffer, offset, count);
            }
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}

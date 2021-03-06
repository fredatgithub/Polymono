﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    /// <summary>
    /// Represents an enumeration of a socket closing state.
    /// </summary>
    public enum ClosingState
    {
        NotClosing,
        SendingShutdownRequest,
        ReceivingShutdownRequest,
        SendingFinalData,
        ReceivingFinalData,
        FinaliseShutdown
    }

    /// <summary>
    /// State object for reading client data asynchronously.
    /// </summary>
    class SocketState
    {
        /// <summary>
        /// Local socket ID.
        /// </summary>
        public int ID = 0;
        /// <summary>
        /// Transmission handler to remote end point on the network.
        /// </summary>
        public Socket RemoteEndPoint;
        /// <summary>
        /// Return method after packets have been processed.
        /// </summary>
        public AsyncCallback NetworkCallback;
        /// <summary>
        /// Buffer of bit data ready to be sent/received across the network.
        /// <para>Initialised capacity of PacketHandler.BufferSize</para>
        /// </summary>
        public byte[] ByteBuffer = new byte[PacketHandler.BufferSize];
        /// <summary>
        /// Buffer of string data ready to be processed into bits or read.
        /// </summary>
        public StringBuilder DataBuffer = new StringBuilder();
        /// <summary>
        /// The closing state of the end point.
        /// </summary>
        public ClosingState ClosingState = ClosingState.NotClosing;
        /// <summary>
        /// The success state.
        /// </summary>
        public bool ConnectSuccess = false;
    }
}

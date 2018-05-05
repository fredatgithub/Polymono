using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    enum PacketType
    {
        Null,
        Connect,
        Disconnect,
        Message,
        MoveState,
        Move,
        DiceRoll,
        StartGame,
        EndTurn,
        ClientSync,
    }

    class Packet
    {
        public byte[] Bytes;

        public PacketType Type;
        public int TargetID;
        public bool Terminate;
        public byte[] Data;

        public Packet(PacketType type, int targetID, bool terminate, byte[] bytes)
        {
            Type = type;
            TargetID = targetID;
            Terminate = terminate;
            Data = bytes;
            Bytes = new byte[PacketHandler.BufferSize];
            Serialise();
        }

        public Packet(byte[] buffer)
        {
            Bytes = buffer;
            Data = new byte[PacketHandler.DataSize];
            Deserialise();
        }

        public void AppendData(byte[] data)
        {
            byte[] temp = new byte[Data.Length + data.Length];
            for (int i = 0; i < Data.Length; i++)
                temp[i] = Data[i];
            for (int i = 0; i < data.Length; i++)
                temp[i + Data.Length] = data[i];
            Data = temp;
        }

        private void Serialise()
        {
            byte[] typeBytes = BitConverter.GetBytes((int)Type);
            byte[] targetIDBytes = BitConverter.GetBytes(TargetID);
            byte terminateBytes = Convert.ToByte(Terminate);
            for (int i = 0; i < typeBytes.Length; i++)
                Bytes[i] = typeBytes[i];
            for (int i = 0; i < targetIDBytes.Length; i++)
                Bytes[i + 4] = targetIDBytes[i];
            Bytes[8] = terminateBytes;
            for (int i = 0; i < Data.Length; i++)
                Bytes[i + 9] = Data[i];
            Polymono.Debug($"Packet serialised: {Environment.NewLine}    " +
                   $"[Type: {Type}->{PrintByteArray(typeBytes)}] {Environment.NewLine}    " +
                   $"[Target ID: {TargetID}->{PrintByteArray(targetIDBytes)}] {Environment.NewLine}    " +
                   $"[Terminate: {Terminate}->{terminateBytes}] {Environment.NewLine}    " +
                   $"[Data: {PrintByteArray(Data)}]");
        }

        private void Deserialise()
        {
            Type = (PacketType)BitConverter.ToInt32(Bytes, PacketHandler.TypeOffset);
            TargetID = BitConverter.ToInt32(Bytes, PacketHandler.TargetIDOffset);
            Terminate = BitConverter.ToBoolean(Bytes, PacketHandler.TerminateOffset);
            Buffer.BlockCopy(Bytes, PacketHandler.DataOffset, Data, 0, PacketHandler.DataSize);
            Polymono.Debug($"Packet deserialised: {Environment.NewLine}    " +
                   $"[Type: {Type}] {Environment.NewLine}    " +
                   $"[Target ID: {TargetID}] {Environment.NewLine}    " +
                   $"[Terminate: {Terminate}] {Environment.NewLine}    " +
                   $"[Data Bytes: {PrintByteArray(Data)}]");
        }

        private string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b + " ");
            }
            return sb.ToString();
        }
    }

    class PacketHandler
    {
        public const int BufferSize = 128;

        public const int TypeSize = 4;
        public const int TargetIDSize = 4;
        public const int TerminatorSize = 1;
        public const int DataSize = BufferSize - (TypeSize + TargetIDSize + TerminatorSize);

        public const int TypeOffset = 0;
        public const int TargetIDOffset = TypeOffset + TypeSize;
        public const int TerminateOffset = TargetIDOffset + TargetIDSize;
        public const int DataOffset = TerminateOffset + TerminatorSize;

        public static Packet[] Create(PacketType type, int targetID, byte[] data)
        {
            Polymono.Debug($"====== PacketHandler creation ======");
            Polymono.Debug($"Data length: {data.Length}");
            int PacketsToCreate = (data.Length / DataSize) + 1;
            Polymono.Debug($"Number of packets: {PacketsToCreate}");
            Packet[] packets = new Packet[PacketsToCreate];
            for (int i = 0; i < PacketsToCreate; i++)
            {
                int iterationOffset = (i * DataSize);
                int iterationSize = data.Length - iterationOffset;
                Polymono.Debug($"Interation: {i}");
                Polymono.Debug($"Interation offset: {iterationOffset}");
                Polymono.Debug($"Interation size: {iterationSize}");
                if (iterationSize < DataSize)
                {
                    // Last Packet to populate.
                    byte[] temp = new byte[iterationSize];
                    for (int n = 0; n < iterationSize; n++)
                    {
                        temp[n] = data[n + iterationOffset];
                    }
                    packets[i] = new Packet(type, targetID, true, temp);
                }
                else
                {
                    // There are more packets to populate.
                    // Create temporary set of bytes.
                    byte[] temp = new byte[DataSize];
                    for (int n = 0; n < DataSize; n++)
                    {
                        temp[n] = data[n + iterationOffset];
                    }
                    packets[i] = new Packet(type, targetID, false, temp);

                }
            }
            Polymono.Debug($"====== PacketHandler finished ======");
            return packets;
        }

        public static Packet[] Create(PacketType type, byte[] data)
        {
            return Create(type, int.MaxValue, data);
        }
    }
}

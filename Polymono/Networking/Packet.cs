using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking.Test
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
    }

    class Packet
    {
        // Data buffers.
        public PacketType Type;
        public int TargetID;
        public bool Terminate;
        public string DataBuffer;
        // Byte buffer.
        public byte[] ByteBuffer;

        public Packet(PacketType type, int targetID, bool terminate, string dataBuffer)
        {
            Type = type;
            TargetID = targetID;
            DataBuffer = dataBuffer;
            Terminate = terminate;
        }

        public Packet(byte[] byteBuffer)
        {
            ByteBuffer = byteBuffer;
        }

        public void Encode()
        {
            if (DataBuffer != null)
            {
                Polymono.DebugF("------------------------------START ENCODE--------------------------------");
                Polymono.DebugF($"Type: {Type}");
                Polymono.DebugF($"Target ID: {TargetID}");
                Polymono.DebugF($"Terminate: {Terminate}");
                Polymono.DebugF($"Data buffer: {DataBuffer}");
                Polymono.DebugF("Converting data to byte arrays...");
                // Do object->byte conversions.
                byte[] typeBytes = BitConverter.GetBytes((int)Type);
                byte[] targetIDBytes = BitConverter.GetBytes(TargetID);
                byte terminatorBytes = Convert.ToByte(Terminate);
                byte[] dataBytes = Encoding.UTF8.GetBytes(DataBuffer);
                // Create Byte Buffer allocation.
                ByteBuffer = new byte[PacketHandler.TypeSize + PacketHandler.TargetIDSize +
                    PacketHandler.TerminatorSize + PacketHandler.DataSize];
                // Append the Type onto the Byte Buffer.
                Buffer.BlockCopy(typeBytes, 0, ByteBuffer, 0, PacketHandler.TypeSize);
                // Append the Target ID onto the Byte Buffer.
                Buffer.BlockCopy(targetIDBytes, 0, ByteBuffer, PacketHandler.TypeSize, PacketHandler.TargetIDSize);
                // Append the Terminator onto the Byte Buffer.
                ByteBuffer[PacketHandler.TypeSize + PacketHandler.TargetIDSize] = terminatorBytes;
                // Append the Data Buffer onto the Byte Buffer.
                Buffer.BlockCopy(dataBytes, 0, ByteBuffer,
                    PacketHandler.TypeSize + PacketHandler.TargetIDSize + PacketHandler.TerminatorSize,
                    dataBytes.Length);
                Polymono.DebugF("Conversion to byte arrays finished.");
                Polymono.DebugF($"Type bytes: {PrintByteArray(typeBytes)}");
                Polymono.DebugF($"Target ID bytes: {PrintByteArray(targetIDBytes)}");
                Polymono.DebugF($"Data bytes: {PrintByteArray(dataBytes)}");
                Polymono.DebugF($"Terminator bytes: {terminatorBytes}");
                Polymono.DebugF($"Byte buffer: {PrintByteArray(ByteBuffer)}");
                Polymono.DebugF("-------------------------------END ENCODE---------------------------------");
            }
            else
            {
                Polymono.Print(ConsoleLevel.Warning, "Attempting to encode a packet with no data buffer.");
            }
        }

        public void Decode()
        {
            if (ByteBuffer != null)
            {
                Polymono.DebugF("------------------------------START DECODE--------------------------------");
                Polymono.DebugF($"Byte buffer: {PrintByteArray(ByteBuffer)}");
                Polymono.DebugF("Converting bytes to objects...");
                // Do byte->object conversions.
                Type = (PacketType)BitConverter.ToInt32(ByteBuffer, 0);
                TargetID = BitConverter.ToInt32(ByteBuffer, PacketHandler.TypeSize);
                Terminate = Convert.ToBoolean(
                    ByteBuffer[PacketHandler.TypeSize + PacketHandler.TargetIDSize]);
                DataBuffer = Encoding.UTF8.GetString(ByteBuffer,
                    PacketHandler.TypeSize + PacketHandler.TargetIDSize + PacketHandler.TerminatorSize,
                    PacketHandler.DataSize);
                DataBuffer = DataBuffer.Replace("\0", "");
                Polymono.DebugF("Conversion to objects finished.");
                Polymono.DebugF($"Type: {Type}");
                Polymono.DebugF($"Target ID: {TargetID}");
                Polymono.DebugF($"Terminate: {Terminate}");
                Polymono.DebugF($"DataBuffer: {DataBuffer}");
                Polymono.DebugF("-------------------------------END DECODE---------------------------------");
            }
            else
            {
                Polymono.Print(ConsoleLevel.Warning, "Attempting to encode a packet with no byte buffer.");
            }
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
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking {
    class Packet {
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
                Polymono.Debug("------------------------------START ENCODE--------------------------------");
                Polymono.Debug($"Type: {Type}");
                Polymono.Debug($"Target ID: {TargetID}");
                Polymono.Debug($"Terminate: {Terminate}");
                Polymono.Debug($"Data buffer: {DataBuffer}");
                Polymono.Debug("Converting data to byte arrays...");
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
                Polymono.Debug("Conversion to byte arrays finished.");
                Polymono.Debug($"Type bytes: {PrintByteArray(typeBytes)}");
                Polymono.Debug($"Target ID bytes: {PrintByteArray(targetIDBytes)}");
                Polymono.Debug($"Data bytes: {PrintByteArray(dataBytes)}");
                Polymono.Debug($"Terminator bytes: {terminatorBytes}");
                Polymono.Debug($"Byte buffer: {PrintByteArray(ByteBuffer)}");
                Polymono.Debug("-------------------------------END ENCODE---------------------------------");
            } else
            {
                Polymono.Print(ConsoleLevel.Warning, "Attempting to encode a packet with no data buffer.");
            }
        }

        public void Decode()
        {
            if (ByteBuffer != null)
            {
                Polymono.Debug("------------------------------START DECODE--------------------------------");
                Polymono.Debug($"Byte buffer: {PrintByteArray(ByteBuffer)}");
                Polymono.Debug("Converting bytes to objects...");
                // Do byte->object conversions.
                Type = (PacketType)BitConverter.ToInt32(ByteBuffer, 0);
                TargetID = BitConverter.ToInt32(ByteBuffer, PacketHandler.TypeSize);
                Terminate = Convert.ToBoolean(
                    ByteBuffer[PacketHandler.TypeSize + PacketHandler.TargetIDSize]);
                DataBuffer = Encoding.UTF8.GetString(ByteBuffer,
                    PacketHandler.TypeSize + PacketHandler.TargetIDSize + PacketHandler.TerminatorSize,
                    PacketHandler.DataSize);
                Polymono.Debug("Conversion to objects finished.");
                Polymono.Debug($"Type: {Type}");
                Polymono.Debug($"Target ID: {TargetID}");
                Polymono.Debug($"Terminate: {Terminate}");
                Polymono.Debug($"DataBuffer: {DataBuffer}");
                Polymono.Debug("-------------------------------END DECODE---------------------------------");
            } else
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
            return sb.ToString().Trim();
        }
    }

    enum PacketType {
        Null,
        Connect,
        Disconnect,
        Message,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class PacketHandler
    {
        // Byte buffer size.
        public const int BufferSize = 1024;
        // Data buffer sizes.
        public const int TypeSize = 4;
        public const int TargetIDSize = 4;
        public const int TerminatorSize = 1;
        public const int DataSize = BufferSize - TypeSize - TargetIDSize - TerminatorSize;

        public static Packet[] Create(PacketType type, int targetID, string data)
        {
            Polymono.Debug($"Data length: {data.Length}");
            int PacketsToCreate = (data.Length / DataSize) + 1;
            Polymono.Debug($"Number of packets: {PacketsToCreate}");
            Packet[] packets = new Packet[PacketsToCreate];
            for (int i = 0; i < PacketsToCreate; i++)
            {
                Polymono.Debug($"Interation: {i}");
                int size;
                bool terminate;
                if (data.Length - (i * DataSize) < DataSize)
                {
                    // Last Packet to populate.
                    size = data.Length - (i * DataSize);
                    terminate = true;
                }
                else
                {
                    // There are more packets to populate.
                    size = DataSize;
                    terminate = false;
                }
                packets[i] = new Packet(type, targetID, terminate, data.Substring(i * DataSize, size));
                packets[i].Encode();
            }
            return packets;
        }

        public static Packet[] Create(PacketType type, string data)
        {
            return Create(type, int.MaxValue, data);
        }
    }
}

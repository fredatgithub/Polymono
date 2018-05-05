using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking.Test
{
    class PacketData
    {
        public string Data;
        public byte[] Bytes;
        public PacketType Type;

        public PacketData()
        {
            Data = "";
            Bytes = new byte[0];
            Type = PacketType.Null;
        }

        public void AppendBytes(byte[] bytes)
        {
            byte[] temp = new byte[Bytes.Length + bytes.Length];
            for (int i = 0; i < Bytes.Length; i++)
            {
                temp[i] = Bytes[i];
            }
            for (int i = 0; i < bytes.Length; i++)
            {
                temp[i + Bytes.Length] = bytes[i];
            }
            Bytes = temp;
        }
    }
}

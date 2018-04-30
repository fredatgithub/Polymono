using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Protocol
    {
        /// <summary>
        /// Name:[~ Bytes]
        /// </summary>
        public static string EncodeConnectionRequest(string name)
        {
            return name;
        }

        /// <summary>
        /// Name:[~ Bytes]
        /// </summary>
        public static string DecodeConnectionRequest(string name)
        {
            return name;
        }

        /// <summary>
        /// Success:[1 byte] ID:[4 bytes]
        /// </summary>
        public static string EncodeConnectionResponse(bool success, int id)
        {
            return Convert.ToByte(success) + Convert.ToString(id);
        }

        /// <summary>
        /// Success:[1 byte]
        /// </summary>
        public static bool DecodeConnectionResponseSuccess(string data)
        {
            return Convert.ToBoolean(data.Substring(0, sizeof(byte)));
        }

        /// <summary>
        /// ID:[4 bytes]
        /// </summary>
        public static int DecodeConnectionResponseID(string data)
        {
            return Convert.ToInt32(data.Substring(sizeof(byte)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Networking
{
    class Protocol
    {
        /*
         * -------State changes-------
         *   = Connection Protocol =
         * Connection Request.      [name]
         * Connection Response.     [success, playerid]
         * Handled Disconnection.   [playerid, reason]
         * Unhandled Disconnection. [playerid]
         * 
         *      = Game Protocol =
         * Dice Roll                [dice1, dice2, senderid]
         * Move Done                [senderid]
         * Trade Request            [senderid, playerid]
         * Trade Response           [accept/deny, senderid]
         * Trade Update             [propertyid, senderid]
         * Trade Confirm            [accept/deny]
         * Purchase Property        [propertyid, senderid]
         * Build Property           [propertyid, senderid]
         * End Turn                 [senderid]
         * 
         *     = Chat Protocol =
         * Send Message             [message]
         */

        public class Connection
        {
            /// <summary>
            /// Name:[~ Bytes]
            /// </summary>
            public static byte[] EncodeRequest(string name)
            {
                return Encoding.UTF8.GetBytes(name);
            }
            
            public static string DecodeRequest(byte[] data)
            {
                return GetString(data);
            }

            /// <summary>
            /// Success:[1 byte] ID:[4 bytes]
            /// </summary>
            public static byte[] EncodeResponse(bool success, int id)
            {
                byte[] successBytes = BitConverter.GetBytes(success);
                byte[] idBytes = BitConverter.GetBytes(id);
                byte[] allBytes = new byte[sizeof(bool) + sizeof(int)];
                Buffer.BlockCopy(successBytes, 0, allBytes, 
                    0, 
                    successBytes.Length);
                Buffer.BlockCopy(idBytes, 0, allBytes,
                    successBytes.Length, 
                    idBytes.Length);
                return allBytes;
            }
            
            public static bool DecodeResponseSuccess(byte[] data)
            {
                return BitConverter.ToBoolean(data, 0);
            }
            
            public static int DecodeResponseID(byte[] data)
            {
                return BitConverter.ToInt32(data, sizeof(bool));
            }

            /// <summary>
            /// ID:[4 bytes]
            /// </summary>
            public static byte[] EncodeClient(int id, string name)
            {
                byte[] idBytes = BitConverter.GetBytes(id);
                if (name.Length > 32)
                    name.Substring(0, 32);
                byte[] nameBytes = Encoding.UTF8.GetBytes(name);
                byte[] allBytes = new byte[idBytes.Length + 32];
                Buffer.BlockCopy(idBytes, 0, allBytes,
                    0,
                    idBytes.Length);
                Buffer.BlockCopy(nameBytes, 0, allBytes,
                    idBytes.Length,
                    nameBytes.Length);
                return allBytes;
            }

            public static int DecodeClientID(byte[] data)
            {
                return BitConverter.ToInt32(data, 0);
            }

            public static string DecodeClientName(byte[] data)
            {
                return GetString(data, sizeof(int), 32);
            }

            /// <summary>
            /// ID:[4 bytes]
            /// </summary>
            public static byte[] EncodeDisconnect(int id, string message)
            {
                byte[] idBytes = BitConverter.GetBytes(id);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] allBytes = new byte[sizeof(int) + messageBytes.Length];
                Buffer.BlockCopy(idBytes, 0, allBytes, 
                    0, 
                    idBytes.Length);
                Buffer.BlockCopy(messageBytes, 0, allBytes,
                    idBytes.Length, 
                    messageBytes.Length);
                return allBytes;
            }

            public static int DecodeDisconnectID(byte[] data)
            {
                return BitConverter.ToInt32(data, 0);
            }

            public static string DecodeDisconnectMessage(byte[] data)
            {
                return GetString(data, sizeof(int), data.Length - sizeof(int));
            }
        }

        public class Game
        {
            // Dice roll
            public static byte[] EncodeDiceRoll(int diceOne, int diceTwo, int senderID)
            {
                byte[] diceOneBytes = BitConverter.GetBytes(diceOne);
                byte[] diceTwoBytes = BitConverter.GetBytes(diceTwo);
                byte[] senderIDBytes = BitConverter.GetBytes(senderID);
                byte[] allBytes = new byte[sizeof(int) * 3];
                Buffer.BlockCopy(diceOneBytes, 0, allBytes, 
                    0, 
                    diceOneBytes.Length);
                Buffer.BlockCopy(diceTwoBytes, 0, allBytes, 
                    diceOneBytes.Length, 
                    diceTwoBytes.Length);
                Buffer.BlockCopy(senderIDBytes, 0, allBytes, 
                    diceOneBytes.Length + diceTwoBytes.Length, 
                    senderIDBytes.Length);
                return allBytes;
            }

            public static int DecodeDiceRollOne(byte[] data)
            {
                return BitConverter.ToInt32(data, 0);
            }

            public static int DecodeDiceRollTwo(byte[] data)
            {
                return BitConverter.ToInt32(data, sizeof(int));
            }

            public static int DecodeDiceRollSenderID(byte[] data)
            {
                return BitConverter.ToInt32(data, sizeof(int) * 2);
            }

            // Move state
            public static byte[] EncodeMoveDone(int senderID)
            {
                return BitConverter.GetBytes(senderID);
            }

            public static int DecodeMoveDone(byte[] data)
            {
                return BitConverter.ToInt32(data, 0);
            }

            // Trade Request
            // Trade Response
            // Trade Update
            // Trade Confirm
            // Purchase Property
            // Build Property

            // End Turn
            public static byte[] EncodeEndTurn(int senderID)
            {
                return BitConverter.GetBytes(senderID);
            }

            public static int DecodeEndTurnSenderID(byte[] data)
            {
                return BitConverter.ToInt32(data, 0);
            }

            // Start game
            public static byte[] EncodeStartGame(int senderID)
            {
                return BitConverter.GetBytes(senderID);
            }

            public static int DecodeStartGameSenderID(byte[] data)
            {
                return BitConverter.ToInt32(data, 0);
            }
        }

        public class Chat
        {
            public static byte[] EncodeMessage(string message)
            {
                return Encoding.UTF8.GetBytes(message);
            }

            public static string DecodeMessage(byte[] data)
            {
                return Encoding.UTF8.GetString(data);
            }
        }

        protected static string GetString(byte[] data)
        {
            return GetString(data, 0, data.Length);
        }

        protected static string GetString(byte[] data, int index, int count)
        {
            return Encoding.UTF8.GetString(data, index, count).Replace("\0", "");
        }
    }
}

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

            /// <summary>
            /// ID:[4 bytes]
            /// </summary>
            public static string EncodeDisconnect(int id)
            {
                return Convert.ToString(id);
            }

            /// <summary>
            /// ID:[4 bytes]
            /// </summary>
            public static int DecodeDisconnect(string data)
            {
                return Convert.ToInt32(data.Substring(0, sizeof(int)));
            }
        }

        public class Game
        {
            // Dice roll
            public static string EncodeDiceRoll(int diceOne, int diceTwo, int senderID)
            {
                return diceOne.ToString() + diceTwo.ToString() + senderID.ToString();
            }

            public static int DecodeDiceRollOne(string data)
            {
                return Convert.ToInt32(data.Substring(sizeof(int)));
            }

            public static int DecodeDiceRollTwo(string data)
            {
                return Convert.ToInt32(data.Substring(sizeof(int), sizeof(int)));
            }

            public static int DecodeDiceRollSenderID(string data)
            {
                return Convert.ToInt32(data.Substring(sizeof(int) * 2, sizeof(int)));
            }

            // Move state
            public static string EncodeMoveDone(int senderID)
            {
                return senderID.ToString();
            }

            public static int DecodeMoveDone(string data)
            {
                return Convert.ToInt32(data.Substring(sizeof(int)));
            }

            // Trade Request
            // Trade Response
            // Trade Update
            // Trade Confirm
            // Purchase Property
            // Build Property

            // End Turn
            public static string EncodeEndTurn(int senderID)
            {
                return senderID.ToString();
            }

            public static int DecodeEndTurnSenderID(string data)
            {
                return Convert.ToInt32(data.Substring(sizeof(int)));
            }
        }

        public class Chat
        {
            public static string EncodeMessage(string message)
            {
                return message;
            }

            public static string DecodeMessage(string data)
            {
                return data;
            }
        }
    }
}

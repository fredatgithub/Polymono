using Polymono.Classes.Graphics;
using Polymono.Classes.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Polymono.Classes {
    public enum ConsoleLevel {
        Debug, Normal, Warning, Error
    }

    class Polymono {
        public const int MaxPlayers = 16;

        public static Dictionary<ConsoleLevel, bool> ConsoleLevels;
        //public static INetwork Network;

        public static void Main()
        {
            // -------------------------- SETUP DATA --------------------------
            Thread.CurrentThread.Name = "MainThread";
            ConsoleLevels = new Dictionary<ConsoleLevel, bool>() {
                { ConsoleLevel.Normal, true },
                { ConsoleLevel.Error, true },
                { ConsoleLevel.Warning, true },
                { ConsoleLevel.Debug, false } };
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // -------------------------- DEBUG TEXT --------------------------
            PrintF($"Polymono Version increment: {version}");
            PrintF("Enable debugging text? ", false);
            string input = Console.ReadLine().ToUpperInvariant();
            if (input == "YES")
            {
                ConsoleLevels[ConsoleLevel.Debug] = true;
                PrintF("Debugging is enabled.");
            } else
            {
                PrintF("Debugging is disabled.");
            }
            // ----------------------- SERVER OR CLIENT -----------------------
            /*PrintF("Server or Client: ", false);
            input = Console.ReadLine().ToUpperInvariant();
            if (input == "SERVER")
            {
                PrintF("Address: ", false);
                input = Console.ReadLine();
                Server server = new Server();
                server.Start(2222);
                Network = server;
            } else if (input == "CLIENT")
            {
                PrintF("Address: ", false);
                input = Console.ReadLine();
                Client client = new Client();
                client.Start(input, 2222);
                Network = client;
            }*/
            // ------------------------ SEND MESSAGES ------------------------
            /*PrintF("Begin sending packets...");
            Console.ReadLine();
            if (Network != null)
            {
                string message = "";
                for (int i = 0; i < 1000; i++)
                {
                    message += "Hello world. ";
                }
                Packet[] packets = PacketHandler.Create(PacketType.Message, message);
                foreach (var packet in packets)
                {
                    packet.Encode();
                }
                Network.Send(packets);
            }*/
            // ---------------------- START APPLICATION ----------------------
            Print("Type desired tick rate: ", false);
            input = Console.ReadLine();
            int tickRate = Convert.ToInt32(input);
            if (tickRate < 30)
            {
                tickRate = 30;
            } else if (tickRate > 120)
            {
                tickRate = 120;
            }
            Print($"Using {tickRate} tick rate.");
            using (GameClient game = new GameClient())
            {
                game.Run(60.0d, tickRate);
            }
        }

        #region Logging
        /// <summary>
        /// Prints given text to the console, dependant on logging levels, with prefixed thread information.
        /// </summary>
        /// <param name="level">The level of relevance the text is to print.</param>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void Print(ConsoleLevel level = ConsoleLevel.Normal, string text = "", bool newline = true)
        {
            if (ConsoleLevels[level])
            {
                string interpolatedString = $"{Thread.CurrentThread.Name ?? "BackgroundThread"}[{Thread.CurrentThread.ManagedThreadId}]";
                // Format spacings to align in console.
                if (interpolatedString.Length < 15)
                {
                    interpolatedString += $" \t\t| {text}";
                } else
                {
                    interpolatedString += $" \t| {text}";
                }
                if (newline)
                {
                    Console.WriteLine(interpolatedString);
                } else
                {
                    Console.Write(interpolatedString);
                }
            }
        }

        /// <summary>
        /// Prints given text to the console, using a normal logging level, with prefixed thread information.
        /// </summary>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void Print(string text, bool newline = true)
        {
            Print(ConsoleLevel.Normal, text, newline);
        }

        /// <summary>
        /// Prints given text to the console, using an error logging level, with prefixed thread information.
        /// </summary>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void Error(string text, bool newline = true)
        {
            Print(ConsoleLevel.Error, text, newline);
        }

        /// <summary>
        /// Prints given text to the console, using a debug logging level, with prefixed thread information.
        /// </summary>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void Debug(string text, bool newline = true)
        {
            Print(ConsoleLevel.Debug, text, newline);
        }

        /// <summary>
        /// Prints given text to the console, dependant on logging levels, without prefixed thread information.
        /// </summary>
        /// <param name="level">The level of relevance the text is to print.</param>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void PrintF(ConsoleLevel level = ConsoleLevel.Normal, string text = "", bool newline = true)
        {
            if (ConsoleLevels[level])
            {
                if (newline)
                {
                    Console.WriteLine(text);
                } else
                {
                    Console.Write(text);
                }
            }
        }

        /// <summary>
        /// Prints given text to the console, using a normal logging level, without prefixed thread information.
        /// </summary>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void PrintF(string text, bool newline = true)
        {
            PrintF(ConsoleLevel.Normal, text, newline);
        }

        /// <summary>
        /// Prints given text to the console, using an error logging level, without prefixed thread information.
        /// </summary>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void ErrorF(string text, bool newline = true)
        {
            PrintF(ConsoleLevel.Error, text, newline);
        }

        /// <summary>
        /// Prints given text to the console, using a debug logging level, without prefixed thread information.
        /// </summary>
        /// <param name="text">The text to print to console.</param>
        /// <param name="newline">Whether to append a newline.</param>
        public static void DebugF(string text, bool newline = true)
        {
            PrintF(ConsoleLevel.Debug, text, newline);
        }
        #endregion
    }
}

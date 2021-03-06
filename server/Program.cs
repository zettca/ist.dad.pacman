﻿using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace server
{
    class Program
    {
        public static Uri endpoint;
        public static int msec;
        public static int numPlayers;
        public static string gameName = "pacman";

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    Pacman Server                   ║");
            Console.WriteLine("║             DAD 2017-2018, IST - Group 4           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");

            Console.WriteLine("{0} arguments.", args.Length);
            foreach (string arg in args)
            {
                Console.WriteLine("arg: {0}", arg);
            }

            try
            {
                endpoint = new Uri(args.Length > 0 ? args[0] : "tcp://localhost:8086/OGPGameServer");
                msec = (args.Length > 1) ? Int32.Parse(args[1]) : 20;
                numPlayers = (args.Length > 2) ? Int32.Parse(args[2]) : 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Expected arguments: <url> <msec> <numPlayers>");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            Console.WriteLine("URI:\t{0}", endpoint);
            Console.WriteLine("MSEC:\t{0}", msec);
            Console.WriteLine("Pla#:\t{0}", numPlayers);
            Console.WriteLine("Path:\t{0}", endpoint.AbsolutePath);
            Console.WriteLine("Requested Host:\t{0}", endpoint.Host);

            TcpChannel channel = new TcpChannel(endpoint.Port);
            ChannelServices.RegisterChannel(channel, false);

            string objName = endpoint.AbsolutePath.Replace("/", "");
            Console.WriteLine("objName:\t{0}", objName);
            Console.WriteLine();
            // get channel host
            ChannelDataStore data = (ChannelDataStore)channel.ChannelData;
            foreach (string uriStr in data.ChannelUris)
            {
                Console.WriteLine("Server at:\t{0}/{1}", uriStr, objName);
            }
            Console.WriteLine();

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerGameService),
                objName,
                WellKnownObjectMode.Singleton);

            Console.ReadKey();
        }
    }

}

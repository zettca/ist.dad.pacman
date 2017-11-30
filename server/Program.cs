using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace server
{
    class Program
    {
        public static int port, numPlayers;
        public static string gameName;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    Pacman Server                   ║");
            Console.WriteLine("║             DAD 2017-2018, IST - Group 4           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");

            try
            {
                port = Int32.Parse(args[0]);
                numPlayers = Int32.Parse(args[1]);
                gameName = args[2];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Expected arguments: <port> <numPlayers> <gameName>");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerGameService),
                "OGPGameServer",
                WellKnownObjectMode.Singleton);

            Console.ReadKey();
        }
    }

}

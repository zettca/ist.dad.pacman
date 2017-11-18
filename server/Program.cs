using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

using services;

namespace server
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    Pacman Server                   ║");
            Console.WriteLine("║             DAD 2017-2018, IST - Group 4           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");

            TcpChannel channel = new TcpChannel(8086);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerGameService),
                "OGPGameServer",
                WellKnownObjectMode.Singleton);

            Console.ReadLine();
        }
    }

    class ServerGameService : MarshalByRefObject, IGameServer
    {
        List<IGameClient> clients;
        List<string> messages;

        ServerGameService()
        {
            clients = new List<IGameClient>();
            messages = new List<string>();
        }

        public void RegisterPlayer(string port)
        {
            string endpoint = "tcp://localhost:" + port + "/GameClient";
            IGameClient client = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);
            clients.Add(client);

            Console.WriteLine("New client bound to " + endpoint);

            // return gameState ?
        }
    }
}

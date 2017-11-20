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


    // TODO: implement something better, or send Keys object through network ?
    public struct PlayerKeyCommand
    {
        public int keyValue;
        public bool isKeyDown;
        public PlayerKeyCommand(int keyVal, bool isDown)
        {
            keyValue = keyVal;
            isKeyDown = isDown;
        }
    }

    class ServerGameService : MarshalByRefObject, IGameServer
    {
        List<IGameClient> clients;
        List<string> messages;
        List<PlayerKeyCommand> playerInputQueue;

        ServerGameService()
        {
            clients = new List<IGameClient>();
            messages = new List<string>();
            playerInputQueue = new List<PlayerKeyCommand>();
        }

        public void RegisterPlayer(string port)
        {
            string endpoint = "tcp://localhost:" + port + "/GameClient";
            IGameClient client = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);
            clients.Add(client);

            Console.WriteLine("New client bound to " + endpoint);

            // TODO: return init gameState ?
        }

        public void SendKey(int keyValue, bool isKeyDown)
        {
            // TODO: find who the player is
            playerInputQueue.Add(new PlayerKeyCommand(keyValue, isKeyDown));
            Console.WriteLine("INPUT RECEIVED: " + keyValue.ToString() + " " + isKeyDown.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

using services;
using System.Threading;
using System.Runtime.Remoting.Channels;

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
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerGameService),
                "OGPGameServer",
                WellKnownObjectMode.Singleton);

            Console.ReadLine();
        }
    }

    // TODO: implement something better, or send Keys object through network ?
    public struct PlayerAction
    {
        public string playerId;
        public int keyValue;
        public bool isKeyDown;
        public PlayerAction(string pid, int keyVal, bool isDown)
        {
            playerId = pid;
            keyValue = keyVal;
            isKeyDown = isDown;
        }
    }

    class ServerGameService : MarshalByRefObject, IGameServer
    {
        List<IGameClient> clients;
        List<string> messages;
        List<PlayerAction> playerInputQueue;

        // TODO: get configs from stdin?
        int maxPlayers = 5, msPerRound = 1000;
        string gameName = "pacman";

        ServerGameService()
        {
            this.clients = new List<IGameClient>();
            this.messages = new List<string>();
            this.playerInputQueue = new List<PlayerAction>();

            if (this.gameName == "pacman".ToLower())
            {
                //this.stateMachine = new PacmanStateMachine(); // TODO: some initial state
            }
        }

        public bool RegisterPlayer(int port)
        {
            // TODO: properly refuse connection with exceptions? bool is fine tho
            if (this.clients.Count == this.maxPlayers) return false;

            string endpoint = "tcp://localhost:" + port.ToString() + "/GameClient";
            IGameClient client = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);
            this.clients.Add(client);

            //client.SendGameState(null); // TODO: implement actual Game State

            Console.WriteLine("New client bound to " + endpoint);

            if (this.clients.Count == this.maxPlayers)
            {
                ThreadStart ts = new ThreadStart(this.GameInstanceThread);
                Thread thread = new Thread(ts);
                thread.Start();
            }

            return true;
        }

        private void GameInstanceThread()
        {
            while (true)
            {
                Console.WriteLine("Waited " + this.msPerRound.ToString());
                Thread.Sleep(this.msPerRound);
            }
        }

        public void SendKey(int keyValue, bool isKeyDown)
        {
            // TODO: find who the player is
            string playerId = "player12";
            playerInputQueue.Add(new PlayerAction(playerId, keyValue, isKeyDown));
            Console.WriteLine("INPUT RECEIVED: " + keyValue.ToString() + " " + isKeyDown.ToString());
        }

        public void SendMessage(string msg)
        {
            if (msg.Trim().Length > 0)
            {
                Console.WriteLine("Message received: " + msg);
                this.messages.Add(msg);

                Thread thread = new Thread(() => BroadcastMessage(this.clients, msg));
                thread.Start();
            }
        }

        public List<string> GetMessageHistory()
        {
            return this.messages;
        }

        private void BroadcastMessage(List<IGameClient> clients, string msg)
        {
            foreach (IGameClient client in clients)
            {
                client.SendMessage(msg);
            }
        }
    }
}

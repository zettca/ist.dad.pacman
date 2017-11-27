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

    class ServerGameService : MarshalByRefObject, IGameServer
    {
        List<IGameClient> clients;
        List<string> messages;
        List<PlayerAction> playerInputQueue;
        StateMachine gameInstance;

        // TODO: get configs from stdin?
        int maxPlayers = 3, msPerRound = 1000;
        string gameName = "pacman";

        ServerGameService()
        {
            this.clients = new List<IGameClient>();
            this.messages = new List<string>();
            this.playerInputQueue = new List<PlayerAction>();
        }

        private void StartGame(string gameId)
        {
            IGameState initialState;
            switch (gameId)
            {
                case "pacman":
                    initialState = new PacmanGameState(3, 5, 5, 300, 300);
                    break;
                default:
                    initialState = null;
                    Console.WriteLine("Unknown game...");
                    break;
            }
            gameInstance = new StateMachine(initialState);

            ThreadStart ts = new ThreadStart(GameInstanceThread);
            Thread thread = new Thread(ts);
            thread.Start();
        }

        public bool RegisterPlayer(int port)
        {
            // TODO: properly refuse connection with exceptions? bool is fine tho
            if (this.clients.Count > this.maxPlayers) return false;

            string endpoint = "tcp://localhost:" + port.ToString() + "/GameClient";
            IGameClient client = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);
            this.clients.Add(client);

            //client.SendGameState(null); // TODO: implement actual Game State

            Console.WriteLine("New client bound to " + endpoint);

            if (this.clients.Count == this.maxPlayers)
            {
                StartGame(this.gameName.ToLower());
            }

            return true;
        }

        private void GameInstanceThread()
        {
            while (true)
            {
                Console.WriteLine("Waited " + this.msPerRound.ToString());
                gameInstance.ApplyTransitions(playerInputQueue);

                // TODO: send gameState to players

                ThreadStart ts = new ThreadStart(SendGameState);
                Thread thread = new Thread(ts);
                thread.Start();


                Thread.Sleep(this.msPerRound);
            }
        }

        private void SendGameState()
        {
            foreach (IGameClient client in clients)
            {
                client.SendGameState(this.gameInstance.CurrentState);
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

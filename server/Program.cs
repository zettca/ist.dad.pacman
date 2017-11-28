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
                Console.WriteLine("Malformed arguments. Press any key to exit");
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

    class ServerGameService : MarshalByRefObject, IGameServer
    {
        List<IGameClient> clients;
        List<string> messages;
        List<PlayerAction> playerInputQueue;
        StateMachine gameInstance;

        ServerGameService()
        {
            clients = new List<IGameClient>();
            messages = new List<string>();
            playerInputQueue = new List<PlayerAction>();
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
            if (clients.Count > Program.numPlayers) return false;

            string endpoint = "tcp://localhost:" + port.ToString() + "/GameClient";
            IGameClient client = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);
            clients.Add(client);

            //client.SendGameState(null); // TODO: implement actual Game State

            Console.WriteLine("New client bound to " + endpoint);

            if (clients.Count == Program.numPlayers)
            {
                StartGame(Program.gameName);
            }

            return true;
        }

        private void GameInstanceThread()
        {
            const int MS_PER_ROUND = 1000;
            while (true)
            {
                Console.WriteLine("Waited " + MS_PER_ROUND.ToString());
                gameInstance.ApplyTransitions(playerInputQueue);

                // TODO: send gameState to players

                ThreadStart ts = new ThreadStart(SendGameState);
                Thread thread = new Thread(ts);
                thread.Start();


                Thread.Sleep(MS_PER_ROUND);
            }
        }

        private void SendGameState()
        {
            IGameState gameState = gameInstance.CurrentState;
            foreach (IGameClient client in clients)
            {
                // TODO: properly serialize GameState
                //client.SendGameState(gameState);
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
                messages.Add(msg);

                Thread thread = new Thread(() => BroadcastMessage(clients, msg));
                thread.Start();
            }
        }

        public List<string> GetMessageHistory()
        {
            return messages;
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

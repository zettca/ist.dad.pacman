using services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace server
{
    class ServerGameService : MarshalByRefObject, IGameServer
    {
        Dictionary<string, IGameClient> clients;
        List<string> messages;
        List<PlayerAction> playerInputQueue;
        StateMachine gameInstance;

        ICollection<IGameClient> ClientConnections { get => clients.Values; }

        ServerGameService()
        {
            clients = new Dictionary<string, IGameClient>();
            messages = new List<string>();
            playerInputQueue = new List<PlayerAction>();
        }

        private void StartGame(string gameId)
        {
            IGameState initialState;
            switch (gameId)
            {
                case "pacman":
                    Console.WriteLine(clients.Count.ToString());
                    Console.WriteLine(Program.numPlayers);
                    initialState = new PacmanGameState(clients.Keys.ToList(), Program.numPlayers, 5, 5, 300, 300);
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

        public bool RegisterPlayer(int port, string username)
        {
            if (clients.Count >= Program.numPlayers || clients.ContainsKey(username)) return false;

            string endpoint = "tcp://localhost:" + port.ToString() + "/GameClient";
            IGameClient clientConnection = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);

            clients.Add(username, clientConnection);

            //client.SendGameState(null);

            Console.WriteLine("New client connected: " + endpoint);

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
                gameInstance.ApplyTransitions(playerInputQueue);

                ThreadStart ts = new ThreadStart(SendGameState);
                Thread thread = new Thread(ts);
                thread.Start();

                Thread.Sleep(MS_PER_ROUND);
            }
        }

        private void SendGameState()
        {
            IGameState gameState = gameInstance.CurrentState;
            foreach (IGameClient client in clients.Values)
            {
                // TODO: properly serialize GameState
                client.SendGameState(gameState);
                Console.WriteLine("Sent GameState:");
                Console.WriteLine(gameState.ToString());
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

                Thread thread = new Thread(() => BroadcastMessage(clients.Values, msg));
                thread.Start();
            }
        }

        public List<string> GetMessageHistory()
        {
            return messages;
        }

        private void BroadcastMessage(ICollection<IGameClient> clients, string msg)
        {
            foreach (IGameClient client in clients)
            {
                client.SendMessage(msg);
            }
        }
    }
}

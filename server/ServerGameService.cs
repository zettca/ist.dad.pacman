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
        Dictionary<Guid, string> clientNames;
        List<Message> messages;
        List<PlayerAction> playerActions;
        StateMachine gameInstance;

        ICollection<IGameClient> ClientConnections { get => clients.Values; }

        ServerGameService()
        {
            clients = new Dictionary<string, IGameClient>();
            clientNames = new Dictionary<Guid, string>();
            messages = new List<Message>();
            playerActions = new List<PlayerAction>();
        }

        private IGameState GetInitialGameState(string gameId)
        {
            switch (gameId)
            {
                case "pacman":
                    return new PacmanGameState(clientNames.Keys.ToList(), Program.numPlayers, 5, 5, 300, 300);
                default:
                    return null;
            }
        }

        private void StartGame(string gameId)
        {
            gameInstance = new StateMachine(GetInitialGameState(gameId));

            ThreadStart ts = new ThreadStart(GameInstanceThread);
            Thread thread = new Thread(ts);
            thread.Start();
        }

        public Guid RegisterPlayer(int port, string username)
        {
            if (clients.Count >= Program.numPlayers || clients.ContainsKey(username))
                return Guid.Empty;

            string endpoint = "tcp://localhost:" + port.ToString() + "/GameClient";
            IGameClient clientConnection = (IGameClient)Activator.GetObject(typeof(IGameClient), endpoint);
            Guid clientGuid = Guid.NewGuid();

            clients.Add(username, clientConnection);
            clientNames.Add(clientGuid, username);

            Console.WriteLine("New client \"(" + username + ")\" connected at " + endpoint);

            if (clients.Count == Program.numPlayers)
            {
                StartGame(Program.gameName);
            }

            return clientGuid;
        }

        private void GameInstanceThread()
        {
            const int MS_PER_ROUND = 100;
            while (true)
            {
                gameInstance.ApplyTransitions(playerActions);
                playerActions.Clear();
                gameInstance.ApplyTick();

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
                try
                {
                    client.SendGameState(gameState);
                    // TODO: remove client from clients
                }
                catch (Exception)
                {
                    Console.WriteLine("Client disconnected!");
                }
            }

            Console.WriteLine("GameState: " + gameState.ToString());
        }

        public void SendKey(Guid pid, int keyValue, bool isKeyDown)
        {
            string playerName = clientNames[pid];
            playerActions.Add(new PlayerAction(pid, keyValue, isKeyDown));
            Console.WriteLine(String.Format("INPUT from {0}: {1} {2}", playerName, keyValue, isKeyDown));
        }

        public void SendMessage(Guid pid, string msg)
        {
            if (msg.Trim().Length > 0)
            {
                Message message = new Message(clientNames[pid], msg);
                Console.WriteLine(message.ToString());
                messages.Add(message);

                Thread thread = new Thread(() => BroadcastMessage(clients.Values, message));
                thread.Start();
            }
        }

        public List<Message> GetMessageHistory()
        {
            return messages;
        }

        private void BroadcastMessage(ICollection<IGameClient> clients, Message msg)
        {
            foreach (IGameClient client in clients)
            {
                client.SendMessage(msg);
            }
        }
    }
}

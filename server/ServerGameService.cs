using services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace server
{
    struct ServiceClient
    {
        public Uri Uri;
        public Guid UID;
        public string Name;
        public IGameClient Conn;

        public ServiceClient(Uri endpoint, Guid uid, string username, IGameClient conn)
        {
            Uri = endpoint;
            UID = uid;
            Name = username;
            Conn = conn;
        }
    }

    class ServerGameService : MarshalByRefObject, IGameServer
    {
        List<ServiceClient> clients;
        List<Message> messages;
        List<PlayerAction> playerActions;
        StateMachine gameInstance;

        List<Uri> ClientUris { get => clients.Select((cli) => cli.Uri).ToList(); }
        List<Guid> ClientIds { get => clients.Select((cli) => cli.UID).ToList(); }
        List<string> ClientNames { get => clients.Select((cli) => cli.Name).ToList(); }
        List<IGameClient> ClientConns { get => clients.Select((cli) => cli.Conn).ToList(); }

        ServerGameService()
        {
            clients = new List<ServiceClient>();
            messages = new List<Message>();
            playerActions = new List<PlayerAction>();
        }

        private IGameState GetInitialGameState(string gameId)
        {
            switch (gameId)
            {
                case "pacman":
                    return new PacmanGameState(ClientIds, Program.numPlayers, 300, 300);
                default:
                    return null;
            }
        }

        private void StartGame(string gameId)
        {
            gameInstance = new StateMachine(GetInitialGameState(gameId));
            new Thread(() => GameInstanceThread()).Start();
        }

        public Guid RegisterPlayer(Uri endpoint, string username)
        {
            Console.WriteLine("Trying to register new player at " + endpoint);
            if (clients.Count >= Program.numPlayers || clients.Exists((cli) => cli.Name == username))
                return Guid.Empty;

            IGameClient clientConnection = (IGameClient)Activator.GetObject(
                typeof(IGameClient), endpoint.AbsoluteUri);

            Console.WriteLine("AbsoluteUri: " + endpoint.AbsoluteUri);

            if (clientConnection != null)
                Console.WriteLine("\tGot remote object.");
            else
                Console.WriteLine("\tFailed to get remote object.");

            foreach (var peer in clients)
            {
                // register new peer on existing clients
                Console.Write("\tRegister " + endpoint.AbsoluteUri);
                Console.WriteLine(" on " + peer.Conn.Uri);
                peer.Conn.RegisterNewClient(endpoint);

                // register existing clients on new peer
                Console.Write("\tRegister " + peer.Conn.Uri);
                Console.WriteLine(" on " + clientConnection.Uri);
                clientConnection.RegisterNewClient(peer.Conn.Uri);
            }

            Guid clientGuid = Guid.NewGuid();

            clients.Add(new ServiceClient(endpoint, clientGuid, username, clientConnection));

            Console.WriteLine("New client \"(" + username + ")\" connected at " + endpoint);
            Console.WriteLine(clientConnection.Uri);

            if (clients.Count == Program.numPlayers)
            {
                StartGame(Program.gameName);
            }

            return clientGuid;
        }

        private void GameInstanceThread()
        {
            new Thread(() => GameStart()).Start();

            while (!gameInstance.CurrentState.HasEnded)
            {
                List<PlayerAction> actionsToProcess = new List<PlayerAction>(playerActions);
                playerActions.Clear();
                gameInstance.ApplyTransitions(actionsToProcess);
                gameInstance.ApplyTick();

                new Thread(() => SendGameState()).Start();
                Thread.Sleep(Program.msec);
            }

            Console.WriteLine("Game has ended!");
            new Thread(() => GameEnd()).Start();
        }

        private void GameStart()
        {
            foreach (var client in clients)
            {
                client.Conn.SendGameStart(gameInstance.CurrentState, ClientUris);
            }
        }

        private void GameEnd()
        {
            Dictionary<Guid, int> scores = new Dictionary<Guid, int>();
            PacmanGameState gameState = (PacmanGameState)gameInstance.CurrentState;

            foreach (var player in gameState.PlayerData)
            {
                scores.Add(player.Pid, player.Score);
            }

            foreach (var client in clients)
            {
                client.Conn.SendScoreboard(null);
            }
        }

        private void SendGameState()
        {
            foreach (var client in clients)
            {
                try
                {
                    client.Conn.SendGameState(gameInstance.CurrentState);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        public void SendKey(Guid pid, int keyValue, bool isKeyDown)
        {
            playerActions.Add(new PlayerAction(pid, keyValue, isKeyDown));
            Console.WriteLine("INPUT from {0}: {1} {2}", pid.ToString().Substring(0, 6), keyValue, isKeyDown);
        }
    }
}

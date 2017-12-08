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
        public string Name;
        public IGameClient Conn;

        public ServiceClient(Uri endpoint, string username, IGameClient conn)
        {
            Uri = endpoint;
            Name = username;
            Conn = conn;
        }
    }

    class ServerGameService : MarshalByRefObject, IGameServer, ISlaveControl
    {
        List<ServiceClient> clients;
        List<ChatMessage> messages;
        List<PlayerAction> playerActions;
        StateMachine gameInstance;

        internal List<List<string>> gameDataByRound;

        List<Uri> ClientUris { get => clients.Select((cli) => cli.Uri).ToList(); }
        List<string> ClientNames { get => clients.Select((cli) => cli.Name).ToList(); }
        List<IGameClient> ClientConns { get => clients.Select((cli) => cli.Conn).ToList(); }

        ServerGameService()
        {
            clients = new List<ServiceClient>();
            messages = new List<ChatMessage>();
            playerActions = new List<PlayerAction>();

            this.gameDataByRound = new List<List<string>>();
        }

        private IGameState GetInitialGameState(string gameId)
        {
            switch (gameId)
            {
                case "pacman":
                    return new PacmanGameState(ClientNames, Program.numPlayers, 348, 305);
                default:
                    return null;
            }
        }

        public void updateGameDataByRound(PacmanGameData gameData)
        {
            List<string> result = new List<string>();

            gameData.PlayerData.ForEach((player) => result.Add(player.ToString()));
            gameData.GhostData.ForEach((ghost) => result.Add(ghost.ToString()));
            gameData.WallData.ForEach((wall) => result.Add(wall.ToString()));
            gameData.FoodData.ForEach((food) => result.Add(food.ToString()));

            gameDataByRound.Add(result);
        }

        private void StartGame(string gameId)
        {
            PacmanGameState gameState = GetInitialGameState(gameId) as PacmanGameState;
            gameInstance = new StateMachine(gameState);
            updateGameDataByRound(gameState.Data as PacmanGameData);
            new Thread(() => GameInstanceThread()).Start();
        }

        public bool RegisterPlayer(Uri endpoint, string userID)
        {
            Console.WriteLine("Trying to register new player at " + endpoint);
            if (clients.Count >= Program.numPlayers || clients.Exists((cli) => cli.Name == userID))
                return false;

            IGameClient clientConnection = (IGameClient)Activator.GetObject(
                typeof(IGameClient), endpoint.AbsoluteUri);

            Console.WriteLine("AbsoluteUri: " + endpoint.AbsoluteUri);

            if (clientConnection == null)
            {
                Console.WriteLine("\tFailed to get remote object.");
                return false;
            }

            clients.Add(new ServiceClient(endpoint, userID, clientConnection));

            Console.WriteLine("New client ({0}) connected at {1} | {2}",
                userID, endpoint, clientConnection.Uri);

            if (clients.Count == Program.numPlayers) StartGame(Program.gameName);

            return true;
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
                updateGameDataByRound(gameInstance.CurrentState.Data as PacmanGameData);

                new Thread(() => SendGameState()).Start();
                Thread.Sleep(Program.msec);
            }

            Console.WriteLine("Game has ended!");
            new Thread(() => GameEnd()).Start();
        }

        private void GameStart()
        {
            // TODO: handle exceptions
            clients.ForEach((cli) => cli.Conn.SendGameStart(gameInstance.CurrentState.Data, ClientUris));
        }
        private void GameEnd()
        {
            // TODO: handle exceptions
            clients.ForEach((cli) => cli.Conn.SendGameEnd(gameInstance.CurrentState.Data));
        }

        private void SendGameState()
        {
            // TODO: handle exceptions
            clients.ForEach((cli) => cli.Conn.SendGameState(gameInstance.CurrentState.Data));
        }

        public void SendKeys(string pid, bool[] keys)
        {
            playerActions.Add(new PlayerAction(pid, keys));
            Console.WriteLine("INPUT from {0}: {1}", pid, String.Join(" ", keys));
        }

        public void GlobalStatus()
        {
            throw new NotImplementedException();
        }

        public void InjectDelay()
        {
            throw new NotImplementedException();
        }

        public List<string> LocalState(int round)
        {
            return gameDataByRound[round];
        }
    }
}

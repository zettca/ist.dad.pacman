using services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace server
{
    public enum ClientState
    {
        ALIVE, DEAD
    }

    struct ServiceClient
    {
        public Uri Uri;
        public string Name;
        public IGameClient Conn;
        public ClientState State;


        public ServiceClient(Uri endpoint, string username, IGameClient conn)
        {
            Uri = endpoint;
            Name = username;
            Conn = conn;
            State = ClientState.ALIVE;
        }
    }

    class ServerStuffs
    {
        internal List<ServiceClient> clients;
        internal List<ChatMessage> messages;
        internal List<PlayerAction> playerActions;
        internal StateMachine gameInstance;

        public ServerStuffs()
        {
            clients = new List<ServiceClient>();
            messages = new List<ChatMessage>();
            playerActions = new List<PlayerAction>();
        }
    }

    class ServerGameService : MarshalByRefObject, IGameServer, ISlaveControl
    {
        private ServerStuffs stuffs;

        internal List<List<string>> gameDataByRound;

        List<ServiceClient> Clients { get => stuffs.clients; }
        List<PlayerAction> PlayerActions { get => stuffs.playerActions; }
        StateMachine GameInstance { get => stuffs.gameInstance; }
        List<Uri> ClientUris { get => stuffs.clients.Select((cli) => cli.Uri).ToList(); }
        List<string> ClientNames { get => stuffs.clients.Select((cli) => cli.Name).ToList(); }
        List<IGameClient> ClientConns { get => stuffs.clients.Select((cli) => cli.Conn).ToList(); }
        IGameData GameData { get => stuffs.gameInstance.CurrentState.Data; }

        ServerGameService()
        {
            stuffs = new ServerStuffs();
            gameDataByRound = new List<List<string>>();
            new Thread(() => PingLoop()).Start();
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
            stuffs.gameInstance = new StateMachine(GetInitialGameState(gameId));
            new Thread(() => GameInstanceThread()).Start();
        }

        private void PingLoop()
        {
            while (true)
            {
                PingAll();
                Thread.Sleep(1000);
            }
        }

        private void PingAll()
        {
            // pings all clients, removing them if exception is thrown
            List<int> deads = new List<int>();

            for (int i = 0; i < Clients.Count; i++)
            {
                try
                {
                    Clients[i].Conn.Ping();
                }
                catch
                {
                    deads.Add(i);
                }
            }
            if (deads.Count > 0)
            {
                foreach (var iDead in deads)
                {
                    Clients.RemoveAt(iDead);
                }
            }
        }

        public bool RegisterPlayer(Uri endpoint, string userID)
        {
            lock (this)
            {
                Console.WriteLine("Trying to register new player at " + endpoint);
                if (Clients.Count >= Program.numPlayers || Clients.Exists((cli) => cli.Name == userID))
                    return false;

                IGameClient clientConnection = (IGameClient)Activator.GetObject(
                    typeof(IGameClient), endpoint.AbsoluteUri);


                Console.WriteLine("AbsoluteUri: " + endpoint.AbsoluteUri);

                if (clientConnection == null)
                {
                    Console.WriteLine("\tFailed to get remote object.");
                    return false;
                }

                Clients.Add(new ServiceClient(endpoint, userID, clientConnection));

                Console.WriteLine("New client ({0}) connected at {1} | {2}",
                    userID, endpoint, clientConnection.Uri);

                PingAll();
                if (Clients.Count == Program.numPlayers) StartGame(Program.gameName);

                return true;
            }
        }

        private bool AnyClientAlive()
        {
            foreach (var client in Clients)
            {
                var player = GameInstance.CurrentState.GetPlayer(client.Name);
                if (player.Alive) return true;
            }
            Console.WriteLine("Clients still alive");
            return false;
        }

        private void GameInstanceThread()
        {
            updateGameDataByRound(GameData as PacmanGameData);

            SendGameStart();

            while (AnyClientAlive() && (!GameInstance.CurrentState.HasEnded))
            {
                List<PlayerAction> actionsToProcess = new List<PlayerAction>(PlayerActions);
                PlayerActions.Clear();
                GameInstance.ApplyTransitions(actionsToProcess);
                GameInstance.ApplyTick();
                updateGameDataByRound(GameData as PacmanGameData);

                new Thread(() => SendGameState(GameData.Copy())).Start();

                Thread.Sleep(Program.msec);
            }

            Console.WriteLine("Game has ended!");
            SendGameEnd();
        }

        private void GamesStuffs(Action<IGameClient> method)
        {
            lock (this)
            {
                for (int index = 0; index < Clients.Count; index++)
                {
                    try
                    {
                        method(Clients[index].Conn);
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException || ex is System.Net.Sockets.SocketException)
                        {
                            PingAll();
                        }
                    }
                }
            }
        }

        private void SendGameStart()
        {
            GamesStuffs((conn) => conn.SendGameStart(GameData, ClientUris));
        }

        private void SendGameEnd()
        {
            GamesStuffs((conn) => conn.SendGameEnd(GameData));
        }

        private void SendGameState(IGameData gameData)
        {
            GamesStuffs((conn) => conn.SendGameState(gameData));
        }

        public void SendKeys(string pid, bool[] keys)
        {
            PlayerActions.Add(new PlayerAction(pid, keys));
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
            //lock (this)
            {
                if (round <= gameDataByRound.Count)
                {
                    List<string> result = gameDataByRound[round - 1];
                    return result;
                }
                else
                {
                    Console.WriteLine("Waiting for round : " + round + " on Server");
                    while (gameDataByRound.Count < round) { }
                    List<string> result = gameDataByRound[round - 1];
                    return result;
                }
            }
        }
    }
}

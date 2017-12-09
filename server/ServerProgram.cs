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

    class ServerProgram : IGameServer, ISlaveControl
    {
        private StateMachine gameInstance;
        private List<ServiceClient> clients;
        private List<PlayerAction> playerActions;

        internal Dictionary<int, List<string>> gameDataByRound;
        public int _round = 0;
        ServerProgram mainObject;

        public ServerProgram()
        {
            Clients = new List<ServiceClient>();
            playerActions = new List<PlayerAction>();
            gameDataByRound = new Dictionary<int, List<string>>();
            new Thread(() => PingLoop()).Start();
        }

        StateMachine GameInstance { get => gameInstance; }
        List<PlayerAction> PlayerActions { get => playerActions; }
        List<ServiceClient> ClientsCopy;
        List<ServiceClient> Clients { get => clients; set => clients = value; }
        List<Uri> ClientUris { get => Clients.Select((cli) => cli.Uri).ToList(); }
        List<string> ClientNames { get => Clients.Select((cli) => cli.Name).ToList(); }
        List<IGameClient> ClientConns { get => Clients.Select((cli) => cli.Conn).ToList(); }

        IGameData GameStateData { get => gameInstance.CurrentState.Data; }

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

        private void StartGame(string gameId)
        {
            gameInstance = new StateMachine(GetInitialGameState(gameId));

            GetPacmanStringResult(1, GameStateData);

            Monitor.PulseAll(this);

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
            lock (Clients)
            {
                if (deads.Count > 0)
                {
                    foreach (var iDead in deads)
                    {
                        Clients.RemoveAt(iDead);
                    }
                }
            }
        }

        private bool AnyClientAlive()
        {
            lock (this)
            {
                foreach (var client in Clients)
                {
                    var player = GameInstance.CurrentState.GetPlayer(client.Name);
                    if (player.Alive) return true;
                }
                return false;
            }
        }

        private void GameInstanceThread()
        {
            int round = 1;

            SendGameStart(round);

            while (AnyClientAlive() && (!GameInstance.CurrentState.HasEnded))
            {
                List<PlayerAction> actionsToProcess = new List<PlayerAction>(PlayerActions);
                PlayerActions.Clear();
                GameInstance.ApplyTransitions(actionsToProcess);
                GameInstance.ApplyTick();
                round += 1;
                GetPacmanStringResult(round, GameStateData.Copy());
                if (_round.Equals(round))
                {
                    Monitor.PulseAll(mainObject);
                }
                new Thread(() => SendGameState(round, GameStateData.Copy())).Start();

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

        private void SendGameStart(int round)
        {
            GamesStuffs((conn) => conn.SendGameStart(round, GameStateData, ClientUris));
        }

        private void SendGameEnd()
        {
            GamesStuffs((conn) => conn.SendGameEnd(GameStateData));
        }

        private void SendGameState(int round, IGameData gameData)
        {
            GamesStuffs((conn) => conn.SendGameState(round, gameData));
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

        public void SendKeys(string userID, bool[] keys)
        {
            PlayerActions.Add(new PlayerAction(userID, keys));
        }

        public void GlobalStatus()
        {
            lock (this)
            {

                Clients.ForEach((cl) =>
                {
                    Console.WriteLine(cl.Name + "is Alive");
                });
            }
        }

        public void InjectDelay()
        {
            throw new NotImplementedException();
        }

        public void GetPacmanStringResult(int round, IGameData gameData)
        {
            PacmanGameData data = gameData as PacmanGameData;
            List<string> result = new List<string>();

            data.PlayerData.ForEach((player) => result.Add(player.ToString()));
            data.GhostData.ForEach((ghost) => result.Add(ghost.ToString()));
            data.WallData.ForEach((wall) => result.Add(wall.ToString()));
            data.FoodData.ForEach((food) => result.Add(food.ToString()));

            gameDataByRound.Add(round, result);
        }

        public List<string> LocalState(int round)
        {
            lock (this)
            {
                if (gameDataByRound.ContainsKey(round))
                {
                    return gameDataByRound[round];
                }
                else
                {
                    _round = round;
                    mainObject = this;
                    Monitor.Wait(this);
                    return gameDataByRound[round];
                }
            }
        }
    }
}

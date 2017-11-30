using System;
using System.Collections.Generic;
using System.Linq;

namespace services
{
    [Serializable]
    public struct Vec2 // Integer vector2
    {
        private int x, y;

        public Vec2(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public override string ToString()
        {
            return String.Format("({0},{1})", x, y);
        }
    }

    [Serializable]
    public class PacmanGameState : IGameState
    {
        const int SPEED = 4, DIST = 20;

        private List<PlayerData> playerData;
        private List<EntityData> ghostData;
        private List<EntityData> foodData;

        public List<PlayerData> PlayerData { get => playerData; }
        public List<EntityData> GhostData { get => ghostData; }
        public List<EntityData> FoodData { get => foodData; }

        private Vec2 NewRandomVector(int maxX, int maxY)
        {
            Random rnd = new Random();
            return new Vec2(rnd.Next(maxX), rnd.Next(maxY));
        }

        public PacmanGameState(List<string> playerNames, int numPlayers, int numGhosts, int numFoods, int windowX, int windowY)
        {
            playerData = new List<PlayerData>();
            ghostData = new List<EntityData>();
            foodData = new List<EntityData>();

            for (int i = 0; i < numPlayers; i++)
                playerData.Add(new PlayerData(playerNames[i], new Vec2(10, 10 + DIST * i)));

            for (int i = 0; i < numGhosts; i++)
                ghostData.Add(new EntityData(NewRandomVector(windowX, windowY)));

            for (int i = 0; i < numFoods; i++)
                foodData.Add(new EntityData(NewRandomVector(windowX, windowY)));
        }

        public PlayerData GetPlayer(string pid)
        {
            foreach (var player in this.playerData)
            {
                if (player.Pid == pid) return player;
            }

            return null;
        }

        public IGameState ApplyAction(PlayerAction action)
        {
            PlayerData player = GetPlayer(action.playerId);

            if (player == null) return this; // TODO: handle better

            player.Direction = this.UpdateDirection(ref player, action);
            player.Position = this.UpdatePosition(ref player, action);
            // TODO: check invalid position here instead?
            player.Score = this.UpdateScore(ref player, action);
            player.Alive = this.UpdateAlive(ref player, action);

            return this;
        }

        private Vec2 UpdateDirection(ref PlayerData player, PlayerAction action)
        {
            int MULT = (action.isKeyDown) ? 1 : 0;
            switch (action.keyValue)
            {
                case 37: // left
                    return new Vec2(-1 * MULT, player.Direction.Y);
                case 38: // up
                    return new Vec2(player.Direction.X, -1 * MULT);
                case 39: // right
                    return new Vec2(1 * MULT, player.Direction.Y);
                case 40: // down
                    return new Vec2(player.Direction.X, 1 * MULT);
                default:
                    return player.Direction;
            }
        }

        private Vec2 UpdatePosition(ref PlayerData player, PlayerAction action)
        {
            return new Vec2(
                player.Position.X + player.Direction.X * SPEED,
                player.Position.Y + player.Direction.Y * SPEED);
        }

        private int UpdateScore(ref PlayerData player, PlayerAction action)
        {
            // TODO: check collision with food
            return player.Score;
        }

        private bool UpdateAlive(ref PlayerData player, PlayerAction action)
        {
            // TODO: check collision with ghosts
            return player.Alive;
        }

        public override string ToString()
        {
            string names = String.Join(" ", PlayerData.Select(player => player.Pid));
            string positions = String.Join(" ", PlayerData.Select(player => player.Position.ToString()));
            string scores = String.Join(" ", PlayerData.Select(player => player.Score.ToString()));
            return String.Join(Environment.NewLine, names, positions, scores);
        }
    }

    [Serializable]
    public class PlayerData : EntityData
    {
        private string pid;
        private int score;
        private Vec2 direction;

        public string Pid { get => pid; set => pid = value; }
        public int Score { get => score; set => score = value; }
        internal Vec2 Direction { get => direction; set => direction = value; }

        public PlayerData(string pid, Vec2 pos) : this(pid, pos.X, pos.Y) { }

        public PlayerData(string pid, int x, int y) : this(pid, x, y, 0, true) { }

        public PlayerData(string pid, int x, int y, int score, bool alive) : base(x, y, alive)
        {
            Pid = pid;
            Score = score;
            Direction = new Vec2(0, 0);
        }
    }

    [Serializable]
    public class EntityData
    {
        private bool alive;
        private Vec2 position;

        public bool Alive { get => alive; set => alive = value; }
        public Vec2 Position { get => position; set => position = value; }

        public EntityData(Vec2 pos) : this(pos.X, pos.Y) { }

        public EntityData(int x, int y) : this(x, y, true) { }

        public EntityData(int x, int y, bool alive)
        {
            Position = new Vec2(x, y);
            Alive = alive;
        }
    }

    public class StateMachine
    {
        private IGameState currentState;

        public IGameState CurrentState { get { return currentState; } }

        public StateMachine(IGameState initialState)
        {
            currentState = initialState;
        }

        public IGameState ApplyTransition(PlayerAction action)
        {
            return CurrentState.ApplyAction(action);
        }

        public IGameState ApplyTransitions(List<PlayerAction> actions)
        {
            foreach (PlayerAction action in actions)
            {
                CurrentState.ApplyAction(action);
            }

            return CurrentState;
        }

    }
}

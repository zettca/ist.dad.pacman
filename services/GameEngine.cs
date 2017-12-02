using System;
using System.Collections.Generic;

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

        private int windowX, windowY;

        public List<PlayerData> PlayerData { get => playerData; }
        public List<EntityData> GhostData { get => ghostData; }
        public List<EntityData> FoodData { get => foodData; }

        private Vec2 NewRandomVector(int maxX, int maxY)
        {
            Random rnd = new Random();
            return new Vec2(rnd.Next(maxX), rnd.Next(maxY));
        }

        public PacmanGameState(List<Guid> playerNames, int numPlayers, int numGhosts, int numFoods, int windowX, int windowY)
        {
            playerData = new List<PlayerData>();
            ghostData = new List<EntityData>();
            foodData = new List<EntityData>();
            this.windowX = windowX;
            this.windowY = windowY;

            for (int i = 0; i < numPlayers; i++)
                playerData.Add(new PlayerData(playerNames[i], new Vec2(10, 10 + DIST * i)));

            for (int i = 0; i < numGhosts; i++)
                ghostData.Add(new EntityData(NewRandomVector(windowX, windowY)));

            for (int i = 0; i < numFoods; i++)
                foodData.Add(new EntityData(NewRandomVector(windowX, windowY)));
        }

        public PlayerData GetPlayer(Guid pid)
        {
            foreach (var player in this.playerData)
            {
                if (player.Pid == pid) return player;
            }

            return null;
        }

        public IGameState ApplyAction(PlayerAction action)
        {
            PlayerData player = GetPlayer(action.pid);

            if (player == null) return this; // TODO: handle better

            player.Direction = UpdateDirection(player.Direction, action);
            return this;
        }

        public IGameState ApplyTick()
        {
            foreach (var player in playerData)
            {
                player.Position = UpdatePosition(player);

                int score = ProcessCollisionScore(player.Position);
                if (score < 0)
                {
                    player.Alive = false;
                }
                else if (score > 0)
                {
                    player.Score += score;
                }
            }

            return this;
        }

        private Vec2 UpdateDirection(Vec2 dir, PlayerAction action)
        {
            int MULT = (action.isKeyDown) ? 1 : 0;
            switch (action.keyValue)
            {
                case 37: // left
                    return new Vec2(-1 * MULT, dir.Y);
                case 38: // up
                    return new Vec2(dir.X, -1 * MULT);
                case 39: // right
                    return new Vec2(1 * MULT, dir.Y);
                case 40: // down
                    return new Vec2(dir.X, 1 * MULT);
                default:
                    return dir;
            }
        }

        private Vec2 UpdatePosition(PlayerData player)
        {
            Vec2 pos = new Vec2(
                player.Position.X + player.Direction.X * SPEED,
                player.Position.Y + player.Direction.Y * SPEED);

            if (pos.X <= 0) pos.X = 0;
            else if (pos.X >= windowX) pos.X = windowX;
            else if (pos.Y <= 0) pos.Y = 0;
            else if (pos.Y >= windowY) pos.Y = windowY;

            return pos;
        }

        private bool DoBoxesIntersect(Vec2 pos1, Vec2 size1, Vec2 pos2, Vec2 size2)
        {
            return ((pos1.X - pos2.X) * 2 < (size1.X + size2.X)) &&
                   ((pos1.Y - pos2.Y) * 2 < (size1.Y + size2.Y));
        }

        private int ProcessCollisionScore(Vec2 playerPos)
        {
            foreach (var ghost in ghostData)
            {
                if (DoBoxesIntersect(playerPos, new Vec2(20, 20), ghost.Position, new Vec2(20, 20)))
                {
                    return -1;
                }
            }

            foreach (var food in foodData)
            {
                if (DoBoxesIntersect(playerPos, new Vec2(20, 20), food.Position, new Vec2(20, 20)))
                {
                    food.Alive = false;
                    return 10;
                }
            }

            return 0;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var player in playerData)
            {
                output += String.Format("{0} {1} {2}" + Environment.NewLine, player.Pid, player.Position, player.Score);
            }
            return output;
        }
    }

    [Serializable]
    public class PlayerData : EntityData
    {
        private Guid pid;
        private int score;
        private Vec2 direction;

        public Guid Pid { get => pid; set => pid = value; }
        public int Score { get => score; set => score = value; }
        internal Vec2 Direction { get => direction; set => direction = value; }

        public PlayerData(Guid pid, Vec2 pos) : this(pid, pos.X, pos.Y) { }

        public PlayerData(Guid pid, int x, int y) : this(pid, x, y, 0, true) { }

        public PlayerData(Guid pid, int x, int y, int score, bool alive) : base(x, y, alive)
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

        public IGameState ApplyTransitions(ICollection<PlayerAction> actions)
        {
            foreach (var action in actions)
            {
                CurrentState.ApplyAction(action);
            }

            return CurrentState;
        }

        public IGameState ApplyTick()
        {
            return CurrentState.ApplyTick();
        }
    }
}

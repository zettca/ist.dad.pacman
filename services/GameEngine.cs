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
        const int SPEED = 8, SIZE = 30;

        private List<PlayerData> playerData;
        private List<EntityData> ghostData;
        private List<EntityData> foodData;

        private int windowX, windowY;

        public List<PlayerData> PlayerData { get => playerData; }
        public List<EntityData> GhostData { get => ghostData; }
        public List<EntityData> FoodData { get => foodData; }

        private Random rnd = new Random();
        private Vec2 NewRandomVector(int maxX, int maxY)
        {
            return new Vec2(rnd.Next(maxX), rnd.Next(maxY));
        }

        public PacmanGameState(List<Guid> playerIDs, int numPlayers, int numGhosts, int numFoods, int windowX, int windowY)
        {
            playerData = new List<PlayerData>();
            ghostData = new List<EntityData>();
            foodData = new List<EntityData>();
            this.windowX = windowX;
            this.windowY = windowY;

            for (int i = 0; i < numPlayers; i++)
                playerData.Add(
                    new PlayerData(playerIDs[i],
                        new Vec2(8, 40 * (i + 1)),
                        new Vec2(SIZE, SIZE)));

            for (int i = 0; i < numGhosts; i++)
                ghostData.Add(new EntityData(NewRandomVector(windowX, windowY), new Vec2(SIZE, SIZE)));

            for (int i = 0; i < numFoods; i++)
                foodData.Add(new EntityData(NewRandomVector(windowX, windowY), new Vec2(20, 20)));
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
                ProcessCollision(player);
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
            if (pos.Y <= 0) pos.Y = 0;
            if (pos.X >= windowX) pos.X = windowX;
            if (pos.Y >= windowY) pos.Y = windowY;

            return pos;
        }

        private bool DoBoxesIntersect(EntityData e1, EntityData e2)
        {
            return ((e1.Position.X - e2.Position.X) * 2 < (e1.Size.X + e2.Size.X)) &&
                   ((e1.Position.Y - e2.Position.Y) * 2 < (e1.Size.Y + e2.Size.Y));
        }

        private void ProcessCollision(PlayerData player)
        {
            foreach (var ghost in ghostData)
            {
                if (DoBoxesIntersect(player, ghost))
                {
                    player.Alive = false;
                }
            }

            foreach (var food in foodData)
            {
                if (DoBoxesIntersect(player, food))
                {
                    food.Alive = false;
                    player.Score += 10;
                }
            }
        }

        public override string ToString()
        {
            string output = "";
            foreach (var player in playerData)
            {
                string shortName = player.Pid.ToString().Substring(0, 8);
                output += String.Format("{0} {1} {2}", shortName, player.Position, player.Score);
                output += Environment.NewLine;
            }
            return output;
        }
    }

    [Serializable]
    public class PlayerData : EntityData
    {
        private int score;
        private Vec2 direction;

        public int Score { get => score; set => score = value; }
        public Vec2 Direction { get => direction; set => direction = value; }

        public PlayerData(Vec2 pos, Vec2 size) : this(new Guid(), pos, size) { }

        public PlayerData(Guid pid, Vec2 pos, Vec2 size) : this(pid, pos, size, 0, true) { }

        public PlayerData(Guid pid, Vec2 pos, Vec2 size, int score, bool alive) : base(pos, size, alive)
        {
            Pid = pid;
            Score = score;
            Direction = new Vec2(0, 0);
        }
    }

    [Serializable]
    public class EntityData
    {
        private Guid pid;
        private bool alive;
        private Vec2 position;
        private Vec2 size;

        public Guid Pid { get => pid; set => pid = value; } // internal modifier ?
        public bool Alive { get => alive; set => alive = value; }
        public Vec2 Position { get => position; set => position = value; }
        public Vec2 Size { get => size; set => size = value; }

        public EntityData(Vec2 pos, Vec2 size) : this(pos, size, true) { }

        public EntityData(Vec2 pos, Vec2 size, bool alive) : this(new Guid(), pos, size, alive) { }

        public EntityData(Guid guid, Vec2 pos, Vec2 size, bool alive)
        {
            Pid = guid;
            Position = pos;
            Size = size;
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

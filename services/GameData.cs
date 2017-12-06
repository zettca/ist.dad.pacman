using System;
using System.Collections.Generic;
using System.Linq;

namespace services
{
    [Serializable]
    public class Vec2 // Integer Vector2
    {
        private int x, y;

        public Vec2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public void Invert()
        {
            X *= -1;
            Y *= -1;
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", x, y);
        }
    }

    [Serializable]
    public class PacmanGameData : IGameData
    {
        private List<PlayerData> playerData;
        private List<EntityData> ghostData;
        private List<EntityData> foodData;
        private List<EntityData> wallData;

        public List<PlayerData> PlayerData { get => playerData; }
        public List<EntityData> GhostData { get => ghostData; }
        public List<EntityData> FoodData { get => foodData; }
        public List<EntityData> WallData { get => wallData; }

        public PacmanGameData()
        {
            playerData = new List<PlayerData>();
            ghostData = new List<EntityData>();
            foodData = new List<EntityData>();
            wallData = new List<EntityData>();
        }
    }

    [Serializable]
    public class PlayerData : EntityData
    {
        private int score;

        public int Score { get => score; set => score = value; }

        public PlayerData(Vec2 pos, Vec2 size) : this(Guid.NewGuid(), pos, size) { }

        public PlayerData(Guid pid, Vec2 pos, Vec2 size) : this(pid, pos, size, 0, true) { }

        public PlayerData(Guid pid, Vec2 pos, Vec2 size, int score, bool alive) :
            base(pos, new Vec2(0, 0), size, alive)
        {
            Pid = pid;
            Score = score;
        }
    }

    [Serializable]
    public class EntityData
    {
        private Guid pid;
        private bool alive;
        private Vec2 position, direction, size;

        public Guid Pid { get => pid; set => pid = value; }
        public bool Alive { get => alive; set => alive = value; }
        public Vec2 Position { get => position; set => position = value; }
        public Vec2 Direction { get => direction; set => direction = value; }
        public Vec2 Size { get => size; set => size = value; }

        public EntityData(Vec2 pos, Vec2 size) : this(pos, new Vec2(0, 0), size) { }

        public EntityData(Vec2 pos, Vec2 dir, Vec2 size) : this(pos, dir, size, true) { }

        public EntityData(Vec2 pos, Vec2 dir, Vec2 size, bool alive) : this(Guid.NewGuid(), pos, dir, size, alive) { }

        public EntityData(Guid guid, Vec2 pos, Vec2 dir, Vec2 size, bool alive)
        {
            Pid = guid;
            Position = pos;
            Direction = dir;
            Size = size;
            Alive = alive;
        }

        public override string ToString()
        {
            return String.Format("{0}({1}) at {1}\tSize: {2}\tAlive: {3}",
                GetType().ToString(), pid.ToString().Substring(0, 6), position, size, alive);
        }
    }

    public class StateMachine
    {
        private IGameState currentState;

        public IGameState CurrentState { get { return currentState; } }

        public StateMachine(IGameState initialState) => currentState = initialState;

        public IGameState ApplyTransition(PlayerAction action) => CurrentState.ApplyAction(action);

        public IGameState ApplyTransitions(ICollection<PlayerAction> actions) =>
            actions.Aggregate(currentState, (state, action) => ApplyTransition(action));

        public IGameState ApplyTick() => CurrentState.ApplyTick();
    }
}

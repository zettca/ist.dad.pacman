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
        internal List<PlayerData> playerData;
        internal List<EntityData> ghostData;
        internal List<EntityData> foodData;
        internal List<EntityData> wallData;

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

        public IGameData Copy()
        {
            return new PacmanGameData
            {
                playerData = new List<PlayerData>(PlayerData),
                ghostData = new List<EntityData>(GhostData),
                foodData = new List<EntityData>(FoodData),
                wallData = new List<EntityData>(WallData),
            };
        }
    }

    [Serializable]
    public class PlayerData : EntityData
    {
        private int score;

        public int Score { get => score; set => score = value; }

        public PlayerData(string pid, Vec2 pos, Vec2 size) : this(pid, pos, size, 0, true) { }

        public PlayerData(string pid, Vec2 pos, Vec2 size, int score, bool alive) :
            base(pid, pos, new Vec2(0, 0), size, alive)
        {
            Score = score;
        }

        public override string ToString() =>
            String.Format("{0}, {1}, {2}, {3}", ID, (Alive) ? "P" : "L", Position.X, Position.Y);
    }

    [Serializable]
    public class EntityData
    {
        private string entityID;
        private bool alive;
        private Vec2 position, direction, size;

        public string ID { get => entityID; set => entityID = value; }
        public bool Alive { get => alive; set => alive = value; }
        public Vec2 Position { get => position; set => position = value; }
        public Vec2 Direction { get => direction; set => direction = value; }
        public Vec2 Size { get => size; set => size = value; }

        public EntityData(string id, Vec2 pos, Vec2 size) : this(id, pos, new Vec2(0, 0), size) { }

        public EntityData(string id, Vec2 pos, Vec2 dir, Vec2 size) : this(id, pos, dir, size, true) { }

        public EntityData(string id, Vec2 pos, Vec2 dir, Vec2 size, bool alive)
        {
            ID = id;
            Position = pos;
            Direction = dir;
            Size = size;
            Alive = alive;
        }

        public override string ToString() =>
            String.Format("{0}, {1}, {2}", entityID.Substring(0, 1), Position.X, Position.Y);
    }
}

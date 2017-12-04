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

        public override string ToString()
        {
            return String.Format("({0},{1})", x, y);
        }
    }

    [Serializable]
    public class PacmanGameState : IGameState
    {
        const int SPEED = 8, SIZE = 30;
        const int TILE_SIZE = 40;

        private int windowX, windowY;

        private List<PlayerData> playerData;
        private List<EntityData> ghostData;
        private List<EntityData> foodData;
        private List<EntityData> wallData;

        public List<PlayerData> PlayerData { get => playerData; }
        public List<EntityData> GhostData { get => ghostData; }
        public List<EntityData> FoodData { get => foodData; }
        public List<EntityData> WallData { get => wallData; }

        public bool HasEnded => !AnyPlayerAlive(playerData) || !AnyEntityAlive(foodData);

        public PacmanGameState(List<Guid> playerIDs, int numPlayers, int windowX, int windowY)
        {
            playerData = new List<PlayerData>();
            ghostData = new List<EntityData>();
            foodData = new List<EntityData>();
            wallData = new List<EntityData>();
            this.windowX = windowX;
            this.windowY = windowY;

            for (int i = 0; i < numPlayers; i++)
            {
                playerData.Add(
                    new PlayerData(playerIDs[i],
                        new Vec2(8, TILE_SIZE * (i + 1)),
                        new Vec2(SIZE, SIZE)));
            }

            DrawStaticMap();
        }

        private void DrawStaticMap()
        {
            ghostData.Add(new EntityData(new Vec2(200, 120), new Vec2(1, 0), new Vec2(SIZE, SIZE)));
            ghostData.Add(new EntityData(new Vec2(260, 200), new Vec2(0, 1), new Vec2(SIZE, SIZE)));
            ghostData.Add(new EntityData(new Vec2(260, 20), new Vec2(-1, -1), new Vec2(SIZE, SIZE)));

            wallData.Add(new EntityData(new Vec2(60, 0), new Vec2(10, 100)));
            wallData.Add(new EntityData(new Vec2(200, 60), new Vec2(60, 10)));

            for (int i = 1; i < 5; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    foodData.Add(new EntityData(new Vec2(20 * 3 * i, 20 * 3 * j), new Vec2(SIZE / 2, SIZE / 2)));
                }
            }
        }

        public PlayerData GetPlayer(Guid pid)
        {
            foreach (var player in playerData)
            {
                if (player.Pid == pid) return player;
            }

            return null;
        }

        public IGameState ApplyAction(PlayerAction action)
        {
            PlayerData player = GetPlayer(action.pid);

            if (player == null || !player.Alive) return this;

            player.Direction = UpdateDirection(player.Direction, action);
            return this;
        }

        public IGameState ApplyTick()
        {
            foreach (var player in playerData)
            {
                player.Position = UpdatePlayerPosition(player);
                ProcessCollision(player);
            }

            foreach (var ghost in ghostData)
            {
                ghost.Position = UpdateGhostPosition(ghost);
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

        private Vec2 UpdatePlayerPosition(PlayerData player)
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

        private Vec2 UpdateGhostPosition(EntityData ghost)
        {
            Vec2 pos = new Vec2(
                ghost.Position.X + ghost.Direction.X * SPEED,
                ghost.Position.Y + ghost.Direction.Y * SPEED);

            if (pos.X <= 0 || pos.X >= windowX) ghost.Direction.X *= -1;
            if (pos.Y <= 0 || pos.Y >= windowY) ghost.Direction.Y *= -1;

            return pos;
        }

        private bool DoBoxesIntersect(EntityData e1, EntityData e2)
        {
            return (Math.Abs(e1.Position.X - e2.Position.X) * 2 < (e1.Size.X + e2.Size.X)) &&
                   (Math.Abs(e1.Position.Y - e2.Position.Y) * 2 < (e1.Size.Y + e2.Size.Y));
        }

        private void ProcessCollision(PlayerData player)
        {
            if (player.Alive == false) return;

            foreach (var ghost in ghostData)
            {
                if (DoBoxesIntersect(player, ghost))
                {
                    player.Alive = false;
                    player.Direction = new Vec2(0, 0);
                }
            }

            foreach (var food in foodData)
            {
                if (food.Alive && DoBoxesIntersect(player, food))
                {
                    food.Alive = false;
                    player.Score += 10;
                }
            }

            foreach (var wall in wallData)
            {

            }
        }

        private bool AnyEntityAlive(List<EntityData> entities)
        {
            foreach (var ent in entities)
            {
                if (ent.Alive) return true;
            }
            return false;
        }

        private bool AnyPlayerAlive(List<PlayerData> players)
        {
            foreach (var ent in players)
            {
                if (ent.Alive) return true;
            }
            return false;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var player in playerData)
            {
                output += player.Pid.ToString().Substring(0, 8) + " " + player.ToString();
                output += Environment.NewLine;
            }
            return output;
        }
    }

    [Serializable]
    public class PlayerData : EntityData
    {
        private int score;

        public int Score { get => score; set => score = value; }

        public PlayerData(Vec2 pos, Vec2 size) : this(Guid.NewGuid(), pos, size) { }

        public PlayerData(Guid pid, Vec2 pos, Vec2 size) : this(pid, pos, size, 0, true) { }

        public PlayerData(Guid pid, Vec2 pos, Vec2 size, int score, bool alive) : base(pos, new Vec2(0, 0), size, alive)
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
            return String.Format("{0} at {1}\tSize: {2}\tAlive: {3}",
                GetType().ToString(), position, size, alive);
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

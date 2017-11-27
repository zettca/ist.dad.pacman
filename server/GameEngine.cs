using services;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace server
{
    public class PacmanGameState : MarshalByRefObject, IGameState
    {
        private List<PlayerData> playerData;
        private List<EntityData> ghostData;
        private List<EntityData> foodData;

        public List<PlayerData> PlayerData { get => playerData; }
        public List<EntityData> GhostData { get => ghostData; }
        public List<EntityData> FoodData { get => foodData; }

        private Vector2 NewRandomVector(int maxX, int maxY)
        {
            Random rnd = new Random();
            return new Vector2(rnd.Next(maxX), rnd.Next(maxY));
        }

        public PacmanGameState(int numPlayers, int numGhosts, int numFoods, int windowX, int windowY)
        {
            playerData = new List<PlayerData>();
            ghostData = new List<EntityData>();
            foodData = new List<EntityData>();

            for (int i = 0; i < numPlayers; i++)
                playerData.Add(new PlayerData("pid" + i.ToString(), NewRandomVector(windowX, windowY)));

            for (int i = 0; i < numGhosts; i++)
                ghostData.Add(new EntityData(NewRandomVector(windowX, windowY)));

            for (int i = 0; i < numFoods; i++)
                foodData.Add(new EntityData(NewRandomVector(windowX, windowY)));
        }

        public IGameState ApplyAction(PlayerAction action)
        {
            PlayerData player = null;
            foreach (var pl in this.playerData)
            {
                if (action.playerId == pl.Pid)
                {
                    player = pl;
                    break;
                }
            }

            if (player == null) return this; // TODO: handle better

            player.Direction = this.UpdateDirection(player, action);
            player.Position = this.UpdatePosition(player, action);
            // TODO: check invalid position here instead?
            player.Score = this.UpdateScore(player, action);
            player.Alive = this.UpdateAlive(player, action);

            return this;
        }

        private Vector2 UpdateDirection(PlayerData player, PlayerAction action)
        {
            switch (action.keyValue)
            {
                case 37: // left
                    return new Vector2((action.isKeyDown) ? -1 : 0, player.Direction.Y);
                case 38: // up
                    return new Vector2(player.Direction.X, (action.isKeyDown) ? -1 : 0);
                case 39: // right
                    return new Vector2((action.isKeyDown) ? 1 : 0, player.Direction.Y);
                case 40: // down
                    return new Vector2(player.Direction.X, (action.isKeyDown) ? 1 : 0);
                default:
                    return player.Direction;
            }
        }

        private Vector2 UpdatePosition(PlayerData player, PlayerAction action)
        {
            int mul = 2;
            return new Vector2(
                player.Position.X + player.Direction.X * mul,
                player.Position.Y + player.Direction.Y * mul); ;
        }

        private int UpdateScore(PlayerData player, PlayerAction action)
        {

            // TODO: check collision with food
            throw new NotImplementedException();
        }

        private bool UpdateAlive(PlayerData player, PlayerAction action)
        {
            // TODO: check collision with ghosts
            throw new NotImplementedException();
        }
    }

    public class PlayerData : EntityData
    {
        private string pid;
        private int score;
        private Vector2 direction;

        public string Pid { get => pid; set => pid = value; }
        public int Score { get => score; set => score = value; }
        public Vector2 Direction { get => direction; set => direction = value; }

        public PlayerData(string pid, Vector2 pos) : this(pid, (int)pos.X, (int)pos.Y) { }

        public PlayerData(string pid, int x, int y) : this(pid, x, y, 0, true) { }

        public PlayerData(string pid, int x, int y, int score, bool alive) : base(x, y, alive)
        {
            Pid = pid;
            Score = score;
            Direction = new Vector2(0, 0);
        }
    }

    public class EntityData
    {
        private Vector2 position;
        private bool alive;

        public Vector2 Position { get => position; set => position = value; }
        public bool Alive { get => alive; set => alive = value; }

        public EntityData(Vector2 pos) : this((int)pos.X, (int)pos.Y) { }

        public EntityData(int x, int y) : this(x, y, true) { }

        public EntityData(int x, int y, bool alive)
        {
            Position = new Vector2(x, y);
            Alive = alive;
        }
    }

    public class StateMachine
    {
        private IGameState currentState;

        public IGameState CurrentState { get { return currentState; } }

        public StateMachine(IGameState initialState)
        {
            this.currentState = initialState;
        }

        public IGameState ApplyTransition(PlayerAction action)
        {
            return this.CurrentState.ApplyAction(action);
        }

        public IGameState ApplyTransitions(List<PlayerAction> actions)
        {
            foreach (PlayerAction action in actions)
            {
                this.CurrentState.ApplyAction(action);
            }

            return this.CurrentState;
        }

    }
}

using services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace server
{

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

    public class PacmanGameState : IGameState
    {
        const int
            SPEED = 2,
            PACMAN_SIZE = 25,
            GHOST_SIZE = 30,
            COIN_SIZE = 15,
            TILE_SIZE = 40;

        private int windowX, windowY;
        private PacmanGameData gameData;

        public IGameData Data { get => gameData; set => gameData = (PacmanGameData)value; }
        public List<PlayerData> PlayerData { get => gameData.PlayerData; }
        public List<EntityData> GhostData { get => gameData.GhostData; }
        public List<EntityData> FoodData { get => gameData.FoodData; }
        public List<EntityData> WallData { get => gameData.WallData; }

        public bool HasEnded => !AnyPlayerAlive(PlayerData) || !AnyEntityAlive(FoodData);


        public PacmanGameState(List<string> playerIDs, int numPlayers, int windowX, int windowY)
        {
            gameData = new PacmanGameData();
            this.windowX = windowX;
            this.windowY = windowY;

            for (int i = 0; i < numPlayers; i++)
            {
                PlayerData.Add(
                    new PlayerData(playerIDs[i],
                        new Vec2(8, TILE_SIZE * (i + 1)),
                        new Vec2(PACMAN_SIZE, PACMAN_SIZE)));
            }

            DrawStaticMap();
        }

        private void DrawStaticMap()
        {
            const int OFFSET_X = 8, OFFSET_Y = 40;

            GhostData.Add(new EntityData("M", new Vec2(301 - OFFSET_X, 72 - OFFSET_Y), new Vec2(1, 1), new Vec2(GHOST_SIZE, GHOST_SIZE)));
            GhostData.Add(new EntityData("M", new Vec2(221 - OFFSET_X, 273 - OFFSET_Y), new Vec2(1, 0), new Vec2(GHOST_SIZE, GHOST_SIZE)));
            GhostData.Add(new EntityData("M", new Vec2(180 - OFFSET_X, 73 - OFFSET_Y), new Vec2(1, 0), new Vec2(GHOST_SIZE, GHOST_SIZE)));

            WallData.Add(new EntityData("W", new Vec2(288 - OFFSET_X, 240 - OFFSET_Y), new Vec2(15, 95)));
            WallData.Add(new EntityData("W", new Vec2(128 - OFFSET_X, 240 - OFFSET_Y), new Vec2(15, 95)));
            WallData.Add(new EntityData("W", new Vec2(248 - OFFSET_X, 40 - OFFSET_Y), new Vec2(15, 95)));
            WallData.Add(new EntityData("W", new Vec2(88 - OFFSET_X, 40 - OFFSET_Y), new Vec2(15, 95)));

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    FoodData.Add(new EntityData("C", new Vec2(TILE_SIZE * i, TILE_SIZE * j), new Vec2(COIN_SIZE, COIN_SIZE)));
                }
            }
        }

        public PlayerData GetPlayer(string pid) =>
            PlayerData.FirstOrDefault((player) => player.ID == pid);

        public IGameState ApplyAction(PlayerAction action)
        {
            PlayerData player = GetPlayer(action.playerID);

            if (player == null || !player.Alive) return this;

            player.Direction = UpdateDirection(player.Direction, action);
            return this;
        }

        public IGameState ApplyTick()
        {
            foreach (var player in PlayerData)
            {
                player.Position = UpdatePlayerPosition(player);
                ProcessCollision(player);
            }

            foreach (var ghost in GhostData)
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

        private Vec2 PositionStep(EntityData entity)
        {
            return new Vec2(
                entity.Position.X + entity.Direction.X * SPEED,
                entity.Position.Y + entity.Direction.Y * SPEED);
        }

        private Vec2 UpdatePlayerPosition(PlayerData player)
        {
            Vec2 pos = PositionStep(player);

            if (pos.X <= 0) pos.X = 0;
            if (pos.Y <= 0) pos.Y = 0;
            if (pos.X + player.Size.X >= windowX) pos.X = windowX - player.Size.X;
            if (pos.Y + player.Size.Y >= windowY) pos.Y = windowY - player.Size.Y;

            return pos;
        }

        private Vec2 UpdateGhostPosition(EntityData ghost)
        {
            Vec2 pos = PositionStep(ghost);

            if (pos.X <= 0 || pos.X + ghost.Size.X >= windowX) ghost.Direction.X *= -1;
            if (pos.Y <= 0 || pos.Y + ghost.Size.Y >= windowY) ghost.Direction.Y *= -1;

            foreach (var wall in WallData)
            {
                if (DoBoxesIntersect(ghost, wall))
                {
                    ghost.Direction.Invert();
                    ghost.Position = PositionStep(ghost);
                    return PositionStep(ghost); // iterate twice
                }
            }

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

            foreach (var ghost in GhostData)
            {
                if (DoBoxesIntersect(player, ghost))
                {
                    player.Alive = false;
                    player.Direction = new Vec2(0, 0);
                }
            }

            foreach (var food in FoodData)
            {
                if (food.Alive && DoBoxesIntersect(player, food))
                {
                    food.Alive = false;
                    player.Score += 10;
                }
            }

            foreach (var wall in WallData)
            {
                if (DoBoxesIntersect(player, wall))
                {
                    player.Direction = new Vec2(0, 0);
                }
            }
        }

        private bool AnyEntityAlive(List<EntityData> entities) => entities.Any((ent) => ent.Alive);

        private bool AnyPlayerAlive(List<PlayerData> players) => players.Any((pl) => pl.Alive);

        public override string ToString() =>
            PlayerData.Aggregate("", (res, pl) => res + pl.ToString() + Environment.NewLine);
    }
}

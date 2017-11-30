using System.Collections.Generic;

namespace services
{
    // TODO: implement something better, or send Keys object through network ?
    public struct PlayerAction
    {
        public string playerId;
        public int keyValue;
        public bool isKeyDown;
        public PlayerAction(string pid, int keyVal, bool isDown)
        {
            playerId = pid;
            keyValue = keyVal;
            isKeyDown = isDown;
        }
    }

    public interface IGameState
    {
        IGameState ApplyAction(PlayerAction action);
    }

    public interface IGameServer // Client >> Server
    {
        bool RegisterPlayer(int port, string username);
        void SendKey(int keyValue, bool isKeyDown);
        void SendMessage(string msg);
        List<string> GetMessageHistory();
    }

    public interface IGameClient // Server > Client
    {
        void SendGameState(IGameState state); // TODO: Marshall some GameState
        void SendMessage(string msg);
    }
}

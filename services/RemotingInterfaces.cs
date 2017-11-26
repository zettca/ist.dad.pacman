using System.Collections.Generic;

namespace services
{
    public interface IGameServer // Client >> Server
    {
        bool RegisterPlayer(int port);
        void SendKey(int keyValue, bool isKeyDown);
        void SendMessage(string msg);
        List<string> GetMessageHistory();
    }

    public interface IGameClient // Server > Client
    {
        void SendGameState(object state); // TODO: Marshall some GameState
        void SendMessage(string msg);
    }
}

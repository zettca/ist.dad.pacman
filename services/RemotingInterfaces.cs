using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace services
{
    public interface IGameServer // Client >> Server
    {
        bool RegisterPlayer(int port);
        void SendKey(int keyValue, bool isKeyDown);
        void SendMessage(string msg);
    }

    public interface IGameClient // Server > Client
    {
        void SendGameState(string state); // TODO: Marshall some GameState
        void SendMessage(string msg);
        void SendMessageHistory(List<string> msgs);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace services
{
    public interface IGameServer // Client >> Server
    {
        void RegisterPlayer(string port);
        void SendKey(int keyValue, bool isKeyDown);
    }

    public interface IGameClient // Server > Client
    {
        void SendGameState(string state /* TODO: implement GameState something */);
    }
}

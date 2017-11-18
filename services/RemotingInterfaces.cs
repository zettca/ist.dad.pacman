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
    }

    public interface IGameClient // Server > Client
    {
        void SendGameState(string state /* TODO: implement GameState*/);
    }
}

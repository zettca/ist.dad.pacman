using services;
using System;
using System.Collections.Generic;

namespace server
{
    public class ServerGameService : MarshalByRefObject, IGameServer, ISlaveControl
    {
        private ServerProgram server;

        ServerGameService()
        {
            server = new ServerProgram();
        }

        public bool RegisterPlayer(Uri endpoint, string userID)
        {
            return server.RegisterPlayer(endpoint, userID);
        }

        public void SendKeys(string pid, bool[] keys)
        {
            server.SendKeys(pid, keys);
        }

        public void GlobalStatus()
        {
            server.GlobalStatus();
        }

        public void InjectDelay()
        {
            server.InjectDelay();
        }

        public List<string> LocalState(int round)
        {
            return server.LocalState(round);
        }
    }
}

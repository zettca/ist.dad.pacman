using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace pcs
{
    class Program
    {
        const string BASE_DIR = "..\\..\\";
        const string objName = "PCS";

        const string serverPath = BASE_DIR + "pacman\\bin\\Debug\\pacman.exe";
        const string clientPath = BASE_DIR + "server\\bin\\Debug\\server.exe";

        // args: port
        static void Main(string[] args)
        {
            int port = 11000;

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            // get channel host
            ChannelDataStore data = (ChannelDataStore)channel.ChannelData;
            foreach (string uriStr in data.ChannelUris)
            {
                Console.WriteLine("PCS at:\t{0}/{1}", uriStr, objName);
            }
            Console.WriteLine();

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PCSService),
                objName,
                WellKnownObjectMode.Singleton);

            Console.Read();
        }
    }

    class PCSService : MarshalByRefObject, IPCS
    {
        public void Crash(string pid)
        {
            throw new NotImplementedException();
        }

        public void Freeze(string pid)
        {
            throw new NotImplementedException();
        }

        public void GlobalStatus()
        {
            throw new NotImplementedException();
        }

        public void InjectDelay(string src_pid, string dst_pid)
        {
            throw new NotImplementedException();
        }

        public void LocalState(string pid, string round_id)
        {
            throw new NotImplementedException();
        }

        public void StartClient(string pid, string pcs_url, string client_url, string msec, string num_players, string file_name)
        {
            throw new NotImplementedException();
        }

        public void StartServer(string pid, string pcs_url, string server_url, string msec, string num_players)
        {
            throw new NotImplementedException();
        }

        public void Unfreeze(string pid)
        {
            throw new NotImplementedException();
        }
    }
}

using PCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace pcs
{
    class Program
    {
        const string BASE_DIR = "..\\..\\..\\";
        const string objName = "PCS";

        public const string clientPath = BASE_DIR + "pacman\\bin\\Debug\\pacman.exe";
        public const string serverPath = BASE_DIR + "server\\bin\\Debug\\server.exe";

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

    public class PCSService : MarshalByRefObject, IPCS
    {
        private Dictionary<string, Process> processes = new Dictionary<string, Process>();

        public void Crash(string pid)
        {
            throw new NotImplementedException();
        }

        public void Freeze(string pid)
        {
            Process p = processes[pid];
            ProcessControl.Suspend(p);
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

        public void StartClient(string pid, string client_url, string msec, string num_players, string file_name, string server_url)
        {
            Console.WriteLine("# StartClient:");
            Console.WriteLine("\tpid={0}", pid);
            Console.WriteLine("\tclient_url={0}", client_url);
            Console.WriteLine("\tmsec={0}", msec);
            Console.WriteLine("\tnum_players={0}", num_players);
            Console.WriteLine("\tfile_name={0}", file_name);
            Console.WriteLine("\tsever_url={0}", server_url);

            Process p = new Process();
            p.StartInfo.FileName = Program.clientPath;
            // server_endpoint username client_endpoint MSEC_PER_ROUND
            p.StartInfo.Arguments = server_url + " " + pid + " " + client_url + " " + msec
                + ((file_name != null) ? (" " + file_name) : "");
            p.Start();
            processes.Add(pid, p);
        }

        public void StartServer(string pid, string server_url, string msec, string num_players)
        {
            Console.WriteLine("# StartServer:");
            Console.WriteLine("\tpid={0}", pid);
            Console.WriteLine("\tserver_url={0}", server_url);
            Console.WriteLine("\tmsec={0}", msec);
            Console.WriteLine("\tnum_players={0}", num_players);

            Process p = new Process();
            p.StartInfo.FileName = Program.serverPath;
            // endpoint msec numPlayers 
            p.StartInfo.Arguments = server_url + " " + msec + " " + num_players;
            p.Start();
            processes.Add(pid, p);
        }

        public void Unfreeze(string pid)
        {
            Process p = processes[pid];
            ProcessControl.Resume(p);
        }

        public void Print(string str)
        {
            Console.WriteLine(str);
        }
        
    }
}

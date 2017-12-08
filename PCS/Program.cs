using PCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using services;

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
        private Dictionary<string, string> urlByPid = new Dictionary<string, string>();

        public void Crash(string pid)
        {
            try
            {
                Process p = processes[pid];
                processes.Remove(pid);
                urlByPid.Remove(pid);
                if(!p.HasExited)
                    p.Kill();
            }
            catch (InvalidOperationException){ Console.WriteLine("Process {0} has already exited", pid); }
            catch (Win32Exception) { Console.WriteLine("Process {0} could not be terminated or is already terminating", pid); }
            catch (KeyNotFoundException) { Console.WriteLine("Process {0} has already been terminated", pid); }
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

        public List<string> LocalState(string pid, string round_id)
        {
            Uri uri = new Uri(urlByPid[pid]);
            ISlaveControl clientConnection = (ISlaveControl)Activator.GetObject(typeof(ISlaveControl),
                 uri.AbsoluteUri);
            List<string> result = clientConnection.LocalState(Int32.Parse(round_id));
            return result;
        }

        public void StartClient(string pid, string client_url, string msec, string num_players, string file_name, string server_url)
        {
            Console.WriteLine("# StartClient:");
            Console.WriteLine("\tpid={0}", pid);
            Console.WriteLine("\tclient_url={0}", client_url);
            Console.WriteLine("\tmsec={0}", msec);
            Console.WriteLine("\tnum_players={0}", num_players);
            Console.WriteLine("\tfile_name={0}", file_name);
            Console.WriteLine("\tserver_url={0}", server_url);

            if (processes.ContainsKey(pid))
                if (processes[pid].HasExited)
                {
                    processes.Remove(pid);
                    urlByPid.Remove(pid);
                }
            if (!processes.ContainsKey(pid))
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = Program.clientPath;
                    // server_endpoint username client_endpoint MSEC_PER_ROUND
                    p.StartInfo.Arguments = server_url + " " + pid + " " + client_url + " " + msec
                        + ((file_name != null) ? (" " + file_name) : "");
                    p.Start();
                    processes.Add(pid, p);
                    urlByPid.Add(pid, client_url);
                }
                catch(InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception) { Console.WriteLine("Couldn't Initialize the Client"); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", pid);
        }

        public void StartServer(string pid, string server_url, string msec, string num_players)
        {
            Console.WriteLine("# StartServer:");
            Console.WriteLine("\tpid={0}", pid);
            Console.WriteLine("\tserver_url={0}", server_url);
            Console.WriteLine("\tmsec={0}", msec);
            Console.WriteLine("\tnum_players={0}", num_players);

            if (processes.ContainsKey(pid))
                if (processes[pid].HasExited)
                {
                    processes.Remove(pid);
                    urlByPid.Remove(pid);
                }
            if (!processes.ContainsKey(pid))
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = Program.serverPath;
                    // endpoint msec numPlayers 
                    p.StartInfo.Arguments = server_url + " " + msec + " " + num_players;
                    p.Start();
                    processes.Add(pid, p);
                    urlByPid.Add(pid, server_url);
                }
                catch (InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception e) { Console.WriteLine("Couldn't Initialize the Server"); Console.WriteLine(e); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", pid);
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

using System;
using System.Collections.Generic;
using System.Threading;
using pcs;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace PuppetMaster
{
    class Program
    {
        private static Dictionary<string, IPCS> pcsByUrl = new Dictionary<string, IPCS>();
        private static Dictionary<string, IPCS> pcsByPID = new Dictionary<string, IPCS>();
        public static List<string> knownServers = new List<string>();


        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Error: expected PCS endpoint URL(s) as arguments.");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            TcpChannel channel = new TcpChannel(0);

            ChannelServices.RegisterChannel(channel, false);


            string line;
            Console.WriteLine("Reading lines...");
            Console.Write("> ");
            while ((line = Console.ReadLine()) != null)
            {
                string[] parameters = line.Split(' ');

                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "StartClient":
                            int len = parameters.Length;
                            if (len.Equals(6) || len.Equals(7))
                            {
                                new Thread(() => StartClient(parameters[1], parameters[2], parameters[3], parameters[4], parameters[5],
                                    len.Equals(7) ? parameters[6] : null)).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: PID PCS_URL CLIENT_URL MSEC_PER_ROUND NUM_PLAYERS [filename]");
                            break;

                        case "StartServer":
                            if (parameters.Length.Equals(6))
                            {
                                new Thread(() => StartServer(parameters[1], parameters[2], parameters[3], parameters[4], parameters[5])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: PID PCS_URL SERVER_URL MSEC_PER_ROUND NUM_PLAYERS");
                            break;

                        case "GlobalStatus":
                            new Thread(() => GlobalStatus()).Start();
                            break;

                        case "Crash":
                            if (parameters.Length.Equals(2))
                            {
                                new Thread(() => Crash(parameters[1])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: PID");
                            break;

                        case "Freeze":
                            if (parameters.Length.Equals(2))
                            {
                                new Thread(() => Freeze(parameters[1])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: PID");
                            break;

                        case "Unfreeze":
                            if (parameters.Length.Equals(2))
                            {
                                new Thread(() => Unfreeze(parameters[1])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: PID");
                            break;

                        case "InjectDelay":
                            if (parameters.Length.Equals(3))
                            {
                                new Thread(() => InjectDelay(parameters[1], parameters[2])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: src_PID dst_PID");
                            break;

                        case "LocalState":
                            if (parameters.Length.Equals(2))
                            {
                                new Thread(() => LocalState(parameters[1], parameters[2])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: PID round_id");
                            break;

                        case "Wait":
                            if (parameters.Length.Equals(2))
                            {
                                int ms = Int32.Parse(parameters[1]);
                                Thread.Sleep(ms);
                            }
                            else
                                Console.WriteLine("Expected arguments: x_ms");
                            break;
                    }
                }

                Console.Write("> ");
            }
        }

        private static void Wait(string ms)
        {
            Thread.Sleep(Int32.Parse(ms));
        }

        private static void LocalState(string pid, string round_id)
        {
            throw new NotImplementedException();
        }

        private static void InjectDelay(string src_pid, string dst_pid)
        {
            throw new NotImplementedException();
        }

        private static void Unfreeze(string pid)
        {
            try
            {
                IPCS pcs = pcsByPID[pid];
                pcs.Unfreeze(pid);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Unknown PID: {0}", pid);
            }
        }

        private static void Freeze(string pid)
        {
            try
            {
                IPCS pcs = pcsByPID[pid];
                pcs.Freeze(pid);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Unknown PID: {0}", pid);
            }
        }

        private static void GlobalStatus()
        {
            throw new NotImplementedException();
        }

        private static void Crash(string pid)
        {
            try
            {
                IPCS pcs = pcsByPID[pid];
                pcs.Crash(pid);
                pcsByPID.Remove(pid);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Unknown PID: {0}", pid);
            }
        }

        private static void StartServer(string pid, string pcs_url, string server_url, string msec, string num_players)
        {
            try
            {
                IPCS pcs = getOrConnectToPCS(pcs_url);
                pcs.StartServer(pid, server_url, msec, num_players);
                knownServers.Add(server_url);
                pcsByPID[pid] = pcs;
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("Invalid PCS_URL: {0}", e);
            }
        }

        private static void StartClient(string pid, string pcs_url, string client_url, string msec, string num_players, string file_name)
        {
            try
            {
                IPCS pcs = getOrConnectToPCS(pcs_url);
                try
                {
                    string server_url = knownServers[0];
                    pcs.StartClient(pid, client_url, msec, num_players, file_name, server_url);
                    pcsByPID[pid] = pcs;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("No known servers. Please start a server first.");
                }
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("Invalid PCS_URL: {0}", pcs_url);
            }
        }

        private static IPCS getOrConnectToPCS(string pcs_url)
        {
            try
            {
                return pcsByUrl[pcs_url];
            }
            catch (KeyNotFoundException e)
            {
                new Uri(pcs_url);
                Console.WriteLine("Connecting to {0}", pcs_url);
                IPCS pcs = Activator.GetObject(typeof(PCSService), pcs_url) as IPCS;
                pcs.Print("Hello there :)");
                pcsByUrl.Add(pcs_url, pcs);
                return pcs;
            }
        }
    }
}

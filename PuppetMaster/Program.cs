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
        private static Dictionary<string, IPCS> pcsDict = new Dictionary<string, IPCS>();
        public static List<string> knownServers = new List<string>();


        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Error: expected PCS endpoint URL(s) as arguments.");
                System.Environment.Exit(-1);
            }

            TcpChannel channel = new TcpChannel(0);

            ChannelServices.RegisterChannel(channel, false);
            foreach (string arg in args)
            {
                Console.WriteLine("Connecting to {0}", arg);
                IPCS pcs = Activator.GetObject(typeof(PCSService), arg) as IPCS;
                pcs.Print("Hello there :)");
                pcsDict.Add(arg, pcs);
            }

            string line;
            Console.WriteLine("Reading lines...");
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
                                new Thread(() => Wait(parameters[1])).Start();
                            }
                            else
                                Console.WriteLine("Expected arguments: x_ms");
                            break;
                    }
                }
            }
        }

        private static void Wait(string ms)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        private static void Freeze(string pid)
        {
            throw new NotImplementedException();
        }

        private static void GlobalStatus()
        {
            throw new NotImplementedException();
        }

        private static void Crash(string pid)
        {
            throw new NotImplementedException();
        }

        private static void StartServer(string pid, string pcs_url, string server_url, string msec, string num_players)
        {
            try
            {
                IPCS pcs = pcsDict[pcs_url];
                pcs.StartServer(pid, server_url, msec, num_players);
                knownServers.Add(server_url);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Invalid PCS_URL: {0}", e);
            }
        }

        private static void StartClient(string pid, string pcs_url, string client_url, string msec, string num_players, string file_name)
        {
            try
            {
                IPCS pcs = pcsDict[pcs_url];
                try
                {
                    string server_url = knownServers[0];
                    pcs.StartClient(pid, client_url, msec, num_players, file_name, server_url);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("No known servers. Please start a server first.");
                }
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Invalid PCS_URL: {0}", pcs_url);
            }
        }
    }
}

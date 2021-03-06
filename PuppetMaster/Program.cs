﻿using System;
using System.Collections.Generic;
using System.Threading;
using pcs;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.IO;

namespace PuppetMaster
{
    class PuppetMaster
    {
        private Dictionary<string, IPCS> pcsByUrl;
        private Dictionary<string, IPCS> pcsByPID;
        public List<string> knownServers;

        public PuppetMaster()
        {
            pcsByUrl = new Dictionary<string, IPCS>();
            pcsByPID = new Dictionary<string, IPCS>();
            knownServers = new List<string>();
        }

        public void parseLine(string line)
        {
            string[] parameters = line.Split(' ');

            if (parameters.Length <= 0)
                return;

            switch (parameters[0])
            {
                case "StartClient":
                    int len = parameters.Length;
                    if (len.Equals(6) || len.Equals(7))
                    {
                        StartClient(parameters[1], parameters[2], parameters[3], parameters[4], parameters[5],
                            len.Equals(7) ? parameters[6] : null);
                    }
                    else
                        Console.WriteLine("Expected arguments: PID PCS_URL CLIENT_URL MSEC_PER_ROUND NUM_PLAYERS [filename]");
                    break;

                case "StartServer":
                    if (parameters.Length.Equals(6))
                    {
                        StartServer(parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
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
                    if (parameters.Length.Equals(3))
                    {
                        LocalState(parameters[1], parameters[2]);
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

        private void Wait(string ms)
        {
            Thread.Sleep(Int32.Parse(ms));
        }

        private void LocalState(string pid, string round_id)
        {
            try
            {
                IPCS pcs = pcsByPID[pid];
                List<String> result = pcs.LocalState(pid, round_id);

                if (result != null)
                {
                    string path = "..\\..\\LocalState-" + pid + '-' + round_id;
                    using (StreamWriter file = new StreamWriter(@path, false))
                    {
                        result.ForEach((line) =>
                        {
                            file.WriteLine(line);
                        //TODO Uncomment before submiting the project !
                        Console.WriteLine(line);
                    });
                    }
                }
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Unknown PID: {0}", pid);
            }
        }

        private void InjectDelay(string src_pid, string dst_pid)
        {
            Console.WriteLine("Not Implemented InjectDelay");
        }

        private void Unfreeze(string pid)
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

        private void Freeze(string pid)
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

        private void GlobalStatus()
        {
            Console.WriteLine("Not Implemented GlobalStatus");
        }

        private void Crash(string pid)
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

        private void StartServer(string pid, string pcs_url, string server_url, string msec, string num_players)
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

        private void StartClient(string pid, string pcs_url, string client_url, string msec, string num_players, string file_name)
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

        private IPCS getOrConnectToPCS(string pcs_url)
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


    class Program
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(0);

            ChannelServices.RegisterChannel(channel, false);

            PuppetMaster pM = new PuppetMaster();

            if (args.Length > 0)
            {
                Console.WriteLine("Reading file...");
                var lines = File.ReadAllLines(args[0]);
                foreach (string l in lines)
                {
                    pM.parseLine(l);
                }
            }

            string line;
            Console.WriteLine("Reading lines...");
            Console.Write("> ");
            while ((line = Console.ReadLine()) != null)
            {
                pM.parseLine(line);
                Console.Write("> ");
            }
        }
    }
}

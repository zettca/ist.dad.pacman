using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            string line;

            while ((line = Console.ReadLine()) != null)
            {
                string[] parameters = line.Split(' ');

                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "StartClient":
                            if (parameters.Length.Equals(7)){
                                new Thread(() => StartClient(parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]));
                            }
                            else
                                Console.WriteLine("Expected arguments: PID PCS_URL CLIENT_URL MSEC_PER_ROUND NUM_PLAYERS [filename]");
                            break;

                        case "StartServer":
                            if (parameters.Length.Equals(6)){
                                new Thread(() => StartServer(parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]));
                            }
                            else
                                Console.WriteLine("Expected arguments: PID PCS_URL SERVER_URL MSEC_PER_ROUND NUM_PLAYERS");
                            break;

                        case "GlobalStatus":
                            new Thread(() => GlobalStatus());
                            break;

                        case "Crash":
                            if (parameters.Length.Equals(2)){
                                new Thread(() => Crash(parameters[1]));
                            }
                            else
                                Console.WriteLine("Expected arguments: PID");
                            break;

                        case "Freeze":
                            if (parameters.Length.Equals(2)){
                                new Thread(() => Freeze(parameters[1]));
                            }
                            else
                                Console.WriteLine("Expected arguments: PID");
                            break;

                        case "Unfreeze":
                            if (parameters.Length.Equals(2)){
                                new Thread(() => Unfreeze(parameters[1]));
                            }
                            else
                                Console.WriteLine("Expected arguments: PID");
                            break;

                        case "InjectDelay":
                            if (parameters.Length.Equals(3)){
                                new Thread(() => InjectDelay(parameters[1],parameters[2]));
                            }
                            else
                                Console.WriteLine("Expected arguments: src_PID dst_PID");
                            break;

                        case "LocalState":
                            if (parameters.Length.Equals(2)){
                                new Thread(() => LocalState(parameters[1], parameters[2]));
                            }
                            else
                                Console.WriteLine("Expected arguments: PID round_id");
                            break;

                        case "Wait":
                            if (parameters.Length.Equals(2)){
                                new Thread(() => Wait(parameters[1]));
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
            throw new NotImplementedException();
        }

        private static void StartClient(string pid, string pcs_url, string client_url, string msec, string num_players, string file_name)
        {
            throw new NotImplementedException();
        }
    }
}

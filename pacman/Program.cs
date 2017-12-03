using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;


namespace pacman
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// 
        /// args: username client_endpoint MSEC_PER_ROUND server_endpoint
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            const string DEFAULT_SERVER = "tcp://localhost:8086/OGPGameServer";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("{0} arguments.", args.Length);
            foreach (string arg in args)
            {
                Console.WriteLine("arg: {0}", arg);
            }

            List<string[]> lines = new List<string[]>();
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                if (line.Trim() != "") lines.Add(line.Split(' '));
            }

            Uri endpoint;
            if (args.Length > 0)
            {
                endpoint = new Uri(args[0]);
            }
            else
            {
                Random rand = new Random();
                int randPort = 9000 + rand.Next(999);
                endpoint = new Uri("tcp://localhost:" + randPort + "/ClientService");
            }

            string username = (args.Length > 1) ? args[1] : endpoint.Port.ToString("D4");
            int msec = (args.Length > 2) ? Int32.Parse(args[2]) : 100;
            string server = (args.Length > 3) ? args[3] : DEFAULT_SERVER;

            Console.WriteLine("URI:\t{0}", endpoint);
            Console.WriteLine("PID:\t{0}", username);
            Console.WriteLine("MSEC:\t{0}", msec);
            Console.WriteLine("SERV:\t{0}", server);
            Console.WriteLine("Path:\t{0}", endpoint.AbsolutePath);
            Console.WriteLine("Requested Host:\t{0}", endpoint.Host);

            TcpChannel channel = new TcpChannel(endpoint.Port);
            ChannelServices.RegisterChannel(channel, false);

            // get channel host
            ChannelDataStore data = (ChannelDataStore)channel.ChannelData;
            Uri uri = new Uri(data.ChannelUris[0]);
            string host = uri.Host;
            Console.WriteLine("Current Host:\t{0}", host);

            Application.Run(new FormPacman(endpoint, username, msec, server, lines));
        }

    }
}

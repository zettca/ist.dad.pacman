using System;
using System.Windows.Forms;


namespace pacman
{
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Random rand = new Random();
            string username = (args.Length > 0) ? args[0] : rand.Next(9999).ToString("D4");
            Application.Run(new FormPacman(username));
        }

    }
}

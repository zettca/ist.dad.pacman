using services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;

namespace pacman
{
    public partial class FormPacman : Form
    {
        private string serverEndpoint;
        private string username;
        IGameServer server;
        Guid guid; // identifies the player on the server
        private PacmanClientService peer;
        private List<IGameClient> peers = new List<IGameClient>();
        Uri uri;

        private bool goup, godown, goleft, goright;
        private bool readingFromFile = false;
        private List<string[]> stdinLines;
        private int numRounds = 0;

        Image
            imgLeft = Properties.Resources.Left,
            imgRight = Properties.Resources.Right,
            imgDown = Properties.Resources.Down,
            imgUp = Properties.Resources.Up;

        public FormPacman(Uri uri, string username, int msec, string serverEndpoint, List<string[]> lines)
        {
            InitializeComponent();
            this.username = username;
            this.Text = username + " - Pacman Client at " + uri;
            this.uri = uri;
            // msec not needed yet.
            this.serverEndpoint = serverEndpoint;

            if (lines.Count > 0)
            {
                readingFromFile = true;
                stdinLines = lines;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (goleft) return;
                    goleft = true;
                    break;
                case Keys.Right:
                    if (goright) return;
                    goright = true;
                    break;
                case Keys.Up:
                    if (goup) return;
                    goup = true;
                    break;
                case Keys.Down:
                    if (godown) return;
                    godown = true;
                    break;
                case Keys.Enter:
                    tbMsg.Enabled = true;
                    tbMsg.Focus();
                    return;
                default:
                    return;
            }

            server.SendKey(guid, e.KeyValue, true);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    goleft = false;
                    break;
                case Keys.Right:
                    goright = false;
                    break;
                case Keys.Up:
                    goup = false;
                    break;
                case Keys.Down:
                    godown = false;
                    break;
                default:
                    return;
            }

            server.SendKey(guid, e.KeyValue, false);
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                services.Message msg = new services.Message(username, tbMsg.Text);
                BroadcastMessage(msg);
                AddMessage(msg);

                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            peer = new PacmanClientService(uri, username);
            string objName = uri.AbsolutePath.Replace("/", "");
            Console.WriteLine("objName:\t{0}", objName);
            RemotingServices.Marshal(peer, objName, typeof(PacmanClientService));

            Console.WriteLine("Created PacmanClientService at " + uri.AbsoluteUri);
            Console.WriteLine("Connecting to server at " + serverEndpoint);

            peer.form = this;
            server = Activator.GetObject(typeof(IGameServer), serverEndpoint) as IGameServer;

            guid = server.RegisterPlayer(uri, username);

            if (guid == Guid.Empty)
            {
                MessageBox.Show("Server refused connection. Maybe room is already full?"); // TODO: handle with exception? ignore?
                this.Close();
            }

        }

        public void AddMessage(services.Message msg)
        {
            tbChat.Text += msg.ToString();
        }

        public void UpdateGame(PacmanGameState gameState)
        {
            numRounds++;
            if (readingFromFile)
            {
                string[] line = stdinLines[0];
                stdinLines.RemoveAt(0);
                if (Int32.Parse(line?[0]) == numRounds)
                {
                    server.SendKey(guid, Int32.Parse(line[1]), Convert.ToBoolean(line[2]));
                }
                else if (line?[0] == null)
                {
                    readingFromFile = false;
                }
            }

            foreach (var player in gameState.PlayerData)
            {
                PictureBox pic = panelCanvas.Controls.Find(player.Pid.ToString(), true)[0] as PictureBox;
                Image img = GetPacmanDirectionImage(player.Direction);
                pic.Location = new Point(player.Position.X, player.Position.Y);
                if (!player.Alive) pic.BackColor = Color.Red;
                if (img != null && pic.Image != img) pic.Image = img;
                if (player.Pid == guid)
                {
                    labelTitle.Text = player.Position.ToString();
                    labelScore.Text = player.Score.ToString();
                }
            }

            foreach (var ghost in gameState.GhostData)
            {
                PictureBox pic = panelCanvas.Controls.Find(ghost.Pid.ToString(), true)[0] as PictureBox;
                pic.Location = new Point(ghost.Position.X, ghost.Position.Y);
            }

            foreach (var food in gameState.FoodData)
            {
                PictureBox pic = panelCanvas.Controls.Find(food.Pid.ToString(), true)[0] as PictureBox;
                if (!food.Alive && pic.Visible) pic.Visible = false;
            }
        }

        public void DrawGame(PacmanGameState gameState)
        {
            foreach (var player in gameState.PlayerData)
            {
                PictureBox pic = CreatePictureForEntity(player, imgLeft);
                if (player.Pid == guid) pic.BackColor = Color.Gray;
            }

            foreach (var ghost in gameState.GhostData)
            {
                CreatePictureForEntity(ghost, Properties.Resources.pink_guy);
            }

            foreach (var food in gameState.FoodData)
            {
                CreatePictureForEntity(food, Properties.Resources.cccc);
            }

            foreach (var wall in gameState.WallData)
            {
                AddMessage(new services.Message("cenas", wall.Position.ToString() + wall.Size.ToString()));
                CreatePictureForEntity(wall, null);
            }
        }

        private PictureBox CreatePictureForEntity(EntityData entity, Image image)
        {
            PictureBox pic = new PictureBox
            {
                Name = entity.Pid.ToString(),
                Size = new Size(entity.Size.X, entity.Size.Y),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(entity.Position.X, entity.Position.Y),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = (image != null) ? Color.Transparent : Color.DarkBlue,
                Image = image,
            };
            panelCanvas.Controls.Add(pic);
            return pic;
        }


        private void DrawObject(EntityData entity, Image image)
        {
            Control[] pics = panelCanvas.Controls.Find(entity.Pid.ToString(), true);
            PictureBox pic = (pics.Length == 0) ? CreatePictureForEntity(entity, image) : pics[0] as PictureBox;

            pic.Location = new Point(entity.Position.X, entity.Position.Y);
            if (!entity.Alive) pic.Hide();
        }

        private Image GetPacmanDirectionImage(Vec2 dir)
        {
            if (dir.X > 0) return imgRight;
            if (dir.X < 0) return imgLeft;
            if (dir.Y > 0) return imgDown;
            if (dir.Y < 0) return imgUp;

            return null;
        }

        private void BroadcastMessage(services.Message msg)
        {
            foreach (IGameClient peer in peers)
            {
                string peerName = ((PacmanClientService)peer).username;
                Console.WriteLine("Message from " + username + " to " + peerName);

                Thread thread = new Thread(() => peer.SendMessage(msg));
                thread.Start();
            }
        }

        internal void AddPeer(IGameClient peer)
        {
            peers.Add(peer);
        }

        public void Alert(string msg)
        {
            MessageBox.Show(msg);
        }
    }

    delegate void StringHandler(string msg);
    delegate void GameHandler(PacmanGameState state);
    delegate void MessageHandler(services.Message msg);

    public class PacmanClientService : MarshalByRefObject, IGameClient
    {
        public FormPacman form;
        public string username { get; private set; }
        private Uri endpoint;

        public PacmanClientService(Uri endpoint, string username)
        {
            this.username = username;
            this.endpoint = endpoint;
        }

        public void RegisterNewClient(Uri peerClientObjectEndpoint)
        {
            IGameClient peer = (IGameClient)Activator.GetObject(typeof(IGameClient),
                peerClientObjectEndpoint.AbsoluteUri);

            form.AddPeer(peer);
        }

        public void SendGameStart(IGameState state, List<Uri> peerEndpoints)
        {
            form.Invoke(new GameHandler(form.DrawGame), (PacmanGameState)state);
        }

        public void SendGameState(IGameState state)
        {
            form.Invoke(new GameHandler(form.UpdateGame), (PacmanGameState)state);
        }

        public void SendMessage(services.Message msg)
        {
            form.Invoke(new MessageHandler(form.AddMessage), msg);
        }

        public Uri Uri => endpoint;

        public void SendScoreboard(Dictionary<Guid, int> scoreboard)
        {
            if (scoreboard == null) return;
            string scores = "";
            foreach (var entry in scoreboard)
            {
                scores += String.Format("{0} {1}" + Environment.NewLine, entry.Key + " " + entry.Value);
            }
            form.Invoke(new StringHandler(form.Alert), scores);
        }
    }
}

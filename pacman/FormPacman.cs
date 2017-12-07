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
                ChatMessage msg = new ChatMessage(peer.Clock, username, tbMsg.Text);
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

        public void AddMessage(ChatMessage msg)
        {
            tbChat.Text += msg.ToString();
        }

        public void UpdateGame(PacmanGameData gameData)
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

            foreach (var player in gameData.PlayerData)
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

            foreach (var ghost in gameData.GhostData)
            {
                PictureBox pic = panelCanvas.Controls.Find(ghost.Pid.ToString(), true)[0] as PictureBox;
                pic.Location = new Point(ghost.Position.X, ghost.Position.Y);
            }

            foreach (var food in gameData.FoodData)
            {
                PictureBox pic = panelCanvas.Controls.Find(food.Pid.ToString(), true)[0] as PictureBox;
                if (!food.Alive && pic.Visible) pic.Visible = false;
            }
        }

        public void DrawGame(PacmanGameData gameData)
        {
            gameData.PlayerData.ForEach((player) =>
            {
                PictureBox pic = CreatePictureForEntity(player, imgLeft);
                if (player.Pid == guid) pic.BackColor = Color.Gray;
            });

            gameData.GhostData.ForEach((ghost) =>
                CreatePictureForEntity(ghost, Properties.Resources.pink_guy));

            gameData.FoodData.ForEach((food) =>
                CreatePictureForEntity(food, Properties.Resources.cccc));

            gameData.WallData.ForEach((wall) => CreatePictureForEntity(wall, null));
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

        private Image GetPacmanDirectionImage(Vec2 dir)
        {
            if (dir.X > 0) return imgRight;
            if (dir.X < 0) return imgLeft;
            if (dir.Y > 0) return imgDown;
            if (dir.Y < 0) return imgUp;

            return null;
        }

        private void BroadcastMessage(ChatMessage msg) =>
            peers.ForEach((peer) => new Thread(() => peer.SendMessage(msg)).Start());

        internal void AddPeer(Uri peerEndpoint) =>
            peers.Add((IGameClient)Activator.GetObject(typeof(IGameClient),
                peerEndpoint.AbsoluteUri));

        internal string WinnerMessage(Guid winnerId) =>
            (winnerId == guid) ? "YOU WON!" : "You Lost :(";
    }

    delegate void StringHandler(string msg);
    delegate void GamerHandler(Guid winnerId);
    delegate void GameHandler(PacmanGameData data);
    delegate void MessageHandler(ChatMessage msg);

    public class PacmanClientService : MarshalByRefObject, IGameClient
    {
        public FormPacman form;
        public string username { get; private set; }
        private Uri endpoint;
        public Dictionary<Uri, int> Clock { get; }

        public Uri Uri => endpoint;

        public PacmanClientService(Uri endpoint, string username)
        {
            this.username = username;
            this.endpoint = endpoint;

            Clock = new Dictionary<Uri, int>();
        }

        public void SendGameStart(IGameData data, List<Uri> peerEndpoints)
        {
            form.Invoke(new GameHandler(form.DrawGame), (PacmanGameData)data);
            peerEndpoints.ForEach((peerUri) =>
            {
                form.AddPeer(peerUri);
                Clock.Add(peerUri, (peerUri == endpoint) ? 1 : 0);
            });
        }

        public void SendGameState(IGameData data) =>
            form.Invoke(new GameHandler(form.UpdateGame), (PacmanGameData)data);

        public void SendMessage(ChatMessage msg)
        {
            form.Invoke(new MessageHandler(form.AddMessage), msg);
            Clock[endpoint]++;
        }

        public void SendScoreboard(Guid winnerId) =>
            SendMessage(new ChatMessage(null, "SERVER", form.WinnerMessage(winnerId)));
    }
}

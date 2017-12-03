using services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
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

        // direction player is moving in. Only one will be true
        bool goup, godown, goleft, goright;

        Image
            imgLeft = Properties.Resources.Left,
            imgRight = Properties.Resources.Right,
            imgDown = Properties.Resources.Down,
            imgUp = Properties.Resources.Up;


        public FormPacman(Uri uri, string username, int msec, string serverEndpoint)
        {
            InitializeComponent();
            this.username = username;
            this.Text = username + " - Pacman Client at " + uri;
            this.uri = uri;
            // msec not needed yet.
            this.serverEndpoint = serverEndpoint;
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
            RemotingServices.Marshal(peer, objName, typeof(PacmanClientService));

            Console.WriteLine("Created PacmanClientService at " + uri.AbsoluteUri);

            PacmanClientService.form = this;
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

        public void DrawGame(PacmanGameState gameState)
        {
            foreach (var player in gameState.PlayerData)
            {
                DrawPacman(player);
                if (player.Pid == guid)
                {
                    labelScore.Text = player.Score.ToString();
                    labelTitle.Text = String.Format("({0}, {1})", player.Position.X, player.Position.Y);
                }
            }

            //AddMessage(new services.Message("Debug", gameState.GhostData.Count.ToString()));
            foreach (var ghost in gameState.GhostData)
            {
                DrawStatic(ghost, Properties.Resources.pink_guy);
            }

            //AddMessage(new services.Message("Debug", gameState.GhostData.Count.ToString()));
            foreach (var food in gameState.FoodData)
            {
                DrawStatic(food, Properties.Resources.cccc);
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
                BackColor = Color.Transparent,
                Image = image,
            };
            Controls.Add(pic);
            return pic;
        }


        private void DrawStatic(EntityData entity, Image image)
        {
            Control[] pics = Controls.Find(entity.Pid.ToString(), true);

            if (pics.Length == 0)
            {
                CreatePictureForEntity(entity, image);
            }
            else if (entity.Alive == false)
            {
                foreach (Control c in pics)
                {
                    c.Hide();
                }
            }
        }

        private Image GetNewDirectionImage(Vec2 dir)
        {
            if (dir.X > 0) return imgRight;
            if (dir.X < 0) return imgLeft;
            if (dir.Y > 0) return imgDown;
            if (dir.Y < 0) return imgUp;

            return null;
        }

        private void DrawPacman(PlayerData player)
        {
            Control[] pics = Controls.Find(player.Pid.ToString(), true);

            PictureBox pic = (pics.Length == 0) ? CreatePictureForEntity(player, imgLeft) : pics[0] as PictureBox;

            pic.Location = new Point(player.Position.X, player.Position.Y);
            Image img = GetNewDirectionImage(player.Direction);
            if (img != null && pic.Image != img)
            {
                pic.Image = img;
            }
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
    }

    delegate void GameHandler(PacmanGameState state);
    delegate void MessageHandler(services.Message msg);

    public class PacmanClientService : MarshalByRefObject, IGameClient
    {
        public static FormPacman form;
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

        public void SendGameState(IGameState state)
        {
            // TODO: handle conversion and error catching
            PacmanGameState gameState = (PacmanGameState)state;
            form.Invoke(new GameHandler(form.DrawGame), (PacmanGameState)state);
        }

        public void SendMessage(services.Message msg)
        {
            form.Invoke(new MessageHandler(form.AddMessage), msg);
        }

        public Uri GetUri()
        {
            return endpoint;
        }
    }
}

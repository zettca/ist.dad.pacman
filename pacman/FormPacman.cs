using services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;



namespace pacman
{
    public partial class FormPacman : Form
    {
        const string SERVER_ENDPOINT = "tcp://localhost:8086/OGPGameServer";
        string username;
        IGameServer server;
        Guid guid; // identifies the player on the server

        // direction player is moving in. Only one will be true
        bool goup, godown, goleft, goright;

        Image
            imgLeft = Properties.Resources.Left,
            imgRight = Properties.Resources.Right,
            imgDown = Properties.Resources.Down,
            imgUp = Properties.Resources.Up;


        public FormPacman(string username)
        {
            InitializeComponent();
            this.username = username;
            this.Text = username + " - Pacman Client";
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
                server.SendMessage(guid, tbMsg.Text);
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TcpChannel channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, false);

            PacmanClientService service = new PacmanClientService();
            RemotingServices.Marshal(service, "GameClient",
                typeof(PacmanClientService));

            // Get port that was automatically generated
            int port = new Uri(((ChannelDataStore)channel.ChannelData).ChannelUris[0]).Port;

            PacmanClientService.form = this;
            server = Activator.GetObject(typeof(IGameServer), SERVER_ENDPOINT) as IGameServer;
            guid = server.RegisterPlayer(port, username);
            this.AddMessageList(server.GetMessageHistory());
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

        public void AddMessageList(List<services.Message> msgs)
        {
            tbChat.Clear();
            foreach (var msg in msgs)
            {
                AddMessage(msg);
            }
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
    }

    delegate void GameHandler(PacmanGameState state);
    delegate void MessageHandler(services.Message msg);
    delegate void MessageListHandler(List<services.Message> msgs);

    public class PacmanClientService : MarshalByRefObject, IGameClient
    {
        public static FormPacman form;

        public PacmanClientService()
        {

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
    }
}

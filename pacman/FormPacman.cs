using services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;

namespace pacman
{
    public partial class FormPacman : Form
    {
        private string serverEndpoint;
        private string userID;
        IGameServer server;
        private PacmanClientService peerService;
        private List<IGameClient> peers = new List<IGameClient>();
        Uri uri;
        internal bool gameStarted = false;

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
            this.userID = username;
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

        private void SendKeys()
        {
            bool[] keys = { goleft, goup, goright, godown };
            server.SendKeys(userID, keys);
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
                    if (gameStarted) {
                        tbMsg.Enabled = true;
                        tbMsg.Focus();
                    }
                    return;
                default:
                    return;
            }

            SendKeys();
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

            SendKeys();
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbMsg.Text.Trim().Length == 0) return;
                ChatMessage msg = new ChatMessage(peerService.Clock, userID, tbMsg.Text);
                BroadcastMessage(msg);
                AddMessage(msg);

                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            peerService = new PacmanClientService(uri, userID);
            string objName = uri.AbsolutePath.Replace("/", "");
            Console.WriteLine("objName:\t{0}", objName);
            RemotingServices.Marshal(peerService, objName, typeof(PacmanClientService));

            Console.WriteLine("Created PacmanClientService at " + uri.AbsoluteUri);
            Console.WriteLine("Connecting to server at " + serverEndpoint);

            peerService.form = this;
            server = Activator.GetObject(typeof(IGameServer), serverEndpoint) as IGameServer;

            if (!server.RegisterPlayer(uri, userID))
            {
                MessageBox.Show("Server refused connection. Maybe room is already full?"); // TODO: handle with exception? ignore?
                this.Close();
            }
        }

        private Dictionary<Uri, int> ClocksMax(Dictionary<Uri, int> a, Dictionary<Uri, int> b)
        {
            Dictionary<Uri, int> clocksMax = new Dictionary<Uri, int>();

            a.Keys.ToList().ForEach((key) => clocksMax.Add(key, Math.Max(a[key], b[key])));

            return clocksMax;
        }


        public void AddMessage(ChatMessage msg)
        {
            tbChat.Text += String.Join(", ", msg.Clock.Values.Select(x => x.ToString()).ToArray()) + Environment.NewLine;
            peerService.Clock = ClocksMax(peerService.Clock, msg.Clock);
            peerService.Clock[uri]++;
            tbChat.Text += msg.ToString();
        }

        internal void UpdateGame(PacmanGameData gameData)
        {
            numRounds++;

            if (readingFromFile)
            {
                if (stdinLines.Count > 0)
                {
                    string[] line = stdinLines[0];
                    stdinLines.RemoveAt(0);
                    if (Int32.Parse(line?[0]) == numRounds)
                    {
                        switch (line?[1])
                        {
                            case "LEFT":
                                server.SendKeys(userID, new bool[] { true, false, false, false });
                                break;
                            case "UP":
                                server.SendKeys(userID, new bool[] { false, true, false, false });
                                break;
                            case "RIGHT":
                                server.SendKeys(userID, new bool[] { false, false, true, false });
                                break;
                            case "DOWN":
                                server.SendKeys(userID, new bool[] { false, false, false, true });
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    readingFromFile = false;
                }
            }
            numRounds++;

            foreach (var player in gameData.PlayerData)
            {
                PictureBox pic = panelCanvas.Controls.Find(player.ID.ToString(), true)[0] as PictureBox;
                Image img = GetPacmanDirectionImage(player.Direction);
                pic.Location = new Point(player.Position.X, player.Position.Y);
                if (!player.Alive) pic.BackColor = Color.Red;
                if (img != null && pic.Image != img) pic.Image = img;
                if (player.ID == userID)
                {
                    labelState.Text = player.Position.ToString();
                    labelScore.Text = player.Score.ToString();
                }
            }

            foreach (var ghost in gameData.GhostData)
            {
                PictureBox pic = panelCanvas.Controls.Find(ghost.ID.ToString(), true)[0] as PictureBox;
                pic.Location = new Point(ghost.Position.X, ghost.Position.Y);
            }

            foreach (var food in gameData.FoodData)
            {
                PictureBox pic = panelCanvas.Controls.Find(food.ID.ToString(), true)[0] as PictureBox;
                if (!food.Alive && pic.Visible) pic.Visible = false;
            }
        }

        internal void DrawGame(PacmanGameData gameData)
        {
            gameData.FoodData.ForEach((food) =>
                CreatePictureForEntity(food, Properties.Resources.cccc, 100));

            gameData.GhostData.ForEach((ghost) =>
                CreatePictureForEntity(ghost, Properties.Resources.pink_guy, 5));

            gameData.PlayerData.ForEach((player) =>
            {
                PictureBox pic = CreatePictureForEntity(player, imgLeft, 2);
                if (player.ID == userID) pic.BackColor = Color.Gray;
            });

            gameData.WallData.ForEach((wall) => CreatePictureForEntity(wall, null, 4));
        }

        private PictureBox CreatePictureForEntity(EntityData entity, Image image, int zIndex)
        {
            PictureBox pic = new PictureBox
            {
                Name = entity.ID.ToString(),
                Size = new Size(entity.Size.X, entity.Size.Y),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(entity.Position.X, entity.Position.Y),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = (image != null) ? Color.Transparent : Color.DarkBlue,
                Image = image,
            };
            panelCanvas.Controls.Add(pic);
            panelCanvas.Controls.SetChildIndex(pic, zIndex);
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

        internal void EndGame(PacmanGameData data)
        {
            foreach (var player in data.PlayerData)
            {
                if (player.ID == userID)
                {
                    labelState.Text = "Game Ended!";
                }
            }
        }

        private void BroadcastMessage(ChatMessage msg) =>
            peers.ForEach((peer) => new Thread(() => peer.SendMessage(msg)).Start());

        internal void AddPeer(Uri peerEndpoint) =>
            peers.Add((IGameClient)Activator.GetObject(typeof(IGameClient),
                peerEndpoint.AbsoluteUri));

        internal string WinnerMessage(string winnerId) =>
            (winnerId == userID) ? "YOU WON!" : "You Lost :(";
    }

    delegate void StringHandler(string msg);
    delegate void GameHandler(PacmanGameData data);
    delegate void MessageHandler(ChatMessage msg);


    public class PacmanClientService : MarshalByRefObject, IGameClient, ISlaveControl
    {
        public FormPacman form;
        public string username { get; private set; }
        private Uri endpoint;
        public Dictionary<Uri, int> Clock { get; set; }

        internal Dictionary<int, List<string>> gameDataByRound;

        public Uri Uri => endpoint;

        Thread _mainThread = null;

        int _round = 0;

        public PacmanClientService(Uri endpoint, string username)
        {
            this.username = username;
            this.endpoint = endpoint;

            this.gameDataByRound = new Dictionary<int, List<string>>();

            Clock = new Dictionary<Uri, int>();
        }

        public void GetPacmanStringResult(int round, IGameData gameData)
        {
            PacmanGameData data = gameData as PacmanGameData;
            List<string> result = new List<string>();

            data.PlayerData.ForEach((player) => result.Add(player.ToString()));
            data.GhostData.ForEach((ghost) => result.Add(ghost.ToString()));
            data.WallData.ForEach((wall) => result.Add(wall.ToString()));
            data.FoodData.ForEach((food) => result.Add(food.ToString()));

            gameDataByRound.Add(round, result);
        }

        public void SendGameStart(int round, IGameData data, List<Uri> peerEndpoints)
        {
            GetPacmanStringResult(round, data);
            form.Invoke(new GameHandler(form.DrawGame), (PacmanGameData)data);
            peerEndpoints.ForEach((peerUri) =>
            {
                if (peerUri != endpoint) form.AddPeer(peerUri);
                Clock.Add(peerUri, 0);
            });
            Clock[endpoint]++;
            form.gameStarted = true;
        }

        public void SendGameState(int round, IGameData data)
        {
            GetPacmanStringResult(round, data);
            form.Invoke(new GameHandler(form.UpdateGame), (PacmanGameData)data);
        }

        public void SendMessage(ChatMessage msg) =>
            form.Invoke(new MessageHandler(form.AddMessage), msg);

        public void SendGameEnd(IGameData data) =>
            form.Invoke(new GameHandler(form.EndGame), (PacmanGameData)data);

        public void GlobalStatus()
        {
            throw new NotImplementedException();
        }

        public void InjectDelay()
        {
            throw new NotImplementedException();
        }

        public List<string> LocalState(int round)
        {
            lock (this)
            {
                if (gameDataByRound.ContainsKey(round))
                {
                    return gameDataByRound[round];
                }
                else
                {
                    _round = round;
                    _mainThread = Thread.CurrentThread;
                    while(!gameDataByRound.ContainsKey(round)) { }
                    return gameDataByRound[round]; ;
                }
            }
        }

        public void Ping()
        {
            return;
        }
    }
}

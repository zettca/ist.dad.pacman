﻿using services;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;



namespace pacman
{
    public partial class FormPacman : Form
    {
        const string SERVER_ENDPOINT = "tcp://localhost:8086/OGPGameServer";
        IGameServer server;

        // direction player is moving in. Only one will be true
        bool goup;
        bool godown;
        bool goleft;
        bool goright;

        int boardRight = 320;
        int boardBottom = 320;
        int boardLeft = 0;
        int boardTop = 40;
        //player speed
        int speed = 5;

        int score = 0; int total_coins = 61;

        //ghost speed for the one direction ghosts
        int ghost1 = 5;
        int ghost2 = 5;

        //x and y directions for the bi-direccional pink ghost
        int ghost3x = 5;
        int ghost3y = 5;

        public FormPacman()
        {
            InitializeComponent();
            labelTitle.Visible = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (goleft) break;
                    goleft = true;
                    pacman.Image = Properties.Resources.Left;
                    break;
                case Keys.Right:
                    if (goright) break;
                    goright = true;
                    pacman.Image = Properties.Resources.Right;
                    break;
                case Keys.Up:
                    if (goup) break;
                    goup = true;
                    pacman.Image = Properties.Resources.Up;
                    break;
                case Keys.Down:
                    if (godown) break;
                    godown = true;
                    pacman.Image = Properties.Resources.down;
                    break;
                case Keys.Enter:
                    tbMsg.Enabled = true;
                    tbMsg.Focus();
                    return;
                default:
                    return;
            }

            server.SendKey(e.KeyValue, true);
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

            server.SendKey(e.KeyValue, false);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelScore.Text = "Score: " + score;

            //move player
            if (goleft)
            {
                if (pacman.Left > (boardLeft))
                    pacman.Left -= speed;
            }
            if (goright)
            {
                if (pacman.Left < (boardRight))
                    pacman.Left += speed;
            }
            if (goup)
            {
                if (pacman.Top > (boardTop))
                    pacman.Top -= speed;
            }
            if (godown)
            {
                if (pacman.Top < (boardBottom))
                    pacman.Top += speed;
            }
            //move ghosts
            redGhost.Left += ghost1;
            yellowGhost.Left += ghost2;

            // if the red ghost hits the picture box 4 then wereverse the speed
            if (redGhost.Bounds.IntersectsWith(pictureBox1.Bounds))
                ghost1 = -ghost1;
            // if the red ghost hits the picture box 3 we reverse the speed
            else if (redGhost.Bounds.IntersectsWith(pictureBox2.Bounds))
                ghost1 = -ghost1;
            // if the yellow ghost hits the picture box 1 then wereverse the speed
            if (yellowGhost.Bounds.IntersectsWith(pictureBox3.Bounds))
                ghost2 = -ghost2;
            // if the yellow chost hits the picture box 2 then wereverse the speed
            else if (yellowGhost.Bounds.IntersectsWith(pictureBox4.Bounds))
                ghost2 = -ghost2;
            //moving ghosts and bumping with the walls end
            //for loop to check walls, ghosts and points
            foreach (Control x in this.Controls)
            {
                // checking if the player hits the wall or the ghost, then game is over
                if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost")
                {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds))
                    {
                        pacman.Left = 0;
                        pacman.Top = 25;
                        labelTitle.Text = "GAME OVER";
                        labelTitle.Visible = true;
                        timer1.Stop();
                    }
                }
                if (x is PictureBox && x.Tag == "coin")
                {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds))
                    {
                        this.Controls.Remove(x);
                        score++;
                        //TODO check if all coins where "eaten"
                        if (score == total_coins)
                        {
                            //pacman.Left = 0;
                            //pacman.Top = 25;
                            labelTitle.Text = "GAME WON!";
                            labelTitle.Visible = true;
                            timer1.Stop();
                        }
                    }
                }
            }
            pinkGhost.Left += ghost3x;
            pinkGhost.Top += ghost3y;

            if (pinkGhost.Left < boardLeft ||
                pinkGhost.Left > boardRight ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds)))
            {
                ghost3x = -ghost3x;
            }
            if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
            {
                ghost3y = -ghost3y;
            }
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                server.SendMessage(tbMsg.Text);
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
            bool isConnected = server.RegisterPlayer(port);
            this.AddMessageList(server.GetMessageHistory());
            if (!isConnected)
            {
                MessageBox.Show("Server refused connection"); // TODO: handle with exception? ignore?
            }

        }

        public void AddMessage(string msg)
        {
            this.tbChat.Text += msg + Environment.NewLine;
        }

        public void AddMessageList(List<string> msgs)
        {
            this.tbChat.Clear();
            foreach (string msg in msgs)
            {
                this.tbChat.Text += msg + Environment.NewLine;
            }
        }
    }

    delegate void MessageHandler(string msg);
    delegate void MessageListHandler(List<string> msgs);

    public class PacmanClientService : MarshalByRefObject, IGameClient
    {
        public static FormPacman form;

        public PacmanClientService()
        {

        }

        public void SendGameState(object state)
        {
            // TODO: update Form to match GameState
            //throw new NotImplementedException();
        }

        public void SendMessage(string msg)
        {
            form.Invoke(new MessageHandler(form.AddMessage), msg);
        }
    }
}
using System.Collections.Generic;
using System;

namespace services
{
    public struct PlayerAction
    {
        public string PID;
        public bool[] Keys;
        public PlayerAction(string pid, bool[] keys)
        {
            PID = pid;
            Keys = keys;
        }
    }

    [Serializable]
    public class ChatMessage
    {
        private string sender, message;
        private Dictionary<Uri, int> clock;

        public string Sender { get => sender; set => sender = value; }
        public string Message { get => message; set => message = value; }
        public Dictionary<Uri, int> Clock { get => clock; set => clock = value; }

        public ChatMessage(Dictionary<Uri, int> clock, string sender, string message)
        {
            Clock = clock;
            Sender = sender;
            Message = message;
        }

        public override string ToString() =>
            String.Format("{0}: {1}", Sender, Message) + Environment.NewLine;
    }

    public interface IGameState
    {
        bool HasEnded { get; }
        IGameData Data { get; }

        PlayerData GetPlayer(string pid);
        IGameState ApplyTick();
        IGameState ApplyAction(PlayerAction action);
    }

    public interface IGameData
    {
        List<PlayerData> PlayerData { get; }

        IGameData Copy();
    }

    // Client >> Server
    public interface IGameServer
    {
        bool RegisterPlayer(Uri endpoint, string userID);
        void SendKeys(string userID, bool[] keys);
    }

    // Server > Client; Client -> Client
    public interface IGameClient
    {
        Uri Uri { get; }

        void Ping();
        void SendGameEnd(IGameData data);
        void SendGameStart(int round, IGameData data, List<Uri> peerEndpoints);
        void SendGameState(int round, IGameData data);
        void SendMessage(ChatMessage message);
    }

    public interface ISlaveControl // for controlling server and clients
    {
        void GlobalStatus();
        void InjectDelay();
        List<string> LocalState(int round);
    }
}

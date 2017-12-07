using System.Collections.Generic;
using System;

namespace services
{

    public struct PlayerAction
    {
        public Guid pid;
        public int keyValue;
        public bool isKeyDown;
        public PlayerAction(Guid pid, int keyVal, bool isDown)
        {
            this.pid = pid;
            this.keyValue = keyVal;
            this.isKeyDown = isDown;
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

        IGameState ApplyTick();
        IGameState ApplyAction(PlayerAction action);
    }

    public interface IGameData
    {
        List<PlayerData> PlayerData { get; }
    }

    public interface IGameServer // Client >> Server
    {
        Guid RegisterPlayer(Uri endpoint, string username);
        void SendKey(Guid from, int keyValue, bool isKeyDown);
    }

    public interface IGameClient // Server > Client; Client -> Client
    {
        Uri Uri { get; }

        void SendScoreboard(Guid winner);
        void SendGameStart(IGameData data, List<Uri> peerEndpoints);
        void SendGameState(IGameData data);
        void SendMessage(ChatMessage message);
    }
}

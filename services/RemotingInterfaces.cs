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
        public string sender, message;

        public ChatMessage(string sender, string message)
        {
            this.sender = sender;
            this.message = message;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", sender, message) + Environment.NewLine;
        }
    }

    public interface IGameState
    {
        bool HasEnded { get; }

        IGameState ApplyTick();
        IGameState ApplyAction(PlayerAction action);
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
        void SendGameStart(IGameState state, List<Uri> peerEndpoints);
        void SendGameState(IGameState state);
        void SendMessage(ChatMessage message);

        // used by server to broadcast new clients
        void RegisterNewClient(Uri peerClientObjectEndpoint);
    }
}

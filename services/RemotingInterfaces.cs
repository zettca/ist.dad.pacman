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
    public class Message
    {
        public string sender, message;

        public Message(string sender, string message)
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
        void SendGameState(IGameState state); // TODO: Marshall some GameState
        void SendMessage(Message message);

        // used by server to broadcast new clients
        void RegisterNewClient(Uri peerClientObjectEndpoint);
        Uri GetUri();
    }
}

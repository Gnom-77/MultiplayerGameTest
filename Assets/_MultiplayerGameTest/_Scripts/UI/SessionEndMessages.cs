using _MultiplayerGameTest._Scripts.Core.Networking;

namespace _MultiplayerGameTest._Scripts.UI
{
    public class SessionEndMessages
    {
        public static string Get(SessionEndReason reason)
        {
            switch (reason)
            {
                case SessionEndReason.SessionFull: return "The session is full.";
                case SessionEndReason.VersionMismatch: return "Game version differs from the host.";
                case SessionEndReason.DuplicatePlayer: return "You are already in this session.";
                case SessionEndReason.ConnectionTimeout: return "Connection timed out.";
                case SessionEndReason.Kicked: return "You were kicked by the host.";
                case SessionEndReason.ConnectionLost: return "Connection to the host was lost.";
                default: return "Disconnected.";
            }
        }
    }
}
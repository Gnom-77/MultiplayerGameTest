using System;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public interface INetworkSessionService
    {
        SessionState State { get; }
        bool IsHost { get; }

        event Action<SessionState> StateChanged;
        event Action<SessionEndReason> SessionEnded;

        void StartHost(ushort port);
        void Join(ConnectionTarget target);
        void Shutdown();
    }
}
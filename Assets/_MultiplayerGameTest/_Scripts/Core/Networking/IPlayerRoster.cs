using System;
using System.Collections.Generic;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Read-only view of the players currently in the session. Synchronized from the host.
    /// </summary>
    public interface IPlayerRoster
    {
        IReadOnlyList<PlayerData> Players { get; }
        ulong LocalClientId { get; }
        event Action Changed;
    }
}
using System;
using Unity.Netcode;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class NetworkPlayerRoster : NetworkBehaviour
    {
        private NetworkList<NetworkPlayerInfo> _players;

        public event Action Changed;

        public int Count => _players.Count;
        public NetworkPlayerInfo this[int index] => _players[index];

        private void Awake() => _players = new NetworkList<NetworkPlayerInfo>();

        public override void OnNetworkSpawn()
        {
            _players.OnListChanged += HandleListChanged;
            Changed?.Invoke();
        }

        public override void OnNetworkDespawn() => _players.OnListChanged -= HandleListChanged;

        public void Add(NetworkPlayerInfo info)
        {
            if (!IsServer || IndexOf(info.ClientId) >= 0)
                return;

            _players.Add(info);
        }

        public void Remove(ulong clientId)
        {
            if (!IsServer)
                return;

            var index = IndexOf(clientId);
            if (index >= 0)
                _players.RemoveAt(index);
        }

        private int IndexOf(ulong clientId)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].ClientId == clientId)
                    return i;
            }

            return -1;
        }

        private void HandleListChanged(NetworkListEvent<NetworkPlayerInfo> _) => Changed?.Invoke();
    }
}
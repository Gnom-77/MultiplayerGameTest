namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public readonly struct PlayerData
    {
        public ulong ClientId { get; }
        public string PlayerId { get; }
        public string Name { get; }
        public bool IsHost { get; }

        public PlayerData(ulong clientId, string playerId, string name, bool isHost)
        {
            ClientId = clientId;
            PlayerId = playerId;
            Name = name;
            IsHost = isHost;
        }
    }
}
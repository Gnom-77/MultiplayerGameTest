using System;
using Unity.Collections;
using Unity.Netcode;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public struct NetworkPlayerInfo : INetworkSerializable, IEquatable<NetworkPlayerInfo>
    {
        public ulong ClientId;
        public FixedString64Bytes PlayerId;
        public FixedString64Bytes Name;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref Name);
        }

        public bool Equals(NetworkPlayerInfo other) =>
            ClientId == other.ClientId && PlayerId.Equals(other.PlayerId) && Name.Equals(other.Name);
    }
}
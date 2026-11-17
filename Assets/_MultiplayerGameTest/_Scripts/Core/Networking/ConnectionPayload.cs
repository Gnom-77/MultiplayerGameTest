using System;
using System.Text;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Data sent by a client during the connection handshake. Validated by the host.
    /// </summary>
    [Serializable]
    public class ConnectionPayload
    {
        private const int MaxSizeBytes = 1024;
        
        public string playerId;
        public string playerName;
        public string buildVersion;

        public byte[] ToBytes() => Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));

        public static bool TryParse(byte[] data, out ConnectionPayload payload)
        {
            payload = null;
            if (data == null || data.Length == 0 || data.Length > MaxSizeBytes)
                return false;

            try
            {
                payload = JsonUtility.FromJson<ConnectionPayload>(Encoding.UTF8.GetString(data));
                return payload != null && !string.IsNullOrEmpty(payload.playerId);
            }
            catch (Exception)
            {
                payload = null;
                return false;
            }
        }
    }
}
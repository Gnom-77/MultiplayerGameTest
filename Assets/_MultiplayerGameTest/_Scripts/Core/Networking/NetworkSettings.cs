using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Static network configuration. Can be overridden by command line arguments.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Network Settings", fileName = "NetworkSettings")]
    public class NetworkSettings : ScriptableObject
    {
        [SerializeField] private TransportKind defaultTransport = TransportKind.UnityTransport;
        [SerializeField] private string defaultAdress = "127.0.0.1";
        [SerializeField, Range(1024, 65535)] private int defaultPort = 1234;
        [SerializeField, Range(2, 4)] private int maxPlayers = 4;
        
        public TransportKind DefaultTransport => defaultTransport;
        public string DefaultAdress => defaultAdress;
        public int DefaultPort => Mathf.Clamp(defaultPort, 1024, 65535);
        public int MaxPlayers => maxPlayers;
    }
}
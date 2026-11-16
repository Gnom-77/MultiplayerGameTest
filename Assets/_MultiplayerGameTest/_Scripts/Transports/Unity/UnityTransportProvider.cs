using _MultiplayerGameTest._Scripts.Core.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace _MultiplayerGameTest._Scripts.Transports.Unity
{
    /// <summary>
    /// Unity Transport (UDP, IP:port) implementation.
    /// </summary>
    public class UnityTransportProvider : INetworkTransportProvider
    {
        public TransportKind Kind => TransportKind.UnityTransport;
        
        public void ConfigureForHost(NetworkManager networkManager, int port)
        {
            var transport = GetOrAddTransport(networkManager);
            transport.SetConnectionData("127.0.0.1", (ushort)port, "0.0.0.0");
        }

        public void ConfigureForClient(NetworkManager networkManager, ConnectionTarget target)
        {
            var transport = GetOrAddTransport(networkManager);
            transport.SetConnectionData(target.Address, (ushort)target.Port);
        }
        
        private static UnityTransport GetOrAddTransport(NetworkManager networkManager)
        {
            if (!networkManager.TryGetComponent(out UnityTransport transport))
                transport = networkManager.gameObject.AddComponent<UnityTransport>();

            networkManager.NetworkConfig.NetworkTransport = transport;
            return transport;
        }
    }
}
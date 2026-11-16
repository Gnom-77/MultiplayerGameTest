using Unity.Netcode;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public interface INetworkTransportProvider
    {
        TransportKind Kind { get; }
        
        /// <summary>
        /// Prepare the transport to listen as host. Must be called before StartHost.
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="port"></param>
        void ConfigureForHost(NetworkManager networkManager, int  port);
        
        /// <summary>
        /// Prepare the transport to connect to the target. Must be called before StartClient.
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="target"></param>
        void ConfigureForClient(NetworkManager networkManager, ConnectionTarget target);
    }
}
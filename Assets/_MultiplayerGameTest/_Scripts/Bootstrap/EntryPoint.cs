using _MultiplayerGameTest._Scripts.Core.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Bootstrap
{
    public class EntryPoint : IStartable
    {
        private const string MainMenuScene = "MainMenu";
        
        private readonly LaunchOptions _launchOptions;
        private readonly INetworkTransportProvider _transportProvider;
        private readonly NetworkManager _networkManager;

        public EntryPoint(LaunchOptions options, INetworkTransportProvider transportProvider, NetworkManager networkManager)
        {
            _launchOptions = options;
            _transportProvider = transportProvider;
            _networkManager = networkManager;
        }
        
        public void Start()
        {
            Debug.Log($"Transport provider: {_transportProvider.Kind}, options: {_launchOptions}");
            
            _transportProvider.ConfigureForHost(_networkManager, _launchOptions.Port);
            Debug.Log($"NetworkTransport assigned: {_networkManager.NetworkConfig.NetworkTransport != null}");
            
            SceneManager.LoadScene(MainMenuScene);
        }
    }
}
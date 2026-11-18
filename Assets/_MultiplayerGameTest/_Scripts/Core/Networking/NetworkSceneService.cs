using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class NetworkSceneService : INetworkSceneService
    {
        private readonly NetworkManager _networkManager;

        public NetworkSceneService(NetworkManager networkManager) => _networkManager = networkManager;

        public void LoadForAll(string sceneName)
        {
            if (!_networkManager.IsServer)
            {
                Debug.LogWarning("[Scenes] Only the host can load network scenes.");
                return;
            }

            var status = _networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
                Debug.LogError($"[Scenes] Failed to start loading '{sceneName}': {status}");
        }
    }
}
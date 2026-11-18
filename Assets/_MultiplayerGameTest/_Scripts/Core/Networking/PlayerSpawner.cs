using System;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Host-only: spawns a player object for every client once it has loaded the game scene.
    /// Also covers late joiners, who load the current scene during synchronization.
    /// Player objects are destroyed by NGO together with their owner, so no cleanup is needed here.
    /// </summary>
    public class PlayerSpawner : IInitializable, IDisposable
    {
        private const float SpawnRadius = 3f;
        private const float SpawnHeight = 1.1f;
        private const int SpawnSlots = 8;

        private readonly NetworkManager _networkManager;
        private readonly NetworkObject _playerPrefab;

        private NetworkSceneManager _sceneManager;

        public PlayerSpawner(NetworkManager networkManager, NetworkObject playerPrefab)
        {
            _networkManager = networkManager;
            _playerPrefab = playerPrefab;
        }

        public void Initialize()
        {
            _networkManager.OnServerStarted += Bind;
            _networkManager.OnServerStopped += HandleServerStopped;
        }

        public void Dispose()
        {
            if (_networkManager == null)
                return;

            Unbind();
            _networkManager.OnServerStarted -= Bind;
            _networkManager.OnServerStopped -= HandleServerStopped;
        }

        private void Bind()
        {
            Unbind();
            _sceneManager = _networkManager.SceneManager;
            _sceneManager.OnSceneEvent += HandleSceneEvent;
        }

        private void Unbind()
        {
            if (_sceneManager == null)
                return;

            _sceneManager.OnSceneEvent -= HandleSceneEvent;
            _sceneManager = null;
        }

        private void HandleServerStopped(bool wasHost) => Unbind();

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.LoadComplete:
                    if (sceneEvent.SceneName == SceneNames.Game)
                        SpawnFor(sceneEvent.ClientId);
                    break;

                case SceneEventType.SynchronizeComplete:
                    // A late joiner finished synchronizing: spawn only if the game is already running.
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneNames.Game)
                        SpawnFor(sceneEvent.ClientId);
                    break;
            }
        }

        private void SpawnFor(ulong clientId)
        {
            if (!_networkManager.ConnectedClients.TryGetValue(clientId, out var client) ||
                client.PlayerObject != null)
                return;

            var angle = (clientId % SpawnSlots) * Mathf.PI * 2f / SpawnSlots;
            var position = new Vector3(Mathf.Cos(angle) * SpawnRadius, SpawnHeight, Mathf.Sin(angle) * SpawnRadius);

            var instance = Object.Instantiate(_playerPrefab, position, Quaternion.identity);
            // Destroyed with the game scene, so a return to the lobby leaves no stale players.
            instance.SpawnAsPlayerObject(clientId, destroyWithScene: true);

            Debug.Log($"[Spawner] Player spawned for client {clientId}.");
        }
    }
}
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Tracks scene loading across the session.
    /// Host: remembers which clients must load the scene, marks each one as it reports in,
    /// drops those who disconnect, kicks those who exceed the timeout, and tells everyone
    /// when the load is finished so all loading screens close together.
    /// Client: shows the loading screen on a load or a late-join synchronization and hides it
    /// on the host's "finished" message (or when synchronization completes).
    /// </summary>
    public class SceneLoadCoordinator : ISceneLoadState, IInitializable, ITickable, IDisposable
    {
        private const string LoadFinishedMessage = "scene-load-finished";

        private readonly NetworkManager _networkManager;
        private readonly NetworkSettings _settings;
        private readonly Dictionary<ulong, ClientLoadStatus> _statuses = new();
        private readonly List<ulong> _scratch = new();

        private NetworkSceneManager _sceneManager;
        private bool _hostLoadActive;
        private float _deadline;

        public SceneLoadCoordinator(NetworkManager networkManager, NetworkSettings settings)
        {
            _networkManager = networkManager;
            _settings = settings;
        }

        public bool IsLoading { get; private set; }
        public string SceneName { get; private set; } = string.Empty;
        public IReadOnlyDictionary<ulong, ClientLoadStatus> ClientStatuses => _statuses;

        public event Action Changed;

        public void Initialize()
        {
            _networkManager.OnServerStarted += Bind;
            _networkManager.OnClientStarted += Bind;
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            _networkManager.OnClientStopped += HandleClientStopped;
        }

        public void Dispose()
        {
            if (_networkManager == null)
                return;

            Unbind();
            _networkManager.OnServerStarted -= Bind;
            _networkManager.OnClientStarted -= Bind;
            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            _networkManager.OnClientStopped -= HandleClientStopped;
        }

        public void Tick()
        {
            if (!_hostLoadActive || Time.realtimeSinceStartup < _deadline)
                return;

            // Whoever is still loading after the timeout is considered stuck.
            _scratch.Clear();
            foreach (var pair in _statuses)
            {
                if (pair.Value == ClientLoadStatus.Loading)
                    _scratch.Add(pair.Key);
            }

            foreach (var clientId in _scratch)
            {
                _statuses[clientId] = ClientLoadStatus.TimedOut;
                Debug.LogWarning($"[SceneLoad] Client {clientId} timed out while loading '{SceneName}'.");

                if (clientId != NetworkManager.ServerClientId)
                    _networkManager.DisconnectClient(clientId, SessionEndReason.ConnectionTimeout.ToString());
            }

            FinishHostLoad();
        }

        private void Bind()
        {
            var sceneManager = _networkManager.SceneManager;
            if (sceneManager == null || ReferenceEquals(sceneManager, _sceneManager))
                return;

            Unbind();
            _sceneManager = sceneManager;
            _sceneManager.OnSceneEvent += HandleSceneEvent;
            _networkManager.CustomMessagingManager?.RegisterNamedMessageHandler(
                LoadFinishedMessage, HandleLoadFinishedMessage);
        }

        private void Unbind()
        {
            if (_sceneManager != null)
            {
                _sceneManager.OnSceneEvent -= HandleSceneEvent;
                _sceneManager = null;
            }

            _networkManager.CustomMessagingManager?.UnregisterNamedMessageHandler(LoadFinishedMessage);
        }

        private void HandleClientConnected(ulong clientId) => Bind();

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    if (_networkManager.IsServer)
                        BeginHostLoad(sceneEvent.SceneName);
                    else
                        Show(sceneEvent.SceneName);
                    break;

                case SceneEventType.LoadComplete:
                    if (_networkManager.IsServer)
                        MarkLoaded(sceneEvent.ClientId);
                    break;

                case SceneEventType.Synchronize:
                    if (!_networkManager.IsServer)
                        Show(sceneEvent.SceneName);
                    break;

                case SceneEventType.SynchronizeComplete:
                    if (!_networkManager.IsServer)
                        Hide();
                    break;
            }
        }

        private void BeginHostLoad(string sceneName)
        {
            _statuses.Clear();
            foreach (var clientId in _networkManager.ConnectedClientsIds)
                _statuses[clientId] = ClientLoadStatus.Loading;

            _hostLoadActive = true;
            _deadline = Time.realtimeSinceStartup + _settings.SceneLoadTimeoutSeconds;
            Show(sceneName);
        }

        private void MarkLoaded(ulong clientId)
        {
            if (!_hostLoadActive || !_statuses.ContainsKey(clientId))
                return;

            _statuses[clientId] = ClientLoadStatus.Loaded;
            Changed?.Invoke();
            TryFinishHostLoad();
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!_networkManager.IsServer || !_hostLoadActive)
                return;

            if (_statuses.TryGetValue(clientId, out var status) && status == ClientLoadStatus.Loading)
            {
                _statuses[clientId] = ClientLoadStatus.Dropped;
                Debug.Log($"[SceneLoad] Client {clientId} dropped during loading, no longer waiting for it.");
                Changed?.Invoke();
                TryFinishHostLoad();
            }
        }

        private void TryFinishHostLoad()
        {
            foreach (var status in _statuses.Values)
            {
                if (status == ClientLoadStatus.Loading)
                    return;
            }

            FinishHostLoad();
        }

        private void FinishHostLoad()
        {
            _hostLoadActive = false;

            using var writer = new FastBufferWriter(4, Allocator.Temp);
            writer.WriteByteSafe(1);
            _networkManager.CustomMessagingManager.SendNamedMessageToAll(LoadFinishedMessage, writer);

            Debug.Log($"[SceneLoad] '{SceneName}' loaded for everyone.");
            Hide();
        }

        private void HandleLoadFinishedMessage(ulong senderClientId, FastBufferReader payload) => Hide();

        private void HandleClientStopped(bool wasHost)
        {
            Unbind();
            _hostLoadActive = false;
            _statuses.Clear();
            SceneName = string.Empty;
            IsLoading = false;
            Changed?.Invoke();
        }

        private void Show(string sceneName)
        {
            SceneName = sceneName ?? string.Empty;
            IsLoading = true;
            Changed?.Invoke();
        }

        private void Hide()
        {
            IsLoading = false;
            Changed?.Invoke();
        }
    }
}
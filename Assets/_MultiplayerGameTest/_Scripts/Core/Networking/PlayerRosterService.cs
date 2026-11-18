using System;
using System.Collections.Generic;
using _MultiplayerGameTest._Scripts.Core.Identity;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class PlayerRosterService : IPlayerRoster, IInitializable, IDisposable
    {
        private const int MaxNameLength = 20;

        private readonly NetworkManager _networkManager;
        private readonly NetworkPlayerRoster _rosterPrefab;
        private readonly ConnectionApprovalHandler _approvalHandler;
        private readonly IPlayerIdentityProvider _identity;
        private readonly List<PlayerData> _players = new();

        private NetworkPlayerRoster _roster;

        public PlayerRosterService(
            NetworkManager networkManager,
            NetworkPlayerRoster rosterPrefab,
            ConnectionApprovalHandler approvalHandler,
            IPlayerIdentityProvider identity)
        {
            _networkManager = networkManager;
            _rosterPrefab = rosterPrefab;
            _approvalHandler = approvalHandler;
            _identity = identity;
        }

        public IReadOnlyList<PlayerData> Players => _players;
        public ulong LocalClientId => _networkManager.LocalClientId;

        public event Action Changed;

        public void Initialize()
        {
            _networkManager.OnServerStarted += HandleServerStarted;
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            _networkManager.OnClientStopped += HandleClientStopped;
        }

        public void Dispose()
        {
            Unbind();
            if (_networkManager == null)
                return;

            _networkManager.OnServerStarted -= HandleServerStarted;
            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            _networkManager.OnClientStopped -= HandleClientStopped;
        }

        private void HandleServerStarted()
        {
            var instance = Object.Instantiate(_rosterPrefab);
            instance.NetworkObject.Spawn(destroyWithScene: false);

            Bind(instance);
            _roster.Add(CreateInfo(NetworkManager.ServerClientId, _identity.PlayerId, _identity.PlayerName));
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (_networkManager.IsServer)
            {
                if (clientId == NetworkManager.ServerClientId || _roster == null)
                    return;

                if (!_approvalHandler.TryGetPayload(clientId, out var payload))
                {
                    Debug.LogWarning($"[Roster] No handshake data for client {clientId}.");
                    return;
                }

                _roster.Add(CreateInfo(clientId, payload.playerId, payload.playerName));
                return;
            }

            if (clientId != _networkManager.LocalClientId)
                return;

            var replicated = Object.FindFirstObjectByType<NetworkPlayerRoster>();
            if (replicated == null)
            {
                Debug.LogWarning("[Roster] Replicated roster was not found after connect.");
                return;
            }

            Bind(replicated);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!_networkManager.IsServer)
                return;

            _approvalHandler.Forget(clientId);
            if (_roster != null)
                _roster.Remove(clientId);
        }

        private void HandleClientStopped(bool wasHost) => Unbind();

        private void Bind(NetworkPlayerRoster roster)
        {
            Unbind();
            _roster = roster;
            _roster.Changed += Rebuild;
            Rebuild();
        }

        private void Unbind()
        {
            if (_roster != null)
            {
                _roster.Changed -= Rebuild;
                _roster = null;
            }

            if (_players.Count == 0)
                return;

            _players.Clear();
            Changed?.Invoke();
        }

        private void Rebuild()
        {
            _players.Clear();
            if (_roster != null)
            {
                for (var i = 0; i < _roster.Count; i++)
                {
                    var info = _roster[i];
                    _players.Add(new PlayerData(
                        info.ClientId,
                        info.PlayerId.ToString(),
                        info.Name.ToString(),
                        info.ClientId == NetworkManager.ServerClientId));
                }
            }

            Changed?.Invoke();
        }

        private static NetworkPlayerInfo CreateInfo(ulong clientId, string playerId, string playerName)
        {
            playerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
            if (playerName.Length > MaxNameLength)
                playerName = playerName.Substring(0, MaxNameLength);

            return new NetworkPlayerInfo
            {
                ClientId = clientId,
                PlayerId = ToFixed(playerId),
                Name = ToFixed(playerName)
            };
        }

        private static FixedString64Bytes ToFixed(string value)
        {
            var result = new FixedString64Bytes();
            result.CopyFromTruncated(value ?? string.Empty);
            return result;
        }
    }
}
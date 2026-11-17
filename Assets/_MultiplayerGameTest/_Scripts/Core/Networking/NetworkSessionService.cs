using System;
using _MultiplayerGameTest._Scripts.Core.Identity;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class NetworkSessionService : INetworkSessionService, IInitializable, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly INetworkTransportProvider _transportProvider;
        private readonly ConnectionApprovalHandler _approvalHandler;
        private readonly IPlayerIdentityProvider _identity;

        private SessionState _state = SessionState.Idle;
        private bool _shutdownRequested;
        private string _remoteDisconnectReason;

        public NetworkSessionService(
            NetworkManager networkManager,
            INetworkTransportProvider transportProvider,
            ConnectionApprovalHandler approvalHandler,
            IPlayerIdentityProvider identity)
        {
            _networkManager = networkManager;
            _transportProvider = transportProvider;
            _approvalHandler = approvalHandler;
            _identity = identity;
        }

        public SessionState State => _state;
        public bool IsHost => _networkManager.IsHost;

        public event Action<SessionState> StateChanged;
        public event Action<SessionEndReason> SessionEnded;

        public void Initialize()
        {
            _approvalHandler.Attach();
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            _networkManager.OnClientStopped += HandleClientStopped;
        }

        public void Dispose()
        {
            _approvalHandler.Detach();
            if (_networkManager == null)
                return;

            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            _networkManager.OnClientStopped -= HandleClientStopped;
        }

        public void StartHost(ushort port)
        {
            if (_state != SessionState.Idle)
                return;

            BeginSession();
            _transportProvider.ConfigureForHost(_networkManager, port);

            if (!_networkManager.StartHost())
                AbortStart();
        }

        public void Join(ConnectionTarget target)
        {
            if (_state != SessionState.Idle)
                return;

            BeginSession();
            _transportProvider.ConfigureForClient(_networkManager, target);

            if (!_networkManager.StartClient())
                AbortStart();
        }

        public void Shutdown()
        {
            if (_state == SessionState.Idle)
                return;

            _shutdownRequested = true;
            _networkManager.Shutdown();
        }

        private void BeginSession()
        {
            _shutdownRequested = false;
            _remoteDisconnectReason = null;

            var payload = new ConnectionPayload
            {
                playerId = _identity.PlayerId,
                playerName = _identity.PlayerName,
                buildVersion = Application.version
            };

            _networkManager.NetworkConfig.ConnectionApproval = true;
            _networkManager.NetworkConfig.ConnectionData = payload.ToBytes();

            SetState(SessionState.Connecting);
        }

        private void AbortStart()
        {
            if (_networkManager.IsListening)
                _networkManager.Shutdown();

            FinishSession(SessionEndReason.Unknown);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (_networkManager.IsServer)
                Debug.Log($"[Session] Client {clientId} connected. Players: {_networkManager.ConnectedClientsIds.Count}");

            if (clientId == _networkManager.LocalClientId && _state == SessionState.Connecting)
                SetState(SessionState.Connected);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (_networkManager.IsServer)
                return;

            var reason = _networkManager.DisconnectReason;
            if (!string.IsNullOrEmpty(reason))
                _remoteDisconnectReason = reason;
        }

        private void HandleClientStopped(bool wasHost) => FinishSession(ResolveEndReason());

        private SessionEndReason ResolveEndReason()
        {
            if (_shutdownRequested)
                return SessionEndReason.LocalRequest;

            if (!string.IsNullOrEmpty(_remoteDisconnectReason) &&
                Enum.TryParse(_remoteDisconnectReason, out SessionEndReason parsed))
                return parsed;

            return _state == SessionState.Connecting
                ? SessionEndReason.ConnectionTimeout
                : SessionEndReason.ConnectionLost;
        }

        private void FinishSession(SessionEndReason reason)
        {
            if (_state == SessionState.Idle)
                return;

            _shutdownRequested = false;
            _remoteDisconnectReason = null;

            Debug.Log($"[Session] Ended: {reason}");
            SetState(SessionState.Idle);
            SessionEnded?.Invoke(reason);
        }

        private void SetState(SessionState state)
        {
            if (_state == state)
                return;

            _state = state;
            StateChanged?.Invoke(state);
        }
    }
}
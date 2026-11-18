using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class ConnectionApprovalHandler
    {
        private readonly NetworkManager _networkManager;
        private readonly NetworkSettings _settings;
        private readonly Dictionary<ulong, ConnectionPayload> _approvedPayloads = new();
        
        public ConnectionApprovalHandler(NetworkManager networkManager, NetworkSettings settings)
        {
            _networkManager = networkManager;
            _settings = settings;
        }
        
        public void Attach() => _networkManager.ConnectionApprovalCallback = Approve;
        
        public void Detach()
        {
            if (_networkManager != null)
                _networkManager.ConnectionApprovalCallback = null;
        }
        
        public bool TryGetPayload(ulong clientId, out ConnectionPayload payload) =>
            _approvedPayloads.TryGetValue(clientId, out payload);
        
        public void Forget(ulong clientId) => _approvedPayloads.Remove(clientId);
        
        private void Approve(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false;

            if (request.ClientNetworkId == NetworkManager.ServerClientId)
            {
                response.Approved = true;
                return;
            }

            if (!ConnectionPayload.TryParse(request.Payload, out var payload))
            {
                Reject(response, SessionEndReason.Unknown, request.ClientNetworkId);
                return;
            }

            if (_networkManager.ConnectedClientsIds.Count >= _settings.MaxPlayers)
            {
                Reject(response, SessionEndReason.SessionFull, request.ClientNetworkId);
                return;
            }

            _approvedPayloads[request.ClientNetworkId] = payload;
            response.Approved = true;
        }
        
        private static void Reject(NetworkManager.ConnectionApprovalResponse response, 
            SessionEndReason reason, ulong clientId)
        {
            response.Approved = false;
            response.Reason = reason.ToString();
            Debug.Log($"[Approval] Client {clientId} rejected: {reason}");
        }

    }
}
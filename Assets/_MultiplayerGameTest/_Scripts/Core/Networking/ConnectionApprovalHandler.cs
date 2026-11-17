using Unity.Netcode;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class ConnectionApprovalHandler
    {
        private readonly NetworkManager _networkManager;
        private readonly NetworkSettings _settings;
        
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
        
        private void Approve(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false;

            if (request.ClientNetworkId == NetworkManager.ServerClientId)
            {
                response.Approved = true;
                return;
            }

            if (!ConnectionPayload.TryParse(request.Payload, out _))
            {
                Reject(response, SessionEndReason.Unknown, request.ClientNetworkId);
                return;
            }

            if (_networkManager.ConnectedClientsIds.Count >= _settings.MaxPlayers)
            {
                Reject(response, SessionEndReason.SessionFull, request.ClientNetworkId);
                return;
            }

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
using _MultiplayerGameTest._Scripts.Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Gameplay.Items
{
    /// <summary>
    /// A physical item. The host simulates physics while it is free.
    /// While held, ownership belongs to the holder, who moves the item in front of the camera
    /// (NetworkTransform uses Authority Mode = Owner), and everyone has its colliders disabled.
    /// All state changes are made by the host; clients only read the holder variable.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkItem : NetworkBehaviour
    {
                public const ulong NoHolder = ulong.MaxValue;

        [SerializeField] private Rigidbody body;

        private readonly NetworkVariable<ulong> _holder = new(NoHolder);

        private Collider[] _colliders;
        private Transform _anchor;
        private Vector3 _pendingVelocity;
        private bool _hasPendingVelocity;

        public bool IsHeld => _holder.Value != NoHolder;
        public ulong HolderClientId => _holder.Value;

        private void Awake() => _colliders = GetComponentsInChildren<Collider>();

        public override void OnNetworkSpawn()
        {
            _holder.OnValueChanged += HandleHolderChanged;
            ApplyHeldState(_holder.Value);

            if (!IsServer)
                return;

            // Ownership falls back to the host when the holder leaves, instead of despawning the item.
            NetworkObject.DontDestroyWithOwner = true;
            NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            _holder.OnValueChanged -= HandleHolderChanged;

            if (IsServer && NetworkManager != null)
                NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        public bool ServerTryGrab(ulong clientId)
        {
            if (!IsServer || IsHeld)
                return false;

            _hasPendingVelocity = false;
            _holder.Value = clientId;

            if (clientId != NetworkManager.ServerClientId)
                NetworkObject.ChangeOwnership(clientId);

            ApplyHeldState(clientId);
            return true;
        }

        public void ServerRelease(Vector3 velocity)
        {
            if (!IsServer)
                return;

            _holder.Value = NoHolder;

            if (OwnerClientId != NetworkManager.ServerClientId)
                NetworkObject.RemoveOwnership();

            ApplyHeldState(NoHolder);

            _pendingVelocity = velocity;
            _hasPendingVelocity = true;
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (_holder.Value == clientId)
            {
                Debug.Log($"Holder {clientId} left, dropping '{name}'.");
                ServerRelease(Vector3.zero);
            }
        }

        private void HandleHolderChanged(ulong previous, ulong current) => ApplyHeldState(current);

        private void ApplyHeldState(ulong holder)
        {
            var held = holder != NoHolder;

            foreach (var itemCollider in _colliders)
                itemCollider.enabled = !held;

            _anchor = null;
            if (held && NetworkManager != null && holder == NetworkManager.LocalClientId)
                _anchor = FindLocalHoldPoint();
        }

        private Transform FindLocalHoldPoint()
        {
            var player = NetworkManager.LocalClient?.PlayerObject;
            if (player != null && player.TryGetComponent(out PlayerInteraction interaction))
                return interaction.HoldPoint;

            return null;
        }

        private void FixedUpdate()
        {
            if (!_hasPendingVelocity || !IsServer || !IsOwner)
                return;

            _hasPendingVelocity = false;
            body.isKinematic = false;
            body.linearVelocity = _pendingVelocity;
        }

        private void LateUpdate()
        {
            if (!_anchor || !IsHeld || !IsOwner)
                return;

            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            transform.SetPositionAndRotation(_anchor.position, _anchor.rotation);
        }
    }
}
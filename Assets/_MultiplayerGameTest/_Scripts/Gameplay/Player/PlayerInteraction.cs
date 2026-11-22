using _MultiplayerGameTest._Scripts.Gameplay.Items;
using Unity.Netcode;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Gameplay.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInteraction : NetworkBehaviour
    {
                private const ulong NoItem = ulong.MaxValue;

        [SerializeField] private PlayerController controller;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private ItemSettings settings;

        private readonly NetworkVariable<ulong> _heldItemId = new(NoItem);
        private float _nextSpawnTime;

        public Transform HoldPoint => holdPoint;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                enabled = false;
        }

        // ---------------------------------------------------------------- Owner: input

        private void Update()
        {
            var input = controller.InputReader;
            if (input == null || !controller.InputActive)
                return;

            if (input.SpawnItemPressed)
                RequestSpawnItemRpc(0);

            var holding = _heldItemId.Value != NoItem;

            if (input.InteractPressed)
            {
                if (holding)
                    RequestReleaseRpc();
                else if (TryFindTargetItem(out var target))
                    RequestGrabRpc(target);
            }

            if (holding && input.ThrowPressed)
                RequestThrowRpc(cameraTransform.forward);

            if (holding && input.DeletePressed)
                RequestDeleteRpc();
        }

        private bool TryFindTargetItem(out NetworkObject target)
        {
            target = null;
            var ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (!Physics.Raycast(ray, out var hit, settings.GrabDistance, ~0, QueryTriggerInteraction.Ignore))
                return false;

            var item = hit.collider.GetComponentInParent<NetworkItem>();
            if (item == null || item.IsHeld)
                return false;

            target = item.NetworkObject;
            return true;
        }

        // ---------------------------------------------------------------- Host: requests

        [Rpc(SendTo.Server)]
        public void RequestSpawnItemRpc(int prefabIndex, RpcParams rpcParams = default)
        {
            if (!IsRequestFromOwner(rpcParams) || Time.time < _nextSpawnTime)
                return;

            if (prefabIndex < 0 || prefabIndex >= settings.PrefabCount)
                return;

            if (Object.FindObjectsByType<NetworkItem>(FindObjectsSortMode.None).Length >= settings.MaxItems)
                return;

            _nextSpawnTime = Time.time + settings.SpawnCooldownSeconds;

            var position = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
            var instance = Instantiate(settings.GetPrefab(prefabIndex), position, Quaternion.identity);

            instance.Spawn(destroyWithScene: true);
        }

        [Rpc(SendTo.Server)]
        public void RequestGrabRpc(NetworkObjectReference itemReference, RpcParams rpcParams = default)
        {
            if (!IsRequestFromOwner(rpcParams) || GetHeldItem() != null)
                return;

            if (!itemReference.TryGet(out var itemObject) || !itemObject.TryGetComponent(out NetworkItem item))
                return;

            var maxDistance = settings.ServerGrabDistance;
            if ((item.transform.position - transform.position).sqrMagnitude > maxDistance * maxDistance)
                return;

            if (!item.ServerTryGrab(OwnerClientId))
                return;

            _heldItemId.Value = itemObject.NetworkObjectId;
        }

        [Rpc(SendTo.Server)]
        public void RequestReleaseRpc(RpcParams rpcParams = default)
        {
            if (!IsRequestFromOwner(rpcParams))
                return;

            var item = GetHeldItem();
            if (item == null || item.HolderClientId != OwnerClientId)
                return;

            item.ServerRelease(Vector3.zero);
            _heldItemId.Value = NoItem;
        }

        [Rpc(SendTo.Server)]
        public void RequestThrowRpc(Vector3 direction, RpcParams rpcParams = default)
        {
            if (!IsRequestFromOwner(rpcParams) || !IsUsableDirection(direction))
                return;

            var item = GetHeldItem();
            if (item == null || item.HolderClientId != OwnerClientId)
                return;

            item.ServerRelease(direction.normalized * settings.ThrowSpeed);
            _heldItemId.Value = NoItem;
        }

        [Rpc(SendTo.Server)]
        public void RequestDeleteRpc(RpcParams rpcParams = default)
        {
            if (!IsRequestFromOwner(rpcParams))
                return;

            var item = GetHeldItem();
            if (!item || item.HolderClientId != OwnerClientId)
                return;

            _heldItemId.Value = NoItem;
            item.NetworkObject.Despawn();
        }

        // ---------------------------------------------------------------- Helpers
        private bool IsRequestFromOwner(RpcParams rpcParams) =>
            rpcParams.Receive.SenderClientId == OwnerClientId;

        private NetworkItem GetHeldItem()
        {
            if (_heldItemId.Value == NoItem)
                return null;

            var spawned = NetworkManager.SpawnManager.SpawnedObjects;
            if (spawned.TryGetValue(_heldItemId.Value, out var itemObject) &&
                itemObject.TryGetComponent(out NetworkItem item))
                return item;

            return null;
        }

        private static bool IsUsableDirection(Vector3 direction) =>
            float.IsFinite(direction.x) && float.IsFinite(direction.y) && float.IsFinite(direction.z) &&
            direction.sqrMagnitude > 0.0001f;
    }
}
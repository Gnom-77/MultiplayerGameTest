using Unity.Netcode;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Gameplay.Items
{
    [CreateAssetMenu(menuName = "Game/Item Settings", fileName = "ItemSettings")]
    public class ItemSettings : ScriptableObject
    {
        [SerializeField] private NetworkObject[] itemPrefabs;
        [SerializeField, Min(0.5f)] private float grabDistance = 3f;
        // Extra distance the host tolerates for latency: both sides move while the request is in flight.
        [SerializeField, Min(0f)] private float serverDistanceSlack = 1.5f;
        [SerializeField, Min(1f)] private float throwSpeed = 12f;
        [SerializeField, Min(1)] private int maxItems = 20;
        [SerializeField, Min(0f)] private float spawnCooldownSeconds = 0.3f;

        public int PrefabCount => itemPrefabs?.Length ?? 0;
        public NetworkObject GetPrefab(int index) => itemPrefabs[index];
        public float GrabDistance => grabDistance;
        public float ServerGrabDistance => grabDistance + serverDistanceSlack;
        public float ThrowSpeed => throwSpeed;
        public int MaxItems => maxItems;
        public float SpawnCooldownSeconds => spawnCooldownSeconds;
    }
}
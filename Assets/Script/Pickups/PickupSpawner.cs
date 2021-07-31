using Game.Level;

using Photon.Pun;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Game.Pickup
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class PickupSpawner : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Name of prefabs randomly spawned when a player is destroyed.")]
        private string[] pickupPrefabs;
#pragma warning restore CS0649

        private static PickupSpawner instance;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"{nameof(Server)} is singlenton.");
                Destroy(this);
                return;
            }
            instance = this;
        }

        public static void Spawn(Rigidbody2D rigidbody)
        {
            GameObject pickup = Server.InstantiatePrefab(instance.pickupPrefabs[Random.Range(0, instance.pickupPrefabs.Length)], null, rigidbody.position, Quaternion.Euler(0, 0, rigidbody.rotation));
            Rigidbody2D instanceRigidbody = pickup.GetComponent<Rigidbody2D>();
            instanceRigidbody.velocity = rigidbody.velocity / 2;
        }
    }
}
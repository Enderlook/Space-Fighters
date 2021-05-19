using Enderlook.Enumerables;

using Photon.Pun;

using UnityEngine;

namespace Game.Level
{
    [RequireComponent(typeof(PhotonView))]
    public sealed class GameManager : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [SerializeField]
        private string playerPrefab;

        [SerializeField]
        private float spawnRadius;
#pragma warning restore CS0649

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
            int number = players.FindIndexBy(e => e.IsLocal);

            float x = transform.position.x + (spawnRadius * Mathf.Cos(2 * Mathf.PI * number / players.Length));
            float y = transform.position.y + (spawnRadius * Mathf.Sin(2 * Mathf.PI * number / players.Length));
            Vector3 position = new Vector3(x, y);

            Vector3 direction = (Vector3.zero - position).normalized;
            float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, z - 90);

            GameObject player = PhotonNetwork.Instantiate(playerPrefab, position, rotation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
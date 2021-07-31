using Photon.Pun;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class Server : MonoBehaviourPunCallbacks
    {
        public const bool IsFullAuth = true;

#pragma warning disable CS0649
        [SerializeField, Tooltip("Prefab of player to spawn.")]
        private string playerPrefab;

        [SerializeField, Tooltip("Radius from center of this transform to spawn players.")]
        private float spawnRadius;
#pragma warning restore CS0649

        private static Server instance;

        private Photon.Realtime.Player server;

        public static bool IsServer => PhotonNetwork.LocalPlayer == ServerPlayer;

        public static Photon.Realtime.Player ServerPlayer => instance.server;

        public static Photon.Realtime.Player[] Clients {
            get {
                if (IsFullAuth)
                {
                    if (IsServer)
                        return PhotonNetwork.PlayerListOthers;
                    return PhotonNetwork.PlayerList.Where(e => e.ActorNumber != ServerPlayer.ActorNumber).ToArray();
                }
                else
                    return PhotonNetwork.PlayerList;
            }
        }

        public static byte ClientsCount => (byte)(IsFullAuth ? PhotonNetwork.CurrentRoom.PlayerCount - 1 : PhotonNetwork.CurrentRoom.PlayerCount);

        private Queue<Action> actions = new Queue<Action>();

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

            if (PhotonNetwork.IsMasterClient)
            {
                this.RPC(() => RPC_SetServer(PhotonNetwork.LocalPlayer), RpcTarget.AllBuffered);

                Photon.Realtime.Player[] players = Clients;
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].TagObject = i;
                    (Vector2 position, float rotation) tuple = GetSpawnPosition(i, players.Length);
                    InstantiatePrefab(playerPrefab, players[i], tuple.position, Quaternion.Euler(0, 0, tuple.rotation));
                }
            }
            else
            {
                AfterServerIsConnected(() =>
                {
                    Photon.Realtime.Player[] players = Clients;
                    for (int i = 0; i < players.Length; i++)
                        players[i].TagObject = i;
                });
            }
        }

        public static (Vector2 position, float rotation) GetSpawnPosition(Photon.Realtime.Player player)
        {
            Debug.Assert(player.TagObject != null);
            return instance.GetSpawnPosition((int)player.TagObject, ClientsCount);
        }

        private (Vector2 position, float rotation) GetSpawnPosition(int playerIndex, int totalPlayers)
        {
            float j = 2 * Mathf.PI * playerIndex / totalPlayers;
            Vector2 position = (Vector2)transform.position + new Vector2(Mathf.Cos(j), Mathf.Sin(j)) * spawnRadius;

            Vector2 direction = (Vector2.zero - position).normalized;
            float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            return (position, z - 90);
        }

        public static void AfterServerIsConnected(Action action)
        {
            if (ServerPlayer is null)
                instance.actions.Enqueue(action);
            else
                action();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (!IsServer)
                return;

            foreach (PhotonView photonView in FindObjectsOfType<PhotonView>())
                if (GetPlayerOwner(photonView) == otherPlayer)
                    PhotonNetwork.Destroy(photonView);
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) => server = newMasterClient;

        public static Photon.Realtime.Player GetPlayerOwner(PhotonView view)
        {
            if (view.InstantiationData is null)
                return view.Owner;
            return (Photon.Realtime.Player)view.InstantiationData[0];
        }

        [PunRPC]
        private void RPC_SetServer(Photon.Realtime.Player server)
        {
            this.server = server;

            while (actions.TryDequeue(out Action action))
                action();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }

        public static GameObject InstantiatePrefab(string prefab, Photon.Realtime.Player owner, Vector3 position, Quaternion rotation)
            => PhotonNetwork.InstantiateRoomObject(prefab, position, rotation, data: new object[] { owner });
    }
}
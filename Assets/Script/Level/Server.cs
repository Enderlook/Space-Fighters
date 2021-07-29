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
        [SerializeField]
        private string playerPrefab;

        [SerializeField]
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
            }
            instance = this;

            if (PhotonNetwork.IsMasterClient)
            {
                this.RPC(() => RPC_SetServer(PhotonNetwork.LocalPlayer), RpcTarget.AllBuffered);

                Photon.Realtime.Player[] players = Clients;
                for (int i = 0; i < players.Length; i++)
                {
                    float j = 2 * Mathf.PI * i / players.Length;
                    Vector3 position = (Vector2)transform.position + new Vector2(Mathf.Cos(j), Mathf.Sin(j)) * spawnRadius;

                    Vector3 direction = (Vector3.zero - position).normalized;
                    float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, 0, z - 90);

                    InstantiatePrefab(playerPrefab, players[i], position, rotation);
                }
            }
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
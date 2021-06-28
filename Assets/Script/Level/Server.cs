using Photon.Pun;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Game.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class Server : MonoBehaviourPunCallbacks
    {
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

        private Dictionary<Photon.Realtime.Player, List<WeakReference<PhotonView>>> objects;

        private ConditionalWeakTable<PhotonView, Photon.Realtime.Player> owners = new ConditionalWeakTable<PhotonView, Photon.Realtime.Player>();

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

                objects = new Dictionary<Photon.Realtime.Player, List<WeakReference<PhotonView>>>();
                Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
                for (int i = 0; i < players.Length; i++)
                {
                    objects.Add(players[i], new List<WeakReference<PhotonView>>());

                    float x = transform.position.x + (spawnRadius * Mathf.Cos(2 * Mathf.PI * i / players.Length));
                    float y = transform.position.y + (spawnRadius * Mathf.Sin(2 * Mathf.PI * i / players.Length));
                    Vector3 position = new Vector3(x, y);

                    Vector3 direction = (Vector3.zero - position).normalized;
                    float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, 0, z - 90);

                    InstantiatePrefab(playerPrefab, players[i], position, rotation);
                }
            }
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (!IsServer)
                return;

            foreach (WeakReference<PhotonView> reference in objects[otherPlayer])
                if (reference.TryGetTarget(out PhotonView photonView) && photonView != null)
                    PhotonNetwork.Destroy(photonView);
        }

        public static Photon.Realtime.Player GetPlayerOwner(PhotonView view)
        {
            if (instance.owners.TryGetValue(view, out Photon.Realtime.Player owner))
                return owner;
            owner = (Photon.Realtime.Player)view.InstantiationData[0];
            instance.owners.Add(view, owner);
            return owner;
        }

        [PunRPC]
        private void RPC_SetServer(Photon.Realtime.Player server) => this.server = server;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }

        public static GameObject InstantiatePrefab(string prefab, Photon.Realtime.Player owner, Vector3 position, Quaternion rotation)
        {
            GameObject instance = PhotonNetwork.InstantiateRoomObject(prefab, position, rotation, data: new object[] { owner });

            List<WeakReference<PhotonView>> list = Server.instance.objects[owner];
            foreach (PhotonView view in instance.GetComponentsInChildren<PhotonView>())
                list.Add(new WeakReference<PhotonView>(view));

            return instance;
        }
    }
}
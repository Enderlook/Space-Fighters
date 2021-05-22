using Enderlook.Unity.Utils;

using Photon.Pun;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView)), DefaultExecutionOrder(1)]
    public sealed class PlayerScore : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Holder of all players.")]
        private RectTransform playersHolder;

        [SerializeField, Tooltip("Prefab of player name in UI.")]
        private Text playerPrefab;
#pragma warning restore CS0649

        private Dictionary<Photon.Realtime.Player, (Color color, int kills)> players = new Dictionary<Photon.Realtime.Player, (Color color, int kills)>();

        private static PlayerScore instance;
        private static PlayerScore Instance {
            get {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayerScore>();
                    int i = 0;
                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                        instance.players.Add(player, (GetPlayerColor(i++), 0));
                    instance.UpdateValues();
                }
                return instance;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            PlayerScore _ = Instance;
        }

        public static Color GetShipColor(Photon.Realtime.Player owner)
        {
            Color color = Instance.players[owner].color;
            color.SetSaturation(.3f);
            return color;
        }

        public static void IncreaseCounter(Photon.Realtime.Player player)
            => Instance.photonView.RPC(nameof(RPC_IncreaseCounter), RpcTarget.All, player);

        [PunRPC]
        private void RPC_IncreaseCounter(Photon.Realtime.Player player)
        {
            (Color color, int kills) info = players[player];
            players[player] = (info.color, info.kills + 1);
            UpdateValues();
        }

        private void UpdateValues()
        {
            for (int i = playersHolder.childCount - 1; i >= 0; i--)
                Destroy(playersHolder.GetChild(i).gameObject);

            foreach (KeyValuePair<Photon.Realtime.Player, (Color color, int kills)> kvp in players
                .OrderByDescending(e => e.Value.kills)
                .ThenByDescending(e => e.Key.NickName))
            {
                Text text = Instantiate(playerPrefab, playersHolder);
                text.text = $"{kvp.Key.NickName} ({kvp.Value.kills})";
                text.color = kvp.Value.color;
            }
        }

        private static Color GetPlayerColor(int index)
        {
            // https://gamedev.stackexchange.com/a/46469 from https://gamedev.stackexchange.com/questions/46463/how-can-i-find-an-optimum-set-of-colors-for-10-players
            return Color.HSVToRGB(index * 0.618033988749895f % 1.0f, 5f, 1f);
        }
    }
}
